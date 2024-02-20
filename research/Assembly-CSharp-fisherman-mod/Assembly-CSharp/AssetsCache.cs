using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ObjectModel;
using SharedLib.Game;
using UnityEngine;

public class AssetsCache : MonoBehaviour
{
	internal void Start()
	{
		base.StartCoroutine(this.InitInventoryItemCache());
	}

	public void SubscribeEvents()
	{
	}

	public void UnsubscribeEvents()
	{
	}

	public bool AllAssetsChachesInited
	{
		get
		{
			return this._fishAssets != null && this._itemAssets != null && this._globalVariables != null;
		}
	}

	private IEnumerator InitInventoryItemCache()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || CacheLibrary.MapCache.AllLicenses == null)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotInventoryItemAssets += this.Instance_OnGotInventoryItemAssets;
		PhotonConnectionFactory.Instance.OnGettingInventoryItemAssetsFailed += this.Instance_OnGettingInventoryItemAssetsFailed;
		PhotonConnectionFactory.Instance.GetInventoryItemAssets();
		yield break;
	}

	private void Instance_OnGotFishAssets(IEnumerable<FishAssetInfo> fishAssets)
	{
		PhotonConnectionFactory.Instance.OnGotFishAssets -= this.Instance_OnGotFishAssets;
		foreach (FishAssetInfo fishAssetInfo in fishAssets)
		{
			this._fishAssets[fishAssetInfo.FishId] = fishAssetInfo.Asset;
		}
		base.StartCoroutine(this.InitGlobalVariablesCache());
		ManagerScenes.ProgressOfLoad += 0.08f;
	}

	private IEnumerator InitGlobalVariablesCache()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || CacheLibrary.MapCache.AllLicenses == null)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotGlobalVariables += this.Instance_OnGotGlobalVariables;
		PhotonConnectionFactory.Instance.OnGettingGlobalVariablesFailed += this.Instance_OnGettingGlobalVariablesFailed;
		PhotonConnectionFactory.Instance.GetGlobalVariables();
		yield break;
	}

	private void Instance_OnGotInventoryItemAssets(IEnumerable<ItemAssetInfo> itemAssets)
	{
		PhotonConnectionFactory.Instance.OnGotInventoryItemAssets -= this.Instance_OnGotInventoryItemAssets;
		foreach (ItemAssetInfo itemAssetInfo in itemAssets)
		{
			this._itemAssets[itemAssetInfo.ItemId] = itemAssetInfo;
		}
		base.StartCoroutine(this.InitFishCache());
		ManagerScenes.ProgressOfLoad += 0.08f;
	}

	private IEnumerator InitFishCache()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || CacheLibrary.MapCache.AllLicenses == null)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotFishAssets += this.Instance_OnGotFishAssets;
		PhotonConnectionFactory.Instance.OnGettingFishAssetsFailed += this.Instance_OnGettingFishAssetsFailed;
		PhotonConnectionFactory.Instance.GetFishAssets();
		yield break;
	}

	private void Instance_OnGotGlobalVariables(Hashtable vars)
	{
		PhotonConnectionFactory.Instance.OnGotGlobalVariables -= this.Instance_OnGotGlobalVariables;
		this._globalVariables = vars;
		if (vars.ContainsKey("IsRetail"))
		{
			Inventory.IsRetail = (bool)vars["IsRetail"];
		}
		if (vars.ContainsKey("ExchangeRate"))
		{
			Inventory.ExchangeRate = () => (float)vars["ExchangeRate"];
		}
		if (vars.ContainsKey("PondDayStart"))
		{
			InGameTimeHelper.Init((int)vars["PondDayStart"]);
		}
		if (vars.ContainsKey("DefaultInventoryCapacity"))
		{
			Inventory.DefaultInventoryCapacity = (int)vars["DefaultInventoryCapacity"];
		}
		if (vars.ContainsKey("MaxInventoryCapacity"))
		{
			Inventory.MaxInventoryCapacity = (int)vars["MaxInventoryCapacity"];
		}
		if (vars.ContainsKey("DefaultRodSetupCapacity"))
		{
			Inventory.DefaultRodSetupCapacity = (int)vars["DefaultRodSetupCapacity"];
		}
		if (vars.ContainsKey("MaxRodSetupCapacity"))
		{
			Inventory.MaxRodSetupCapacity = (int)vars["MaxRodSetupCapacity"];
		}
		if (vars.ContainsKey("DefaultBuoyCapacity"))
		{
			Inventory.DefaultBuoyCapacity = (int)vars["DefaultBuoyCapacity"];
		}
		if (vars.ContainsKey("MaxBuoyCapacity"))
		{
			Inventory.MaxBuoyCapacity = (int)vars["MaxBuoyCapacity"];
		}
		if (vars.ContainsKey("DefaultChumRecipesCapacity"))
		{
			Inventory.DefaultChumRecipesCapacity = (int)vars["DefaultChumRecipesCapacity"];
		}
		if (vars.ContainsKey("MaxChumRecipesCapacity"))
		{
			Inventory.MaxChumRecipesCapacity = (int)vars["MaxChumRecipesCapacity"];
		}
		if (vars.ContainsKey("MixBasePossiblePercentageMin"))
		{
			Inventory.MixBasePossiblePercentageMin = (float)vars["MixBasePossiblePercentageMin"];
		}
		if (vars.ContainsKey("MixAromaPossiblePercentage"))
		{
			Inventory.MixAromaPossiblePercentage = (float)vars["MixAromaPossiblePercentage"];
		}
		if (vars.ContainsKey("MixParticlePossiblePercentage"))
		{
			Inventory.MixParticlePossiblePercentage = (float)vars["MixParticlePossiblePercentage"];
		}
		if (vars.ContainsKey("SilverRepairRate"))
		{
			Inventory.SilverRepairRate = (float)vars["SilverRepairRate"];
		}
		if (vars.ContainsKey("GoldRepairRate"))
		{
			Inventory.GoldRepairRate = (float)vars["GoldRepairRate"];
		}
		if (vars.ContainsKey("ChumGroundbaitsMixTime"))
		{
			Inventory.ChumGroundbaitsMixTime = (float)vars["ChumGroundbaitsMixTime"];
		}
		if (vars.ContainsKey("ChumCarpbaitsMixTime"))
		{
			Inventory.ChumCarpbaitsMixTime = (float)vars["ChumCarpbaitsMixTime"];
		}
		if (vars.ContainsKey("ChumMethodMixMixTime"))
		{
			Inventory.ChumMethodMixMixTime = (float)vars["ChumMethodMixMixTime"];
		}
		if (vars.ContainsKey("ChumGroundbaitsMixTimePremium"))
		{
			Inventory.ChumGroundbaitsMixTimePremium = (float)vars["ChumGroundbaitsMixTimePremium"];
		}
		if (vars.ContainsKey("ChumCarpbaitsMixTimePremium"))
		{
			Inventory.ChumCarpbaitsMixTimePremium = (float)vars["ChumCarpbaitsMixTimePremium"];
		}
		if (vars.ContainsKey("ChumMethodMixMixTimePremium"))
		{
			Inventory.ChumMethodMixMixTimePremium = (float)vars["ChumMethodMixMixTimePremium"];
		}
		if (vars.ContainsKey("ChumHandCapacity"))
		{
			Inventory.ChumHandCapacity = (float)vars["ChumHandCapacity"];
		}
		if (vars.ContainsKey("ChumSplitTime"))
		{
			Inventory.ChumSplitTime = (float)vars["ChumSplitTime"];
		}
		if (vars.ContainsKey("ChumSplitTimePremium"))
		{
			Inventory.ChumSplitTimePremium = (float)vars["ChumSplitTimePremium"];
		}
		if (vars.ContainsKey("ChumSplitTimeHands"))
		{
			Inventory.ChumSplitTimeHands = (float)vars["ChumSplitTimeHands"];
		}
		if (vars.ContainsKey("ChumSplitTimeHandsPremium"))
		{
			Inventory.ChumSplitTimeHandsPremium = (float)vars["ChumSplitTimeHandsPremium"];
		}
		if (vars.ContainsKey("ChumHandMaxCastLength"))
		{
			Inventory.ChumHandMaxCastLength = (float)vars["ChumHandMaxCastLength"];
		}
		if (vars.ContainsKey("UgcMaxDaysScheduledFuture"))
		{
			UserCompetitionPublic.UgcMaxDaysScheduledFuture = (int)vars["UgcMaxDaysScheduledFuture"];
		}
		if (vars.ContainsKey("UgcCompetitionFeeCommission"))
		{
			UserCompetitionPublic.UgcCompetitionFeeCommission = (int)vars["UgcCompetitionFeeCommission"];
		}
		if (vars.ContainsKey("UgcTicksCountBeforeStart"))
		{
			UserCompetitionPublic.UgcTicksCountBeforeStart = (int)vars["UgcTicksCountBeforeStart"];
		}
		if (vars.ContainsKey("LevelCap"))
		{
			Profile.LevelCap = (int)vars["LevelCap"];
		}
		ManagerScenes.ProgressOfLoad += 0.08f;
	}

	private void Instance_OnGettingFishAssetsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGettingFishAssetsFailed -= this.Instance_OnGettingFishAssetsFailed;
		throw new Exception("GetFishAssets is failed");
	}

	private void Instance_OnGettingInventoryItemAssetsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGettingInventoryItemAssetsFailed -= this.Instance_OnGettingInventoryItemAssetsFailed;
		throw new Exception("GetInventoryItemAssets is failed");
	}

	private void Instance_OnGettingGlobalVariablesFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGettingGlobalVariablesFailed -= this.Instance_OnGettingGlobalVariablesFailed;
		throw new Exception("GetGlobalVariables is failed");
	}

	public string GetFishAssetPath(int fishId)
	{
		return (!this._fishAssets.ContainsKey(fishId)) ? null : this._fishAssets[fishId];
	}

	public ItemAssetInfo GetItemAssetPath(int itemId)
	{
		return (!this._itemAssets.ContainsKey(itemId)) ? null : this._itemAssets[itemId];
	}

	public Hashtable GlobalVariables
	{
		get
		{
			return this._globalVariables;
		}
	}

	private Dictionary<int, string> _fishAssets = new Dictionary<int, string>();

	private Dictionary<int, ItemAssetInfo> _itemAssets = new Dictionary<int, ItemAssetInfo>();

	private Hashtable _globalVariables;
}
