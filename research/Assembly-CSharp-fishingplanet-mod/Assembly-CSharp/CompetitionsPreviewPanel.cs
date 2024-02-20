using System;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionsPreviewPanel : ActivityStateControlled
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnJoinFinish = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnShowDetails = delegate
	{
	};

	public string JoinFinishBtnText
	{
		get
		{
			return this._joinFinishBtnText.text;
		}
	}

	private void Awake()
	{
		this.SetDefaultWidth();
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (this._tournament != null)
		{
			if (this._tournament.IsDone || (this._tournament.EndDate < TimeHelper.UtcTime() && this._tournament.IsStarted))
			{
				this.Finish();
			}
			else if (this._tournament.IsActive)
			{
				this.Started();
			}
			else if (this._tournament.StartDate > TimeHelper.UtcTime())
			{
				if (this._tournament.RegistrationStart > TimeHelper.UtcTime())
				{
					this.RegistrationCountdown();
				}
				else
				{
					this.StartsIn();
				}
			}
			else if (this._tournament.EndDate > TimeHelper.UtcTime() && !this._tournament.IsActive)
			{
				this.StartsIn();
			}
		}
		this.CheckTexts();
	}

	public void JoinFinish()
	{
		if (this._multyCount > 1)
		{
			this.OnShowDetails();
		}
		else
		{
			this.OnJoinFinish();
		}
	}

	public void ShowDetails()
	{
		this.OnShowDetails();
	}

	public void Clear()
	{
		LogHelper.Log("___kocha CompetitionsPreviewPanel:Clear");
		this._multyCount = 0;
		this._tournament = null;
	}

	public void SetActive(bool isActive)
	{
		if (base.gameObject.activeSelf != isActive)
		{
			base.gameObject.SetActive(isActive);
		}
	}

	public bool IsJoinFinishBtnChildOf(Transform t)
	{
		return t.IsChildOf(this._joinFinishBtn.transform);
	}

	public void JoinFinishBtnClick()
	{
		this._joinFinishBtn.onClick.Invoke();
	}

	public void SetStatus(TournamentStatus s, bool isJoined, bool isFinished)
	{
		LogHelper.Log("___kocha CoMapCtrl---CompetitionsPreviewPanel:SetStatus {0} isJoined:{1} isFinished:{2}", new object[] { s, isJoined, isFinished });
		if (s == TournamentStatus.RegAndStarting)
		{
			ColorBlock colors = this._joinFinishBtn.colors;
			colors.normalColor = (isJoined ? this._finishIdleColor : this._joinIdleColor);
			colors.highlightedColor = (isJoined ? this._finishSelectedColor : this._joinSelectedColor);
			colors.pressedColor = (isJoined ? this._finishPressedColor : this._joinPressedColor);
			this._joinFinishBtn.colors = colors;
			this._joinFinishBtn.interactable = (!isFinished && (!isJoined || TournamentHelper.ProfileTournament != null)) || this._multyCount > 1;
			if (this._multyCount == 1 && this._joinFinishBtn.interactable && this._tournament != null && this._tournament.IsUgc())
			{
				this._joinFinishBtn.interactable = UserCompetitionHelper.IsUgcEnabled;
			}
			this._joinFinishBtnText.text = ScriptLocalization.Get((!isJoined) ? "JoinTitle" : "FinishCaption");
			this._joinFinishBtn.gameObject.SetActive(true);
		}
		else
		{
			this._joinFinishBtn.interactable = false;
			this._joinFinishBtn.gameObject.SetActive(false);
		}
	}

	public void SetActiveMulty(int multyCount)
	{
		if (this._multyCount != multyCount)
		{
			this._multyCount = multyCount;
			bool flag = this._multyCount > 1;
			if (flag)
			{
				this._competitionMultyText.text = ScriptLocalization.Get("MySportsEventsCaption");
				this._counterMultyText.text = string.Format("({0})", multyCount);
			}
			else
			{
				TMP_Text competitionMultyText = this._competitionMultyText;
				string empty = string.Empty;
				this._counterMultyText.text = empty;
				competitionMultyText.text = empty;
			}
			this._competitionMultyText.gameObject.SetActive(flag);
			this._counterMultyText.gameObject.SetActive(flag);
			this._thumbnail.gameObject.SetActive(!flag);
			this._topAlignedText.gameObject.SetActive(!flag);
			this._bottomAlignedText.gameObject.SetActive(!flag);
		}
	}

	public void PushTournament(Tournament t)
	{
		if (t != null)
		{
			if (this._tournament == null || this._tournament.TournamentId != t.TournamentId)
			{
				this.SetDefaultWidth();
			}
			if (t.IsUgc())
			{
				this.UpdateBottomAlignedText(t.IsRegistered);
				UserCompetitionPublic userCompetitionPublic = TournamentManager.Instance.UserGeneratedCompetitions.FirstOrDefault((UserCompetitionPublic p) => p.TournamentId == t.TournamentId);
				this._topAlignedText.text = ((userCompetitionPublic == null) ? t.Name : UserCompetitionHelper.GetDefaultName(userCompetitionPublic));
				if (!t.IsSponsored)
				{
					this._topAlignedText.text = string.Format("<color=#FFFFFF>{0}</color>", this._topAlignedText.text);
				}
				if (this._topAlignedText.alignment != 257)
				{
					this._topAlignedText.alignment = 257;
				}
			}
			else
			{
				this.UpdateBottomAlignedText(t.IsRegistered);
				this._topAlignedText.text = string.Format("<color=#00FF00>\ue789 {0}</color>", t.Name);
			}
			this._thumbnailLdbl.Load(t.ImageBID, this._thumbnail, "Textures/Inventory/{0}");
		}
		this._tournament = t;
		if (this.defaultWidth <= 0f)
		{
			this.SetDefaultWidth();
		}
		this.CheckTexts();
	}

	public void ClearEvents()
	{
		this.OnJoinFinish = delegate
		{
		};
		this.OnShowDetails = delegate
		{
		};
	}

	private string GetTimerValue(DateTime dateTime)
	{
		TimeSpan timeSpan = dateTime - TimeHelper.UtcTime();
		if (dateTime.Year - TimeHelper.UtcTime().Year < 1 && timeSpan.TotalSeconds > 0.0)
		{
			return timeSpan.GetFormated(true, true);
		}
		if (Time.time - this.lastTime >= 0.4f)
		{
			this.waitingIndex++;
			if (this.waitingIndex >= this.waiting.Length)
			{
				this.waitingIndex = 0;
			}
			this.lastTime = Time.time;
		}
		return this.waiting[this.waitingIndex];
	}

	private void Finish()
	{
		this._topAlignedText.text = string.Format("<color=#D0D0D0>{0}</color>", ScriptLocalization.Get("FinishedStatusText"));
		if (this._topAlignedText.alignment != 257)
		{
			this._topAlignedText.alignment = 257;
		}
		this._bottomAlignedText.text = string.Format("<color=#FFEE44>{0}</color>", ScriptLocalization.Get("ViewInfoTitle"));
	}

	private void RegistrationCountdown()
	{
		this._topAlignedText.text = string.Format(ScriptLocalization.Get("RegTimeTournamentOpensIn"), string.Format("\n<color=#FFEE44>\ue807 {0}</color>\n", this.GetTimerValue(this._tournament.RegistrationStart))).Trim();
		if (this._topAlignedText.alignment != 4097)
		{
			this._topAlignedText.alignment = 4097;
		}
		this._bottomAlignedText.text = string.Empty;
	}

	private void StartsIn()
	{
		this._topAlignedText.text = string.Format("<color=#D0D0D0>{0}\n\ue807 {1}</color>", ScriptLocalization.Get("StartsInStatus"), this.GetTimerValue(this._tournament.StartDate));
		if (!this._tournament.IsRegistered)
		{
			if (this._topAlignedText.alignment != 4097)
			{
				this._topAlignedText.alignment = 4097;
			}
			this._bottomAlignedText.text = string.Empty;
		}
		else
		{
			if (this._topAlignedText.alignment != 257)
			{
				this._topAlignedText.alignment = 257;
			}
			this._bottomAlignedText.text = string.Format("<color=#00FF00>{0}</color>", ScriptLocalization.Get("AppliedTournamentCaption"));
		}
	}

	private void Started()
	{
		this._topAlignedText.text = string.Format("<color=#00FF00>\ue789 {0}</color>\n<color=#FFEE44>\ue807 {1}</color>", ScriptLocalization.Get("StartedStatus"), this.GetTimerValue(this._tournament.EndDate));
		if (this._topAlignedText.alignment != 4097)
		{
			this._topAlignedText.alignment = 4097;
		}
		this._bottomAlignedText.text = string.Empty;
	}

	private void SetDefaultWidth()
	{
		if (this._tweener == null)
		{
			this._tweener = base.GetComponent<ToggleTweenEffects>();
		}
		if (this.defaultWidth == 0f)
		{
			this.defaultWidth = (this._topAlignedText.transform.parent as RectTransform).rect.width;
			this.defaultRectWidth = (base.transform as RectTransform).rect.width;
			this.currWidth = this.defaultRectWidth;
		}
		else
		{
			(base.transform as RectTransform).SetSizeWithCurrentAnchors(0, this.defaultRectWidth);
			this.currWidth = this.defaultRectWidth;
			this.currScaleMultiplier = 1f;
		}
		if (this.defaultHeight == 0f)
		{
			this.defaultHeight = (this._topAlignedText.transform.parent as RectTransform).rect.height;
			this.defaultRectHeight = (base.transform as RectTransform).rect.height;
		}
		else
		{
			(base.transform as RectTransform).SetSizeWithCurrentAnchors(1, this.defaultRectHeight);
		}
		if (this.defaultScale == 0f)
		{
			this.defaultScale = base.transform.localScale.x;
		}
		else if (this._shouldShrink)
		{
			base.transform.localScale = this.defaultScale * Vector3.one;
			this.currScaleMultiplier = 1f;
		}
		this.ResetTweenerDefaultScaleIfNecessary();
	}

	private void ResetTweenerDefaultScaleIfNecessary()
	{
		if (this._tweener != null && this._tweener.initialScale.x != this.defaultScale * this.currScaleMultiplier && this.defaultScale != 0f)
		{
			this._tweener.initialScale = Vector3.one * this.defaultScale * this.currScaleMultiplier;
		}
	}

	private void UpdateBottomAlignedText(bool isRegistered)
	{
		if (isRegistered)
		{
			this._bottomAlignedText.text = string.Format("<color=#00FF00>{0}</color>", ScriptLocalization.Get("AppliedTournamentCaption"));
		}
		else
		{
			this._bottomAlignedText.text = string.Format("<color=#FFEE44>{0}</color>", ScriptLocalization.Get("ViewInfoTitle"));
		}
	}

	private void CheckTexts()
	{
		if (this.PreferredHeight > 0.85f * this.defaultHeight)
		{
			this.currWidth += 35f;
			this.currScaleMultiplier *= 0.97f;
			(base.transform as RectTransform).SetSizeWithCurrentAnchors(0, this.currWidth);
			if (this._shouldShrink)
			{
				DOTween.Kill(base.transform, false);
				base.transform.localScale = Vector3.one * this.defaultScale * this.currScaleMultiplier;
			}
			this.ResetTweenerDefaultScaleIfNecessary();
		}
	}

	private float PreferredHeight
	{
		get
		{
			if (this._multyCount > 1)
			{
				return this._competitionMultyText.preferredHeight;
			}
			return this._topAlignedText.preferredHeight + this._bottomAlignedText.preferredHeight;
		}
	}

	[SerializeField]
	private TextMeshProUGUI _joinFinishBtnText;

	[SerializeField]
	private ColorCodedButton _joinFinishBtn;

	[SerializeField]
	private TextMeshProUGUI _competitionMultyText;

	[SerializeField]
	private TextMeshProUGUI _counterMultyText;

	[SerializeField]
	private TextMeshProUGUI _topAlignedText;

	[SerializeField]
	private TextMeshProUGUI _bottomAlignedText;

	[SerializeField]
	private Image _thumbnail;

	[SerializeField]
	private bool _shouldShrink;

	[Space(5f)]
	[SerializeField]
	private Color _joinIdleColor = new Color(1f, 0.32f, 0.168f);

	[SerializeField]
	private Color _joinSelectedColor = new Color(1f, 0.32f, 0.168f);

	[SerializeField]
	private Color _joinPressedColor = new Color(1f, 0.32f, 0.168f);

	[SerializeField]
	private Color _finishIdleColor = new Color(1f, 0.32f, 0.168f);

	[SerializeField]
	private Color _finishSelectedColor = new Color(1f, 0.32f, 0.168f);

	[SerializeField]
	private Color _finishPressedColor = new Color(1f, 0.32f, 0.168f);

	private ResourcesHelpers.AsyncLoadableImage _thumbnailLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private int _multyCount;

	private float defaultWidth;

	private float defaultHeight;

	private float defaultScale;

	private float defaultRectWidth;

	private float defaultRectHeight;

	private ToggleTweenEffects _tweener;

	private float currWidth;

	private float currScaleMultiplier = 1f;

	private const string NameFormat = "<color=#00FF00>\ue789 {0}</color>";

	private const string UGCNameFormat = "<color=#FFFFFF>{0}</color>";

	private const float ChangeInterval = 0.4f;

	private readonly string[] waiting = new string[]
	{
		".",
		". .",
		". . .",
		". .",
		".",
		string.Empty
	};

	private float lastTime;

	private int waitingIndex = 5;

	private Tournament _tournament;
}
