//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using UnityEngine;
using System.Collections;

/// <summary>
/// Loads the current level name into the given text mesh.
/// </summary>
public class SMDemoCurrentLevel : MonoBehaviour {
	
	public TextMesh textMesh;
	
	void Start () {
		if ( textMesh != null ) {
			textMesh.text = Application.loadedLevelName;
		}
	}
	
}
