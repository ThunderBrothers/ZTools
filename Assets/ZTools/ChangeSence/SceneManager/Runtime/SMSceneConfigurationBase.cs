//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SMSceneConfigurationBase : ScriptableObject
{
	
	/// <summary>
	/// Flag marking, if this configuration is the currently active configuration.
	/// </summary>
	public bool activeConfiguration = false;
	
	/// <summary>
	/// An array containing the levels of this configuration.
	/// </summary>
	public string[] levels = new string[0];
	
	/// <summary>
	/// An array containing the screens of this configuration.
	/// </summary>
	public string[] screens = new string[0];
	
	/// <summary>
	/// the id of the first screen of this configuration.
	/// </summary>
	public string firstScreen;
	
	/// <summary>
	/// the id of the screen to be loaded after the last level of this configuration.
	/// </summary>
	public string firstScreenAfterLevel;
	public string _guid;
	
	public virtual bool IsValid (HashSet<string> validScenes)
	{
		bool result = true;
		if (String.IsNullOrEmpty (firstScreen)) {
			Debug.LogWarning ("The scene configuration '" + name + "' has no first screen. The first screen determines " +
				"which unity scene is shown when the game launches. To set a first screen, open this scene configuration, " +
				"then select a scene that should be the first screen and press the 'First Screen' button.", this);
			result = false;
		}
		if (String.IsNullOrEmpty (firstScreenAfterLevel)) {
			Debug.LogWarning ("The scene configuration '" + name + "' has not set up which scene should be loaded after the " +
				"last level of the game has been played. To set this up, open this scene configuration, then select a scene " +
				"that should be played after the last level and click the 'After last Level' button.", this);
			result = false;
		}
		
		foreach (string screen in screens) {
			if (!validScenes.Contains (screen)) {
				Debug.LogWarning ("The scene configuration '" + name + "' references a unity scene named '" + screen + "', hoever this " +
				 	"scene seems to have disappeared from the project. Please open this scene configuration and check if the screen " +
				 	 "setup is still according to your liking.", this);
				result = false;
			}
		}
		foreach (string level in levels) {
			if (!validScenes.Contains (level)) {
				Debug.LogWarning ("The scene configuration '" + name + "' references a unity scene named '" + level + "', hoever this " +
				 	"scene seems to have disappeared from the project. Please open this scene configuration and check if the level " +
				 	 "setup is still according to your liking.", this);
				result = false;
			}
		}			
		return result;
	}
	
}



