using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using Leaderboard;
using ObjectModel;
using Photon.Interfaces.LeaderBoards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardFishInit : Top100CommonPage
{
	protected override Top100Proxy CurrentProxy
	{
		get
		{
			return this._serverProxy;
		}
	}

	protected override void Start()
	{
		string text = ScriptLocalization.Get("UGC_FiltersCaption").ToLower();
		this._filters.text = char.ToUpper(text[0]) + text.Substring(1);
		this._captionNumber.text = ScriptLocalization.Get("NumberColumnCaption");
		this._captionName.text = PlayerProfileHelper.UsernameCaption;
		this._captionFishName.text = ScriptLocalization.Get("FishNameCaption");
		this._captionLevel.text = ScriptLocalization.Get("LevelMenuCaption");
		this._captionRank.text = ScriptLocalization.Get("RankMenuCaption");
		this._captionWeight.text = string.Format("{0}, {1}", ScriptLocalization.Get("WeightCaption"), MeasuringSystemManager.FishWeightSufix());
		base.Start();
	}

	private void RebuildDefaultFishesList()
	{
		List<FishBrief> list = CacheLibrary.MapCache.FishesLight.Where((FishBrief x) => !x.CodeName.EndsWith("T") && !x.CodeName.EndsWith("U") && !x.CodeName.EndsWith("Y")).ToList<FishBrief>();
		foreach (FishBrief fishBrief in CacheLibrary.MapCache.FishesLight)
		{
			string code = fishBrief.CodeName.TrimEnd(new char[] { 'T', 'U', 'Y' });
			if (list.FirstOrDefault((FishBrief x) => x.CodeName == code) == null)
			{
				list.Add(fishBrief);
			}
		}
		list.Sort(delegate(FishBrief a, FishBrief b)
		{
			int num = 0;
			if (num == 0)
			{
				num = FishHelper.EventFishCategories.Contains((FishTypes)CacheLibrary.MapCache.GetParentFishCategory(a.CategoryId).CategoryId).CompareTo(FishHelper.EventFishCategories.Contains((FishTypes)CacheLibrary.MapCache.GetParentFishCategory(b.CategoryId).CategoryId));
				if (num == 0)
				{
					num = string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
				}
			}
			return num;
		});
		this._defaultFishes = list.Select((FishBrief x) => new FishSelectorElement
		{
			FishId = x.FishId,
			CodeName = x.CodeName,
			Text = x.Name.ToUpper()
		}).ToList<IScrollSelectorElement>();
		this._defaultFishes.Insert(0, new FishSelectorElement
		{
			FishId = -1,
			Text = ScriptLocalization.Get("AllCaption").ToUpper(),
			_isDefault = true
		});
	}

	private void CreateSelectors()
	{
		this.selectorScope = Object.Instantiate<ScrollSelectorInit>(this.selectorPrefab, this.selectorParent);
		List<IScrollSelectorElement> list = new List<IScrollSelectorElement>
		{
			new ScopeSelectorElement
			{
				Scope = 1,
				Text = ScriptLocalization.Get("AllCaption").ToUpper(),
				_isDefault = true
			},
			new ScopeSelectorElement
			{
				Scope = 3,
				Text = ScriptLocalization.Get("ChatFriendsCaption").ToUpper()
			},
			new ScopeSelectorElement
			{
				Scope = 4,
				Text = ScriptLocalization.Get("MyFish").ToUpper()
			}
		};
		this.selectorScope.Init(ScriptLocalization.Get("UGC_PlayersCaption").ToUpper(), list, 0, "ponds", new Action(this.Refresh));
		this.selectorFishes = Object.Instantiate<ScrollSelectorInit>(this.selectorPrefab, this.selectorParent);
		this.RebuildDefaultFishesList();
		this.selectorFishes.Init(ScriptLocalization.Get("FishCaption").ToUpper(), this._defaultFishes, 0, "fish name", new Action(this.Refresh));
		this.selectorForm = Object.Instantiate<ScrollSelectorInit>(this.selectorPrefab, this.selectorParent);
		List<IScrollSelectorElement> list2 = (from UIHelper.FishTypes x in Enum.GetValues(typeof(UIHelper.FishTypes))
			select new FormSelectorElement
			{
				Form = (int)x,
				Text = UIHelper.GetTypeNameByFishType(x).ToUpper()
			}).ToList<IScrollSelectorElement>();
		list2.Insert(0, new FormSelectorElement
		{
			Form = -1,
			Text = ScriptLocalization.Get("AllCaption").ToUpper(),
			_isDefault = true
		});
		this.selectorForm.Init(ScriptLocalization.Get("TypeSortCaption").ToUpper(), list2, 0, "fish type", new Action(this.Refresh));
		this.selectorPonds = Object.Instantiate<ScrollSelectorInit>(this.selectorPrefab, this.selectorParent);
		List<IScrollSelectorElement> list3 = (from x in CacheLibrary.MapCache.CachedPonds
			where x.PondId != 2
			select new PondSelectorElement
			{
				PondId = x.PondId,
				Text = x.Name.ToUpper()
			}).ToList<IScrollSelectorElement>();
		list3.Insert(0, new PondSelectorElement
		{
			PondId = -1,
			Text = ScriptLocalization.Get("AllCaption").ToUpper(),
			_isDefault = true
		});
		this.selectorPonds.Init(ScriptLocalization.Get("UGC_Location").ToUpper(), list3, 0, "ponds", new Action(this.OverrideFishesListAndRefresh));
	}

	protected override void Awake()
	{
		base.Awake();
		this.TglGlobal.onValueChanged.AddListener(delegate(bool t)
		{
			if (t)
			{
				this.Refresh();
			}
			else
			{
				this.HidePanel();
			}
		});
		this.CreateSelectors();
	}

	protected override void InitProxy()
	{
		this._serverProxy = new Top100Proxy("LeaderboardFishInit", new Action(this.SendServerRequest), new ServerProxy<TopLeadersResult>.IsMyRespond(this.IsMyRespond), new int[] { 12, 42 });
		this._serverProxy.ERequestFailure += this.OnGetLeaderboardsFailed;
		this._serverProxy.ERespond += this.OnGetLeaderboards;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this._serverProxy.CancelRequest();
		this._serverProxy.ERequestFailure -= this.OnGetLeaderboardsFailed;
		this._serverProxy.ERespond -= this.OnGetLeaderboards;
	}

	internal void OnEnable()
	{
		if (this.TglGlobal.isOn)
		{
			base.FastHidePanel();
			this.ShowPanel();
		}
	}

	public void OverrideFishesListAndRefresh()
	{
		Pond pondInfoFromCache = CacheLibrary.MapCache.GetPondInfoFromCache((this.selectorPonds.GetCurrentValue() as PondSelectorElement).PondId);
		List<IScrollSelectorElement> list;
		if (pondInfoFromCache == null)
		{
			list = this._defaultFishes;
		}
		else
		{
			List<string> codeNames = new List<string>();
			int i = 0;
			while (i < pondInfoFromCache.FishIds.Length)
			{
				LeaderboardFishInit.<OverrideFishesListAndRefresh>c__AnonStorey1 <OverrideFishesListAndRefresh>c__AnonStorey2 = new LeaderboardFishInit.<OverrideFishesListAndRefresh>c__AnonStorey1();
				<OverrideFishesListAndRefresh>c__AnonStorey2.id = pondInfoFromCache.FishIds[i];
				IScrollSelectorElement scrollSelectorElement = this._defaultFishes.FirstOrDefault((IScrollSelectorElement x) => (x as FishSelectorElement).FishId == <OverrideFishesListAndRefresh>c__AnonStorey2.id);
				if (scrollSelectorElement != null)
				{
					goto IL_EB;
				}
				FishBrief fish = CacheLibrary.MapCache.FishesLight.FirstOrDefault((FishBrief x) => x.FishId == <OverrideFishesListAndRefresh>c__AnonStorey2.id);
				if (fish != null)
				{
					scrollSelectorElement = this._defaultFishes.FirstOrDefault((IScrollSelectorElement x) => fish.CodeName.StartsWith((x as FishSelectorElement).CodeName) && !string.IsNullOrEmpty((x as FishSelectorElement).CodeName));
					goto IL_EB;
				}
				Debug.LogError("Pond contains fish not present in fishes cache");
				IL_12E:
				i++;
				continue;
				IL_EB:
				string text = (scrollSelectorElement as FishSelectorElement).CodeName.TrimEnd(new char[] { 'T', 'U', 'Y' });
				if (!codeNames.Contains(text))
				{
					codeNames.Add(text);
					goto IL_12E;
				}
				goto IL_12E;
			}
			list = this._defaultFishes.Where((IScrollSelectorElement x) => (x as FishSelectorElement).FishId == -1 || codeNames.Contains((x as FishSelectorElement).CodeName)).ToList<IScrollSelectorElement>();
		}
		this.selectorFishes.SavePrevious();
		this.selectorFishes.SetElements(list);
		this.Refresh();
	}

	public void Refresh()
	{
		if (this.TglGlobal.isOn)
		{
			this.ShowPanel();
		}
		if (this.selectorFishes.GetCurrentValue() == null)
		{
			this.FillData(null);
			return;
		}
		int pondId = (this.selectorPonds.GetCurrentValue() as PondSelectorElement).PondId;
		int fishId = (this.selectorFishes.GetCurrentValue() as FishSelectorElement).FishId;
		TopScope scope = (this.selectorScope.GetCurrentValue() as ScopeSelectorElement).Scope;
		int form = (this.selectorForm.GetCurrentValue() as FormSelectorElement).Form;
		if (pondId != this._lastPondId || fishId != this._lastFishId || scope != this._lastScope || this._lastForm != form)
		{
			this._serverProxy.ClearCache();
		}
		if (!this._serverProxy.IsLoading)
		{
			this._serverProxy.SendRequest();
			this._lastPondId = pondId;
			this._lastFishId = fishId;
			this._lastScope = scope;
			this._lastForm = form;
		}
	}

	private void SendServerRequest()
	{
		int? num = new int?((this.selectorPonds.GetCurrentValue() as PondSelectorElement).PondId);
		int? num2 = new int?((this.selectorFishes.GetCurrentValue() as FishSelectorElement).FishId);
		int form = (this.selectorForm.GetCurrentValue() as FormSelectorElement).Form;
		TopScope scope = (this.selectorScope.GetCurrentValue() as ScopeSelectorElement).Scope;
		TopFishOrder topFishOrder = 1;
		bool flag = num2 == -1 && form == -1;
		int[] array = ((!flag) ? this.GetFishIdArrayForRequest(num2.Value, form) : null);
		if (!flag && (array == null || array.Length == 0))
		{
			this._serverProxy.CancelRequest();
			this.FillData(null);
			return;
		}
		this.Displayer.LoadEntriesList(null);
		base.ShowLoading(true);
		base.ShowEmpty(false);
		PhotonConnectionFactory.Instance.GetTopFish(scope, topFishOrder, (!(num == -1)) ? num : null, null, array);
	}

	private int[] GetFishIdArrayForRequest(int fishId, int fishForm)
	{
		FishBrief fishBrief = CacheLibrary.MapCache.FishesLight.FirstOrDefault((FishBrief x) => x.FishId == fishId);
		if (fishBrief != null)
		{
			string code = fishBrief.CodeName;
			switch (fishForm + 1)
			{
			case 1:
				code += "Y";
				break;
			case 2:
				return (from x in CacheLibrary.MapCache.FishesLight
					where x.CodeName == code
					select x into y
					select y.FishId).ToArray<int>();
			case 3:
				code += "T";
				break;
			case 4:
				code += "U";
				break;
			}
			return (from x in CacheLibrary.MapCache.FishesLight
				where x.CodeName.Contains(code) && x.CodeName.Length - code.Length <= 1
				select x into y
				select y.FishId).ToArray<int>();
		}
		return (from x in CacheLibrary.MapCache.FishesLight
			where UIHelper.GetFishType(x) == (UIHelper.FishTypes)fishForm
			select x into y
			select y.FishId).ToArray<int>();
	}

	private bool IsMyRespond(TopLeadersResult result)
	{
		return result.Kind == 2;
	}

	private void OnGetLeaderboards(TopLeadersResult result)
	{
		this.FillData(result);
	}

	private new void OnGetLeaderboardsFailed()
	{
	}

	private void FillData(TopLeadersResult result)
	{
		if (result != null && result.Fish != null)
		{
			this.Displayer.LoadEntriesList(from x in result.Fish
				orderby x.Weight descending
				select (x));
		}
		else
		{
			this.Displayer.LoadEntriesList(null);
		}
		base.ShowEmpty(result == null || result.Fish == null || result.Fish.ToList<TopFish>().Count == 0);
		base.ShowLoading(false);
	}

	[SerializeField]
	private TextMeshProUGUI _filters;

	[SerializeField]
	private TextMeshProUGUI _captionNumber;

	[SerializeField]
	private TextMeshProUGUI _captionName;

	[SerializeField]
	private TextMeshProUGUI _captionLevel;

	[SerializeField]
	private TextMeshProUGUI _captionRank;

	[SerializeField]
	private TextMeshProUGUI _captionFishName;

	[SerializeField]
	private TextMeshProUGUI _captionWeight;

	public ScrollSelectorInit selectorPrefab;

	private ScrollSelectorInit selectorFishes;

	private List<IScrollSelectorElement> _defaultFishes;

	private ScrollSelectorInit selectorForm;

	private ScrollSelectorInit selectorScope;

	private ScrollSelectorInit selectorPonds;

	public Transform selectorParent;

	public GameObject PondItemPrefab;

	public Toggle TglGlobal;

	public GameObject FishItemPrefab;

	public GameObject LoadingText;

	private bool _isDropDownInited;

	private bool _isInited;

	private GameObject _fishTypeAll;

	private GameObject _fishTypeSelf;

	private GameObject _pondScopeAll;

	private GameObject _pondScopeSelf;

	private Top100Proxy _serverProxy;

	private int _lastPondId;

	private int _lastFishId;

	private TopScope _lastScope;

	private int _lastForm;

	public FishLeaderboard Displayer;
}
