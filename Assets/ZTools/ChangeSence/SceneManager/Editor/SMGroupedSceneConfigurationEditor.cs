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
/// Editor for <see cref="SMGroupedSceneConfiguration"/> objects.
/// </summary>
[CustomEditor(typeof(SMGroupedSceneConfiguration))]
public class SMGroupedSceneConfigurationEditor : SMSceneConfigurationEditorBase<SMGroupedSceneConfiguration> {
	
	private const string SceneListHeight = "SMGroupedSceneConfigurationEditor.sceneListHeight";
	private const string LevelListHeight = "SMGroupedSceneConfigurationEditor.LevelListHeight";
	private const string GroupListWidth = "SMGroupedSceneConfigurationEditor.GroupListWidth";
	
	private SMLevelRenderer levelRenderer;	
	private SMSceneRenderer sceneRenderer;
	private CUListData sceneListData = new CUListData(true);	
	private CUListData groupListData = new CUListData(false);
	private CUListData levelListData = new CUListData(true);
	private float sceneListHeight = 200f;
	private float levelListHeight = 200f;
	private float groupListWidth = 200f;
	
	private bool initStyle = true;
	private GUIStyle boxStyle;
	private GUIStyle warnIconStyle;
	
	private string[] currentGroupLevels;
	
	/// <summary>
	/// Adds a new scene configuration at the currently selected path.
	/// </summary>
	[MenuItem("Assets/Create/Grouped Scene Configuration")]
	public static void AddConfiguration() {
		var configuration = CreateConfiguration<SMGroupedSceneConfiguration>();
		SMGroupedSceneConfigurationOperation.Build(configuration).AddGroup("Default").Apply(configuration);
	}
	
	public override void OnEnable() {
		sceneListHeight = EditorPrefs.GetFloat(SceneListHeight, 200f);
		levelListHeight = EditorPrefs.GetFloat(LevelListHeight, 200f);
		groupListWidth = EditorPrefs.GetFloat(GroupListWidth, 200f);
		levelRenderer = new SMLevelRenderer();
		sceneRenderer = new SMSceneRenderer(Target);
		
		sceneListData.DragSource = new SMSceneListDragSource();
		sceneListData.DropTarget = new SMSceneListDropTarget(DropLevelInScenes);
		groupListData.DragSource = new SMGroupListDragSource();
		groupListData.DropTarget = new SMGroupListDropTarget(DropGroup, DropScenesInGroup);
		levelListData.DragSource = new SMLevelListDragSource();
		levelListData.DropTarget = new SMLevelListDropTarget(DropScenesInLevels);
		
		base.OnEnable();
	}
	
	public void OnDisable() {
		EditorPrefs.SetFloat(SceneListHeight, sceneListHeight);
		EditorPrefs.SetFloat(LevelListHeight, levelListHeight);
		EditorPrefs.SetFloat(GroupListWidth, groupListWidth);
	}
		
	protected override void FixInvalidScenes() {
		SMGroupedSceneConfigurationOperation.Build(Target).Ignore(invalidScreens).Ignore(invalidLevels).Apply(Target);
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
			groupListData.ClearSelection();
			CheckConfiguration();
		}
		
		RebuildLevelOfCurrentGroup();
		
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
		SMWorkflowActionType action = (SMWorkflowActionType) EditorGUILayout.EnumPopup(new GUIContent("Action after Group", ""), Target.actionAfterGroup);		
		if (action != Target.actionAfterGroup) {
			ChangeToWorkflowAction(action);
		}
		GUILayout.Space(5);
		
		sceneListHeight = CUResizableContainer.BeginVertical(sceneListHeight);
		sceneListData = CUListControl.SelectionList(sceneListData, scenes, sceneRenderer, "Available Scenes");
		CUResizableContainer.EndVertical();
		
		GUI.enabled = !sceneListData.Empty && SelectedGroup != null;
		EditorGUILayout.BeginHorizontal();		
		if (GUILayout.Button("Level")) {
			ChangeToLevel();
		}
		GUI.enabled = !sceneListData.Empty;
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
		
