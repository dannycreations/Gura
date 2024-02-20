using System;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class PremiumShopHandler : MonoBehaviour
{
	private void Start()
	{
		if (!this.PremiumShopPage.GetComponent<AlphaFade>().IsShowing)
		{
			this.PremiumShopPage.GetComponent<AlphaFade>().FastHidePanel();
		}
	}

	private void OnEnable()
	{
		this.SetPremiumSalesAvailable();
	}

	public void SetPremiumSalesAvailable()
	{
		if (StaticUserData.PremiumSalesAvailable != null)
		{
			GameObject discountImage = this.DiscountImage;
			bool flag;
			if (!DashboardTabSetter.IsNewPremShopEnabled)
			{
				flag = StaticUserData.PremiumSalesAvailable.Any((SaleFlags x) => x.HasSales);
			}
			else
			{
				flag = false;
			}
			discountImage.SetActive(flag);
		}
	}

	public void Click()
	{
		PhotonConnectionFactory.Instance.CaptureActionInStats("OpenGameScreen", FormsEnum.PremiumShop.ToString(), (!DashboardTabSetter.IsNewPremShopEnabled) ? "old" : "new", "1");
		ShopMainPageHandler.OpenPremiumShop();
	}

	public GameObject PremiumShopPage;

	public GameObject DiscountImage;
}
