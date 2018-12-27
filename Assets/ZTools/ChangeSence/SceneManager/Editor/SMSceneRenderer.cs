//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Implementation of <see cref="CUItemRenderer"/> for rendering string items in a list.
/// </summary>
public class SMSceneRenderer : CUItemRenderer<string> {
	
	private Texture levelMarker;
	private Texture screenMarker;
	private GUIStyle addon;
	
	private SMSceneConfigurationBase configuration;
	
	public SMSceneRenderer(SMSceneConfigurationBase configuration) {
		this.configuration = configuration;
	}
	
	public override float MeasureHeight (string item) {
		return 36f;
	}
	
	public override void Arrange (string item, int itemIndex, bool selected, bool focused, Rect itemRect) {
		if (levelMarker == null) {
			levelMarker = SMEditorResources.SMLevelMarker;
			screenMarker = SMEditorResources.SMScreenMarker;
			addon = new GUIStyle(ListStyle.item);
			addon.alignment = TextAnchor.MiddleRight;
			addon.padding.right += 10;
		}
		
		GUIStyle backgroundStyle = itemIndex % 2 == 1 ? ListStyle.oddBackground : ListStyle.evenBackground;
		backgroundStyle.Draw(itemRect, false, false, selected, false);					
        ListStyle.item.Draw(new Rect(itemRect.x + 32, itemRect.y, itemRect.width, itemRect.height), 
		                    new GUIContent(item), true, false, selected, false);
		
		if (Array.IndexOf(configuration.levels, item) > -1) {
			GUI.DrawTexture(new Rect(itemRect.x + 4, itemRect.y + 4, 28, 28), levelMarker);	
		} else if (Array.IndexOf(configuration.screens, item) > -1) {
			GUI.DrawTexture(new Rect(itemRect.x + 4, itemRect.y + 4, 28, 28), screenMarker);						
		}			
		
		string addonText = "";
		if (item == configuration.firstScreen) {
			addonText = "First Screen";
		}
		
		if (item == configuration.firstScreenAfterLevel) {
			addonText = Append(addonText, "After last Level");
		}
		
		if (configuration is SMGroupedSceneConfiguration) {
			if (item == ((SMGroupedSceneConfiguration)configuration).firstScreenAfterGroup) {
				addonText = Append(addonText, "After Group");
			}
		}
		
		if (!String.IsNullOrEmpty(addonText)) {
			addon.Draw(itemRect, addonText, false, false, selected, false);
		}							
	}
	
	private string Append(string text, string addon) {
		if(String.IsNullOrEmpty(text)) {
			return addon;
		}
		
		return text + ", " + addon;
	}
}

