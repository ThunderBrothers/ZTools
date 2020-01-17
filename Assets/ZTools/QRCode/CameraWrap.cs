using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aquaivy.Unity;
using UnityEngine.UI;

namespace ZTools.QRCode
{
    public class CameraWrap
    {

        private WebCamTexture webCameraTexture;

        private DateTime lastCameraTaskTime;
        private Func<bool> isDecoding;
        private Action<Color32[], int, int> decodeAsync;
        private Vector2 resolution;
        public GameObject rayimage;
        private TaskLite cameraTask;
        public int DecodeInterval = 500;

        public CameraWrap(Vector2 resolution, Func<bool> isDecoding, Action<Color32[], int, int> decodeAsync)
        {
            this.resolution = resolution;
            this.isDecoding = isDecoding;
            this.decodeAsync = decodeAsync;
        }

        public void Open()
        {
            webCameraTexture = GetCameraTexture();

            if (webCameraTexture == null)
            {
                Debug.Log("Not found available camera");
                return;
            }

            Debug.Log($"CameraWrap: Camera Opened, total: {WebCamTexture.devices.Length}  use: {webCameraTexture.deviceName}  resolution:{webCameraTexture.requestedWidth}x{webCameraTexture.requestedHeight}");

            //rayimage = GameObject.Find("CameraRawImage");
            if (rayimage != null)
            {
                rayimage.SetActive(true);
                var renderer = rayimage.GetComponent<RawImage>();
                renderer.texture = webCameraTexture;
            }
            webCameraTexture.Play();

            StartCameraTask();
        }

        public void Close()
        {
            if (webCameraTexture != null)
            {
                Debug.Log($"CameraWrap: Camera Closed");
                webCameraTexture.Stop();
                webCameraTexture = null;
            }

            rayimage.SetActive(false);
            cameraTask?.Release();
            cameraTask = null;
        }

        public void StartScan()
        {
            StartCameraTask();
        }
        public void StopScan()
        {
            cameraTask?.Release();
            cameraTask = null;
        }

        private WebCamTexture GetCameraTexture()
        {
            // 方案一：Linq查询（结构体如何判空？）
            //WebCamDevice? device = WebCamTexture.devices.FirstOrDefault(o => o.isFrontFacing);
            //if (device == null || string.IsNullOrEmpty(device.Value.name))
            //    return null;

            //WebCamTexture camera = new WebCamTexture(device.Value.name, (int)resolution.x, (int)resolution.y);

            //return camera;


            // 方案二：传统自己查找
            if (WebCamTexture.devices.Length == 0)
                return null;

            WebCamTexture camera = null;

            foreach (var device in WebCamTexture.devices)
            {

                if (device.isFrontFacing)
                {
                    camera = new WebCamTexture(device.name, (int)resolution.x, (int)resolution.y);
                    return camera;
                }
            }

            string name = WebCamTexture.devices[0].name;
            //Log.Info($"Not found FrontFacing camera, use first camera: {name}");
            camera = new WebCamTexture(name, (int)resolution.x, (int)resolution.y);
            return camera;
        }

        private void StartCameraTask()
        {
            lastCameraTaskTime = DateTime.Now;
            cameraTask = TaskLite.Invoke(t =>
            {
                if ((DateTime.Now - lastCameraTaskTime).TotalMilliseconds < DecodeInterval)
                {
                    return false;
                }
                if (isDecoding())
                {
                    return false;
                }

                lastCameraTaskTime = DateTime.Now;

                var data = webCameraTexture.GetPixels32();
                var width = webCameraTexture.width;
                var height = webCameraTexture.height;

                decodeAsync.Invoke(data, width, height);

                return t < 0;
            });
        }
    }
}