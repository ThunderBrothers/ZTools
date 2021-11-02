using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 实操的模型判断题目
/// </summary>
public class ExecuteExamiation : MonoBehaviour
{
    [HideInInspector]
    public List<ExecuteExamiationItemData> executeExamiationTarget = new List<ExecuteExamiationItemData>();
    [HideInInspector]
    public int score;
    [HideInInspector]
    public string stem;

    public Material rightMaterial;
    public Material wrongMaterial;
    public Material highlightmaterial;

    public Image resultImage;
    public Text stemText;
    public Sprite rightSprite;
    public Sprite wrongSprite;

    /// <summary>
    /// 题目的数据，json方便传递，会被制作到bundle动态加载
    /// </summary>
    [Header("数据")] 
    public string examinationData;
    public CustomExaminationData customExaminationData;

    public Dictionary<string, List<Material>> allMaterialDic = new Dictionary<string, List<Material>>();

    // Start is called before the first frame update
    [ContextMenu("SetValue")]

    void Start()
    {
        Debug.LogWarning("Start");
        rightMaterial = transform.Find("RightMaterial").GetComponent<MeshRenderer>().sharedMaterial;
        wrongMaterial = transform.Find("WrongMaterial").GetComponent<MeshRenderer>().sharedMaterial;
        highlightmaterial = transform.Find("Highlightmaterial").GetComponent<MeshRenderer>().sharedMaterial;
        ConfigData();
        Debug.Log("executeExamiationTarget.Count = " + executeExamiationTarget.Count);
        for (int i = 0;i < executeExamiationTarget.Count;i++)
        {
            MeshRenderer[] mr = executeExamiationTarget[i].target.GetComponentsInChildren<MeshRenderer>();
            List<Material> mtl = new List<Material>();
            for (int k = 0; k < mr.Length; k++)
            {
                mtl.Add(mr[k].sharedMaterial);
            }
            if (!allMaterialDic.ContainsKey(executeExamiationTarget[i].target.name))
            {
                Debug.Log($"target= {executeExamiationTarget[i].target.name}, GetComponentsInChildren Material.Count ={mtl.Count}");
                allMaterialDic.Add(executeExamiationTarget[i].target.name, mtl);
            } 
        }

        Transform title = transform.Find("TitleImage");
        Debug.Log("title" + title);
        rightSprite = transform.Find("TitleImage/Right").GetComponent<SpriteRenderer>().sprite;
        wrongSprite = transform.Find("TitleImage/Wrong").GetComponent<SpriteRenderer>().sprite;

        stemText = transform.Find("TitleImage/StemText").GetComponent<Text>();
        resultImage = transform.Find("TitleImage/ResultImage").GetComponent<Image>();
        stemText.text = stem;
    }

    string flow_data = "";
    /// <summary>
    /// 也是对外的接口，加载课程后初始化当前进度的题目答案
    /// </summary>
    /// <param name="index"></param>
    public void CheckRight(int index)
    {
        Debug.Log("实操题目执行此题目选项index =" + index);
        if (customExaminationData.questionType == QuestionType.Flow)
        {
            if(index>10)//表示还原
            {
                for (int i = 0; i < executeExamiationTarget.Count; i++)
                {
                    MeshRenderer[] mr = executeExamiationTarget[i].target.GetComponentsInChildren<MeshRenderer>();
                    for (int k = 0; k < mr.Length; k++)
                    {
                        //mr[k].sharedMaterial = wrongMaterial;
                        Material[] temp = new Material[mr[k].materials.Length];
                        for (int j = 0; j < mr[k].materials.Length; j++)
                        {
                            temp[j] = highlightmaterial;
                        }
                        mr[k].materials = temp;
                    }
                  
                }
                transform.Find("rightresult").gameObject.SetActive(true);

            }
            else//正常选择
            {
                int tempidx = index + 1;
                flow_data = flow_data + tempidx.ToString();
                if (flow_data.Length >= executeExamiationTarget.Count)
                {
                    if (flow_data.Equals(customExaminationData.flow_answer))
                    {
                        UpdateAnswer(true, int.Parse(flow_data));
                        transform.Find("rightresult").gameObject.SetActive(true);
                        transform.Find("rightaudio").gameObject.SetActive(true);
                    }
                    else
                    {
                        UpdateAnswer(false, int.Parse(flow_data));
                        transform.Find("rightresult").gameObject.SetActive(true);
                        transform.Find("wrongaudio").gameObject.SetActive(true);
                    }

                }
            }

           
        }
        else
        {
            for (int i = 0; i < executeExamiationTarget.Count; i++)
            {
                executeExamiationTarget[i].target.GetComponent<ExecuteEventTrigger>().triggerToggle = false;
                EventTrigger eventTrigger = executeExamiationTarget[i].target.GetComponent<EventTrigger>();
                if (eventTrigger != null)
                {
                    Destroy(eventTrigger);
                }
                if (executeExamiationTarget[i].right)
                {
                    MeshRenderer[] mr = executeExamiationTarget[i].target.GetComponentsInChildren<MeshRenderer>();
                    for (int j = 0; j < mr.Length; j++)
                    {
                        //mr[j].sharedMaterial = rightMaterial;
                        Material[] temp = new Material[mr[j].materials.Length];
                        for (int k = 0; k < mr[j].materials.Length; k++)
                        {
                            temp[k] = rightMaterial;
                        }
                        mr[j].materials = temp;
                    }
                    if (index == i)
                    {
                        Debug.Log("实操题目题目选项index =" + index + "  正确");
                        UpdateAnswer(true, index);
                    }
                }
                else if (i == index)
                {
                    UpdateAnswer(false, index);
                    MeshRenderer[] mr = executeExamiationTarget[i].target.GetComponentsInChildren<MeshRenderer>();
                    for (int k = 0; k < mr.Length; k++)
                    {
                        //mr[k].sharedMaterial = wrongMaterial;
                        Material[] temp = new Material[mr[k].materials.Length];
                        for (int j = 0; j < mr[k].materials.Length; j++)
                        {
                            temp[j] = wrongMaterial;
                        }
                        mr[k].materials = temp;
                    }
                }
            }
        }

 
    }

