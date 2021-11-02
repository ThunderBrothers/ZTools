using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Video;

[CustomEditor(typeof(VideoPlayer),true)] //这里与DrawDefaultInspector结合使用会和原始界面有些不一样
public class VideoPlayerInspector : Editor
{
    public bool autoSetResolution = false;
    private VideoPlayer videoPlayer;
    private Renderer renderer;

    private void OnEnable()
    {
        videoPlayer = target as VideoPlayer;
        renderer = videoPlayer.targetMaterialRenderer;
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUI.changed && autoSetResolution)
        {
            //根据视频分辨率设置面板比例
            if (videoPlayer.clip != null && renderer != null)
            {
                Vector3 scale = new Vector3(videoPlayer.clip.width / 100f, videoPlayer.clip.height / 100f, 0.01f);
                renderer.gameObject.transform.localScale = scale;
                BoxCollider box = renderer.gameObject.GetComponentInParent<BoxCollider>();
                if (box)
                {
                    box.size = scale;
                }
            }
        }
    }
}