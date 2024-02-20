using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseCompetitions : MainPageItem
{
	protected UgcMenuStateManager MenuMgr
	{
		get
		{
			return MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager;
		}
	}

	protected virtual void Awake()
	{
		this.RightMenu.alpha = 0f;
		this.Localize();
	}

	protected override void OnEnable()
	{
		ShortcutExtensions.DOKill(this.UpButtons, false);
		this.UpButtons.anchoredPosition = new Vector2(-1200f, 0f);
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOAnchorPos(this.UpButtons, Vector2.zero, 0.25f, false), new TweenCallback(this.UpButtonsAnimCompleted));
	}

	protected override void OnDisable()
	{
		this.DateSelectedGo.SetActive(false);
		this.UpButtons.anchoredPosition = new Vector2(-1200f, 0f);
	}

	protected virtual void Update()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.Mouse && this.TournamentId != null && this.PointerActiveTournamentId != null)
		{
			int? tournamentId = this.TournamentId;
			int valueOrDefault = tournamentId.GetValueOrDefault();
			int? pointerActiveTournamentId = this.PointerActiveTournamentId;
			if (valueOrDefault == pointerActiveTournamentId.GetValueOrDefault() && tournamentId != null == (pointerActiveTournamentId != null))
			{
				this.CurDblClickTime += Time.deltaTime;
			}
		}
	}

	public virtual void Search()
	{
		this.IsSearching = true;
		this.TrySearch();
	}

	public virtual void Filter()
	{
	}

	public virtual void ResetAll()
	{
		this.IsSearching = false;
		this.SearchField.text = string.Empty;
		foreach (KeyValuePair<int, UesrCompetitionItem> keyValuePair in this.Data)
		{
			keyValuePair.Value.SetActive(true);
		}
		if (this.TournamentId != null)
		{
			if (!this.Data.ContainsKey(this.TournamentId.Value))
			{
				this.TournamentId = null;
				this.SelectNew();
				return;
			}
			int siblingIndex = this.Data[this.TournamentId.Value].GetSiblingIndex();
			float num = (float)siblingIndex / (float)this.ItemsParent.transform.childCount;
			if (this.ItemsScrollbar.direction == 2)
			{
				num = 1f - num;
			}
			base.StartCoroutine(this.Scroll(this.ItemsScrollRect, num));
		}
		else
		{
			this.SelectNew();
		}
	}

	public virtual void Join()
	{
	}

	public virtual void ChangeDt(int value)
	{
		if (base.gameObject.activeSelf)
		{
			this.CurrentShowingDate = this.CurrentShowingDate.AddDays((double)value);
			this.Refresh();
		}
	}

	protected virtual void Refresh()
	{
	}

	protected virtual void UpdateSelectedDt()
	{
		this.DateSelected.text = string.Format("{0}, {1}", MeasuringSystemManager.GetFullDayCaption(this.CurrentShowingDate), MeasuringSystemManager.DateTimeShortString(this.CurrentShowingDate));
		this.DateSelected.alpha = ((!this.IsCurrentDay(this.CurrentShowingDate)) ? 0.6f : 1f);
	}

	protected bool IsCurrentDay(DateTime dt)
	{
		DateTime dateTime = TimeHelper.UtcTime();
		return dateTime.Year == dt.Year && dateTime.Month == dt.Month && dateTime.Day == dt.Day;
	}

	protected virtual IEnumerator Scroll(ScrollRect sr, float v)
	{
		yield return new WaitForEndOfFrame();
		sr.verticalNormalizedPosition = v;
		yield break;
	}

	protected virtual IEnumerator StartEdit()
	{
		yield return new WaitForEndOfFrame();
		this.SearchField.Select();
		this.SearchField.ActivateInputField();
		this.SearchField.MoveTextEnd(true);
		yield break;
	}

	protected virtual void Localize()
	{
		this.NameCaption.text = ScriptLocalization.Get("NameCaption");
		this.PondCaption.text = ScriptLocalization.Get("LocationCaption");
		this.SearchField.placeholder.GetComponent<Text>().text = string.Format("{0} ({1},{2})", ScriptLocalization.Get("SearchShopButton"), this.NameCaption.text, this.PondCaption.text);
	}

	protected virtual void UpButtonsAnimCompleted()
	{
		this.DateSelectedGo.SetActive(true);
	}

	protected virtual void TrySearch()
	{
		string text = ((!string.IsNullOrEmpty(this.SearchField.text)) ? this.SearchField.text.Trim(new char[] { ' ' }).ToUpper() : string.Empty);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, UesrCompetitionItem> keyValuePair in this.Data)
		{
			bool flag = keyValuePair.Value.IsSearch(text);
			keyValuePair.Value.SetActive(flag);
			if (flag)
			{
				list.Add(keyValuePair.Key);
			}
		}
		if (list.Count > 0)
		{
			if (this.TournamentId == null)
			{
				PlayButtonEffect.SetToogleOn(true, this.Data[list[0]].Tgl);
			}
			else if (!list.Contains(this.TournamentId.Value))
			{
				PlayButtonEffect.SetToogleOn(false, this.Data[this.TournamentId.Value].Tgl);
				PlayButtonEffect.SetToogleOn(true, this.Data[list[0]].Tgl);
			}
		}
		else
		{
			this.SetActiveRightMenu(false);
			this.TournamentId = null;
		}
	}

	protected UesrCompetitionItem CurCompetition
	{
		get
		{
			return (!this.Data.ContainsKey(this.Id())) ? null : this.Data[this.Id()];
		}
	}

	protected virtual int Id()
	{
		int? tournamentId = this.TournamentId;
		return (tournamentId == null) ? 0 : tournamentId.Value;
	}

	protected virtual void UpdateCurCompetition()
	{
	}

	protected virtual void Clear()
	{
		for (int i = 0; i < this.ItemsParent.transform.childCount; i++)
		{
			Object.Destroy(this.ItemsParent.transform.GetChild(i).gameObject);
		}
		this.Data.Clear();
	}

	protected virtual void SelectNew()
	{
		int? num = null;
		if (this.TournamentId != null)
		{
			num = this.TournamentId;
		}
		else
		{
			List<int> list = this.Data.Keys.ToList<int>();
			if (list.Count > 0)
			{
				num = new int?(list[0]);
			}
		}
		if (num != null)
		{
			PlayButtonEffect.SetToogleOn(false, this.Data[num.Value].Tgl);
			PlayButtonEffect.SetToogleOn(true, this.Data[num.Value].Tgl);
		}
		this.UpdateCurCompetition();
	}

	protected virtual void OnTglValueChanged(bool v, int newTournamentId)
	{
		if (v)
		{
			if (this.TournamentId == newTournamentId)
			{
				if (SettingsManager.InputType == InputModuleManager.InputType.Mouse && base.ShouldUpdate())
				{
					if (this.CurDblClickTime > 0f && this.CurDblClickTime <= 0.5f && this.TournamentId != null && this.PointerActiveTournamentId != null)
					{
						int? tournamentId = this.TournamentId;
						int valueOrDefault = tournamentId.GetValueOrDefault();
						int? pointerActiveTournamentId = this.PointerActiveTournamentId;
						if (valueOrDefault == pointerActiveTournamentId.GetValueOrDefault() && tournamentId != null == (pointerActiveTournamentId != null))
						{
							this.Join();
						}
					}
					this.CurDblClickTime = 0f;
				}
			}
			else
			{
				this.SetActiveRightMenu(false);
				this.TournamentId = new int?(newTournamentId);
				this.UpdateCurCompetition();
			}
		}
	}

	protected virtual void SetActiveRightMenu(bool flag)
	{
		ShortcutExtensions.DOKill(this.RightMenu, false);
		ShortcutExtensions.DOFade(this.RightMenu, (float)((!flag) ? 0 : 1), 0.15f);
	}

	protected virtual void InitItems<T, TN>(List<T> ts) where TN : UesrCompetitionItem
	{
		this.Clear();
		ToggleGroup component = this.ItemsParent.GetComponent<ToggleGroup>();
		for (int i = 0; i < ts.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.ItemsParent, this.ItemPrefab);
			T t = ts[i];
			TN component2 = gameObject.GetComponent<TN>();
			component2.OnPointerActive += delegate(bool v)
			{
				this.PointerActive<T>(v, t);
			};
			component2.Tgl.group = component;
			component2.Tgl.onValueChanged.AddListener(delegate(bool v)
			{
				this.OnTglValueChanged(v, this.GetItemTournamentId<T>(t));
			});
			this.InitItem<T, TN>(t, component2);
			this.Data[this.GetItemTournamentId<T>(t)] = component2;
		}
		if (this.TournamentId != null && !this.Data.ContainsKey(this.TournamentId.Value))
		{
			this.TournamentId = null;
		}
		this.SelectNew();
	}

	protected virtual int GetItemTournamentId<T>(T t)
	{
		return 0;
	}

	protected virtual void InitItem<T, TN>(T ts, TN item) where TN : UesrCompetitionItem
	{
	}

	protected void PointerActive<T>(bool v, T t)
	{
		if (v)
		{
			if (this.PointerActiveTournamentId == null || this.PointerActiveTournamentId.Value != this.GetItemTournamentId<T>(t))
			{
				this.CurDblClickTime = 0f;
				this.PointerActiveTournamentId = new int?(this.GetItemTournamentId<T>(t));
			}
		}
		else if (this.PointerActiveTournamentId != null && this.PointerActiveTournamentId.Value == this.GetItemTournamentId<T>(t))
		{
			this.CurDblClickTime = 0f;
			this.PointerActiveTournamentId = null;
		}
	}

	[SerializeField]
	protected TextMeshProUGUI DateSelected;

	[SerializeField]
	protected GameObject DateSelectedGo;

	[SerializeField]
	protected InputField SearchField;

	[SerializeField]
	protected ScreenKeyboard SearchFieldScreenKeyboard;

	[SerializeField]
	protected Scrollbar ItemsScrollbar;

	[SerializeField]
	protected ScrollRect ItemsScrollRect;

	[SerializeField]
	protected CanvasGroup RightMenu;

	[SerializeField]
	protected RectTransform UpButtons;

	[SerializeField]
	protected GameObject ItemPrefab;

	[SerializeField]
	protected GameObject ItemsParent;

	[Space(5f)]
	[SerializeField]
	protected TextMeshProUGUI NameCaption;

	[SerializeField]
	protected TextMeshProUGUI PondCaption;

	[Space(5f)]
	[SerializeField]
	protected Image Image;

	[SerializeField]
	protected TextMeshProUGUI Name;

	[SerializeField]
	protected TextMeshProUGUI Temp;

	[SerializeField]
	protected TextMeshProUGUI TempIco;

	[SerializeField]
	protected TextMeshProUGUI TempIco2;

	[SerializeField]
	protected TextMeshProUGUI TempWater;

	[SerializeField]
	protected TextMeshProUGUI TempWaterIco;

	[SerializeField]
	protected TextMeshProUGUI Wind;

	[SerializeField]
	protected TextMeshProUGUI EntrySum;

	[SerializeField]
	protected TextMeshProUGUI EntryCurrency;

	protected DateTime CurrentShowingDate;

	protected bool IsInited;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	protected const float PosX0 = -1200f;

	protected const float AnimTime = 0.25f;

	protected const float AnimTimeRightMenu = 0.15f;

	protected const float DblClickTime = 0.5f;

	protected float CurDblClickTime;

	protected bool IsDblClickEnable;

	protected int? PointerActiveTournamentId;

	protected int? TournamentId;

	protected readonly Dictionary<int, UesrCompetitionItem> Data = new Dictionary<int, UesrCompetitionItem>();

	protected bool IsSearching;
}
