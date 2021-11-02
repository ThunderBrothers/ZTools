using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using static BundleBuildWindowEditor;
using System.Linq;
using UnityEngine.Assertions.Must;
using System;

[CustomEditor(typeof(CoursewareMakerManager))]
public class CoursewareMakerManagerInspector : Editor
{
    /// <summary>
    /// StepConfig的位置在整体配置表得序号
    /// 格式stage = i，subdivide = j，setp = k
    /// "#" + i + "@" + j + "@" + k
    /// </summary>
    public List<string> allSetpTargetIndexCode;
    //单个打包
    public int currentSelectStepIndex;
    public List<StepConfig> allSetpTargetPrefabs = new List<StepConfig>();
    private string[] allSetpTargetPrefabsName;
    //多个打包
    private int stage;
    public List<BuildStageData> allStage = new List<BuildStageData>();
    private string[] allStageName;
    public Dictionary<string, StepConfig> selectedStageAllSetpCache = new Dictionary<string, StepConfig>();


    private CoursewareMakerManager coursewareMakerManager;

    public CoursewareBuildConfig coursewareBuildConfig;
    private string currentOutputPath;

    private void OnEnable()
    {
        coursewareMakerManager = (CoursewareMakerManager)target;
        RefreshData();
        ReadBuildConfig();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);
        GUILayout.Label("选择打包的物体名称");
      
        //单步骤打包
        if (allSetpTargetPrefabsName != null && allSetpTargetPrefabsName.Length > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("当前选择物体名称:   " + allSetpTargetPrefabsName[currentSelectStepIndex]);
            GUILayout.Space(10);
            currentSelectStepIndex = GUILayout.Toolbar(currentSelectStepIndex, allSetpTargetPrefabsName);
        }

        GUILayout.Space(10);
        if (GUI.changed)
        {
            //如果属性改变时调用
            RefreshData();
            ReadBuildConfig();
        }

