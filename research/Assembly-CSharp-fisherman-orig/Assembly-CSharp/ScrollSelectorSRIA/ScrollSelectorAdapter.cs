using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;

namespace ScrollSelectorSRIA
{
	public class ScrollSelectorAdapter : SRIA<MyPParams, MyItemViewsHolder>
	{
		protected override void Start()
		{
			ActivityState parentActivityState = ActivityState.GetParentActivityState(base.transform);
			if (parentActivityState != null)
			{
				this._cg = parentActivityState.CanvasGroup;
			}
			base.Start();
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			FishSelectorElement fishSelectorElement = this._Params.Data[newOrRecycled.ItemIndex] as FishSelectorElement;
			newOrRecycled.Text.text = fishSelectorElement.Text;
			newOrRecycled.Text.fontStyle = ((!fishSelectorElement.IsSelected) ? 0 : 1);
			ShortcutExtensions.DOFade(newOrRecycled.SelectorIcon, (!fishSelectorElement.IsSelected) ? 0f : 1f, 0f);
		}

		public IEnumerator Init(List<IScrollSelectorElement> data, int index, float normalizedOffset)
		{
			while (!base.Initialized)
			{
				yield return null;
			}
			this._Params.Data = data;
			this.ResetItems(this._Params.Data.Count, false, false);
			this.RebuildLayoutDueToScrollViewSizeChange();
			this.ScrollTo(index, normalizedOffset, 0.5f);
			yield break;
		}

		private CanvasGroup _cg;

		public UINavigation rootNavigation;
	}
}
