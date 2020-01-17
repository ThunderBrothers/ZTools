using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;

namespace ZTools.QRCode
{
    public class QRCodeDecoder
    {
        private Action<string> decodeSuccess;
        private Action<string> decodeFailure;

        public QRCodeDecoder(Action<string> decodeSuccess, Action<string> decodeFailure)
        {
            this.decodeSuccess = decodeSuccess;
            this.decodeFailure = decodeFailure;
        }

        public void Decode(Color32[] data, int width, int height)
        {
            var barcodeReader = new BarcodeReader { AutoRotate = true, TryInverted = true };
            try
            {
                Result result = barcodeReader.Decode(data, width, height);
                if (result != null)
                {
                    Debug.Log($"QRCodeDecoder decode success: {result.Text}");
                    decodeSuccess?.Invoke(result.Text);
                }
                else
                {
                    Debug.Log("Did not identify the qr code");
                    decodeFailure?.Invoke(string.Empty);
                }
                data = null;
            }
            catch (Exception ex)
            {
                Debug.LogError("QRCodeDecoder Decode error" + ex.ToString());
                decodeFailure?.Invoke(ex.ToString());
            }
        }

        public void Decode(byte[] data, int width, int height)
        {
            var barcodeReader = new BarcodeReader { AutoRotate = true, TryInverted = true };
            try
            {
                Result result = barcodeReader.Decode(data, width, height, RGBLuminanceSource.BitmapFormat.RGB24);
                if (result != null)
                {
                    Debug.Log($"QRCodeDecoder decode success: {result.Text}");
                    decodeSuccess?.Invoke(result.Text);
                }
                else
                {
                    Debug.Log("Did not identify the qr code");
                    decodeFailure?.Invoke(string.Empty);
                }
                data = null;
            }
            catch (Exception ex)
            {
                Debug.LogError("QRCodeDecoder Decode error" + ex.ToString());
                decodeFailure?.Invoke(ex.ToString());
            }
        }
    }
}
