//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

/// <summary>
/// Base class for all scene configuration editors
/// </summary>
public abstract class SMSceneConfigurationEditorBase<T> : Editor where T:SMSceneConfigurationBase {
	
	protected string[] scenes;
	protected Dictionary<string, string> sceneLookup;
	protected string configurationGuid;
	
	private bool fixActiveState;
	protected string[] invalidScreens;
	protected string[] invalidLevels;
		
	/// <summary>
	/// Creates a new scene configuration and changes the selection to the new object
	/// </summary>
	/// <returns>
	/// The created configuration
	/// </returns>
	/// <typeparam name='C'>
	/// The type of configuration we want to create
	/// </typeparam>
	protected static C CreateConfiguration<C>() where C:SMSceneConfigurationBase {
		var name = typeof(C).Name;
		
		var path = "Assets";
		foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);

            }
            break;
        }		
		
		
		path = AssetDatabase.GenerateUniqueAssetPath (path + "/" + name + ".asset");
		C configuration = ScriptableObject.CreateInstance<C> ();
		AssetDatabase.CreateAsset (configuration, path);
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow ();
        Selection.activeObject = configuration;
		
		if (SMSceneConfigurationUtil.FindConfigurations().Count == 1) {
			configuration.activeConfiguration = true;
			EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
		}

		return configuration;
	}	
	
	public virtual void OnEnable() {
		sceneLookup = SMSceneConfigurationUtil.CreateSceneLookup();
		List<string> sceneNames = new List<string>(sceneLookup.Keys);
		sceneNames.Sort();
		scenes = sceneNames.ToArray();
		CheckConfiguration();		
	}
	
	/// <summary>
	/// The scene configuration that is currently being edited.
	/// </summary>
	protected T Target {
		get {
			return (T)target;
		}
	}		
	
	protected void CheckConfiguration() {
		invalidScreens = Array.FindAll(Target.screens, screen => !sceneLookup.ContainsKey(screen));
		invalidLevels = Array.FindAll(Target.levels, level => !sceneLookup.ContainsKey(level));
		if (Target.activeConfiguration) {
			fixActiveState = SMSceneConfigurationUtil.FindConfigurations().Exists(configuration => configuration.activeConfiguration && configuration != Target);
		}
		UpdateGuid();
	}
	
	protected bool Invalid {
		get {
			return (invalidScreens != null && invalidScreens.Length > 0) || (invalidLevels != null && invalidLevels.Length > 0) || fixActiveState;
		}
	}
					
	private void UpdateGuid() {
		string guid = Guid.NewGuid().ToString();
		Target._guid = guid;
		configurationGuid = guid;
	}
		
	

	protected bool IsScreen(string scene) {
		return Array.IndexOf(Target.screens, scene) > -1;
	}
	
	protected bool IsLevel(string scene) {
		return Array.IndexOf(Target.levels, scene) > -1;
	}
	
	/// <summary>
	/// Fixes configuration issues such as multiple active configurations or removed scenes
	/// </summary>
	protected void FixConfiguration() {
		List<UnityEngine.Object> objects = new List<UnityEngine.Object>(SMSceneConfigurationUtil.FindConfigurations().ToArray());
		CUUndoUtility.RegisterUndo(objects.ToArray(), "Fixing configuration");
		UpdateGuid();
		
		FixInvalidScenes();
		
		if (fixActiveState) {
			SMSceneConfigurationUtil.EnsureActiveConfiguration(Target, false);
		}
		
		invalidScreens = null;
		invalidLevels = null;		
		fixActiveState = false;
		EditorUtility.SetDirty(Target);
	}
	
	protected abstract void FixInvalidScenes();
	
	
		

	
	/// <summary>
	/// Syncs the settings back to the scene configuration when the scene configuration is modified.
	/// </summary>
	protected void SyncBuildSettingsIfRequired() {
		if (Target.activeConfiguration) {
			SMSceneConfigurationUtil.SyncWithBuildSettings(Target,  sceneLookup);
		}
	}
	
	/// <summary>
	///  Called before anything is changed on the scene configuration. This will register an undo state
	///  for the scene configuration.
	/// </summary>
	/// <param name="operation">the performed operation. this is placed in the undo menu.</param>
	protected void BeforeChange(string operation) {
		CUUndoUtility.RegisterUndo(Target, operation);
	}
	
	[MenuItem("Assets/Verify Scene Configurations")]
	public static void VerifyConfigurations() {
		bool successful = true;
		List<SMSceneConfigurationBase> configurations = SMSceneConfigurationUtil.FindConfigurations();
		Dictionary<string, string> lookup = SMSceneConfigurationUtil.CreateSceneLookup();
			
		HashSet<string> validScenes = new HashSet<string>();
		foreach(string scene in lookup.Keys) {
			validScenes.Add(scene);
		}
				 
		SMSceneConfigurationBase activeConfiguration = null;
		int activeConfigurations = 0;
		foreach(SMSceneConfigurationBase configuration in configurations) {
			successful &= configuration.IsValid(validScenes);
			
			if (configuration.activeConfiguration) {
				activeConfigurations++;
				activeConfiguration = configuration;
			}
		}
		
		if (activeConfigurations == 0) {
			Debug.LogWarning("Currently no scene configuration is active. This will lead to issues when your game is " +
				"started as Scene Manager doesn't know which scene configuration it should load. Please activate one " +
				"of your scene configurations. To activate a scene configuration, select it in the project view " +
				"and then click on the 'Activate' button in the inspector.");
			successful = false;
		}  else if (activeConfigurations > 1) {
			Debug.LogWarning("Currently more than one scene configuration is active. This will lead to issues when your game is " +
				"started as Scene Manager doesn't know which scene configuration it should load. Please select the configuration you " +
				"want to keep active in the project view and then press the 'Fix Configuration' button in the inspector. This will deactivate all other " +
				"scene configurations. To activate another scene configuration, select it in the project view " +
				"and then click on the 'Activate' button in the inspector.");
			successful = false;
		}  else {
			SMSceneConfigurationUtil.SyncWithBuildSettings(activeConfiguration, lookup);	
		}

		if (successful) {
			Debug.Log("All your scene configurations are valid.");
		}
	}
	
}

