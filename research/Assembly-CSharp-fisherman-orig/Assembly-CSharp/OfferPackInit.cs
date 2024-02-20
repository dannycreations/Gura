using System;
using TMPro;
using UnityEngine;

public class OfferPackInit : PremiumItemBase
{
	protected override void Update()
	{
		base.Update();
		if (this.TimeRemain.TotalSeconds > 0.0)
		{
			this._remainTimer.text = this.TimeRemain.GetFormated(true, true);
		}
	}

	public override void Init(PremiumCategoryManager.PremiumItem p)
	{
		this.ProductItem = p;
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", base.Product.PromotionImageBID));
		this.Silvers.text = this.ProductItem.Offer.Name;
		if (this.ProductItem.Offer.LifetimeRemain != null)
		{
			this.HasRemain = this.TimeRemain.TotalSeconds > 0.0;
			this.SetTimeRemain(this.TimeRemain);
		}
		this.InitMain();
	}

	protected override GameObject RemainGo()
	{
		return this._remainGo;
	}

	protected override Vector3 GetSelectedScale
	{
		get
		{
			return this._selectedScale;
		}
	}

	protected override bool IsChangeColor()
	{
		return false;
	}

	[SerializeField]
	private GameObject _remainGo;

	[SerializeField]
	private TextMeshProUGUI _remainTimer;

	private readonly Vector3 _selectedScale = new Vector3(1.0125f, 1.0125f, 1f);
}
