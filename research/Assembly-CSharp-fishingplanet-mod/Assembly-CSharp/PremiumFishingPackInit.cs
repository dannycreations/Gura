using System;
using UnityEngine;

public class PremiumFishingPackInit : PremiumItemBase
{
	public override void Init(PremiumCategoryManager.PremiumItem p)
	{
		base.Init(p);
		if (p.WhatsNew != null && p.WhatsNew.End != null)
		{
			this.TimeRemain = p.WhatsNew.End.Value - TimeHelper.UtcTime();
			this.HasRemain = this.TimeRemain.TotalSeconds > 0.0;
		}
	}

	public override void Select(bool isCaptureAction)
	{
		base.Select(isCaptureAction);
		this.ShineGo.SetActive(true);
	}

	public override void Deselect()
	{
		base.Deselect();
		this.ShineGo.SetActive(false);
	}

	public override string GetImageHeaderPath()
	{
		return string.Format("Textures/Inventory/{0}", base.Product.ItemListImageBID);
	}

	public override string GetDescription()
	{
		return (base.Product == null) ? string.Empty : base.Product.Desc;
	}

	protected override Vector3 GetSelectedScale
	{
		get
		{
			return this._selectedScale;
		}
	}

	protected override void SetCurrency()
	{
		this.CurrencyName.text = base.Product.Name;
	}

	protected override void SetPrice()
	{
	}

	protected override bool IsChangeColor()
	{
		return false;
	}

	[SerializeField]
	private GameObject ShineGo;

	private readonly Vector3 _selectedScale = new Vector3(1.025f, 1.025f, 1f);
}
