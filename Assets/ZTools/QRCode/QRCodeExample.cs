using Aquaivy.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ZTools.QRCode
{
    public class QRCodeExample : MonoBehaviour
    {
        private CameraWrap cameraWrap;
        private QRCodeDecoder decoder;

        public RawImage image;
        public Button button;
        public Text text;

        public bool Decoding { get; private set; }
        private Vector2 cameraResolution = new Vector2(640, 400);
        // Start is called before the first frame update
        void Start()
        {
            Init();
            button.onClick.AddListener(()=> {
                cameraWrap.StartScan();
                text.text = "";
                button.interactable = false;
            });
        }

        public void Init()
        {
            decoder = new QRCodeDecoder(DecodeSuccess, DecodeFailure);
            cameraWrap = new CameraWrap(cameraResolution, () => Decoding, DecodeAsync);
            cameraWrap.rayimage = image.gameObject;
            cameraWrap.Open();
        }

        public void DecodeSuccess(string text)
        {
            TaskLite.Invoke(t =>
            {
                Decoding = false;
                // Open/Close camera must be in main thread
                cameraWrap?.StopScan();
                button.interactable = true;
                this.text.text = text;
                return true;
            });
        }

        private void DecodeFailure(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                DecodeNothing();
                TaskLite.Invoke((t)=>
                {
                    text.text = "DecodeNothing";
                    return true;
                });
               
                return;
            }

            TaskLite.Invoke(t =>
            {
                Decoding = false;
                // Open/Close camera must be in main thread
                cameraWrap.StopScan();
                button.interactable = true;
                return true;
            });
        }

        private void DecodeAsync(Color32[] data, int width, int height)
        {
            Decoding = true;
            text.text = "";
            Task.Run(() =>
            {
                decoder.Decode(data, width, height);
            });
        }

        private void DecodeNothing()
        {
            TaskLite.Invoke(t =>
            {
                Decoding = false;
                return true;
            });
        }

        public void StartScan()
        {
            cameraWrap.Open();
        }

        public void StopScan()
        {
            cameraWrap?.StopScan();
            cameraWrap?.Close();
        }
    }
}
