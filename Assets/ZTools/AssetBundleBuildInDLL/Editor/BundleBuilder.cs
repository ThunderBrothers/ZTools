using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

namespace ZTools.AssetBundleBuildInDLL
{
    public class BundleBuilder : EditorWindow
    {
        #region 参数
        /// <summary>
        /// Unity安装路径
        /// </summary>
        private static string unityInstallPath;
        private string tempPath = "";
        /// <summary>
        ///  Resources中途路径
        /// </summary>
        private string resourcesPath;
        /// <summary>
        /// Bundle配置路径
        /// </summary>
        private string bundleConfig;
        /// <summary>
        /// 工具配置路径保存目录
        /// </summary>
        private string configFilePath;
        /// <summary>
        /// AssetBundle保存路径
        /// </summary>
        private string outputPath = "";
        /// <summary>
        /// dll保持到的二级制文件
        /// </summary>
        private string dllConfig;
        /// <summary>
        /// 配置EventTrigger配置json路径
        /// </summary>
        private string eventTriggerConfigPath = "";
        /// <summary>
        /// 要打包的脚本文件夹
        /// </summary>
        private string scriptsPath = "";
        /// <summary>
        /// 要打包的物体
        /// </summary>
        private GameObject tagetObj;
        /// <summary>
        /// AB包平台
        /// </summary>
        private BuildTarget platform = BuildTarget.StandaloneWindows64;
        private StreamReader sr = null;
        private StreamWriter sw = null;
        private StreamWriter swEventConfig = null;
        /// <summary>
        /// 生成记录
        /// </summary>
        private string tips;
        #endregion
        #region 临时变量
        private GameObject lastPrefab;
        private bool changed = false;
        private bool canBuildt = false;
        private bool hasBundleTrigger = false;
        private bool hasMono = false;
        private int inspectIndex;
        private List<string> createColliderRecords = new List<string>();
        private AllTriggerToDesingerJson allTriggerToDesingerConfig = new AllTriggerToDesingerJson();
        private string dllName = "output.bytes";
        #endregion

        [MenuItem("ZTools/BundleBuildInDLL")]
        static void BundleBuildWindow()
        {
            GetWindow(typeof(BundleBuilder));
            FindInstallPath();
        }
        private void OnEnable() {
            Reset();
        }
        private void OnDisable() {
            Reset();
            sr?.Close();
            sw?.Close();
            swEventConfig?.Close();
        }
        private void OnGUI() {
            #region 统一
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("帮助"))
            {
                Help();
            }
            if (GUILayout.Button("重置工具"))
            {
                Reset();
            }
            GUILayout.EndHorizontal();
            #endregion
            //安装目录
            GUILayout.BeginHorizontal();
            GUILayout.Label("Unity安装目录", GUILayout.Width(80));
            EditorGUILayout.SelectableLabel(unityInstallPath);
            GUILayout.EndHorizontal();
            if (CreateButtonAndSelectPath<string>("设置Unity安装目录", "Unity安装路径", "您没有选择或者选择错误的安装路径", ref unityInstallPath, "/unity"))
            {
                changed = true;
            }

            //脚本目录
            GUILayout.BeginHorizontal();
            GUILayout.Label("脚本目录", GUILayout.Width(80));
            EditorGUILayout.SelectableLabel(scriptsPath);
            GUILayout.EndHorizontal();
            if (CreateButtonAndSelectPath<string>("选择脚本路径", "选择要打包的脚本文件夹", "您没有选择或者选择错误", ref scriptsPath))
            {
                changed = true;
            }
           
            //保存路径
            GUILayout.BeginHorizontal();
            GUILayout.Label("AssetBundle保存目录", GUILayout.Width(120));
            EditorGUILayout.SelectableLabel(outputPath);
            GUILayout.EndHorizontal();
            if (CreateButtonAndSelectSavePath<string>("设置AssetBundle保存目录", "AssetBundle保存目录", "", "请重新选择路径", ref outputPath))
            {
                changed = true;
            }

            //物体
            tagetObj = EditorGUILayout.ObjectField("打包物体", tagetObj, typeof(GameObject),false) as GameObject;
            if (lastPrefab != tagetObj)
            {
                lastPrefab = tagetObj;
                Reset();
                CheckMonoForGameObject(lastPrefab);
            }


