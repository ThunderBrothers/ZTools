using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Threading;

[CustomEditor(typeof(CoursewareBuildConfig))]
public class CoursewareBuildConfigInspector : Editor
{
    private CoursewareBuildConfig coursewareBuildConfig;
    private ToolConfigData toolConfigData;
    private bool finish = false;

    private void OnEnable()
    {
        if(toolConfigData == null)
        {
            toolConfigData = ToolWidgets.configData;
        }
        coursewareBuildConfig = target as CoursewareBuildConfig;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);
        if (GUILayout.Button("导出配置文件"))
        {
            UnityEngine.Debug.Log("导出配置文件");
            CreateBuildDataAndFile();
        }
        if (GUILayout.Button("导出课程"))
        {
            UnityEngine.Debug.Log("导出所有Bundle");
            BuidlZipFile();
        }
        if (finish)
        {
            EditorUtility.DisplayDialog("提示", "Zip压缩完成", "确定");
            finish = false;
        } 
    }

    /// <summary>
    /// 根据索引生成一个Zip包
    /// </summary>
    private void BuidlZipFile()
    {
        CreateBuildDataAndFile();
        BuildResources();
    }

    /// <summary>
    /// 生成配置文件
    /// </summary>
    private void CreateBuildDataAndFile()
    {
        BuildCoursewareData coursewareData = new BuildCoursewareData();
        //课程名称与类型
        //步骤详细配置
        coursewareData = coursewareBuildConfig.coursewareData;
        string buildDataStr = JsonConvert.SerializeObject(coursewareData);
        string path = string.Format("{0}/{1}/Resources/Output/{2}/CoursewareBuildData.json",toolConfigData.toolPath, toolConfigData.CoursewareMakerToolPath, coursewareBuildConfig.coursewareData.coursewareName);
        Debug.Log(path);
        ToolWidgets.WriteFile(path, buildDataStr);
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// 打包资源
    /// </summary>
    private void BuildResources()
    {
        finish = false;
        Debug.Log("BuildResources");
        string startPath = string.Format("{0}/{1}/Resources/Output/{2}", toolConfigData.toolPath, toolConfigData.CoursewareMakerToolPath, coursewareBuildConfig.coursewareData.coursewareName);
        string zipPath = string.Format("{0}/{1}/Resources/Output/{2}.zip", toolConfigData.toolPath, toolConfigData.CoursewareMakerToolPath, coursewareBuildConfig.coursewareData.coursewareName);

        //先不使用多线程，下面的直接卡住主线程 ，禁用其他操作
        //Thread thread = new Thread(()=> {
        //    ZipFile.CreateFromDirectory(startPath, zipPath);
        //    Debug.Log("BuildResources finish =" + startPath);
        //    finish = true;
        //});
        //thread.Start();

        ZipFile.CreateFromDirectory(startPath, zipPath);
        Debug.Log("BuildResources finish =" + startPath);
        finish = true;
    }
}
