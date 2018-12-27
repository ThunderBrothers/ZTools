//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/SceneManager Demo/Intro")]
public class SMDemoIntro : MonoBehaviour
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
		
		GUILayout.Label ("Scene Manager Demo - Intro");
		GUILayout.FlexibleSpace ();
		
		
		if (GUILayout.Button ("GoTo Main Menu")) {
			SMGameEnvironment.Instance.SceneManager.LoadScreen("New Scene");
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndArea ();
	}
}
