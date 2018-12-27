// (C) 2013 Ancient Light Studios. All rights reserved.
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;
using UnityEngine;

/// <summary>
/// Level progress which can be serialized from/to XML.
/// </summary>
public class SMSerializableLevelProgress : SMILevelProgress
{

	private Dictionary<string,SMLevelStatus> levelStatus = new Dictionary<string, SMLevelStatus>();
	private Dictionary<string,SMGroupStatus> groupStatus = new Dictionary<string, SMGroupStatus>();
	private string lastLevelId;
	private string currentLevelId;

	/// <summary>
	/// Default constructor. Will create a new empty level progress.
	/// </summary>
	public SMSerializableLevelProgress() {
	}

	/// <summary>
	/// Constructor taking a serialized string as argument. The level progress will be restored from this
	/// serialized string.
	public SMSerializableLevelProgress(string serializedProgress) {
		Deserialize(serializedProgress);
	}
	
	
	public SMLevelStatus this[string levelId] {
		get {
			return GetLevelStatus(levelId);
		}
		set {
			SetLevelStatus(levelId, value);
		}
	}
	
	public SMLevelStatus GetLevelStatus (string levelId)
	{
		return levelStatus.ContainsKey(levelId) ? levelStatus[levelId] : SMLevelStatus.New;
	}
	
	public void SetLevelStatus (string levelId, SMLevelStatus levelStatus)
	{

		this.levelStatus[levelId] = levelStatus;
		Debug.Log("Serialized: " + Serialize());
	} 
	
	public SMGroupStatus GetGroupStatus (string groupId)
	{
		return groupStatus.ContainsKey(groupId) ? groupStatus[groupId] : SMGroupStatus.New;
	}
	
	public void SetGroupStatus (string groupId, SMGroupStatus groupStatus)
	{
		this.groupStatus[groupId] = groupStatus;
	}
	
	public string LastLevelId {
		get {
			return lastLevelId;
		}
		set {
			lastLevelId = value;
		}
	}
	
	public string CurrentLevelId {
		get {
			return currentLevelId;
		}
		set {
			currentLevelId = value;
		}
	}
	
	public bool HasPlayed {
		get {
			return lastLevelId != null;
		}
	}	
	
	public void ResetLastLevel() {
		lastLevelId = null;
		currentLevelId = null;
	}

	/// <summary>
	/// Serialize the contents of this level progress into a string.
	/// </summary>
	public string Serialize() {
		var serializedForm = new SerializedForm();
		serializedForm.currentLevelId = currentLevelId;
		serializedForm.lastLevelId = lastLevelId;
		foreach( var entry in levelStatus) {
			serializedForm.levelStatus.Add (new SerializedForm.Item(entry.Key, (int) entry.Value));
		}
		foreach( var entry in groupStatus) {
			serializedForm.groupStatus.Add (new SerializedForm.Item(entry.Key, (int) entry.Value));
		}

		XmlSerializer serializer = new XmlSerializer(typeof(SerializedForm));
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
		settings.Indent = false;
		settings.OmitXmlDeclaration = false;
		
		using(StringWriter textWriter = new StringWriter()) {
			using(XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
				serializer.Serialize(xmlWriter, serializedForm);
			}
			return textWriter.ToString();
		}
	}

	private void Deserialize(string serialized) {
		if(string.IsNullOrEmpty(serialized)) {
			return;
		}
		
		XmlSerializer serializer = new XmlSerializer(typeof(SerializedForm));
		
		XmlReaderSettings settings = new XmlReaderSettings();
		// No settings need modifying here

		SerializedForm result;
		using(StringReader textReader = new StringReader(serialized)) {
			using(XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
				result = (SerializedForm)serializer.Deserialize(xmlReader);
			}
		}

		this.currentLevelId = result.currentLevelId;
		this.lastLevelId = result.lastLevelId;
		this.levelStatus = new Dictionary<string, SMLevelStatus>();
		foreach( var item in result.levelStatus) {
			levelStatus[item.key] = (SMLevelStatus) item.value;
		}

		this.groupStatus = new Dictionary<string, SMGroupStatus>();
		foreach(var item in result.groupStatus) {
			groupStatus[item.key] = (SMGroupStatus) item.value;
		}

	}

	public class SerializedForm {
		[XmlArray]
		public List<Item> levelStatus = new List<Item>();
		[XmlArray]
		public List<Item> groupStatus = new List<Item>();
		[XmlAttribute]
		public string lastLevelId;
		[XmlAttribute]
		public string currentLevelId;
		public class Item {
			[XmlAttribute]
			public string key;
			[XmlAttribute]
			public int value;
			
			public Item() {
				
			}
			
			public Item(string key, int value) {
				this.key = key;
				this.value = value;
			}
		}

	}


}

