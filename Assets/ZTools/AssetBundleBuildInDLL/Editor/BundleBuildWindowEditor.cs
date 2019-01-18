using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Runtime.InteropServices;
using System.Linq;
using Microsoft.Win32;
using static BundleEventTriggerDesigner;
using UnityEngine.Events;
using System.Reflection;

public class BundleBuildWindowEditor : EditorWindow {

    [MenuItem("StarView/Bundle script window")]
    static void BuildScriptToBytes() {
        Rect wr = new Rect(0, 0, 700, 400);
        BundleBuildWindowEditor window = (BundleBuildWindowEditor)EditorWindow.GetWindowWithRect(
            typeof(BundleBuildWindowEditor), wr, true, "BundleBuildWindow");
        window.Show();
        window.autoRepaintOnSceneChange = true;
    }
    private string unityInstallPath;
    private string scriptPath;
    private string dllName = "output.bytes";
    private string prefabDir;
    private string configPath = "";
    private string configFilePath = "";
    private StreamReader sr = null;
    private StreamWriter sw = null;
    private StreamWriter swEventConfig = null;
    private bool init = false;
    private int prefabNum = 0;
    private int stepNum = 0;
    private bool changed = false;
    private GameObject prefabs;
    private GameObject lastPrefab;
    private string bundleRecordFile = "";
    private string eventTriggerConfigPath = "";

    private UnityEngine.Object dllFile = null;
    private UnityEngine.Object recordFile = null;
    private bool generateScript = false;
    private bool generateSummary = false;
    private string[] platforms = new string[] { "android", "win64", "hololens" };
    private int index = 0;
    private int spaceValue = 15;

    private bool androidBuild = true;
    private bool pcBuild = true;
    private bool hololensBuild = false;
    string savePath;
    private bool canBuildt = false;
    private string tips;
    private int inspectIndex;
    private List<string> createColliderRecords = new List<string>();
    private bool withoutColldider = false;
    private bool hasMono = false;
    private bool hasBundleTrigger = false;
    private AllTriggerToDesingerJson allTriggerToDesingerConfig = new AllTriggerToDesingerJson();
    private bool isCreateJson = false;//是否进行本地加载，生成索引json文件

    private void OnDestroy() {
        Reset();
        sr?.Close();
        sw?.Close();
        swEventConfig?.Close();
    }

    void OnGUI() {
        if (!init)
        {
            Reset();
        }
        if (!Directory.Exists(configPath))
            Directory.CreateDirectory(configPath);


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择unity安装路径", GUILayout.Width(200)))
        {
            unityInstallPath = EditorUtility.OpenFolderPanel("unity安装路径", "", "");
            changed = true;
        }
        EditorGUILayout.SelectableLabel(unityInstallPath);
        GUILayout.EndHorizontal();

        //脚本文件夹路径
        if (hasMono)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选择需要打包的脚本文件夹", GUILayout.Width(200)))
            {
                scriptPath = EditorUtility.OpenFolderPanel("需要打包的脚本文件夹路径", "", "");
                changed = true;
            }
            EditorGUILayout.SelectableLabel(scriptPath);
            GUILayout.EndHorizontal();
        }
        //保存bundle路径
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择保存bundle文件路径", GUILayout.Width(200)))
        {
            savePath = EditorUtility.SaveFilePanel("选择保存bundle文件路径", "", "Unnamed", "assetbundle");
            changed = true;
        }
        EditorGUILayout.SelectableLabel(savePath);
        GUILayout.EndHorizontal();