            //打包平台
            platform = (BuildTarget)EditorGUILayout.EnumPopup("打包平台", platform);

            //打包
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.BeginVertical();
            //检查是否可以进行打包操作
            if (GUILayout.Button("打包检查", GUILayout.Width(200)))
            {
                if (tagetObj == null)
                {
                    UnityEngine.Debug.LogError("打包物体空");
                    tips = "打包物体空\n";
                }
                else if (CheckCollider(tagetObj) && CheckOtherConfig())
                {
                    //检查是否有BundleTrigger组件
                    CheckBundleEventTriggerComponent();
                    canBuildt = true;
                    tips = "准备完成可以打包";

                }
                else if (!CheckCollider(tagetObj))
                {
                    canBuildt = false;
                    inspectIndex = EditorUtility.DisplayDialogComplex("提示", "检查到Prefab无Collider可以点击自动创建，或者自行加添Collider作为物体点击交互区"
                                                        , "自动创建", "取消", "自行添加");
                    if (inspectIndex == 0)
                    {
                        createColliderRecords.Clear();
                        CreateColliderWithRender(tagetObj, ref createColliderRecords);
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
            GUILayout.Space(10);
            if (canBuildt)
            {
                if (GUILayout.Button("打包", GUILayout.Width(200)))
                {
                    canBuildt = false;
                    Building();
                    Reset();
                    tagetObj = null;
                }
            }
            EditorGUILayout.EndVertical();
            //提示框
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Bundle信息表");
            GUILayout.Space(5);
            EditorGUILayout.SelectableLabel(tips, GUILayout.Height(80));
            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //记录有改变
            if (changed)
            {
                sw = new StreamWriter(new FileStream(configFilePath, FileMode.OpenOrCreate));
                sw.WriteLine(unityInstallPath);
                sw.WriteLine(scriptsPath);
                sw.WriteLine(outputPath);
                sw.Flush();
                sw.Close();
                changed = false;
            }
        }
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
            GenerateSummary(bundleConfig);
            //生成触发器读取值的json文件
            CreateEventTriggerInfoJsonConfig(eventTriggerConfigPath);
            AssetDatabase.Refresh();

            if (tagetObj != null)
            {
                List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
                if (tagetObj.GetType() == typeof(GameObject))
                {
                    //string copyPath = AssetDatabase.GetAssetPath(prefabs);
                    GameObject obj = tagetObj;
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
                    builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/ZTools/AssetBundleBuildInDLL/Resources/BundleConfig/output.bytes", typeof(UnityEngine.TextAsset)));
                }
                if (hasBundleTrigger)
                {
                    builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/ZTools/AssetBundleBuildInDLL/Resources/BundleConfig/eventTriggerConfig.json", typeof(UnityEngine.TextAsset)));
                }
                builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/ZTools/AssetBundleBuildInDLL/Resources/BundleConfig/bundleRecord.bytes", typeof(UnityEngine.TextAsset)));
                string projectName = outputPath.Substring(outputPath.LastIndexOf("/") + 1, outputPath.LastIndexOf(".") - outputPath.LastIndexOf("/") - 1);
                string dirPath = outputPath.Substring(0, outputPath.LastIndexOf("/"));
                UnityEngine.Debug.Log("输出路径 " + dirPath + " projectName " + projectName);


                string timeTick = System.DateTime.UtcNow.Ticks.ToString();
                string gameobjectScale = CalculateScale(tagetObj);

                BuildPipeline.BuildAssetBundle(tagetObj, builds.ToArray(), dirPath + "/" + projectName + "-" +platform.ToString() + timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, platform);
                //自动打开文件目录
                OpenFile(dirPath);
            }
        }
        //打包前处理
        private void PreBuidleHandler() {
            BundleEventTriggerDesigner[] bundleEventTriggers = tagetObj.GetComponentsInChildren<BundleEventTriggerDesigner>();
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
            if (platform != BuildTarget.NoTarget && Directory.Exists(outputPath.Substring(0, outputPath.LastIndexOf("/"))))
            {
                ready = true;
                return ready;
            }
            return ready;
        }
        //检查BundleEventTrigger组件
        private void CheckBundleEventTriggerComponent() {
            BundleEventTriggerDesigner[] bundleEventTriggers = tagetObj.GetComponentsInChildren<BundleEventTriggerDesigner>();
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
        /// <summary>
        /// 生成prefab物体上所有BundleEventTrigger组件对应的配置信息
        /// 保存到一个json文件一起打入Bundle
        /// </summary>
        private void GenerateBundleTriggerSummary() {
            //获取所有BundleEventTrigger组件
            BundleEventTriggerDesigner[] bundleEventTriggers = tagetObj.GetComponentsInChildren<BundleEventTriggerDesigner>();
            allTriggerToDesingerConfig.allJson = new List<triggerToDesingerJson>();
            //给持续数据赋值以及修改物体标号
            for (int i = 0; i < bundleEventTriggers.Length; i++)
            {
                //标记设计师
                GameObject desingerObj = bundleEventTriggers[i].gameObject;
                desingerObj.name += string.Format("--D[{0}]", i);//Desinger
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
        private void Reset()
        {
            resourcesPath = Application.dataPath + "/ZTools/AssetBundleBuildInDLL/Resources/BundleConfig";
            bundleConfig = resourcesPath + "/bundleRecord.bytes";
            configFilePath = resourcesPath + "/config.bytes";
            eventTriggerConfigPath = resourcesPath + "/eventTriggerConfig.json";
           
            dllConfig = resourcesPath + "/output.bytes";
           
            if (File.Exists(configFilePath))
            {
                sr = new StreamReader(new FileStream(configFilePath, FileMode.Open));
                unityInstallPath = sr.ReadLine();
                scriptsPath = sr.ReadLine();
                outputPath = sr.ReadLine();
                sr.Close();
            }
            tips += "选择Unity安装路径(Editor文件夹的上级目录)\n";
            tips += "选择打包脚本文件夹\n";
            tips += "选择Bundle保存文件夹\n";
            tips += "选择打包Prefab\n";
            tips += "选择打包对应平台\n";
            tips += "点击打包检查\n";

            tempPath = "";
            platform = BuildTarget.StandaloneWindows64;
            inspectIndex = 1;
            canBuildt = false;
            hasMono = false;
            hasBundleTrigger = false;
            allTriggerToDesingerConfig = new AllTriggerToDesingerJson();
            changed = false;
        }

        private void Help()
        {
            
        }
        /// <summary>
        /// 通过注册表获取安装路径
        /// </summary>
        private static void FindInstallPath()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Unity Technologies\Installer\Unity");
            if (key != null)
            {
                unityInstallPath = key?.GetValue("Location x64").ToString();
            }
            if (string.IsNullOrEmpty(unityInstallPath))
            {
                UnityEngine.Debug.LogWarning("系统没有找到默认的Unity安装路径，请检查Unity安装路径或者手动设置");
            } 
        }
        /// <summary>
        /// 创建一个Button打开一个选择目录界面并且修改处理后的路径
        /// </summary>
        /// <param name="buttonTitle">按钮名称</param>
        /// <param name="folderTitle">Foler标题</param>
        /// <param name="checker">返回目录检查</param>
        /// <param name="warning">检查未通过提示</param>
        /// <param name="value">返回值</param>
        private bool CreateButtonAndSelectPath<T>(string buttonTitle,string folderTitle,string warning,ref T value, string checker = "")
        {
            bool change = false;
            if (GUILayout.Button(buttonTitle, GUILayout.Width(200)))
            {
                tempPath = EditorUtility.OpenFolderPanel(folderTitle, "", "");
                //路径检查小写checker
                if (!string.IsNullOrEmpty(tempPath) && (checker == "" || tempPath.ToLower().Contains(checker)))
                {
                    value = (T)Convert.ChangeType(tempPath,typeof(T));
                    change = true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning(warning);
                }
            }
            return change;
        }
        /// <summary>
        /// 创建一个Button打开一个选择目录界面并且修改处理后的路径附带文件名称
        /// </summary>
        /// <param name="buttonTitle">按钮名称</param>
        /// <param name="folderTitle">Foler标题</param>
        /// <param name="checker">返回目录检查</param>
        /// <param name="warning">检查未通过提示</param>
        /// <param name="value">返回值</param>
        private bool CreateButtonAndSelectSavePath<T>(string buttonTitle, string folderTitle, string checker, string warning, ref T value) {
            bool change = false;
            if (GUILayout.Button(buttonTitle, GUILayout.Width(200)))
            {
                tempPath = EditorUtility.SaveFilePanel(folderTitle, "", "Unnamed", "assetbundle");
                //路径检查小写checker
                if (!string.IsNullOrEmpty(tempPath) && tempPath.ToLower().Contains(checker))
                {
                    value = (T)Convert.ChangeType(tempPath, typeof(T));
                    change = true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning(warning);
                }
            }
            return change;
        }
        /// <summary>
        /// 处理脚本生成dll
        /// </summary>
        /// <param name="dllPath">批处理程序生的保存dll的二级制文件目录</param>
        /// <param name="scriptPath">打包的脚本文件夹</param>
        /// <param name="unityInstallPath">安装目录索引msc.bat</param>
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
                string toolsPath = Application.dataPath + "/ZTools/AssetBundleBuildInDLL/";
                dllList.Add(new FileInfo(toolsPath + "Plugins/BundleEventInfoBase.dll"));
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
                string basePath = toolsPath + "Scripts/BundleInternalScripts/BundleEventTrigger.cs";
                scriptPaths.Add(basePath);
                basePath = toolsPath + "Scripts/BundleInternalScripts/BundleEventTriggerDesigner.cs";
                scriptPaths.Add(basePath);
                basePath = toolsPath + "Scripts/BundleInternalScripts/AllTriggerToDesingerJson.cs";
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
                string byteFileFullName = "\"" + resourcesPath + "/" + dllName + "\"";
                cmd += " -out:" + byteFileFullName;
                UnityEngine.Debug.Log(cmd);
                Process.Start("cmd", cmd);
            }
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
        /// <summary>
        /// 生成加载引用记录
        /// </summary>
        /// <param name="bundleRecordFile">bundleRecord.bytes文件记录脚本关在位置</param>
        private void GenerateSummary(string bundleRecordFile) {
            sw = new StreamWriter(new FileStream(bundleRecordFile, FileMode.Create));

            List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
            sw.WriteLine(tagetObj.name);

            if (tagetObj.GetType() == typeof(GameObject))
            {
                GameObject obj = tagetObj;
                MonoBehaviour[] compoment = obj.GetComponentsInChildren<MonoBehaviour>(true);
                List<MonoBehaviour> customMB = new List<MonoBehaviour>();
                foreach (MonoBehaviour mb in compoment)
                {
                    string ns = mb.GetType().Namespace;

                    if (ns == null || (!ns.Contains("UnityEngine") && !ns.Contains("UnityEditor")))
                    {
                        //UnityEngine.Debug.Log(mb.name + " costom namespace " + ns);
                        customMB.Add(mb);
                    }
                }

                compoment = customMB.ToArray();
                UnityEngine.Debug.Log("处理的组件个数：" + compoment.Length);
                sw.WriteLine(compoment.Length);
                if (compoment != null && compoment.Length > 0)
                {
                    for (int j = 0; j < compoment.Length; j++)
                    {
                        if (compoment[j] == null) continue;
                        UnityEngine.Debug.Log("被处理过的组件：" + compoment[j].GetType().Name);
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
        /// <summary>
        /// 查找批处理程序路径
        /// </summary>
        /// <param name="rootPath">安装根目录</param>
        /// <returns></returns>
        static string FindMcsPath(string rootPath) {
            string mcsPath = rootPath + "/Editor/Data/MonoBleedingEdge/bin/mcs.bat";
            FileInfo mscInfo = new FileInfo(mcsPath);
            if (mscInfo.Exists)
            {
                return mcsPath;
            }else
            {
                UnityEngine.Debug.LogWarning("检查" + mcsPath +"路径是否存在文件msc.bat");
            }
            return "";
            //ArrayList allfiles = GetAllFiles(rootPath);
            //foreach (FileInfo f in allfiles)
            //{
            //    if (f.FullName.EndsWith("mcs.bat"))
            //        return f.FullName;
            //}
            //return "";
        }
        private ArrayList GetBundleScripts() {
            ArrayList fileList = new ArrayList();
            BundleEventTriggerDesigner[] bundleEventTriggers = tagetObj.GetComponentsInChildren<BundleEventTriggerDesigner>();
            if (bundleEventTriggers != null && bundleEventTriggers.Length > 0)
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
        /// <summary>
        /// 按照给定string目录返回查找的文件，仅支持单层文件夹
        /// </summary>
        /// <param name="path">脚本文件夹路径</param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
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
    }
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
}