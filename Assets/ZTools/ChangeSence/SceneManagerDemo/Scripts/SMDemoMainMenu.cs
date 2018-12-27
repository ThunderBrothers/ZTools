//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/SceneManager Demo/MainMenu")]
public class SMDemoMainMenu : MonoBehaviour
{
	private SMILevelProgress levelProgress;
	private string[] transitionName = new string[] {"Plain", "Fade", "Blinds", "Newspaper", "Cartoon", "Ninja", "Cinema", "Pixelate", "Tetris", "Tiles"};
	private string[] transitionPrefab = new string[] {"Transitions/SMPlainTransition", "Transitions/SMFadeTransition", 
		"Transitions/SMBlindsTransition", "Transitions/SMNewspaperTransition", "Transitions/SMCartoonTransition", 
		"Transitions/SMNinjaTransition", "Transitions/SMCinemaTransition", "Transitions/SMPixelateTransition", 
		"Transitions/SMTetrisTransition", "Transitions/SMTilesTransition"};
	private int selectionIndex;
	
	
	// Use this for initialization
	void Start () 
    {
		selectionIndex = Array.IndexOf(transitionPrefab, SMGameEnvironment.Instance.SceneManager.TransitionPrefab);
		levelProgress = SMGameEnvironment.Instance.SceneManager.UnmodifiableLevelProgress;
	}

	void OnGUI ()
	{
		SMSceneManager sceneManager = SMGameEnvironment.Instance.SceneManager;
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		
		
		GUILayout.Label ("Scene Manager Demo - Main Menu");
		GUILayout.FlexibleSpace();
		if ( sceneManager.ConfigurationName.Contains("DemoGame") ) 
        {
			GUILayout.Label ("This is the demo version of the game with only 2 levels.");
		}
		else 
        {
			GUILayout.Label ("This is the full version of the game with 4 levels.");
		}
		
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button ("New Game")) 
        {
			sceneManager.LoadFirstLevel();	
		}
		
		if ( levelProgress.HasPlayed ) 
        {
			if (GUILayout.Button ("Continue Playing "+ levelProgress.LastLevelId )) 
            {
				sceneManager.LoadNextLevel();
			}
		}
		
		if (GUILayout.Button("Reset Progress"))
        {
			sceneManager.LevelProgress.ResetLastLevel();
			foreach(string levelId in sceneManager.Levels)
            {
				sceneManager.LevelProgress.SetLevelStatus(levelId, SMLevelStatus.New);
			}
			foreach(string groupId in sceneManager.Groups) 
            {
				sceneManager.LevelProgress.SetGroupStatus(groupId, SMGroupStatus.New);
			}
			levelProgress = sceneManager.UnmodifiableLevelProgress;
		}
		
		GUILayout.FlexibleSpace ();
		GUILayout.Label("Start an already played level...");
		
		if(GUILayout.Button("Select Level")) {
			sceneManager.LoadScreen("LevelSelectionMenu");
		}
	
		GUILayout.FlexibleSpace ();
		GUILayout.Label("Select transition for switching between levels");
		selectionIndex = GUILayout.SelectionGrid(selectionIndex, transitionName, 5);
		sceneManager.TransitionPrefab = transitionPrefab[selectionIndex];
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();		

		GUILayout.EndArea ();
	}
	
}
