using System;
using UnityEngine;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public interface ISRIA : IScrollRectProxy
	{
		event Action<int, int> ItemsRefreshed;

		bool Initialized { get; }

		BaseParams BaseParameters { get; }

		MonoBehaviour AsMonoBehaviour { get; }

		double ContentVirtualSizeToViewportRatio { get; }

		double ContentVirtualInsetFromViewportStart { get; }

		double ContentVirtualInsetFromViewportEnd { get; }

		int VisibleItemsCount { get; }

		int RecyclableItemsCount { get; }

		bool IsDragging { get; }

		void ChangeItemsCount(ItemCountChangeMode changeMode, int itemsCount, int indexIfAppendingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false);

		void SetVirtualAbstractNormalizedScrollPosition(double pos, bool computeVisibilityNow);

		int GetItemsCount();

		void ScrollTo(int itemIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f);

		bool SmoothScrollTo(int itemIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, bool overrideCurrentScrollingAnimation = false);

		AbstractViewsHolder GetViewsHolderOfClosestItemToViewportPoint(float viewportPoint01, float itemPoint01, out float distance);
	}
}
