//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
/// <summary>
/// Interface for tracking the level progress of a player. 
/// Scene manager does not make any assumptions on how the level progress is being stored. To store the level
/// progress, implement this class and set your implementation to the <see cref="SMSceneManager.LevelProgress"/>
/// property.
/// </summary>
public interface SMILevelProgress {

	/// <summary>
	/// The status of a level. 
	/// </summary>
	/// <param name="levelId">
	/// the Id of the level
	/// </param>
	/// <remarks>@deprecated Replaced by SetLevelStatus and GetLevelStatus with Scene Manager 1.4.0</remarks>
	[ObsoleteAttribute("Replaced by SetLevelStatus and GetLevelStatus with Scene Manager 1.4.0")]
	SMLevelStatus this[string levelId] {
		get;
		set;
	}
	
	/// <summary>
	/// The last played level. 
	/// </summary>
	string LastLevelId {
		get;
		set;
	}
	
	/// <summary>
	/// The ID of the level that the user is currently playing or will be playing after a cutscene.
	/// </summary>
	/// <remarks>@since version 1.4.0</remarks>
	string CurrentLevelId {
		get;
		set;
	}
	
	/// <summary>
	/// Returns true, if the player already played at least one level. 
	/// </summary>
	bool HasPlayed {
		get;
	}	
	
	/// <summary>
	/// Called when the player has beaten the last level of the game.
	/// </summary>
	void ResetLastLevel();
	
	
	/// <summary>
	/// Gets the status of a group.
	/// </summary>
	/// <param name='groupId'>
	/// The id of the group to check the status for.
	/// </param>
	/// <remarks>@since version 1.4.0</remarks>
	SMGroupStatus GetGroupStatus(string groupId);
	
	/// <summary>
	/// Sets the status of a group.
	/// </summary>
	/// <param name='groupId'>
	/// The id of the group to set the status for.
	/// </param>
	/// <param name='groupStatus'>
	/// The new status of the group.
	/// </param>
	/// <remarks>@since version 1.4.0</remarks>
	void SetGroupStatus(string groupId, SMGroupStatus groupStatus);

	/// <summary>
	/// Gets the status of a level.
	/// </summary>
	/// <param name='levelId'>
	/// The id of the level to check the status for.
	/// </param>
	/// <remarks>@since version 1.4.0</remarks>
	SMLevelStatus GetLevelStatus(string levelId);
	
	/// <summary>
	/// Sets the status of a level.
	/// </summary>
	/// <param name='levelId'>
	/// The id of the level to set the status for.
	/// </param>
	/// <param name='levelStatus'>
	/// The new status of the level.
	/// </param>
	/// <remarks>@since version 1.4.0</remarks>
	void SetLevelStatus(string levelId, SMLevelStatus levelStatus);
	
}
