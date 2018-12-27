using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZComparer : IComparer<char>
{
    public string temp;
    public int Compare(char x, char y)
    {
        temp += x;
        return x.CompareTo(y);
    }
}
public class Word : MonoBehaviour {

    public string _str;
    public List<char> _chaList;
	// Use this for initialization
	void Start () {
        int length = _str.Length;
        _chaList = new List<char>();
        for (int i = 0;i < length;i++)
        {
            _chaList.Add(_str[i]);
        }
        ZComparer z = new ZComparer();
        _chaList.Sort(z);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
