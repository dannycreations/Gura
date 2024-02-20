using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.ChatExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefabAndData<ChatMessageModel>
	{
		public Sprite[] availableChatImages;
	}
}
