//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;

/// <summary>
/// Helper class which finds and loads the currently active scene configuration.
/// </summary>
public class SMSceneConfigurationLoader {
	
	/// <summary>
	/// Loads the currently active scene configuration.
	/// </summary>
	/// <param name="directory">
	/// A <see cref="System.String"/> denoting the folder where all scene configurations are located at.
	/// </param>
	/// <returns>
	/// A <see cref="SMSceneConfiguration"/> that has been loaded or null if no scene configurations could be found
	/// or no scene configuration was active.
	/// </returns>
	public static SMIConfigurationAdapter LoadActiveConfiguration(string directory) {
		var resources = Resources.LoadAll(directory, typeof(SMSceneConfiguration));
		foreach(var resource in resources) {
			var configuration = (SMSceneConfiguration)resource;
			if (configuration.activeConfiguration) {
				return new SMSceneConfigurationAdapter(configuration);
			}
		}
		
	    resources = Resources.LoadAll(directory, typeof(SMGroupedSceneConfiguration));
		foreach(var resource in resources) {
			var configuration = (SMGroupedSceneConfiguration)resource;
			if (configuration.activeConfiguration) {
				return new SMGroupedSceneConfigurationAdapter(configuration);
			}
		}
		
		Debug.LogError("No active scene configuration found at " + directory + ". Please check that one scene " +
			"configuration within the " + directory + " folder is active. To activate a scene configuration, select it in " +
			"the project view and then press the 'Activate' button in the inspector.");
		return null;
	}
	
}

