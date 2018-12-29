using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ZTools.EditorExample
{
    /// <summary>
    /// CAConfigEditor自定义了界面
    /// 读取数据
    /// </summary>
    public class CAConfig : MonoBehaviour
    {
        [Header("在脚本上右键LoadAsset可重新加载资源",order = 0)]
        [Header("资源引用", order = 5)]
        public CABullet bullet;
        [Header("___________显示读取值______________")]
        public BulletType Type = BulletType.Real;
        public int Speed;
        public int Damage;
        public GameObject eff;

        //脚本右键菜单增加LoadAsset
        [ContextMenu("LoadAsset")]
        public void LoadAsset()
        {
            string path = "Assets/ZTools/EditorExample/Recoures/BulletAsset.asset";
            bullet = AssetDatabase.LoadAssetAtPath<CABullet>(path);
        }
        //脚本右键菜单增加LoadAsset
        [ContextMenu("ShowAssetData")]
        public void ShowAssetData()
        {
            Type = bullet.Type;
            Speed = bullet.Speed;
            Damage = bullet.Damage;
            eff = bullet.eff;
        }
        private void OnEnable()
        {
            LoadAsset();
        }
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

