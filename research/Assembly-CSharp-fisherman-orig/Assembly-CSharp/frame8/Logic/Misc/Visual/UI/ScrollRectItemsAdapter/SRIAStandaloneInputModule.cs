﻿using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public class SRIAStandaloneInputModule : StandaloneInputModule, ISRIAPointerInputModule
	{
		public Dictionary<int, PointerEventData> GetPointerEventData()
		{
			return this.m_PointerData;
		}
	}
}
