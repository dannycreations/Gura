using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.GlobalMap;
using DG.Tweening;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetPondsOnGlobalMap : ActivityStateControlled
{
	public static GlobeHelpInput GlobeHelp { get; private set; }

	public static List<Pond> Ponds
	{
		get
		{
			return SetPondsOnGlobalMap._pondsButtons.Keys.ToList<Pond>();
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action OnActive;

	public static bool ActiveSouthAmerica { get; private set; }

	public static bool ActiveEurope { get; private set; }

	public static int PondId
	{
		get
		{
			KeyValuePair<int, Toggle> keyValuePair = SetPondsOnGlobalMap._pondsButtonsToggle.FirstOrDefault((KeyValuePair<int, Toggle> p) => p.Value.isOn);
			return keyValuePair.Equals(default(KeyValuePair<int, Toggle>)) ? (-1) : keyValuePair.Key;
		}
	}

	public static TournamentManager TournamentMgr { get; private set; }

	private void Awake()
	{
		SetPondsOnGlobalMap.TournamentMgr = base.gameObject.AddComponent<TournamentManager>();
		SetPondsOnGlobalMap.MapTextureImage = this.MapTexture.GetComponent<RawImage>();
		SetPondsOnGlobalMap.GlobeHelp = this._globeHelp;
		base.GetComponent<ActivityState>().OnStart += this.SetPondsOnGlobalMap_OnStart;
	}

	private void Update()
	{
		if (!CursorManager.IsShowCursor)
		{
			CursorManager.ShowCursor();
		}
		if (GlobalConsts.IsDebugLoading && this._startTransferAt > 0f && this._startTransferAt < Time.time)
		{
			this._startTransferAt = -1f;
			int num = Random.Range(0, this._debugPins.Count);
			LogHelper.Log("Load pond {0}", new object[] { this._debugPins[num] });
			ShowPondInfo.Instance.RequestPondInfo(this._debugPins[num]);
			ShowPondInfo.Instance.TravelInit.OnClickToTravel();
		}
	}

	protected override void Start()
	{
		base.Start();
		PhotonConnectionFactory.Instance.OnPondUnlocked += this.OnGotPondUnlocked;
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected override void OnDestroy()
	{
		TournamentManager tournamentMgr = SetPondsOnGlobalMap.TournamentMgr;
		tournamentMgr.OnRefreshed = (Action)Delegate.Remove(tournamentMgr.OnRefreshed, new Action(this.GetUserGeneratedCompetitions));
		TournamentManager tournamentMgr2 = SetPondsOnGlobalMap.TournamentMgr;
		tournamentMgr2.OnRefreshedTournaments = (Action)Delegate.Remove(tournamentMgr2.OnRefreshedTournaments, new Action(this.RefreshedTournaments));
		SetPondsOnGlobalMap.TournamentMgr = null;
		base.StopAllCoroutines();
		SetPondsOnGlobalMap._pondsButtonsToggle.Clear();
		SetPondsOnGlobalMap._pondsButtonsRts.Clear();
		SetPondsOnGlobalMap._pondsButtonsCg.Clear();
		SetPondsOnGlobalMap._pondsTransforms.Clear();
		SetPondsOnGlobalMap._pondsButtons.Clear();
		base.GetComponent<ActivityState>().OnStart -= this.SetPondsOnGlobalMap_OnStart;
		PhotonConnectionFactory.Instance.OnPondUnlocked -= this.OnGotPondUnlocked;
		PhotonConnectionFactory.Instance.OnGotStats -= this.OnGotStats;
		PhotonConnectionFactory.Instance.OnGotOpenTournaments -= this.PhotonServerOnGotOpenTournaments;
		PhotonConnectionFactory.Instance.OnGotAllPondWeather -= this.Instance_OnGotAllPondWeather;
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		this._isSubscribed = false;
		base.OnDestroy();
	}

	public static Ponds LastPondId
	{
		get
		{
			Ponds ponds = (Ponds)((!CustomPlayerPrefs.HasKey("LastPondId")) ? 119 : CustomPlayerPrefs.GetInt("LastPondId"));
			if (ponds == Assets.Scripts.UI._2D.GlobalMap.Ponds.FirstStep)
			{
				ponds = Assets.Scripts.UI._2D.GlobalMap.Ponds.LoneStar;
			}
			return ponds;
		}
	}

	public void SetupPonds()
	{
		this._debugPins.Clear();
		this._startTransferAt = Time.time + (float)GlobalConsts.TransferIn;
		this._languagesInit = ChangeLanguage.GetCurrentLanguage;
		this._firstInited = true;
		this.Clear();
		InitPondPin initPondPin = null;
		int lastPondId = (int)SetPondsOnGlobalMap.LastPondId;
		InitPondPin initPondPin2 = null;
		ToggleGroup component = this.ToggleGroup.GetComponent<ToggleGroup>();
		List<Pond> list = CacheLibrary.MapCache.CachedPonds.Where((Pond x) => x.IsVisible && x.PondId != 2).ToList<Pond>();
		for (int i = 0; i < list.Count; i++)
		{
			Pond pond = list[i];
			int pondId = pond.PondId;
			if (pond.Region.RegionId == 3)
			{
				SetPondsOnGlobalMap.ActiveSouthAmerica = true;
			}
			else if (pond.Region.RegionId == 2)
			{
				SetPondsOnGlobalMap.ActiveEurope = true;
			}
			InitPondPin initPondPin3 = this.CreateButton(pond, i, component);
			SetPondsOnGlobalMap._pondsButtons.Add(pond, initPondPin3.Toggle);
			Toggle toggle = initPondPin3.Toggle;
			SetPondsOnGlobalMap._pondsButtonsToggle[pondId] = toggle;
			SetPondsOnGlobalMap._pondsButtonsRts[pondId] = toggle.GetComponent<RectTransform>();
			SetPondsOnGlobalMap._pondsButtonsCg[pondId] = toggle.GetComponent<CanvasGroup>();
			toggle.isOn = false;
			toggle.onValueChanged.AddListener(delegate(bool b)
			{
				if (b)
				{
					(from p in SetPondsOnGlobalMap._pondsButtons
						where p.Key.PondId != pondId && p.Value.isOn
						select p.Value.GetComponent<InitPondPin>()).ToList<InitPondPin>().ForEach(delegate(InitPondPin p)
					{
						p.TurnOff();
					});
				}
			});
			initPondPin = initPondPin ?? initPondPin3;
			InitPondPin initPondPin4 = initPondPin3;
			initPondPin4.Init(pond);
			if (!GlobalMapHelper.IsActive(pond))
			{
				initPondPin4.AddState(InitPondPin.PondStates.InDeveloping);
			}
			if (pond.PondLocked())
			{
				initPondPin4.SetLocked(true);
			}
			else
			{
				this._debugPins.Add(pondId);
			}
			if (lastPondId != 0 && lastPondId == pondId)
			{
				initPondPin2 = initPondPin3;
			}
		}
		if (initPondPin2 != null)
		{
			this.ActivateToggle(initPondPin2);
		}
		else if (initPondPin != null)
		{
			this.ActivateToggle(initPondPin);
		}
		this.SelectActivePond();
		SetPondsOnGlobalMap._pondsTransforms = SetPondsOnGlobalMap._pondsButtons.Values.Select((Toggle p) => p.GetComponent<Transform>()).ToList<Transform>();
		UINavigation nav = this.ToggleGroup.GetComponent<UINavigation>();
		Enum.GetValues(typeof(UINavigation.Bindings)).Cast<UINavigation.Bindings>().ToList<UINavigation.Bindings>()
			.ForEach(delegate(UINavigation.Bindings p)
			{
				nav.SetUpdateRegion(p, false);
			});
		foreach (KeyValuePair<Pond, Toggle> keyValuePair in SetPondsOnGlobalMap._pondsButtons)
		{
			Toggle value = keyValuePair.Value;
			value.GetComponent<InitPondPin>().AddState(InitPondPin.PondStates.InitedAll);
			if (!SetPondsOnGlobalMap.IsActivePondButtons)
			{
				value.gameObject.SetActive(false);
			}
			value.GetComponent<CanvasGroup>().alpha = 0f;
			PondBtnBinding pondBindings = PondBtnsBindings.GetPondBindings((Ponds)keyValuePair.Key.PondId);
			if (pondBindings != null)
			{
				Selectable selectable = null;
				Selectable selectable2 = null;
				Selectable selectable3 = null;
				Selectable selectable4 = null;
				foreach (KeyValuePair<Pond, Toggle> keyValuePair2 in SetPondsOnGlobalMap._pondsButtons)
				{
					if (keyValuePair2.Key.PondId == pondBindings.Left)
					{
						selectable = keyValuePair2.Value;
					}
					else if (keyValuePair2.Key.PondId == pondBindings.Right)
					{
						selectable2 = keyValuePair2.Value;
					}
					else if (keyValuePair2.Key.PondId == pondBindings.Up)
					{
						selectable3 = keyValuePair2.Value;
					}
					else if (keyValuePair2.Key.PondId == pondBindings.Down)
					{
						selectable4 = keyValuePair2.Value;
					}
				}
				Selectable selectable5 = value;
				Navigation navigation = default(Navigation);
				navigation.mode = 4;
				navigation.selectOnUp = selectable3;
				navigation.selectOnDown = selectable4;
				navigation.selectOnLeft = selectable;
				navigation.selectOnRight = selectable2;
				selectable5.navigation = navigation;
			}
			else
			{
				LogHelper.Error("[Programmers] - can't found PondBindings for PondId:{0}", new object[] { keyValuePair.Key.PondId });
			}
		}
	}

	public static void SetPosition(int pondId, float x, float y)
	{
		SetPondsOnGlobalMap._pondsButtonsRts[pondId].position = new Vector3(x, y, SetPondsOnGlobalMap._pondsButtonsRts[pondId].position.z);
	}

	public static void SetNormalizedPosition(int pondId, float x, float y)
	{
		RectTransform rectTransform = SetPondsOnGlobalMap._pondsButtonsRts[pondId].parent as RectTransform;
		SetPondsOnGlobalMap._pondsButtonsRts[pondId].anchoredPosition = new Vector2(x * rectTransform.rect.width, y * rectTransform.rect.height);
	}

	public static void DoFade(int pondId, float alpha, float time)
	{
		ShortcutExtensions.DOKill(SetPondsOnGlobalMap._pondsButtonsCg[pondId], false);
		if (alpha >= 1f && !SetPondsOnGlobalMap.IsActivePondButtons)
		{
			SetPondsOnGlobalMap._pondsButtonsToggle[pondId].gameObject.SetActive(true);
		}
		TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOFade(SetPondsOnGlobalMap._pondsButtonsCg[pondId], alpha, time), 28), delegate
		{
			if (alpha <= 0f && !SetPondsOnGlobalMap.IsActivePondButtons)
			{
				SetPondsOnGlobalMap._pondsButtonsToggle[pondId].gameObject.SetActive(false);
			}
		});
	}

	public static bool IsPondBtnVisible(int pondId)
	{
		return SetPondsOnGlobalMap._pondsButtonsCg[pondId].alpha > 0f;
	}

	public static void SetScalePrc(int pondId, float scalePrc)
	{
		SetPondsOnGlobalMap._pondsButtonsRts[pondId].localScale = Vector3.one * scalePrc / 100f;
	}

	public void SelectActivePond()
	{
		if (EventSystem.current != null && this.ToggleGroup != null)
		{
			Toggle toggle = this.ToggleGroup.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault<Toggle>();
			if (toggle != null)
			{
				UINavigation.SetSelectedGameObject(toggle.gameObject);
			}
		}
	}

	public static void SortPonds()
	{
		List<Transform> list = SetPondsOnGlobalMap._pondsTransforms.OrderBy((Transform e) => e.localScale.x).ToList<Transform>();
		for (int i = 0; i < list.Count; i++)
		{
			list[i].SetSiblingIndex(i);
		}
	}

	private InitPondPin CreateButton(Pond pondBrief, int index, ToggleGroup tg)
	{
		InitPondPin initPondPin;
		if (index < this.PondPinsPool.Length)
		{
			initPondPin = this.PondPinsPool[index];
			initPondPin.transform.SetParent(this.MapTexture.transform);
			initPondPin.transform.localScale = Vector3.one;
			initPondPin.gameObject.SetActive(true);
		}
		else
		{
			initPondPin = Object.Instantiate<InitPondPin>(this.ButtonPondPrefab, this.MapTexture.transform, false);
		}
		initPondPin.name = string.Format("btnPond{0}", pondBrief.PondId);
		initPondPin.Toggle.group = tg;
		initPondPin.GetComponent<DoubleClickAction>().ActionCalled += this.DoubleClick;
		HintElementId componentInChildren = initPondPin.GetComponentInChildren<HintElementId>();
		componentInChildren.SetElementId(pondBrief.Asset, null, null);
		return initPondPin;
	}

	private void DoubleClick(object sender, EventArgs e)
	{
	}

	private void Clear()
	{
		for (int i = 0; i < this.MapTexture.transform.childCount; i++)
		{
			Object.Destroy(this.MapTexture.transform.GetChild(i).gameObject);
		}
	}

	private void OnGotPondUnlocked(int pondId, int accesibleLevel)
	{
		foreach (KeyValuePair<Pond, Toggle> keyValuePair in SetPondsOnGlobalMap._pondsButtons)
		{
			if (keyValuePair.Key.MinLevel <= accesibleLevel)
			{
				keyValuePair.Value.GetComponent<InitPondPin>().SetLocked(false);
			}
		}
	}

	private void ActivateToggle(InitPondPin pin)
	{
		pin.Toggle.isOn = true;
		if (PhotonConnectionFactory.Instance != null)
		{
			pin.GetPondInfo();
		}
	}

	private void SetPondsOnGlobalMap_OnStart()
	{
		if (this._languagesInit != ChangeLanguage.GetCurrentLanguage && !this._firstInited)
		{
			this.SetupPonds();
		}
		base.GetComponent<ActivityState>().OnStart -= this.SetPondsOnGlobalMap_OnStart;
		SetPondsOnGlobalMap.OnActive();
		if (!this._isSubscribed)
		{
			this._isSubscribed = true;
			PhotonConnectionFactory.Instance.OnGotStats += this.OnGotStats;
			PhotonConnectionFactory.Instance.OnGotOpenTournaments += this.PhotonServerOnGotOpenTournaments;
			this.UpdateEvents();
			base.StartCoroutine(this.RequestAllPondsWeather(5f));
			base.StartCoroutine(this.RequestStats(4f));
			base.StartCoroutine(this.GetUgcAndTournaments(2f));
		}
	}

	private IEnumerator RequestStats(float s)
	{
		yield return new WaitForSeconds(s);
		PhotonConnectionFactory.Instance.RequestStats();
		yield break;
	}

	private IEnumerator GetUgcAndTournaments(float s)
	{
		yield return new WaitForSeconds(s);
		TournamentManager tournamentMgr = SetPondsOnGlobalMap.TournamentMgr;
		tournamentMgr.OnRefreshedTournaments = (Action)Delegate.Combine(tournamentMgr.OnRefreshedTournaments, new Action(this.RefreshedTournaments));
		TournamentManager tournamentMgr2 = SetPondsOnGlobalMap.TournamentMgr;
		tournamentMgr2.OnRefreshed = (Action)Delegate.Combine(tournamentMgr2.OnRefreshed, new Action(this.GetUserGeneratedCompetitions));
		SetPondsOnGlobalMap.TournamentMgr.FullRefresh();
		yield break;
	}

	private IEnumerator RequestAllPondsWeather(float s)
	{
		yield return new WaitForSeconds(s);
		PhotonConnectionFactory.Instance.OnGotAllPondWeather += this.Instance_OnGotAllPondWeather;
		PhotonConnectionFactory.Instance.GetAllPondsWeather();
		yield break;
	}

	private void Instance_OnGotAllPondWeather(Dictionary<int, WeatherDesc[]> weatherHash)
	{
		PhotonConnectionFactory.Instance.OnGotAllPondWeather -= this.Instance_OnGotAllPondWeather;
		foreach (KeyValuePair<int, WeatherDesc[]> keyValuePair in weatherHash)
		{
			if (SetPondsOnGlobalMap._pondsButtonsToggle.ContainsKey(keyValuePair.Key))
			{
				SetPondsOnGlobalMap._pondsButtonsToggle[keyValuePair.Key].GetComponent<InitPondPin>().UpdateWeather(keyValuePair.Value);
			}
		}
	}

	private void RefreshedTournaments()
	{
		PhotonConnectionFactory.Instance.GetOpenTournaments(new int?(1), null);
	}

	private void GetUserGeneratedCompetitions()
	{
		this.ClearStateForAll(InitPondPin.PondStates.Ugc);
		this.UpdatePinsState(SetPondsOnGlobalMap.TournamentMgr.UserGeneratedCompetitions.Select((UserCompetitionPublic p) => p.PondId).ToList<int>(), InitPondPin.PondStates.Ugc);
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		SetPondsOnGlobalMap._pondsButtons.Values.ToList<Toggle>().ForEach(delegate(Toggle p)
		{
			if (SetPondsOnGlobalMap.IsActivePondButtons)
			{
				p.gameObject.SetActive(true);
			}
			else
			{
				p.gameObject.SetActive(p.GetComponent<CanvasGroup>().alpha >= 1f);
			}
		});
	}

	private static bool IsActivePondButtons
	{
		get
		{
			return SettingsManager.InputType == InputModuleManager.InputType.GamePad;
		}
	}

	private void OnGotStats(PlayerStats stats)
	{
		PhotonConnectionFactory.Instance.OnGotStats -= this.OnGotStats;
		List<StatAchievement> list = stats.Achievements.Where((StatAchievement p) => p.CurrentStage != null).ToList<StatAchievement>();
		foreach (KeyValuePair<Pond, Toggle> keyValuePair in SetPondsOnGlobalMap._pondsButtons)
		{
			InitPondPin component = keyValuePair.Value.GetComponent<InitPondPin>();
			if (!component.IsLocked)
			{
				Ponds pondId = (Ponds)keyValuePair.Key.PondId;
				int achivementId = PondsMapping.GetPondAchivement(pondId);
				if (achivementId != -1)
				{
					if (!list.Exists((StatAchievement p) => p.AchivementId == achivementId))
					{
						component.AddState(InitPondPin.PondStates.New);
					}
				}
				else
				{
					Debug.LogErrorFormat("[Client DB] OnGotStats - can't found 'first' achivementId for pond [{0} {1}] in dictionary PondsMapping.PondToAchivement", new object[]
					{
						keyValuePair.Key.PondId,
						pondId
					});
				}
			}
		}
	}

	private void UpdateEvents()
	{
		MarketingEvent currentEvent = EventsController.CurrentEvent;
		if (currentEvent != null && currentEvent.Config != null && currentEvent.Config.PondIds != null && currentEvent.Finish > TimeHelper.UtcTime() && currentEvent.Start <= TimeHelper.UtcTime())
		{
			MarketingEventHoliday holiday = currentEvent.Config.Holiday;
			if (this._eventHolidayMap.ContainsKey(holiday))
			{
				this.UpdatePinsState(currentEvent.Config.PondIds.ToList<int>(), this._eventHolidayMap[holiday]);
			}
		}
	}

	private void PhotonServerOnGotOpenTournaments(List<Tournament> tournaments)
	{
		this.ClearStateForAll(InitPondPin.PondStates.Tournament);
		if (tournaments != null)
		{
			List<int> list = (from p in tournaments
				where p.KindId != 3 && this._tournamentStates.Contains(TournamentHelper.GetCompetitionStatus(p))
				select p.PondId).Distinct<int>().ToList<int>();
			this.UpdatePinsState(list, InitPondPin.PondStates.Tournament);
		}
	}

	private void UpdatePinsState(List<int> ponds, InitPondPin.PondStates state)
	{
		(from p in SetPondsOnGlobalMap._pondsButtons
			where ponds.Contains(p.Key.PondId)
			select p.Value.GetComponent<InitPondPin>()).ToList<InitPondPin>().ForEach(delegate(InitPondPin p)
		{
			p.AddState(state);
		});
	}

	private void ClearStateForAll(InitPondPin.PondStates state)
	{
		SetPondsOnGlobalMap._pondsButtons.Select((KeyValuePair<Pond, Toggle> p) => p.Value.GetComponent<InitPondPin>()).ToList<InitPondPin>().ForEach(delegate(InitPondPin p)
		{
			p.RemoveState(state);
		});
	}

	// Note: this type is marked as 'beforefieldinit'.
	static SetPondsOnGlobalMap()
	{
		SetPondsOnGlobalMap.OnActive = delegate
		{
		};
		SetPondsOnGlobalMap._pondsButtons = new Dictionary<Pond, Toggle>();
		SetPondsOnGlobalMap._pondsTransforms = new List<Transform>();
		SetPondsOnGlobalMap._pondsButtonsRts = new Dictionary<int, RectTransform>();
		SetPondsOnGlobalMap._pondsButtonsCg = new Dictionary<int, CanvasGroup>();
		SetPondsOnGlobalMap._pondsButtonsToggle = new Dictionary<int, Toggle>();
	}

	[SerializeField]
	private GlobeHelpInput _globeHelp;

	public GameObject MapTexture;

	public static RawImage MapTextureImage;

	public InitPondPin ButtonPondPrefab;

	[SerializeField]
	private InitPondPin[] PondPinsPool;

	public GameObject ToggleGroup;

	private Language _languagesInit;

	private bool _firstInited;

	private static Dictionary<Pond, Toggle> _pondsButtons;

	private List<int> _debugPins = new List<int>();

	private float _startTransferAt = -1f;

	private static List<Transform> _pondsTransforms;

	private bool _isSubscribed;

	private static Dictionary<int, RectTransform> _pondsButtonsRts;

	private static Dictionary<int, CanvasGroup> _pondsButtonsCg;

	private static Dictionary<int, Toggle> _pondsButtonsToggle;

	private readonly Dictionary<MarketingEventHoliday, InitPondPin.PondStates> _eventHolidayMap = new Dictionary<MarketingEventHoliday, InitPondPin.PondStates>
	{
		{
			MarketingEventHoliday.Helloween,
			InitPondPin.PondStates.Halloween
		},
		{
			MarketingEventHoliday.Independence4thJuly,
			InitPondPin.PondStates.IndependenceDay
		},
		{
			MarketingEventHoliday.NewYear,
			InitPondPin.PondStates.NewYear
		},
		{
			MarketingEventHoliday.StPartick,
			InitPondPin.PondStates.SaintPatrick
		},
		{
			MarketingEventHoliday.ThanksgivingDay,
			InitPondPin.PondStates.ThanksgivingDay
		}
	};

	private readonly IList<TournamentStatus> _tournamentStates = new ReadOnlyCollection<TournamentStatus>(new List<TournamentStatus>
	{
		TournamentStatus.Signed,
		TournamentStatus.RegAndStarting,
		TournamentStatus.ScoreSet,
		TournamentStatus.Registration
	});

	private const bool IsHandNavigation = true;
}
