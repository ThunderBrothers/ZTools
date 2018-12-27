//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;

public class SMGroupedSceneConfigurationAdapter : SMIConfigurationAdapter {
	
	private SMGroupedSceneConfiguration configuration;
	private Dictionary<string, string[]> levelsInGroup = new Dictionary<string, string[]>();
	private Dictionary<string,string> groupOfLevel = new Dictionary<string, string>();
	
	public SMGroupedSceneConfigurationAdapter(SMGroupedSceneConfiguration configuration) {
		this.configuration = configuration;
		
		for(int i = 0; i < configuration.groups.Length; i++) {
			var groupOffset = i;
			
			var group = Groups[i];
			
			var start = configuration.groupOffset[groupOffset];
			var end = (groupOffset + 1 == Groups.Length) /* last group ?*/ ? Levels.Length : configuration.groupOffset[groupOffset+1];
			var len = end - start;
			
			var result = new string[len];
			Array.Copy(Levels, start, result, 0, len);
			levelsInGroup[group] = result;
			
			foreach(var lvl in result) {
				groupOfLevel[lvl] = group;
			}
		}
	}
	
	public string Name { 
		get { return configuration.name; } 
	}
	
	public string[] Levels { 
		get { return configuration.levels; }
	}
	
	public string[] Groups { 
		get { return configuration.groups; } 
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
			return configuration.actionAfterGroup;
		}
	}
	
	public string FirstScreenAfterGroup { 
		get { return configuration.firstScreenAfterGroup; }
	}
	
	public string[] GetLevelsInGroup(string groupId) {
		if (!GroupExists(groupId)) {
			throw new ArgumentException("Group " + groupId + " does not exist");
		}

		return levelsInGroup[groupId];
	}
	
	
	public string GetGroupOfLevel(string levelId) {
		if ( !LevelExists(levelId) ) {
			throw new ArgumentException("Level " + levelId + " does not exist");
		}
		return groupOfLevel[levelId];
		
	}
	
	public bool LevelExists (string levelId)
	{
		return groupOfLevel.ContainsKey(levelId);
	}
	
	public bool GroupExists (string groupId)
	{
		return levelsInGroup.ContainsKey(groupId);
	}
	
}

