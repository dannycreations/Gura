using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.GlobalMap;
using DG.Tweening;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InitPondPin : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	public Toggle Toggle
	{
		get
		{
			return this._toggle;
		}
	}

	public bool IsLocked
	{
		get
		{
			return this._isLocked;
		}
		private set
		{
			if (value != this._isLocked)
			{
				this._isLocked = value;
				this.UpdatePin();
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event InitPondPin.ClickAction OnClicked;

	private void Start()
	{
		this.UpdateState();
		this.UpdateWeather();
	}

	private void OnEnable()
	{
		InitPondPin.OnClicked += this.SetInactiveColor;
	}

	private void OnDisable()
	{
		InitPondPin.OnClicked -= this.SetInactiveColor;
	}

	private void Update()
	{
		if (this._toggle.isOn)
		{
			this.PinImage.color = this.ActiveColor;
		}
		this._timer += Time.deltaTime;
		if (this._timer >= this._maxTime && this._pond != null)
		{
			this._timer = 0f;
			this._lockEndTime = PondHelper.LockEndTime(this._pond);
			bool flag = GlobalMapHelper.IsActive(this._pond);
			if (flag && this.InState(InitPondPin.PondStates.InDeveloping))
			{
				this.RemoveState(InitPondPin.PondStates.InDeveloping);
				this.UpdateWeather();
			}
			else if (!flag && !this.InState(InitPondPin.PondStates.InDeveloping))
			{
				this.AddState(InitPondPin.PondStates.InDeveloping);
				this.UpdateWeather();
			}
			if (this._lockEndTime == null && this.InState(InitPondPin.PondStates.InDeveloping))
			{
				this._lockEndTime = GlobalMapHelper.GetActivationTime(this._pond.PondId);
			}
		}
		DateTime? lockEndTime = this._lockEndTime;
		if (lockEndTime != null && this._lockEndTime.Value > TimeHelper.UtcTime())
		{
			this.UnlockTime.gameObject.SetActive(true);
			this.UnlockTime.text = this._lockEndTime.Value.GetTimeFinishInValue(true);
			if (this.IsLocked)
			{
				this.SetLocked(false);
			}
		}
		else
		{
			this.UnlockTime.gameObject.SetActive(false);
			bool flag2 = this._pond.PondLocked();
			if (!this.IsLocked && flag2)
			{
				this.SetLocked(true);
			}
			else if (this.IsLocked && !flag2)
			{
				this.SetLocked(false);
			}
			if (!this.IsLocked)
			{
				bool flag3 = this._pond.PondPaidLocked();
				if ((flag3 && !this._lockedIcon.activeSelf) || (!flag3 && this._lockedIcon.activeSelf))
				{
					this._lockedIcon.SetActive(flag3);
					this.UpdateWeather();
					this.SetLocked(this.IsLocked);
				}
			}
		}
		this.RotationPin();
	}

	private bool IsRotationInBack { get; set; }

	public void SetLocked(bool flag)
	{
		this.IsLocked = flag;
		bool flag2 = this._pond.PondPaidLocked();
		this.PinImage.color = ((!flag && GlobalMapHelper.IsActive(this._pond)) ? ((!flag2) ? this._normalColor : this.PaidColor) : this.InActiveColor);
		this._lockedIcon.SetActive(flag || flag2);
		this._paidIco.gameObject.SetActive(flag2);
		this._weatherIco.gameObject.SetActive(!this._lockedIcon.gameObject.activeSelf);
	}

	private void UpdatePin()
	{
		this.PondLevel.gameObject.SetActive(this._isLocked);
	}

	public void Init(Pond pondBrief)
	{
		this.HighlightObject.SetActive(pondBrief.HasAction);
		string text = pondBrief.Name.ToUpper();
		int num = text.IndexOf("-", StringComparison.Ordinal);
		if (num != -1)
		{
			num++;
			text = string.Format("{0}\n{1}", text.Substring(0, num), text.Substring(num));
		}
		this.PondName.text = text;
		this.PondLevel.text = pondBrief.OriginalMinLevel.ToString();
		this._pond = pondBrief;
		float num2 = this.PondName.preferredHeight / 26f;
		if (num2 > 1f)
		{
			RectTransform rectTransform = this.PondName.GetComponent<RectTransform>();
			float y = rectTransform.anchoredPosition.y;
			rectTransform = this.UnlockTime.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y - 26f * num2);
		}
		this.Start();
	}

	public void SetColor()
	{
		if (InitPondPin.OnClicked != null)
		{
			InitPondPin.OnClicked();
		}
	}

	public void SetInactiveColor()
	{
		if (!this._toggle.isOn)
		{
			this.PinImage.color = ((!this.IsLocked && GlobalMapHelper.IsActive(this._pond)) ? ((!this._pond.PondPaidLocked()) ? this._normalColor : this.PaidColor) : this.InActiveColor);
		}
	}

	public void TurnOff()
	{
		this._toggle.isOn = false;
		this.SetInactiveColor();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this._isPointerEnter = true;
		if (!this._toggle.isOn)
		{
			this.Pin.GetComponent<Shadow>().enabled = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this._isPointerEnter = false;
		this.Pin.GetComponent<Shadow>().enabled = false;
		if (this.IsLocked || !GlobalMapHelper.IsActive(this._pond))
		{
			this.PinImage.color = this.InActiveColor;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		this.OnPointerEnter(new PointerEventData(EventSystem.current));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.OnPointerExit(new PointerEventData(EventSystem.current));
	}

	public void CallTravelWithConfirmation()
	{
		ShowPondInfo.Instance.TravelInit.OnClickToTravelConfirmed();
	}

	public void GetPondInfo()
	{
		if (this._pond != null)
		{
			CacheLibrary.MapCache.GetPondInfo(this._pond.PondId);
		}
	}

	public void ValueChanged()
	{
		if (this._pond != null && this.InState(InitPondPin.PondStates.InitedAll) && this._toggle.isOn && PhotonConnectionFactory.Instance != null)
		{
			ShowPondInfo.Instance.RequestPondInfo(this._pond.PondId);
			PhotonConnectionFactory.Instance.ChangeSelectedElement(GameElementType.PondPin, this._pond.Asset, null);
		}
	}

	public void AddState(InitPondPin.PondStates s)
	{
		this._state |= s;
		this.UpdateState();
	}

	public void RemoveState(InitPondPin.PondStates s)
	{
		this._state &= ~s;
		this.UpdateState();
	}

	public void UpdateWeather(WeatherDesc[] w)
	{
		if (this._pond != null)
		{
			this._pond.Weather = w;
			this.UpdateWeather();
		}
	}

	private void UpdateState()
	{
		this._newPond.alpha = 0f;
		string text = string.Empty;
		if (!this.InState(InitPondPin.PondStates.InDeveloping))
		{
			List<InitPondPin.PondStates> list = InitPondPin.StateIcons.Keys.ToList<InitPondPin.PondStates>();
			for (int i = 0; i < list.Count; i++)
			{
				if (this.InState(list[i]) && (false || (list[i] != InitPondPin.PondStates.Tournament && list[i] != InitPondPin.PondStates.Ugc)))
				{
					text += InitPondPin.StateIcons[list[i]];
				}
			}
		}
		this._stateIco.text = text;
	}

	private void UpdateWeather()
	{
		this._weather = string.Empty;
		if (this._pond != null)
		{
			if (this.InState(InitPondPin.PondStates.InDeveloping))
			{
				this._weather = InitPondPin.StateIcons[InitPondPin.PondStates.InDeveloping];
			}
			else if (this._pond.Weather != null && this._pond.Weather.Length > 0)
			{
				this._weather = WeatherHelper.GetWeatherIcon(this._pond.Weather.First((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString()).Icon);
			}
		}
		this._weatherIco.text = this._weather;
	}

	private bool InState(InitPondPin.PondStates s)
	{
		return (this._state & s) == s;
	}

	private void RotationPin()
	{
		if (!this.IsLocked && !this._pond.PondPaidLocked() && (this.InState(InitPondPin.PondStates.Tournament) || this.InState(InitPondPin.PondStates.Ugc)))
		{
			float y = this.Pin.transform.rotation.eulerAngles.y;
			bool flag = y >= 90f && y <= 270f;
			if (this._isPointerEnter)
			{
				if (!this.IsRotationInBack)
				{
					this.IsRotationInBack = true;
					ShortcutExtensions.DOKill(this.Pin.transform, true);
					TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DORotate(this.Pin.transform, (!flag) ? Vector3.zero : new Vector3(0f, 180f, 0f), 0.25f, 0), delegate
					{
						this.IsRotationInBack = false;
					});
				}
			}
			else
			{
				ShortcutExtensions.DOKill(this.Pin.transform, true);
				this.Pin.transform.Rotate(Vector3.up, 80f * Time.deltaTime);
			}
			if (flag)
			{
				this._weatherIco.text = ((!this.InState(InitPondPin.PondStates.Ugc)) ? InitPondPin.StateIcons[InitPondPin.PondStates.Tournament] : InitPondPin.StateIcons[InitPondPin.PondStates.Ugc]);
			}
			else
			{
				this._weatherIco.text = this._weather;
			}
		}
		else if (this.Pin.transform.rotation != Quaternion.identity)
		{
			ShortcutExtensions.DOKill(this.Pin.transform, true);
			this.Pin.transform.rotation = Quaternion.identity;
			this._weatherIco.text = this._weather;
		}
	}

	[SerializeField]
	private Text _paidIco;

	[SerializeField]
	private Text _stateIco;

	[SerializeField]
	private Text _weatherIco;

	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private GameObject _lockedIcon;

	[SerializeField]
	private Color _normalColor = Color.white;

	[SerializeField]
	private CanvasGroup _newPond;

	public GameObject HighlightObject;

	public GameObject Pin;

	public Image PinImage;

	public Color ActiveColor;

	public Color InActiveColor;

	public Color PaidColor = new Color(0.40784314f, 0.47843137f, 0.59607846f);

	public Text PondName;

	public Text PondLevel;

	public Text UnlockTime;

	private const float PondNamePreferredHeight = 26f;

	public static readonly Dictionary<InitPondPin.PondStates, string> StateIcons = new Dictionary<InitPondPin.PondStates, string>
	{
		{
			InitPondPin.PondStates.Halloween,
			"\ue68a"
		},
		{
			InitPondPin.PondStates.IndependenceDay,
			"\ue68b"
		},
		{
			InitPondPin.PondStates.SaintPatrick,
			"\ue68c"
		},
		{
			InitPondPin.PondStates.ThanksgivingDay,
			"\ue68d"
		},
		{
			InitPondPin.PondStates.NewYear,
			"\ue68e"
		},
		{
			InitPondPin.PondStates.Tournament,
			"\ue68f"
		},
		{
			InitPondPin.PondStates.New,
			string.Empty
		},
		{
			InitPondPin.PondStates.InDeveloping,
			"\ue69b"
		},
		{
			InitPondPin.PondStates.Ugc,
			"\ue632"
		}
	};

	private InitPondPin.PondStates _state;

	private Pond _pond;

	private bool _isLocked;

	private float _maxTime = 2f;

	private float _timer = 2f;

	private DateTime? _lockEndTime;

	private string _weather;

	private bool _isPointerEnter;

	private const float PinSpeedRotation = 80f;

	private const float PinSpeedRotationBack = 0.25f;

	private const bool IsUsePinRotation = true;

	public delegate void ClickAction();

	[Flags]
	public enum PondStates : ushort
	{
		None = 0,
		New = 1,
		Tournament = 2,
		Halloween = 4,
		SaintPatrick = 8,
		IndependenceDay = 16,
		NewYear = 32,
		ThanksgivingDay = 64,
		InDeveloping = 128,
		InitedAll = 256,
		Ugc = 512
	}
}
