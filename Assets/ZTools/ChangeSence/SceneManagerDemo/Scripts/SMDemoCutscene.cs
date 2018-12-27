//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using UnityEngine;
using System.Collections;

public class SMDemoCutscene : MonoBehaviour
{

	public float length = 5f;
	private string group;
	
	void Start ()
	{
		var prevLevel = SMGameEnvironment.Instance.SceneManager.LevelProgress.LastLevelId;
		group = SMGameEnvironment.Instance.SceneManager.GroupOfLevel (prevLevel);
	}
	
	void Update ()
	{
		if (length > 0) {
			length -= Time.deltaTime;
			if (length <= 0) {
				length = 0;
				SMGameEnvironment.Instance.SceneManager.LoadNextLevel ();
			}
		}
	}
	
	public void OnGUI ()
	{
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.Label ("Scene Manager Demo - Cutscene");
		GUILayout.Label ("You finished group '" + group + "' (continuing in " + length.ToString ("0.0") + " secs)");
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();
	}
}
