using UnityEngine;
using ZTools.CsvReader;


public class ReaderTest : MonoBehaviour {

    public TypeARCsvLevel config1;
    public string CSVType;
    public string AllCsvPathRoot = "H:/ZTools/ZTools/Assets/ZTools/CsvReader/csv/";
	public string IniRootPathAR = "list.xlsx";

    // Use this for initialization
    void Start () {
        //读表
        CsvManager.Ini(AllCsvPathRoot, IniRootPathAR);
    }
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            config1 = CsvManager.GetLevel("1");
        }
	}
}
