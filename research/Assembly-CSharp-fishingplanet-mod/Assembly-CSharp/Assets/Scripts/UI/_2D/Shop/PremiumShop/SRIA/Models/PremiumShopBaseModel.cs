using System;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models
{
	public abstract class PremiumShopBaseModel
	{
		public PremiumShopBaseModel()
		{
			this.CachedType = base.GetType();
		}

		public Type CachedType { get; private set; }

		public virtual int GetHeight()
		{
			return this.Height;
		}

		public int ItemId;

		public Vector4 Margin = Vector4.zero;

		public int Height;
	}
}
