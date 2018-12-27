//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;

/// <summary>
/// Unmodifiable implementation of <see cref="SMILevelProgress" />.
/// </summary>
public class SMUnmodifiableLevelProgress : SMILevelProgress {
	
	private Dictionary<string,SMLevelStatus> status = new Dictionary<string, SMLevelStatus>();
	private Dictionary<string,SMGroupStatus> groupStatus = new Dictionary<string, SMGroupStatus>();
	private bool hasPlayed;
	private string lastLevelId;
	private string currentLevelId;
	
	public SMUnmodifiableLevelProgress (SMILevelProgress levelProgress, SMIConfigurationAdapter configurationAdapter) {
		foreach(var level in configurationAdapter.Levels ) {
			status.Add(level, levelProgress.GetLevelStatus(level));
		}
		foreach(var group in configurationAdapter.Groups ) {
			groupStatus.Add (group, levelProgress.GetGroupStatus(group));
		}
		hasPlayed = levelProgress.HasPlayed;
		lastLevelId = levelProgress.LastLevelId;
		currentLevelId = levelProgress.CurrentLevelId;
	}
	
	
	public SMLevelStatus this[string levelId] {
		get {
			return GetLevelStatus(levelId);
		}
		set {
			throw new NotImplementedException ();
		}
	}
	
	public SMLevelStatus GetLevelStatus (string levelId) {
		if ( status.ContainsKey(levelId)) {
			return status[levelId];			
		}
		return SMLevelStatus.New;
	}
	
	public void SetLevelStatus (string levelId, SMLevelStatus levelStatus)	{
		throw new NotImplementedException ();
	}
	
	public SMGroupStatus GetGroupStatus (string groupId) {
		if (groupStatus.ContainsKey(groupId)) {
			return groupStatus[groupId];
		}
		return SMGroupStatus.New;
	}
	
	
	public void SetGroupStatus (string groupId, SMGroupStatus groupStatus) {
		throw new NotImplementedException ();
	}
	
	public bool HasPlayed {
		get {
			return hasPlayed;
		}
	}	
	
	public string LastLevelId {
		get {
			return lastLevelId;
		}
		set {
			throw new NotImplementedException ();
		}
	}

	public string CurrentLevelId {
		get {
			return currentLevelId;
		}
		set {
			throw new NotImplementedException ();
		}
	}

	
	public void ResetLastLevel () {
		throw new NotImplementedException ();
	}

}

