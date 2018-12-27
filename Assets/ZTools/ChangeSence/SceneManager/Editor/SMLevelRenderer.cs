//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Implementation of <see cref="CUItemRenderer"/> for rendering string items in a list.
/// </summary>
public class SMLevelRenderer : CUItemRenderer<string> {
	
	public override float MeasureHeight (string item) {
		return 36f;
	}
	
	public override void Arrange (string item, int itemIndex, bool selected, bool focused, Rect itemRect) {
		GUIStyle backgroundStyle = itemIndex % 2 == 1 ? ListStyle.oddBackground : ListStyle.evenBackground;
		backgroundStyle.Draw(itemRect, false, false, selected, false);					
        ListStyle.item.Draw(new Rect(itemRect.x + 32, itemRect.y, itemRect.width, itemRect.height), 
		                    new GUIContent(item), true, false, selected, false);
		
		GUI.DrawTexture(new Rect(itemRect.x + 4, itemRect.y + 4, 28, 28), SMEditorResources.SMLevelMarker);	
	}
}

