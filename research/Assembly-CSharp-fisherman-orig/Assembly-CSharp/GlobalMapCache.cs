using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using Scripts.Common;
using UnityEngine;

public class GlobalMapCache : MonoBehaviour
{
	public List<State> AllStates
	{
		get
		{
			return this._allStates;
		}
	}

	public MetadataForUserCompetition UgcMetadata { get; protected set; }

	public MetadataForUserCompetitionSearch UgcMetadataForSearch { get; protected set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalMapTournamentsCacheEventArgs> OnTournaments;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalMapFishCacheEventArgs> OnFishes;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalMapPondCacheEventArgs> OnGetPond;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnRefreshedPondsInfo;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalMapLicenseCacheEventArgs> OnUpdateLicenses;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<GlobalMapBoatDescCacheEventArgs> OnGetBoatDescs;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFishesBriefsLoaded = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFishCategoriesLoaded = delegate
	{
	};

	public bool HasFishCategories
	{
		get
		{
			return this._fishCategories != null;
		}
	}

	public bool AllMapChachesInited
	{
		get
		{
			return this.AllLicenses != null && this._pondCache != null && this.FishesBriefs != null && this.UgcMetadata != null && this.UgcMetadataForSearch != null && this.AllStates != null;
		}
	}

	public Dictionary<int, Pond>.ValueCollection CachedPonds
	{
		get
		{
			return this._pondCache.Values;
		}
	}

	public List<FishBrief> FishesLight
	{
		get
		{
			return this.FishesBriefs;
		}
	}

	public List<InventoryItemBrief> InventoryItemLight
	{
		get
		{
			return this.InventoryItemBriefs;
		}
	}

	public bool IsAbTestActive(Constants.AB_TESTS t)
	{
		return this._abTestsSelection.ContainsKey(t) && this._abTestsSelection[t];
	}

	internal void Start()
	{
		this.SubscribeEvents();
		base.StartCoroutine(this.InitGlobalMapCache());
	}

	internal void OnDestroy()
	{
		this.UnsubscribeEvents();
	}

	public void SubscribeEvents()
	{
		PhotonConnectionFactory.Instance.OnGotFishList += this.OnGotFishes;
		PhotonConnectionFactory.Instance.OnErrorGettingFishList += this.Instance_OnErrorGettingFishList;
		PhotonConnectionFactory.Instance.OnGotFishCategories += this.OnGotFishCategories;
		PhotonConnectionFactory.Instance.OnPondLevelInvalidated += this.Instance_OnPondLevelInvalidated;
		PhotonConnectionFactory.Instance.OnPondUnlocked += this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnGotBoats += this.OnGotBoats;
	}

	public void UnsubscribeEvents()
	{
		PhotonConnectionFactory.Instance.OnErrorGettingFishList -= this.Instance_OnErrorGettingFishList;
		PhotonConnectionFactory.Instance.OnGotFishList -= this.OnGotFishes;
		PhotonConnectionFactory.Instance.OnGotFishCategories -= this.OnGotFishCategories;
		PhotonConnectionFactory.Instance.OnPondUnlocked -= this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnGotBoats -= this.OnGotBoats;
	}

	private void PhotonServerOnGotMyTournaments(List<Tournament> tournaments)
	{
	}

	public InventoryItemBrief GetInventoryItemBriefById(int id)
	{
		return (!this.InventoryItemLightDict.ContainsKey(id)) ? null : this.InventoryItemLightDict[id];
	}

	private void Instance_OnErrorGettingFishList(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void InstanceOnGettingTournamentsFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void Instance_OnGotLicenses(IEnumerable<ShopLicense> licenses)
	{
		PhotonConnectionFactory.Instance.OnGotLicenses -= this.Instance_OnGotLicenses;
		this.AllLicenses = licenses.ToList<ShopLicense>();
		ManagerScenes.ProgressOfLoad += 0.1f;
		base.StartCoroutine(this.InitFishesBriefsCache());
	}

	private void Instance_OnPondLevelInvalidated()
	{
		this.RefreshPondsCache();
	}

	public void GetBoats()
	{
	}

	public void GetFishes(int[] fishIds)
	{
		int[] array = fishIds.Except(this._fishesCache.Select((Fish x) => x.FishId)).ToArray<int>();
		if (array.Length == 0)
		{
			this.GetFishes();
		}
		else
		{
			PhotonConnectionFactory.Instance.GetFishList(array);
		}
	}

	public Pond GetPondInfoFromCache(int pondId)
	{
		Pond pond = null;
		this._pondCache.TryGetValue(pondId, out pond);
		return pond;
	}

	public void GetPondInfo(int pondId)
	{
		this._requestedPond = pondId;
		if (this._pondCache.ContainsKey(pondId))
		{
			Debug.Log("GetPondInfo");
			this.GetPond(this._pondCache[pondId]);
		}
	}

	public void GetFishCategories()
	{
		if (this._fishCategories == null && !this._isGetFishCategoriesRequested)
		{
			LogHelper.Log("___kocha GetFishCategories");
			this._isGetFishCategoriesRequested = true;
			PhotonConnectionFactory.Instance.GetFishCategories();
		}
	}

	public void GetBoatDescs()
	{
		if (this._allBoats == null)
		{
			PhotonConnectionFactory.Instance.GetBoats();
		}
		else if (this.OnGetBoatDescs != null)
		{
			this.OnGetBoatDescs(this, new GlobalMapBoatDescCacheEventArgs
			{
				Items = this._allBoats
			});
		}
	}

	public FishCategory GetFishCategory(int categoryId)
	{
		if (this._fishCategories != null)
		{
			return this._fishCategories.FirstOrDefault((FishCategory fCategory) => categoryId == fCategory.CategoryId);
		}
		return null;
	}

	public FishCategory GetParentFishCategory(int categoryId)
	{
		FishCategory fishCategory;
		for (;;)
		{
			fishCategory = this.GetFishCategory(categoryId);
			if (fishCategory == null || fishCategory.ParentCategoryId == null)
			{
				break;
			}
			categoryId = fishCategory.ParentCategoryId.Value;
		}
		return fishCategory;
	}

	private void OnGotBoats(IEnumerable<BoatDesc> boats)
	{
		this._allBoats = boats;
		if (this.OnGetBoatDescs != null)
		{
			this.OnGetBoatDescs(this, new GlobalMapBoatDescCacheEventArgs
			{
				Items = this._allBoats
			});
		}
	}

	protected void GetPond(Pond pond)
	{
		if (this.OnGetPond != null)
		{
			Debug.Log("GetPond");
			this.OnGetPond(this, new GlobalMapPondCacheEventArgs
			{
				Pond = pond
			});
		}
	}

	protected void OnGotFishes(IEnumerable<Fish> fishes)
	{
		List<Fish> list = ((fishes == null) ? new List<Fish>() : fishes.ToList<Fish>());
		if (list.Count > 0)
		{
			this._fishesCache.AddRange(list);
		}
		this.GetFishes();
	}

	protected void OnGotFishCategories(IEnumerable<FishCategory> fish)
	{
		if (fish == null || fish.Count<FishCategory>() == 0)
		{
			return;
		}
		this._fishCategories = fish;
		this.OnFishCategoriesLoaded();
	}

	protected void GetFishes()
	{
		if (this.OnFishes != null)
		{
			this.OnFishes(this, new GlobalMapFishCacheEventArgs
			{
				Items = this._fishesCache
			});
		}
	}

	private void OnGotPondUnlocked(int pondId, int accesibleLevel)
	{
		foreach (KeyValuePair<int, Pond> keyValuePair in this._pondCache)
		{
			Pond value = keyValuePair.Value;
			if (value.MinLevel <= accesibleLevel && PhotonConnectionFactory.Instance.Profile.Level < accesibleLevel)
			{
				keyValuePair.Value.MinLevel = 1;
			}
		}
		for (int i = 0; i < this.AllLicenses.Count; i++)
		{
			if (this.AllLicenses[i].MinLevel <= accesibleLevel && PhotonConnectionFactory.Instance.Profile.Level < accesibleLevel)
			{
				this.AllLicenses[i].MinLevel = new int?(1);
			}
		}
		if (this.OnUpdateLicenses != null)
		{
			this.OnUpdateLicenses(this, new GlobalMapLicenseCacheEventArgs
			{
				Items = this.AllLicenses
			});
		}
	}

	private IEnumerator InitGlobalMapCache()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || CacheLibrary.ProductCache.Products == null)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotAllPondInfos += this.Instance_OnGotAllPondsInfo;
		PhotonConnectionFactory.Instance.GetAllPondInfos(StaticUserData.AllCountriesId);
		yield break;
	}

