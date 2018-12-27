//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;

public interface SMIConfigurationAdapter {
	
	string Name { get; }
	
	string[] Levels { get; }
	
	string[] Groups { get; }
	
	string[] Screens { get; }
	
	string FirstScreen { get; }
	
	string FirstScreenAfterLastLevel { get; }
	
	SMWorkflowActionType ActionAfterGroup { get; }
	
	string FirstScreenAfterGroup { get; }
	
	string[] GetLevelsInGroup(string groupId);
	
	string GetGroupOfLevel(string levelId);
	
	bool LevelExists(string levelId);
	
	bool GroupExists(string groupId);
	
}
