//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using System.Collections.Generic;

/// <summary>
/// Utility class to manipulate lists
/// </summary>
public class ListOperation<T> {
	
	private List<T> source;
	
	public ListOperation(List<T> source) {
		this.source = source;
	}
	
	public void MoveToTop(IList<T> items) {
		// set containing all items to be moved for faster lookup
		HashSet<T> itemsLookup = new HashSet<T>(items);
		// original position of all items
		Dictionary<T, int> positionLookup = GetPositionLookup();
		
		source.Sort(delegate(T o1, T o2) {
			bool o1Selected = itemsLookup.Contains(o1);
			bool o2Selected = itemsLookup.Contains(o2);
			
			// both selected or none of both selected? keep original order
			if (o1Selected == o2Selected) {
				return positionLookup[o1] - positionLookup[o2];
			}

			return o1Selected ? -1 : 1;
		});		
	}
	
	public void MoveUp(IList<T> items) {
		// set containing all items to be moved for faster lookup
		HashSet<T> itemsLookup = new HashSet<T>(items);
		// original position of all items
		Dictionary<T, int> positionLookup = GetPositionLookup();
		// contains the group of each item. each item not in the items collection is its own group. adjacent items of the items collection share the same group
		Dictionary<T, int> groupLookup = GetGroupLookup(itemsLookup);
		
		source.Sort(delegate(T o1, T o2) {
			bool o1Selected = itemsLookup.Contains(o1);
			bool o2Selected = itemsLookup.Contains(o2);
			int o1Group = groupLookup[o1];
			int o2Group = groupLookup[o2];
			
			// both selected or none of both selected? keep original order
			if (o1Selected == o2Selected) {
				return positionLookup[o1] - positionLookup[o2];
			}

			if (o1Selected) {
				return o1Group -1 <= o2Group ? -1 : 1;	
			} else {
				return o2Group -1 <= o1Group ? 1 : -1;				
			}
		});		
	}
	
	public void MoveDown(IList<T> items) {
		// set containing all items to be moved for faster lookup
		HashSet<T> itemsLookup = new HashSet<T>(items);
		// original position of all items
		Dictionary<T, int> positionLookup = GetPositionLookup();
		// contains the group of each item. each item not in the items collection is its own group. adjacent items of the items collection share the same group
		Dictionary<T, int> groupLookup = GetGroupLookup(itemsLookup);
		
		source.Sort(delegate(T o1, T o2) {
			bool o1Selected = itemsLookup.Contains(o1);
			bool o2Selected = itemsLookup.Contains(o2);
			int o1Group = groupLookup[o1];
			int o2Group = groupLookup[o2];
			
			// both selected or none of both selected? keep original order
			if (o1Selected == o2Selected) {
				return positionLookup[o1] - positionLookup[o2];
			}

			if (o1Selected) {
				return o1Group +1 >= o2Group ? 1 : -1;	
			} else {
				return o2Group +1 >= o1Group ? -1 : 1;				
			}
		});		
	}

	public void MoveToBottom(IList<T> items) {
		// set containing all items to be moved for faster lookup
		HashSet<T> itemsLookup = new HashSet<T>(items);
		// original position of all items
		Dictionary<T, int> positionLookup = GetPositionLookup();
		
		source.Sort(delegate(T o1, T o2) {
			bool o1Selected = itemsLookup.Contains(o1);
			bool o2Selected = itemsLookup.Contains(o2);
			
			// both selected or none of both selected? keep original order
			if (o1Selected == o2Selected) {
				return positionLookup[o1] - positionLookup[o2];
			}

			return o1Selected ? 1 : -1;
		});				
	}
	
	public void MoveTo(IList<T> items, int index) {
		// set containing all items to be moved for faster lookup
		HashSet<T> itemsLookup = new HashSet<T>(items);
		// original position of all items
		Dictionary<T, int> positionLookup = GetPositionLookup();
		
		source.Sort(delegate(T o1, T o2) {
			bool o1Selected = itemsLookup.Contains(o1);
			bool o2Selected = itemsLookup.Contains(o2);
			
			// both selected or none of both selected? keep original order
			if (o1Selected == o2Selected) {
				return positionLookup[o1] - positionLookup[o2];
			}
			
			if (o1Selected) {
				return positionLookup[o2] < index ? 1 : -1;
			} else {
				return positionLookup[o1] < index ? -1 : 1;
			}
		});				
	}
	
	private Dictionary<T, int> GetPositionLookup() {
		Dictionary<T, int> result = new Dictionary<T, int>();
		foreach(T item in source) {
			result.Add(item, result.Count);
		}
		return result;
	}
	
	private Dictionary<T, int> GetGroupLookup(HashSet<T> itemsLookup) {
		Dictionary<T, int> result = new Dictionary<T, int>();
		int group = 0;
		bool groupStarted = false;
		foreach(T item in source) {
			bool selected = itemsLookup.Contains(item);
			if (!selected || !groupStarted) {
				group++;
			}
			groupStarted = selected;
			result.Add(item, group);
		}
		return result;
	}
	
	public static IList<T> FilterList(IList<T> items, IList<int> indices) {
		IList<T> result = new List<T>();
		foreach(int index in indices) {
			result.Add(items[index]);
		}
		return result;
	}
			
}
