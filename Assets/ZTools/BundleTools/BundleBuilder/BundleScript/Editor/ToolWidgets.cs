using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class ToolWidgets
{
    public static ToolConfigData configData => ReadConfig();

    public static ToolConfigData ReadConfig()
    {
        ToolConfigData config;
        config = Resources.Load<ToolConfigData>("ToolConfigData");
        UnityEngine.Debug.Log("ReadConfig");
        if (config == null)
        {
            CreateConfig();
            config = Resources.Load<ToolConfigData>("ToolConfigData");
            if (config == null)
            {
                UnityEngine.Debug.LogError("工具配置文件空");
            }
        }
        else
        {
            UnityEngine.Debug.Log("读取工具配置文件");
        }
        return config;
    }

    private static void CreateConfig()
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

    public static string ConvertDateTimeToInt()
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        long t = (System.DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
        return t.ToString();
    }

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    public static bool GetOFN([In, Out] OpenFileName ofn)
    {
        return GetOpenFileName(ofn);
    }

    //检查Collider
    public static bool CheckCollider(GameObject obj)
    {
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
    public static bool CheckMonoForGameObject(GameObject target)
    {
        bool has = false;
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
                has = true;
            }
            else
            {
                has = false;
            }
        }
        return has;
    }

    //计算bundle包围盒大小
    public static string CalculateScale(GameObject obj)
    {
        string scaleStr = "0_0_0";
        if (obj == null)
        {
            return scaleStr;
        }
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

    static string FloatHandle(float x)
    {
        return (Mathf.Round(x * 100f) / 100f).ToString();
    }

    public static ArrayList GetAllFiles(string path, string fileExtension = "")
    {
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

    public static ArrayList GetAllFiles(DirectoryInfo info, string fileExtension = "")
    {
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

    public static void OpenFile(string path)
    {
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
        ofn.title = "Open Project";
        ofn.defExt = "JPG";//显示文件的类型  注意 一下项目不一定要全选 但是0x00000008项不要缺少
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        System.Diagnostics.Process.Start(ofn.initialDir);
    }

    public static string FindMcsPath(string rootPath)
    {
        string mcsPath = rootPath + "/Editor/Data/MonoBleedingEdge/bin/mcs.bat";
        FileInfo mcs = new FileInfo(mcsPath);
        if (mcs.Exists)
        {
            return mcsPath;
        }
        return "";
    }

    /// <summary>
    /// 通过注册表获取安装路径
    /// </summary>
    public static string FindInstallPath()
    {
        string str = null;
        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Unity Technologies\Installer\Unity");
        str = key.GetValue("Location x64").ToString();
        if (string.IsNullOrEmpty(str))
        {
            UnityEngine.Debug.LogWarning("请检查Unity安装路径或者手动设置");
        }
        return str;
    }

    public static void WriteFile(string path,string str)
    {
        using (StreamWriter streamWriter = new StreamWriter(path))
        {
            streamWriter.Write(str);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
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
