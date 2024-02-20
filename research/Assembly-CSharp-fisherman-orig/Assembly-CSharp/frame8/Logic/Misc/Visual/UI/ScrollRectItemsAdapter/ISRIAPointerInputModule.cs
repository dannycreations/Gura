using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public interface ISRIAPointerInputModule
	{
		Dictionary<int, PointerEventData> GetPointerEventData();
	}
}
