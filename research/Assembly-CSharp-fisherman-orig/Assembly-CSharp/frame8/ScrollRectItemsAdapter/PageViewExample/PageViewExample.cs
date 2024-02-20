using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.PageViewExample
{
	public class PageViewExample : SRIA<MyParams, PageViewsHolder>
	{
		protected override void Start()
		{
			base.Start();
			DrawerCommandPanel.Instance.Init(this, true, false, false, false, false);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.simulateLowEndDeviceSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.galleryEffectSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.OnItemCountChangeRequested;
			base.GetComponentInChildren<DiscreteScrollbar>().getItemsCountFunc = () => this._Params.Data.Count;
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		protected override PageViewsHolder CreateViewsHolder(int itemIndex)
		{
			PageViewsHolder pageViewsHolder = new PageViewsHolder();
			pageViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return pageViewsHolder;
		}

		protected override void UpdateViewsHolder(PageViewsHolder newOrRecycled)
		{
			PageModel pageModel = this._Params.Data[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateViews(pageModel);
		}

		public void ScrollToPage(int index)
		{
			this.SmoothScrollTo(index, 0.7f, 0.5f, 0.5f, null, false);
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			this._Params.Data.Clear();
			for (int i = 0; i < newCount; i++)
			{
				this._Params.Data.Add(this.CreateNewModel(i, C.GetRandomTextBody(180, int.MaxValue), Random.Range(0, this._Params.availableImages.Length)));
			}
			this.ResetItems(this._Params.Data.Count, false, false);
		}

		private PageModel CreateNewModel(int itemIdex, string body, int iconIndex)
		{
			return new PageModel
			{
				title = "Page " + itemIdex,
				body = body,
				image = this._Params.availableImages[iconIndex]
			};
		}
	}
}
