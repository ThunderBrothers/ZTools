using UnityEngine;
using System.Collections;
using ZTools.CsvReader;

[System.Serializable]
public class TypeARGoldConfig : CsvBase {
    public int id;
    public int type;
    public int count;
    public float rent;
    public float huntingrent;
    public float bushGolds;
    public float height;
    public float boom;
    public float rotateSpeedBoom;
    public float rotateSpeedState;
    public float inertia;
    public int personal;
    public float autoPick;

}
