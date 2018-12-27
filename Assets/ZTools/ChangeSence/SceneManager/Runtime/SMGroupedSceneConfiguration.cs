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
/// The scene configuration as used by <see cref="SMSceneManager"/>
/// </summary>
public class SMGroupedSceneConfiguration : SMSceneConfigurationBase {	
	
	/// <summary>
	/// An array containing the groups of this configuration.
	/// </summary>
	public string[] groups = new string[0];
	
	public int[] groupOffset = new int[0];
		
	/// <summary>
	/// The action that should be performed after a group is finished (excluding the last group).
	/// </summary>
	public SMWorkflowActionType actionAfterGroup;
	
	public string firstScreenAfterGroup;
		
	public override bool IsValid (HashSet<string> validScenes) {
		bool result = base.IsValid(validScenes);
		
		if (actionAfterGroup == SMWorkflowActionType.LoadScreen) {
			if (String.IsNullOrEmpty(firstScreenAfterGroup)) {
				Debug.LogWarning("In configuration '" + name + "' you configured that you want to load a screen after a group has finished. " +
					"However you have not set up the screen that should actually be loaded after a group, so Scene Manager doesn't " +
					"know what to do in this case. To fix this, select the scene configuration in the project view. In the " +
					"inspector select a screen that you want to display after each group and press the 'After Group' button. " +
					"If you don't want to show a screen after each group choose 'Load Next Level' in the " +
					"'Action after group' dropdown in the inspector.", this);
				result = false;
			}
		}
		
		if (groups.Length == 0) {
			Debug.LogWarning("The scene configuration '" + name+"' has no groups set up. Please add at least one group by " +
				"selecting the scene configuration in the project view and then click the '+' button below the list of " +
				"groups in the inspector.", this);
			result = false;
		}
		
		if (levels.Length == 0) {
			Debug.LogWarning("The scene configuration '" + name + "' does not contain a level. To add a level select the scene " +
				"configuration in the project view. Then in the inspector view select a unity scene that should be a " +
				"level and press the 'Level' button.", this);	
			result = false;
		} 
		
		for(int i = 1; i < groupOffset.Length; i++) {
			if (groupOffset[i-1] == groupOffset[i]) {
				Debug.LogWarning("The scene configuration '" + name + "' contains a group with no levels in it. To add a level " +
					"to a group, first select the scene configuration in the project view. Now in the inspector view, choose " +
					"a level and drag it over the group into which you want to place it.", this);
				result = false;
				break;
			}
		}
		
		return result;
	}

}

