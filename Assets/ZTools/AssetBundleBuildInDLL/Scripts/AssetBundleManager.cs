using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class AssetBundleManager : MonoBehaviour {
    private List<Shader> shaderList = new List<Shader>();
    private List<Material> materials = new List<Material>();
    private List<GameObject> assetLoaded = new List<GameObject>();
    public static AssetBundleManager Instance = null;
    public System.Reflection.Assembly dll = null;
    /// <summary>
    /// 缓存所有(BundleEventTriggerInfo)单个触发事件信息
    /// </summary>
    Dictionary<string, List<BundleEventTriggerInfo>> triggerInfoCache = new Dictionary<string, List<BundleEventTriggerInfo>>();
    /// <summary>
    /// 读取json信息
    /// </summary>
    AllTriggerToDesingerJson triggerDesingerJsonInfos;

    public string path;
    public string name;

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        StartCoroutine(LaodBundle());
       
    }

    IEnumerator LaodBundle() {
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path + "\\" + name);
        yield return request.SendWebRequest();
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        LoadAssetBundle(bundle, transform);
        yield return null;
        LoadJsonForTrigger(bundle);
        yield return null;
        HandleJsonForTrigger(bundle);
        yield return null;
        bundle.Unload(false);
    }



    public void UnloadAsset() {
        foreach (GameObject g in assetLoaded)
        {
            if (g != null) DestroyImmediate(g);
        }
        shaderList.Clear();
        materials.Clear();
        assetLoaded.Clear();
        triggerInfoCache.Clear();
        dll = null;
    }

    public void LoadAssetBundle(AssetBundle content, Transform container) {
        if (content == null) return;
        string[] names = content.GetAllAssetNames();
        foreach (string n in names)
        {
            if (n.Contains(".shader"))
            {
                Shader s = content.LoadAsset<Shader>(n);
                shaderList.Add(s);
            }
        }
        Shader.WarmupAllShaders();
        ResetAllMaterial(content);
        TextAsset config = content.LoadAsset<TextAsset>("bundleRecord");
        List<string> gameObjectNames = new List<string>();
        List<string> scriptNames = new List<string>();
        string contentName = "";
        if (config != null)
        {
            byte[] c = config.bytes;
            MemoryStream ms = new MemoryStream(c);
            StreamReader sr = new StreamReader(ms);

            contentName = sr.ReadLine();// +".prefab";
            string count = sr.ReadLine();
            string scriptName = sr.ReadLine();
            string gameObjectName = "";
            while (!string.IsNullOrEmpty(scriptName))
            {
                scriptNames.Add(scriptName);
                gameObjectName = sr.ReadLine();
                if (gameObjectName != null)
                    gameObjectNames.Add(gameObjectName);
                scriptName = sr.ReadLine();
            }

            TextAsset asset = content.LoadAsset<TextAsset>("output");
            if (asset != null)
            {
                dll = System.Reflection.Assembly.Load(asset.bytes);
                /**
                Debug.Log("load dll success!");
                System.Type[] tps = dll.GetTypes();
                if (tps != null && tps.Length > 0)
                {
                    foreach (System.Type tp in tps)
                    {
                        Debug.Log("found in dll " + tp.FullName);
                    }
                }
                **/
            }
            sr.Close();
            ms.Close();

            GameObject obj = (GameObject)GameObject.Instantiate((GameObject)content.LoadAsset(contentName));
            assetLoaded.Add(obj);
            obj.transform.parent = container;
            obj.transform.position = container.position;
            obj.transform.localScale = Vector3.one;

            obj.name = contentName;
            if (dll != null)
            {
                for (int i = 0; i < gameObjectNames.Count; i++)
                {
                    System.Type t = dll.GetType(scriptNames[i]);
                    Transform trans = obj.transform.parent.Find(gameObjectNames[i]);
                    Component res = null;
                    if (trans != null && t != null)
                    {
                        res = trans.gameObject.AddComponent(t);
                    }
                }
            }
            ResetGameObjectShader(obj.transform);
        }
    }
  
    public void LoadJsonForTrigger(AssetBundle content) {
        TextAsset aa = content.LoadAsset<TextAsset>("eventTriggerConfig");
        triggerDesingerJsonInfos = JsonConvert.DeserializeObject<AllTriggerToDesingerJson>(aa.text);
        TextAsset asset = content.LoadAsset<TextAsset>("output");
        dll = System.Reflection.Assembly.Load(asset.bytes);
        for (int i = 0; i < triggerDesingerJsonInfos.allJson.Count; i++)
        {
            string triggerName = triggerDesingerJsonInfos.allJson[i].objName;
            List<BundleEventTriggerInfo> bundleEventTriggerInfos = new List<BundleEventTriggerInfo>();
            GameObject target;
            UnityEngine.Object method;
            BundleEventTriggerType triggerType;
            List<BundleEventTriggerJson> bundleEventTriggerJsons = triggerDesingerJsonInfos.allJson[i].bundleEventTriggerDesigners.bundleEventTriggerJsons;
            for (int j = 0; j < bundleEventTriggerJsons.Count; j++)
            {
                target = GameObject.Find(bundleEventTriggerJsons[j].target).gameObject;
                Type componentType = dll.GetType(bundleEventTriggerJsons[j].method);
                Component mb = target.GetComponent(componentType);
                method = mb;
                triggerType = (BundleEventTriggerType)bundleEventTriggerJsons[j].triggerType;
                bundleEventTriggerInfos.Add(new BundleEventTriggerInfo(target, method, triggerType));
            }
            triggerInfoCache.Add(triggerName, bundleEventTriggerInfos);
        }
    }
    public List<BundleEventTriggerInfo> GetJsonForTrigger(string triggerName) {
        List<BundleEventTriggerInfo> selfBundleEventTriggerInfo = new List<BundleEventTriggerInfo>();
        if (triggerInfoCache.ContainsKey(triggerName))
        {
            selfBundleEventTriggerInfo = triggerInfoCache[triggerName];
        }
        return selfBundleEventTriggerInfo;
    }

    /// <summary>
    /// 将json数据处理赋值给BundleEventTrigger组件
    /// </summary>
    /// <param name="content">bundle</param>
    public void HandleJsonForTrigger(AssetBundle content) {
        TextAsset asset = content.LoadAsset<TextAsset>("output");
        dll = System.Reflection.Assembly.Load(asset.bytes);

        for (int i = 0; i < triggerInfoCache.Count; i++)
        {
            //获取BundleEventTrigger组件物体
            GameObject triggerObj = GameObject.Find(triggerDesingerJsonInfos.allJson[i].objName);
            //获取物体上所有组件信息
            Component[] components = triggerObj.GetComponents<Component>();
            for (int j = 0; j < components.Length; j++)
            {
                Type typecomponents = components[j].GetType();
                //遍历所有组件如果是BundleEventTrigger就赋值
                if (typecomponents == dll.GetType("BundleEventTrigger"))
                {
                    TriggerAssignment(components[j], triggerDesingerJsonInfos.allJson[i].objName);
                }
            }
        }
    }

    /// <summary>
    /// 针对BundleEventTrigger组件
    /// 反射去调用针对BundleEventTrigger中某的初始化函数AddTriggerByElement
    /// 对selfBundleEventTriggerInfo的List增加元素
    /// </summary>
    /// <param name="t"></param>
    /// <param name="value"></param>
    private void TriggerAssignment(Component t, string value) {
        //获取所有公共属性
        PropertyInfo[] infos = t.GetType().GetProperties();
        //获取所有公共值
        //FieldInfo[] infos = t.GetType().GetFields();
        foreach (PropertyInfo info in infos)
        {
            if (info != null)
            {
                //判断熟悉是否时一样的
                //整理当遍历到List时进入
                if (info.PropertyType.IsGenericType)
                {
                    Type type = t.GetType();
                    //反射获取重载的函数
                    //标记   获取BundleEventTriggerType参数的函数会获取不到，可能是不允许有自定义类型吧
                    //MethodInfo method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(BundleEventTriggerType) });
                    MethodInfo method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int) });
                    for (int i = 0; i < triggerInfoCache[value].Count; i++)
                    {
                        object[] parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType };
                        method.Invoke(t, parameters);
                    }
                    object subObj = info.GetValue(t);
                    #region 尝试用反射调用List.Add函数，运行时貌似不行
                    ////扩容List
                    //object subObj = info.GetValue(t);
                    //for (int i = 0; i < triggerInfoCache[value].Count; i++)
                    //{
                    //    MethodInfo m = info.PropertyType.GetMethod("Add");
                    //    m.Invoke(subObj, new object[] { null });
                    //    //调用List的Add方法回报错 说类型转换不正确
                    //    //MethodInfo m = type.GetMethod("Add");
                    //    //BundleEventTriggerInfo iii = triggerInfoCache[value][i];
                    //    //m.Invoke(subObj, new object[] { iii });
                    //}
                    #endregion
                }
            }
        }
    }

    private void ResetAllMaterial(AssetBundle content) {
        object[] ms = content.LoadAllAssets(typeof(Material));
        //if(ms != null) Debug.Log("material count " + ms.Length);
        foreach (object o in ms)
        {
            Material m = (Material)o;
            materials.Add(m);
            var shaderName = m.shader.name;

            var newShader = Shader.Find(shaderName);
            if (newShader != null)
            {
                m.shader = newShader;
                // Debug.Log("refresh shader success for " + shaderName+ " in material " + m.name);
            }
            else
            {
                bool find = false;
                foreach (Shader sh in shaderList)
                {
                    if (sh.name.Equals(shaderName))
                    {
                        find = true;
                        m.shader = sh;
                        // Debug.Log("refresh shader success for " + shaderName + " in material " + m.name);
                        break;
                    }
                }
                // if(!find)Debug.Log("unable to refresh shader: " + shaderName + " in material " + m.name);
            }
        }

    }

    private IEnumerator SetMaterial(GameObject target, Material[] ms, bool activeSelf) {
        yield return 1;
        target.GetComponent<MeshRenderer>().sharedMaterials = ms;
        target.GetComponent<MeshRenderer>().sharedMaterial = ms[0];
        target.GetComponent<MeshRenderer>().materials = ms;
        target.GetComponent<MeshRenderer>().material = ms[0];
        target.SetActive(activeSelf);
    }

    private void ResetGameObjectShader(Transform target) {
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            int mcount = meshRenderer.sharedMaterials.Length;
            Material[] newMaterials = new Material[mcount];
            for (int j = 0; j < mcount; j++)
            {
                Material _mater = meshRenderer.sharedMaterials[j];
                // Debug.Log("check material " + _mater.name);
                bool resetMaterial = false;
                if (_mater != null)
                {
                    foreach (Material m in materials)
                    {
                        if (m.name.Equals(_mater.name))
                        {
                            // Debug.Log("reset material " + m.name);
                            Material nm = new Material(m);
                            newMaterials[j] = nm;
                            resetMaterial = true;
                            continue;
                        }
                    }
                }
                if (resetMaterial) continue;
                newMaterials[j] = _mater;
                if (_mater == null || _mater.shader == null) continue;
                Shader s = Shader.Find(_mater.shader.name);
                if (s != null) newMaterials[j].shader = s;
                else
                {
                    foreach (Shader sh in shaderList)
                    {
                        if (sh == null) continue;
                        if (sh.name.Equals(_mater.shader.name))
                        {
                            newMaterials[j].shader = sh;
                            continue;
                        }
                    }
                }
            }


            StartCoroutine(SetMaterial(target.gameObject, newMaterials, target.gameObject.activeSelf));
            target.gameObject.SetActive(false);
        }


        for (int i = 0; i < target.childCount; i++)
        {
            ResetGameObjectShader(target.GetChild(i));
        }
    }
}


