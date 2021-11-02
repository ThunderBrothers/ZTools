using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

/// <summary>
/// 课程设计播放适配组件
/// 使用带自定义参数的基类
/// </summary>
public class CoursewareVideoPlayer : BundleEventInfoParameterBase
{
    public VideoPlayer videoPlayer;
    private bool isPlaying = false;
    private bool isPrepare = false;

    public MeshRenderer playButtonMesh;
    public MeshRenderer replayButtonMesh;
    public Renderer playckBackMesh;
    public TextMesh playTimeText;
    public Transform PrecessTs;
    private float _process = 0;
    private long FrameNow;
    public float process
    {
        get
        {
            return _process;
        }
        set
        {
            _process = value;
            PrecessTs.localScale = new Vector3(_process, PrecessTs.localScale.y, PrecessTs.localScale.z);
        }
    }

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        playButtonMesh = transform.Find("Background/PlayState").GetComponent<MeshRenderer>();
        replayButtonMesh = transform.Find("Background/ReplayState").GetComponent<MeshRenderer>();
        playckBackMesh = transform.Find("Background").GetComponent<MeshRenderer>();
        playTimeText = transform.Find("Background/LengthText").GetComponent<TextMesh>();
        PrecessTs = transform.Find("Background/PlayLengthStartForward/PlayLengthStart");
    }

    private void Start()
    {
        videoPlayer.prepareCompleted += VideoPlay;
        videoPlayer.loopPointReached += VideoFinish;
    }

    void OnEnable()
    {
        if (videoPlayer.clip != null)
        {
            if (!videoPlayer.isPlaying)
            {
                PrecessTs.parent.gameObject.SetActive(false);
                if (playckBackMesh == null)
                {
                    playckBackMesh = this.GetComponent<MeshRenderer>();
                }
                playckBackMesh.enabled = false;
                videoPlayer.Prepare();
            }
        }
    }

    private void Update()
    {
        if (!isPlaying)
            return;
        FrameNow = videoPlayer.frame;
        if (videoPlayer.frameCount > 0)
        {
            if (videoPlayer.frameCount == 0)
                process = 0;
            else
                process = ((float)FrameNow / (float)(videoPlayer.frameCount));
        }
        ShowPlayTime();
    }

    /// <summary>
    /// 显示当前播放时间
    /// </summary>
    void ShowPlayTime()
    {
        int house = (int)(videoPlayer.time / 3600);
        int mins = (int)((videoPlayer.time - house * 3600) / 60);
        string second = (videoPlayer.time - mins * 60 - house * 3600).ToString("00");
        playTimeText.text = house.ToString("00") + ":" + mins.ToString("00") + ":" + second;
    }

    /// <summary>
    /// 播放
    /// </summary>
    public void VideoPlay(VideoPlayer player)
    {
        if (!PrecessTs.parent.gameObject.activeSelf)
        {
            PrecessTs.parent.gameObject.SetActive(true);
        }
        playckBackMesh.enabled = true;
        playButtonMesh.enabled = false;
        replayButtonMesh.enabled = false;
        isPrepare = true;
        videoPlayer.Play();
        isPlaying = true;
    }

    /// <summary>
    /// 视频暂停
    /// </summary>
    public void VideoPauseOrPlay()
    {
        if (!isPrepare)
        {
            Debug.LogError("CoursewareVideoPlayer video is not prepared");
            return;
        }
        if (isPlaying)
        {
            videoPlayer.Pause();
            isPlaying = false;
            playButtonMesh.enabled = true;
            replayButtonMesh.enabled = false;
        }
        else
        {
            videoPlayer.Play();
            isPlaying = true;
            playButtonMesh.enabled = false;
            replayButtonMesh.enabled = false;
        }
    }

    /// <summary>
    /// 重置播放
    /// </summary>
    public void ResetPlay()
    {
        FrameNow = 1;
        if (videoPlayer.frameCount == 0)
            process = 0;
        else
            process = ((float)FrameNow / (float)(videoPlayer.frameCount));
        videoPlayer.frame = FrameNow;
        playTimeText.text = "00" + ":" + "00" + ":" + "00";
    }

    /// <summary>
    /// 视频暂停
    /// </summary>
    public void VideoPause()
    {
        FrameNow = videoPlayer.frame;
        videoPlayer.Pause();  
    }

    public void VideoFinish(VideoPlayer source)
    {
        Debug.Log("VideoFinish");
        replayButtonMesh.enabled = true;
        isPlaying = false;
    }



    public override void OnBundleAction(PointerEventData eventData, int _IntParameter)
    {
       
    }

    public override void OnBundleAction(PointerEventData eventData, float _FloatParameter)
    {
        
    }

    public override void OnBundleAction(PointerEventData eventData, string _StringParameter)
    {
      
    }

    public override void OnBundleAction(PointerEventData eventData, bool _BoolParameter)
    {
        
    }

    public override void OnBundleAction(PointerEventData eventData)
    {
        Debug.Log("CoursewareVideoPlayer");
        VideoPauseOrPlay();
    }

    public override void OnReceiveMsg(string msg)
    {
        
    }

    public override bool supportPRS()
    {
        return true;
    }
}
