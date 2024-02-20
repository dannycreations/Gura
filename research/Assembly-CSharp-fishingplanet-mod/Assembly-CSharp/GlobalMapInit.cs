using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class GlobalMapInit : MonoBehaviour
{
	internal void Awake()
	{
		PhotonConnectionFactory.Instance.OnCheckedForProductSales += this.PhotonServer_OnCheckedForProductSales;
		PhotonConnectionFactory.Instance.OnCheckForProductSalesFailed += this.PhotonServer_OnCheckForProductSalesFailed;
		PhotonConnectionFactory.Instance.OnGotItemCategories += this.OnGotItemCategories;
		PhotonConnectionFactory.Instance.CheckForProductSales();
		PhotonConnectionFactory.Instance.GetItemCategories(false);
		MenuPrefabsList component = base.GetComponent<MenuPrefabsList>();
		if (component != null && !component.Inited)
		{
			component.Awake();
		}
		if (component.helpPanelAS != null)
		{
			component.helpPanelAS.Show(false);
		}
		component.globalMapForm.SetActive(true);
		component.globalMapFormAS.Show(false);
		UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
		component.topMenuForm.SetActive(true);
		component.topMenuFormAS.Show(false);
		SysInfoHelper.SendSystemInfo();
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnCheckedForProductSales -= this.PhotonServer_OnCheckedForProductSales;
		PhotonConnectionFactory.Instance.OnCheckForProductSalesFailed -= this.PhotonServer_OnCheckForProductSalesFailed;
		PhotonConnectionFactory.Instance.OnGotItemCategories -= this.OnGotItemCategories;
	}

	public void RefreshPonds()
	{
	}

	private void PhotonServer_OnCheckForProductSalesFailed(Failure failure)
	{
		StaticUserData.PremiumSalesAvailable = new List<SaleFlags>();
	}

	private void PhotonServer_OnCheckedForProductSales(List<SaleFlags> items)
	{
		StaticUserData.PremiumSalesAvailable = items ?? new List<SaleFlags>();
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.SetPremiumSalesAvailable();
		}
	}

	public void OnGotItemCategories(List<InventoryCategory> categories)
	{
		StaticUserData.AllCategories = categories;
	}
}
