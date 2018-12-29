using UnityEngine;
using System.Collections;
using ZTools.CsvReader;

[System.Serializable]
public class TypeARCsvLevel : CsvBase {
    public int id;
    public int level;
    public string info;
    public int limit;
    public int num;
    public string[] animal;
    
}