	private IEnumerator InitFishesBriefsCache()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || CacheLibrary.ProductCache.Products == null)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotFishBrief += this.Instance_OnGotFishBrief;
		PhotonConnectionFactory.Instance.GetFishBrief();
		yield break;
	}

	private void Instance_OnGotAllPondsInfo(List<Pond> ponds)
	{
		PhotonConnectionFactory.Instance.OnGotAllPondInfos -= this.Instance_OnGotAllPondsInfo;
		this._pondCache = new Dictionary<int, Pond>();
		for (int i = 0; i < ponds.Count; i++)
		{
			this._pondCache.Add(ponds[i].PondId, ponds[i]);
		}
		if (this._requestedPond != 0)
		{
			this.GetPond(this._pondCache[this._requestedPond]);
			this._requestedPond = 0;
		}
		ManagerScenes.ProgressOfLoad += 0.1f;
		base.StartCoroutine(this.InitLicensesCache());
	}

	private IEnumerator InitLicensesCache()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || CacheLibrary.ProductCache.Products == null)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotLicenses += this.Instance_OnGotLicenses;
		PhotonConnectionFactory.Instance.GetLicenses(null, null, null);
		yield break;
	}

	public void RefreshPondsCache()
	{
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.IsConnectedToGameServer)
		{
			PhotonConnectionFactory.Instance.OnGotAllPondLevels += this.Instance_OnGotAllPondLevel;
			PhotonConnectionFactory.Instance.GetAllPondLevels(StaticUserData.AllCountriesId);
		}
	}

	public void RefreshMetadataForUserCompetition()
	{
		base.StartCoroutine(this.InitMetadataForUserCompetition());
	}

	private void Instance_OnGotAllPondLevel(List<PondLevelInfo> ponds)
	{
		PhotonConnectionFactory.Instance.OnGotAllPondLevels -= this.Instance_OnGotAllPondLevel;
		for (int i = 0; i < ponds.Count; i++)
		{
			this._pondCache[ponds[i].PondId].MinLevel = ponds[i].MinLevel;
			this._pondCache[ponds[i].PondId].MinLevelExpiration = ponds[i].MinLevelExpiration;
		}
		if (this.OnRefreshedPondsInfo != null)
		{
			this.OnRefreshedPondsInfo(this, null);
		}
		ManagerScenes.ProgressOfLoad += 0.1f;
	}

	private void Instance_OnGotFishBrief(IEnumerable<FishBrief> fishAssets)
	{
		PhotonConnectionFactory.Instance.OnGotFishBrief -= this.Instance_OnGotFishBrief;
		List<FishBrief> list;
		if (fishAssets != null)
		{
			list = fishAssets.OrderBy((FishBrief p) => p.Name).ToList<FishBrief>();
		}
		else
		{
			list = new List<FishBrief>();
		}
		this.FishesBriefs = list;
		this.OnFishesBriefsLoaded();
		ManagerScenes.ProgressOfLoad += 0.1f;
		base.StartCoroutine(this.InitStates());
	}

	private IEnumerator InitMetadataForUserCompetition()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetition += this.Instance_OnGetMetadataForCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetition += this.Instance_OnFailureGetMetadataForCompetition;
		PhotonConnectionFactory.Instance.GetMetadataForCompetition();
		yield break;
	}

	private void Instance_OnGetMetadataForCompetition(MetadataForUserCompetition metadata)
	{
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetition -= this.Instance_OnGetMetadataForCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetition -= this.Instance_OnFailureGetMetadataForCompetition;
		this.UgcMetadata = metadata;
		LogHelper.Log("___kocha MetadataForCompetition Templates:{0}", new object[] { (metadata.Templates == null) ? (-1) : metadata.Templates.Length });
		ManagerScenes.ProgressOfLoad += 0.1f;
		base.StartCoroutine(this.InitSearchMetadataForUserCompetition());
	}

	private void Instance_OnFailureGetMetadataForCompetition(UserCompetitionFailure failure)
	{
		LogHelper.Error("FailureGetMetadataForCompetition {0}", new object[] { failure.FullErrorInfo });
		PhotonConnectionFactory.Instance.GetMetadataForCompetition();
	}

	private IEnumerator InitSearchMetadataForUserCompetition()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetitionSearch += this.GetMetadataForCompetitionSearch;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetitionSearch += this.FailureGetMetadataForCompetitionSearch;
		PhotonConnectionFactory.Instance.GetMetadataForCompetitionSearch();
		yield break;
	}

	private void GetMetadataForCompetitionSearch(MetadataForUserCompetitionSearch metadata)
	{
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetitionSearch -= this.GetMetadataForCompetitionSearch;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetitionSearch -= this.FailureGetMetadataForCompetitionSearch;
		this.UgcMetadataForSearch = metadata;
		LogHelper.Log("___kocha MetadataForCompetitionSearch FishCategories:{0}", new object[] { (metadata.FishCategories == null) ? (-1) : metadata.FishCategories.Length });
		ManagerScenes.ProgressOfLoad += 0.1f;
		base.StartCoroutine(this.InitAbTestsSelection());
	}

	private void FailureGetMetadataForCompetitionSearch(UserCompetitionFailure failure)
	{
		LogHelper.Error("FailureGetMetadataForCompetitionSearch {0}", new object[] { failure.FullErrorInfo });
		PhotonConnectionFactory.Instance.GetMetadataForCompetitionSearch();
	}

	private IEnumerator InitStates()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotStates += this.Instance_OnGotStates;
		PhotonConnectionFactory.Instance.OnGettingStatesFailed += this.Instance_OnGettingStatesFailed;
		PhotonConnectionFactory.Instance.GetStates(StaticUserData.AllCountriesId);
		yield break;
	}

	private void Instance_OnGotStates(IEnumerable<State> o)
	{
		PhotonConnectionFactory.Instance.OnGotStates -= this.Instance_OnGotStates;
		PhotonConnectionFactory.Instance.OnGettingStatesFailed -= this.Instance_OnGettingStatesFailed;
		this._allStates = o.ToList<State>();
		ManagerScenes.ProgressOfLoad += 0.1f;
		this.RefreshMetadataForUserCompetition();
	}

	private void Instance_OnGettingStatesFailed(Failure failure)
	{
		LogHelper.Error("Instance_OnGettingStatesFailed {0}", new object[] { failure.FullErrorInfo });
		PhotonConnectionFactory.Instance.GetStates(StaticUserData.AllCountriesId);
	}

	private IEnumerator InitAbTestsSelection()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotAbTestSelection += this.Instance_OnGotAbTestSelection;
		PhotonConnectionFactory.Instance.OnGettingAbTestSelectionFailed += this.Instance_OnGettingAbTestSelectionFailed;
		PhotonConnectionFactory.Instance.GetAbTestSelection(this._abTestsSelection.Keys.Select((Constants.AB_TESTS p) => (int)p).ToArray<int>());
		yield break;
	}

	private void Instance_OnGotAbTestSelection(Dictionary<int, bool> abTests)
	{
		PhotonConnectionFactory.Instance.OnGotAbTestSelection -= this.Instance_OnGotAbTestSelection;
		PhotonConnectionFactory.Instance.OnGettingAbTestSelectionFailed -= this.Instance_OnGettingAbTestSelectionFailed;
		foreach (KeyValuePair<int, bool> keyValuePair in abTests)
		{
			if (Enum.IsDefined(typeof(Constants.AB_TESTS), keyValuePair.Key))
			{
				this._abTestsSelection[(Constants.AB_TESTS)keyValuePair.Key] = keyValuePair.Value;
			}
		}
		ManagerScenes.ProgressOfLoad += 0.1f;
		base.StartCoroutine(this.InitAllItemBriefs());
	}

	private void Instance_OnGettingAbTestSelectionFailed(Failure failure)
	{
		LogHelper.Error("Instance_OnGettingAbTestSelectionFailed {0}", new object[] { failure.FullErrorInfo });
	}

	private IEnumerator InitAllItemBriefs()
	{
		while (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			yield return null;
		}
		PhotonConnectionFactory.Instance.OnGotItemBriefs += this.Instance_GetAllItemBriefs;
		PhotonConnectionFactory.Instance.OnErrorGettingItemBriefs += this.Instance_GetAllItemBriefsFailed;
		PhotonConnectionFactory.Instance.GetAllItemBriefs();
		yield break;
	}

	private void Instance_GetAllItemBriefs(InventoryItemBrief[] items)
	{
		PhotonConnectionFactory.Instance.OnGotItemBriefs -= this.Instance_GetAllItemBriefs;
		PhotonConnectionFactory.Instance.OnErrorGettingItemBriefs -= this.Instance_GetAllItemBriefsFailed;
		LogHelper.Log("GetAllItemBriefs Length:{0}", new object[] { items.Length });
		this.InventoryItemBriefs = items.ToList<InventoryItemBrief>();
		this.InventoryItemBriefs.ForEach(delegate(InventoryItemBrief p)
		{
			this.InventoryItemLightDict[p.ItemId] = p;
		});
	}

	private void Instance_GetAllItemBriefsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGotItemBriefs -= this.Instance_GetAllItemBriefs;
		PhotonConnectionFactory.Instance.OnErrorGettingItemBriefs -= this.Instance_GetAllItemBriefsFailed;
		LogHelper.Error("Instance_GetAllItemBriefsFailed {0}", new object[] { failure.FullErrorInfo });
	}

	protected List<Tournament> _tournamentsCache = new List<Tournament>();

	protected List<Fish> _fishesCache = new List<Fish>();

	protected List<FishBrief> FishesBriefs;

	protected List<InventoryItemBrief> InventoryItemBriefs;

	private IEnumerable<BoatDesc> _allBoats;

	protected IEnumerable<FishCategory> _fishCategories;

	private List<State> _allStates;

	protected Dictionary<int, Pond> _pondCache = new Dictionary<int, Pond>();

	private int _requestedPond;

	private readonly Dictionary<Constants.AB_TESTS, bool> _abTestsSelection = new Dictionary<Constants.AB_TESTS, bool>
	{
		{
			Constants.AB_TESTS.SKIP_CHARACTER_CUSTOMIZATION_ON_START,
			false
		},
		{
			Constants.AB_TESTS.INITIAL_MONEY_TEST,
			false
		},
		{
			Constants.AB_TESTS.NEW_PREMIUM_SHOP_IMPLEMETATION,
			false
		},
		{
			Constants.AB_TESTS.PREM_SHOP_BUY_OPTIONS,
			false
		}
	};

	public List<ShopLicense> AllLicenses;

	private readonly Dictionary<int, InventoryItemBrief> InventoryItemLightDict = new Dictionary<int, InventoryItemBrief>();

	private bool _isGetFishCategoriesRequested;
}
