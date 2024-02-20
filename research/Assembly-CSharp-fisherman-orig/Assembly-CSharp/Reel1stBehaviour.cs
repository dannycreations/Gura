using System;
using System.Collections.Generic;
using ObjectModel;
using Phy;
using UnityEngine;

public class Reel1stBehaviour : ReelBehaviour
{
	public Reel1stBehaviour(ReelController owner, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, slot, null)
	{
		this._animation = this._owner.GetComponent<Animation>();
		Reel reel = rodAssembly.ReelInterface as Reel;
		this._owner.maxLoad = reel.MaxLoad * 9.81f;
		this.minReelForce = this._owner.maxLoad;
		this.maxReelForce = this._owner.maxLoad;
		this._owner.speed = reel.Speed;
		if (reel.FrictionSectionsCount != null)
		{
			this._owner.numFrictionSections = reel.FrictionSectionsCount.Value;
		}
		base.InstanceId = reel.InstanceId.ToString();
		this.RestoreSettings(() => PhotonConnectionFactory.Instance.Profile.Settings);
		if (transitionData != null && transitionData.spawnedFish != null)
		{
			this.SetFightMode();
		}
	}

	public new GameObject gameObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.gameObject;
		}
	}

	public bool Armed { get; set; }

	public float MaxLoad
	{
		get
		{
			return this._owner.maxLoad;
		}
	}

	public int FrictionSectionsCount
	{
		get
		{
			return this._owner.numFrictionSections;
		}
	}

	public Animation Animation
	{
		get
		{
			return this._animation;
		}
	}

	public override void Init()
	{
		base.Init();
		this.UpdateLineOnRodLength();
		this._renderers = RenderersHelper.GetAllRenderersForObject<SkinnedMeshRenderer>(this._owner.transform);
	}

	public void SetVisibility(bool flag)
	{
		for (int i = 0; i < this._renderers.Count; i++)
		{
			this._renderers[i].enabled = flag;
		}
	}

	private void UpdateLineOnRodLength()
	{
		base.Rod.LineOnRodLength = (base.Rod.RodTipLocator.transform.position - base.ReelHandler.LineArcLocator.transform.position).magnitude;
	}

	private void RestoreSettings(Func<Dictionary<string, string>> getSettingsFunc)
	{
		this._getSettingsFunc = getSettingsFunc;
		ReelSettingsPersister.ReelSetting reelSettings = ReelSettingsPersister.GetReelSettings(getSettingsFunc(), base.InstanceId);
		this.currentReelSpeedSection = reelSettings.Speed;
		this.reelSpeedBackup = reelSettings.Speed;
		this.currentFrictionSection = reelSettings.Friction;
		this.SetServerFriction();
		this.SetServerReelSpeed();
	}

	public override void Update()
	{
		base.Update();
		if (GameFactory.IsRodAssembling || base.RodSlot.PendingServerOp)
		{
			return;
		}
		RandomSounds randomSounds = ((!base.Line.IsBraid) ? this.monoFluoroTensionSounds : this.braidTensionSounds);
		if (base.Line.AppliedForce / base.Line.MaxLoad > 0.85f)
		{
			if (randomSounds.Source == null || !randomSounds.Source.isPlaying)
			{
				randomSounds.playRandomSound(0.1f);
			}
		}
		else if (randomSounds.Source != null && randomSounds.Source.isPlaying)
		{
			randomSounds.Source.Stop();
		}
		if (base.Rod1st != null && base.Rod1st.Player != null && base.Rod1st.Player.State == typeof(PlayerPhotoMode))
		{
			return;
		}
		if (base.IsReeling)
		{
			if (this.rollSounds.Source == null || !this.rollSounds.Source.isPlaying)
			{
				this.rollSounds.playRandomSound(0f);
				this.rollSounds.Source.loop = true;
				this._reelingFrom = Time.time;
			}
			else
			{
				float num = Time.time - this._reelingFrom;
				float num2 = Mathf.Clamp01(num / 0.5f);
				this.rollSounds.Source.volume = 0.22f * num2;
			}
		}
		if (!base.IsReeling && this.rollSounds.Source != null && this.rollSounds.Source.isPlaying)
		{
			if (this._stopReelingAt < 0f)
			{
				float num3 = Time.time - this._reelingFrom;
				float num4 = Mathf.Min(num3, 0.5f);
				this._stopReelingAt = Time.time + num4;
			}
			else if (this._stopReelingAt > Time.time)
			{
				float num5 = (this._stopReelingAt - Time.time) / 0.5f;
				this.rollSounds.Source.volume = 0.22f * num5;
			}
			else
			{
				this._stopReelingAt = -1f;
				this.rollSounds.Source.Stop();
			}
		}
		if (base.RodSlot.IsInHands)
		{
			float num6 = ControlsController.ControlsActions.GetAxis("Mouse ScrollWheel");
			if (base.Rod1st != null && base.Rod1st.Player != null && base.Rod1st.Player.IsRodPodVisible() && ShowHudElements.Instance.IsAngleMeterActive())
			{
				num6 = 0f;
			}
			if ((num6 > 0f && SettingsManager.MouseWheel == MouseWheelValue.Drag) || (ControlsController.ControlsActions.IncFriction.WasPressed && (!(ShowHudElements.Instance != null) || !ShowHudElements.Instance.IsEquipmentChangeBusy())))
			{
				this.IncrementFriction();
			}
			if ((num6 < 0f && SettingsManager.MouseWheel == MouseWheelValue.Drag) || (ControlsController.ControlsActions.DecFriction.WasPressed && (!(ShowHudElements.Instance != null) || !ShowHudElements.Instance.IsEquipmentChangeBusy())))
			{
				this.DecrementFriction();
			}
			if ((num6 > 0f && SettingsManager.MouseWheel == MouseWheelValue.Reel) || (ControlsController.ControlsActions.IncSpeed.WasPressed && (!(ShowHudElements.Instance != null) || !ShowHudElements.Instance.IsEquipmentChangeBusy())))
			{
				this.IncrementReelSpeed(false);
			}
			if ((num6 < 0f && SettingsManager.MouseWheel == MouseWheelValue.Reel) || (ControlsController.ControlsActions.DecSpeed.WasPressed && (!(ShowHudElements.Instance != null) || !ShowHudElements.Instance.IsEquipmentChangeBusy())))
			{
				this.DecrementReelSpeed(false);
			}
		}
		this.CalculateAppliedForce();
		float num7 = 0f;
		float? num8 = this.priorAppliedForce;
		if (num8 != null && Time.deltaTime != 0f)
		{
			if (this.priorAppliedForce.Value == 0f)
			{
				num7 = base.AppliedForce / Time.deltaTime;
			}
			else
			{
				num7 = (base.AppliedForce - this.priorAppliedForce.Value) / (this.priorAppliedForce.Value * Time.deltaTime);
			}
		}
		this.priorAppliedForce = new float?(base.AppliedForce);
		float num9 = 1f;
		if (base.Tackle != null && base.Tackle.Fish != null && base.AppliedForce > 2f)
		{
			num9 = Mathf.Clamp01(1f - num7 / 2f);
		}
		this.reelSpeedChanger.target = num9;
		this.reelSpeedChanger.update(Time.deltaTime);
		if (this.rollSounds.Source != null)
		{
			if (num9 < 0.1f)
			{
				this.rollSounds.Source.volume = 0f;
			}
			else if (num9 < 0.2f)
			{
				this.rollSounds.Source.volume *= Mathf.Lerp(0f, 0.1f, 1f - (0.2f - num9) / 0.1f);
			}
			else if (num9 < 1f)
			{
				this.rollSounds.Source.volume *= Mathf.Lerp(0.1f, 1f, (num9 - 0.2f) / 0.8f);
			}
		}
		if (this.IsFightingMode && base.Line.IsSlacked && Time.time > this.nextHapticSlackTime)
		{
			if (SettingsManager.VibrateIfNotTensioned)
			{
				base.Rod.TriggerHapticPulseOnRod(0.2f, 0.2f);
			}
			this.nextHapticSlackTime = Time.time + 0.2f;
		}
	}

	public override void CalculateAppliedForce()
	{
		if (base.Rod.RodAssembly.IsRodDisassembled || base.Tackle.IsShowing)
		{
			base.AppliedForce = 0f;
			return;
		}
		float num = base.Rod.AdjustedAppliedForce * 1f * base.Rod.ReelDamper;
		float num2;
		if (base.Tackle.IsClipStrike)
		{
			num *= 1f + 4f * (1f - Mathf.Clamp01(base.Rod.transform.forward.y));
			num2 = num;
		}
		else
		{
			num2 = Mathf.Min(num, this.CurrentFrictionForce);
		}
		base.AppliedForce = this.appliedForceAverager.UpdateAndGet(num2, Time.deltaTime);
	}

	public void IncrementFriction()
	{
		this.currentFrictionSection++;
		if (this.currentFrictionSection > this._owner.numFrictionSections)
		{
			this.currentFrictionSection = this._owner.numFrictionSections;
		}
		this.PersistFriction();
	}

	public void DecrementFriction()
	{
		this.currentFrictionSection--;
		if (this.currentFrictionSection < 0)
		{
			this.currentFrictionSection = 0;
		}
		this.PersistFriction();
	}

	public bool IsForced
	{
		get
		{
			return false;
		}
	}

	private void SaveReelSpeedBackup()
	{
		this.reelSpeedBackup = this.currentReelSpeedSection;
	}

	public void IncrementReelSpeed(bool silent = false)
	{
		if (base.Tackle == null)
		{
			return;
		}
		this.currentReelSpeedSection++;
		if (this.currentReelSpeedSection > this._owner.numReelSpeedSections)
		{
			this.currentReelSpeedSection = this._owner.numReelSpeedSections;
		}
		else
		{
			base.Tackle.FinishDragPeriod();
		}
		if (silent)
		{
			this.SaveReelSpeedBackup();
		}
		this.PersistSpeed();
		this.SaveReelSpeedBackup();
	}

	public void DecrementReelSpeed(bool silent = false)
	{
		if (base.Tackle == null)
		{
			return;
		}
		this.currentReelSpeedSection--;
		if (this.currentReelSpeedSection < 0)
		{
			this.currentReelSpeedSection = 0;
		}
		else
		{
			base.Tackle.FinishDragPeriod();
		}
		if (silent)
		{
			this.SaveReelSpeedBackup();
		}
		this.PersistSpeed();
		this.SaveReelSpeedBackup();
	}

	public float CurrentRelativeSpeed
	{
		get
		{
			return base.SectionToSpeedMultiplier(this.currentReelSpeedSection);
		}
	}

	public override void SetHighSpeedMode()
	{
		if (base.HasMaxSpeed)
		{
			return;
		}
		this.currentReelSpeedSection = this._owner.numReelSpeedSections;
		this.SetServerReelSpeed();
	}

	public void SetNormalSpeedMode()
	{
		if (!this._isFighting && base.HasMaxSpeed && this.reelSpeedBackup != this.currentReelSpeedSection)
		{
			this.currentReelSpeedSection = this.reelSpeedBackup;
			this.SetServerReelSpeed();
		}
	}

	public void BlockLineLengthChange(float duration)
	{
		this.blockTimeStamp = Time.time + duration;
	}

	public float CurrentForce
	{
		get
		{
			float num = (this.maxReelForce - this.minReelForce) / (float)this._owner.numReelSpeedSections;
			float num2 = this.maxReelForce - num * (float)this.currentReelSpeedSection;
			if (num2 < this.minReelForce)
			{
				return this.minReelForce;
			}
			return num2;
		}
	}

	public bool IsOverloaded
	{
		get
		{
			return base.AppliedForce >= this.MaxLoad;
		}
	}

	public float IndicatorForce
	{
		get
		{
			if (!base.IsIndicatorOn || this.forceIndicatorMaxMode == FightIndicator.ThreeBands)
			{
				return 0f;
			}
			return Mathf.Max(new float[]
			{
				base.Line.AppliedForce / base.Line.MaxLoad,
				base.AppliedForce / this._owner.maxLoad,
				base.Rod.AppliedForce / base.Rod.MaxLoad
			});
		}
	}

	public RullerTackleType ForceRuller
	{
		get
		{
			if (!base.IsIndicatorOn || this.forceIndicatorMaxMode == FightIndicator.ThreeBands)
			{
				return RullerTackleType.None;
			}
			float num = base.Rod.AppliedForce / base.Rod.MaxLoad;
			float num2 = base.AppliedForce / this._owner.maxLoad;
			float num3 = base.Line.AppliedForce / base.Line.MaxLoad;
			if (num > num2 && num > num3)
			{
				return RullerTackleType.Rod;
			}
			if (num2 > num && num2 > num3)
			{
				return RullerTackleType.Reel;
			}
			return RullerTackleType.Line;
		}
	}

	public bool IncreaseLineLength()
	{
		if (StaticUserData.RodInHand.IsRodDisassembled)
		{
			return false;
		}
		if (base.CurrentFriction > 0.999f)
		{
			return false;
		}
		float num = base.Rod.AdjustedAppliedForce * base.Rod.ReelDamper;
		float num2 = num / this.CurrentFrictionForce;
		base.UpdateFrictionState(num2);
		if (this.CurrentFrictionForce < num)
		{
			if (this.CurrentFrictionForce < 0.001f)
			{
				this.lineDrainSpeed = this._owner.speed * 4f;
			}
			else
			{
				this.lineDrainSpeed = this._owner.speed * 4f * Mathf.Clamp01(num2 - 1f);
			}
			FishingRodSimulation sim = base.RodSlot.Sim;
			sim.FinalLineLength += this.lineDrainSpeed * Time.deltaTime;
			this.CalcSwiftMoveFrictionMultiplier();
			return true;
		}
		this.lineDrainSpeed = 0f;
		this.swiftMoveAmplifier = 1f;
		return false;
	}

	public float UpdateLineLengthOnReeling()
	{
		if (StaticUserData.RodInHand.IsRodDisassembled)
		{
			return 0f;
		}
		if (base.Line.SecuredLineLength <= base.Line.MinLineLength)
		{
			return 0f;
		}
		if (Time.time < this.blockTimeStamp)
		{
			return 0f;
		}
		float num = base.Rod.AdjustedAppliedForce * base.Rod.ReelDamper * 1f;
		float num2 = Mathf.Lerp(1f, 0f, num / this.CurrentFrictionForce);
		if (base.CurrentFriction < 0.001f)
		{
			if (base.Line.SecuredLineLength >= base.Line.MaxLineLength)
			{
				num2 = 0f;
				this.lineDrainSpeed = 0f;
				base.UpdateFrictionState(0f);
			}
			else
			{
				num2 = 1f;
				if (num > this._owner.LoosenFrictionForce)
				{
					this.lineDrainSpeed = Mathf.Clamp01(num / this._owner.LoosenFrictionForce - 1f) * this._owner.speed;
				}
				else
				{
					this.lineDrainSpeed = 0f;
				}
				FishingRodSimulation sim = base.RodSlot.Sim;
				sim.FinalLineLength += this.lineDrainSpeed * Time.deltaTime;
				if (base.RodSlot.Sim.FinalLineLength > base.Line.MaxLineLength)
				{
					base.RodSlot.Sim.FinalLineLength = base.Line.MaxLineLength;
				}
				base.UpdateFrictionState(2f);
			}
		}
		else if (base.CurrentSpeed <= 0.001f)
		{
			if (base.Line.SecuredLineLength >= base.Line.MaxLineLength)
			{
				num2 = 0f;
				this.lineDrainSpeed = 0f;
				base.UpdateFrictionState(0f);
			}
			else
			{
				this.lineDrainSpeed = base.CurrentSpeed - this._owner.speed * 3f * Mathf.Clamp01(num / this.CurrentFrictionForce - 1f);
				FishingRodSimulation sim2 = base.RodSlot.Sim;
				sim2.FinalLineLength -= this.lineDrainSpeed * Time.deltaTime;
				if (base.RodSlot.Sim.FinalLineLength > base.Line.MaxLineLength)
				{
					base.RodSlot.Sim.FinalLineLength = base.Line.MaxLineLength;
				}
				num2 = 1f;
				base.UpdateFrictionState(num / this.CurrentFrictionForce);
			}
		}
		else
		{
			float num3 = Mathf.Clamp01(1f - base.AppliedForce / (this.CurrentForce * 1f));
			if (num < this.CurrentFrictionForce)
			{
				this.lineDrainSpeed = -this.reelSpeedChanger.value * base.CurrentSpeed * num3;
			}
			else
			{
				this.lineDrainSpeed = Mathf.Clamp01(num / this.CurrentFrictionForce - 1f) * this._owner.speed * 3f;
			}
			num2 = this.reelSpeedChanger.value * num3;
			if (this.lineDrainSpeed > 0f && base.Line.SecuredLineLength >= base.Line.MaxLineLength)
			{
				num2 = 1f;
				this.lineDrainSpeed = 0f;
				base.UpdateFrictionState(2f);
			}
			else
			{
				FishingRodSimulation sim3 = base.RodSlot.Sim;
				sim3.FinalLineLength += this.lineDrainSpeed * Time.deltaTime;
				if (base.RodSlot.Sim.FinalLineLength > base.Line.MaxLineLength)
				{
					base.RodSlot.Sim.FinalLineLength = base.Line.MaxLineLength;
				}
				if (this.lineDrainSpeed < 0f)
				{
					this.lineDrainSpeed = 0f;
				}
				base.UpdateFrictionState(num / this.CurrentFrictionForce);
			}
		}
		this.CalcSwiftMoveFrictionMultiplier();
		if (this.rollSounds.Source != null)
		{
			this.rollSounds.Source.pitch = Mathf.Lerp(0.5f, 1.3f, num2);
		}
		return num2 * this.CurrentRelativeSpeed / 1.2f;
	}

	private void CalcSwiftMoveFrictionMultiplier()
	{
		if (this.lineDrainSpeed > 0f && base.Rod.TipMoveSpeedFromTackle > this.lineDrainSpeed)
		{
			this.swiftMoveAmplifier = Mathf.Clamp(base.Rod.TipMoveSpeedFromTackle / (4f * this.lineDrainSpeed), 1f, 20f);
		}
		else
		{
			this.swiftMoveAmplifier = 1f;
		}
	}

	public bool IsFightingMode
	{
		get
		{
			return this._isFighting;
		}
	}

	public override void SetFightMode()
	{
		if (this._isFighting)
		{
			return;
		}
		this._isFighting = true;
		this.currentReelSpeedSection = this._owner.numReelSpeedSections;
		this.SetServerReelSpeed();
	}

	public override void SetDragMode()
	{
		if (!this._isFighting)
		{
			return;
		}
		this._isFighting = false;
		this.currentReelSpeedSection = this.reelSpeedBackup;
		this.SetServerReelSpeed();
	}

	public override void OnItemBroken()
	{
		base.IsIndicatorOn = false;
	}

	private void PersistFriction()
	{
		ReelSettingsPersister.SetReelFriction(this._getSettingsFunc(), base.InstanceId, this.currentFrictionSection);
		this.SetServerFriction();
	}

	private void PersistSpeed()
	{
		ReelSettingsPersister.SetReelSpeed(this._getSettingsFunc(), base.InstanceId, this.currentReelSpeedSection);
		if (this.currentReelSpeedSection != this.reelSpeedBackup)
		{
			this.SetServerReelSpeed();
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.ReelSpeedChangeClip, SettingsManager.InterfaceVolume);
		}
	}

	private void SetServerFriction()
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		PhotonConnectionFactory.Instance.ChangeIndicator(GameIndicatorType.FrictionPosition, this.currentFrictionSection);
	}

	private void SetServerReelSpeed()
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		if (base.Rod1st != null && base.Rod1st.Player != null && (base.Rod1st.Player.IsRodActive || base.Rod1st.Player.IsBoatFishing))
		{
			PhotonConnectionFactory.Instance.ChangeIndicator(GameIndicatorType.ReelSpeed, this.currentReelSpeedSection);
		}
	}

	private float? priorAppliedForce;

	private float swiftMoveAmplifier;

	private readonly ValueChanger reelSpeedChanger = new ValueChanger(0f, 0f, 2f, new float?(2f));

	private Animation _animation;

	private List<SkinnedMeshRenderer> _renderers;

	private Func<Dictionary<string, string>> _getSettingsFunc;

	private float _reelingFrom = -1f;

	private float _stopReelingAt = -1f;

	private const float hapticSlackTimeINterval = 0.2f;

	private float nextHapticSlackTime;

	private float blockTimeStamp = -1f;

	private int reelSpeedBackup = 1;

	private bool _isFighting;
}
