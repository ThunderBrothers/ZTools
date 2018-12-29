using UnityEngine;
using System.Collections.Generic;
using ZTools.ZMath;


public class ZComparerTest : MonoBehaviour {

    public List<GameObject> objs = new List<GameObject>();
    // Use this for initialization
    void Start () {
        objs.Sort(new ZComparerTemplate());
        for (int i = 0;i < objs.Count;i++)
        {
            objs[i].transform.position = Vector3.right * 1.5f * i;
        }
    }
	
	// Update is called once per frame
	void Update () {
       
	}
  
}

