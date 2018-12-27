//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/SceneManager Demo/Level Selector")]
public class SMDemoLevelSelector : MonoBehaviour
{
	private SMILevelProgress levelProgress;
	private string activeGroup;
	private string[] activeGroupLevels;
	private SMSceneManager sceneManager;
	
	
	// Use this for initialization
	void Start ()
	{
		sceneManager = SMGameEnvironment.Instance.SceneManager;
		levelProgress = sceneManager.UnmodifiableLevelProgress;
		activeGroup = sceneManager.Groups [0];
		activeGroupLevels = sceneManager.LevelsInGroup(activeGroup);
	}

	void OnGUI ()
	{
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		
		
		GUILayout.Label ("Scene Manager Demo - Level Selector");
		GUILayout.FlexibleSpace ();
		if (sceneManager.ConfigurationName.Contains ("DemoGame")) {
			GUILayout.Label ("This is the demo version of the game with only 2 levels.");
		} else {
			GUILayout.Label ("This is the full version of the game with 4 levels.");
		}
				
		GUILayout.FlexibleSpace ();
		
		if (sceneManager.ConfigurationName.Contains ("Grouped")) {
			foreach (var group in sceneManager.Groups) {
				var groupStatus = levelProgress.GetGroupStatus (group);
				if (GUILayout.Button (group + " [" + groupStatus + "]")) {
					activeGroup = group;
					activeGroupLevels = sceneManager.LevelsInGroup(activeGroup);
				}
				if (activeGroup == group) {
					RenderLevels (activeGroupLevels);			
				}
			}
		} else {
			RenderLevels (sceneManager.Levels);
		}
		
		GUILayout.FlexibleSpace ();

		if (GUILayout.Button ("Reset Progress")) {
			sceneManager.LevelProgress.ResetLastLevel ();
			foreach (string levelId in sceneManager.Levels) {
				sceneManager.LevelProgress.SetLevelStatus (levelId, SMLevelStatus.New);
			}

			foreach(string groupId in sceneManager.Groups) {
				sceneManager.LevelProgress.SetGroupStatus(groupId, SMGroupStatus.New);
			}

			levelProgress = sceneManager.UnmodifiableLevelProgress;
		}

		
		if (GUILayout.Button ("Return to main menu")) {
			sceneManager.LoadScreen ("MainMenu");
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();		

		GUILayout.EndArea ();
	}

	private void RenderLevels (string[] levels)
	{
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		foreach (string levelId in levels) {
			GUI.enabled = levelProgress.GetLevelStatus (levelId) != SMLevelStatus.New; 
			if (GUILayout.Button (levelId)) {
				sceneManager.LoadLevel (levelId);
			}
			GUI.enabled = true;
		}
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

	}
	
}
