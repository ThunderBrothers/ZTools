using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Linq;
using System;

public class Examination : MonoBehaviour
{
    /// <summary>
    /// 题目的数据，json方便传递，会被制作到bundle动态加载
    /// </summary>
    public string examinationData;
    //[HideInInspector]
    public CustomExaminationData customExaminationData = new CustomExaminationData();

    public Text stemText;
    public List<Button> buttons;
    public Sprite rightSprite;
    public Sprite wrongSprite;
    public HorizontalOrVerticalLayoutGroup layoutGroup;


    // Start is called before the first frame update
    [ContextMenu("SetValue")]
    void Start()
    {
        stemText = transform.Find("SetmText").GetComponent<Text>();
        layoutGroup = transform.Find("Buttons").GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
        buttons = transform.Find("Buttons").GetComponentsInChildren<Button>().ToList(); 
        stemText.text = customExaminationData.stem;

        rightSprite = transform.Find("Right").GetComponent<SpriteRenderer>().sprite;
        wrongSprite = transform.Find("Wrong").GetComponent<SpriteRenderer>().sprite;
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].GetComponentInChildren<Text>().text = customExaminationData.examinationItemDatas[i].option;
            buttons[i].onClick.AddListener(delegate { CheckRight(index); });
        }
        layoutGroup.SetLayoutVertical();
    }

    /// <summary>
    /// 也是对外的接口，加载课程后初始化当前进度的题目答案
    /// </summary>
    /// <param name="index"></param>
    public void CheckRight(int index)
    {
        Debug.Log("选择了题目index =" + index);
        //0 无效项 1 正确 2 错误
        int[] results = new int[customExaminationData.examinationItemDatas.Count];
        for (int i = 0; i < customExaminationData.examinationItemDatas.Count; i++)
        {
            buttons[i].interactable = false;
            if (customExaminationData.examinationItemDatas[i].right)
            {
                results[i] = 1;
                if (index == i)
                {
                    UpdateAnswer(true,index);
                }
            }
            else if (index == i)
            {
                results[i] = 2;
                UpdateAnswer(false, index);
            }
        }
        SetButtonPerformance(results);
    }

    public void SetButtonPerformance(int[] results)
    {
        for (int i = 0; i < results.Length; i++)
        {
            SpriteState spriteState = buttons[i].spriteState;
            if (results[i] == 1)
            {
                spriteState.disabledSprite = rightSprite;
                buttons[i].spriteState = spriteState;
            }
            else if (results[i] == 2)
            {
                buttons[i].GetComponent<Image>().sprite = wrongSprite;
                spriteState.disabledSprite = wrongSprite;
                buttons[i].spriteState = spriteState;
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
    public void UpdateAnswer(bool right,int index)
    {
        try
        {
            Debug.Log("获得分数" + (right ? customExaminationData.score + "  正确" : 0 + "  错误") + $"SendMessageUpwards 向上发送MessageReceiver函数，内容Examination-type-{right}-select-{index}-score -{customExaminationData.score}");
            SendMessageUpwards("MessageReceiver", "Examination" + "-type-" + right + "-select-" + index + "-score-" + customExaminationData.score);
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
    public void InitData(string data)
    {
        Debug.Log("InitData =" + data);
        customExaminationData = JsonConvert.DeserializeObject<CustomExaminationData>(data);
        examinationData = data;
    }
}