using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

namespace ZTools.AssetBundleBuildInDLL
{
    public class BundleBuilder : EditorWindow
    {
        //Unity安装路径
        private static string unityInstallPath;
        private string tempPath = "";
        //Resources中途路径
        private string resourcesPath;
        //Bundle配置路径
        private string bundleConfig;
        //AssetBundle保存路径
        private string outputPath = "";
        //dll保持到的二级制文件
        private string dllConfig;
        //要打包的脚本文件夹
        private string scriptsPath = "";
        //要打包的物体
        private GameObject tagetObj;
        //AB包平台
        private BuildTarget platform = BuildTarget.StandaloneWindows64;

        private StreamReader sr = null;
        private StreamWriter sw = null;

        //生成记录
        private string debug;


        [MenuItem("ZTools/BundleBuildInDLL")]
        static void BundleBuildWindow()
        {
            GetWindow(typeof(BundleBuilder));
            FindInstallPath();
        }
        private void OnEnable()
        {
            FindInstallPath();
            bundleConfig = Application.dataPath + "ZTools/AssetBundleBuildInDLL/Resources" + "/bundleRecord.bytes";
            dllConfig = Application.dataPath + "ZTools/AssetBundleBuildInDLL/Resources" + "/output.bytes";
            debug = "打包记录    ";
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
            GUILayout.Label("Unity安装目录");
            EditorGUILayout.SelectableLabel(unityInstallPath);
            CreateButtonAndSelectPath<string>("设置Unity安装目录", "Unity安装路径", "/unity", "请重新选择路径", ref unityInstallPath);
            GUILayout.EndHorizontal();
            //脚本目录
            GUILayout.BeginHorizontal();
            GUILayout.Label("脚本目录");
            EditorGUILayout.SelectableLabel(scriptsPath);
            CreateButtonAndSelectPath<string>("选择脚本路径", "选择要打包的脚本文件夹", "/script", "请重新选择路径", ref scriptsPath);
            GUILayout.EndHorizontal();
            //物体
            tagetObj = EditorGUILayout.ObjectField("打包物体", tagetObj, typeof(GameObject)) as GameObject;
            //打包平台
            platform = (BuildTarget)EditorGUILayout.EnumPopup("打包平台", platform);
            //保存路径
            GUILayout.BeginHorizontal();
            GUILayout.Label("AssetBundle保存目录");
            EditorGUILayout.SelectableLabel(outputPath);
            CreateButtonAndSelectPath<string>("设置AssetBundle保存目录", "AssetBundle保存目录", "", "请重新选择路径", ref outputPath);
            GUILayout.EndHorizontal();
            //打包
            if (tagetObj != null &&!string.IsNullOrEmpty(dllConfig) && !string.IsNullOrEmpty(scriptsPath) && !string.IsNullOrEmpty(unityInstallPath))
            {
                if (GUILayout.Button("打包"))
                {
                    GenerateScript(dllConfig, scriptsPath, unityInstallPath);
                    GenerateSummary(bundleConfig);
                }
                EditorGUILayout.SelectableLabel(debug);
            }
        }

