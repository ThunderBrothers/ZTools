using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class BundleTest : MonoBehaviour {


    public GameObject obj;


    // Use this for initialization
    void Start() {

        obj.SendMessage("LogSomething", SendMessageOptions.RequireReceiver);
    }

}
