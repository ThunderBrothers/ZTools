//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class to modify scene configurations
/// </summary>
public class SMSceneConfigurationOperation {
	
	private string firstScreen;
	private string firstScreenAfterLevel;
	private List<string> screens = new List<string>();
	private List<string> levels = new List<string>();
	
	private SMSceneConfigurationOperation() {
	}
	
	public static SMSceneConfigurationOperation Build(SMSceneConfiguration configuration) {
		SMSceneConfigurationOperation result = new SMSceneConfigurationOperation();		
		result.firstScreen = configuration.firstScreen;
		result.firstScreenAfterLevel = configuration.firstScreenAfterLevel;
		result.screens.AddRange(configuration.screens);
		result.levels.AddRange(configuration.levels);
		return result;
	}	
	
	public SMSceneConfigurationOperation FirstScreen(string firstScreen) {
		this.firstScreen = firstScreen;
		return this;
	}
	
	public SMSceneConfigurationOperation FirstScreenAfterLevel(string firstScreenAfterLevel) {
		this.firstScreenAfterLevel = firstScreenAfterLevel;
		return this;
	}
	
	public SMSceneConfigurationOperation Ignore(IList<string> scenes) {
		if (scenes != null) {
			screens.RemoveAll(item => scenes.Contains(item));
			levels.RemoveAll(item => scenes.Contains(item));
		}
		return this;
	}
	
	public SMSceneConfigurationOperation Screen(IList<string> scenes) {
		if (scenes != null) {
			levels.RemoveAll(item => scenes.Contains(item));
			
			List<string> tmp = new List<string>(scenes);
			tmp.RemoveAll(item => screens.Contains(item));
			screens.AddRange(tmp);
		}
		return this;	
	}
	
	public SMSceneConfigurationOperation Level(IList<string> scenes) {
		if (scenes != null) {
			screens.RemoveAll(item => scenes.Contains(item));
			
			List<string> tmp = new List<string>(scenes);
			tmp.RemoveAll(item => levels.Contains(item));
			levels.AddRange(tmp);
		}
		return this;		
	}

	public SMSceneConfigurationOperation MoveLevelToTop(IList<string> levelsToMove) {
		new ListOperation<string>(levels).MoveToTop(levelsToMove);
		return this;
	}
	
	public SMSceneConfigurationOperation MoveLevelUp(IList<string> levelsToMove) {
		new ListOperation<string>(levels).MoveUp(levelsToMove);
		return this;
	}
	
	public SMSceneConfigurationOperation MoveLevelDown(IList<string> levelsToMove) {
		new ListOperation<string>(levels).MoveDown(levelsToMove);
		return this;
	}
	
	public SMSceneConfigurationOperation MoveLevelToBottom(IList<string> levelsToMove) {
		new ListOperation<string>(levels).MoveToBottom(levelsToMove);
		return this;
	}
	
	public SMSceneConfigurationOperation MoveLevelToPosition(IList<string> levelsToMove, int index) {
		new ListOperation<string>(levels).MoveTo(levelsToMove, index);
		return this;
	}
	
	public void Apply(SMSceneConfiguration configuration) {
		configuration.screens = screens.ToArray();
		configuration.levels = levels.ToArray();
		
		if (firstScreen != null && !screens.Contains(firstScreen)) {	
			firstScreen = null;
		}
		configuration.firstScreen = firstScreen;
		
		if (firstScreenAfterLevel != null && !screens.Contains(firstScreenAfterLevel)) {
			firstScreenAfterLevel = null;
		}
		configuration.firstScreenAfterLevel = firstScreenAfterLevel;
	}
	
}

