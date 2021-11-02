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

public class BundleBuildWindowEditor : EditorWindow
{
    [MenuItem("Bundle Tools/Build window")]
    public static void BuildWindowGUI()
    {
        Rect wr = new Rect(0, 0, 700, 400);
        window = (BundleBuildWindowEditor)EditorWindow.GetWindowWithRect(
            typeof(BundleBuildWindowEditor), wr, true, "Build window");
        window.Show();
        window.autoRepaintOnSceneChange = true;
    }

    //[MenuItem("Bundle Tools/Asset")]
    static void CreateConfig()
    {
        ScriptableObject config = ScriptableObject.CreateInstance<ToolConfigData>();
        if (!config)
        {
            UnityEngine.Debug.LogError("创建配置文件错误");
            return;
        }
        string path = Application.dataPath + "/BundleTools/Resources";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path = string.Format("Assets/BundleTools/Resources/{0}.asset", (typeof(ToolConfigData).ToString()));
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.Refresh();
    }

    public static BundleBuildWindowEditor window;
    private string unityInstallPath;
    private string scriptPath;
    private string dllName = "output.bytes";

    private StreamReader sr = null;
    private StreamWriter sw = null;
    private StreamWriter swEventConfig = null;
    private bool init = false;
    private bool changed = false;
    
    public GameObject prefabs;
    private GameObject buildTarget;
    private GameObject lastPrefab;

    private int spaceValue = 15;

    private bool androidBuild = true;
    private bool pcBuild = true;
    private bool hololensBuild = false;
    string savePath;
    private bool canBuildt = false;
    private string tips;
    private int inspectIndex;
    private List<string> createColliderRecords = new List<string>();

    private bool hasMono = false;
    private bool hasBundleTrigger = false;
    //自定义组件判断
    //UI题目课件
    private bool hasBundleCustomComponent = false;
    //实操课件
    private bool hasExecuteExamiationComponent = false;

    private AllTriggerToDesingerJson allTriggerToDesingerConfig = new AllTriggerToDesingerJson();
    private AllCustomExaminationDataJson customExaminationDatas = new AllCustomExaminationDataJson();
    private bool isCreateJson = false;//是否进行本地加载，生成索引json文件
    private BulidAssetsSetting bulidAssetsSetting = null;

    private BuildErrorType errorType = BuildErrorType.None;
    private bool hasError = false;

    //外部调用Bundle工具打包
    private ToolConfigData toolConfigData;
    private Action<BulidAssetsSetting> buildFinishAction;
    private bool externalBuildAndSetConfig = false;

    private void OnDestroy()
    {
        Reset();
        sr?.Close();
        sw?.Close();
        sr = null;
        sw = null;
        swEventConfig?.Close();
    }

    private void OnEnable()
    {
        UnityEngine.Debug.Log("OnEnable");
        ReadConfig();
    }

