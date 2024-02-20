using System;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrossHair : MonoBehaviour
{
	private void Awake()
	{
		this._iconInitialPosition = this.IconParent.localPosition;
		base.gameObject.SetActive(false);
		this._isVisible = false;
		this.MainCanvasGroup.alpha = 0f;
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	public void Activate(bool flag)
	{
		if (this._isVisible != flag)
		{
			this._isVisible = flag;
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			float alpha = this.MainCanvasGroup.alpha;
			float num = ((!flag) ? 5f : 2f);
			float num2 = ((!flag) ? (alpha / num) : ((1f - alpha) / num));
			this._actionTill = Time.time + num2;
			this.Update();
		}
	}

	public void SetState(CrossHair.CrossHairState state, bool forceUpdate = false)
	{
		if (GameFactory.Player == null)
		{
			return;
		}
		RodController rodController = ((!(GameFactory.Player.CurrentLookAtPod != null) || !GameFactory.Player.CurrentLookAtPod.IsSlotOccupied(GameFactory.Player.CurrentLookAtSlot)) ? null : GameFactory.Player.CurrentLookAtPod.GetOccupiedSlotData(GameFactory.Player.CurrentLookAtSlot).Rod);
		Rod rod = ((!(rodController != null) || rodController.Behaviour == null) ? null : RodHelper.FindRodInSlot(rodController.Behaviour.RodSlot.Index, null));
		if (this._currentState == state && !forceUpdate && (rod == null || this._lastRod == rod))
		{
			return;
		}
		this._lastRod = rod;
		this._currentState = state;
		DOTween.Kill(this.Icon, false);
		DOTween.Kill(this.IconParent, false);
		this.Icon.color = Color.white;
		this.IconParent.localPosition = this._iconInitialPosition;
		this.IconParent.localRotation = Quaternion.identity;
		bool flag = false;
		switch (state)
		{
		case CrossHair.CrossHairState.TakeStand:
			this.Icon.text = "\ue70b";
			this.JumpingComponent.enabled = false;
			this._isFiller = true;
			this.FullRingCG.alpha = 1f;
			this.SectoredRingCG.alpha = 0f;
			this.HelpText.text = string.Format(ScriptLocalization.Get("TakeRodStandHelp"), HotkeyIcons.GetIcoByActionName("RodStandCancel", out flag));
			break;
		case CrossHair.CrossHairState.ChangeRodOnStand:
			this.Icon.text = "\ue71a";
			TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(this.IconParent, Vector3.forward * 180f, 1f, 3), 28), -1, 2);
			this.JumpingComponent.enabled = false;
			this._isFiller = true;
			this.FullRingCG.alpha = 1f;
			this.SectoredRingCG.alpha = 0f;
			if (this._lastRod != null)
			{
				this.HelpText.text = string.Format(ScriptLocalization.Get("ReplaceRodOnStandHelp"), HotkeyIcons.GetIcoByActionName("RodStandSubmit", out flag), this._lastRod.Name, RodHelper.FindRodInHands().Name);
				if (SettingsManager.InputType != InputModuleManager.InputType.GamePad)
				{
					ShowHudElements.Instance.ShowHintCount("RodStandReplaceWithKeysHelp", string.Format(ScriptLocalization.Get("RodStandReplaceWithKeysHelp"), HotkeyIcons.GetIcoByActionName("RodStandStandaloneHotkeyModifier", out flag), GameFactory.Player.CurrentLookAtSlot + 1), 5);
				}
			}
			break;
		case CrossHair.CrossHairState.TakeRod:
			this.Icon.text = "\ue71c";
			if (rodController != null && rodController.Behaviour != null && rodController.Behaviour.IsFishHooked)
			{
				this.Icon.color = Color.green;
			}
			TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.OnStepComplete<Tweener>(ShortcutExtensions.DOFade(this.Icon, 1f, 0.5f), delegate
			{
				this.Icon.text = ((!(this.Icon.text == "\ue71b")) ? "\ue71b" : "\ue71c");
			}), -1, 2);
			this.JumpingComponent.enabled = false;
			this._isFiller = true;
			this.FullRingCG.alpha = 1f;
			this.SectoredRingCG.alpha = 0f;
			if (this._lastRod != null)
			{
				this.HelpText.text = string.Format(ScriptLocalization.Get("TakeRodFromStandHelp"), HotkeyIcons.GetIcoByActionName("RodStandSubmit", out flag), this._lastRod.Name);
				if (SettingsManager.InputType != InputModuleManager.InputType.GamePad)
				{
					ShowHudElements.Instance.ShowHintCount("RodStandTakeWithKeysHelp", string.Format(ScriptLocalization.Get("RodStandTakeWithKeysHelp"), HotkeyIcons.GetIcoByActionName("RodStandStandaloneHotkeyModifier", out flag), GameFactory.Player.CurrentLookAtSlot + 1), 5);
				}
			}
			break;
		case CrossHair.CrossHairState.PutRod:
			this.Icon.text = "\ue70f";
			this.JumpingComponent.enabled = true;
			this._isFiller = false;
			this.FullRingCG.alpha = 0f;
			this.SectoredRingCG.alpha = 1f;
			this.HelpText.text = string.Format(ScriptLocalization.Get("PutRodOnStandHelp"), HotkeyIcons.GetIcoByActionName("RodStandSubmit", out flag));
			if (GameFactory.Player.CurrentLookAtPod != null && SettingsManager.InputType != InputModuleManager.InputType.GamePad)
			{
				ShowHudElements.Instance.ShowHintCount("RodStandPutWithKeysHelp", string.Format(ScriptLocalization.Get("RodStandPutWithKeysHelp"), HotkeyIcons.GetIcoByActionName("RodStandStandaloneHotkeyModifier", out flag), (GameFactory.Player.CurrentLookAtSlot < 0) ? (GameFactory.Player.CurrentLookAtPod.FindFreeSlot() + 1) : (GameFactory.Player.CurrentLookAtSlot + 1)), 5);
			}
			break;
		case CrossHair.CrossHairState.TakeBoat:
		{
			this.Icon.text = "\ue733";
			this.JumpingComponent.enabled = (this._isFiller = false);
			CanvasGroup fullRingCG = this.FullRingCG;
			float num = 0f;
			this.SectoredRingCG.alpha = num;
			fullRingCG.alpha = num;
			this._helpTextLeft.text = HotkeyIcons.GetIcoByActionName("InteractObject", out flag);
			break;
		}
		case CrossHair.CrossHairState.None:
		{
			this.JumpingComponent.enabled = (this._isFiller = false);
			CanvasGroup fullRingCG2 = this.FullRingCG;
			float num = 0f;
			this.SectoredRingCG.alpha = num;
			fullRingCG2.alpha = num;
			TMP_Text icon = this.Icon;
			string text = string.Empty;
			this._helpTextLeft.text = text;
			text = text;
			this.HelpText.text = text;
			icon.text = text;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
	}

	private void Update()
	{
		if (this._isFiller && ((this._currentState == CrossHair.CrossHairState.TakeStand && ControlsController.ControlsActions.RodStandCancel.IsPressedMandatory) || (this._currentState != CrossHair.CrossHairState.TakeStand && ControlsController.ControlsActions.RodStandSubmit.IsPressedMandatory)))
		{
			float num = ((!ControlsController.ControlsActions.RodStandCancel.IsPressedMandatory) ? (2f * this.fillTime) : this.fillTime);
			this.FilledRing.fillAmount = Mathf.Min(1f, Mathf.Sqrt(num));
			this.fillTime += Time.deltaTime;
		}
		else
		{
			this.FilledRing.fillAmount = 0f;
			this.fillTime = 0f;
		}
		if (this._actionTill > 0f)
		{
			if (this._actionTill < Time.time)
			{
				this._actionTill = -1f;
				if (!this._isVisible)
				{
					base.gameObject.SetActive(false);
					this.SetState(CrossHair.CrossHairState.None, false);
				}
			}
			float num2 = Mathf.Max(this._actionTill - Time.time, 0f);
			float num3 = ((!this._isVisible) ? 5f : 2f);
			float num4 = 1f - Mathf.Clamp01(num2 * num3);
			this.MainCanvasGroup.alpha = ((!this._isVisible) ? Mathf.Lerp(1f, 0f, num4) : Mathf.Lerp(0f, 1f, num4));
		}
	}

	private void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.SetState(this._currentState, true);
	}

	private const string openedHandIcon = "\ue71c";

	private const string closedHandIcon = "\ue71b";

	private const string replaceIcon = "\ue71a";

	private const string downArrowIcon = "\ue70f";

	private const string rodStandIcon = "\ue70b";

	private const string TakeBoatIcon = "\ue733";

	[SerializeField]
	private TextMeshProUGUI Icon;

	[SerializeField]
	private Transform IconParent;

	[SerializeField]
	private TextMeshProUGUI HelpText;

	[SerializeField]
	private TextMeshProUGUI _helpTextLeft;

	[SerializeField]
	private ArrowJamping JumpingComponent;

	[SerializeField]
	private CanvasGroup MainCanvasGroup;

	[SerializeField]
	private CanvasGroup FullRingCG;

	[SerializeField]
	private CanvasGroup SectoredRingCG;

	[SerializeField]
	private Image FilledRing;

	private Vector3 _iconInitialPosition;

	private const float SPEED = 2f;

	private const float OFF_SPEED = 5f;

	private bool _isVisible;

	private float _actionTill = -1f;

	private InventoryItem _lastRod;

	private CrossHair.CrossHairState _currentState = CrossHair.CrossHairState.None;

	private bool _isFiller;

	private float fillTime;

	public enum CrossHairState : byte
	{
		TakeStand,
		ChangeRodOnStand,
		TakeRod,
		PutRod,
		TakeBoat,
		None
	}
}
