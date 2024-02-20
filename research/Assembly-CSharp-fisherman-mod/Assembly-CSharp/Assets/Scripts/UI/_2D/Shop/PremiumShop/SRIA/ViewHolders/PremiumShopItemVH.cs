using System;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using frame8.Logic.Misc.Other.Extensions;
using TMPro;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.ViewHolders
{
	public class PremiumShopItemVH : HeaderPremiumShopGroupVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("Description", out this._description);
			this.root.GetComponentAtPath("Image", out this._image);
		}

		internal override void UpdateViews(PremiumShopBaseModel model)
		{
			base.UpdateViews(model);
			PremiumShopItemModel premiumShopItemModel = model as PremiumShopItemModel;
			this._description.text = premiumShopItemModel.Description;
			this._imageLdbl.Load(premiumShopItemModel.ImageBID, this._image, "Textures/Inventory/{0}");
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(PremiumShopItemModel);
		}

		private TextMeshProUGUI _description;

		private Image _image;

		private ResourcesHelpers.AsyncLoadableImage _imageLdbl = new ResourcesHelpers.AsyncLoadableImage();
	}
}
