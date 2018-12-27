//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Utility class for working with scene configurations.
/// </summary>
public class SMSceneConfigurationUtil
{
	/// <summary>
	/// Creates a lookup for unity scenes
	/// </summary>
	/// <returns>
	/// The scene lookup with the scene name as key and its path as value
	/// </returns>
	public static Dictionary<string, string> CreateSceneLookup() {
		var unityScenes = FindUnityScenes();
		Dictionary<string, string> lookup = new Dictionary<string, string>(unityScenes.Length);
		string rootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;

		foreach(FileInfo file in unityScenes) {
			string relativeName = file.FullName.Substring(rootPath.Length + 1);
			relativeName = relativeName.Replace("\\", "/"); // always to forward slashes when synching build settings
			lookup[Path.GetFileNameWithoutExtension(file.Name)] = relativeName;
		}
		return lookup;
	}
	
	/// <summary>
	/// Ensures that the given configuration is activated. If it is already the active configuration this method
	/// will do nothing. Otherwise it will activate the given configuration and deactivate any other active
	/// configuration.
	/// </summary>
	/// <param name="configurationToBeActivated">
	/// A <see cref="SMSceneConfigurationBase"/> that is to be activated.
	/// </param>
	public static void EnsureActiveConfiguration(SMSceneConfigurationBase configurationToBeActivated, bool registerUndo) {
		List<SMSceneConfigurationBase> allConfigurations = FindConfigurations();
		if (registerUndo) {
			CUUndoUtility.RegisterUndo(allConfigurations.ToArray(), "Activate scene configuration");
		}

		foreach(SMSceneConfigurationBase configuration in allConfigurations ) {
			configuration.activeConfiguration = configuration == configurationToBeActivated;
			EditorUtility.SetDirty(configuration);
		}
	}
	
	/// <summary>
	/// Syncronizes the build settings so that they resemble the list of scenes which are in the currently
	/// active scene configuration.
	/// </summary>	
	public static void SyncWithBuildSettings(SMSceneConfigurationBase configuration, Dictionary<string, string> lookup) {
		List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();
		
		if (!String.IsNullOrEmpty(configuration.firstScreen)) {
			if (lookup.ContainsKey(configuration.firstScreen)) {
				newScenes.Add(new EditorBuildSettingsScene(lookup[configuration.firstScreen], true));
			}
		}
		
		foreach(string screen in configuration.screens) {
			if (screen != configuration.firstScreen && lookup.ContainsKey(screen)) {
				newScenes.Add(new EditorBuildSettingsScene(lookup[screen], true));
			}
		}

		foreach(string level in configuration.levels) {
			if (level != configuration.firstScreen && lookup.ContainsKey(level)) {
				newScenes.Add(new EditorBuildSettingsScene(lookup[level], true));
			}
		}
		
		EditorBuildSettings.scenes = newScenes.ToArray();
	}	
	
	/// <summary>
	/// Returns all scene configuration in the current project
	/// </summary>
	public static List<SMSceneConfigurationBase> FindConfigurations() {
		DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
		string directoryPath = directory.Parent.FullName;

		FileInfo[] files = directory.GetFiles("*.asset", SearchOption.AllDirectories);
		List<SMSceneConfigurationBase> allConfigurations = new List<SMSceneConfigurationBase>();

		foreach(FileInfo file in files) {
			string filePath = file.FullName.Substring(directoryPath.Length + 1);
			SMSceneConfigurationBase configuration = AssetDatabase.LoadAssetAtPath(filePath, typeof(SMSceneConfigurationBase)) as SMSceneConfigurationBase;
			if (configuration != null) {
				allConfigurations.Add(configuration);
			}
		}	
		return allConfigurations;
	}	
	
	
	/// <summary>
	/// Finds all unity scenes
	/// </summary>
	/// <returns>
	/// The unity scenes.
	/// </returns>
	protected static FileInfo[] FindUnityScenes() {
		DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
		return directory.GetFiles("*.unity", SearchOption.AllDirectories);	
	}

}

