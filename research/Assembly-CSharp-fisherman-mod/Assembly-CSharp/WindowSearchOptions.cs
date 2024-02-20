using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class WindowSearchOptions : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<FilterForUserCompetitions> OnSelect = delegate(FilterForUserCompetitions filter)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this._dataFilters = new List<WindowList.WindowListElem>
		{
			new WindowList.WindowListElem
			{
				Name = ScriptLocalization.Get("LocationCaption")
			},
			new WindowList.WindowListElem
			{
				Name = ScriptLocalization.Get("FishCaption")
			},
			new WindowList.WindowListElem
			{
				Name = ScriptLocalization.Get("UGC_FormatTypes")
			},
			new WindowList.WindowListElem
			{
				Name = ScriptLocalization.Get("UGC_RulesCaption")
			},
			new WindowList.WindowListElem
			{
				Name = ScriptLocalization.Get("UGC_ShowEnded")
			}
		};
		this._valueColor = this._searchResults[0].Value.color;
		for (int i = 0; i < this._searchResults.Length; i++)
		{
			this._searchResultsList[this._searchResults[i].Filter] = this._searchResults[i];
		}
	}

	public void Init(FilterForUserCompetitions filterCompetitions)
	{
		this._filterCompetitions = filterCompetitions;
		this.GetMetadataForCompetitionSearch(CacheLibrary.MapCache.UgcMetadataForSearch);
	}

	protected void SetActiveItem(int i, List<IWindowListItem> items, ref int selectedIndex)
	{
		items[selectedIndex].SetActive(false);
		selectedIndex = i;
		items[selectedIndex].SetActive(true);
	}

	protected void SetActiveItem(int i, List<IWindowListItem> items)
	{
		WindowSearchOptions.Filters filters = (WindowSearchOptions.Filters)this._selectedIndexFilters;
		if (filters == WindowSearchOptions.Filters.ShowEnded)
		{
			for (int j = 0; j < items.Count; j++)
			{
				items[j].SetActive(j == i);
			}
		}
		else
		{
			items[i].SetActive(!items[i].IsActive);
		}
		List<int> list = new List<int>();
		for (int k = 0; k < items.Count; k++)
		{
			if (items[k].IsActive)
			{
				list.Add(k);
			}
		}
		this._selectedIdx[filters] = list;
		this.UpdateFilterOpts(filters);
	}

	protected override void AcceptActionCalled()
	{
		this._filterCompetitions.PondIds = this.FillArray<int>(WindowSearchOptions.Filters.Locations, (int i) => this._ponds.ElementAt(i).PondId);
		this._filterCompetitions.Formats = this.FillArray<UserCompetitionFormat>(WindowSearchOptions.Filters.CompetitionType, (int i) => this._competitionTypes.ElementAt(i));
		this._filterCompetitions.ScoreTypes = this.FillArray<TournamentScoreType>(WindowSearchOptions.Filters.Rulesets, (int i) => this._scoreTypes.ElementAt(i));
		this._filterCompetitions.FishCategories = this.FillArray<int>(WindowSearchOptions.Filters.Fish, (int i) => this._fishes.ElementAt(i).CategoryId);
		this._filterCompetitions.ShowEnded = this._selectedIdx[WindowSearchOptions.Filters.ShowEnded].Contains(1);
		this.OnSelect(this._filterCompetitions);
	}

	private void SetFilter(int i)
	{
		this._selectedIndexFiltersOpts = 0;
		this._dataFiltersOpts.Clear();
		this.Clear(this._itemsFiltersOpts);
		WindowSearchOptions.Filters filters = (WindowSearchOptions.Filters)i;
		this._dataFiltersOpts.AddRange(this._cachedFiltersData[filters]);
		WindowList.CreateList(this._dataFiltersOpts, this._itemsRootFiltersOpts, this._itemFiltersOptsPrefab, this._itemsFiltersOpts, this._tg, this._selectedIndexFiltersOpts, delegate(int idx)
		{
			if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
			{
				this.SetActiveItem(idx, this._itemsFiltersOpts);
			}
		}, delegate(int idx)
		{
			this.SetActiveItem(idx, this._itemsFiltersOpts);
		}, this._selectedIdx[filters], null);
	}

	private void UpdateFilterOpts(WindowSearchOptions.Filters fType)
	{
		List<int> list = this._selectedIdx[fType];
		Text value = this._searchResultsList[fType].Value;
		value.text = ScriptLocalization.Get("None");
		value.color = this._noneColor;
		this._itemsFilters[(int)fType].UpdateText(this._dataFilters[(int)fType].Name);
		if (list.Count > 0)
		{
			value.color = this._valueColor;
			this.UpdateFilterOpts(fType, list, (int)fType, value);
		}
	}

	private void UpdateFilterOpts(WindowSearchOptions.Filters fType, List<int> idxs, int fIdx, Text value)
	{
		List<WindowList.WindowListElem> selected = new List<WindowList.WindowListElem>();
		idxs.ForEach(delegate(int p)
		{
			selected.Add(this._cachedFiltersData[fType].ElementAt(p));
		});
		value.text = string.Join(",", selected.Select((WindowList.WindowListElem p) => p.Name).ToArray<string>());
		if (fType != WindowSearchOptions.Filters.ShowEnded)
		{
			this._itemsFilters[fIdx].UpdateText(string.Format("{0} <color=#FFDD77FF>({1})</color>", this._dataFilters[fIdx].Name, idxs.Count));
		}
	}

	private void Clear(List<IWindowListItem> items)
	{
		items.ForEach(delegate(IWindowListItem p)
		{
			p.Remove();
		});
		items.Clear();
	}

	private void InitCachedFiltersData()
	{
		this._ponds = (from p in CacheLibrary.MapCache.CachedPonds
			where p.PondId != 2
			orderby p.OriginalMinLevel
			select p).ToList<Pond>();
		this._cachedFiltersData[WindowSearchOptions.Filters.Locations] = this._ponds.Select((Pond p) => new WindowList.WindowListElem
		{
			Name = p.Name
		}).ToList<WindowList.WindowListElem>();
		this._competitionTypes = UserCompetitionHelper.EnumToList<UserCompetitionFormat>(UserCompetitionFormat.None);
		this._cachedFiltersData[WindowSearchOptions.Filters.CompetitionType] = this._competitionTypes.Select((UserCompetitionFormat p) => new WindowList.WindowListElem
		{
			Name = UgcConsts.GetCompetitionFormatLoc(p).Name
		}).ToList<WindowList.WindowListElem>();
		this._scoreTypes = new List<TournamentScoreType>
		{
			TournamentScoreType.TotalWeight,
			TournamentScoreType.TotalFishCount,
			TournamentScoreType.TotalScore,
			TournamentScoreType.TotalWeightByLineMaxLoad,
			TournamentScoreType.TotalFishTypeCount,
			TournamentScoreType.BestWeightMatch,
			TournamentScoreType.BiggestFish,
			TournamentScoreType.SmallestFish,
			TournamentScoreType.BiggestSizeDiff,
			TournamentScoreType.LongestFish,
			TournamentScoreType.TotalLength
		};
		this._cachedFiltersData[WindowSearchOptions.Filters.Rulesets] = this._scoreTypes.Select((TournamentScoreType p) => new WindowList.WindowListElem
		{
			Name = UgcConsts.GetScoreTypeLoc(p).Name
		}).ToList<WindowList.WindowListElem>();
		this._showEnded = new List<string> { "NoCaption", "YesCaption" };
		this._cachedFiltersData[WindowSearchOptions.Filters.ShowEnded] = this._showEnded.Select((string p) => new WindowList.WindowListElem
		{
			Name = ScriptLocalization.Get(p)
		}).ToList<WindowList.WindowListElem>();
	}

	private void InitIdxs()
	{
		this.FillSelectedIdx<int>(WindowSearchOptions.Filters.Locations, this._filterCompetitions.PondIds, (int p) => this._ponds.FindIndex((Pond el) => el.PondId == p));
		this.FillSelectedIdx<UserCompetitionFormat>(WindowSearchOptions.Filters.CompetitionType, this._filterCompetitions.Formats, (UserCompetitionFormat p) => this._competitionTypes.FindIndex((UserCompetitionFormat el) => el == p));
		this.FillSelectedIdx<TournamentScoreType>(WindowSearchOptions.Filters.Rulesets, this._filterCompetitions.ScoreTypes, (TournamentScoreType p) => this._scoreTypes.FindIndex((TournamentScoreType el) => el == p));
		this.FillSelectedIdx<int>(WindowSearchOptions.Filters.Fish, this._filterCompetitions.FishCategories, (int p) => this._fishes.FindIndex((MetadataForUserCompetitionSearch.FishCategory el) => el.CategoryId == p));
		this._selectedIdx[WindowSearchOptions.Filters.ShowEnded] = new List<int> { (!this._filterCompetitions.ShowEnded) ? 0 : 1 };
	}

	private T[] FillArray<T>(WindowSearchOptions.Filters fType, Func<int, T> funcGetValue)
	{
		List<int> list = this._selectedIdx[fType];
		List<T> data = new List<T>();
		list.ForEach(delegate(int p)
		{
			data.Add(funcGetValue(p));
		});
		return data.ToArray();
	}

	private void FillSelectedIdx<T>(WindowSearchOptions.Filters fType, T[] initData, Func<T, int> funcGetIndex)
	{
		this._selectedIdx[fType] = new List<int>();
		if (initData != null)
		{
			for (int i = 0; i < initData.Length; i++)
			{
				this._selectedIdx[fType].Add(funcGetIndex(initData[i]));
			}
		}
	}

	private void Init()
	{
		this.InitCachedFiltersData();
		this.InitIdxs();
		this._selectedIndexFilters = 0;
		this._selectedIndexFiltersOpts = 0;
		this.SetFilter(this._selectedIndexFilters);
		WindowList.CreateList(this._dataFilters, this._itemsRootFilters, this._itemFiltersPrefab, this._itemsFilters, this._tg, this._selectedIndexFilters, delegate(int i)
		{
			this.SetActiveItem(i, this._itemsFilters, ref this._selectedIndexFilters);
			this.SetFilter(i);
		}, delegate(int i)
		{
			this.SetActiveItem(i, this._itemsFilters, ref this._selectedIndexFilters);
			this.SetFilter(i);
		}, null, null);
		UserCompetitionHelper.EnumToList<WindowSearchOptions.Filters>().ForEach(new Action<WindowSearchOptions.Filters>(this.UpdateFilterOpts));
	}

	private void GetMetadataForCompetitionSearch(MetadataForUserCompetitionSearch metadata)
	{
		this._fishes = metadata.FishCategories.ToList<MetadataForUserCompetitionSearch.FishCategory>();
		this._cachedFiltersData[WindowSearchOptions.Filters.Fish] = this._fishes.Select((MetadataForUserCompetitionSearch.FishCategory p) => new WindowList.WindowListElem
		{
			Name = p.Name
		}).ToList<WindowList.WindowListElem>();
		this.Init();
	}

	[SerializeField]
	private WindowSearchOptions.SelectedResult[] _searchResults;

	[SerializeField]
	private GameObject _itemsRootFilters;

	[SerializeField]
	private GameObject _itemFiltersPrefab;

	[SerializeField]
	private GameObject _itemsRootFiltersOpts;

	[SerializeField]
	private GameObject _itemFiltersOptsPrefab;

	[SerializeField]
	private ToggleGroup _tg;

	private int _selectedIndexFilters;

	private readonly List<IWindowListItem> _itemsFilters = new List<IWindowListItem>();

	private List<WindowList.WindowListElem> _dataFilters;

	private int _selectedIndexFiltersOpts;

	private readonly List<IWindowListItem> _itemsFiltersOpts = new List<IWindowListItem>();

	private readonly List<WindowList.WindowListElem> _dataFiltersOpts = new List<WindowList.WindowListElem>();

	private FilterForUserCompetitions _filterCompetitions;

	private List<MetadataForUserCompetitionSearch.FishCategory> _fishes = new List<MetadataForUserCompetitionSearch.FishCategory>();

	private List<Pond> _ponds = new List<Pond>();

	private List<UserCompetitionFormat> _competitionTypes = new List<UserCompetitionFormat>();

	private List<TournamentScoreType> _scoreTypes = new List<TournamentScoreType>();

	private List<string> _showEnded = new List<string>();

	private readonly Dictionary<WindowSearchOptions.Filters, List<int>> _selectedIdx = new Dictionary<WindowSearchOptions.Filters, List<int>>
	{
		{
			WindowSearchOptions.Filters.Locations,
			new List<int>()
		},
		{
			WindowSearchOptions.Filters.Fish,
			new List<int>()
		},
		{
			WindowSearchOptions.Filters.CompetitionType,
			new List<int>()
		},
		{
			WindowSearchOptions.Filters.Rulesets,
			new List<int>()
		},
		{
			WindowSearchOptions.Filters.ShowEnded,
			new List<int>()
		}
	};

	private readonly Dictionary<WindowSearchOptions.Filters, List<WindowList.WindowListElem>> _cachedFiltersData = new Dictionary<WindowSearchOptions.Filters, List<WindowList.WindowListElem>>();

	private readonly Dictionary<WindowSearchOptions.Filters, WindowSearchOptions.SelectedResult> _searchResultsList = new Dictionary<WindowSearchOptions.Filters, WindowSearchOptions.SelectedResult>();

	private readonly Color _noneColor = new Color(0.4862745f, 0.4862745f, 0.4862745f);

	private Color _valueColor;

	[Serializable]
	public class SelectedResult
	{
		[SerializeField]
		public WindowSearchOptions.Filters Filter;

		[SerializeField]
		public Text Value;
	}

	public enum Filters : byte
	{
		Locations,
		Fish,
		CompetitionType,
		Rulesets,
		ShowEnded
	}

	private enum EntryFeeIdxs
	{
		ChatAllCaption,
		MoneySortCaption,
		GoldsButtonPopup
	}
}
