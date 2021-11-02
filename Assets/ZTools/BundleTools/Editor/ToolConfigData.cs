using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 工具配置数据资源，保存插件配置数据
/// </summary>
public class ToolConfigData : ScriptableObject
{
    /// <summary>
    /// 工具相对Asset的目录,相对于Asset
    /// </summary>
    public string toolAssetPath;
    /// <summary>
    /// 制作bundle文件夹
    /// </summary>
    public string bundleToolPath;
    /// <summary>
    /// 制作课程文件夹
    /// </summary>
    public string CoursewareMakerToolPath;

    /// <summary>
    /// 工具输出文件目录,相对于Asset
    /// </summary>
    public string toolOutputPath;
    /// <summary>
    /// 工具完整目录，完整目录
    /// </summary>
    public string toolPath;

    //[HideInInspector]
    public string unityInstallPath;
    //[HideInInspector]
    public string scriptPath;

    public string savePath;

    public string dllName;
   
    public string configPath;
    public string bundleRecordFile;
    public string eventTriggerConfigPath;
    public string customComponentConfigPath;
    public string bundleInternalScriptsPath;

    public string outputRecoredFileName;
    public string outputEventTriggerConfigFileName;
    public string outputCustomComponentConfigFileName;
    public string outputDllFileName;



    [Header("自定义的脚本目录")]
    public DefaultAsset scriptPathPathQuote;

    private void OnEnable()
    {
        toolAssetPath = "Assets/BundleTools";
        toolOutputPath = "Assets/BundleTools/BundleBuilder/Resources/BundleOutput";
        toolPath = Application.dataPath + "/BundleTools";
        configPath = toolPath + "/BundleBuilder/Resources/BundleOutput";
        bundleRecordFile = toolPath + "/BundleBuilder/Resources/BundleOutput/bundleRecord.bytes";
        eventTriggerConfigPath = toolPath + "/BundleBuilder/Resources/BundleOutput/eventTriggerConfig.json";
        customComponentConfigPath = toolPath + "/BundleBuilder/Resources/BundleOutput/customComponent.json";
        bundleInternalScriptsPath = "/BundleScript/BundleInternalScripts";
        bundleToolPath = "BundleBuilder";
        CoursewareMakerToolPath = "CoursewareMaker";


        outputRecoredFileName = "bundleRecord.bytes";
        outputEventTriggerConfigFileName = "eventTriggerConfig.json";
        outputCustomComponentConfigFileName = "customComponent.json";
        outputDllFileName = "logic";
    }  
}       

[CustomEditor(typeof(ToolConfigData))]
public class BundleConfigInspector : Editor
{
    public ToolConfigData _target;

    private void OnEnable()
    {
        _target = (ToolConfigData)target;
        _target.scriptPathPathQuote = AssetDatabase.LoadAssetAtPath(_target.scriptPath, typeof(DefaultAsset)) as DefaultAsset;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SetData();
    }

    public void SetData()
    {
        _target.scriptPath = AssetDatabase.GetAssetPath(_target.scriptPathPathQuote);
    }
}
