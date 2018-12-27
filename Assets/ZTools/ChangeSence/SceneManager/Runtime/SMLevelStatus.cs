//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

public enum SMLevelStatus {
	
	/// <summary>
	/// level wasn't loaded yet
	/// </summary>
	New,
	
	/// <summary>
	/// The previous level has been finished. Automatically set for the first level.
	/// </summary>
	Open,
	
	/// <summary>
	/// level loaded but not yet finished
	/// </summary>
	Visited,
	
	/// <summary>
	/// level finished
	/// </summary>
	Done
}

