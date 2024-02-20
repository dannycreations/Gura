using System;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PremiumDashboardMonitoring : MonoBehaviour
{
	private void Start()
	{
		this.MoneyBuyText.text = ScriptLocalization.Get("BuyMoneyButtonCaption");
	}

	private void Update()
	{
		if (!this._inited && StaticUserData.PremiumSalesAvailable != null)
		{
			this._inited = true;
			if (StaticUserData.PremiumSalesAvailable.Any((SaleFlags x) => x.HasSales))
			{
				this.ShopMenu.GetComponent<ChangeColor>().UseColor[1] = new Color(0.92941177f, 0.78431374f, 0.3882353f);
				this.ShopMenu.GetComponent<ChangeColor>().SetColor(1);
				if (StaticUserData.PremiumSalesAvailable.Any((SaleFlags x) => x.HasSales && x.ProductTypeId == 1))
				{
					this.ButtonSilver.UseColor[1] = new Color(0.92941177f, 0.78431374f, 0.3882353f);
					this.ButtonSilver.SetColor(1);
					this.MoneyBuyText.text = ScriptLocalization.Get("SaleButtonCaption");
				}
			}
		}
	}

	public GameObject ShopMenu;

	public ChangeColorOther ButtonSilver;

	public Text MoneyBuyText;

	private bool _inited;
}
