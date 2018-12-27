//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor for <see cref="SMSceneConfiguration"/> objects.
/// </summary>
[CustomEditor(typeof(SMSceneConfiguration))]
public class SMSceneConfigurationEditor : SMSceneConfigurationEditorBase<SMSceneConfiguration> {
	
	private const string SceneListHeight = "SMSceneConfigurationEditor.sceneListHeight";
	private const string LevelListHeight = "SMSceneConfigurationEditor.LevelListHeight";
	
	private SMLevelRenderer levelRenderer;	
	private SMSceneRenderer sceneRenderer;
	private CUListData sceneListData = new CUListData(true);	
	private CUListData levelListData = new CUListData(true);
	private float sceneListHeight = 200f;
	private float levelListHeight = 200f;
	
	private bool initStyle = true;
	private GUIStyle boxStyle;
	private GUIStyle warnIconStyle;
	
	/// <summary>
	/// Adds a new scene configuration at the currently selected path.
	/// </summary>
	[MenuItem("Assets/Create/Scene Configuration")]
	public static void AddConfiguration() {
    	CreateConfiguration<SMSceneConfiguration>();
	}
	
	public override void OnEnable() {
		sceneListHeight = EditorPrefs.GetFloat(SceneListHeight, 200f);
		levelListHeight = EditorPrefs.GetFloat(LevelListHeight, 200f);
		levelRenderer = new SMLevelRenderer();
		sceneRenderer = new SMSceneRenderer(Target);
		
		sceneListData.DragSource = new SMSceneListDragSource();
		sceneListData.DropTarget = new SMSceneListDropTarget(DropLevelInScenes);
		levelListData.DragSource = new SMLevelListDragSource();
		levelListData.DropTarget = new SMLevelListDropTarget(DropScenesInLevels);

		base.OnEnable();
	}
	
	public void OnDisable() {
		EditorPrefs.SetFloat(SceneListHeight, sceneListHeight);
		EditorPrefs.SetFloat(LevelListHeight, levelListHeight);
	}
				
	protected override void FixInvalidScenes() {
		SMSceneConfigurationOperation.Build(Target).Ignore(invalidScreens).Ignore(invalidLevels).Apply(Target);
	}
		
	/// <summary>
	/// Draws the inspector GUI.
	/// </summary>
	public override void OnInspectorGUI() {
		if (initStyle) {
			boxStyle = (GUIStyle) "GroupBox";
			warnIconStyle = (GUIStyle) "CN EntryWarn";
			initStyle = false;
		}
		
		if (Target._guid != configurationGuid) {
			// guid is different. undo used
			sceneListData.ClearSelection();
			levelListData.ClearSelection();
			CheckConfiguration();
		}
		EditorGUILayout.Space();
		ConfigurationStatusGUI();
		EditorGUILayout.Space();
		SceneGUI();
		EditorGUILayout.Space();
	}
	
