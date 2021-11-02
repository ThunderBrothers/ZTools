using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 课程设计界面类
/// </summary>
public class CoursewareMakerWindowEditor : EditorWindow
{
    /// <summary>
    /// 课程设计物体根节点
    /// </summary>
    private static GameObject coursewareRootPrefab;
    private static string prefabPath;
    /// <summary>
    /// 生成的根节点，所有被打包的Bundle物体的父物体
    /// </summary>
    private static GameObject coursewareObj;
    private static ToolConfigData toolConfigData;



    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareMaker")]
    public static void CoursewareMaker()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/CoursewareRootPrefab.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "CoursewareRootGameobject";
        CoursewareMakerManager coursewareMakerManager = coursewareObj.GetComponent<CoursewareMakerManager>();
        coursewareMakerManager.buildCoursewareData.coursewareName = "CoursewareName";
    }


    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/VideoPlayer")]
    public static void CoursewareVideoPlayer()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/VideoPlayer.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New VideoPlayer";
    }

    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/AudioPlayer")]
    public static void CoursewareAudioPlayer()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/AudioPlayer.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New AudioPlayer";
    }

    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/CustomExamination")]
    public static void CoursewareExamination()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/Examination.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New Examination";
    }

    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/Text")]
    public static void CoursewareText()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/Text.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New Text";
    }

    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/HightGameObjectTemplete")]
    public static void CoursewareHightGameObjectTemplete()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/HightGameObjectTemplete.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New HightGameObjectTemplete";
    }

    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/ExecuteExamiation")]
    public static void ExecuteExamiationGameObjectTemplete()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/ExecuteExamiation.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New ExecuteExamiation";
    }

    /// <summary>
    /// 生成被打包物体的根节点
    /// </summary>
    [MenuItem("Bundle Tools/CreateCoursewareComponent/Background")]
    public static void BackgroundGameObjectTemplete()
    {
        toolConfigData = ReadConfig();
        prefabPath = toolConfigData.toolAssetPath + "/CoursewareMaker/Resources/InternalComponentPrefab/Background.prefab";
        coursewareRootPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        coursewareObj = Instantiate(coursewareRootPrefab, null) as GameObject;
        coursewareObj.name = "A New Background";
    }


    public static ToolConfigData ReadConfig()
    {
        ToolConfigData temp = null;
        UnityEngine.Debug.Log("ReadConfig");
        if (temp == null)
        {
            temp = Resources.Load<ToolConfigData>("ToolConfigData");
            if (temp == null)
            {
                UnityEngine.Debug.LogError("ToolConfigData工具配置文件空");
            }
            else
            {
                UnityEngine.Debug.Log("ToolConfigData读取完成");
            }
        }
        else
        {
            UnityEngine.Debug.Log("ToolConfigData读取完成");
        }
        return temp;
    }
}
