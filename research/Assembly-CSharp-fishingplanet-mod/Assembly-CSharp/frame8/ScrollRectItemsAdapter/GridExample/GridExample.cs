using System;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.GridExample
{
	public class GridExample : GridAdapter<GridParams, MyCellViewsHolder>, ILazyListSimpleDataManager<BasicModel>
	{
		public LazyList<BasicModel> Data { get; private set; }

		protected override void Awake()
		{
			this.Data = new LazyList<BasicModel>(new Func<int, BasicModel>(this.CreateNewModel), 0);
			base.Awake();
		}

		protected override void Start()
		{
			DrawerCommandPanel.Instance.Init(this, true, false, true, false, false);
			base.Start();
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0.3f;
			DrawerCommandPanel.Instance.ItemCountChangeRequested += new Action<int>(base.LazySetNewItems<BasicModel>);
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		public override void Refresh(bool contentPanelEndEdgeStationary, bool keepVelocity = false)
		{
			this._CellsCount = this.Data.Count;
			base.Refresh(DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, keepVelocity);
		}

		protected override void UpdateCellViewsHolder(MyCellViewsHolder viewsHolder)
		{
			BasicModel model = this.Data[viewsHolder.ItemIndex];
			viewsHolder.title.text = "Loading";
			viewsHolder.overlayImage.color = Color.white;
			int itemIndexAtRequest = viewsHolder.ItemIndex;
			string imageURLAtRequest = model.imageURL;
			viewsHolder.iconRemoteImageBehaviour.Load(imageURLAtRequest, true, delegate(bool fromCache, bool success)
			{
				if (!this.IsRequestStillValid(viewsHolder.ItemIndex, itemIndexAtRequest, imageURLAtRequest))
				{
					return;
				}
				viewsHolder.overlayImage.CrossFadeAlpha(0f, 0.5f, false);
				viewsHolder.title.text = model.title;
			}, null);
		}

		private bool IsRequestStillValid(int itemIndex, int itemIdexAtRequest, string imageURLAtRequest)
		{
			return this._CellsCount > itemIndex && itemIdexAtRequest == itemIndex && imageURLAtRequest == this.Data[itemIndex].imageURL;
		}

		private BasicModel CreateNewModel(int index)
		{
			return new BasicModel
			{
				title = "Item " + index,
				imageURL = C.GetRandomSmallImageURL()
			};
		}
	}
}
