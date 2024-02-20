using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuyProductManager
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action<int> OnPurchaseSuccessful;

	public static void InitiateProductPurchase(StoreProduct product)
	{
		if (StaticUserData.IsSteam)
		{
			if (product.TypeId == 2)
			{
				SteamFriends.ActivateGameOverlayToWebPage(product.ExternalShopLink);
			}
			else if (product.TypeId == 1 || product.TypeId == 3 || product.TypeId == 4)
			{
				BuyProductManager.BuyInSteam(product.ProductId);
			}
			else
			{
				BuyProductManager.BuyService(product);
			}
		}
	}

	public static void Dispose()
	{
	}

	private static void BuyService(StoreProduct product)
	{
		BuyClick.Buy(product);
	}

	private static void BuyInStandalone(int productId)
	{
		if (BuyProductManager._urlType == BuyProductManager.UrlTypes.BuySubscription)
		{
			Application.OpenURL(PhotonConnectionFactory.Instance.BuySubscriptionUrl(productId));
		}
		else if (BuyProductManager._urlType == BuyProductManager.UrlTypes.BuyMoney)
		{
			Application.OpenURL(PhotonConnectionFactory.Instance.BuyMoneyUrl(productId));
		}
	}

	private static void BuyInSteam(int productId)
	{
		if (!SteamUtils.IsOverlayEnabled())
		{
			MessageBox messageBox = BuyProductManager._helpers.ShowMessage(null, ScriptLocalization.Get("SteamOverlayNotFoundCaption"), ScriptLocalization.Get("SteamOverlayNotFoundMessage"), false, true, false, null);
			messageBox.OpenFast();
			return;
		}
		if (StaticUserData.ClientType == ClientTypes.SteamWindows)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		}
		BuyProductManager.StartTransaction(productId);
	}

	private static void StartTransaction(int productId)
	{
		BuyProductManager._helpers.MenuPrefabsList.topMenuForm.transform.Find("Overlay").gameObject.SetActive(true);
		BuyProductManager._txnResponseCallback = Callback<MicroTxnAuthorizationResponse_t>.Create(new Callback<MicroTxnAuthorizationResponse_t>.DispatchDelegate(BuyProductManager.SteamTransactionCallback));
		BuyProductManager.SubscribeToStartSteamTransactionEvents();
		PhotonConnectionFactory.Instance.StartSteamTransaction(productId, BuyProductManager.GetMyIp());
	}

	private static string GetMyIp()
	{
		return string.Empty;
	}

	private static void SteamTransactionCallback(MicroTxnAuthorizationResponse_t result)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		BuyProductManager._helpers.MenuPrefabsList.topMenuForm.transform.Find("Overlay").gameObject.SetActive(false);
		PhotonConnectionFactory.Instance.OnProductDelivered += BuyProductManager.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnFinalizeSteamTransactionFailed += BuyProductManager.OnFinalizeSteamTransactionFailed;
		PhotonConnectionFactory.Instance.FinalizeSteamTransaction(BuyProductManager._orderId);
		BuyProductManager._txnResponseCallback.Unregister();
	}

	private static void SubscribeToStartSteamTransactionEvents()
	{
		PhotonConnectionFactory.Instance.OnSteamTransactionStarted += BuyProductManager.OnSteamTransactionStarted;
		PhotonConnectionFactory.Instance.OnStartSteamTransactionFailed += BuyProductManager.OnStartSteamTransactionFailed;
	}

	private static void UnsubscribeFromStartSteamTransactionEvents()
	{
		PhotonConnectionFactory.Instance.OnSteamTransactionStarted -= BuyProductManager.OnSteamTransactionStarted;
		PhotonConnectionFactory.Instance.OnStartSteamTransactionFailed -= BuyProductManager.OnStartSteamTransactionFailed;
	}

	private static void OnSteamTransactionStarted(string orderId)
	{
		BuyProductManager.UnsubscribeFromStartSteamTransactionEvents();
		BuyProductManager._orderId = orderId;
	}

	private static void OnStartSteamTransactionFailed(Failure failure)
	{
		BuyProductManager.UnsubscribeFromStartSteamTransactionEvents();
		Debug.LogError(failure.FullErrorInfo);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		BuyProductManager._helpers.MenuPrefabsList.topMenuForm.transform.Find("Overlay").gameObject.SetActive(false);
	}

	private static void OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		PhotonConnectionFactory.Instance.OnProductDelivered -= BuyProductManager.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnFinalizeSteamTransactionFailed -= BuyProductManager.OnFinalizeSteamTransactionFailed;
	}

	private static void OnFinalizeSteamTransactionFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnProductDelivered -= BuyProductManager.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnFinalizeSteamTransactionFailed -= BuyProductManager.OnFinalizeSteamTransactionFailed;
		Debug.LogError(failure.FullErrorInfo);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		BuyProductManager._helpers.MenuPrefabsList.topMenuForm.transform.Find("Overlay").gameObject.SetActive(false);
	}

	// Note: this type is marked as 'beforefieldinit'.
	static BuyProductManager()
	{
		BuyProductManager.OnPurchaseSuccessful = delegate
		{
		};
		BuyProductManager._helpers = new MenuHelpers();
		BuyProductManager._urlType = BuyProductManager.UrlTypes.BuySubscription;
	}

	private static MenuHelpers _helpers;

	private static Callback<MicroTxnAuthorizationResponse_t> _txnResponseCallback;

	private static BuyProductManager.UrlTypes _urlType;

	private static string _orderId;

	public enum UrlTypes : byte
	{
		BuySubscription,
		BuyMoney
	}
}