		GUI.enabled = GUI.enabled && Target.actionAfterGroup == SMWorkflowActionType.LoadScreen;
		if (GUILayout.Button("After Group")) {
			ChangeToFirstScreenAfterGroup();
		}
		
		EditorGUILayout.EndHorizontal();
		GUI.enabled = true;
		EditorGUILayout.Space();
		
		levelListHeight = CUResizableContainer.BeginVertical(levelListHeight);
		EditorGUILayout.BeginHorizontal();
		groupListWidth = CUResizableContainer.BeginHorizontal(groupListWidth);
		int lastGroup = groupListData.First;
		groupListData = CUListControl.SelectionList(groupListData, Target.groups, levelRenderer, "Groups");	
		if (lastGroup != groupListData.First) {
			// group changed reset level selection
			levelListData.ClearSelection();
		}
		
		CUResizableContainer.EndHorizontal();
		GUI.enabled = SelectedGroup != null;
		levelListData = CUListControl.SelectionList(levelListData, currentGroupLevels, levelRenderer, "Levels");	
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		CUResizableContainer.EndVertical();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("+")) {
			AddGroup();
		}
		GUI.enabled = !groupListData.Empty && Target.groups.Length > 1;
		if (GUILayout.Button("-")) {
			RemoveGroup();
		}
		GUI.enabled = !groupListData.Empty;
		if (GUILayout.Button("Rename")) {
			RenameGroup();
		}
		GUI.enabled = true;
		GUILayout.FlexibleSpace();
		GUI.enabled = !levelListData.Empty;
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
		GUI.enabled = true;		
		EditorGUILayout.EndHorizontal();
	}

	private string SelectedGroup {
		get { 
			return groupListData.Empty && groupListData.First < Target.groups.Length ? null : Target.groups[groupListData.First];
		}
	}
	
	private void RebuildLevelOfCurrentGroup() {
		currentGroupLevels = new string[0];
		string currentGroup = SelectedGroup;
		if (currentGroup != null) {
			for(int i = 0; i < Target.groups.Length; i++) {
				if (Target.groups[i] == currentGroup) {
					int start = Target.groupOffset[i];
					int end = (i + 1 == Target.groups.Length) ? Target.levels.Length : Target.groupOffset[i + 1];
					int length = end - start;
					currentGroupLevels = new string[length];
					Array.Copy(Target.levels, start, currentGroupLevels, 0, length);
				}
			}
		}	
	}
	
	private bool VerifyGroupName(string name) {
		// name required
		if (String.IsNullOrEmpty(name)) {
			return false;
		}
		
		// unique name
		if (Array.IndexOf(Target.groups, name) != -1) {
			return false;
		}	
		
		return true;
	}

	/// <summary>
	/// Adds a new group to the configuration
	/// </summary>
	private void AddGroup() {
		// save position before opening the dialog. otherwise the position will be resetted
		Vector2 position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);		
		CUTextInputDialog.ShowDialog("Name", "", AddGroup, VerifyGroupName).CenterAt(position);
		GUIUtility.ExitGUI();
	}
	
	private void AddGroup(string name) {
		BeforeChange("Add group");
		SMGroupedSceneConfigurationOperation.Build(Target).AddGroup(name).Apply(Target);
		groupListData.SetSelectedItems(Target.groups, new string[] {name});
		levelListData.ClearSelection();
		EditorUtility.SetDirty(Target);		
	}
	
	/// <summary>
	/// Removes the group from the configuration
	/// </summary>
	private void RemoveGroup() {
		BeforeChange("Remove group");
		SMGroupedSceneConfigurationOperation.Build(Target).RemoveGroup(SelectedGroup).Apply(Target);
		groupListData.ClearSelection();
		levelListData.ClearSelection();
		EditorUtility.SetDirty(Target);		
	}
	
	/// <summary>
	/// Changes the name of a group
	/// </summary>
	private void RenameGroup() {
		// save position before opening the dialog. otherwise the position will be resetted
		Vector2 position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
		CUTextInputDialog.ShowDialog("Name", SelectedGroup, RenameGroup, VerifyGroupName).CenterAt(position);
		GUIUtility.ExitGUI();
	}	
	
	private void RenameGroup(string name) {
		BeforeChange("Rename group");
		SMGroupedSceneConfigurationOperation.Build(Target).RenameGroup(name, SelectedGroup).Apply(Target);
		groupListData.SetSelectedItems(Target.groups, new string[] {name});
		levelListData.ClearSelection();
		EditorUtility.SetDirty(Target);		
	}
	
	private void ChangeToIgnore() {
		BeforeChange("Change scenes to ignore");
		IList<string> selectedScenes = sceneListData.GetSelectedItems(scenes);
		SMGroupedSceneConfigurationOperation.Build(Target).Ignore(selectedScenes).Apply(Target);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();
		EditorUtility.SetDirty(Target);
	}

	private void ChangeToScreen() {
		BeforeChange("Change scenes to screen");
		IList<string> selectedScenes = sceneListData.GetSelectedItems(scenes);
		SMGroupedSceneConfigurationOperation.Build(Target).Screen(selectedScenes).Apply(Target);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();
		EditorUtility.SetDirty(Target);
	}

	private void ChangeToLevel() {
		BeforeChange("Change scenes to level");
		IList<string> selectedScenes = sceneListData.GetSelectedItems(scenes);
		SMGroupedSceneConfigurationOperation.Build(Target).Level(selectedScenes, SelectedGroup).Apply(Target);
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
			SMGroupedSceneConfigurationOperation.Build(Target).FirstScreen(scene).Apply(Target);
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
			SMGroupedSceneConfigurationOperation.Build(Target).FirstScreenAfterLevel(scene).Apply(Target);
			SyncBuildSettingsIfRequired();
			EditorUtility.SetDirty(Target);
		}
	}
	
	/// <summary>
	/// Makes the currently selected scene the first screen after each group of the game.
	/// </summary>
	private void ChangeToWorkflowAction(SMWorkflowActionType action) {
		BeforeChange("Change action after group");
		SMGroupedSceneConfigurationOperation.Build(Target).ActionAfterGroup(action).Apply(Target);
		SyncBuildSettingsIfRequired();
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Makes the currently selected scene the first screen after each group of the game.
	/// </summary>
	private void ChangeToFirstScreenAfterGroup() {
		string scene = scenes[sceneListData.First];
		if (Target.firstScreenAfterGroup != scene) {
			BeforeChange("Change scene to be first screen after group");
			SMGroupedSceneConfigurationOperation.Build(Target).FirstScreenAfterGroup(scene).Apply(Target);
			SyncBuildSettingsIfRequired();
			EditorUtility.SetDirty(Target);
		}
	}
	
	/// <summary>
	/// Moves the selected level into the top position making it the first level.
	/// </summary>
	private void MoveToFirst() {
		BeforeChange("Move level to first position");
		IList<string> selectedLevels = levelListData.GetSelectedItems(currentGroupLevels);
		SMGroupedSceneConfigurationOperation.Build(Target).MoveLevelToTop(selectedLevels, SelectedGroup).Apply(Target);
		RebuildLevelOfCurrentGroup();
		levelListData.SetSelectedItems(currentGroupLevels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Moves the selected level one position up.
	/// </summary>
	private void MoveUp() {
		BeforeChange("Move level up");
		IList<string> selectedLevels = levelListData.GetSelectedItems(currentGroupLevels);
		SMGroupedSceneConfigurationOperation.Build(Target).MoveLevelUp(selectedLevels, SelectedGroup).Apply(Target);
		RebuildLevelOfCurrentGroup();
		levelListData.SetSelectedItems(currentGroupLevels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Moves the selected level one position down.
	/// </summary>
	private void MoveDown() {
		BeforeChange("Move level down");
		IList<string> selectedLevels = levelListData.GetSelectedItems(currentGroupLevels);
		SMGroupedSceneConfigurationOperation.Build(Target).MoveLevelDown(selectedLevels, SelectedGroup).Apply(Target);
		RebuildLevelOfCurrentGroup();
		levelListData.SetSelectedItems(currentGroupLevels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	///  Moves the selected level to the last position, making it the last level of the game.
	/// </summary>
	private void MoveToLast() {
		BeforeChange("Move level to last position");
		IList<string> selectedLevels = levelListData.GetSelectedItems(currentGroupLevels);
		SMGroupedSceneConfigurationOperation.Build(Target).MoveLevelToBottom(selectedLevels, SelectedGroup).Apply(Target);
		RebuildLevelOfCurrentGroup();
		levelListData.SetSelectedItems(currentGroupLevels, selectedLevels);
		EditorUtility.SetDirty(Target);
	}
	
	/// <summary>
	/// Drop scenes from the scene list into the level list or move levels inside the level list
	/// </summary>
	private void DropScenesInLevels(IList<int> sceneIndices, int index, Type dragSource) {
		if (dragSource.Equals(typeof(SMSceneListDragSource))) {
			BeforeChange("Add scenes as level");
			IList<string> selectedScenes = ListOperation<string>.FilterList(scenes, sceneIndices);
			SMGroupedSceneConfigurationOperation.Build(Target).Level(selectedScenes, SelectedGroup).MoveLevelToPosition(selectedScenes, index, SelectedGroup).Apply(Target);
			RebuildLevelOfCurrentGroup();
			levelListData.SetSelectedItems(currentGroupLevels, selectedScenes);
			SyncBuildSettingsIfRequired();
		} else {
			BeforeChange("Move level");
			IList<string> selectedLevel = ListOperation<string>.FilterList(currentGroupLevels, sceneIndices);
			SMGroupedSceneConfigurationOperation.Build(Target).MoveLevelToPosition(selectedLevel, index, SelectedGroup).Apply(Target);
			RebuildLevelOfCurrentGroup();
			levelListData.SetSelectedItems(currentGroupLevels, selectedLevel);
		}
		EditorUtility.SetDirty(Target);		
	}
	
	/// <summary>
	/// Drop levels from the level list into the scene list
	/// </summary>
	private void DropLevelInScenes(IList<int> levelIndices) {
		BeforeChange("Remove levels");
		IList<string> selectedLevels = ListOperation<string>.FilterList(currentGroupLevels, levelIndices);
		SMGroupedSceneConfigurationOperation.Build(Target).Ignore(selectedLevels).Apply(Target);
		RebuildLevelOfCurrentGroup();
		sceneListData.SetSelectedItems(scenes, selectedLevels);
		levelListData.ClearSelection();
		SyncBuildSettingsIfRequired();		
		EditorUtility.SetDirty(Target);		
	}

	/// <summary>
	/// Move groups inside the group list
	/// </summary>
	private void DropGroup(int group, int index) {
		BeforeChange("Move group");
		string selectedGroup = Target.groups[group];
		SMGroupedSceneConfigurationOperation.Build(Target).MoveGroupToPosition(selectedGroup, index).Apply(Target);
		RebuildLevelOfCurrentGroup();
		groupListData.SetSelectedItem(Target.groups, selectedGroup);
		EditorUtility.SetDirty(Target);				
	}
	
	/// <summary>
	/// Drop scenes from the scene list or levels from the level list into the group list
	/// </summary>
	private void DropScenesInGroup(IList<int> sceneIndices, int index, Type dragSource) {
		if (dragSource.Equals(typeof(SMSceneListDragSource))) {
			BeforeChange("Add scenes as level");
			IList<string> selectedScenes = ListOperation<string>.FilterList(scenes, sceneIndices);
			SMGroupedSceneConfigurationOperation.Build(Target).Level(selectedScenes, Target.groups[index]).Apply(Target);
			RebuildLevelOfCurrentGroup();
			levelListData.ClearSelection();
			SyncBuildSettingsIfRequired();		
		} else {
			BeforeChange("Move level");
			IList<string> selectedLevel = ListOperation<string>.FilterList(currentGroupLevels, sceneIndices);
			SMGroupedSceneConfigurationOperation.Build(Target).Level(selectedLevel, Target.groups[index]).Apply(Target);
			RebuildLevelOfCurrentGroup();
			levelListData.ClearSelection();
		}
		EditorUtility.SetDirty(Target);		
	}
	
}