        GUILayout.Space(spaceValue);
        prefabs = EditorGUILayout.ObjectField("需要打包的prefab:", prefabs, typeof(GameObject)) as GameObject;
        if (lastPrefab != prefabs)
        {
            lastPrefab = prefabs;
            Reset();
            CheckMonoForGameObject(lastPrefab);
        }
        GUILayout.Space(spaceValue);
        //-----1
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        androidBuild = EditorGUILayout.Toggle("assetbundle for android", androidBuild);
        pcBuild = EditorGUILayout.Toggle("assetbundle for pc", pcBuild);
        hololensBuild = EditorGUILayout.Toggle("assetbundle for hololens", hololensBuild);
        GUILayout.Space(10);
        isCreateJson = EditorGUILayout.Toggle("生成本地加载索引json文件", isCreateJson);
        EditorGUILayout.EndVertical();
        //提示框
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Bundle信息表");
        GUILayout.Space(5);
        EditorGUILayout.SelectableLabel(tips, GUILayout.Height(80));
        EditorGUILayout.EndVertical();
        //-----1
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(spaceValue);
        //检查是否可以进行打包操作
        if (GUILayout.Button("打包检查", GUILayout.Width(200)))
        {
            if (prefabs == null)
            {
                UnityEngine.Debug.LogError("打包物体空");
                tips = "打包物体空\n";
            }
            else if (CheckCollider(prefabs) && CheckOtherConfig())
            {
                //检查是否有BundleTrigger组件
                CheckBundleEventTriggerComponent();
                canBuildt = true;
                tips = "准备完成可以打包";

            }
            else if (!CheckCollider(prefabs))
            {
                canBuildt = false;
                inspectIndex = EditorUtility.DisplayDialogComplex("提示", "检查到Prefab无Collider可以点击自动创建，或者自行加添Collider作为物体点击交互区"
                                                    , "自动创建", "取消", "自行添加");
                if (inspectIndex == 0)
                {
                    createColliderRecords.Clear();
                    CreateColliderWithRender(prefabs, ref createColliderRecords);
                    tips = "自动创建完成,点击检查\n";
                    tips += $"遍历{createColliderRecords.Count}个物体\n";
                    tips += $"其中有{createColliderRecords.Where(x => x == "H").ToList().Count}个有Mesh但本身附带Collider\n";
                    tips += $"创建{createColliderRecords.Where(x => x == "MR").ToList().Count}个MeshCollider\n";
                    tips += $"创建{createColliderRecords.Where(x => x == "SMR").ToList().Count}个BoxCollider\n";

                    //创建记录待续
                    //---------------------》》》》》》》》》》》》》》》》》》》》》》》    



                }
                else if (inspectIndex == 2)
                {
                    tips = "在选择打包物体上自行添加Collider\n并且重复制作Prefab保存，然后拖拽到打包选择区进行打包检查";
                    UnityEngine.Debug.LogError("在选择打包物体上自行添加Collider\n并且重复制作Prefab保存，然后拖拽到打包选择区进行打包检查");
                }
            }
            else if (!CheckOtherConfig())
            {
                tips = "保存路径或者打包平台配置错误，重新配置";
            }
        }
        GUILayout.Space(spaceValue);
        if (canBuildt)
        {
            if (GUILayout.Button("打包", GUILayout.Width(200)))
            {
                canBuildt = false;
                Building();
                Reset();
                prefabs = null;
            }
        }
        if (changed)
        {
            sw = new StreamWriter(new FileStream(configFilePath, FileMode.OpenOrCreate));
            sw.WriteLine(unityInstallPath);
            sw.WriteLine(scriptPath);
            sw.WriteLine(savePath);
            sw.Flush();
            sw.Close();
            changed = false;
        }
    }
    //打包
    private void Building() {

        //在生成记录摘要前配置BundleTrigger记录
        //BundleTrigger记录会标记被操作物体(修改其名称，给其挂载对应执行脚本)
        //配置后在进行脚本配置信息
        if (hasBundleTrigger)
        {
            GenerateBundleTriggerSummary();
        }
        if (hasMono)
        {
            GenerateScript(unityInstallPath);
        }
        //打包前处理
        PreBuidleHandler();
        AssetDatabase.Refresh();
        //脚本挂载索引
        GenerateSummary(bundleRecordFile);
        //生成触发器读取值的json文件
        CreateEventTriggerInfoJsonConfig(eventTriggerConfigPath);
        AssetDatabase.Refresh();

        if (prefabs != null)
        {
            List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
            if (prefabs.GetType() == typeof(GameObject))
            {
                //string copyPath = AssetDatabase.GetAssetPath(prefabs);
                GameObject obj = prefabs;
                //MonoBehaviour[] compoment = obj.GetComponentsInChildren<MonoBehaviour>();
                //if (compoment != null && compoment.Length > 0)
                //{
                //    for (int j = 0; j < compoment.Length; j++)
                //    {
                //        GameObject.DestroyImmediate(compoment[j], true);
                //    }
                //}
                //TestAsset[0] = AssetDatabase.GetAssetPath(obj);
                //UnityEngine.Debug.Log(TestAsset[0]);
                builds.Add(obj);
            }
            AssetDatabase.Refresh();
            if (hasMono)
            {
                builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/Resources/BundleConfig/output.bytes", typeof(UnityEngine.TextAsset)));
            }
            if (hasBundleTrigger)
            {
                builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/Resources/BundleConfig/eventTriggerConfig.json", typeof(UnityEngine.TextAsset)));
            }
            builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/Resources/BundleConfig/bundleRecord.bytes", typeof(UnityEngine.TextAsset)));
            string projectName = savePath.Substring(savePath.LastIndexOf("/") + 1, savePath.LastIndexOf(".") - savePath.LastIndexOf("/") - 1);
            string dirPath = savePath.Substring(0, savePath.LastIndexOf("/"));
            UnityEngine.Debug.Log("dir path " + dirPath + " projectName " + projectName);


            string timeTick = System.DateTime.UtcNow.Ticks.ToString();
            string gameobjectScale = CalculateScale(prefabs);
            if (androidBuild)
            {
                BuildPipeline.BuildAssetBundle(prefabs, builds.ToArray(), dirPath + "/" + projectName + "_android_" + timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android);
                CreateJsonConfig(dirPath, projectName, "android", timeTick, gameobjectScale);
            }
            if (pcBuild)
            {
                BuildPipeline.BuildAssetBundle(prefabs, builds.ToArray(), dirPath + "/" + projectName + "_pc_" + timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows64);
                CreateJsonConfig(dirPath, projectName, "pc", timeTick, gameobjectScale);
            }
            if (hololensBuild)
            {
                BuildPipeline.BuildAssetBundle(prefabs, builds.ToArray(), dirPath + "/" + projectName + "_hololens_" + timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.WSAPlayer);
                CreateJsonConfig(dirPath, projectName, "hololens", timeTick, gameobjectScale);
            }
            //自动打开文件目录
            OpenFile(dirPath);
        }
    }
    //打包前处理
    private void PreBuidleHandler() {
        BundleEventTriggerDesigner[] bundleEventTriggers = prefabs.GetComponentsInChildren<BundleEventTriggerDesigner>();
        if (bundleEventTriggers != null && bundleEventTriggers.Length > 0)
        {
            for (int i = 0; i < bundleEventTriggers.Length; i++)
            {
                BundleEventTriggerDesigner betd = bundleEventTriggers[i];
                //销毁设计师
                DestroyImmediate(betd, true);
            }
        }
    }
    //检查Collider
    private bool CheckCollider(GameObject obj) {
        bool hasColliders = true;
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Collider collider = renderers[i].GetComponent<Collider>();
            if (collider == null)
            {
                hasColliders = false;
            }
        }
        return hasColliders;
    }
    //检查Mono
    private void CheckMonoForGameObject(GameObject target) {
        if (target != null)
        {
            MonoBehaviour[] monoBehaviours = target.GetComponentsInChildren<MonoBehaviour>(true);
            List<MonoBehaviour> customMB = new List<MonoBehaviour>();
            foreach (MonoBehaviour mb in monoBehaviours)
            {
                string ns = mb.GetType().Namespace;

                if (ns == null || (!ns.Contains("UnityEngine") && !ns.Contains("UnityEditor")))
                {
                    customMB.Add(mb);
                }
            }
            monoBehaviours = customMB.ToArray();
            if (monoBehaviours.Length > 0)
            {
                hasMono = true;
            }
            else
            {
                hasMono = false;
            }
        }
    }
    //检查其他配置
    private bool CheckOtherConfig() {
        bool ready = false;
        if ((androidBuild || pcBuild || hololensBuild) && Directory.Exists(savePath.Substring(0, savePath.LastIndexOf("/"))))
        {
            ready = true;
            return ready;
        }
        return ready;
    }
    //检查BundleEventTrigger组件
    private void CheckBundleEventTriggerComponent() {
        BundleEventTriggerDesigner[] bundleEventTriggers = prefabs.GetComponentsInChildren<BundleEventTriggerDesigner>();
        if (bundleEventTriggers != null && bundleEventTriggers.Length > 0)
        {
            hasBundleTrigger = true;
        }
        else
        {
            hasBundleTrigger = false;
        }
    }
    //创建Collider和记录
    private void CreateColliderWithRender(GameObject root, ref List<string> record) {
        Transform target = null;
        target = root.transform;
        EditorUtility.SetDirty(root);
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            string str = CreateCollider(renderers[i].transform);
            record.Add(str);
        }
    }
    /// <summary>
    /// 指定Transform上创建Collider
    /// </summary>
    /// <param name="transform">目标</param>
    /// <returns>创建记录
    /// "-"遍历到但没有达到创建条件
    /// "MR" MeshRenderer
    /// "SMR" SkinnedMeshRenderer
    /// "SR" SpriteRenderer
    /// "SR" SpriteRenderer
    /// "H"  有MeshRenderer但是也有自带的Collider
    /// </returns>
    private string CreateCollider(Transform transform) {
        string record = "-";
        if (transform != null)
        {
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer == null)
            {
                return record;
            }
            Type type = renderer.GetType();
            GameObject obj = renderer.gameObject;
            Collider _self = obj.GetComponent<Collider>();
            //模型
            if (type == typeof(MeshRenderer))
            {
                if (_self == null)
                {
                    obj.AddComponent<MeshCollider>();
                    UnityEngine.Debug.Log("创建MeshCollider ---- For" + obj.name);
                    record = "MR";//MeshRenderer
                    return record;
                }
                else
                {
                    UnityEngine.Debug.Log("有MeshRenderer但是也有自带的Collider" + obj.name);
                    record = "H";//有MeshRenderer但是也有自带的Collider
                    return record;
                }
            }
            //蒙皮
            else if (type == typeof(SkinnedMeshRenderer))
            {
                //需要判断大小
                if (_self == null)
                {
                    obj.AddComponent<BoxCollider>();
                    record = "SMR";//SkinnedMeshRenderer
                }
                return record;
            }
            //图片
            else if (type == typeof(SpriteRenderer))
            {
                //需要判断大小？？？？
                if (_self == null)
                {
                    obj.AddComponent<BoxCollider>();
                    record = "SR";//SpriteRenderer
                }
                return record;
            }
            //特效？？？？
            else if (type == typeof(SpriteRenderer))
            {
                //需要判断大小？？？？
                if (_self == null)
                {
                    obj.AddComponent<BoxCollider>();
                    record = "SR";//SpriteRenderer
                }
                return record;
            }
        }
        return record;
    }


    //计算bundle包围盒大小
    private string CalculateScale(GameObject obj) {
        string scaleStr = "0_0_0";
        Collider[] cls = obj.GetComponentsInChildren<Collider>(true);
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        foreach (Collider c in cls)
        {
            bounds.Encapsulate(c.bounds);
        }
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Bounds temp = r.bounds;
            bounds.Encapsulate(temp);
        }
        scaleStr = FloatHandle(bounds.size.x) + "_" + FloatHandle(bounds.size.y) + "_" + FloatHandle(bounds.size.z);
        return scaleStr;
    }

    private string FloatHandle(float x) {
        return (Mathf.Round(x * 100f) / 100f).ToString();
    }

    private void CreateJsonConfig(string dirPath, string projectName, string platForm, string projectId, string gameobjectScale) {
        if (!isCreateJson)
        {
            UnityEngine.Debug.Log("不生成Json文件");
            return;
        }
        sw = new StreamWriter(new FileStream(dirPath + "/" + projectName + "_" + platForm + ".json", FileMode.OpenOrCreate));
        DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(Project));
        Project p = new Project();
        p.ProjectName = projectName;
        p.ProjectId = projectId;
        //2018.12.14  赵子夜修改json文件保存路径为相对路径 
        //安卓本地加载bundle时 ab放置位置及Application.persistentDataPath
        //供粘贴用      "/storage/emulated/0/Android/data/com.lenovo.starview/files/"
        p.FilePath = projectName + "_" + platForm + "_" + projectId + "-" + gameobjectScale + ".assetbundle";
        MemoryStream ms = new MemoryStream();
        dc.WriteObject(ms, p);
        byte[] dataBytes = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(dataBytes, 0, (int)ms.Length);
        sw.WriteLine(Encoding.UTF8.GetString(dataBytes));
        sw.Flush();
        sw.Close();
    }
    /// <summary>
    /// 生成prefab物体上所有BundleEventTrigger组件对应的配置信息
    /// 保存到一个json文件一起打入Bundle
    /// </summary>
    private void GenerateBundleTriggerSummary() {
        //获取所有BundleEventTrigger组件
        BundleEventTriggerDesigner[] bundleEventTriggers = prefabs.GetComponentsInChildren<BundleEventTriggerDesigner>();
        allTriggerToDesingerConfig.allJson = new List<triggerToDesingerJson>();
        //给持续数据赋值以及修改物体标号
        for (int i = 0;i < bundleEventTriggers.Length; i++)
        {
            //标记设计师
            GameObject desingerObj = bundleEventTriggers[i].gameObject;
            desingerObj.name += string.Format("--D[{0}]",i);//Desinger
            EditorUtility.SetDirty(desingerObj);
            triggerToDesingerJson temp = new triggerToDesingerJson();
            
            //增加触发器
            BundleEventTrigger bet = desingerObj.AddComponent<BundleEventTrigger>();
            //赋值形成可预览的物体
            bet.triggers = bundleEventTriggers[i].bundleEventTriggerInfos;
            //给设计师中所有被操作物体做标记
            if (bundleEventTriggers[i].bundleEventTriggerInfos != null && bundleEventTriggers[i].bundleEventTriggerInfos.Count > 0)
            {
                for (int j = 0; j < bundleEventTriggers[i].bundleEventTriggerInfos.Count; j++)
                {
                    //属于设计师名 + 标号
                    string str = bundleEventTriggers[i].bundleEventTriggerInfos[j].target.name;
                    //如果有重复事件添加在同一个物体上，Desinger只需要标记一次
                    if (str.Contains("--D["))
                    {
                        bundleEventTriggers[i].bundleEventTriggerInfos[j].target.name += string.Format("->{0}[{1}]", bundleEventTriggers[i].bundleEventTriggerInfos[j].method.name, j);
                    }
                    else
                    {
                        bundleEventTriggers[i].bundleEventTriggerInfos[j].target.name += string.Format("{0}->{1}[{2}]", desingerObj.name, bundleEventTriggers[i].bundleEventTriggerInfos[j].method.name, j);
                    } 
                }
            }
            //修改完成后记录
            temp.objName = desingerObj.name;
            temp.bundleEventTriggerDesigners = bundleEventTriggers[i].GetJsonInfo();
            //修改后保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allTriggerToDesingerConfig.allJson.Add(temp);
        }
    }


    private void GenerateSummary(string bundleRecordFile) {
        sw = new StreamWriter(new FileStream(bundleRecordFile, FileMode.Create));

        List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
        sw.WriteLine(prefabs.name);

        if (prefabs.GetType() == typeof(GameObject))
        {
            GameObject obj = prefabs;
            MonoBehaviour[] compoment = obj.GetComponentsInChildren<MonoBehaviour>(true);
            List<MonoBehaviour> customMB = new List<MonoBehaviour>();
            foreach (MonoBehaviour mb in compoment)
            {
                string ns = mb.GetType().Namespace;

                if (ns == null || (!ns.Contains("UnityEngine") && !ns.Contains("UnityEditor")))
                {
                    UnityEngine.Debug.Log(mb.name + " costom namespace " + ns);
                    customMB.Add(mb);
                }
            }

            compoment = customMB.ToArray();
            UnityEngine.Debug.Log("compoment num" + compoment.Length);
            sw.WriteLine(compoment.Length);
            if (compoment != null && compoment.Length > 0)
            {
                for (int j = 0; j < compoment.Length; j++)
                {
                    if (compoment[j] == null) continue;
                    UnityEngine.Debug.Log(compoment[j].GetType().Name);
                    GameObject target = compoment[j].gameObject;
                    string compomentPath = target.name;
                    while (target.transform.parent != null)
                    {
                        target = target.transform.parent.gameObject;
                        compomentPath = target.name + "/" + compomentPath;
                    }
                    sw.WriteLine(compoment[j].GetType().Name);
                    sw.WriteLine(compomentPath);

                    GameObject.DestroyImmediate(compoment[j], true);
                }
            }
            builds.Add(obj);
        }
        sw.Flush();
        sw.Close();
    }

    private void CreateEventTriggerInfoJsonConfig(string dirPath) {

        //覆盖生成
        swEventConfig = new StreamWriter(new FileStream(dirPath, FileMode.Create));
        DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(AllTriggerToDesingerJson));
        AllTriggerToDesingerJson attdc = new AllTriggerToDesingerJson();
        attdc = allTriggerToDesingerConfig;
        MemoryStream ms = new MemoryStream();
        dc.WriteObject(ms, attdc);
        byte[] dataBytes = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(dataBytes, 0, (int)ms.Length);
        swEventConfig.WriteLine(Encoding.UTF8.GetString(dataBytes));
        swEventConfig.Flush();
        swEventConfig.Close();
    }

    private void GenerateScript(string unityInstallPath) {
        if (!string.IsNullOrEmpty(unityInstallPath))
        {
            string mcsPath = FindMcsPath(unityInstallPath);
            UnityEngine.Debug.Log(mcsPath);
            if (!mcsPath.EndsWith("mcs.bat"))
            {
                UnityEngine.Debug.LogError("mcs not find,check your unity install path!");
                return;
            }
            string cmd = "/c echo 'start generate scripte:'&& " + "\"" + mcsPath + "\"";
            ArrayList dllList = new ArrayList();
            dllList.Add(new FileInfo(Application.dataPath + "/Plugins/BundleEventInfoBase.dll"));
            if (!string.IsNullOrEmpty(unityInstallPath))
            {
                dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/Managed/UnityEngine.dll"));
                dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll"));
                dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/Managed/UnityEditor.dll"));
            }
            if (dllList != null && dllList.Count > 0)
            {
                foreach (FileInfo f in dllList)
                {
                    UnityEngine.Debug.Log(f.FullName);
                    if (f.FullName.Contains(".dll"))
                    {
                        cmd += " -r:";
                        cmd = cmd + "\"" + f.FullName + "\" ";
                    }
                }
            }
            //获取所有要打包脚本的路径和脚本名称
            ArrayList scriptPaths = GetBundleScripts();
            //固定脚本
            //string basePath = @"E:\Projects\AssetBundleBuilder\Assets\BundleScript\BundleInternalScripts\BundleEventInfoBase.cs";
            //scriptPaths.Add(basePath);
            string basePath = @"E:\Projects\AssetBundleBuilder\Assets\BundleScript\BundleInternalScripts\BundleEventTrigger.cs";
            scriptPaths.Add(basePath);
            basePath = @"E:\Projects\AssetBundleBuilder\Assets\BundleScript\BundleInternalScripts\BundleEventTriggerDesigner.cs";
            scriptPaths.Add(basePath);
            basePath = @"E:\Projects\AssetBundleBuilder\Assets\BundleScript\BundleInternalScripts\AllTriggerToDesingerJson.cs";
            scriptPaths.Add(basePath);
            if (scriptPaths != null && scriptPaths.Count > 0)
            {
                cmd += " -target:library ";
                foreach (string f in scriptPaths)
                {
                    if (f.EndsWith(".cs"))
                        cmd = cmd + f + " ";
                }
            }
            string byteFileFullName = "\"" + configPath + "/" + dllName + "\"";
            cmd += " -out:" + byteFileFullName;
            UnityEngine.Debug.Log(cmd);
            Process.Start("cmd", cmd);
            generateScript = true;
        }
    }

    static string FindMcsPath(string rootPath) {
        string mcsPath = rootPath + "/Editor/Data/MonoBleedingEdge/bin/mcs.bat";
        FileInfo mcs = new FileInfo(mcsPath);
        if (mcs.Exists)
            return mcsPath;
        ArrayList allfiles = GetAllFiles(rootPath);
        foreach (FileInfo f in allfiles)
        {
            if (f.FullName.EndsWith("mcs.bat"))
                return f.FullName;
        }
        return "";
    }
    /// <summary>
    /// 获取所有要打包脚本的资源路径
    /// </summary>
    /// <returns></returns>
    private ArrayList GetBundleScripts() {
        ArrayList fileList = new ArrayList();
        BundleEventTriggerDesigner[] bundleEventTriggers = prefabs.GetComponentsInChildren<BundleEventTriggerDesigner>();
        if (bundleEventTriggers != null  && bundleEventTriggers.Length > 0)
        {
            for (int i = 0; i < bundleEventTriggers.Length; i++)
            {
                BundleEventTriggerDesigner betd = bundleEventTriggers[i];
                for (int j = 0; j < betd.bundleEventTriggerInfos.Count; j++)
                {
                    BundleEventTriggerInfo beti = betd.bundleEventTriggerInfos[j];
                    string path = AssetDatabase.GetAssetPath(beti.method);
                    //转换目录格式
                    string temp = Application.dataPath;
                    path = temp.Replace("/Assets", "") + "\\" + path;
                    path = path.Replace("/", "\\");
                    if (!fileList.Contains(path))
                    {
                        fileList.Add(path);
                    }
                }
            }
        }
        return fileList;
    }
    static ArrayList GetAllFiles(string path, string fileExtension = "") {
        ArrayList fileList = new ArrayList();
        DirectoryInfo info = new DirectoryInfo(path);
        if (!info.Exists)
            return fileList;
        int i = 0;
        FileInfo[] files = info.GetFiles();
        foreach (FileInfo f in files)
        {
            i++;
            UnityEngine.Debug.Log(f.FullName + " " + i);
            if (f.FullName.EndsWith(fileExtension)) fileList.Add(f);

        }
        //if (files != null)
        //fileList.AddRange (files);
        DirectoryInfo[] dirs = info.GetDirectories();
        foreach (DirectoryInfo d in dirs)
        {
            i++;
            UnityEngine.Debug.Log(d.FullName + " " + i);
        }
        if (dirs == null)
            return fileList;
        foreach (DirectoryInfo d in dirs)
        {
            fileList.AddRange(GetAllFiles(d, fileExtension).ToArray());
        }
        UnityEngine.Debug.Log(fileList.Count);
        return fileList;
    }

    static ArrayList GetAllFiles(DirectoryInfo info, string fileExtension = "") {
        if (!info.Exists)
            return null;
        ArrayList fileList = new ArrayList();
        FileInfo[] files = info.GetFiles();
        int i = 0;
        if (files != null)
        {
            foreach (FileInfo f in files)
            {
                i++;
                UnityEngine.Debug.Log(f.FullName + " " + i);
                if (f.FullName.EndsWith(fileExtension))
                    fileList.Add(f);
            }
        }

        DirectoryInfo[] dirs = info.GetDirectories();
        foreach (DirectoryInfo d in dirs)
        {
            i++;
            UnityEngine.Debug.Log(d.FullName + " " + i);
        }
        if (dirs == null)
            return fileList;
        foreach (DirectoryInfo d in dirs)
        {
            ArrayList temp = GetAllFiles(d, fileExtension);
            if (temp != null && temp.Count > 0) fileList.Add(temp.ToArray());
        }

        UnityEngine.Debug.Log(fileList.Count);
        return fileList;
    }
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    public static bool GetOFN([In, Out] OpenFileName ofn) {
        return GetOpenFileName(ofn);
    }
    private void OpenFile(string path) {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "All Files\0*.*\0\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        string _path = path.Replace('/', '\\');
        //默认路径
        ofn.initialDir = path;
        //ofn.initialDir = "D:\\MyProject\\UnityOpenCV\\Assets\\StreamingAssets";
        ofn.title = "Open Project";
        ofn.defExt = "JPG";//显示文件的类型  注意 一下项目不一定要全选 但是0x00000008项不要缺少
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        System.Diagnostics.Process.Start(ofn.initialDir);
    }
    private void Reset() {
        init = true;
        configPath = Application.dataPath + "/Resources/BundleConfig";
        configFilePath = Application.dataPath + "/Resources/BundleConfig/config.bytes";
        bundleRecordFile = Application.dataPath + "/Resources/BundleConfig/bundleRecord.bytes";
        eventTriggerConfigPath = Application.dataPath + "/Resources/BundleConfig/eventTriggerConfig.json";
        if (File.Exists(configFilePath))
        {
            sr = new StreamReader(new FileStream(configFilePath, FileMode.Open));
            unityInstallPath = sr.ReadLine();
            scriptPath = sr.ReadLine();
            savePath = sr.ReadLine();
            sr.Close();
        }
        tips += "选择Unity安装路径(Editor文件夹的上级目录)\n";
        tips += "选择打包脚本文件夹\n";
        tips += "选择Bundle保存文件夹\n";
        tips += "选择打包Prefab\n";
        tips += "选择打包对应平台\n";
        tips += "点击打包检查\n";
        inspectIndex = 1;
        androidBuild = true;
        pcBuild = true;
        withoutColldider = false;
        canBuildt = false;
        hasMono = false;
        hasBundleTrigger = false;
        allTriggerToDesingerConfig = new AllTriggerToDesingerJson();
        isCreateJson = false;
    }
    /// <summary>
    /// 通过注册表获取安装路径
    /// </summary>
    private static string FindInstallPath() {
        string str = null;
        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Unity Technologies\Installer\Unity");
        str = key.GetValue("Location x64").ToString();
        if (string.IsNullOrEmpty(str))
        {
            UnityEngine.Debug.LogWarning("请检查Unity安装路径或者手动设置");
        }
        return str;
    }

   
}