        if (GUILayout.Button("单个打包步骤"))
        {
            if (allSetpTargetPrefabsName != null && allSetpTargetPrefabsName.Length > 0)
            {
                if (allSetpTargetPrefabsName[currentSelectStepIndex] == "Error")
                {
                    UnityEngine.Debug.LogError("打包步骤 =" + currentSelectStepIndex + "打包物体未被设置");
                }
                else
                {
                    UnityEngine.Debug.Log("打包步骤 =" + currentSelectStepIndex + "打包物体名称 =" + allSetpTargetPrefabsName[currentSelectStepIndex]);
                    CreateOrUpdateCongifFile();
                    BundleBuildWindowEditor.BuildWindowGUI();
                    BundleBuildWindowEditor.window.StartUpBuild(BuildFinishCallback);
                    BundleBuildWindowEditor.window.prefabs = allSetpTargetPrefabs[currentSelectStepIndex].target;
                    BundleBuildWindowEditor.window.CoursewareSetBuild(currentOutputPath, allSetpTargetPrefabs[currentSelectStepIndex].target.name + ".assetbundle", allSetpTargetPrefabs[currentSelectStepIndex].setpName, allSetpTargetIndexCode[currentSelectStepIndex], allSetpTargetPrefabs[currentSelectStepIndex].coursewareType);
                }
            }
            else
            {
                UnityEngine.Debug.Log("没有可打包的步骤");
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("批量打包------------------------------");
        GUILayout.Space(10);
        //按照一级菜单打包
        if (allSetpTargetPrefabsName != null && allSetpTargetPrefabsName.Length > 0)
        {
            stage = GUILayout.Toolbar(stage, allStageName);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("一级菜单所有步骤统一打包"))
        {
            if (allStageName != null && allStageName.Length > 0)
            {
                selectedStageAllSetpCache.Clear();
                for (int j = 0; j < coursewareMakerManager.buildCoursewareData.stageDatas[stage].subdivideMenuDatas.Count; j++)
                {
                    for (int k = 0; k < coursewareMakerManager.buildCoursewareData.stageDatas[stage].subdivideMenuDatas[j].stepConfigs.Count; k++)
                    {
                        StepConfig sc = coursewareMakerManager.buildCoursewareData.stageDatas[stage].subdivideMenuDatas[j].stepConfigs[k];
                        try
                        {

                            Debug.Log("At =" + stage + "-" + j + "-" + k + "   name=" + sc.setpName);
                            selectedStageAllSetpCache.Add("#" + stage + "@" + j + "@" + k, sc);
                            //EditorCoroutineRunner.StartEditorCoroutine(BatchBuildUpdate());
                            UnityEngine.Debug.Log("打包步骤 =" + currentSelectStepIndex + "打包物体名称 =" + allSetpTargetPrefabsName[currentSelectStepIndex]);
                            CreateOrUpdateCongifFile();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("At =" + stage + "-" + j + "-" + k + "name=" + sc.setpName + " = = = => " + ex.ToString());
                        }
                    }
                }
                CheckHasCacheBuildTask();
            }
            else
            {
                UnityEngine.Debug.Log("没有可打包的步骤");
            }
        }
    }

    private void CheckHasCacheBuildTask()
    {
        if (selectedStageAllSetpCache != null && selectedStageAllSetpCache.Count > 0)
        {
            KeyValuePair<string, StepConfig> temp;
            temp = selectedStageAllSetpCache.FirstOrDefault();

            UnityEngine.Debug.Log("打包步骤 =" + currentSelectStepIndex + "打包物体名称 =" + allSetpTargetPrefabsName[currentSelectStepIndex]);
            try
            {
                CreateOrUpdateCongifFile();
                BundleBuildWindowEditor.BuildWindowGUI();
                BundleBuildWindowEditor.window.StartUpBuild(BuildFinishCallback);
                BundleBuildWindowEditor.window.prefabs = temp.Value.target;
                BundleBuildWindowEditor.window.CoursewareSetBuild(currentOutputPath, temp.Value.target.name + ".assetbundle", temp.Value.setpName, temp.Key, temp.Value.coursewareType);
                BundleBuildWindowEditor.window.ExecuteBuildCheck();
                //这里还是需要手动点击，要不然必现dll文件没有被打包到工程里，但若打断点就会打包进去，应该还是异步写文件的情况，这里暂时手动点击把
                //BundleBuildWindowEditor.window.ExecuteBuildTask();
            }
            catch (Exception ex)
            {
                Debug.LogError(temp.Key + " = = = => " + ex.ToString());
            } 
        }
    }

    private void RefreshData()
    {
        //UnityEngine.Debug.Log("属性改变时调用");
        //联合使用共同组合成Dictionary
        allSetpTargetIndexCode = new List<string>();
        allSetpTargetPrefabs = new List<StepConfig>();

        allStage = new List<BuildStageData>();
        for (int i = 0; i < coursewareMakerManager.buildCoursewareData.stageDatas.Count; i++)
        {
            allStage.Add(coursewareMakerManager.buildCoursewareData.stageDatas[i]);
            for (int j = 0; j < coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas.Count; j++)
            {
                for (int k = 0; k < coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas[j].stepConfigs.Count; k++)
                {
                    allSetpTargetIndexCode.Add("#" + i + "@" + j + "@" + k);
                    allSetpTargetPrefabs.Add(coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas[j].stepConfigs[k]);
                    if (coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas[j].stepConfigs[k].target != null)
                    {
                        coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas[j].stepConfigs[k].bundleFileName = coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas[j].stepConfigs[k].target.name;
                        coursewareMakerManager.buildCoursewareData.stageDatas[i].subdivideMenuDatas[j].stepConfigs[k].indexCode = "#" + i + "@" + j + "@" + k;
                    }
                }
            }
        }
        //单步骤打包
        allSetpTargetPrefabsName = new string[allSetpTargetPrefabs.Count];
        for (int i = 0; i < allSetpTargetPrefabs.Count; i++)
        {
            if (allSetpTargetPrefabs[i].target != null)
            {
                allSetpTargetPrefabsName[i] = allSetpTargetPrefabs[i].setpName;
            }
            else
            {
                allSetpTargetPrefabsName[i] = "Error";
            }
        }
        //按照一级菜单打包
        allStageName = new string[allStage.Count];
        for (int i = 0; i < allStage.Count; i++)
        {
            allStageName[i] = allStage[i].stageName;
        }
    }

    /// <summary>
    /// 创建
    /// </summary>
    private void CreateOrUpdateCongifFile()
    {
        if (coursewareBuildConfig == null)
        {
            UnityEngine.Debug.Log("ReadConfig");
            coursewareBuildConfig = Resources.Load<CoursewareBuildConfig>($"Output/{coursewareMakerManager.buildCoursewareData.coursewareName}/CoursewareBuildConfig");
            if (coursewareBuildConfig == null)
            {
                //创建数据
                coursewareBuildConfig = ScriptableObject.CreateInstance<CoursewareBuildConfig>();
                coursewareMakerManager.coursewareItemDatas = new Dictionary<string, CoursewareItemData>();
                coursewareBuildConfig.allCoursewareDatas = new List<CoursewareItemData>();
                if (!coursewareBuildConfig)
                {
                    UnityEngine.Debug.LogError("创建配置文件错误");
                    return;
                }
                string path = Application.dataPath + $"/BundleTools/CoursewareMaker/Resources/Output/{coursewareMakerManager.buildCoursewareData.coursewareName}";
                currentOutputPath = path;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = string.Format("Assets/BundleTools/CoursewareMaker/Resources/Output/{0}/{1}.asset", coursewareMakerManager.buildCoursewareData.coursewareName, (typeof(CoursewareBuildConfig).ToString()));
                AssetDatabase.CreateAsset(coursewareBuildConfig, path);
                AssetDatabase.Refresh();
            }
            SetCoursewareDataFile();
        }
        else
        {
            SetCoursewareDataFile();
        }
    }

    private void SetCoursewareDataFile()
    {
        //Inspector面板显示打包资源
        coursewareMakerManager.scriptableObject = coursewareBuildConfig;
        //配置打包资源内容
        coursewareBuildConfig.SetStepTargetPrefabsOriginal(allSetpTargetPrefabs);
    }

    public void BuildFinishCallback(BulidAssetsSetting data)
    {
        CoursewareItemData coursewareItemData = new CoursewareItemData();
        Debug.Log("BulidAssetsSetting =" + data.ToString());
        try
        {
            coursewareItemData.index = data.indexCode;
            coursewareItemData.name = data.prefabs.name;
            coursewareItemData.stepName = data.stepName;
            coursewareItemData.coursewareType = data.coursewareType;
            GameObject obj = FindSetpPrefabByName(data.prefabs.name);
            coursewareItemData.targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(obj);
            string assetsPtah = string.Format("Assets/BundleTools/CoursewareMaker/Resources/Output/{0}/{1}.assetbundle", coursewareMakerManager.buildCoursewareData.coursewareName, data.prefabs.name);
            AssetDatabase.Refresh();
            string assetBundlePath = assetsPtah;
            coursewareItemData.assetBundlePath = assetBundlePath;
            coursewareItemData.assetbundleFile = AssetDatabase.LoadAssetAtPath(assetBundlePath, typeof(UnityEngine.Object));
            if (coursewareMakerManager.coursewareItemDatas.ContainsKey(data.indexCode))
            {
                coursewareMakerManager.coursewareItemDatas[coursewareItemData.index] = coursewareItemData;
            }
            else
            {
                coursewareMakerManager.coursewareItemDatas.Add(coursewareItemData.index, coursewareItemData);
            }
            coursewareBuildConfig.SetCoursewareDatas(coursewareMakerManager.coursewareItemDatas.Values.ToList(), coursewareMakerManager.buildCoursewareData);
        }
        catch (Exception ex)
        {
            Debug.LogError(data.indexCode + " = = = => " + ex.ToString());
        }
       

        //检查是否还有打包任务
        if (selectedStageAllSetpCache != null && selectedStageAllSetpCache.Count > 0)
        {
            if (selectedStageAllSetpCache.ContainsKey(data.indexCode))
            {
                selectedStageAllSetpCache.Remove(data.indexCode);
            }

            CheckHasCacheBuildTask();
        }
    }

    private GameObject FindSetpPrefabByName(string name)
    {
        GameObject obj = null;
        for (int i = 0; i < allSetpTargetPrefabs.Count; i++)
        {
            if (allSetpTargetPrefabs[i].target.name == name)
            {
                obj = allSetpTargetPrefabs[i].target;
                return obj;
            }
        }
        return obj;
    }

    private void ReadBuildConfig()
    {
        if (coursewareMakerManager.scriptableObject != null)
        {
            string path = Application.dataPath + $"/BundleTools/CoursewareMaker/Resources/Output/{coursewareMakerManager.buildCoursewareData.coursewareName}";
            currentOutputPath = path;
            CoursewareBuildConfig coursewareBuildConfig = coursewareMakerManager.scriptableObject as CoursewareBuildConfig;
            coursewareMakerManager.coursewareItemDatas = new Dictionary<string, CoursewareItemData>();
            if (coursewareBuildConfig != null)
            {
                coursewareBuildConfig.SetCoursewareDatas(coursewareMakerManager.coursewareItemDatas.Values.ToList(), coursewareMakerManager.buildCoursewareData);
                if (coursewareBuildConfig.allCoursewareDatas != null)
                {
                    coursewareBuildConfig.allCoursewareDatas.ForEach((item) =>
                    {
                        coursewareMakerManager.coursewareItemDatas.Add(item.index, item);
                    });
                }
            }
        }
        else
        {
            coursewareMakerManager.scriptableObject = Resources.Load<CoursewareBuildConfig>(string.Format("Resources/Output/{0}", coursewareMakerManager.buildCoursewareData.coursewareName));
        }
    }
}
