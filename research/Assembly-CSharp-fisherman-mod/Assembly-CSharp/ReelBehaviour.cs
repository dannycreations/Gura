using System;
using UnityEngine;

public class ReelBehaviour
{
	public ReelBehaviour(ReelController owner, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		this._owner = owner;
		this._reelHandler = this._owner.GetComponent<ReelHandler>();
		this.ReelType = rodAssembly.ReelInterface.ReelType;
		this.RodSlot = slot;
		if (transitionData != null)
		{
			this.currentReelSpeedSection = transitionData.reelSpeedSection;
			this.currentFrictionSection = transitionData.reelFrictionSection;
		}
		this.Rod1st = this.Rod as Rod1stBehaviour;
	}

	public ReelController Owner
	{
		get
		{
			return this._owner;
		}
	}

	public GameObject gameObject
	{
		get
		{
			return this._owner.gameObject;
		}
	}

	public string InstanceId { get; set; }

	public bool IsReeling { get; set; }

	public float LineDrainSpeed
	{
		get
		{
			return this.lineDrainSpeed;
		}
	}

	public int CurrentFrictionSection
	{
		get
		{
			return this.currentFrictionSection;
		}
	}

	public int CurrentReelSpeedSection
	{
		get
		{
			return this.currentReelSpeedSection;
		}
	}

	public bool HasMaxSpeed
	{
		get
		{
			return this.currentReelSpeedSection == this._owner.numReelSpeedSections;
		}
	}

	public bool IsIndicatorOn { get; set; }

	public float LineAdjustedFrictionForce
	{
		get
		{
			return Mathf.Min(this.CurrentFrictionForce, this._owner.maxLoad / this.Rod.ReelDamper);
		}
	}

	public bool IsFrictioning { get; private set; }

	public float AppliedForce { get; protected set; }

	public ReelHandler ReelHandler
	{
		get
		{
			return this._reelHandler;
		}
	}

	public ReelTypes ReelType { get; private set; }

	private protected GameFactory.RodSlot RodSlot { protected get; private set; }

	protected RodBehaviour Rod
	{
		get
		{
			return this.RodSlot.Rod;
		}
	}

	private protected Rod1stBehaviour Rod1st { protected get; private set; }

	protected LineBehaviour Line
	{
		get
		{
			return this.RodSlot.Line;
		}
	}

	protected TackleBehaviour Tackle
	{
		get
		{
			return this.RodSlot.Tackle;
		}
	}

	public virtual void Init()
	{
	}

	public void UpdateFrictionState(float relativeForce = 0f)
	{
		if (relativeForce > 1f && !this.IsLineDepleted)
		{
			this.IsFrictioning = true;
			if (this.frictionSounds.Source == null || !this.frictionSounds.Source.isPlaying)
			{
				this.frictionSounds.playRandomSound(0.22f);
				this.frictionSounds.Source.loop = true;
			}
			this.frictionSounds.Source.pitch = Mathf.Clamp(relativeForce, 1f, 3f);
		}
		else
		{
			this.IsFrictioning = false;
			if (this.frictionSounds != null && this.frictionSounds.Source != null && this.frictionSounds.Source.isPlaying)
			{
				this.frictionSounds.Source.Stop();
			}
		}
	}

	public void StopSounds()
	{
		if (this.rollSounds != null)
		{
			this.rollSounds.StopSound();
		}
		if (this.frictionSounds != null)
		{
			this.frictionSounds.StopSound();
		}
		if (this.braidTensionSounds != null)
		{
			this.braidTensionSounds.StopSound();
		}
		if (this.monoFluoroTensionSounds != null)
		{
			this.monoFluoroTensionSounds.StopSound();
		}
	}

	public void SetMuteSounds(bool mute = true)
	{
		if (this.rollSounds != null)
		{
			this.rollSounds.SetMuteSound(mute);
		}
		if (this.frictionSounds != null)
		{
			this.frictionSounds.SetMuteSound(mute);
		}
		if (this.braidTensionSounds != null)
		{
			this.braidTensionSounds.SetMuteSound(mute);
		}
		if (this.monoFluoroTensionSounds != null)
		{
			this.monoFluoroTensionSounds.SetMuteSound(mute);
		}
	}

	public void Destroy()
	{
		this.StopSounds();
		this.gameObject.SetActive(false);
		Object.Destroy(this.gameObject);
		this.RodSlot = null;
		this._owner = null;
	}

