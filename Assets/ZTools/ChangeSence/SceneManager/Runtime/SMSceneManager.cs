//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is the main interaction point with the SceneManager API.
/// </summary> 
public class SMSceneManager 
{
	
	private SMILevelProgress levelProgress;
	private SMIConfigurationAdapter configurationAdapter;
    //各种不同的切换方式
    //SMBlindsTransition    百叶窗过渡
    //SMCartoonTransition   卡通过渡
    //SMCinemaTransition    电影转型
    //SMFadeTransition      衰落过渡
    //SMNewspaperTransition 报业转型
    //SMNinjaTransition     忍耐状态
    //SMPixelateTransition  像素化
    //SMPlainTransition     平原
    //SMTetrisTransition    俄罗斯方块
    //SMTilesTransition     瓷砖
    string defaultTransitionPrefab = "Transitions/SMTilesTransition";
	
	/// <summary>
	/// The level progress tracker that this instance of Scene Manager is currently using.
	/// </summary>
	public SMILevelProgress LevelProgress {
		get {
			return levelProgress;
		}
		set {
			levelProgress = value;
			
			if (FirstLevel != null && levelProgress.GetLevelStatus(FirstLevel) == SMLevelStatus.New) {
				levelProgress.SetLevelStatus(FirstLevel, SMLevelStatus.Open);
			}
			
			if (FirstGroup != null && levelProgress.GetGroupStatus(FirstGroup) == SMGroupStatus.New) {
				levelProgress.SetGroupStatus(FirstGroup, SMGroupStatus.Open);
			}
		}
	}
	
	
	/// <summary>
	/// Returns an unmodifiable version of the level progress tracker, that doesn't change it's state
	/// anymore. Useful for drawing GUI. The result of this method should be saved to a variable
	/// as constructing this object is rather expensive.
	/// </summary>
	public SMILevelProgress UnmodifiableLevelProgress {
		get {
			return new SMUnmodifiableLevelProgress(LevelProgress, configurationAdapter);
		}
	}
	
	/// <summary>
	/// The name of the scene configuration that this instance Scene Manager is currently using.
	/// </summary>
	public string ConfigurationName {
		get {
			return configurationAdapter.Name;
		}
	}
	
	/// <summary>
	/// Gets or sets the path of the default transition prefab.
	/// </summary>
	/// <value>
	/// path to the prefab.
	/// </value>
	/// <remarks>@since version 1.2.0</remarks>
	public string TransitionPrefab {
		get {
			return defaultTransitionPrefab;
		}
		set {
			if (string.IsNullOrEmpty(value)) {
				Debug.LogWarning("Transition prefab must not be null or empty. Keeping previous transition.");
			} else {
				defaultTransitionPrefab = value;
			}
		}
	}
	
	
	/// <summary>
	/// Ctor.
	/// </summary>
	/// <param name="sceneConfiguration">
	/// A <see cref="SMSceneConfiguration"/>, that Scene Manager should use.
	/// </param>
	/// <remarks>@deprecated Replaced by Ctor with SMIConfigurationAdapter with Scene Manager 1.4.0</remarks>
	[ObsoleteAttribute("Replaced by Ctor with SMIConfigurationAdapter with Scene Manager 1.4.0")]
	public SMSceneManager(SMSceneConfiguration sceneConfiguration) : this(new SMSceneConfigurationAdapter(sceneConfiguration)) {
	}
	
	/// <summary>
	/// Ctor.
	/// </summary>
	/// <param name="configurationAdapter">
	/// A <see cref="SMIConfigurationAdapter"/>, that Scene Manager should use.
	/// </param>
	/// <remarks>@since version 1.4.0</remarks>
	public SMSceneManager(SMIConfigurationAdapter configurationAdapter) {
		if (configurationAdapter == null) {
			throw new ArgumentException("Scene Manager has been given no scene configuration to work on. Cannot continue.");
		}
		this.configurationAdapter = configurationAdapter;
	}
	
	/// <summary>
	/// Convience property which returns the first level of the scene configuration used by this instance of
	/// Scene Manager.
	/// </summary>
	public string FirstLevel {
		get {
			return configurationAdapter.Levels.Length > 0 ? configurationAdapter.Levels[0] : null;
		}
	}
	