    void OnGUI()
    {
        if (!init)
        {
            Reset();
        }

        if (GUILayout.Button("重置工具", GUILayout.Width(680)))
        {
            ConfigTool();
        }

        GUILayout.Space(spaceValue);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择unity安装路径", GUILayout.Width(200)))
        {
            unityInstallPath = EditorUtility.OpenFolderPanel("unity安装路径", "", "");
            WriteConfig();
            if (string.IsNullOrEmpty(unityInstallPath))
            {
                errorType = BuildErrorType.UnityInstallPathSelectError;
                hasError = true;
                ReadConfig();               
            }
            changed = true;
        }
        EditorGUILayout.SelectableLabel(unityInstallPath);
        GUILayout.EndHorizontal();


        //保存bundle路径
        if (externalBuildAndSetConfig)
        {
            savePath = bulidAssetsSetting.outputPath;
            GUILayout.Label("保存bundle文件路径 =" + savePath); 
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选择保存bundle文件路径", GUILayout.Width(200)))
            {
                savePath = EditorUtility.SaveFilePanel("选择保存bundle文件路径", "", "Unnamed", "assetbundle");
                WriteConfig();
                if (string.IsNullOrEmpty(savePath))
                {
                    errorType = BuildErrorType.BundleSavePathError;
                    hasError = true;
                    ReadConfig();
                }
                changed = true;
            }
            EditorGUILayout.SelectableLabel(savePath);
            GUILayout.EndHorizontal();
        }
       

        GUILayout.Space(spaceValue);
        prefabs = EditorGUILayout.ObjectField("需要打包的prefab:", prefabs, typeof(GameObject), false) as GameObject;
        if (lastPrefab != prefabs)
        {
            lastPrefab = prefabs;
            Reset();
            hasMono = ToolWidgets.CheckMonoForGameObject(lastPrefab);
        }
        GUILayout.Space(spaceValue);
        //-----1
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        androidBuild = EditorGUILayout.Toggle("assetbundle for android", androidBuild);
        pcBuild = EditorGUILayout.Toggle("assetbundle for pc", pcBuild);
        hololensBuild = EditorGUILayout.Toggle("assetbundle for hololens", hololensBuild);
        GUILayout.Space(10);
        //2019.7.8  去掉生成本地加载测试的配置文件
        //isCreateJson = EditorGUILayout.Toggle("生成本地加载索引json文件", isCreateJson);
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
            ExecuteBuildCheck();
        }
        GUILayout.Space(spaceValue);
        if (canBuildt)
        {
            if (GUILayout.Button("打包", GUILayout.Width(200)))
            {
                canBuildt = false;
                ExecuteBuildTask();
            }
        }
        if (changed)
        {
            WriteConfig();           
            changed = false;
        }
        hasError = false;
    }

    #region 打包处理的流程
    public void ExecuteBuildCheck()
    {
        if (prefabs == null)
        {
            UnityEngine.Debug.LogError("打包物体空");
            tips = "打包物体空\n";
        }
        else if (ToolWidgets.CheckCollider(prefabs) && CheckOtherConfig())
        //else if (CheckOtherConfig())
        {
            hasMono = ToolWidgets.CheckMonoForGameObject(prefabs);
            //检查是否有BundleTrigger组件
            CheckBundleEventTriggerComponent();
            //检查是否有自定义组件组件
            CheckBundleCustomComponent();
            canBuildt = true;
            tips = "准备完成可以打包";

        }
        else if (!ToolWidgets.CheckCollider(prefabs))
        // else if (true)
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
                tips += $"创建{createColliderRecords.Where(x => x == "SMR").ToList().Count}个图片渲染器BoxCollider\n";
                tips += $"创建{createColliderRecords.Where(x => x == "T").ToList().Count}个特效渲染器的BoxCollider\n";
                tips += $"创建{createColliderRecords.Where(x => x == "R").ToList().Count}个其他渲染器的BoxCollider\n";
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

    public void ExecuteBuildTask()
    {
        //复制一份作为打包的目标
        if (CopyPrefabForBuild())
        {
            Building();
            BuildToPackage(bulidAssetsSetting);
            Reset();//读取一次配置文件
            WriteConfig();
        }
        else
        {
            errorType = BuildErrorType.BundleSavePrefab;
            UnityEngine.Debug.LogError("打包出现错误，错误代码 =" + errorType.ToString());
        }
    }

    public void StartUpBuild(Action<BulidAssetsSetting> callback)
    {
        buildFinishAction = callback;
        externalBuildAndSetConfig = true;
    }
    //打包
    private void Building()
    {
        //生成文件名
        string timeTick = ToolWidgets.ConvertDateTimeToInt();
        //在生成记录摘要前配置BundleTrigger记录
        //BundleTrigger记录会标记被操作物体(修改其名称，给其挂载对应执行脚本)
        //配置后在进行脚本配置信息
        if (hasBundleTrigger)
        {
            GenerateBundleTriggerSummary();
        }
        if (hasBundleCustomComponent || hasExecuteExamiationComponent)
        {
            customExaminationDatas = new AllCustomExaminationDataJson();
            customExaminationDatas.customExaminationDatas = new List<CustomExaminationData>();
            //自定义组件
            if (hasBundleCustomComponent)
            {
                GenerateBundleCustomComponentSummary();
            }
            //自定义组件，实操组件
            if (hasExecuteExamiationComponent)
            {
                GenerateBundleCustomExecuteExamiationComponentSummary();
            }
        }
        if (hasMono)
        {
            dllName = $"{toolConfigData.outputDllFileName}{timeTick}.bytes";
            GenerateScript(unityInstallPath);
        }

        if (hasError)
        {
            tips = "打包出现错误，错误代码 =" + errorType.ToString() + "\n";
            UnityEngine.Debug.LogError("打包出现错误，错误代码 =" + errorType.ToString());
            errorType = BuildErrorType.None;
            hasError = false;
            return;
        }
        //打包前处理
        PreBuidleHandler();
        AssetDatabase.Refresh();
        //脚本挂载索引
        GenerateSummary(toolConfigData.bundleRecordFile);
        //生成触发器读取值的json文件
        CreateEventTriggerInfoJsonConfig(toolConfigData.eventTriggerConfigPath);
        //生成自定义组件的json文件
        CreateCustomComponentJsonConfig(toolConfigData.customComponentConfigPath);
        AssetDatabase.Refresh();

        if (buildTarget != null)
        {
            List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
            if (buildTarget.GetType() == typeof(GameObject))
            {
                GameObject obj = buildTarget;
                builds.Add(obj);
            }

            bool goon = EditorUtility.DisplayDialog("提示", "内容配置完成继续打包？", "继续", "取消");
            if (goon)
            {
                UnityEngine.Debug.Log("继续打包");
                AssetDatabase.SaveAssets();
            }
            else
            {
                Reset();//读取一次配置文件
                WriteConfig();//记录一次配置文件，保存这次打包的dll名称
                //记录一次配置文件，保存这次打包的dll名称
                EditorUtility.DisplayDialog("提示", "本次被打包物体取消", "确定");
                UnityEngine.Debug.Log("取消打包");
                return;
            }
            AssetDatabase.Refresh();
            if (hasMono)
            {
                string monoAssetPath = $"{toolConfigData.toolOutputPath}/{dllName}";
                //UnityEngine.Debug.LogError("monoAssetPath=" + monoAssetPath);
                builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath(monoAssetPath, typeof(UnityEngine.TextAsset)));
            }
            if (hasBundleTrigger)
            {
                string triggerAssetPath = $"{toolConfigData.toolOutputPath}/{toolConfigData.outputEventTriggerConfigFileName}";
                //UnityEngine.Debug.LogError("assetPath=" + triggerAssetPath);
                builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath(triggerAssetPath, typeof(UnityEngine.TextAsset)));
            }
            //自定义组件
            if (hasBundleCustomComponent|| hasExecuteExamiationComponent)
            {
                string triggerAssetPath = $"{toolConfigData.toolOutputPath}/{toolConfigData.outputCustomComponentConfigFileName}";
                //UnityEngine.Debug.LogError("assetPath=" + triggerAssetPath);
                builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath(triggerAssetPath, typeof(UnityEngine.TextAsset)));
            }
            //if (hasExecuteExamiationComponent)
            //{
            //    string triggerAssetPath = $"{toolConfigData.toolOutputPath}/{toolConfigData.customComponentConfigPath}";
            //    //UnityEngine.Debug.LogError("assetPath=" + triggerAssetPath);
            //    builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath(triggerAssetPath, typeof(UnityEngine.TextAsset)));
            //}
            
            string recordAssetPath = $"{toolConfigData.toolOutputPath}/{toolConfigData.outputRecoredFileName}";
            //UnityEngine.Debug.LogError("assetPath=" + recordAssetPath);
            builds.Add((UnityEngine.Object)AssetDatabase.LoadAssetAtPath(recordAssetPath, typeof(UnityEngine.TextAsset)));

            //如果时外部调用的打包则使用外部的配置bulidAssetsSetting信息
            if (externalBuildAndSetConfig)
            {
                UnityEngine.Debug.Log("外部调用打包" + externalBuildAndSetConfig.ToString());
                bulidAssetsSetting.builds = builds;
            }
            else
            {
                //string projectName = savePath.Substring(savePath.LastIndexOf("/") + 1, savePath.LastIndexOf(".") - savePath.LastIndexOf("/") - 1);
                string dirPath = savePath.Substring(0, savePath.LastIndexOf("/"));
                //UnityEngine.Debug.Log("dir path " + dirPath + " projectName " + projectName);
                UnityEngine.Debug.Log("调用打包" + externalBuildAndSetConfig.ToString());
                bulidAssetsSetting = new BulidAssetsSetting();
                //配置打包设置 
                bulidAssetsSetting.builds = builds;
                bulidAssetsSetting.outputPath = dirPath;
                bulidAssetsSetting.assetsPtah = dirPath;
                bulidAssetsSetting.fileName = "";
                bulidAssetsSetting.timeTick = timeTick;
                bulidAssetsSetting.gameobjectScale = "";
            }
            //打包物体
            bulidAssetsSetting.prefabs = buildTarget;
        }
    }

    private void BuildToPackage(BulidAssetsSetting assetsSet)
    {
        string gameobjectScale = ToolWidgets.CalculateScale(assetsSet.prefabs);
        assetsSet.gameobjectScale = gameobjectScale;
        if (externalBuildAndSetConfig)
        {
            if (androidBuild)
            {
                
                BuildPipeline.BuildAssetBundle(assetsSet.prefabs, assetsSet.builds.ToArray(), assetsSet.outputPath +"/" + assetsSet.fileName, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);
                //CreateJsonConfig(assetsSet.dirPath, assetsSet.projectName, "android", assetsSet.timeTick, gameobjectScale);
            }
        }
        else
        {
            if (androidBuild)
            {
                BuildPipeline.BuildAssetBundle(assetsSet.prefabs, assetsSet.builds.ToArray(), assetsSet.assetsPtah + "/" + assetsSet.fileName + "_android_" + assetsSet.timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);
                //CreateJsonConfig(assetsSet.dirPath, assetsSet.projectName, "android", assetsSet.timeTick, gameobjectScale);
            }
            //if (pcBuild)
            //{
            //    BuildPipeline.BuildAssetBundle(assetsSet.prefabs, assetsSet.builds.ToArray(), assetsSet.dirPath + "/" + assetsSet.projectName + "_pc_" + assetsSet.timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows64);
            //    //CreateJsonConfig(assetsSet.dirPath, assetsSet.projectName, "pc", assetsSet.timeTick, gameobjectScale);
            //}
            //if (hololensBuild)
            //{
            //    BuildPipeline.BuildAssetBundle(assetsSet.prefabs, assetsSet.builds.ToArray(), assetsSet.dirPath + "/" + assetsSet.projectName + "_hololens_" + assetsSet.timeTick + "-" + gameobjectScale + ".assetbundle", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.WSAPlayer);
            //    //CreateJsonConfig(assetsSet.dirPath, assetsSet.projectName, "hololens", assetsSet.timeTick, gameobjectScale);
            //}

            //自动打开文件目录
            ToolWidgets.OpenFile(assetsSet.assetsPtah);
        }
        //打包完成
        errorType = BuildErrorType.None;
        hasError = false;
        if (window != null)
        {
            window.Close();
        }
        FinishBuild(assetsSet);
    }

    public void CoursewareSetBuild(string outputPath,string filename,string stepName ,string indexCode, CoursewareType coursewareType)
    {
        bulidAssetsSetting = new BulidAssetsSetting();
        //配置打包设置
        bulidAssetsSetting.indexCode = indexCode;
        bulidAssetsSetting.prefabs = buildTarget;
        bulidAssetsSetting.outputPath = outputPath;
        bulidAssetsSetting.fileName = filename;
        bulidAssetsSetting.stepName = stepName;
        bulidAssetsSetting.coursewareType = coursewareType;
        bulidAssetsSetting.timeTick = ToolWidgets.ConvertDateTimeToInt();
    }

    //打包前处理
    private void PreBuidleHandler()
    {
        BundleEventTriggerDesigner[] bundleEventTriggers = buildTarget.transform.GetComponentFormChild<BundleEventTriggerDesigner>().ToArray();
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

    //打包结束
    private void FinishBuild(BulidAssetsSetting assetsSet)
    {
        if (buildFinishAction != null)
        {
            buildFinishAction.Invoke(assetsSet);
        }
        buildFinishAction = null;
        externalBuildAndSetConfig = false;
        UnityEngine.Debug.Log("FinishBuild");
        ReadConfig();
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(buildTarget));
        buildTarget = null;
        prefabs = null;
        bulidAssetsSetting = null;
    }

    //复制一个Prefba文件出来打包，不破坏原有文件
    private bool CopyPrefabForBuild()
    {
        bool saveSucess = false;
        GameObject temp = Instantiate(prefabs);
        temp.name = prefabs.name;
        buildTarget = PrefabUtility.SaveAsPrefabAssetAndConnect(temp, toolConfigData.toolOutputPath + $"/{temp.name}.prefab", InteractionMode.UserAction);
        DestroyImmediate(temp);

        if (buildTarget != null)
        {
            saveSucess = true;
            AssetDatabase.Refresh();
        }
        return saveSucess;
    }

    #endregion

    #region 工具
    //检查其他配置
    private bool CheckOtherConfig()
    {
        bool ready = false;
        if ((androidBuild || pcBuild || hololensBuild) && Directory.Exists(savePath.Substring(0, savePath.LastIndexOf("/"))))
        {
            ready = true;
            return ready;
        }
        return ready;
    }
    //检查BundleEventTrigger组件
    private void CheckBundleEventTriggerComponent()
    {
        BundleEventTriggerDesigner[] bundleEventTriggers = prefabs.transform.GetComponentFormChild<BundleEventTriggerDesigner>().ToArray();
        if (bundleEventTriggers != null && bundleEventTriggers.Length > 0)
        {
            hasBundleTrigger = true;
        }
        else
        {
            hasBundleTrigger = false;
        }
    }

    //检查是否有自定义组件组件
    private void CheckBundleCustomComponent()
    {
        //题目UI组件
        Examination[] bundleCustomComponents = prefabs.transform.GetComponentFormChild<Examination>().ToArray();
        if (bundleCustomComponents != null && bundleCustomComponents.Length > 0)
        {
            hasBundleCustomComponent = true;
        }
        else
        {
            hasBundleCustomComponent = false;
        }
        //实操组件
        ExecuteExamiation[] bundleCustomExecuteExamiationComponents = prefabs.transform.GetComponentFormChild<ExecuteExamiation>().ToArray();
        if (bundleCustomExecuteExamiationComponents != null && bundleCustomExecuteExamiationComponents.Length > 0)
        {
            hasExecuteExamiationComponent = true;
        }
        else
        {
            hasExecuteExamiationComponent = false;
        }
    }

    //创建Collider和记录
    private void CreateColliderWithRender(GameObject root, ref List<string> record)
    {
        Transform target = null;
        target = root.transform;
        EditorUtility.SetDirty(root);
        Renderer[] renderers = target.transform.GetComponentFormChild<Renderer>(true).ToArray();
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
    private string CreateCollider(Transform transform)
    {
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
            //特效
            else if (type == typeof(ParticleSystemRenderer))
            {
                //需要判断大小Render
                if (_self == null)
                {
                    BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                    boxCollider.enabled = false;
                    record = "T";//SpriteRenderer
                }
                return record;
            }
            //其他Render
            else if (type == typeof(Renderer))
            {
                //需要判断大小Render
                if (_self == null)
                {
                    obj.AddComponent<BoxCollider>();
                    record = "R";//SpriteRenderer
                }
                return record;
            }
        }
        return record;
    }
    #endregion

    #region 处理Bundle文件的主逻辑
    private void CreateJsonConfig(string dirPath, string projectName, string platForm, string projectId, string gameobjectScale)
    {
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
        sw = null;
    }

    /// <summary>
    /// 生成prefab物体上所有BundleEventTrigger组件对应的配置信息
    /// 保存到一个json文件一起打入Bundle
    /// </summary>
    private void GenerateBundleTriggerSummary()
    {
        //获取所有BundleEventTrigger组件
        BundleEventTriggerDesigner[] bundleEventTriggers = buildTarget.transform.GetComponentFormChild<BundleEventTriggerDesigner>().ToArray();
        allTriggerToDesingerConfig.allJson = new List<triggerToDesingerJson>();
        //给持续数据赋值以及修改物体标号
        for (int i = 0; i < bundleEventTriggers.Length; i++)
        {
            //标记设计师
            GameObject desingerObj = bundleEventTriggers[i].gameObject;
            desingerObj.name += string.Format("--D[{0}]", i);//Desinger
            EditorUtility.SetDirty(desingerObj);
            triggerToDesingerJson temp = new triggerToDesingerJson();

            //给设计师中所有被操作物体做标记
            if (bundleEventTriggers[i].bundleEventTriggerInfos != null && bundleEventTriggers[i].bundleEventTriggerInfos.Count > 0)
            {
                for (int j = 0; j < bundleEventTriggers[i].bundleEventTriggerInfos.Count; j++)
                {
                    //属于设计师名 + 标号
                    if (bundleEventTriggers[i].bundleEventTriggerInfos[j].target == null)
                    {
                        UnityEngine.Debug.LogError("检查BundleEventTriggerInfo组件，存在无target值的设计师组件");
                    }
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
            //增加触发器
            BundleEventTrigger bet = desingerObj.AddComponent<BundleEventTrigger>();
            //赋值形成可预览的物体
            bet.triggers = bundleEventTriggers[i].bundleEventTriggerInfos;
            //修改完成后记录
            temp.objName = desingerObj.name;
            temp.bundleEventTriggerDesigners = bundleEventTriggers[i].GetJsonInfo();
            //修改后保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allTriggerToDesingerConfig.allJson.Add(temp);
        }
    }

    private void GenerateBundleCustomComponentSummary()
    {
        //获取所有自定义组件
        Examination[] bundleCustomComponent = buildTarget.transform.GetComponentFormChild<Examination>().ToArray();
     
        //给持续数据赋值以及修改物体标号
        //UI类的考题组件
        if (bundleCustomComponent != null)
        {
            for (int i = 0; i < bundleCustomComponent.Length; i++)
            {
                //标记设计师
                GameObject desingerObj = bundleCustomComponent[i].gameObject;
                //这里针对有动画的模型不能修改
                //desingerObj.name += string.Format("--D[{0}]", i);//CustomComponent
                EditorUtility.SetDirty(desingerObj);
                CustomExaminationData temp = new CustomExaminationData();
                //修改完成后记录
                temp = bundleCustomComponent[i].customExaminationData;
                temp.objName = desingerObj.name;
                //修改后保存
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                customExaminationDatas.customExaminationDatas.Add(temp);
            }
        }
    }

    private void GenerateBundleCustomExecuteExamiationComponentSummary()
    {
        //实操类的考题组件
        ExecuteExamiation[] executeExamiations = buildTarget.transform.GetComponentFormChild<ExecuteExamiation>().ToArray();
        customExaminationDatas = new AllCustomExaminationDataJson();
        customExaminationDatas.customExaminationDatas = new List<CustomExaminationData>();
        if (executeExamiations != null)
        {
            for (int i = 0; i < executeExamiations.Length; i++)
            {
                //标记设计师
                GameObject desingerObj = executeExamiations[i].gameObject;
                //这里针对有动画的模型不能修改
                //desingerObj.name += string.Format("--D[{0}]", i);//CustomComponent
                EditorUtility.SetDirty(desingerObj);
                CustomExaminationData temp;
                CustomExaminationData data = executeExamiations[i].GetCustomConfigData();
                //修改完成后记录
                temp = data;
                temp.objName = desingerObj.name;
                temp.score = data.score;
                temp.stem = data.stem;
                //修改后保存
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                customExaminationDatas.customExaminationDatas.Add(temp);
            }
        }
    }

    private void GenerateSummary(string bundleRecordFile)
    {
        sw = new StreamWriter(new FileStream(bundleRecordFile, FileMode.Create));

        List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
        sw.WriteLine(buildTarget.name);

        if (buildTarget.GetType() == typeof(GameObject))
        {
            MonoBehaviour[] compoment = buildTarget.transform.GetComponentsFormChild<MonoBehaviour>(true).ToArray();
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
            //写入dll名称
            sw.WriteLine(dllName);
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
            builds.Add(buildTarget);
        }
        sw.Flush();
        sw.Close();
        sw = null;
    }

    private void CreateEventTriggerInfoJsonConfig(string dirPath)
    {
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

    private void CreateCustomComponentJsonConfig(string dirPath)
    {
        //覆盖生成
        swEventConfig = new StreamWriter(new FileStream(dirPath, FileMode.Create));
        DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(AllCustomExaminationDataJson));
        AllCustomExaminationDataJson acedj = new AllCustomExaminationDataJson();
        acedj = customExaminationDatas;
        MemoryStream ms = new MemoryStream();
        dc.WriteObject(ms, acedj);
        byte[] dataBytes = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(dataBytes, 0, (int)ms.Length);
        swEventConfig.WriteLine(Encoding.UTF8.GetString(dataBytes));
        swEventConfig.Flush();
        swEventConfig.Close();
    }


    private void GenerateScript(string unityInstallPath)
    {
        if (!string.IsNullOrEmpty(unityInstallPath))
        {
            string mcsPath = ToolWidgets.FindMcsPath(unityInstallPath);
            UnityEngine.Debug.Log(mcsPath);
            if (!mcsPath.EndsWith("mcs.bat"))
            {
                UnityEngine.Debug.LogError("mcs not find,check your unity install path!");
                errorType = BuildErrorType.McsNotFind;
                hasError = true;
                return;
            }
            string cmd = "/c echo 'start generate scripte:'&& " + "\"" + mcsPath + "\"";
            ArrayList dllList = new ArrayList();
            dllList.Add(new FileInfo( $"{toolConfigData.toolPath}/{toolConfigData.bundleToolPath}/Plugins/BundleEventInfoBase.dll"));
            if (!string.IsNullOrEmpty(unityInstallPath))
            {
                dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/Managed/UnityEngine.dll"));
                dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll"));
                dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/Managed/UnityEditor.dll"));
                if (hasBundleCustomComponent || hasExecuteExamiationComponent)
                {
                    dllList.Add(new FileInfo(toolConfigData.toolAssetPath + "/Plugins/Newtonsoft.Json.dll"));
                }
            }
            else
            {
                errorType = BuildErrorType.UnityInstallPathError;
                hasError = true;
                return;
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
            if (hasBundleTrigger)
            {
                string basePath = $"{toolConfigData.toolPath}/{toolConfigData.bundleToolPath}/{toolConfigData.bundleInternalScriptsPath}/AllTriggerToDesingerJson.cs";
                scriptPaths.Add(basePath);
                basePath = $"{toolConfigData.toolPath}/{toolConfigData.bundleToolPath}/{toolConfigData.bundleInternalScriptsPath}/BundleEventTrigger.cs";
                scriptPaths.Add(basePath);
                basePath = $"{toolConfigData.toolPath}/{toolConfigData.bundleToolPath}/{toolConfigData.bundleInternalScriptsPath}/BundleEventTriggerDesigner.cs";
                scriptPaths.Add(basePath);
            }

            if (hasBundleCustomComponent)
            {
                string basePath = $"{toolConfigData.toolPath}/{toolConfigData.bundleToolPath}/{toolConfigData.bundleInternalScriptsPath}/AllTriggerToDesingerJson.cs";
                scriptPaths.Add(basePath);
                basePath = $"{toolConfigData.toolPath}/{toolConfigData.CoursewareMakerToolPath}/CustomEvent/Examination/Examination.cs";
                scriptPaths.Add(basePath);
            }
            if (hasExecuteExamiationComponent)
            {
                string basePath = $"{toolConfigData.toolPath}/{toolConfigData.bundleToolPath}/{toolConfigData.bundleInternalScriptsPath}/AllTriggerToDesingerJson.cs";
                scriptPaths.Add(basePath);
                basePath = $"{toolConfigData.toolPath}/{toolConfigData.CoursewareMakerToolPath}/CustomEvent/ExecuteExamiation/ExecuteEventTrigger.cs";
                scriptPaths.Add(basePath);
                basePath = $"{toolConfigData.toolPath}/{toolConfigData.CoursewareMakerToolPath}/CustomEvent/ExecuteExamiation/ExecuteExamiation.cs";
                scriptPaths.Add(basePath);
            }


            if (scriptPaths != null && scriptPaths.Count > 0)
            {
                cmd += " -target:library ";
                foreach (string f in scriptPaths)
                {
                    if (f.EndsWith(".cs"))
                        cmd = cmd + f + " ";
                }
            }
            AssetDatabase.Refresh();
            string byteFileFullName = "\"" + toolConfigData.configPath + "/" + dllName + "\"";
            cmd += " -out:" + byteFileFullName;
            UnityEngine.Debug.Log(cmd);
            Process.Start("cmd", cmd);
        }
    }

    /// <summary>
    /// 获取所有要打包脚本的资源路径
    /// </summary>
    /// <returns></returns>
    /// 
    private ArrayList GetBundleScripts()
    {
        ArrayList fileList = new ArrayList();
        BundleEventTriggerDesigner[] bundleEventTriggers = prefabs.transform.GetComponentFormChild<BundleEventTriggerDesigner>().ToArray();
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
    #endregion

    #region 配置
    private void Reset()
    {
        init = true;
        ReadConfig();      
        tips += "选择Unity安装路径(Editor文件夹的上级目录)\n";
        tips += "选择打包脚本文件夹\n";
        tips += "选择Bundle保存文件夹\n";
        tips += "选择打包Prefab\n";
        tips += "选择打包对应平台\n";
        tips += "点击打包检查\n";
        inspectIndex = 1;
        androidBuild = true;
        canBuildt = false;
        hasMono = false;
        hasBundleTrigger = false;
        allTriggerToDesingerConfig = new AllTriggerToDesingerJson();
        isCreateJson = false;
    }

    public void ReadConfig()
    {
        UnityEngine.Debug.Log("ReadConfig");
        if (toolConfigData == null)
        {
            toolConfigData = Resources.Load<ToolConfigData>("ToolConfigData");
        }
        if (toolConfigData != null)
        {
            scriptPath = toolConfigData.scriptPath;
            unityInstallPath = toolConfigData.unityInstallPath;
            savePath = toolConfigData.savePath;
            dllName = toolConfigData.dllName;
        }
        else
        {
            UnityEngine.Debug.LogError("工具配置文件空");
        }
    }

    private void WriteConfig()
    {
        UnityEngine.Debug.Log("ReadConfig");
        if (toolConfigData == null)
        {
            toolConfigData = Resources.Load<ToolConfigData>("ToolConfigData");
        }
        if (toolConfigData != null)
        {
            //toolConfig.scriptPath = scriptPath;
            toolConfigData.unityInstallPath = unityInstallPath;
            toolConfigData.savePath = savePath;
            toolConfigData.dllName = dllName;
        }
        else
        {
            UnityEngine.Debug.LogError("工具配置文件空");
        }
    }

    private void ConfigTool()
    {
        UnityEngine.Debug.Log("重置工具内部错误");
        unityInstallPath = "/重新选择Unity安装路径（Editor文件夹上一层目录）";
        scriptPath = "/选择自定义打包脚本目录";
        savePath = "/选择保存路径";
        string timeTick = System.DateTime.UtcNow.Ticks.ToString();
        dllName = $"logic{ToolWidgets.ConvertDateTimeToInt()}.bytes";
        string fixDllFileName = "";
        ArrayList allfiles = ToolWidgets.GetAllFiles(Application.dataPath + "/Resources/BundleConfig");
        foreach (FileInfo f in allfiles)
        {
            if (f.Name.StartsWith("output") && !f.FullName.EndsWith(".meta"))
                fixDllFileName = f.Name;
        }
        AssetDatabase.RenameAsset($"Assets/Resources/BundleConfig/{fixDllFileName}", dllName);
        AssetDatabase.Refresh();
        WriteConfig();       
        if (window != null)
            window.Close();
    }
    #endregion

    enum BuildErrorType
    {
        None = 0,
        McsNotFind = 1,
        UnityInstallPathError = 2,
        UnityInstallPathSelectError = 3,
        ScriptPathSelectError = 4,
        BundleSavePathError = 5,
        BundleSavePrefab = 6
    }

    public class BulidAssetsSetting
    {
        /// <summary>
        /// 打包的编码标记
        /// </summary>
        public string indexCode;
        /// <summary>
        /// 打包的物体名称
        /// </summary>
        public GameObject prefabs;
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string stepName;
        /// <summary>
        /// 课程内容类型
        /// </summary>
        public CoursewareType coursewareType;
        /// <summary>
        /// 被打包的所有内容
        /// </summary>
        public List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
        /// <summary>
        /// 输出文件夹目录
        /// </summary>
        public string outputPath;
        /// <summary>
        /// 相对于Assets文件夹的目录
        /// </summary>
        public string assetsPtah;
        /// <summary>
        /// 输出的文件名称
        /// </summary>
        public string fileName;
        /// <summary>
        /// 文件打包时的时间戳
        /// </summary>
        public string timeTick;
        /// <summary>
        /// Prefab包围盒大小
        /// </summary>
        public string gameobjectScale;

        public override string ToString()
        {
            return "prefabs.name =" + prefabs.name + "  outputPath =" + outputPath + "   fileName =" + fileName + "   timeTick =" + timeTick + "  gameobjectScale =" + gameobjectScale;
        }
    }
}

public class Project
{
    public string ProjectName;
    public string ProjectId;
    public string FilePath;
}