	public virtual void Start(MonoBehaviour mb)
	{
		this.rollSounds = new RandomSounds("Sounds/Actions/ReelRoll/", mb);
		this.frictionSounds = new RandomSounds("Sounds/Actions/ReelFriction/", mb);
		this.braidTensionSounds = new RandomSounds("Sounds/Actions/LineTension/Braid/sfx_line_tension_b", mb);
		this.monoFluoroTensionSounds = new RandomSounds("Sounds/Actions/LineTension/MonoFluoro/sfx_line_tension", mb);
		this.forceIndicatorMaxMode = SettingsManager.FightIndicator;
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public float CurrentSpeed
	{
		get
		{
			return this._owner.speed * this.SectionToSpeedMultiplier(this.currentReelSpeedSection);
		}
	}

	public float SectionToSpeedMultiplier(int speedSection)
	{
		switch (speedSection)
		{
		case 0:
			return -1.25f;
		case 1:
			return 1f;
		case 2:
			return 1.5f;
		case 3:
			return 2.5f;
		case 4:
			return 4f;
		default:
			return 0f;
		}
	}

	public bool IsFrictionLoosen
	{
		get
		{
			return this.currentFrictionSection < this._owner.numFrictionSections;
		}
	}

	public virtual void CalculateAppliedForce()
	{
		float num = this.Rod.AdjustedAppliedForce * 1f * this.Rod.ReelDamper;
		num = Mathf.Min(num, this.CurrentFrictionForce);
		this.AppliedForce = this.appliedForceAverager.UpdateAndGet(num, Time.deltaTime);
	}

	public void ResetAppledForce()
	{
		this.appliedForceAverager.Clear();
	}

	public float CurrentFriction
	{
		get
		{
			if (this.IsLineDepleted)
			{
				return 1f;
			}
			float num = (this._owner.maxFriction - this._owner.minFriction) / (float)this._owner.numFrictionSections;
			return Mathf.Min(this._owner.minFriction + num * (float)this.currentFrictionSection, this._owner.maxFriction);
		}
	}

	public bool IsLineDepleted
	{
		get
		{
			return this.Line.SecuredLineLength >= this.Line.MaxLineLength;
		}
	}

	public virtual float CurrentFrictionForce
	{
		get
		{
			return Mathf.Max(this._owner.maxLoad * 0.01f, this.IsLineDepleted ? float.MaxValue : Mathf.Min(this._owner.maxLoad * this.CurrentFriction, this.Owner.maxLoad - 0.05f));
		}
	}

	public virtual void SetFightMode()
	{
	}

	public virtual void SetDragMode()
	{
	}

	public virtual void SetHighSpeedMode()
	{
	}

	public virtual void OnItemBroken()
	{
	}

	protected ReelController _owner;

	protected RandomSounds rollSounds;

	protected RandomSounds frictionSounds;

	protected RandomSounds braidTensionSounds;

	protected RandomSounds monoFluoroTensionSounds;

	protected const float FrictionSoundVolume = 1f;

	protected const float MaxRelativeForceSpeed = 2f;

	protected const float MinReelForceToInfluenceSpeed = 2f;

	public const float FrictioningSpeedMultiplier = 4f;

	protected const float FrictioningSpeedMultiplierWhenReeling = 3f;

	protected const float LINE_TENSION_TRESHHOLD = 0.85f;

	protected const float LINE_TENSION_VOLUME = 0.1f;

	protected const float REELING_SOUND_VOLUME = 0.22f;

	protected const float REELING_SOUND_FADE_TIME = 0.5f;

	protected const float FRICTION_SOUND_VOLUME = 0.22f;

	protected const float MIN_REELING_PITCH = 0.5f;

	protected const float MAX_REELING_PITCH = 1.3f;

	protected const float FORCED_REELING_ZERO_VOLUME_FROM_SPEED = 0.1f;

	protected const float FORCED_REELING_LOW_VOLUME_FROM_SPEED = 0.2f;

	protected const float FORCED_REELING_MAX_VOLUME_FROM_SPEED = 1f;

	protected const float FORCED_REELING_LOW_VOLUME_VALUE_K = 0.1f;

	protected float lineDrainSpeed;

	protected int currentFrictionSection = 3;

	protected int currentReelSpeedSection = 1;

	protected readonly TimedAverager appliedForceAverager = new TimedAverager(0.2f, 100);

	protected float minReelForce;

	protected float maxReelForce;

	protected FightIndicator forceIndicatorMaxMode;

	protected ReelHandler _reelHandler;
}
