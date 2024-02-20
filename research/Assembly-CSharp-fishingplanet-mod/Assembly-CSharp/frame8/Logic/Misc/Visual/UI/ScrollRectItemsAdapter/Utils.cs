using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public static class Utils
	{
		public static Vector2? ForceSetPointerEventDistanceToZero(PointerEventData pev)
		{
			Vector2 delta = pev.delta;
			pev.dragging = false;
			return new Vector2?(delta);
		}

		public static PointerEventData GetOriginalPointerEventDataWithPointerDragGO(GameObject pointerDragGOToLookFor)
		{
			if (EventSystem.current.currentInputModule == null)
			{
				return null;
			}
			PointerInputModule pointerInputModule = EventSystem.current.currentInputModule as PointerInputModule;
			if (pointerInputModule == null)
			{
				throw new InvalidOperationException("currentInputModule is not a PointerInputModule");
			}
			ISRIAPointerInputModule isriapointerInputModule = pointerInputModule as ISRIAPointerInputModule;
			Dictionary<int, PointerEventData> dictionary;
			if (isriapointerInputModule == null)
			{
				dictionary = pointerInputModule.GetType().GetField("m_PointerData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pointerInputModule) as Dictionary<int, PointerEventData>;
			}
			else
			{
				dictionary = isriapointerInputModule.GetPointerEventData();
			}
			foreach (PointerEventData pointerEventData in dictionary.Values)
			{
				if (pointerEventData.pointerDrag == pointerDragGOToLookFor)
				{
					return pointerEventData;
				}
			}
			return null;
		}

		public static Color GetRandomColor(bool fullAlpha = false)
		{
			return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), (!fullAlpha) ? Random.Range(0f, 1f) : 1f);
		}
	}
}
