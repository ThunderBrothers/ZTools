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
/// Implementation of the DropTarget interface for the group list
/// </summary>
public class SMGroupListDropTarget : CUListDropTarget {
	
	private DropAtPositionDelegate dropPositionHandler;
	private DropAtItemDelegate dropItemHandler;
	
	public SMGroupListDropTarget(DropAtPositionDelegate dropPositionHandler, DropAtItemDelegate dropItemHandler) {
		this.dropPositionHandler = dropPositionHandler;
		this.dropItemHandler = dropItemHandler;
	}
	
	public bool CanDrop (int index, CUDropType dropType) {
		if (dropType == CUDropType.IntoItem && (SMLevelListDragSource.IsSender || SMSceneListDragSource.IsSender)) {
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			return true;
		}
		if (dropType == CUDropType.AtPosition && SMGroupListDragSource.IsSender) {
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			return true;		
		}
		return false;
	}
	
	public void AcceptDrop(int index, CUDropType dropType) {
		if (CanDrop(index, dropType)) {
			if (SMSceneListDragSource.IsSender) {
				dropItemHandler(SMSceneListDragSource.DragData, index, typeof(SMSceneListDragSource));
			} else if (SMLevelListDragSource.IsSender) {
				dropItemHandler(SMLevelListDragSource.DragData, index, typeof(SMLevelListDragSource));			
			} else {
				dropPositionHandler(SMGroupListDragSource.DragData, index);
			}
			DragAndDrop.AcceptDrag();
		}
	}
	
	public delegate void DropAtPositionDelegate(int group, int index);
	public delegate void DropAtItemDelegate(IList<int> sceneIndices, int index, Type dragSource);
	
}

