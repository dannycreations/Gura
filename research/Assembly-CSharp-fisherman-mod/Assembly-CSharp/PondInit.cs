using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

public class PondInit : MonoBehaviour
{
	internal void Start()
	{
		TutorialSlidesController.FishCounter = 0;
		PhotonConnectionFactory.Instance.OnGotItemCategories += this.OnGotItemCategories;
		if (StaticUserData.AllCategories == null || StaticUserData.AllCategories.Count == 0)
		{
			PhotonConnectionFactory.Instance.GetItemCategories(false);
		}
		PhotonConnectionFactory.Instance.OnCheckedForProductSales += this.PhotonServer_OnCheckedForProductSales;
		PhotonConnectionFactory.Instance.OnCheckForProductSalesFailed += this.PhotonServer_OnCheckForProductSalesFailed;
		PhotonConnectionFactory.Instance.CheckForProductSales();
		if (StaticUserData.FireworkItems == null)
		{
			PhotonConnectionFactory.Instance.OnGotItemsForCategory += this.OnGotItems;
			PhotonConnectionFactory.Instance.GetGlobalItemsFromCategory(new int[] { 91 }, true);
		}
		if (StaticUserData.CurrentLocation != null)
		{
			if (this._pondHelpers.PondPrefabsList.loadingFormAS != null)
			{
				this._pondHelpers.PondPrefabsList.loadingFormAS.Show(true);
			}
			if (PhotonConnectionFactory.Instance.Game != null)
			{
				PhotonConnectionFactory.Instance.Game.Pause();
			}
			Transform component = GameObject.Find(StaticUserData.CurrentLocation.Asset).GetComponent<Transform>();
			GameFactory.Player.Move(component);
			if (string.IsNullOrEmpty(GameFactory.SkyControllerInstance.CurrentSky.AssetBundleName))
			{
				TimeAndWeatherManager.ForceUpdateTime();
				GameFactory.SkyControllerInstance.ChangedSky += this.TransferToLocation_ChangedSky;
				base.StartCoroutine(GameFactory.SkyControllerInstance.SetFirstSky());
			}
			else
			{
				this.TransferToLocation_ChangedSky(null, null);
			}
		}
		if (StaticUserData.CurrentPond != null && !StaticUserData.IS_IN_TUTORIAL)
		{
			if (this._pondHelpers.PondPrefabsList.loadingFormAS != null && this._pondHelpers.PondPrefabsList.loadingFormAS.isActive)
			{
				this._pondHelpers.PondPrefabsList.loadingFormAS.Hide(false);
			}
			if (this._pondHelpers.PondControllerList.PondMainMenu != null)
			{
				this._pondHelpers.PondControllerList.PondMainMenu.SetActive(true);
			}
			if (this._pondHelpers.PondPrefabsList.topMenuFormAS != null)
			{
				this._pondHelpers.PondPrefabsList.topMenuFormAS.Show(false);
			}
			StaticUserData.IsShowDashboard = true;
			if (this._pondHelpers.PondPrefabsList.globalMapForm != null)
			{
				this._pondHelpers.PondPrefabsList.globalMapFormAS.Show(false);
				this._menuhelpers.MenuPrefabsList.currentActiveForm = this._menuhelpers.MenuPrefabsList.globalMapForm;
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalMap, GameScreenTabType.Undefined, null, null, null, null, null);
			}
			if (this._pondHelpers.PondPrefabsList.helpPanel != null)
			{
				this._pondHelpers.PondPrefabsList.helpPanelAS.Show(false);
			}
			this._pondHelpers.PondControllerList.HideGame();
		}
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotItemCategories -= this.OnGotItemCategories;
	}

	public void OnGotItemCategories(List<InventoryCategory> categories)
	{
		StaticUserData.AllCategories = categories;
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

	private void TransferToLocation_ChangedSky(object sender, EventArgs e)
	{
		GameFactory.SkyControllerInstance.ChangedSky -= this.TransferToLocation_ChangedSky;
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			TransferToLocation.Instance.ActivateGameView();
		}
	}

	private void OnGotItems(List<InventoryItem> items)
	{
		if (StaticUserData.FireworkItems == null)
		{
			StaticUserData.FireworkItems = items;
		}
		PhotonConnectionFactory.Instance.OnGotItemsForCategory -= this.OnGotItems;
	}

	private PondHelpers _pondHelpers = new PondHelpers();

	private MenuHelpers _menuhelpers = new MenuHelpers();

	private const int ItemSubscriberId = 1;
}
