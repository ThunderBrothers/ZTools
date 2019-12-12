using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace ZTools.Finder
{
    public class CanBeSearch : MonoBehaviour
    {

        public GameObject target;
        public List<GameObject> gameObjects;
        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                var allTransforms = Resources.FindObjectsOfTypeAll(typeof(Transform));
                var previousSelection = Selection.objects;
                Selection.objects = allTransforms.Cast<Transform>()
                    .Where(x => x != null)
                    .Select(x => x.gameObject)
                    //如果你只想获取所有在Hierarchy中被禁用的物体，反注释下面代码
                    .Where(x => x != null && !x.activeInHierarchy)
                    .Cast<UnityEngine.Object>().ToArray();

                var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
                Selection.objects = previousSelection;
                gameObjects = selectedTransforms.Select(tr => tr.gameObject).ToList();
            }
        }
    }
}
