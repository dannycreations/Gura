using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using LeaderboardSRIA.Models;
using UnityEngine;

namespace Leaderboard
{
	[Serializable]
	public class MyParams : BaseParams
	{
		public void SetItemSize(float value)
		{
			this._DefaultItemSize = value;
		}

		public RectTransform entryPrefab;

		[NonSerialized]
		public List<BaseModel> data = new List<BaseModel>();
	}
}
