using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTools.SingletonModule {
    public class SingleSample : MonoBehaviour {

        // Use this for initialization
        void Start() {
            Debug.Log(SingleManager.GetInstance.str);  
        }

        // Update is called once per frame
        void Update() {

        }
    }
}

