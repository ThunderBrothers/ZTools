using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ZTools.EditorExample
{
    [Serializable]
    public struct MobWave
    {
        public enum WaveType
        {
            Mobs,
            Boss
        }
        public WaveType Type;
        public GameObject Prefab;
        public int Count;
    }
    public class CIList : MonoBehaviour
    {
        [Header("可排序ObjList")]
        public List<GameObject> ObjList = new List<GameObject>();
        [Header("怪物波设置")]
        public List<MobWave> Waves = new List<MobWave>();
        //[Header("图片")]
        //public List<Sprite> S = new List<Sprite>();

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

