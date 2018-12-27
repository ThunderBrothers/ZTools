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
/// Implementation of the DragSource interface for the scene list
/// </summary>
public class SMSceneListDragSource : CUListDragSource {
	
	private const string PathIdentifier = "scenes://data";
	private const string DataIdentifier = "data";
	
	public static bool IsSender {
		get { return DragAndDrop.paths.Length == 1 && DragAndDrop.paths[0] == PathIdentifier; }
	}
	
	public static IList<int> DragData {
		get { return IsSender ? DragAndDrop.GetGenericData(DataIdentifier) as IList<int> : null; }
	}
	
	public bool CanDrag(IList<int> indices) {
		return indices != null && indices.Count > 0;
	}
	
	public void InitializeDrag(IList<int> indices) {
		if (CanDrag(indices)) {
			// TODO: convert indices to items
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.SetGenericData(DataIdentifier, indices);
			DragAndDrop.paths = new string[] {PathIdentifier};
			if (indices.Count == 1) {
				DragAndDrop.StartDrag("1 item");
			} else {	
				DragAndDrop.StartDrag(indices.Count + " items");
			}
		}
	}
	
}

