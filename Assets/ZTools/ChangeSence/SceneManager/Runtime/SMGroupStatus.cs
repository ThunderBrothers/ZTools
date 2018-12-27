//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

public enum SMGroupStatus {
	
	/// <summary>
	/// No level of the group was played yet.
	/// </summary>
	New,
	/// <summary>
	/// At least one level of the group is in state Open. Automatically set for the first
	/// group.
	/// </summary>
	Open,
	/// <summary>
	/// At least one level of the group has been played (but not necessarily finished).
	/// </summary>
	Visited,
	/// <summary>
	/// The last level of the group has been finished.
	/// </summary>
	Done
}