        private void Reset()
        {
            FindInstallPath();
            bundleConfig = Application.dataPath + "ZTools/AssetBundleBuildInDLL/Resources" + "/bundleRecord.bytes";
            dllConfig = Application.dataPath + "ZTools/AssetBundleBuildInDLL/Resources" + "/output.bytes";
            scriptsPath = "";
            outputPath = "";
            tempPath = "";
            tagetObj = null;
            platform = BuildTarget.StandaloneWindows64;
            debug = "打包记录    ";
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
            unityInstallPath = key.GetValue("Location x64").ToString();
            if (string.IsNullOrEmpty(unityInstallPath))
            {
                UnityEngine.Debug.LogWarning("请检查Unity安装路径或者手动设置");
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
        private void CreateButtonAndSelectPath<T>(string buttonTitle,string folderTitle,string checker,string warning,ref T value)
        {
            if (GUILayout.Button(buttonTitle))
            {
                tempPath = EditorUtility.OpenFolderPanel(folderTitle, "", "");
                //路径检查小写checker
                if (!string.IsNullOrEmpty(tempPath) && tempPath.ToLower().Contains(checker))
                {
                    value = (T)Convert.ChangeType(tempPath,typeof(T));
                }
                else
                {
                    UnityEngine.Debug.LogWarning(warning);
                }
            }
        }
        /// <summary>
        /// 处理脚本生成dll
        /// </summary>
        /// <param name="dllPath">批处理程序生的保存dll的二级制文件目录</param>
        /// <param name="scriptPath">打包的脚本文件夹</param>
        /// <param name="unityInstallPath">安装目录索引msc.bat</param>
        private void GenerateScript(string dllPath, string scriptPath, string unityInstallPath) {
            string mcsPath = FindMcsPath(unityInstallPath);
            if (!mcsPath.EndsWith("mcs.bat"))
            {
                UnityEngine.Debug.LogError("检查" + mcsPath + "路径是否存在文件msc.bat");
                return;
            }
            //构建命令
            string cmd = "/c echo 'start generate scripte:'&& " + "\"" + mcsPath + "\"";
            #region 打包脚本引用到的dll到AssetBundle包内 暂时不用
            //ArrayList dllList = GetAllFiles(Application.dataPath + "/Plugins", ".dll");
            //if (!string.IsNullOrEmpty(unityInstallPath))
            //{
            //    dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/Managed/UnityEngine.dll"));
            //    dllList.Add(new FileInfo(unityInstallPath + "/Editor/Data/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll"));
            //}
            //UnityEngine.Debug.Log("dll count: " + dllList.Count);
            //if (dllList != null && dllList.Count > 0)
            //{
            //    foreach (FileInfo f in dllList)
            //    {
            //        UnityEngine.Debug.Log(f.FullName);
            //        if (f.FullName.Contains(".dll"))
            //        {
            //            if (!tag)
            //            {
            //                //tag = true;
            //                cmd += " -r:";
            //            }
            //            cmd = cmd + "\"" + f.FullName + "\" ";
            //        }
            //    }
            //}
            #endregion
            //处理脚本合成一个dll  Process.Start会生成一个文件output.bytes供以后使用
            ArrayList scriptList = GetAllFiles(scriptPath, ".cs");
            debug += " 打包脚本" + scriptList.Count + "个";
            if (scriptList != null && scriptList.Count > 0)
            {
                cmd += " -target:library ";
                foreach (FileInfo f in scriptList)
                {
                    if (f.FullName.EndsWith(".cs"))
                    {
                        cmd = cmd + f.FullName + " ";
                        debug += f.Name + "   ";
                    }
                        
                }
            }
            string byteFileFullName = "\"" + Application.dataPath + "/Resources/BundleConfig" + "/" + "output.bytes" + "\"";
            cmd += " -out:" + byteFileFullName;
            //UnityEngine.Debug.Log(cmd);
            //Process.Start("cmd", cmd);
            
        }
        /// <summary>
        /// 生成加载引用记录
        /// </summary>
        /// <param name="bundleRecordFile">bundleRecord.bytes文件记录脚本关在位置</param>
        private void GenerateSummary(string bundleRecordFile) {
            sw = new StreamWriter(new FileStream(bundleRecordFile, FileMode.OpenOrCreate));

            List<UnityEngine.Object> builds = new List<UnityEngine.Object>();
            List<string> interfaceList = new List<string>();
            sw.WriteLine(tagetObj.name);

            if (tagetObj.GetType() == typeof(GameObject))
            {
                GameObject obj = (GameObject)tagetObj;
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

            foreach (string s in interfaceList)
                sw.WriteLine(s);
            interfaceList.Clear();
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
    }
    public class Project {
        public string ProjectName;
        public string ProjectId;
        public string FilePath;
    }
}