	/// <summary>
	/// Convience getter, which returns all level groups of the scene configuration used by this instance of Scene Manager.
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	public string[] Groups {
		get {
			return configurationAdapter.Groups;
		}
	}
	
	/// <summary>
	/// Convience property which returns the first group of the scene configuration used by this instance of
	/// Scene Manager.
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	public string FirstGroup {
		get {
			return configurationAdapter.Groups.Length > 0 ? configurationAdapter.Groups[0] : null;
		}
	}
	
	/// <summary>
	/// Convience getter, which returns all levels of the scene configuration used by this instance of Scene Manager.
	/// </summary>
	public string[] Levels {
		get {
			return configurationAdapter.Levels;
		}
	}
	
	/// <summary>
	/// Convience getter, which returns all levels in a certain group of the scene configuration used by this instance of Scene Manager.
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	public string[] LevelsInGroup(string groupId) {
		if (!configurationAdapter.GroupExists(groupId)) {
			throw new ArgumentException("Group " + groupId + " does not exist.");
		}
		return configurationAdapter.GetLevelsInGroup(groupId);
	}
	
	/// <summary>
	/// Convience getter, which returns the level group in which the given level is.
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	public string GroupOfLevel(string levelId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level " + levelId + " does not exist.");
		}
		return configurationAdapter.GetGroupOfLevel(levelId);
	}

	/// <summary>
	/// Returns the action that should be performed, after a group is finished.
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	public SMWorkflowActionType ActionAfterGroup {
		get {
			return configurationAdapter.ActionAfterGroup;
		}		
	}
	
	/// <summary>
	/// Convenience getter, which returns the first screen of scene configuration used by this instance of Scene Manager
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	public string FirstScreenAfterGroup {
		get {
			return configurationAdapter.FirstScreenAfterGroup;
		}
	}

	/// <summary>
	/// Convenience getter, which returns the first screen of scene configuration used by this instance of Scene Manager
	/// </summary>
	/// <remarks>@since version 1.2.0</remarks>
	public string FirstScreen {
		get {
			return configurationAdapter.FirstScreen;
		}
	}		 

	/// <summary>
	/// Convenience getter, which returns the first screen after the last level of scene configuration used by 
	/// this instance of Scene Manager
	/// </summary>
	/// <remarks>@since version 1.2.0</remarks>
	public string FirstScreenAfterLastLevel {
		get {
			return configurationAdapter.FirstScreenAfterLastLevel;
		}
	}		 
			 			 			 
	/// <summary>
	/// Convenience function, that loads the last played level. The ID of last played level is being retrieved from the
	/// level progress tracker that this instance of Scene Manager is currently using.
	/// </summary>
	/// <seealso cref="SMSceneManager.LevelProgress"/>
	/// <remarks>@deprecated use LoadNextLevel instead. LoadNextLevel works within levels AND screens since Scene Manager 1.4.0.</remarks>
	[Obsolete("Use LoadNextLevel instead. LoadNextLevel works within levels AND screens since Scene Manager 1.4.0.")]
	public void LoadLastPlayedLevel() {
		LoadLastPlayedLevel(defaultTransitionPrefab);
	}
	
    /// <summary>
	/// Convenience function, that loads the last played level. Uses the given transition to do the transition to 
	/// the level The ID of last played level is being retrieved from the level progress tracker that this 
	/// instance of Scene Manager is currently using.
	/// </summary>
	/// <seealso cref="SMSceneManager.LevelProgress"/>
	/// <param name="transitionPrefab">
	/// the path of the transition to use.
	/// </param> 
	/// <remarks>@deprecated use LoadNextLevel instead. LoadNextLevel works within levels AND screens since Scene Manager 1.4.0.</remarks>
	[Obsolete("Use LoadNextLevel instead. LoadNextLevel works within levels AND screens since Scene Manager 1.4.0.")]
	public void LoadLastPlayedLevel(string transitionPrefab) {
		LoadLevel(LevelProgress.LastLevelId, transitionPrefab);
	}

	/// <summary>
	/// Convience function, that loads the first level as defined by the scene configuration that this
	/// instance of Scene Manager is using.
	/// </summary>
	/// <seealso cref="SMSceneManager.FirstLevel"/>
	public void LoadFirstLevel() {
		LoadFirstLevel(defaultTransitionPrefab);
	}
	
    /// <summary>
	/// Convience function, that loads the first level as defined by the scene configuration that this
	/// instance of Scene Manager is using. Uses the given transition to do the transition to the level.
	/// </summary>
	/// <seealso cref="SMSceneManager.FirstLevel"/>
	/// <param name="transitionPrefab">
	/// The path of the transition to use.
	/// </param>
	public void LoadFirstLevel(string transitionPrefab) {
		LoadLevel(FirstLevel, transitionPrefab);
	}

	/// <summary>
	/// Loads a level and updates the level progress tracker. Do not use this function load a screen. Use
	/// <see cref="SMSceneManager.LoadScreen" instead.
	/// </summary>
	/// <param name="level">
	/// A <see cref="SMLevelDescriptor"/> of the level.
	/// </param>
	/// <seealso cref="SMSceneMananger[]"/>
	public void LoadLevel(string levelId) {
		LoadLevel(levelId, defaultTransitionPrefab);
	}
	
    /// <summary>
	/// Loads a level and updates the level progress tracker. Uses the given transition to do the transition between
	/// the levels. Do not use this function load a screen. Use <see cref="SMSceneManager.LoadScreen" instead.
	/// </summary>
	/// <param name="level">
	/// A <see cref="SMLevelDescriptor"/> of the level.
	/// </param>
	/// <param name="transitionPrefab">
	/// The path of the transition to use.
	/// </param>
	/// <seealso cref="SMSceneMananger[]"/>
	public void LoadLevel(string levelId, string transitionPrefab)
    {
		if (  !configurationAdapter.LevelExists(levelId) )
        {
			throw new ArgumentException("There is no level with the id '"+levelId+"' in the scene configuration");
		}
		if (levelProgress.GetLevelStatus(levelId) != SMLevelStatus.Done)
        {
			levelProgress.SetLevelStatus(levelId, SMLevelStatus.Visited);
		}
		
		string groupId = GroupOfLevel(levelId);
		if (levelProgress.GetGroupStatus(groupId) != SMGroupStatus.Done)
        {
			levelProgress.SetGroupStatus(groupId, SMGroupStatus.Visited);
		}

		levelProgress.LastLevelId = levelId;
		levelProgress.CurrentLevelId = levelId;
		LoadScreen(levelId, transitionPrefab);			
	}

	/// <summary>
	/// Loads the next level as defined by the scene configuration that this instance of Scene Manager is currently
	/// using. If the current level is the last level of the game, this method will load the screen that is configured
	/// to be the screen after the last level. This method assumes, that the currently loaded scene is actually a level.
	/// If you want to load the last played level from the main menu use <see cref="SMSceneManager.LoadLastPlayedLevel"/>
	/// </summary>
	public void LoadNextLevel() 
    {
		LoadNextLevel(defaultTransitionPrefab);
	}
	
    /// <summary>
	/// Loads the next level as defined by the scene configuration that this instance of Scene Manager is currently
	/// using.  Uses the given transition to do the transition between the levels. 
	/// If the current level is the last level of the game, this method will load the screen that is configured
	/// to be the screen after the last level. This method assumes, that the currently loaded scene is actually a level.
	/// If you want to load the last played level from the main menu use <see cref="SMSceneManager.LoadLastPlayedLevel"/>
	/// </summary>
	/// <param name="transitionPrefab">
	/// The path of the transition to use.
	/// </param>
	public void LoadNextLevel(string transitionPrefab) 
    {
        string currentLevel = Application.loadedLevelName;


        if (!configurationAdapter.LevelExists(currentLevel)) {
			// current scene is not a level, but a screen.
			// we try to continue after a cutscene.
			if ( levelProgress.CurrentLevelId == null) {
				LoadFirstLevel(transitionPrefab);
			}
			else {
				LoadLevel(levelProgress.CurrentLevelId, transitionPrefab);
			}
		}
		else {
			LoadNextLevelAfter(currentLevel, transitionPrefab);
		}
	}
	
	
	/// <summary>
	/// Loads the next level after the given level. If the given level is the last level this will load the 
	/// screen that is configured to be the screen after the last level. This will use the default transition prefab.
	/// </summary>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.2.0</remarks>
	public void LoadNextLevelAfter(string levelId)
    {
		LoadNextLevelAfter(levelId, defaultTransitionPrefab);
	}
		
	
	/// <summary>
	/// Loads the next level after the given level. If the given level is the last level this will load the 
	/// screen that is configured to be the screen after the last level. Uses the given transition to do the 
	/// transition between the levels. 
	/// </summary>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <param name='transitionPrefab'>
	///  The path to the transition prefab to use.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.2.0</remarks>
	public void LoadNextLevelAfter(string levelId, string transitionPrefab)
    {
		string nextLevel = GetNextLevelAfter (levelId);
		levelProgress.SetLevelStatus(levelId, SMLevelStatus.Done);	
		
		var thisGroup = GroupOfLevel(levelId);
		
		if (nextLevel == null) {
			// this was the last level
			levelProgress.ResetLastLevel();
			levelProgress.SetGroupStatus(thisGroup, SMGroupStatus.Done);
			LoadScreen (configurationAdapter.FirstScreenAfterLastLevel, transitionPrefab);
		} else {
			var nextGroup = GroupOfLevel(nextLevel);
			if(thisGroup != nextGroup) {
				// group is finished.
				// set group status to done
				levelProgress.SetGroupStatus(thisGroup, SMGroupStatus.Done);
				// show intermediate screen if wanted.
				if (ActionAfterGroup == SMWorkflowActionType.LoadScreen) {
					// set level and group status to "Open" implying that the level and group can
					// potentially be visited after the intermediate screen.
					// only set to open when it's new, otherwise it keeps it's status
					if (levelProgress.GetLevelStatus(nextLevel) == SMLevelStatus.New) {
						levelProgress.SetLevelStatus(nextLevel, SMLevelStatus.Open);
					}
					if (levelProgress.GetGroupStatus(nextGroup) == SMGroupStatus.New) {
						levelProgress.SetGroupStatus(nextGroup, SMGroupStatus.Open);
					}
					levelProgress.CurrentLevelId = nextLevel;
					LoadScreen(FirstScreenAfterGroup, transitionPrefab);
					return;
				}
			}
			// otherwise simply load the next level
			LoadLevel(nextLevel, transitionPrefab);
		}
	}


	/// <summary>
	/// Sets the status of the given level to done and the status of the next level to open if necessary. 
	/// </summary>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.9</remarks>
	public void SetLevelAsFinished(String levelId)
    {
		string nextLevel = GetNextLevelAfter (levelId);
		levelProgress.SetLevelStatus(levelId, SMLevelStatus.Done);	
		
		var thisGroup = GroupOfLevel(levelId);
		
		if (nextLevel == null)
        {
			// this was the last level
			levelProgress.ResetLastLevel();
			levelProgress.SetGroupStatus(thisGroup, SMGroupStatus.Done);
		}
        else
        {
			var nextGroup = GroupOfLevel(nextLevel);
			if(thisGroup != nextGroup) {
				// group is finished.
				// set group status to done
				levelProgress.SetGroupStatus(thisGroup, SMGroupStatus.Done);
				// set group status to "Open" implying that the group can
				// potentially be visited after the intermediate screen.
				// only set to open when it's new, otherwise it keeps it's status
				if (levelProgress.GetGroupStatus(nextGroup) == SMGroupStatus.New) {
					levelProgress.SetGroupStatus(nextGroup, SMGroupStatus.Open);
				}
			}
			// set level status to "Open" implying that the level can
			// potentially be visited after the intermediate screen.
			// only set to open when it's new, otherwise it keeps it's status
			if (levelProgress.GetLevelStatus(nextLevel) == SMLevelStatus.New) {
				levelProgress.SetLevelStatus(nextLevel, SMLevelStatus.Open);
			}
			levelProgress.CurrentLevelId = nextLevel;
			levelProgress.LastLevelId = nextLevel;
		}
	}


	/// <summary>
	/// Gets the next level that is configured after the given level, even if it's in the following group. 
	/// If the given level is the last level will return null. 
	/// </summary>
	/// <returns>
	/// The next level after the given level.
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.2.0</remarks>
	public string GetNextLevelAfter(string levelId) 
    {
		int index = Array.IndexOf (configurationAdapter.Levels, levelId);
		if ( index == -1 ) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}
		
		if (index == configurationAdapter.Levels.Length - 1) {
			return null;
		}
		
		return configurationAdapter.Levels[index+1];
	}

	/// <summary>
	/// Loads the previous level as defined by the scene configuration that this instance of Scene Manager is currently
	/// using. If the current level is the first level of the game, this method will do nothing. This method assumes, 
	/// that the currently loaded scene is actually a level.
	/// </summary>
	/// <remarks>@since version 1.4.7</remarks>
	public void LoadPreviousLevel() {
		LoadPreviousLevel(defaultTransitionPrefab);
	}
	
	/// <summary>
	/// Loads the previous level as defined by the scene configuration that this instance of Scene Manager is currently
	/// using. If the current level is the first level of the game, this method will do nothing. This method assumes, 
	/// that the currently loaded scene is actually a level.
	/// </summary>
	/// <param name="transitionPrefab">
	/// The path of the transition to use.
	/// </param>
	/// <remarks>@since version 1.4.7</remarks>
	public void LoadPreviousLevel(string transitionPrefab) {
		string currentLevel = Application.loadedLevelName;
		
		LoadPreviousLevelBefore(currentLevel, transitionPrefab);
	}
	
	
	/// <summary>
	/// Loads the previous level before the given level. If the given level is the first level this will do nothing. 
	/// This will use the default transition prefab.
	/// </summary>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public void LoadPreviousLevelBefore(string levelId) {
		LoadPreviousLevelBefore(levelId, defaultTransitionPrefab);
	}
	
	
	
	/// <summary>
	/// Loads the previous level before the given level. If the given level is the first level this will do nothing. 
	/// Uses the given transition to do the transition between the levels. 
	/// </summary>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <param name='transitionPrefab'>
	///  The path to the transition prefab to use.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public void LoadPreviousLevelBefore(string levelId, string transitionPrefab) {
		string previousLevel = GetPreviousLevelBefore (levelId);

		if (previousLevel != null) {
			LoadLevel(previousLevel, transitionPrefab);
		}
	}

	/// <summary>
	/// Gets the prevoius level that is configured before the given level, even if it's in the preceding group. 
	/// If the given level is the first level will return null. 
	/// </summary>
	/// <returns>
	/// The prevoius level before the given level.
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public string GetPreviousLevelBefore(string levelId) {
		int index = Array.IndexOf (configurationAdapter.Levels, levelId);
		if ( index == -1 ) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}
		
		if (index == 0) {
			return null;
		}
		
		return configurationAdapter.Levels[index-1];
	}
	
	/// <summary>
	/// Loads the screen with the given screen id. 
	/// </summary>
	/// <param name="screenId">
	/// the id of the screen to be loaded.
	/// </param>
	public void LoadScreen(string screenId) {
		LoadScreen(screenId, defaultTransitionPrefab);
	}
	
    /// <summary>
	/// Loads the screen with the given screen id. ses the given transition to do the transition between the screens. 
	/// </summary>
	/// <param name="screenId">
	/// the id of the screen to be loaded.
	/// </param>
	/// <param name="transitionPrefab">
	/// The path of the transition to use.
	/// </param>
	public void LoadScreen(string screenId, string transitionPrefab) 
    {
		if (string.IsNullOrEmpty(transitionPrefab)) 
        {
			Debug.LogWarning("given transition prefab must not be null or empty.");
			transitionPrefab = defaultTransitionPrefab;
		}
		
		GameObject prefab = (GameObject) Resources.Load(transitionPrefab);
		if (prefab == null)
        {
			throw new ArgumentException("no transition prefab found at path " + transitionPrefab);
		}
		
		GameObject instance = (GameObject)GameObject.Instantiate(prefab);
		SMTransition transition = instance.GetComponent<SMTransition>();
		if (transition == null)
        {
			throw new ArgumentException("no transition found at prefab " + transitionPrefab);
		}
		
		transition.screenId = screenId;
	}

	/// <summary>
	/// Returns whether or not the given level id is the first level in the scene configuration
	/// </summary>
	/// <returns>
	/// True if this is the first level, false otherwise
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsFirstLevel(string levelId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}

		// level is valid so we have at least one level in the list
		return configurationAdapter.Levels[0] == levelId;
	}

	/// <summary>
	/// Returns whether or not the given level id is the last level in the scene configuration
	/// </summary>
	/// <returns>
	/// True if this is the last level, false otherwise
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsLastLevel(string levelId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}
		
		// level is valid so we have at least one level in the list
		return configurationAdapter.Levels[configurationAdapter.Levels.Length - 1] == levelId;
	}

	/// <summary>
	/// Returns whether or not the given level id is the first level in its group
	/// </summary>
	/// <returns>
	/// True if this is the first level in its group, false otherwise
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsFirstLevelInGroup(string levelId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}

		string group = configurationAdapter.GetGroupOfLevel(levelId);
		string[] levelsInGroup = configurationAdapter.GetLevelsInGroup(group);
		return levelsInGroup[0] == levelId;
	}

	/// <summary>
	/// Returns whether or not the given level id is the first level in the given group
	/// </summary>
	/// <returns>
	/// True if this is the first level in the given group, false otherwise
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <param name='groupId'>
	/// Group identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level or group name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsFirstLevelInGroup(string levelId, string groupId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}
		if (!configurationAdapter.GroupExists(groupId)) {
			throw new ArgumentException("Group id " + levelId + " is not known in this configuration.");
		}

		string[] levelsInGroup = configurationAdapter.GetLevelsInGroup(groupId);
		return levelsInGroup[0] == levelId;
	}
	
	/// <summary>
	/// Returns whether or not the given level id is the last level in its group
	/// </summary>
	/// <returns>
	/// True if this is the last level in its group, false otherwise
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsLastLevelInGroup(string levelId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}

		string group = configurationAdapter.GetGroupOfLevel(levelId);
		string[] levelsInGroup = configurationAdapter.GetLevelsInGroup(group);
		return levelsInGroup[levelsInGroup.Length - 1] == levelId;
	}

	/// <summary>
	/// Returns whether or not the given level id is the last level in the given group
	/// </summary>
	/// <returns>
	/// True if this is the last level in the given group, false otherwise
	/// </returns>
	/// <param name='levelId'>
	/// Level identifier.
	/// </param>
	/// <param name='groupId'>
	/// Group identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given level or group name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsLastLevelInGroup(string levelId, string groupId) {
		if (!configurationAdapter.LevelExists(levelId)) {
			throw new ArgumentException("Level id " + levelId + " is not known in this configuration.");
		}
		if (!configurationAdapter.GroupExists(groupId)) {
			throw new ArgumentException("Group id " + levelId + " is not known in this configuration.");
		}

		string[] levelsInGroup = configurationAdapter.GetLevelsInGroup(groupId);
		return levelsInGroup[levelsInGroup.Length - 1] == levelId;
	}
	
	/// <summary>
	/// Returns whether or not the given group id is the first group in the scene configuration
	/// </summary>
	/// <returns>
	/// True if this is the first group, false otherwise
	/// </returns>
	/// <param name='groupId'>
	/// Group identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given group name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsFirstGroup(string groupId) {
		if (!configurationAdapter.GroupExists(groupId)) {
			throw new ArgumentException("Group id " + groupId + " is not known in this configuration.");
		}
		
		// group is valid so we have at least one group in the list
		return configurationAdapter.Groups[0] == groupId;
	}

	/// <summary>
	/// Returns whether or not the given group id is the last group in the scene configuration
	/// </summary>
	/// <returns>
	/// True if this is the last group, false otherwise
	/// </returns>
	/// <param name='groupId'>
	/// Group identifier.
	/// </param>
	/// <exception cref="System.ArgumentException">if the given group name is unknown.</exception>	
	/// <remarks>@since version 1.4.7</remarks>
	public bool IsLastGroup(string groupId) {
		if (!configurationAdapter.GroupExists(groupId)) {
			throw new ArgumentException("Group id " + groupId + " is not known in this configuration.");
		}
		
		// group is valid so we have at least one group in the list
		return configurationAdapter.Groups[configurationAdapter.Groups.Length - 1] == groupId;
	}
}
