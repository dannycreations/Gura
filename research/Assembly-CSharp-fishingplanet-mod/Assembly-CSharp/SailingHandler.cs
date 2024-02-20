using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SailingHandler : MonoBehaviour, IBoatWithEngineMinigameView
{
	public bool IsEngineIgnitionSuccess
	{
		get
		{
			return this.value >= 1f - this._miniGameZone.fillAmount;
		}
	}

	private void Start()
	{
		this._miniGameHelpColor = this._miniGameHelp.color;
		this._miniGameValueColor = this._miniGameValue.color;
		this.OnInputTypeChanged(SettingsManager.InputType);
		this._miniGame.alpha = 0f;
		this._engineIndicator.SetActive(false);
		GameFactory.Player.OnBoarded += this.OnBoarded;
	}

	private void OnDestroy()
	{
		if (GameFactory.Player != null)
		{
			GameFactory.Player.OnBoarded -= this.OnBoarded;
		}
	}

	private void OnEnable()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		if (GameFactory.Player != null)
		{
			this.OnBoarded(GameFactory.Player.IsSailing);
		}
	}

	private void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void Update()
	{
		this.UpdateSailingInfo();
	}

	public void FinishEngineIgnition()
	{
		this.ResetFinishEngineIgnition();
	}

	public void SetupForEngineIgnition(float v)
	{
		this._miniGameZone.fillAmount = Mathf.Clamp01(v);
	}

	public void SetValueAndIndication(float v)
	{
		this.SetValue(v);
		this._miniGameValue.fillAmount = this.value;
	}

	public void SetValue(float v)
	{
		this.value = Mathf.Clamp01(v);
		this._miniGameValueLine.fillAmount = Mathf.Clamp(v, 0.008f, 1f);
		float num = 314.5f;
		float num2 = 470f - num * v;
		this._miniGameLine2.anchoredPosition = new Vector2(this._miniGameLine2.anchoredPosition.x, num2);
		Color miniGameHelpColor = this._miniGameHelpColor;
		if (!this.IsEngineIgnitionSuccess)
		{
			miniGameHelpColor.a = 0.15f;
		}
		Graphic miniGameHelp = this._miniGameHelp;
		Color color = miniGameHelpColor;
		this._miniGameHelpCtrl.color = color;
		miniGameHelp.color = color;
	}

	public void SetActiveEngine(bool flag)
	{
		this._engineIndicator.SetActive(flag);
		this.Boat.SetActive(flag);
		this.Kayak.SetActive(!flag);
	}

	public void FinishEngineIgnitionWithHighlight()
	{
		ShortcutExtensions.DOKill(this._miniGameValue, false);
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOColor(this._miniGameValue, Color.clear, 0.35f), delegate
		{
			ShortcutExtensions.DOColor(this._miniGameValue, this._miniGameValueColor, 0f);
			this.ResetFinishEngineIgnition();
		});
	}

	public void ResetFinishEngineIgnition()
	{
		Graphic miniGameHelp = this._miniGameHelp;
		Color miniGameHelpColor = this._miniGameHelpColor;
		this._miniGameHelpCtrl.color = miniGameHelpColor;
		miniGameHelp.color = miniGameHelpColor;
		this._miniGameZone.fillAmount = 0f;
		this._miniGameValueLine.fillAmount = 0.008f;
		this._miniGameValue.fillAmount = 0f;
		this._miniGameLine2.anchoredPosition = new Vector2(this._miniGameLine2.anchoredPosition.x, 470f);
	}

	public void SetActiveMinigame(bool flag)
	{
		ShortcutExtensions.DOKill(this._bar, false);
		ShortcutExtensions.DOKill(this._miniGame, false);
		float num = ((!flag) ? 0f : 1f);
		ShortcutExtensions.DOFade(this._bar, 1f - num, 0.25f);
		ShortcutExtensions.DOFade(this._miniGame, num, 0.25f);
	}

	public void SetEngineIndicatorOn(bool value)
	{
		this._engineIndicator.SetTurnedOn(value, 0.2f);
	}

	public void IndicateFail()
	{
		this._engineIndicator.IndicateFail();
	}

	public void ShakeEngine(bool success)
	{
		this._engineIndicator.Shake(success);
	}

	private void OnBoarded(bool boarded)
	{
		this._isBoarded = boarded;
		if (boarded)
		{
			this.HideAll();
			this.SetUpHUD();
			this.UpdateSailingInfo();
		}
		if (GameFactory.Player == null || GameFactory.Player.IsBoatFishing || !boarded)
		{
			this._alphaPanel.HidePanel();
		}
		else
		{
			this._alphaPanel.ShowPanel();
		}
	}

	private void SetUpHUD()
	{
		if (GameFactory.Player.IsEnginePresent)
		{
			this._engineIndicator.SetTurnedOn(Mathf.Abs(GameFactory.Player.Stamina) > 0f, 0f);
			this.Engine.SetActive(true);
			ShortcutExtensions.DOFade(this.StaminaPercentage, 0f, 0f);
			ShortcutExtensions.DOFade(this.ThrottleParent, 1f, 0f);
			this.maxPositiveThrottleHeight = -this.ThrottleRect.anchoredPosition.y;
			this.maxNegativeThrottleHeight = (this.ThrottleParent.transform as RectTransform).rect.height - this.maxPositiveThrottleHeight;
			this.ThrottleNegativeRect.anchoredPosition = Vector2.up * this.maxNegativeThrottleHeight;
		}
		if (GameFactory.Player.IsOarPresent)
		{
			this.StaminaIcon.SetActive(true);
			this.Padlle.SetActive(true);
			ShortcutExtensions.DOFade(this.StaminaPercentage, 1f, 0f);
			ShortcutExtensions.DOFade(this.ThrottleParent, 0f, 0f);
		}
		this.SetActiveEngine(GameFactory.Player.IsEnginePresent);
		this.Anchor.SetActive(true);
	}

	private void HideAll()
	{
		this.EngineAction.SetActive(false);
		this.PaddleAction.SetActive(false);
		this.AnchorAction.SetActive(false);
		this.TrollingAction.SetActive(false);
		this._engineIndicator.SetActive(false);
		this.StaminaIcon.SetActive(false);
		this.Anchor.SetActive(false);
		this.Padlle.SetActive(false);
		this.Kayak.SetActive(false);
		this.Boat.SetActive(false);
		this.Engine.SetActive(false);
	}

	private void UpdateSailingInfo()
	{
		if (this._isBoarded)
		{
			if (GameFactory.Player.IsBoatFishing && this._alphaPanel.Alpha == 1f)
			{
				this._alphaPanel.HidePanel();
			}
			if (!GameFactory.Player.IsBoatFishing && this._alphaPanel.Alpha == 0f)
			{
				this._alphaPanel.ShowPanel();
				ShowHudElements.Instance.ShowingFishingHUD.SetActive(false);
			}
			this.Speed.text = string.Format("{0} {1}", (int)MeasuringSystemManager.Speed(GameFactory.Player.BoatVelocity), MeasuringSystemManager.SpeedSufix());
			this.EngineAction.SetActive(!GameFactory.Player.IsAnchored && GameFactory.Player.IsEnginePresent);
			this.PaddleAction.SetActive(!GameFactory.Player.IsAnchored && !GameFactory.Player.IsEnginePresent);
			this.AnchorAction.SetActive(GameFactory.Player.IsAnchored);
			if (GameFactory.Player.IsEnginePresent)
			{
				this.Stamina.text = string.Empty;
				this.StaminaPercentage.fillAmount = 0f;
				this.ThrottlePercentage.text = string.Format("{0}%", (int)(GameFactory.Player.Stamina * 100f));
				this.ThrottleRect.SetSizeWithCurrentAnchors(1, GameFactory.Player.Stamina * ((GameFactory.Player.Stamina <= 0f) ? this.maxNegativeThrottleHeight : this.maxPositiveThrottleHeight));
				this.ThrottleNegativeRect.SetSizeWithCurrentAnchors(1, -GameFactory.Player.Stamina * this.maxNegativeThrottleHeight);
			}
			else
			{
				this.Stamina.text = string.Format("{0}%", (int)(GameFactory.Player.Stamina * 100f));
				this.StaminaPercentage.fillAmount = GameFactory.Player.Stamina;
			}
		}
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		bool flag = type == InputModuleManager.InputType.Mouse;
		TextMeshProUGUI textMeshProUGUI = ((!flag) ? this._miniGameHelpCtrl : this._miniGameHelp);
		bool flag2;
		textMeshProUGUI.text = HotkeyIcons.GetIcoByActionName("IgnitionForward", out flag2);
		this._miniGameHelp.gameObject.SetActive(flag);
		this._miniGameHelpCtrl.gameObject.SetActive(!flag);
	}

	[SerializeField]
	private CanvasGroup _bar;

	[SerializeField]
	private CanvasGroup _miniGame;

	[SerializeField]
	private BoatEngineIndication _engineIndicator;

	[SerializeField]
	private TextMeshProUGUI _miniGameHelp;

	[SerializeField]
	private TextMeshProUGUI _miniGameHelpCtrl;

	[SerializeField]
	private RectTransform _miniGameLine2;

	[SerializeField]
	private Image _miniGameValue;

	[SerializeField]
	private Image _miniGameValueLine;

	[SerializeField]
	private Image _miniGameZone;

	[SerializeField]
	private AlphaFade _alphaPanel;

	[SerializeField]
	private GameObject EngineAction;

	[SerializeField]
	private GameObject PaddleAction;

	[SerializeField]
	private GameObject AnchorAction;

	[SerializeField]
	private GameObject TrollingAction;

	[SerializeField]
	private Text Speed;

	[SerializeField]
	private Text Stamina;

	[SerializeField]
	private GameObject StaminaIcon;

	[SerializeField]
	private Image StaminaPercentage;

	[SerializeField]
	private GameObject Anchor;

	[SerializeField]
	private GameObject Padlle;

	[SerializeField]
	private GameObject Kayak;

	[SerializeField]
	private GameObject Boat;

	[SerializeField]
	private GameObject Engine;

	private float maxPositiveThrottleHeight = 264f;

	private float maxNegativeThrottleHeight = 144f;

	[SerializeField]
	private CanvasGroup ThrottleParent;

	[SerializeField]
	private TextMeshProUGUI ThrottlePercentage;

	[SerializeField]
	private RectTransform ThrottleRect;

	[SerializeField]
	private RectTransform ThrottleNegativeRect;

	private float value;

	private const float MinigameHelpAlphaHidden = 0.15f;

	private const float MinigameAnimTime = 0.25f;

	private const float MinigameLineMin = 0.008f;

	private const float ImY0 = 470f;

	private const float ImY1 = 155.5f;

	private Color _miniGameHelpColor;

	private Color _miniGameValueColor;

	private bool _isBoarded;
}
