using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using FriendsSRIA.Models;
using UnityEngine;

namespace FriendsSRIA
{
	[Serializable]
	public class MyParams : BaseParams
	{
		public RectTransform headerPrefab;

		public RectTransform friendItemPrefab;

		[NonSerialized]
		public List<BaseModel> data = new List<BaseModel>();
	}
}
