//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/SceneManager Demo/Level")]
public class SMDemoLevel : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		
	}

	void OnGUI ()
	{
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ("Scene Manager Demo - Level: " + Application.loadedLevelName);
		GUILayout.FlexibleSpace();
		
		
		if (GUILayout.Button ("Next Level")) {
			SMGameEnvironment.Instance.SceneManager.LoadNextLevel();
		}
		if (GUILayout.Button ("Exit to Main Menu")) {
			SMGameEnvironment.Instance.SceneManager.LoadScreen("MainMenu");
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndArea ();
	}
}
