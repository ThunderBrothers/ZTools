using UnityEngine;
using System.Collections;

[System.Serializable]
public class TypeARCsvConfig : CsvBase {
	public int id;
	/// 游戏最大玩家数量
	public int max_player_count;
	public float time_out;
	public float time_remove;
	public float time_change_point;
}
