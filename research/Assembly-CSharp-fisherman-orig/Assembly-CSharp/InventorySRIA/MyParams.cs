using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using InventorySRIA.Models;
using ObjectModel;
using UnityEngine;

namespace InventorySRIA
{
	[Serializable]
	public class MyParams : BaseParams
	{
		[Space(15f)]
		public StoragePlaces places;

		public DropMeStorage DropMeStorage;

		public DragNDropType TypeObjectOfDragItem;

		public bool isExceedInventory;

		public GroupCategoryIcon[] supportedGroups;

		public InitRods Rods;

		public RectTransform headerGroupPrefab;

		public RectTransform inventoryItemPrefab;

		[NonSerialized]
		public List<BaseModel> data = new List<BaseModel>();
	}
}