	/// <summary>
	/// Draws the label indicating if the currently edited scene configuration is the currently active configuration.
	/// </summary>
	/// <seealso cref="SMSceneConfigurtion.activeConfiguration"/>
	protected void ConfigurationStatusGUI() {
		if (Invalid) {
			
			EditorGUILayout.BeginHorizontal(boxStyle);
			GUILayout.Label(string.Empty, warnIconStyle);
			GUILayout.Label("The configuration is invalid.");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Fix configuration")) {
				FixConfiguration();
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.Space();
		if (Target.activeConfiguration) {
			GUILayout.Label("This configuration will be used by the game.");
		} else {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("The configuration is currently not active.");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Activate")) {
				SMSceneConfigurationUtil.EnsureActiveConfiguration(Target, true);
				SyncBuildSettingsIfRequired();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
	
	/// <summary>
	/// Draws the list of scenes in the inspector GUI.
	/// </summary>
	protected void SceneGUI() {
		sceneListHeight = CUResizableContainer.BeginVertical(sceneListHeight);
		sceneListData = CUListControl.SelectionList(sceneListData, scenes, sceneRenderer, "Available Scenes");
		CUResizableContainer.EndVertical();
		
		GUI.enabled = !sceneListData.Empty;
		EditorGUILayout.BeginHorizontal();		
		if (GUILayout.Button("Level")) {
			ChangeToLevel();
		}
		if (GUILayout.Button("Screen")) {
			ChangeToScreen();
		}
		if (GUILayout.Button("Ignore")) {
			ChangeToIgnore();
		}		
		EditorGUILayout.EndHorizontal();

		GUI.enabled = !sceneListData.Empty && IsScreen(scenes[sceneListData.First]);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("First Screen")) {
			ChangeToFirstScreen();
		}
		if (GUILayout.Button("After last Level")) {
			ChangeToFirstScreenAfterLevel();
		}
				 
		EditorGUILayout.EndHorizontal();
		GUI.enabled = true;
		EditorGUILayout.Space();
		
		levelListHeight = CUResizableContainer.BeginVertical(levelListHeight);
		levelListData = CUListControl.SelectionList(levelListData, Target.levels, levelRenderer, "Levels");	
		CUResizableContainer.EndVertical();
		
		GUI.enabled = !levelListData.Empty;
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("First")) {
			MoveToFirst();
		}
		if (GUILayout.Button("Up")) {
			MoveUp();
		}
		if (GUILayout.Button("Down")) {
			MoveDown();
		}
		if (GUILayout.Button("Last")) {
			MoveToLast();
		}		
		EditorGUILayout.EndHorizontal();
		GUI.enabled = true;		
	}

	private void ChangeToIgnore() {
		BeforeChange("Change scenes to ignore");
		IList<string> selectedScenes = sceneListData.GetSelectedItems(scenes);
		SMSceneConfigurationOperation.Build(Target).Ignore(selectedScenes).Apply(Target);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();
		EditorUtility.SetDirty(Target);
	}

	private void ChangeToScreen() {
		BeforeChange("Change scenes to screen");
		IList<string> selectedScenes = sceneListData.GetSelectedItems(scenes);
		SMSceneConfigurationOperation.Build(Target).Screen(selectedScenes).Apply(Target);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();
		EditorUtility.SetDirty(Target);
	}

	private void ChangeToLevel() {
		BeforeChange("Change scenes to level");
		IList<string> selectedScenes = sceneListData.GetSelectedItems(scenes);
		SMSceneConfigurationOperation.Build(Target).Level(selectedScenes).Apply(Target);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Makes the currently selected scene the first screen of the game.
	/// </summary>
	private void ChangeToFirstScreen() {
		string scene = scenes[sceneListData.First];
		if (Target.firstScreen != scene) {
			BeforeChange("Change scene to be first screen");
			SMSceneConfigurationOperation.Build(Target).FirstScreen(scene).Apply(Target);
			SyncBuildSettingsIfRequired();
			EditorUtility.SetDirty(Target);
		}
	}
	
	/// <summary>
	/// Makes the currently selected scene the first screen after the last level of the game.
	/// </summary>
	private void ChangeToFirstScreenAfterLevel() {
		string scene = scenes[sceneListData.First];
		if (Target.firstScreenAfterLevel != scene) {
			BeforeChange("Change scene to be first screen after last level");
			SMSceneConfigurationOperation.Build(Target).FirstScreenAfterLevel(scene).Apply(Target);
			SyncBuildSettingsIfRequired();
			EditorUtility.SetDirty(Target);
		}
	}
	
	/// <summary>
	/// Moves the selected level into the top position making it the first level.
	/// </summary>
	private void MoveToFirst() {
		BeforeChange("Move level to first position");
		IList<string> selectedLevels = levelListData.GetSelectedItems(Target.levels);
		SMSceneConfigurationOperation.Build(Target).MoveLevelToTop(selectedLevels).Apply(Target);
		levelListData.SetSelectedItems(Target.levels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Moves the selected level one position up.
	/// </summary>
	private void MoveUp() {
		BeforeChange("Move level up");
		IList<string> selectedLevels = levelListData.GetSelectedItems(Target.levels);
		SMSceneConfigurationOperation.Build(Target).MoveLevelUp(selectedLevels).Apply(Target);
		levelListData.SetSelectedItems(Target.levels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Moves the selected level one position down.
	/// </summary>
	private void MoveDown() {
		BeforeChange("Move level down");
		IList<string> selectedLevels = levelListData.GetSelectedItems(Target.levels);
		SMSceneConfigurationOperation.Build(Target).MoveLevelDown(selectedLevels).Apply(Target);
		levelListData.SetSelectedItems(Target.levels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	///  Moves the selected level to the last position, making it the last level of the game.
	/// </summary>
	private void MoveToLast() {
		BeforeChange("Move level to last position");
		IList<string> selectedLevels = levelListData.GetSelectedItems(Target.levels);
		SMSceneConfigurationOperation.Build(Target).MoveLevelToBottom(selectedLevels).Apply(Target);
		levelListData.SetSelectedItems(Target.levels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Drop scenes from the scene list into the level list or move levels inside the level list
	/// </summary>
	private void DropScenesInLevels(IList<int> sceneIndices, int index, Type dragSource) {
		if (dragSource.Equals(typeof(SMSceneListDragSource))) {
			BeforeChange("Add scenes as level");
			IList<string> selectedScenes = ListOperation<string>.FilterList(scenes, sceneIndices);
			SMSceneConfigurationOperation.Build(Target).Level(selectedScenes).MoveLevelToPosition(selectedScenes, index).Apply(Target);
			levelListData.SetSelectedItems(Target.levels, selectedScenes);
			SyncBuildSettingsIfRequired();		
		} else {
			BeforeChange("Move level");
			IList<string> selectedLevel = ListOperation<string>.FilterList(Target.levels, sceneIndices);
			SMSceneConfigurationOperation.Build(Target).MoveLevelToPosition(selectedLevel, index).Apply(Target);
			levelListData.SetSelectedItems(Target.levels, selectedLevel);
		}
		EditorUtility.SetDirty(Target);		
	}
	
	/// <summary>
	/// Drop levels from the level list into the scene list
	/// </summary>
	private void DropLevelInScenes(IList<int> levelIndices) {
		BeforeChange("Remove levels");
		IList<string> selectedLevels = ListOperation<string>.FilterList(Target.levels, levelIndices);
		SMSceneConfigurationOperation.Build(Target).Ignore(selectedLevels).Apply(Target);
		sceneListData.SetSelectedItems(scenes, selectedLevels);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();		
		EditorUtility.SetDirty(Target);		
	}
	
}
