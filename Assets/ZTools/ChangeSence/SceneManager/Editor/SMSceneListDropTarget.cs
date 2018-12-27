//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Implementation of the DropTarget interface for the scene list
/// </summary>
public class SMSceneListDropTarget : CUListDropTarget {
	
	private DropDelegate dropHandler;
	
	public SMSceneListDropTarget(DropDelegate dropHandler) {
		this.dropHandler = dropHandler;
	}
		
	public bool CanDrop (int index, CUDropType dropType) {
		if (dropType == CUDropType.IntoContainer && SMLevelListDragSource.IsSender) {
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			return true;
		}
		return false;
	}
	
	public void AcceptDrop(int index, CUDropType dropType) {
		if (CanDrop(index, dropType)) {
			dropHandler(SMLevelListDragSource.DragData);
			DragAndDrop.AcceptDrag();
		}
	}
	
	public delegate void DropDelegate(IList<int> levelIndices);
	
}

