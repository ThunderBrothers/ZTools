//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using UnityEngine;

/// <summary>
/// Default implementation of the <see cref="SMILevelProgress"/> interface that uses the build-in player preferences
/// to store and load information. See <see cref="UnityEngine.PlayerPrefs"/>.
/// </summary>
public class SMLevelProgress : SMILevelProgress {
	
	private string prefix = "";
	
	/// <summary>
	/// Ctor. Uses an empty prefix. It is recommended to use the Ctor with prefix to avoid name clashes.
	/// </summary>
	public SMLevelProgress() {
	}
	
	/// <summary>
	/// Ctor.
	/// </summary>
	/// <param name="prefix">
	/// A <see cref="System.String"/> denoting a prefix that is being used for storing the level progress. Using
	/// the prefix is recommended to avoid name clashes with other settings of your game.
	/// </param>
	public SMLevelProgress(string prefix) {
		if (!string.IsNullOrEmpty(prefix)) {
			this.prefix = prefix + "_";
		}
	}
	
	public SMLevelStatus this[string levelId] {
		get {
			return GetLevelStatus(levelId);
		}
		set {
			SetLevelStatus(levelId, value);
		}
	}
	
	public SMLevelStatus GetLevelStatus (string levelId)
	{
		return (SMLevelStatus) PlayerPrefs.GetInt(prefix + "LS." + levelId, (int) SMLevelStatus.New);
	}
	
	public void SetLevelStatus (string levelId, SMLevelStatus levelStatus)
	{
		PlayerPrefs.SetInt(prefix + "LS." + levelId, (int) levelStatus);
		
	} 
	
	public SMGroupStatus GetGroupStatus (string groupId)
	{
		return (SMGroupStatus) PlayerPrefs.GetInt(prefix + "GS." + groupId, (int) SMGroupStatus.New);
	}
	
	public void SetGroupStatus (string groupId, SMGroupStatus groupStatus)
	{
		PlayerPrefs.SetInt(prefix + "GS." + groupId, (int) groupStatus);
	}
	
	public string LastLevelId 
    {
		get {
			return PlayerPrefs.GetString(prefix + "LastLevel", "");
		}
		set {
			PlayerPrefs.SetString(prefix + "LastLevel", value);
		}
	}
	
	public string CurrentLevelId {
		get {
			return PlayerPrefs.GetString(prefix + "CurrentLevel", "");
		}
		set {
			PlayerPrefs.SetString(prefix + "CurrentLevel", value);
		}
	}
			
	public bool HasPlayed {
		get {
			return PlayerPrefs.HasKey(prefix + "LastLevel");
		}
	}	
	
	public void ResetLastLevel() {
		PlayerPrefs.DeleteKey(prefix + "LastLevel");
		PlayerPrefs.DeleteKey(prefix + "CurrentLevel");
	}
	
}
