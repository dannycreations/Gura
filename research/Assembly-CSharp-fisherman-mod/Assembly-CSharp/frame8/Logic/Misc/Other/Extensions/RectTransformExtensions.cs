using System;
using System.Collections.Generic;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class RectTransformExtensions
	{
		public static float GetWorldTop(this RectTransform rt)
		{
			return rt.position.y + (1f - rt.pivot.y) * rt.rect.height;
		}

		public static float GetWorldBottom(this RectTransform rt)
		{
			return rt.position.y - rt.pivot.y * rt.rect.height;
		}

		public static float GetWorldLeft(this RectTransform rt)
		{
			return rt.position.x - rt.pivot.x * rt.rect.width;
		}

		public static float GetWorldRight(this RectTransform rt)
		{
			return rt.position.x + (1f - rt.pivot.x) * rt.rect.width;
		}

		public static float GetWorldSignedHorDistanceBetweenCustomPivots(this RectTransform rt, float customPivotOnThisRect01, RectTransform other, float customPivotOnOtherRect01)
		{
			float num = rt.GetWorldRight() - (1f - customPivotOnThisRect01) * rt.rect.width;
			float num2 = other.GetWorldRight() - (1f - customPivotOnOtherRect01) * other.rect.width;
			return num2 - num;
		}

		public static float GetWorldSignedVertDistanceBetweenCustomPivots(this RectTransform rt, float customPivotOnThisRect01, RectTransform other, float customPivotOnOtherRect01)
		{
			float num = rt.GetWorldTop() - (1f - customPivotOnThisRect01) * rt.rect.height;
			float num2 = other.GetWorldTop() - (1f - customPivotOnOtherRect01) * other.rect.height;
			return num2 - num;
		}

		public static float GetInsetFromParentTopEdge(this RectTransform child, RectTransform parentHint)
		{
			float num = (1f - parentHint.pivot.y) * parentHint.rect.height;
			float y = child.localPosition.y;
			return num - child.rect.yMax - y;
		}

		public static float GetInsetFromParentBottomEdge(this RectTransform child, RectTransform parentHint)
		{
			float num = parentHint.pivot.y * parentHint.rect.height;
			float y = child.localPosition.y;
			return num + child.rect.yMin + y;
		}

		public static float GetInsetFromParentLeftEdge(this RectTransform child, RectTransform parentHint)
		{
			float num = parentHint.pivot.x * parentHint.rect.width;
			float x = child.localPosition.x;
			return num + child.rect.xMin + x;
		}

		public static float GetInsetFromParentRightEdge(this RectTransform child, RectTransform parentHint)
		{
			float num = (1f - parentHint.pivot.x) * parentHint.rect.width;
			float x = child.localPosition.x;
			return num - child.rect.xMax - x;
		}

		public static float GetInsetFromParentEdge(this RectTransform child, RectTransform parentHint, RectTransform.Edge parentEdge)
		{
			return RectTransformExtensions._GetInsetFromParentEdge_MappedActions[parentEdge](child, parentHint);
		}

		public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newSize)
		{
			RectTransform rectTransform = child.parent as RectTransform;
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(rectTransform, fixedEdge, child.GetInsetFromParentEdge(rectTransform, fixedEdge), newSize);
		}

		public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentHint, RectTransform.Edge fixedEdge, float newSize)
		{
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(parentHint, fixedEdge, child.GetInsetFromParentEdge(parentHint, fixedEdge), newSize);
		}

		public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newInset, float newSize)
		{
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(child.parent as RectTransform, fixedEdge, newInset, newSize);
		}

		public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentHint, RectTransform.Edge fixedEdge, float newInset, float newSize)
		{
			RectTransformExtensions._SetInsetAndSizeFromParentEdgeWithCurrentAnchors_MappedActions[fixedEdge](child, parentHint, newInset, newSize);
		}

		public static void MatchParentSize(this RectTransform rt)
		{
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector3.zero;
			rt.pivot = Vector2.one * 0.5f;
			rt.anchoredPosition = Vector3.zero;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static RectTransformExtensions()
		{
			Dictionary<RectTransform.Edge, Func<RectTransform, RectTransform, float>> dictionary = new Dictionary<RectTransform.Edge, Func<RectTransform, RectTransform, float>>();
			dictionary.Add(3, new Func<RectTransform, RectTransform, float>(RectTransformExtensions.GetInsetFromParentBottomEdge));
			dictionary.Add(2, new Func<RectTransform, RectTransform, float>(RectTransformExtensions.GetInsetFromParentTopEdge));
			dictionary.Add(0, new Func<RectTransform, RectTransform, float>(RectTransformExtensions.GetInsetFromParentLeftEdge));
			dictionary.Add(1, new Func<RectTransform, RectTransform, float>(RectTransformExtensions.GetInsetFromParentRightEdge));
			RectTransformExtensions._GetInsetFromParentEdge_MappedActions = dictionary;
			RectTransformExtensions._SetInsetAndSizeFromParentEdgeWithCurrentAnchors_MappedActions = new Dictionary<RectTransform.Edge, Action<RectTransform, RectTransform, float, float>>
			{
				{
					3,
					delegate(RectTransform child, RectTransform parentHint, float newInset, float newSize)
					{
						float num = newInset - child.GetInsetFromParentBottomEdge(parentHint);
						Vector2 vector;
						vector..ctor(child.offsetMin.x, child.offsetMin.y + num);
						child.offsetMax = new Vector2(child.offsetMax.x, child.offsetMax.y + (newSize - child.rect.height + num));
						child.offsetMin = vector;
					}
				},
				{
					2,
					delegate(RectTransform child, RectTransform parentHint, float newInset, float newSize)
					{
						float num2 = newInset - child.GetInsetFromParentTopEdge(parentHint);
						Vector2 vector2;
						vector2..ctor(child.offsetMax.x, child.offsetMax.y - num2);
						child.offsetMin = new Vector2(child.offsetMin.x, child.offsetMin.y - (newSize - child.rect.height + num2));
						child.offsetMax = vector2;
					}
				},
				{
					0,
					delegate(RectTransform child, RectTransform parentHint, float newInset, float newSize)
					{
						float num3 = newInset - child.GetInsetFromParentLeftEdge(parentHint);
						Vector2 vector3;
						vector3..ctor(child.offsetMin.x + num3, child.offsetMin.y);
						child.offsetMax = new Vector2(child.offsetMax.x + (newSize - child.rect.width + num3), child.offsetMax.y);
						child.offsetMin = vector3;
					}
				},
				{
					1,
					delegate(RectTransform child, RectTransform parentHint, float newInset, float newSize)
					{
						float num4 = newInset - child.GetInsetFromParentRightEdge(parentHint);
						Vector2 vector4;
						vector4..ctor(child.offsetMax.x - num4, child.offsetMax.y);
						child.offsetMin = new Vector2(child.offsetMin.x - (newSize - child.rect.width + num4), child.offsetMin.y);
						child.offsetMax = vector4;
					}
				}
			};
		}

		private static Dictionary<RectTransform.Edge, Func<RectTransform, RectTransform, float>> _GetInsetFromParentEdge_MappedActions;

		private static Dictionary<RectTransform.Edge, Action<RectTransform, RectTransform, float, float>> _SetInsetAndSizeFromParentEdgeWithCurrentAnchors_MappedActions;
	}
}
