using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using Newtonsoft.Json;

namespace stARkit.Cloud.ARPackageSDK
{
    public class AssetBundleManager : MonoBehaviour
    {
        private List<Shader> shaderList = new List<Shader>();
        private List<Material> materials = new List<Material>();
        private List<GameObject> assetLoaded = new List<GameObject>();
        public static AssetBundleManager Instance = null;
        public System.Reflection.Assembly dll = null;
        private string dllName;
        private GameObject mainObj;
        /// <summary>
        /// 缓存所有(BundleEventTriggerInfo)单个触发事件信息
        /// </summary>
        Dictionary<string, List<BundleEventTriggerInfo>> triggerInfoCache = new Dictionary<string, List<BundleEventTriggerInfo>>();
        /// <summary>
        /// 读取json信息
        /// </summary>
        AllTriggerToDesingerJson triggerDesingerJsonInfos;
        AllCustomExaminationDataJson customExaminationDatas;

        public string path;
        public string name;

        private void Awake()
        {
            //Debug.Log(Application.persistentDataPath + "/test.assetbundle");
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(LoadProject(path + "/" + name + ".assetbundle"));
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                UnloadAsset();
            }
        }
        IEnumerator LoadProject(string filePath)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            LoadAssetBundle(bundle, transform);
            yield return null;
            LoadJsonForTrigger(bundle);
            yield return null;
            HandleJsonForTrigger(bundle);
            yield return null;
        }


        public void UnloadAsset()
        {
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

        public void LoadAssetBundle(AssetBundle content, Transform container)
        {
            if (content == null) return;
            string[] names = content.GetAllAssetNames();
            foreach (string n in names)
            {
                Debug.LogWarning($"遍历所有资源{name.Length}" + n);
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
            string contentName = "";//主物体名称
            dllName = "";//dll名称
            if (config != null)
            {
                byte[] c = config.bytes;
                MemoryStream ms = new MemoryStream(c);
                StreamReader sr = new StreamReader(ms);

                contentName = sr.ReadLine();//读取物体名称
                dllName = sr.ReadLine();//读取dll名称
                string count = sr.ReadLine();//读取脚本个数
                string scriptName = sr.ReadLine();//读取第一个脚本名称
                string gameObjectName = "";
                while (!string.IsNullOrEmpty(scriptName))
                {
                    scriptNames.Add(scriptName);
                    gameObjectName = sr.ReadLine();
                    if (gameObjectName != null)
                        gameObjectNames.Add(gameObjectName);
                    scriptName = sr.ReadLine();
                }
                sr.Close();
                ms.Close();

                mainObj = Instantiate(content.mainAsset as GameObject);
                assetLoaded.Add(mainObj);
                mainObj.transform.parent = container;
                mainObj.transform.position = container.position;
                mainObj.transform.localScale = Vector3.one;

                mainObj.name = contentName;
                //先加载自定义的数据配置
                LoadCustomJsonForComponent(content);
                if (!string.IsNullOrEmpty(dllName))
                {
                    TextAsset asset = content.LoadAsset<TextAsset>(dllName);
                    if (asset != null)
                    {
                        dll = System.Reflection.Assembly.Load(asset.bytes);
                    }
                    if (dll != null)
                    {
                        for (int i = 0; i < gameObjectNames.Count; i++)
                        {
                            //Debug.Log("try to load " + scriptNames[i] + " from dll in bundle");
                            System.Type t = dll.GetType(scriptNames[i]);
                            string objName = gameObjectNames[i].Substring(gameObjectNames[i].LastIndexOf("/") + 1, gameObjectNames[i].Length - gameObjectNames[i].LastIndexOf("/") - 1);
                            Transform trans = mainObj.transform.parent.FindObjFormChild(objName);
                            Component res = null;
                            if (trans != null && t != null)
                            {
                                res = trans.gameObject.AddComponent(t);
                                //自定义的脚本类型，初始化数据
                                if (res.GetType().Name == "Examination" || res.GetType().Name == "ExecuteExamiation")
                                {
                                    customExaminationDatas.customExaminationDatas.ForEach((item) =>
                                    {
                                        if (item.objName.Contains(objName))
                                        {
                                            try
                                            {
                                                MethodInfo method = null;
                                                object[] parameters;
                                                method = res.GetType().GetMethod("InitData", new Type[] { typeof(string) });
                                                parameters = new object[] { JsonConvert.SerializeObject(item)};
                                                method.Invoke(res, parameters);
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.LogError(ex.ToString());
                                            }
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadJsonForTrigger(AssetBundle content)
        {
            TextAsset aa = content.LoadAsset<TextAsset>("eventTriggerConfig");
            if (aa != null)
            {
                //兼容1.0版本的Bundle逻辑BundleEventTriggerJson的无参数类型的Json解析
                triggerDesingerJsonInfos = JsonConvert.DeserializeObject<AllTriggerToDesingerJson>(aa.text);
                TextAsset asset = content.LoadAsset<TextAsset>(dllName);
                dll = System.Reflection.Assembly.Load(asset.bytes);
                for (int i = 0;i < triggerDesingerJsonInfos.allJson.Count;i++)
                {
                    string triggerName = triggerDesingerJsonInfos.allJson[i].objName;
                    List<BundleEventTriggerInfo> bundleEventTriggerInfos = new List<BundleEventTriggerInfo>();
                    GameObject target;
                    UnityEngine.Object method;
                    BundleEventTriggerType triggerType;
                    List<BundleEventTriggerJson> bundleEventTriggerJsons = triggerDesingerJsonInfos.allJson[i].bundleEventTriggerDesigners.bundleEventTriggerJsons;
                    for (int j = 0;j < bundleEventTriggerJsons.Count;j++)
                    {
                        target = mainObj.transform.FindObjFormChild(bundleEventTriggerJsons[j].target).gameObject;
                        Type componentType = dll.GetType(bundleEventTriggerJsons[j].method);
                        Component mb = target.GetComponent(componentType);
                        method = mb;
                        triggerType = (BundleEventTriggerType)bundleEventTriggerJsons[j].triggerType;
                        //生成基本信息
                        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(target, method, triggerType);
                        bundleEventTriggerInfos.Add(temp);
                        //根据参数类型生成参数信息
                        switch (bundleEventTriggerJsons[j].parameterMode)
                        {
                            case (int)BundleListenerMode.None:
                                temp.parameterMode = BundleListenerMode.None; break;
                            case (int)BundleListenerMode.Void:
                                temp.parameterMode = BundleListenerMode.Void; break;
                            case (int)BundleListenerMode.Int:
                                temp.IntParameter = bundleEventTriggerJsons[j].IntParameter;
                                temp.parameterMode = BundleListenerMode.Int; break;
                            case (int)BundleListenerMode.Float:
                                temp.FloatParameter = bundleEventTriggerJsons[j].FloatParameter;
                                temp.parameterMode = BundleListenerMode.Float; break;
                            case (int)BundleListenerMode.String:
                                temp.StringParameter = bundleEventTriggerJsons[j].StringParameter;
                                temp.parameterMode = BundleListenerMode.String; break;
                            case (int)BundleListenerMode.Bool:
                                temp.BoolParameter = bundleEventTriggerJsons[j].BoolParameter;
                                temp.parameterMode = BundleListenerMode.Bool; break;
                            default: break;
                        }
                    }
                    triggerInfoCache.Add(triggerName, bundleEventTriggerInfos);
                }
            }
        }

        public void LoadCustomJsonForComponent(AssetBundle content)
        {
            TextAsset cc = content.LoadAsset<TextAsset>("customComponent");
            if (cc != null)
            {
                customExaminationDatas = JsonConvert.DeserializeObject<AllCustomExaminationDataJson>(cc.text);
                TextAsset asset = content.LoadAsset<TextAsset>(dllName);
                dll = System.Reflection.Assembly.Load(asset.bytes);
                for (int i = 0; i < customExaminationDatas.customExaminationDatas.Count; i++)
                {
                    string triggerName = customExaminationDatas.customExaminationDatas[i].objName;
                }
            }
        }
        public List<BundleEventTriggerInfo> GetJsonForTrigger(string triggerName)
        {
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
        public void HandleJsonForTrigger(AssetBundle content)
        {
            if (string.IsNullOrEmpty(dllName))
            {
                Debug.Log("HandleJsonForTrigger dllName IsNullOrEmpty return");
                return;
            }
            TextAsset asset = content.LoadAsset<TextAsset>(dllName);
            if (asset != null)
            {
                dll = System.Reflection.Assembly.Load(asset.bytes);

                for (int i = 0;i < triggerInfoCache.Count;i++)
                {
                    //获取BundleEventTrigger组件物体
                    GameObject triggerObj = GameObject.Find(triggerDesingerJsonInfos.allJson[i].objName);
                    if (triggerObj == null)
                    {
                        triggerObj = mainObj.transform.FindObjFormChild(triggerDesingerJsonInfos.allJson[i].objName).gameObject;
                    }
                    //获取物体上所有组件信息
                    Component[] components = triggerObj.GetComponents<Component>();
                    for (int j = 0;j < components.Length;j++)
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
        }

        /// <summary>
        /// 针对BundleEventTrigger组件
        /// 反射去调用针对BundleEventTrigger中某的初始化函数AddTriggerByElement
        /// 对selfBundleEventTriggerInfo的List增加元素
        /// </summary>
        /// <param name="t"></param>
        /// <param name="value"></param>
        private void TriggerAssignment(Component t, string value)
        {
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
                        MethodInfo method = null;
                        object[] parameters;
                        for (int i = 0;i < triggerInfoCache[value].Count;i++)
                        {
                            //根据参数类型获取构造函数和构建相应参数
                            switch (triggerInfoCache[value][i].parameterMode)
                            {
                                case BundleListenerMode.None:
                                    method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int), typeof(int) });
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType, triggerInfoCache[value][i].parameterMode }; break;
                                case BundleListenerMode.Void:
                                    method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int), typeof(int) });
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType, triggerInfoCache[value][i].parameterMode }; break;
                                case BundleListenerMode.Int:
                                    method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int), typeof(int), typeof(int) });
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType, triggerInfoCache[value][i].parameterMode, triggerInfoCache[value][i].IntParameter }; break;
                                case BundleListenerMode.Float:
                                    method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int), typeof(int), typeof(float) });
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType, triggerInfoCache[value][i].parameterMode, triggerInfoCache[value][i].FloatParameter }; break;
                                case BundleListenerMode.String:
                                    method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int), typeof(int), typeof(string) });
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType, triggerInfoCache[value][i].parameterMode, triggerInfoCache[value][i].StringParameter }; break;
                                case BundleListenerMode.Bool:
                                    method = type.GetMethod("AddTriggerByElement", new Type[] { typeof(GameObject), typeof(UnityEngine.Object), typeof(int), typeof(int), typeof(bool) });
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType, triggerInfoCache[value][i].parameterMode, triggerInfoCache[value][i].BoolParameter }; break;
                                default:
                                    parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType }; break;
                            }
                            //兼容1.0版本的Bundle逻辑只有AddTriggerByElement方法初始化但没有后面多余的int bool string float参数
                            if (method == null)
                            {
                                method = type.GetMethod("AddTriggerByElement");
                                parameters = new object[] { triggerInfoCache[value][i].target, triggerInfoCache[value][i].method, triggerInfoCache[value][i].triggerType };
                            }
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

        public void ResetAllMaterial(AssetBundle content)
        {
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
                    Debug.LogWarning($"查找到内置Shader={newShader} in material{ m.name}");
                    m.shader = newShader;
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
                            Debug.LogWarning($"加载了文件中的shader = {sh }in material{ m.name}");
                            break;
                        }
                    }
                    if (!find) Debug.LogWarning($"没有正确的shader { shaderName } in material { m.name}");
                }
            }
        }

        private IEnumerator SetMaterial(GameObject target, Material[] ms, bool activeSelf)
        {
            yield return 1;
            target.GetComponent<Renderer>().sharedMaterials = ms;
            target.GetComponent<Renderer>().sharedMaterial = ms[0];
            target.GetComponent<Renderer>().materials = ms;
            target.GetComponent<Renderer>().material = ms[0];
            target.SetActive(activeSelf);
        }

        private void ResetGameObjectShader(Transform target)
        {
            //2019.1.23   赵子夜修尝试修复精灵体组件材质丢失问题
            //MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
            Renderer renderer = target.GetComponent<Renderer>();

            if (renderer != null)
            {
                int mcount = renderer.sharedMaterials.Length;
                Material[] newMaterials = new Material[mcount];
                for (int j = 0;j < mcount;j++)
                {
                    Material _mater = renderer.sharedMaterials[j];
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


            for (int i = 0;i < target.childCount;i++)
            {
                ResetGameObjectShader(target.GetChild(i));
            }
        }
    }
    public static class Expand
    {
        public static Transform FindObjFormChild(this Transform transform, string objname)
        {
            Transform obj = null;
            if (transform.name == objname)
            {
                obj = transform;
                return obj;
            }

            if (transform.childCount > 0)
            {
                Transform child;
                for (int i = 0;i < transform.childCount;i++)
                {
                    child = transform.GetChild(i);
                    if (child.name == objname)
                    {
                        obj = child;
                        return obj;
                    }
                    if (obj == null)
                    {
                        obj = child.FindObjFormChild(objname);
                    }
                }
            }
            return obj;
        }
    }
}
