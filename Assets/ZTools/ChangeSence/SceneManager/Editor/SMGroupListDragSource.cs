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

/// <summary>
/// Implementation of the DragSource interface for the group list
/// </summary>
public class SMGroupListDragSource : CUListDragSource {
	
	private const string PathIdentifier = "groups://data";
	private const string DataIdentifier = "data";
	
	public static bool IsSender {
		get { return DragAndDrop.paths.Length == 1 && DragAndDrop.paths[0] == PathIdentifier; }
	}
	
	public static int DragData {
		get { return IsSender ? (int)DragAndDrop.GetGenericData(DataIdentifier) : -1; }
	}
		
	public bool CanDrag(IList<int> indices) {
		return indices != null && indices.Count == 1;
	}
	
	public void InitializeDrag(IList<int> indices) {
		if (CanDrag(indices)) {
			// TODO: convert indices to items
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.SetGenericData(DataIdentifier, indices[0]);
			DragAndDrop.paths = new string[] {PathIdentifier};
			DragAndDrop.StartDrag("1 item");
		}
	}
}