//#region 储存所有BundleTrigger配置信息类
///// <summary>
///// 储存所有BundleTrigger配置信息类
///// </summary>
//public class BundleEventInfo {
//    public List<BundleTriggerInfo> bundleTriggerEvents;
//}
///// <summary>
///// 每个BundleTrigger组件的信息
///// </summary>
//public class BundleTriggerInfo {
//    public string triggerGameobjectName;
//    public List<EventInfo> eventInfos;
//}
///// <summary>
///// 每种事件的信息
///// </summary>
//public class EventInfo {
//    //public BundleEventTriggerType EventID;//GazeOn GazeOff GazeClick
//    public List<PersistentCalls> Caslls;
//}
///// <summary>
///// 注册的单个持久事件信息
///// </summary>
//public class PersistentCalls {
//    public string Target;//对应UnityEventBase.Delegates.AllCallBack.PersistentCalls.Calls.Element.Tartget
//    public string MethodName;//对应UnityEventBase.Delegates.AllCallBack.PersistentCalls.Calls.Element.MethodName
//    public PersistentListenerMode Mode;//对应UnityEventBase.Delegates.AllCallBack.PersistentCalls.Calls.Element.Mode
//}
//#endregion

public class Project {
    public string ProjectName;
    public string ProjectId;
    public string FilePath;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName {
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

