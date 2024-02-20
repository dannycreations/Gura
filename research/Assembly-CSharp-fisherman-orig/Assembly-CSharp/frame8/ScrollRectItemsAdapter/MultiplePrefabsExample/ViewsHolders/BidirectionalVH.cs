using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.ViewsHolders
{
	public class BidirectionalVH : BaseVH
	{
		public SliderItemBehaviour sliderBehaviour { get; private set; }

		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("TitleText", out this.titleText);
			this.sliderBehaviour = this.root.GetComponent<SliderItemBehaviour>();
			this.sliderBehaviour.ValueChanged += this.SliderBehaviour_ValueChanged;
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(BidirectionalModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			this._Model = null;
			BidirectionalModel bidirectionalModel = model as BidirectionalModel;
			this.sliderBehaviour.Value = bidirectionalModel.value;
			this._Model = bidirectionalModel;
		}

		private void SliderBehaviour_ValueChanged(float newValue)
		{
			if (this._Model != null)
			{
				this._Model.value = newValue;
			}
		}

		private BidirectionalModel _Model;
	}
}
