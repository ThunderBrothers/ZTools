using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 附带参数类型触发示例脚本
/// 设置物体播放动画
/// 根据在设计师组件上写入的参数类型及参数执行一下对应重写函数
/// _StringParameter传入的string类型值
/// 执行播放某个名称的动画
/// </summary>
public class PlayAnimationByName : BundleEventInfoParameterBase
{
    public Animation anim;
    public bool isPlay = true;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animation>();
    }
    
    public override void OnBundleAction(PointerEventData eventData)
    {
        
    }
    /// <summary>
    /// 多人同步回调函数
    /// </summary>
    /// <param name="msg"></param>
    /// --------------------------------注意---------------------------------------------
    /// 只能实现对应的OnBundleAction重写函数中的一个即可，负责导致回调函数无法判断参数类型
    public override void OnReceiveMsg(string msg)
    {
        if (anim != null)
        {
            if (msg == "PlayOrStop")
            {
                if (isPlay)
                {
                    anim.Stop();
                }
                else
                {
                    anim.Play();
                }
                isPlay = !isPlay;
            }
            else
            {
                anim.Play(msg);
            }
        }
        else
        {
            Debug.Log("PlayAnimationByName GetComponent<Animation>()  anim = Null");
        }
    }

    public override bool supportPRS()
    {
        return false;
    }

    public override void OnBundleAction(PointerEventData eventData, int _IntParameter)
    {
       
    }

    public override void OnBundleAction(PointerEventData eventData, float _FloatParameter)
    {
       
    }

    public override void OnBundleAction(PointerEventData eventData, string _StringParameter)
    {
        if (anim != null)
        {
            if (_StringParameter == "PlayOrStop")
            {
                if (isPlay)
                {
                    anim.Stop();
                }
                else
                {
                    anim.Play();
                }
                isPlay = !isPlay;
            }
            else
            {
                anim.Play(_StringParameter);
            }
        }else
        {
            Debug.Log("PlayAnimationByName GetComponent<Animation>()  anim = Null");
        }
        SendMsg(_StringParameter.ToString());
    }

    public override void OnBundleAction(PointerEventData eventData, bool _BoolParameter)
    {
       
    }
}
