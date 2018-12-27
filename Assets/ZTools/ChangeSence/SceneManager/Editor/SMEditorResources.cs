// (C) 2013 Ancient Light Studios. All rights reserved.
using System;
using UnityEngine;

public class SMEditorResources
{
	private static Texture _SMLevelMarker;
	private static Texture _SMScreenMarker;
	
	private static CUEditorAssetUtility assetUtility;
	
	public static Texture SMLevelMarker {
		get {
			if (_SMLevelMarker == null) {
				_SMLevelMarker = LoadTexture("SMLevelMarker.png");
			}
			return _SMLevelMarker;
		}
	}
	
	public static Texture SMScreenMarker {
		get {
			if (_SMScreenMarker == null) {
				_SMScreenMarker = LoadTexture("SMScreenMarker.png");
			}
			return _SMScreenMarker;
		}
	}
	
	
	public static Texture2D LoadTexture(string name) { 
		if (assetUtility == null) {
			assetUtility = new CUEditorAssetUtility(SMEditorResourcesLocator.ResourcePath, "SceneManager", "SMEditorResourcesLocator");		
		}
		return assetUtility.FindTexture(name);
    }
}

