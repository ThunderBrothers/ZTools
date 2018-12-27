//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;

/// <summary>
/// Utility class to manipulate grouped scene configurations
/// </summary>
public class SMGroupedSceneConfigurationOperation {
	
	private string firstScreen;
	private string firstScreenAfterLevel;
	private SMWorkflowActionType actionAfterGroup;
	private string firstScreenAfterGroup;
	private List<string> screens = new List<string>();
	private List<string> groups = new List<string>();
	private Dictionary<string, List<string>> levels = new Dictionary<string, List<string>>();
	
	private SMGroupedSceneConfigurationOperation() {
	}
	
	public static SMGroupedSceneConfigurationOperation Build(SMGroupedSceneConfiguration configuration) {
		SMGroupedSceneConfigurationOperation result = new SMGroupedSceneConfigurationOperation();

		result.firstScreen = configuration.firstScreen;
		result.firstScreenAfterLevel = configuration.firstScreenAfterLevel;
		result.actionAfterGroup = configuration.actionAfterGroup;
		result.firstScreenAfterGroup = configuration.firstScreenAfterGroup;
		result.screens.AddRange(configuration.screens);
		
		int numberOfGroups = configuration.groups.Length;
		for(int i = 0; i < numberOfGroups; i++) {
			string group = configuration.groups[i];
			result.groups.Add(group);
			
			int startOfGroup = configuration.groupOffset[i];
			int endOfGroup = (i + 1 == numberOfGroups) /* last group ?*/ ? configuration.levels.Length : configuration.groupOffset[i+1];
			int length = endOfGroup - startOfGroup;
			
			string[] temp = new string[length];
			Array.Copy(configuration.levels, startOfGroup, temp, 0, length);
			result.levels.Add(group, new List<string>(temp));			
		}
		
		return result;
	}
	
	public SMGroupedSceneConfigurationOperation FirstScreen(string firstScreen) {
		this.firstScreen = firstScreen;
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation FirstScreenAfterLevel(string firstScreenAfterLevel) {
		this.firstScreenAfterLevel = firstScreenAfterLevel;
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation ActionAfterGroup(SMWorkflowActionType actionAfterGroup) {
		this.actionAfterGroup = actionAfterGroup;
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation FirstScreenAfterGroup(string firstScreenAfterGroup) {
		this.firstScreenAfterGroup = firstScreenAfterGroup;
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation Ignore(IList<string> scenes) {
		if (scenes != null) {
			screens.RemoveAll(item => scenes.Contains(item));
			foreach(List<string> levelsInGroup in levels.Values) {
				levelsInGroup.RemoveAll(item => scenes.Contains(item));
			}
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation Screen(IList<string> scenes) {
		if (scenes != null) {
			foreach(List<string> levelsInGroup in levels.Values) {
				levelsInGroup.RemoveAll(item => scenes.Contains(item));
			}
	
			List<string> tmp = new List<string>(scenes);
			tmp.RemoveAll(item => screens.Contains(item));
			screens.AddRange(tmp);
		}
		return this;	
	}
	
	public SMGroupedSceneConfigurationOperation Level(IList<string> scenes, string targetGroup) {
		if (scenes != null) {
			screens.RemoveAll(item => scenes.Contains(item));
			
			foreach(string group in groups) {
				List<string> levelsInGroup = levels[group];
				if (group == targetGroup) {
					// add missing levels
					List<string> tmp = new List<string>(scenes);
					tmp.RemoveAll(item => levelsInGroup.Contains(item));
					levelsInGroup.AddRange(tmp);				
				} else {
					// remove all levels
					levelsInGroup.RemoveAll(item => scenes.Contains(item));
				}
			}
		}
		return this;		
	}

	public SMGroupedSceneConfigurationOperation MoveLevelToTop(IList<string> levelsToMove, string targetGroup) {
		List<string> levelsInGroup = levels[targetGroup];
		if (levelsInGroup != null) {		
			new ListOperation<string>(levelsInGroup).MoveToTop(levelsToMove);
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation MoveLevelUp(IList<string> levelsToMove, string targetGroup) {
		List<string> levelsInGroup = levels[targetGroup];
		if (levelsInGroup != null) {		
			new ListOperation<string>(levelsInGroup).MoveUp(levelsToMove);
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation MoveLevelDown(IList<string> levelsToMove, string targetGroup) {
		List<string> levelsInGroup = levels[targetGroup];
		if (levelsInGroup != null) {		
			new ListOperation<string>(levelsInGroup).MoveDown(levelsToMove);
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation MoveLevelToBottom(IList<string> levelsToMove, string targetGroup) {
		List<string> levelsInGroup = levels[targetGroup];
		if (levelsInGroup != null) {		
			new ListOperation<string>(levelsInGroup).MoveToBottom(levelsToMove);
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation MoveLevelToPosition(IList<string> levelsToMove, int index, string targetGroup) {
		List<string> levelsInGroup = levels[targetGroup];
		if (levelsInGroup != null) {		
			new ListOperation<string>(levelsInGroup).MoveTo(levelsToMove, index);
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation AddGroup(string name) {
		if (!groups.Contains(name)) {
			groups.Add(name);
			levels.Add(name, new List<string>());
		}
		return this;
	}
	
	public SMGroupedSceneConfigurationOperation RemoveGroup(string name) {
		groups.Remove(name);
		levels.Remove(name);
		return this;
	}

	public SMGroupedSceneConfigurationOperation RenameGroup(string newName, string oldName) {
		if (newName != oldName) {
			int index = groups.IndexOf(oldName);
			if (index != -1) {
				groups[index] = newName;
				levels.Add(newName, levels[oldName]);
				levels.Remove(oldName);
			}
		}
		return this;
	}

	public SMGroupedSceneConfigurationOperation MoveGroupToPosition(string groupToMove, int index) {
		new ListOperation<string>(groups).MoveTo(new string[]{groupToMove}, index);
		return this;
	}
		
	public void Apply(SMGroupedSceneConfiguration configuration) {
		configuration.screens = screens.ToArray();
		configuration.groups = groups.ToArray();
		
		List<int> groupOffsets = new List<int>();
		List<string> allLevels = new List<string>();
		foreach(string group in groups) {
			groupOffsets.Add(allLevels.Count);
			allLevels.AddRange(levels[group]);
		}
		
		configuration.groupOffset = groupOffsets.ToArray();
		configuration.levels = allLevels.ToArray();
		
		if (firstScreen != null && !screens.Contains(firstScreen)) {	
			firstScreen = null;
		}
		configuration.firstScreen = firstScreen;
		
		if (firstScreenAfterLevel != null && !screens.Contains(firstScreenAfterLevel)) {
			firstScreenAfterLevel = null;
		}
		configuration.firstScreenAfterLevel = firstScreenAfterLevel;
		
		if (actionAfterGroup == SMWorkflowActionType.LoadNextLevel) {
			firstScreenAfterGroup = null;
		}
		configuration.actionAfterGroup = actionAfterGroup;
		
		if (firstScreenAfterGroup != null && !screens.Contains(firstScreenAfterGroup)) {
			firstScreenAfterGroup = null;
		}
		configuration.firstScreenAfterGroup = firstScreenAfterGroup;	
	}	
}

