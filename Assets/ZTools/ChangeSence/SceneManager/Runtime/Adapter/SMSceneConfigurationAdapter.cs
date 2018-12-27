//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;

public class SMSceneConfigurationAdapter : SMIConfigurationAdapter {
	
	private static string DefaultGroupName = "default";
	private static string[] DefaultGroups = new string[] {DefaultGroupName};
	
	private SMSceneConfiguration configuration;
	
	public SMSceneConfigurationAdapter(SMSceneConfiguration configuration) {
		this.configuration = configuration;
	}
	
	public string Name { 
		get { return configuration.name; } 
	}
	
	public string[] Levels { 
		get { return configuration.levels; }
	}
	
	public string[] Groups { 
		get { return DefaultGroups; } 
	}
	
	public string[] Screens { 
		get { return configuration.screens; }
	}
	
	public string FirstScreen { 
		get { return configuration.firstScreen; } 
	}
	
	public string FirstScreenAfterLastLevel { 
		get { return configuration.firstScreenAfterLevel; }
	}
	
	public SMWorkflowActionType ActionAfterGroup {
		get {
			return SMWorkflowActionType.LoadScreen;
		}
	}
	
	public string FirstScreenAfterGroup { 
		get { return FirstScreenAfterLastLevel; }
	}
	
	public string[] GetLevelsInGroup(string groupId) {
		return Levels;
	}
	
	public string GetGroupOfLevel(string levelId) {
		return DefaultGroupName;
	}
	
	
	public bool LevelExists (string levelId) {
		return Array.IndexOf(Levels, levelId) != -1;
	}
	
	public bool GroupExists (string groupId) {
		return DefaultGroupName.Equals(groupId);
	}
}