    /// <summary>
    /// 接受消息，格式
    /// 主结构   类型 + -type- + 内容
    /// 如考核题目类型
    /// 1 -type-分割  是发送消息的类型，如Examination代表考题
    /// 2 -select-分割  当时选的项序号
    /// 3 如果是考题Examination类型，则内容 = bool + "-select-" + 当时选的项序号 + "-score-" + 分数
    /// 4 可以根据具体的得分情况去做进一步的处理
    /// "Examination" + "-type-" + right + "-select-" + 当时选的项序号 + "-score-" + customExaminationData.score);
    /// </summary>
    /// <param name="right"></param>
    /// <param name="index">当时选的项序号</param>
    public void UpdateAnswer(bool right, int index)
    {
        try
        {
            //界面
            resultImage.gameObject.SetActive(true);
            if (right)
            {
                resultImage.sprite = rightSprite;
            }
            else
            {
                resultImage.sprite = wrongSprite;
            }
            
            Debug.Log("实操题目获得分数" + (right ? score + "  正确" : 0 + "  错误") + $"   SendMessageUpwards 向上发送MessageReceiver函数，内容Examination-type-{right}-select-{index}-score -{score}");
            SendMessageUpwards("MessageReceiver", "Examination" + "-type-" + right + "-select-" + index + "-score-" + score);
        }
        catch (Exception ex)
        {
            Debug.LogError("UpdateAnswer=>" + ex.ToString());
        }
    }

    /// <summary>
    /// 在Start前调用初始化
    /// </summary>
    /// <param name="data"></param>
    [ContextMenu("InitData")]
    public void InitData()
    {
        InitData(examinationData);
    }

    public void InitData(string data)
    {
        Debug.Log("InitData =" + data);
        customExaminationData = JsonConvert.DeserializeObject<CustomExaminationData>(data);
        examinationData = data;
    }

    private void ConfigData()
    {
        Debug.LogWarning("ConfigData");
        //根据记录的配置文件还原题目数据
        customExaminationData.examinationItemDatas.ForEach((item) =>
        {
            try
            {
                ExecuteExamiationItemData temp = new ExecuteExamiationItemData();
                string[] config = item.option.Split(new string[] { "&index=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                Transform tra = transform.Find(config[0]);
                Debug.Log($"{transform}.Find({config[0]}  tra = {tra}  config[1]={config[1]} ");
                temp.target = transform.Find(config[0]).gameObject;
                ExecuteEventTrigger executeEventTrigger = temp.target.GetComponent<ExecuteEventTrigger>();
                executeEventTrigger.index = int.Parse(config[1]);
                temp.right = item.right;
                Debug.Log("还原配置数据 " + "选项物体名称" + item.option + "是正确项" + item.right + " temp.target=" + temp.target?.name + " temp.right=" + temp.right);
                executeExamiationTarget.Add(temp);
                Debug.Log("ConfigData executeExamiationTarget.count" + executeExamiationTarget.Count);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        });
        score = customExaminationData.score;
        stem = customExaminationData.stem;
    }

    public CustomExaminationData GetCustomConfigData()
    {
        CustomExaminationData temp = new CustomExaminationData();
        temp.examinationItemDatas = new List<ExaminationItemData>();
        temp.score = score;
        temp.stem = stem;
        temp.questionType = customExaminationData.questionType;
        temp.flow_answer = customExaminationData.flow_answer;
        executeExamiationTarget.ForEach((item) =>
        {
            ExaminationItemData data = new ExaminationItemData();
            //记录物体的顺位，在加载后还原index，以判断选择选项
            //打包前修改Target的名称，供加载时差找   item.target.name + "EE"
            //option内保存目标物体对于父物体的路径

            //这里针对有动画的模型不能修改
            //item.target.name = item.target.name + "EE";
            item.target.name = item.target.name;
            ExecuteEventTrigger executeEventTrigger = item.target.GetComponent<ExecuteEventTrigger>();
            data.option = GetParName(item.target.transform,transform) + "&index=" + executeEventTrigger.index;
            data.right = item.right;
            temp.examinationItemDatas.Add(data);
        });

        return temp;
    }

    [System.Serializable]
    public class ExecuteExamiationItemData
    {
        /// <summary>
        /// 题目的物体
        /// </summary>
        public GameObject target;
        /// <summary>
        /// 是否 是正确答案
        /// </summary>
        public bool right;
    }

    private string GetParName(Transform child,Transform parent)
    {
        if (child.parent == parent.transform)
        {
            return child.name;
        }
        else
        {
            return GetParName(child.parent, parent) + "/" + child.name;
        }
    }
}
