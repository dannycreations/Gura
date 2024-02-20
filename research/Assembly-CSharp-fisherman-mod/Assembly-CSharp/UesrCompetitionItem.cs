using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UesrCompetitionItem : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnPointerActive = delegate(bool b)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool, int> OnTglValueChanged = delegate(bool b, int i)
	{
	};

	public Toggle Tgl { get; protected set; }

	public UserCompetitionPublic Сompetition { get; private set; }

	protected virtual void Awake()
	{
		this.Tgl = base.GetComponent<Toggle>();
		this.Tgl.onValueChanged.AddListener(delegate(bool v)
		{
			if (this.Сompetition != null)
			{
				this.OnTglValueChanged(v, this.Сompetition.TournamentId);
			}
		});
		this.CursorChanger.OnPointerActive += this.CursorChanger_OnPointerActive;
	}

	public void Init(UserCompetitionPublic c)
	{
		this.PrepareMaterials();
		this.Сompetition = c;
		UserCompetitionHelper.GetDefaultName(this.Name, c);
		this.SetPondName(c.PondId);
		this._lockedIco.SetActive(c.IsPrivate);
		this._level.text = UserCompetitionHelper.GetLevel(c);
		TMP_Text entryFee = this._entryFee;
		double? entranceFee = c.EntranceFee;
		entryFee.text = ((entranceFee == null) ? 0.0 : entranceFee.Value).ToString(CultureInfo.InvariantCulture);
		this._currency.text = MeasuringSystemManager.GetCurrencyIcon(c.Currency);
		this._type.text = UserCompetitionHelper.GetTypeIco(c);
		this._joined.text = UserCompetitionHelper.GetJoined(c);
		if (!c.IsSponsored)
		{
			this.ResetMaterials();
		}
		this.UpdateStatus();
	}

	public void Clear()
	{
		this.Сompetition = null;
		this.SetActive(false);
		PlayButtonEffect.SetToogleOn(false, this.Tgl);
	}

	public void ClearEvents()
	{
		this.OnPointerActive = delegate(bool b)
		{
		};
		this.OnTglValueChanged = delegate(bool b, int i)
		{
		};
	}

	public virtual bool IsSearch(string key)
	{
		return this.Pond.text.Contains(key) || this.Name.text.ToUpper().Contains(key);
	}

	public virtual void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	public virtual void SetSiblingIndex(int i)
	{
		base.gameObject.transform.SetSiblingIndex(i);
	}

	public virtual int GetSiblingIndex()
	{
		return base.gameObject.transform.GetSiblingIndex();
	}

	protected virtual void UpdateStatus()
	{
		this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.None];
		this.StatusIco.color = this.NormalColor;
		this._levelIco.color = new Color(this._levelIco.color.r, this._levelIco.color.g, this._levelIco.color.b, (!this.Сompetition.IsEnded) ? 1f : 0.3f);
		this._imageHost.color = new Color(this._imageHost.color.r, this._imageHost.color.g, this._imageHost.color.b, (!this.Сompetition.IsEnded) ? 1f : 0.3f);
		if (this.Сompetition.IsEnded)
		{
			this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.Finished];
			this.Status.text = ScriptLocalization.Get((!this.Сompetition.IsCanceled) ? "FinishedCaption" : "CanceledStatusText");
			this.Status.color = this.NormalColor;
		}
		else if (this.Сompetition.IsStarted || this.Сompetition.IsRegistered)
		{
			this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.Registered];
			Graphic status = this.Status;
			Color color = this.RegColor;
			this.StatusIco.color = color;
			status.color = color;
			this.Status.text = ScriptLocalization.Get((!this.Сompetition.IsStarted) ? "UGC_AppliedPlayersStateCaption" : "RunningStatusText");
		}
		else
		{
			this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.RegistrationOpened];
			Graphic status2 = this.Status;
			Color color = this.RegColor;
			this.StatusIco.color = color;
			status2.color = color;
			if (this.Сompetition.SortType == UserCompetitionSortType.Automatic)
			{
				DateTime dateTime = this.Сompetition.FixedStartDate.Value.ToLocalTime();
				this.Status.text = string.Format(ScriptLocalization.Get("StartTournamentTimeText"), MeasuringSystemManager.TimeString(dateTime));
			}
			else
			{
				this.Status.text = ScriptLocalization.Get("UGC_CompetitionOpen");
			}
			if (this.Сompetition.IsSponsored)
			{
				if (this.SponsoredStatusLoc.ContainsKey(this.Сompetition.ApproveStatus))
				{
					this.Status.text = ScriptLocalization.Get(this.SponsoredStatusLoc[this.Сompetition.ApproveStatus]);
				}
				if (this.SponsoredStatusColors.ContainsKey(this.Сompetition.ApproveStatus))
				{
					Graphic status3 = this.Status;
					color = this.SponsoredStatusColors[this.Сompetition.ApproveStatus];
					this.StatusIco.color = color;
					status3.color = color;
				}
			}
		}
		TMP_Text status4 = this.Status;
		float num = ((!this.Сompetition.IsEnded) ? 1f : 0.3f);
		this._joined.alpha = num;
		num = num;
		this._type.alpha = num;
		num = num;
		this._currency.alpha = num;
		num = num;
		this._entryFee.alpha = num;
		num = num;
		this._level.alpha = num;
		num = num;
		this.Name.alpha = num;
		num = num;
		this.Pond.alpha = num;
		num = num;
		this.StatusIco.alpha = num;
		status4.alpha = num;
		if (this.Сompetition.IsSponsored)
		{
			UserCompetitionHelper.SetSponsoredMaterial(this._type, "Angler_icon_v3 SDF_Sponsored");
		}
		this._imageHost.gameObject.SetActive(UserCompetitionHelper.IsOwnerHost(this.Сompetition));
		this.StatusIco.gameObject.SetActive(!this._imageHost.gameObject);
	}

	protected virtual void SetPondName(int pondId)
	{
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == pondId);
		if (pond != null)
		{
			this.Pond.text = pond.Name.ToUpper();
		}
	}

	private void CursorChanger_OnPointerActive(bool v)
	{
		this.OnPointerActive(v);
	}

	private void PrepareMaterials()
	{
		if (this._defaultNameSharedMaterial == null)
		{
			this._defaultNameSharedMaterial = this.Name.fontSharedMaterial;
			this._defaultNameColor = this.Name.color;
			this._defaultTypeSharedMaterial = this._type.fontSharedMaterial;
			this._defaultTypeColor = this._type.color;
		}
	}

	private void ResetMaterials()
	{
		this.Name.fontSharedMaterial = this._defaultNameSharedMaterial;
		this.Name.color = this._defaultNameColor;
		this._type.fontSharedMaterial = this._defaultTypeSharedMaterial;
		this._type.color = this._defaultTypeColor;
	}

	[SerializeField]
	protected TextMeshProUGUI StatusIco;

	[SerializeField]
	protected Image Image;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	protected TextMeshProUGUI Name;

	[SerializeField]
	protected TextMeshProUGUI Pond;

	[SerializeField]
	protected TextMeshProUGUI Status;

	[SerializeField]
	protected ChangeCursor CursorChanger;

	[SerializeField]
	private TextMeshProUGUI _level;

	[SerializeField]
	private TextMeshProUGUI _type;

	[SerializeField]
	private TextMeshProUGUI _currency;

	[SerializeField]
	private TextMeshProUGUI _entryFee;

	[SerializeField]
	private TextMeshProUGUI _joined;

	[SerializeField]
	private Image _imageHost;

	[SerializeField]
	private Image _levelIco;

	[SerializeField]
	private GameObject _lockedIco;

	private Material _defaultNameSharedMaterial;

	private Material _defaultTypeSharedMaterial;

	private Color _defaultNameColor;

	private Color _defaultTypeColor;

	protected readonly Dictionary<UesrCompetitionItem.States, string> StatesIco = new Dictionary<UesrCompetitionItem.States, string>
	{
		{
			UesrCompetitionItem.States.None,
			"\ue787"
		},
		{
			UesrCompetitionItem.States.Registered,
			"\ue783"
		},
		{
			UesrCompetitionItem.States.RegistrationOpened,
			"\ue788"
		},
		{
			UesrCompetitionItem.States.Closed,
			"\ue630"
		},
		{
			UesrCompetitionItem.States.Finished,
			"\ue63d"
		}
	};

	protected readonly Color NormalColor = new Color(0.96862745f, 0.96862745f, 0.96862745f);

	protected readonly Color RegColor = new Color(0.49411765f, 0.827451f, 0.12941177f);

	protected readonly Color FinishedColor = new Color(0.6666667f, 0.6666667f, 0.6666667f);

	protected static readonly Color ApprovedColor = new Color(0.3529412f, 0.8f, 1f);

	protected static readonly Color InReviewColor = new Color(1f, 0.827451f, 0.46666667f);

	protected static readonly Color DeclinedColor = new Color(0.6745098f, 0.28235295f, 0.21176471f);

	private const float FinishedAlpha = 0.3f;

	protected readonly Dictionary<UserCompetitionApproveStatus, string> SponsoredStatusLoc = new Dictionary<UserCompetitionApproveStatus, string>
	{
		{
			UserCompetitionApproveStatus.InReview,
			"UGC_CompetitionInReview"
		},
		{
			UserCompetitionApproveStatus.PublishedForReview,
			"UGC_CompetitionInReview"
		},
		{
			UserCompetitionApproveStatus.Approved,
			"UGC_CompetitionApproved"
		},
		{
			UserCompetitionApproveStatus.Declined,
			"BuoyDeclined"
		},
		{
			UserCompetitionApproveStatus.InDevelopment,
			"PondInDevelopingBtn"
		}
	};

	protected readonly Dictionary<UserCompetitionApproveStatus, Color> SponsoredStatusColors = new Dictionary<UserCompetitionApproveStatus, Color>
	{
		{
			UserCompetitionApproveStatus.InReview,
			UesrCompetitionItem.InReviewColor
		},
		{
			UserCompetitionApproveStatus.PublishedForReview,
			UesrCompetitionItem.InReviewColor
		},
		{
			UserCompetitionApproveStatus.Approved,
			UesrCompetitionItem.ApprovedColor
		},
		{
			UserCompetitionApproveStatus.Declined,
			UesrCompetitionItem.DeclinedColor
		},
		{
			UserCompetitionApproveStatus.InDevelopment,
			UesrCompetitionItem.InReviewColor
		}
	};

	protected enum States : byte
	{
		None,
		Registered,
		RegistrationOpened,
		Closed,
		Finished
	}
}
