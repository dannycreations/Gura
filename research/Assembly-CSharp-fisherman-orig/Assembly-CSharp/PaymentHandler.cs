using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class PaymentHandler : MonoBehaviour
{
	public void PaymentOpenClick()
	{
		ShopMainPageHandler.OpenPremiumShop();
	}

	public void MainPageOpenClick(bool isOn)
	{
		if (isOn)
		{
			ShopMainPageHandler.OpenMainShopPage();
		}
	}

	private static MenuHelpers helpers = new MenuHelpers();
}
