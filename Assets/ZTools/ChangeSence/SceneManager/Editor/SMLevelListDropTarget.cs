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
/// Implementation of the DropTarget interface for the level list
/// </summary>
public class SMLevelListDropTarget : CUListDropTarget {
	
	private DropAtPositionDelegate dropHandler;
	
	public SMLevelListDropTarget(DropAtPositionDelegate dropHandler) {
		this.dropHandler = dropHandler;
	}
	
	public bool CanDrop (int index, CUDropType dropType) {
		if (dropType == CUDropType.AtPosition && (SMSceneListDragSource.IsSender || SMLevelListDragSource.IsSender)) {
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			return true;
		} 
		return false;
	}
	
	public void AcceptDrop(int index, CUDropType dropType) {
		if (CanDrop(index, dropType)) {
			if (SMSceneListDragSource.IsSender) {
				dropHandler(SMSceneListDragSource.DragData, index, typeof(SMSceneListDragSource));
			} else {
				dropHandler(SMLevelListDragSource.DragData, index, typeof(SMLevelListDragSource));			
			}
			DragAndDrop.AcceptDrag();
		}
	}
	
	public delegate void DropAtPositionDelegate(IList<int> sceneIndices, int index, Type dragSource);
}

