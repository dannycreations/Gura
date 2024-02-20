using System;
using System.Collections.Generic;
using ObjectModel;
using Phy;
using UnityEngine;

public class FishAi
{
	public FishAi(FishPortrait portrait, IFishAnimationController animationController, Mass tackleTipMass, GameFactory.RodSlot rodSlot)
	{
		this._portrait = portrait;
		this.animationController = animationController;
		this.tackleTipMass = tackleTipMass;
		this.RodSlot = rodSlot;
		if (this._portrait == null)
		{
			this._portrait = FishPortrait.Default;
		}
		this.ApplyBiteTemplate();
	}

	public IFishObject FishObject { get; private set; }

	public GameFactory.RodSlot RodSlot { get; private set; }

	public bool IsAiOn { get; private set; }

	public FishAiBehavior CurrentBehavior
	{
		get
		{
			return this._currentBehavior;
		}
		set
		{
			this._currentBehavior = value;
		}
	}

	public FishBehaviorDirection CurrentBehaviorDirection { get; private set; }

	public float CurrentRelativeForce { get; set; }

	public bool IsFishSwiming { get; private set; }

	public bool IsPassive
	{
		get
		{
			return this.CurrentBehavior == FishAiBehavior.StopFight || this.CurrentBehavior == FishAiBehavior.Shocked || this.CurrentBehavior == FishAiBehavior.SurfaceShock;
		}
	}

	public bool IsShocked
	{
		get
		{
			return this.CurrentBehavior == FishAiBehavior.StopFight || this.CurrentBehavior == FishAiBehavior.Shocked || this.CurrentBehavior == FishAiBehavior.SurfaceShock;
		}
	}

	public bool IsPathCompleted
	{
		get
		{
			Vector3? targetPosition = this.TargetPosition;
			if (targetPosition == null)
			{
				return false;
			}
			if (this.CurrentBehavior == FishAiBehavior.KeepPosition)
			{
				return false;
			}
			float magnitude = (this.mouth.Position - this.TargetPosition.Value).magnitude;
			return magnitude <= 0.25f;
		}
	}

	public bool IsMouthMagnetAttracting
	{
		get
		{
			return this.mouthMagnet != null && this.mouthMagnet.IsAttracting;
		}
	}

	private bool IsBehaviorExpired
	{
		get
		{
			return this.behaviourPeriod > 0f && this.timeSpent > this.behaviourPeriod;
		}
	}

	public bool IsOnPod
	{
		get
		{
			return !this.RodSlot.IsInHands;
		}
	}

	public float CurrentMaxSpeed
	{
		get
		{
			return this._currentMaxSpeed;
		}
	}

	public void Start(IFishObject fishObject)
	{
		this.FishObject = fishObject;
		this.IsAiOn = true;
		this.surfaceShocked = false;
		this.RefreshMasses();
		this.CurrentForce = this.Force;
		this.minForce = this.Force * 0.1f;
		this._currentMaxSpeed = this.Speed;
		this.CurrentRelativeForce = 1f;
		this.CurrentBehavior = FishAiBehavior.None;
		if (this._portrait.IsScript)
		{
			return;
		}
		this.initialJerkTrail = this.GetPortraitTrail(FishAiBehavior.InitialJerk);
		this.keepPositionTrail = this.GetPortraitTrail(FishAiBehavior.KeepPosition);
		this.escapeTrail = this.GetPortraitTrail(FishAiBehavior.Escape);
		this.floatTrail = this.GetPortraitTrail(FishAiBehavior.Float);
		this.sinkTrail = this.GetPortraitTrail(FishAiBehavior.Sink);
		this.headShakeTrail = this.GetPortraitTrail(FishAiBehavior.HeadShake);
		this.candleTrail = this.GetPortraitTrail(FishAiBehavior.Candle);
		this.eightsTrail = this.GetPortraitTrail(FishAiBehavior.Eights);
		this.jumpOutTrail = this.GetPortraitTrail(FishAiBehavior.JumpOut);
		this.shockedTrail = this.GetPortraitTrail(FishAiBehavior.Shocked);
		this.surfaceShockTrail = this.GetPortraitTrail(FishAiBehavior.SurfaceShock);
		FishTrail portraitTrail = this.GetPortraitTrail(FishAiBehavior.StopFight);
		this.stopFightProbability = ((portraitTrail == null) ? 0.5f : portraitTrail.Probability);
		this.allEscapesProbability = ((this.hideTrail != null) ? this.hideTrail.Probability : 0f) + ((this.escapeTrail != null) ? this.escapeTrail.Probability : 0f) + ((this.floatTrail != null) ? this.floatTrail.Probability : 0f) + ((this.sinkTrail != null) ? this.sinkTrail.Probability : 0f);
		this.allManeuvers = new FishTrail[] { this.headShakeTrail, this.candleTrail, this.eightsTrail, this.jumpOutTrail };
		this.allManeuversProbability = ((this.headShakeTrail != null) ? this.headShakeTrail.Probability : 0f) + ((this.candleTrail != null) ? this.candleTrail.Probability : 0f) + ((this.eightsTrail != null) ? this.eightsTrail.Probability : 0f) + ((this.jumpOutTrail != null) ? this.jumpOutTrail.Probability : 0f);
	}

	private FishTrail GetPortraitTrail(FishAiBehavior behavior)
	{
		FishTrail fishTrail = null;
		for (int i = 0; i < this._portrait.Trails.Length; i++)
		{
			FishTrail fishTrail2 = this._portrait.Trails[i];
			if (fishTrail2.Behavior == behavior)
			{
				fishTrail = fishTrail2;
				break;
			}
		}
		return fishTrail;
	}

	public void Attack(float attackDelay, bool isLureTackle)
	{
		this.isLureTackle = isLureTackle;
		this.CurrentBehavior = FishAiBehavior.Attack;
		this.IsAttackDelayed = true;
		this.attackDelay = attackDelay;
		this.attackDelayTimeout = 0f;
	}

	public bool IsAttackDelayed { get; private set; }

	private void UpdateAttackDelay(float dt)
	{
		if (!this.IsAttackDelayed)
		{
			return;
		}
		this.attackDelayTimeout += dt;
		if (this.attackDelayTimeout > this.attackDelay)
		{
			this.IsAttackDelayed = false;
		}
	}

	public void Bite(Magnet mouthMagnet, bool isLureTackle)
	{
		this.isLureTackle = isLureTackle;
		this.mouthMagnet = mouthMagnet;
		this.CurrentBehavior = FishAiBehavior.Bite;
		this.moveForward = true;
	}

	public void Swim()
	{
		this.CurrentBehavior = FishAiBehavior.Swim;
	}

	public void PredatorSwim()
	{
		this.CurrentBehavior = FishAiBehavior.PredatorSwim;
	}

	public void PredatorAttack()
	{
		this.CurrentBehavior = FishAiBehavior.PredatorAttack;
		this.RefreshMasses();
		this.ResetForce();
	}

	public void Hook(float poolPeriod)
	{
		this.RefreshMasses();
		this.ResetForce();
		this.GenerateRandomEscape();
		this.CurrentBehavior = FishAiBehavior.Pool;
		this.timeSpent = 0f;
		this.pathCheckPosition = this.mouth.Position;
		this.pathCheckTimeStamp = 0f;
		this.behaviourPeriod = poolPeriod;
		this.areManeouversDisabled = false;
	}

	public void KeepInMouth(float poolPeriod)
	{
		this.RefreshMasses();
		this.ResetForce();
		this.GenerateEscapePointForPool();
		this.CurrentBehavior = FishAiBehavior.Pool;
		this.timeSpent = 0f;
		this.pathCheckPosition = this.mouth.Position;
		this.pathCheckTimeStamp = 0f;
		this.behaviourPeriod = poolPeriod;
	}

	public void Show()
	{
		this.ResetForce();
		this.animationController.FinishBeating();
		this.animationController.FinishShaking();
		this.CurrentBehavior = FishAiBehavior.None;
		this.IsAiOn = false;
	}

	public void Escape()
	{
		this.RefreshMasses();
		this.ResetForce();
		this.GenerateEscapePointForFinalEscape();
		this.CurrentBehavior = FishAiBehavior.Escape;
		this.timeSpent = 0f;
		this.behaviourPeriod = 5f;
		this.areManeouversDisabled = true;
	}

	public void Stop()
	{
		this.ResetForce();
		this.FishObject = null;
		this.IsAiOn = false;
	}

	public void Update(float dt, float playerForce, Vector3 extPlayerPosition, bool isTackleBeingPulled)
	{
		if (!this.IsAiOn)
		{
			return;
		}
		this.playerPosition = extPlayerPosition;
		this.UpdateAttackDelay(dt);
		if (this.CurrentBehavior == FishAiBehavior.Attack || this.CurrentBehavior == FishAiBehavior.Bite || this.CurrentBehavior == FishAiBehavior.Swim || this.CurrentBehavior == FishAiBehavior.PredatorSwim || this.CurrentBehavior == FishAiBehavior.Wait)
		{
			this.UpdateAttacks(dt, isTackleBeingPulled);
			return;
		}
		if (this.CurrentBehavior == FishAiBehavior.PredatorAttack)
		{
			return;
		}
		this.timeSpent += dt;
		this.isLineTensioned = !this.IsShocked && playerForce > this.CurrentForce * 0.05f;
		this.UpdateStamina();
		this.UpdateManeuvers(dt);
		if (this._portrait.IsScript)
		{
			this.PlayScript();
		}
		else
		{
			if (!this.IsOnPod && GameFactory.Player.IsStriking)
			{
				this.Strike();
			}
			this.ChangeBehavior(playerForce, dt);
		}
		this.ApplyCurrentBehavior();
	}

	private void UpdateAttacks(float dt, bool isTackleBeingPulled)
	{
		if (this.CurrentBehavior == FishAiBehavior.Attack)
		{
			if (this.IsPathCompleted && ((this.tackleTipMass.IsLying && this.isLureTackle) || (this.AttackLure == 1f && !isTackleBeingPulled) || (this.AttackLure == 0f && isTackleBeingPulled)))
			{
				this.CurrentBehavior = FishAiBehavior.Wait;
				this.ResetForce();
			}
			else
			{
				if (this.IsAttackDelayed)
				{
					this._currentMaxSpeed = this.Speed;
				}
				else
				{
					this._currentMaxSpeed = 2f * this.Speed;
				}
				Vector3 normalized = (this.playerPosition - this.mouth.Position).normalized;
				this.TargetPosition = new Vector3?(this.tackleTipMass.Position + (Vector3.down - normalized).normalized * 0.3f);
				float magnitude = (this.TargetPosition.Value - this.mouth.Position).magnitude;
				float num = 1f;
				if (magnitude < 0.25f)
				{
					num = magnitude / 0.25f;
				}
				this.ApplyForce((this.TargetPosition.Value - this.mouth.Position).normalized * this.Force * num, false);
			}
		}
		else if (this.CurrentBehavior == FishAiBehavior.Bite)
		{
			this._currentMaxSpeed = this.Speed * 0.1f;
			this.TargetPosition = new Vector3?(this.tackleTipMass.Position);
			Vector3 targetDirection = this.TargetDirection;
			if (this.IsBiteTemplate)
			{
				if (this.moveForward)
				{
					this.isPaused = targetDirection.magnitude < 0.03f;
					if ((this.isPaused && this.countTastes == 0) || (this.tastePeriod > 0f && this.timeSpent > this.tastePeriod))
					{
						this.moveForward = false;
						if (this.countTastes > 0)
						{
							this.QuitTasting();
						}
						this.CalculateTaste();
						this.TryTaste();
						if (this.mouthMagnet != null)
						{
							this.mouthMagnet.IsAttracting = true;
						}
						this.timeSpent = 0f;
						this.isPaused = false;
					}
				}
				else
				{
					this.isPaused = targetDirection.magnitude > this.RetreatThreshold || this.mouth.Position.y > -0.3f;
					if (this.behaviourPeriod > 0f && this.timeSpent > this.behaviourPeriod)
					{
						this.moveForward = true;
						if (this.mouthMagnet != null)
						{
							this.mouthMagnet.IsAttracting = false;
						}
					}
				}
				this.timeSpent += dt;
			}
			else
			{
				if (this.moveForward && targetDirection.magnitude < 0.03f)
				{
					this.moveForward = false;
					this.retreatDistance = this.RetreatThreshold * Random.Range(0.5f, 1f);
					this.TryGeneratePause();
					if (Random.Range(0f, 1f) < 0.5f)
					{
						this.TryTaste();
						if (this.mouthMagnet != null)
						{
							this.mouthMagnet.IsAttracting = true;
						}
						this.timeSpent = 0f;
						this.tastePeriod = Random.Range(0.5f, 2f);
					}
				}
				else if (!this.moveForward && targetDirection.magnitude > this.retreatDistance)
				{
					this.moveForward = true;
					if (this.mouthMagnet != null)
					{
						this.mouthMagnet.IsAttracting = false;
					}
					this.TryGeneratePause();
				}
				this.timeSpent += dt;
				if (this.isPaused && this.timeSpent > this.behaviourPeriod)
				{
					this.isPaused = false;
				}
				if (this.tastePeriod > 0f && this.timeSpent > this.tastePeriod)
				{
					this.QuitTasting();
					if (this.mouthMagnet != null)
					{
						this.mouthMagnet.IsAttracting = false;
					}
					this.tastePeriod = 0f;
				}
			}
			if (this.isPaused)
			{
				this.ResetForce();
				return;
			}
			float num2 = this.Force * 0.1f;
			if (this.moveForward)
			{
				this.ApplyForce(targetDirection.normalized * num2, false);
			}
			else if (this.IsBiteTemplate && this.countTastes > 0 && this.biteDirection != FishBiteDirection.None)
			{
				this.ApplyForce(this.retreatDirection * num2 * this.factorForce, false);
			}
			else
			{
				this.ApplyForce(targetDirection.normalized * -num2, true);
			}
		}
		else if (this.CurrentBehavior == FishAiBehavior.Swim)
		{
			this._currentMaxSpeed = this.Speed;
			Vector3? targetPosition = this.TargetPosition;
			if (targetPosition != null)
			{
				this.ApplyForce(this.TargetDirection.normalized * this.Force, false);
			}
		}
		else if (this.CurrentBehavior == FishAiBehavior.PredatorSwim)
		{
			this.TargetPosition = new Vector3?(this.tackleTipMass.Position + (this.mouth.Position - this.tackleTipMass.Position).normalized * 0.1f);
			float magnitude2 = (this.mouth.Position - this.tackleTipMass.Position).magnitude;
			float num3 = 1f;
			if (magnitude2 < 0.1f)
			{
				num3 = 0f;
			}
			else if (magnitude2 < 1f)
			{
				float num4 = (magnitude2 - 0.1f) / 0.9f;
				num3 = num4 * 1f + (1f - num4) * 0.1f;
			}
			this.ApplyForce(this.TargetDirection.normalized * this.Force * num3, false);
		}
		else if (this.CurrentBehavior == FishAiBehavior.Wait)
		{
			this.ResetForce();
			if (!this.tackleTipMass.IsLying)
			{
				this.CurrentBehavior = FishAiBehavior.Attack;
			}
		}
	}

	private void TryGeneratePause()
	{
		if (Random.Range(0f, 1f) < 0.8f)
		{
			this.isPaused = true;
			this.behaviourPeriod = Random.Range(0.3f, 1f);
			this.timeSpent = 0f;
		}
	}

	private void TryTaste()
	{
		if (!this.IsOnPod)
		{
			GameFactory.Player.Rod.TriggerHapticPulseOnRod(0.1f, 0.3f);
		}
		if (GameFactory.Water != null)
		{
			GameFactory.Water.AddWaterDisturb(this.tackleTipMass.Position, 0.1f, WaterDisturbForce.Small);
		}
		if (this.IsBiteTemplate)
		{
		}
	}

	private void QuitTasting()
	{
		if (this.IsBiteTemplate)
		{
		}
		if (GameFactory.Water != null)
		{
			GameFactory.Water.AddWaterDisturb(this.tackleTipMass.Position, 0.1f, WaterDisturbForce.Small);
		}
	}

	private void RefreshMasses()
	{
		this.mouth = this.FishObject.Mouth;
		this.root = this.FishObject.Root;
	}

	public void RefreshFishObject(IFishObject fishObject)
	{
		this.FishObject = fishObject;
		this.RefreshMasses();
	}

	private void Strike()
	{
		if (this.striked)
		{
			return;
		}
		this.striked = true;
		if (this.IsBiteTemplate)
		{
		}
		FishTrail[] array = new FishTrail[2];
		float num = 0f;
		if (this.shockedTrail != null)
		{
			num += this.shockedTrail.Probability;
			array[0] = this.shockedTrail;
		}
		if (this.keepPositionTrail != null)
		{
			num += this.keepPositionTrail.Probability;
			float num2 = 0f;
			if (this.Mass > 1f)
			{
				num2 = Mathf.Clamp(0.5f * (this.Mass / 10f), 0f, 0.5f);
			}
			num += num2;
			this.keepPositionTrail.Probability += num2;
			array[1] = this.keepPositionTrail;
		}
		if (num > 0f && Random.Range(0f, 1f) < num)
		{
			FishTrail randomTrail = this.GetRandomTrail(array);
			if (randomTrail == this.shockedTrail)
			{
				this.GoToShocked();
			}
			else if (randomTrail == this.keepPositionTrail)
			{
				this.GoToKeepPosition();
			}
		}
		else if (this.initialJerkTrail != null && Random.Range(0f, 1f) < this.initialJerkTrail.Probability)
		{
			this.GoToInitialJerk();
		}
	}

	private void GoToShocked()
	{
		this.CurrentBehavior = FishAiBehavior.Shocked;
		this.TargetPosition = null;
		this.behaviourPeriod = Random.Range(0.5f, 2f);
		this.timeSpent = 0f;
		this.ResetForce();
	}

	private void GoToSurfaceShock()
	{
		this.CurrentBehavior = FishAiBehavior.SurfaceShock;
		this.TargetPosition = null;
		this.behaviourPeriod = Random.Range(0.5f, 5f);
		this.timeSpent = 0f;
		this.surfaceShocked = true;
		this.ResetForce();
	}

	private void GoToStopFight()
	{
		this.TargetPosition = null;
		this.CurrentBehavior = FishAiBehavior.StopFight;
		this.timeSpent = 0f;
		this.ResetForce();
	}

	private void GoToKeepPosition()
	{
		this.CurrentBehavior = FishAiBehavior.KeepPosition;
		this.TargetPosition = new Vector3?(this.mouth.Position);
		this.behaviourPeriod = Random.Range(0.5f, 3f);
		this.timeSpent = 0f;
	}

	private void GoToInitialJerk()
	{
		this.CurrentBehavior = FishAiBehavior.InitialJerk;
		Vector3 vector = this.mouth.Position - this.playerPosition;
		this.TargetPosition = new Vector3?(this.CorrectTargetPositionDepth(this.mouth.Position + vector.normalized * 5f));
		this.behaviourPeriod = Random.Range(0.5f, 3f);
		this.timeSpent = 0f;
	}

	private void GoToCandle()
	{
		this.TargetPosition = new Vector3?(new Vector3(this.mouth.Position.x, 1f, this.mouth.Position.z));
		this.CurrentBehavior = FishAiBehavior.Candle;
		this.IsManeuver = true;
		this.outOfWater = false;
		this.moveForward = true;
		this.timeSpentInManeuver = 0f;
		this.priorMouthPosition = null;
		this.maxTimeForManeuver = 3f;
		this.animationController.StartSwiming();
	}

	private void GoToJumpOut()
	{
		this.jumpOutShift = this.GetJumpOutShift();
		this.TargetPosition = new Vector3?(new Vector3(this.mouth.Position.x, 1f, this.mouth.Position.z));
		this.CurrentBehavior = FishAiBehavior.JumpOut;
		this.IsManeuver = true;
		this.outOfWater = false;
		this.moveForward = true;
		this.timeSpentInManeuver = 0f;
		this.priorMouthPosition = null;
		this.maxTimeForManeuver = 3f;
		this.animationController.StartSwiming();
	}

	private void GoToHeadShake()
	{
		this.TargetPosition = new Vector3?(new Vector3(this.mouth.Position.x, 1f, this.mouth.Position.z));
		this.CurrentBehavior = FishAiBehavior.HeadShake;
		this.IsManeuver = true;
		this.outOfWater = false;
		this.jumpNumber = 0;
		this.jumps = Random.Range(1, 4);
		this.moveForward = true;
		this.timeSpentInManeuver = 0f;
		this.priorMouthPosition = null;
		this.maxTimeForManeuver = ((this.jumps <= 2) ? 3f : 6f);
		this.animationController.StartSwiming();
	}

	private void GoToEights()
	{
		this.CurrentBehavior = FishAiBehavior.Eights;
		this.moveForward = true;
		this.angle = Quaternion.LookRotation(this.mouth.Position - this.root.Position).eulerAngles.y;
		this.initialAngle = this.angle;
		this.angleWasLess = false;
		this.angularSpeed = Random.Range(120f, 360f);
		this.TargetPosition = null;
		this.timeSpent = 0f;
		this.rotations = 0f;
		this.maxRotations = Random.Range(1f, 3f);
	}

	private void ChangeBehavior(float playerForce, float dt)
	{
		if (this.CurrentBehavior == FishAiBehavior.Hide)
		{
			if (this.TargetDirection.magnitude > 1f)
			{
				this.GenerateRandomBehavior(false);
				return;
			}
			return;
		}
		else if (this.IsManeuver)
		{
			if (this.CheckManeuverFinished())
			{
				this.GenerateRandomBehavior(true);
				return;
			}
			return;
		}
		else
		{
			if (this.CurrentBehavior == FishAiBehavior.Eights && this.rotations > this.maxRotations)
			{
				this.GenerateRandomBehavior(false);
				return;
			}
			Vector3 vector;
			if ((this.CurrentBehavior != FishAiBehavior.Correction || this.IsBehaviorExpired) && this.IsCorrectionNeeded(out vector))
			{
				this.CorrectFishEscapeDirection(vector);
				return;
			}
			this.correctionAttempt = 0;
			if (this.IsBehaviorExpired || this.IsPathCompleted)
			{
				this.GenerateRandomBehavior(false);
				return;
			}
			if (this.behaviourPeriod > 0f)
			{
				return;
			}
			if (this.CheckIfFishIsTiredAndStopFighting(dt, out this.behaviourPeriod))
			{
				this.GoToStopFight();
				return;
			}
			float num = ((!this.IsPassive) ? this.CurrentForce : this.minForce);
			float num2 = playerForce / num - 0.2f;
			if (num2 > 0f && Random.Range(0f, 1f) < (this.Activity * dt * num2 + 0.001f) * 0.33f)
			{
				this.GenerateRandomBehavior(false);
				return;
			}
			Vector3? targetPosition = this.TargetPosition;
			if (targetPosition == null)
			{
				return;
			}
			float magnitude = (this.TargetPosition.Value - this.mouth.Position).magnitude;
			float? num3 = this.priorDistanceToTarget;
			if (num3 != null && this.timeSpent > 1f)
			{
				float? num4 = this.priorDistanceToTarget;
				float? num5 = ((num4 == null) ? null : new float?(num4.GetValueOrDefault() - magnitude));
				float? num6 = ((num5 == null) ? null : new float?(num5.GetValueOrDefault() / dt));
				if (magnitude > this.priorDistanceToTarget || num6 < 0.2f)
				{
					this.GenerateRandomBehavior(false);
				}
			}
			this.priorDistanceToTarget = new float?(magnitude);
			return;
		}
	}

	private void UpdateManeuvers(float dt)
	{
		FishAiBehavior currentBehavior = this.CurrentBehavior;
		switch (currentBehavior)
		{
		case FishAiBehavior.Candle:
			this.TargetPosition = new Vector3?(new Vector3(this.mouth.Position.x, 1f, this.mouth.Position.z));
			if ((double)this.mouth.Position.y > -0.7500000298023224)
			{
				if (Random.Range(0f, 1f) < 0.5f)
				{
					this.animationController.StartShaking();
				}
				else
				{
					this.animationController.StartBeating();
				}
			}
			break;
		case FishAiBehavior.HeadShake:
			this.TargetPosition = new Vector3?(new Vector3(this.mouth.Position.x, 1f, this.mouth.Position.z));
			if (this.mouth.Position.y > -0.6f)
			{
				this.animationController.StartBeating();
			}
			break;
		case FishAiBehavior.JumpOut:
			this.TargetPosition = new Vector3?(new Vector3(this.mouth.Position.x, 1f, this.mouth.Position.z) + this.jumpOutShift);
			if ((double)this.mouth.Position.y > -0.7500000298023224)
			{
				this.animationController.StartSwiming();
			}
			break;
		default:
			if (currentBehavior == FishAiBehavior.Eights)
			{
				float num = this.angularSpeed * dt;
				this.rotations += num / 360f;
				if (this.moveForward)
				{
					this.angle += num;
				}
				else
				{
					this.angle -= num;
				}
				if (this.angle > 360f)
				{
					this.angle -= 360f;
					this.angleWasLess = !this.angleWasLess;
				}
				else if (this.angle < 0f)
				{
					this.angle += 360f;
					this.angleWasLess = !this.angleWasLess;
				}
				if ((this.angleWasLess && this.angle > this.initialAngle) || (!this.angleWasLess && this.angle < this.initialAngle))
				{
					this.moveForward = !this.moveForward;
				}
			}
			break;
		}
	}

	private bool CheckManeuverFinished()
	{
		bool flag = false;
		FishAiBehavior currentBehavior = this.CurrentBehavior;
		if (currentBehavior != FishAiBehavior.HeadShake)
		{
			if (currentBehavior != FishAiBehavior.Candle)
			{
				if (currentBehavior == FishAiBehavior.JumpOut)
				{
					if (this.mouth.Position.y > 0f && !this.outOfWater)
					{
						this.outOfWater = true;
						this.moveForward = false;
						flag = true;
					}
					if (this.outOfWater && this.mouth.Position.y < 0f)
					{
						flag = true;
					}
				}
			}
			else
			{
				if (this.mouth.Position.y > 0f && !this.outOfWater)
				{
					this.outOfWater = true;
					this.moveForward = false;
					flag = true;
				}
				if (this.outOfWater && this.mouth.Position.y < 0f)
				{
					flag = true;
				}
			}
		}
		else
		{
			if (this.mouth.Position.y > 0f && !this.outOfWater)
			{
				this.outOfWater = true;
				this.jumpNumber++;
				this.moveForward = false;
				flag = true;
			}
			if (this.outOfWater && this.mouth.Position.y < 0f)
			{
				if (this.jumpNumber == this.jumps)
				{
					flag = true;
				}
				else
				{
					this.outOfWater = false;
				}
			}
			if (!this.moveForward && this.mouth.Position.y < -0.2f)
			{
				this.moveForward = true;
			}
		}
		if (!flag)
		{
			float? num = this.priorMouthPosition;
			if (num != null)
			{
				float num2 = Mathf.Abs(this.priorMouthPosition.Value - this.mouth.Position.y) / Time.deltaTime;
				if (num2 < 0.2f && Mathf.Abs(this.mouth.Position.y - -0.15f) < 0.1f)
				{
					flag = true;
				}
			}
			this.priorMouthPosition = new float?(this.mouth.Position.y);
		}
		this.timeSpentInManeuver += Time.deltaTime;
		if (!flag && this.timeSpentInManeuver > this.maxTimeForManeuver && this.mouth.Position.y <= 0f)
		{
			return true;
		}
		this.IsManeuver = !flag;
		return flag;
	}

	private void GenerateRandomBehavior(bool avoidManeouvers = false)
	{
		this.timeSpent = 0f;
		this.behaviourPeriod = 0f;
		this.priorDistanceToTarget = null;
		if (this.isTargetAShelter && this.hideTrail != null && this.IsPathCompleted)
		{
			this.CurrentBehavior = FishAiBehavior.Hide;
			this.behaviourPeriod = 0f;
		}
		else if (!this.surfaceShocked && this.mouth.Position.y > -0.15f && this.surfaceShockTrail != null && Random.Range(0f, 1f) < this.surfaceShockTrail.Probability)
		{
			this.GoToSurfaceShock();
		}
		else if (this.CurrentBehavior == FishAiBehavior.Shocked && this.initialJerkTrail != null && Random.Range(0f, 1f) < this.initialJerkTrail.Probability)
		{
			this.GoToInitialJerk();
		}
		else if (Random.Range(0f, this.allEscapesProbability + this.allManeuversProbability) < this.allEscapesProbability || this.CurrentRelativeForce < 0.5f || avoidManeouvers || !this.isLineTensioned || this.areManeouversDisabled)
		{
			this.CurrentBehavior = FishAiBehavior.Escape;
			this.GenerateRandomEscape();
		}
		else
		{
			FishTrail randomTrail = this.GetRandomTrail(this.allManeuvers);
			if (randomTrail == this.eightsTrail)
			{
				this.GoToEights();
			}
			else if (randomTrail == this.headShakeTrail)
			{
				this.GoToHeadShake();
			}
			else if (randomTrail == this.jumpOutTrail)
			{
				this.GoToJumpOut();
			}
			else if (randomTrail == this.candleTrail)
			{
				this.GoToCandle();
			}
		}
	}

	private Vector3 GetJumpOutShift()
	{
		Vector3 normalized = (this.mouth.Position - this.playerPosition).normalized;
		Vector3 vector;
		if (Random.Range(0f, 1f) < 0.5f)
		{
			vector = Quaternion.Euler(0f, 90f, 0f) * normalized;
		}
		else
		{
			vector = Quaternion.Euler(0f, -90f, 0f) * normalized;
		}
		vector.y = 0f;
		return vector;
	}

	private Vector3 TargetDirection
	{
		get
		{
			return this.TargetPosition.Value - this.mouth.Position;
		}
	}

	private void ApplyCurrentBehavior()
	{
		this.ResetFishBuoyancy();
		Vector3? targetPosition = this.TargetPosition;
		if (targetPosition != null)
		{
			Vector3 targetDirection = this.TargetDirection;
			switch (this.CurrentBehavior)
			{
			case FishAiBehavior.KeepPosition:
				this.ApplyForce(targetDirection.normalized * this.CurrentForce * Mathf.Clamp01(targetDirection.magnitude), false);
				goto IL_395;
			case FishAiBehavior.Hide:
				this.ApplyForce(targetDirection.normalized * 1f * this.CurrentForce * Mathf.Clamp01(targetDirection.magnitude), false);
				goto IL_395;
			case FishAiBehavior.Escape:
			case FishAiBehavior.Correction:
				this.ApplyForce(targetDirection.normalized * this.CurrentForce, false);
				goto IL_395;
			case FishAiBehavior.Float:
				this.ApplyForce(targetDirection.normalized * this.CurrentForce, false);
				this.MakeFishFloat();
				goto IL_395;
			case FishAiBehavior.Sink:
				this.ApplyForce(targetDirection.normalized * this.CurrentForce, false);
				this.MakeFishSink();
				goto IL_395;
			case FishAiBehavior.Candle:
				if (this.moveForward)
				{
					this.ApplyForce(targetDirection.normalized * this.FishObject.ComputeSpeedForce(global::FishAi.MaxJumpSpeed), false);
				}
				else
				{
					this.ResetForce();
				}
				goto IL_395;
			case FishAiBehavior.HeadShake:
				if (this.moveForward)
				{
					this.ApplyForce(targetDirection.normalized * this.CurrentForce, false);
				}
				else
				{
					this.ResetForce();
				}
				goto IL_395;
			case FishAiBehavior.JumpOut:
				if (this.moveForward)
				{
					this.ApplyForce(targetDirection.normalized * this.FishObject.ComputeSpeedForce(global::FishAi.MaxJumpSpeed), false);
				}
				else
				{
					this.ResetForce();
				}
				goto IL_395;
			case FishAiBehavior.InitialJerk:
				this.ApplyForce(targetDirection.normalized * 1.5f * this.Force, false);
				goto IL_395;
			case FishAiBehavior.Pool:
			{
				if (this.timeSpent > this.pathCheckTimeStamp + 1f)
				{
					float magnitude = (this.pathCheckPosition - this.mouth.Position).magnitude;
					if (magnitude < 0.5f * this.Speed * 0.2f)
					{
						this.GenerateEscapePointForPool();
					}
					this.pathCheckTimeStamp = this.timeSpent;
					this.pathCheckPosition = this.mouth.Position;
				}
				Vector3 vector = targetDirection.normalized * 0.2f * this.Force;
				if (this.IsBiteTemplateKeepIn)
				{
					vector *= this.factorForce;
					if (this.biteKeepIn.IsShaking)
					{
						if (this.shakeDirection == 0)
						{
							this.shakeDirection = 1;
						}
						else if (this.timeSpent > this.shakePeriod)
						{
							this.shakeDirection = -this.shakeDirection;
							this.shakePeriod += this.timeSpent;
						}
						vector += this.ShakeForce(vector, this.shakeDirection);
					}
					if (this.timeSpent > this.keepInPeriod && this.iKeepIn < this.amountKeepIn)
					{
						this.iKeepIn++;
						this.SetTemplateKeepIn();
						this.keepInPeriod += this.timeSpent;
						this.GenerateEscapePointForPool();
					}
				}
				this.ApplyForce(vector, false);
				goto IL_395;
			}
			}
			this.ResetForce();
			IL_395:;
		}
		else if (this.CurrentBehavior == FishAiBehavior.Eights)
		{
			Vector3 vector2 = Quaternion.Euler(0f, this.angle, 0f) * Vector3.forward;
			this.ApplyForce(vector2 * this.CurrentForce, false);
		}
	}

	private void UpdateStamina()
	{
		this.CurrentForce = this.Force * this.CurrentRelativeForce;
		if (this.CurrentBehavior == FishAiBehavior.Candle || this.CurrentBehavior == FishAiBehavior.JumpOut)
		{
			this._currentMaxSpeed = global::FishAi.MaxJumpSpeed;
		}
		else if (this.CurrentBehavior == FishAiBehavior.HeadShake)
		{
			this._currentMaxSpeed = global::FishAi.MaxHeadShakeSpeed;
		}
		else if (this.CurrentBehaviorDirection == FishBehaviorDirection.To && (this.CurrentBehavior == FishAiBehavior.Escape || this.CurrentBehavior == FishAiBehavior.Float || this.CurrentBehavior == FishAiBehavior.Sink))
		{
			this._currentMaxSpeed = 2f;
		}
		else
		{
			this._currentMaxSpeed = this.Speed * this.CurrentRelativeForce;
		}
	}

	private bool CheckIfFishIsTiredAndStopFighting(float dt, out float restPeriod)
	{
		restPeriod = this.behaviourPeriod;
		if (this.restSkipTime > 0f)
		{
			this.timeSpentAfterRest += dt;
			if (this.timeSpentAfterRest < this.restSkipTime)
			{
				return false;
			}
			this.restSkipTime = 0f;
		}
		float num = (1f - this.CurrentRelativeForce) * dt;
		float num2 = Random.Range(0f, 1f);
		float num3 = Random.Range(0f, 1f);
		if (num2 < num && num3 < this.stopFightProbability)
		{
			restPeriod = Random.Range(0.5f, 2f + 0.3f / this.CurrentRelativeForce);
			this.timeSpentAfterRest = 0f;
			this.restSkipTime = 5f + restPeriod;
			return true;
		}
		return false;
	}

	private void ApplyForce(Vector3 force, bool reverse = false)
	{
		this.IsFishSwiming = true;
		Vector3? targetPosition = this.TargetPosition;
		float num = ((targetPosition == null) ? (this.mouth.WaterHeight - 0.1f) : this.TargetPosition.Value.y);
		if (this.CurrentBehavior != FishAiBehavior.Candle && this.CurrentBehavior != FishAiBehavior.JumpOut && this.mouth.Position.y >= num)
		{
			float magnitude = force.magnitude;
			force.y = Mathf.Min(force.y, Mathf.Lerp(-magnitude, force.y, (this.mouth.Position.y - this.mouth.WaterHeight) / num));
			force = force.normalized * magnitude;
		}
		this.FishObject.SetThrust(force, reverse);
		this.UpdateDirections(force);
	}

	private void UpdateDirections(Vector3 force)
	{
		Vector3 normalized = (this.mouth.Position - this.root.Position).normalized;
		float num = Math3d.AngleSigned(normalized, force.normalized, Vector3.up);
		this.IsRight = num > 30f;
		this.IsLeft = num < -30f;
	}

	private void ResetForce()
	{
		this.IsFishSwiming = false;
		if (this.mouth == null || this.root == null || this.FishObject == null)
		{
			return;
		}
		this.FishObject.SetThrust(Vector3.zero, false);
	}

	private void GenerateRandomEscape()
	{
		this.isTargetAShelter = false;
		List<FishTrail> list = new List<FishTrail>();
		Box box = null;
		if (this.hideTrail != null && GameFactory.FishSpawner.Shelters != null && GameFactory.FishSpawner.Shelters.Count > 0)
		{
			Point3 point = this.mouth.Position.ToPoint3();
			box = GameFactory.FishSpawner.Shelters[0];
			float num = box.Distance(point);
			for (int i = 1; i < GameFactory.FishSpawner.Shelters.Count; i++)
			{
				Box box2 = GameFactory.FishSpawner.Shelters[i];
				if (box2 != box)
				{
					float num2 = box2.Distance(point);
					if (num2 < num)
					{
						box = box2;
						num = num2;
					}
				}
			}
			if (num > 4f)
			{
				box = null;
			}
		}
		list.Add(this.sinkTrail);
		if (box != null && this.hideTrail != null)
		{
			list.Add(this.hideTrail);
		}
		if (this.mouth.Position.y > -0.6f)
		{
			list.Add(this.escapeTrail);
		}
		if (this.mouth.Position.y > -1.2f)
		{
			list.Add(this.floatTrail);
		}
		FishTrail randomTrail = this.GetRandomTrail(list);
		if (randomTrail == this.hideTrail && box != null)
		{
			Vector3 shelterEscapePoint = this.GetShelterEscapePoint(box);
			if (shelterEscapePoint != Vector3.zero)
			{
				this.TargetPosition = new Vector3?(shelterEscapePoint);
				this.isTargetAShelter = true;
				return;
			}
			randomTrail = this.sinkTrail;
		}
		Vector3 vector;
		if (Random.Range(0f, 1f) < 0.2f && this.CanEscapeAwayFromPlayer(out vector))
		{
			this.CurrentBehaviorDirection = FishBehaviorDirection.Away;
		}
		else if (Random.Range(0f, 1f) < 0.05f && this.CanEscapeToPlayer(out vector))
		{
			this.CurrentBehaviorDirection = FishBehaviorDirection.To;
		}
		else
		{
			this.CurrentBehaviorDirection = FishBehaviorDirection.Away;
			Vector3 vector2 = this.mouth.Position - this.playerPosition;
			Vector3 normalized = vector2.normalized;
			Vector3 vector3 = Quaternion.Euler(0f, 90f, 0f) * normalized;
			Vector3 vector4 = Quaternion.Euler(0f, -90f, 0f) * normalized;
			Vector3 vector5 = this.mouth.Position + vector3 * vector2.magnitude;
			Vector3 vector6 = this.mouth.Position + vector4 * vector2.magnitude;
			float num3 = GameFactory.FishSpawner.CheckDepth(vector5.x, vector5.z);
			float num4 = GameFactory.FishSpawner.CheckDepth(vector6.x, vector6.z);
			if (num4 == 0f && num3 == 0f)
			{
				vector5 = this.mouth.Position + vector3 * vector2.magnitude / 4f;
				vector6 = this.mouth.Position + vector4 * vector2.magnitude / 4f;
				num3 = GameFactory.FishSpawner.CheckDepth(vector5.x, vector5.z);
				num4 = GameFactory.FishSpawner.CheckDepth(vector6.x, vector6.z);
				if (num4 == 0f && num3 == 0f)
				{
					return;
				}
			}
			if (num4 != 0f && num3 == 0f)
			{
				vector = vector6;
				this.CurrentBehaviorDirection = FishBehaviorDirection.Left;
			}
			else if (num4 == 0f && num3 != 0f)
			{
				vector = vector5;
				this.CurrentBehaviorDirection = FishBehaviorDirection.Right;
			}
			else
			{
				Vector3? targetPosition = this.TargetPosition;
				if (targetPosition == null)
				{
					if (Random.Range(0f, 1f) < 0.5f)
					{
						vector = vector5;
						this.CurrentBehaviorDirection = FishBehaviorDirection.Right;
					}
					else
					{
						vector = vector6;
						this.CurrentBehaviorDirection = FishBehaviorDirection.Left;
					}
				}
				else
				{
					float num5 = Vector3.Distance(this.TargetPosition.Value, vector5);
					float num6 = Vector3.Distance(this.TargetPosition.Value, vector6);
					if (num5 > num6)
					{
						vector = vector5;
						this.CurrentBehaviorDirection = FishBehaviorDirection.Right;
					}
					else
					{
						vector = vector6;
						this.CurrentBehaviorDirection = FishBehaviorDirection.Left;
					}
				}
			}
		}
		if (randomTrail == this.sinkTrail)
		{
			vector.y = this.mouth.Position.y - 20f;
			this.CurrentBehavior = FishAiBehavior.Sink;
		}
		else if (randomTrail == this.escapeTrail)
		{
			vector.y = this.mouth.Position.y;
		}
		else if (randomTrail == this.floatTrail)
		{
			vector.y = -0.3f;
			this.CurrentBehavior = FishAiBehavior.Float;
		}
		this.TargetPosition = new Vector3?(this.CorrectTargetPositionDepth(vector));
	}

	private Vector3 GetShelterEscapePoint(Box nearestShelter)
	{
		float num = nearestShelter.Scale.X / 2f - 0.3f;
		float num2 = nearestShelter.Scale.Y / 2f - 0.3f;
		float num3 = nearestShelter.Scale.Z / 2f - 0.3f;
		Vector3 vector = nearestShelter.Position.ToVector3();
		Quaternion quaternion = Quaternion.Euler(nearestShelter.Rotation.X, nearestShelter.Rotation.Y, nearestShelter.Rotation.Z);
		float num4 = 0.4f;
		for (int i = 1; i < 20; i++)
		{
			Vector3 vector2;
			vector2..ctor(num * Random.Range(-1f, 1f), num2 * Random.Range(-1f, 1f), num3 * Random.Range(-1f, 1f));
			Vector3 vector3 = vector + quaternion * vector2;
			float groundHight = Math3d.GetGroundHight(vector3);
			if (groundHight < -num4)
			{
				vector3.y = groundHight + 0.1f;
				return vector3;
			}
		}
		return Vector3.zero;
	}

	private bool CanEscapeAwayFromPlayer(out Vector3 point)
	{
		Vector3 vector = this.mouth.Position - this.playerPosition;
		vector.y = 0f;
		float num = Random.Range(-45f, 45f);
		float num2 = Random.Range(0.2f, 0.5f) + 5f;
		point = this.mouth.Position + Quaternion.Euler(0f, num, 0f) * vector.normalized * vector.magnitude * num2;
		return GameFactory.FishSpawner.CheckDepth(point.x, point.z) != 0f;
	}

	private bool CanEscapeToPlayer(out Vector3 point)
	{
		point = Vector3.zero;
		Vector3 vector = this.mouth.Position - this.playerPosition;
		vector.y = 0f;
		if (vector.magnitude < 3f)
		{
			return false;
		}
		float num = Random.Range(-45f, 45f);
		float num2 = Random.Range(0.2f, 0.5f);
		point = this.mouth.Position - Quaternion.Euler(0f, num, 0f) * vector.normalized * vector.magnitude * num2;
		return GameFactory.FishSpawner.CheckDepth(point.x, point.z) != 0f;
	}

	private void GenerateEscapePointForPool()
	{
		Vector3 vector = this.mouth.Position - this.playerPosition;
		vector.y = 0f;
		Vector3 normalized = vector.normalized;
		float num = Random.Range(0f, 1f);
		Vector3 vector2;
		if (num < 0.33f)
		{
			vector2 = normalized * 3f;
		}
		else if (num < 0.66f)
		{
			vector2 = Quaternion.Euler(0f, 90f, 0f) * normalized * 3f;
		}
		else
		{
			vector2 = Quaternion.Euler(0f, -90f, 0f) * normalized * 3f;
		}
		float num2 = this.mouth.Position.y - this.mouth.GroundHeight;
		float num3 = 0f;
		float num4 = this.mouth.Position.y;
		if (this.IsBiteTemplate && this.biteDirection != FishBiteDirection.None)
		{
			if (this.biteDirection == FishBiteDirection.Up)
			{
				num4 = -0.3f;
			}
			else
			{
				if (this.biteDirection == FishBiteDirection.AskewUp)
				{
					num4 = (num4 - 0.3f) * 0.5f;
				}
				else if (this.biteDirection == FishBiteDirection.Aside)
				{
					num4 = this.mouth.GroundHeight * 0.25f;
				}
				else if (this.biteDirection == FishBiteDirection.AskewDown)
				{
					num4 = this.mouth.GroundHeight * 0.5f;
				}
				else if (this.biteDirection == FishBiteDirection.Down)
				{
					num4 = this.mouth.GroundHeight * 0.75f;
				}
				if (num4 > -0.3f)
				{
					num4 = -0.3f;
				}
			}
		}
		else
		{
			num = Random.Range(0f, 1f);
			if (this.mouth.Position.y > -0.3f)
			{
				num3 = -2f;
				num4 = this.mouth.GroundHeight * 0.5f;
			}
			else if (num2 <= 0.2f)
			{
				if (num < 0.3f)
				{
					num3 = 0f;
				}
				else
				{
					num4 = this.mouth.GroundHeight * 0.5f;
				}
			}
			else if (num < 0.4f)
			{
				num4 = this.mouth.GroundHeight * 0.75f;
			}
			else if (num < 0.8f)
			{
				num3 = 0f;
			}
			else
			{
				num4 = this.mouth.GroundHeight * 0.25f;
			}
		}
		vector2.y += num3;
		Vector3 vector3 = this.mouth.Position + vector2;
		vector3.y = num4;
		this.TargetPosition = new Vector3?(vector3);
	}

	private void GenerateEscapePointForFinalEscape()
	{
		Vector3 vector = this.mouth.Position - this.playerPosition;
		vector.y = 0f;
		Vector3 normalized = vector.normalized;
		Vector3 vector2 = this.playerPosition + normalized * 10f;
		Vector3 vector3 = this.playerPosition + Quaternion.Euler(0f, 90f, 0f) * normalized * 10f;
		Vector3 vector4 = this.playerPosition + Quaternion.Euler(0f, -90f, 0f) * normalized * 10f;
		float num = GameFactory.FishSpawner.CheckDepth(vector2.x, vector2.z);
		float num2 = GameFactory.FishSpawner.CheckDepth(vector3.x, vector3.z);
		float num3 = GameFactory.FishSpawner.CheckDepth(vector4.x, vector4.z);
		if (num >= num2 && num >= num3)
		{
			this.TargetPosition = new Vector3?(new Vector3(vector2.x, -num, vector2.z));
		}
		else if (num2 >= num && num2 >= num3)
		{
			this.TargetPosition = new Vector3?(new Vector3(vector3.x, -num2, vector3.z));
		}
		else
		{
			this.TargetPosition = new Vector3?(new Vector3(vector4.x, -num3, vector4.z));
		}
	}

	private bool IsCorrectionNeeded(out Vector3 direction)
	{
		direction = Vector3.zero;
		float num = Mathf.Abs(this.playerPosition.y - this.mouth.Position.y);
		float magnitude = (new Vector2(this.playerPosition.x, this.playerPosition.z) - new Vector2(this.mouth.Position.x, this.mouth.Position.z)).magnitude;
		if (num > magnitude)
		{
			return false;
		}
		if (this.HasObstaclesToPlayerLookingFromWaterSurface(this.mouth.Position))
		{
			return false;
		}
		Vector3 normalized = (this.mouth.Position - this.playerPosition).normalized;
		Vector3 vector = Quaternion.Euler(0f, 90f, 0f) * normalized;
		Vector3 vector2 = Quaternion.Euler(0f, -90f, 0f) * normalized;
		Vector3 vector3 = this.mouth.Position + vector;
		Vector3 vector4 = this.mouth.Position + vector2;
		bool flag = this.HasObstaclesToPlayerLookingFromWaterSurface(vector3);
		bool flag2 = this.HasObstaclesToPlayerLookingFromWaterSurface(vector4);
		if (flag == flag2)
		{
			return false;
		}
		if (flag)
		{
			direction = vector2;
		}
		if (flag2)
		{
			direction = vector;
		}
		return true;
	}

	private bool HasObstaclesToPlayerLookingFromWaterSurface(Vector3 point)
	{
		Vector3 vector;
		vector..ctor(point.x, 0f, point.z);
		Vector3 vector2;
		vector2..ctor(this.playerPosition.x, 1f, this.playerPosition.z);
		Vector3 vector3 = vector2 - vector;
		RaycastHit raycastHit;
		return Physics.Raycast(vector, vector3, ref raycastHit, float.PositiveInfinity, GlobalConsts.FishMask) && raycastHit.distance < vector3.magnitude - 1f;
	}

	private FishTrail GetRandomTrail(IList<FishTrail> trails)
	{
		float num = 0f;
		for (int i = 0; i < trails.Count; i++)
		{
			FishTrail fishTrail = trails[i];
			if (fishTrail != null)
			{
				num += fishTrail.Probability;
			}
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (FishTrail fishTrail2 in trails)
		{
			if (fishTrail2 != null)
			{
				num3 += fishTrail2.Probability;
				if (num2 <= num3)
				{
					return fishTrail2;
				}
			}
		}
		return null;
	}

	private void CorrectFishEscapeDirection(Vector3 direction)
	{
		Vector3 vector = this.mouth.Position - this.playerPosition;
		Vector3 vector2 = this.mouth.Position + direction * vector.magnitude;
		vector2.y = this.mouth.Position.y - 2f;
		this.TargetPosition = new Vector3?(this.CorrectTargetPositionDepth(vector2));
		this.correctionAttempt++;
		this.behaviourPeriod = 2f * (float)this.correctionAttempt;
		this.timeSpent = 0f;
		this.CurrentBehavior = FishAiBehavior.Correction;
	}

	private Vector3 CorrectTargetPositionDepth(Vector3 position)
	{
		float num = GameFactory.FishSpawner.CheckDepth(position.x, position.z);
		if (position.y < -num)
		{
			position.y = -num + 0.1f;
		}
		if (position.y > -0.3f)
		{
			position.y = -0.3f;
		}
		return position;
	}

	private void ResetFishBuoyancy()
	{
		this.mouth.Buoyancy = 0f;
		this.root.Buoyancy = 0f;
	}

	private void MakeFishSink()
	{
		this.mouth.Buoyancy = -0.5f;
		this.root.Buoyancy = -0.5f;
	}

	private void MakeFishFloat()
	{
		this.mouth.Buoyancy = 0.5f;
		this.root.Buoyancy = 0.5f;
	}

	private void PlayScript()
	{
		bool flag = false;
		if ((this.IsManeuver && this.CheckManeuverFinished()) || (!this.IsManeuver && this.IsBehaviorExpired) || this.actionIndex == -1)
		{
			this.actionIndex++;
			flag = true;
			if (this.actionIndex >= this._portrait.Trails.Length)
			{
				this.actionIndex = 0;
			}
		}
		if (!flag)
		{
			return;
		}
		FishTrail fishTrail = this._portrait.Trails[this.actionIndex];
		this.CurrentBehavior = fishTrail.Behavior;
		this.isTargetAShelter = false;
		this.timeSpent = 0f;
		switch (this.CurrentBehavior)
		{
		case FishAiBehavior.Eights:
			this.GoToEights();
			break;
		case FishAiBehavior.Escape:
		case FishAiBehavior.Float:
		case FishAiBehavior.Sink:
			this.GenerateEscapePointForPlay(fishTrail.Direction);
			break;
		case FishAiBehavior.Candle:
			this.GoToCandle();
			break;
		case FishAiBehavior.HeadShake:
			this.GoToHeadShake();
			break;
		case FishAiBehavior.JumpOut:
			this.GoToJumpOut();
			break;
		case FishAiBehavior.SurfaceShock:
			this.GoToSurfaceShock();
			break;
		case FishAiBehavior.InitialJerk:
			this.GoToInitialJerk();
			break;
		case FishAiBehavior.Shocked:
			this.GoToShocked();
			break;
		case FishAiBehavior.StopFight:
			this.GoToStopFight();
			break;
		}
		if (fishTrail.Duration != null)
		{
			this.behaviourPeriod = fishTrail.Duration.Value;
		}
	}

	private void GenerateEscapePointForPlay(FishBehaviorDirection? direction)
	{
		Vector3 vector = this.mouth.Position - this.playerPosition;
		vector.y = 0f;
		Vector3 normalized = vector.normalized;
		if (direction == null)
		{
			direction = new FishBehaviorDirection?(FishBehaviorDirection.Away);
		}
		Vector3 vector2 = Vector3.zero;
		if (direction != null)
		{
			switch (direction.Value)
			{
			case FishBehaviorDirection.Away:
				vector2 = this.playerPosition + normalized * 10f;
				break;
			case FishBehaviorDirection.To:
				vector2 = this.playerPosition - normalized * 10f;
				break;
			case FishBehaviorDirection.Left:
				vector2 = this.playerPosition + Quaternion.Euler(0f, -90f, 0f) * normalized * 10f;
				break;
			case FishBehaviorDirection.Right:
				vector2 = this.playerPosition + Quaternion.Euler(0f, 90f, 0f) * normalized * 10f;
				break;
			}
		}
		this.CurrentBehaviorDirection = direction.Value;
		if (this.CurrentBehavior == FishAiBehavior.Escape)
		{
			vector2.y = this.mouth.Position.y;
		}
		else if (this.CurrentBehavior == FishAiBehavior.Float)
		{
			vector2.y = -0.1f;
		}
		else if (this.CurrentBehavior == FishAiBehavior.Sink)
		{
			vector2.y = 0.1f - GameFactory.FishSpawner.CheckDepth(vector2.x, vector2.z);
		}
		this.TargetPosition = new Vector3?(vector2);
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2} {3}", new object[]
		{
			(!this._portrait.IsScript) ? string.Empty : "Play",
			Enum.GetName(typeof(FishAiBehavior), this.CurrentBehavior),
			(!this.moveForward) ? "B" : "F",
			(!this.isTargetAShelter) ? string.Empty : "(to shelter)"
		});
	}

	private FishBiteTemplate _biteTemplate
	{
		get
		{
			return this._portrait.ResultFishBiteTemplate;
		}
	}

	private FishBitePart[] _biteParts
	{
		get
		{
			return (this._portrait.ResultFishBiteTemplate == null) ? null : this._portrait.ResultFishBiteTemplate.ResultParts;
		}
	}

	private FishBiteKeepIn[] _biteKeepIns
	{
		get
		{
			return (this._portrait.ResultFishBiteTemplate == null) ? null : this._portrait.ResultFishBiteTemplate.ResultKeepIn;
		}
	}

	public FishBiteTemplate BiteTemplate
	{
		get
		{
			return this._biteTemplate;
		}
	}

	public bool IsBiteTemplate
	{
		get
		{
			return this._biteTemplate != null;
		}
	}

	public bool IsBiteTemplateKeepIn
	{
		get
		{
			return this._biteTemplate != null && this.amountKeepIn > 0;
		}
	}

	public float MaxBiteTime
	{
		get
		{
			return this.maxBiteTime;
		}
	}

	public bool IsBiting
	{
		get
		{
			return this._biteTemplate != null && this.countTastes > 0 && !this.endOfBite;
		}
	}

	public bool EndOfBite
	{
		get
		{
			return this._biteTemplate != null && this.endOfBite;
		}
	}

	private Vector3 ShakeForce(Vector3 force, int direction)
	{
		Vector3 zero = Vector3.zero;
		if (direction == 1)
		{
			zero.x = force.z;
			zero.z = -force.x;
		}
		else
		{
			zero.x = -force.z;
			zero.z = force.x;
		}
		return zero.normalized * force.magnitude * this.shakeFactorForce;
	}

	private void ApplyBiteTemplate()
	{
		if (this._biteTemplate != null)
		{
			if (this._biteParts != null)
			{
				this.amountParts = this._biteParts.Length;
				if (this.amountParts > 0)
				{
					this.iPart = 0;
					this.endOfBite = false;
					this.maxBiteTime = 1f;
					foreach (FishBitePart fishBitePart in this._biteParts)
					{
						this.maxBiteTime += fishBitePart.MaxBiteTime;
					}
					this.amountKeepIn = 0;
					if (this._biteKeepIns != null)
					{
						this.amountKeepIn = this._biteKeepIns.Length;
					}
					this.iKeepIn = 0;
					this.SetTemplatePart();
					this.SetTemplateKeepIn();
				}
			}
			else
			{
				LogHelper.Error("Bite template has no Parts section!", new object[0]);
			}
		}
	}

	private void SetTemplatePart()
	{
		if (this._biteParts != null && this.iPart < this.amountParts)
		{
			this.bitePart = this._biteParts[this.iPart];
			this.countTastes = 0;
			this.amountTastes = this.bitePart.Amount;
			this.factorForce = 1f;
		}
	}

	private void CalculateTaste()
	{
		if (this._biteParts != null && !this.endOfBite)
		{
			if (this.countTastes >= this.amountTastes && this.iPart < this.amountParts)
			{
				this.iPart++;
				this.endOfBite = this.iPart >= this.amountParts;
				if (this.endOfBite)
				{
					return;
				}
				this.SetTemplatePart();
			}
			this.tastePeriod = this.bitePart.Elements[this.countTastes].Period;
			this.factorForce = this.bitePart.Elements[this.countTastes].FactorForce;
			this.biteDirection = this.bitePart.Elements[this.countTastes].Direction;
			this.countTastes++;
			if (this.tackleTipMass.IsLying && (this.biteDirection == FishBiteDirection.Down || this.biteDirection == FishBiteDirection.AskewDown))
			{
				this.biteDirection = FishBiteDirection.Aside;
			}
			this.retreatDirection = Vector3.up;
			if (this.biteDirection == FishBiteDirection.Down)
			{
				this.retreatDirection = Vector3.down;
			}
			else if (this.biteDirection == FishBiteDirection.Aside || this.biteDirection == FishBiteDirection.AskewDown || this.biteDirection == FishBiteDirection.AskewUp)
			{
				Vector3 vector = this.mouth.Position - this.tackleTipMass.Position;
				vector.y = 0f;
				float num = vector.magnitude;
				if (Mathf.Approximately(num, 0f))
				{
					vector = Vector3.right;
					num = 1f;
				}
				if (this.biteDirection == FishBiteDirection.AskewDown)
				{
					vector.y = -num;
				}
				else if (this.biteDirection == FishBiteDirection.AskewUp)
				{
					vector.y = num;
				}
				this.retreatDirection = vector.normalized;
			}
			this.behaviourPeriod = this.tastePeriod * 0.5f;
		}
	}

	private void SetTemplateKeepIn()
	{
		if (this._biteKeepIns != null && this.iKeepIn < this.amountKeepIn)
		{
			this.biteKeepIn = this._biteKeepIns[this.iKeepIn];
			this.keepInPeriod = this.biteKeepIn.Period;
			this.factorForce = this.biteKeepIn.FactorForce;
			if (this.tackleTipMass.IsLying)
			{
				this.biteDirection = this.biteKeepIn.BottomDirection;
			}
			else
			{
				this.biteDirection = this.biteKeepIn.Direction;
			}
			if (this.biteKeepIn.IsShaking)
			{
				this.shakePeriod = this.biteKeepIn.ShakePeriod;
				this.shakeFactorForce = this.biteKeepIn.ShakeFactorForce;
				this.shakeDirection = 0;
			}
		}
	}

	private const float PREDATOR_DIST_TO_SLOWDOWN = 1f;

	private const float PREDATOR_DIST_TO_BAIT = 0.1f;

	private const float PREDATOR_FORCE_K = 1f;

	private const float PREDATOR_NEAR_FORCE_K = 0.1f;

	private const float FactorForcePool = 0.2f;

	private const float FactorForceInitialJerk = 1.5f;

	private const float MaxShelterDistance = 4f;

	public const float MinSwimDepth = 0.3f;

	private const float PoolDistance = 3f;

	private const float FinalEscapeDistance = 10f;

	private const float MaxJumpHeight = 1f;

	private static readonly float MaxJumpSpeed = Mathf.Sqrt(19.62f);

	private const float MaxHeadShakeHeight = 0.3f;

	private static readonly float MaxHeadShakeSpeed = Mathf.Sqrt(5.8860006f);

	private const float MaxSpeed2Player = 2f;

	private const float PoolFromShelterDistance = 1f;

	private const float BiteForce = 0.1f;

	public const float TasteDistance = 0.03f;

	public const float BobberMagnetWaterDistrubanceRadius = 0.1f;

	public const float HookTargetDistance = 0.25f;

	public const float AttackTargetVerticalOffset = 0.3f;

	public const float AttackThrustForceFilter = 0.25f;

	private const float HideForceModifier = 1f;

	private const float MinForceRationToChangeBehavior = 0.2f;

	private const float ChangeBehaviorProbabilityModifier = 0.33f;

	private const float MaxManeuverTimeout = 3f;

	private const float MinFishVelocityWhenStucked = 0.2f;

	private const float MinFishDistanceToSurfaceWhenStucked = 0.1f;

	private const float MinFishFightAfterRest = 5f;

	private const float MinBehaviourPeriod = 0.3f;

	private const float MaxBehaviourPeriod = 1f;

	private const float MinTastePeriod = 0.5f;

	private const float MaxTastePeriod = 2f;

	private readonly FishPortrait _portrait;

	private readonly IFishAnimationController animationController;

	public float Activity;

	public float Stamina;

	public float Force;

	public float Speed;

	public float Mass;

	public float RetreatThreshold;

	public float AttackLure;

	private FishAiBehavior _currentBehavior;

	public bool IsManeuver;

	public bool IsRight;

	public bool IsLeft;

	private Vector3 playerPosition;

	public Vector3? TargetPosition;

	private bool isTargetAShelter;

	private float? priorDistanceToTarget;

	public float CurrentForce;

	private float minForce;

	private float behaviourPeriod;

	private float timeSpent;

	private bool surfaceShocked;

	private bool isLineTensioned;

	private bool areManeouversDisabled;

	private float pathCheckTimeStamp;

	private Vector3 pathCheckPosition;

	private float _currentMaxSpeed;

	private Mass mouth;

	private Mass root;

	private FishTrail initialJerkTrail;

	private FishTrail keepPositionTrail;

	private FishTrail hideTrail;

	private FishTrail escapeTrail;

	private FishTrail floatTrail;

	private FishTrail sinkTrail;

	private FishTrail shockedTrail;

	private FishTrail surfaceShockTrail;

	private FishTrail candleTrail;

	private FishTrail eightsTrail;

	private FishTrail headShakeTrail;

	private FishTrail jumpOutTrail;

	private float stopFightProbability;

	private float allEscapesProbability;

	private float allManeuversProbability;

	private FishTrail[] allManeuvers;

	private float attackDelayTimeout;

	private float attackDelay;

	private Mass tackleTipMass;

	private bool isLureTackle;

	private Magnet mouthMagnet;

	private float retreatDistance;

	private bool isPaused;

	private float tastePeriod;

	private bool striked;

	private bool outOfWater;

	private int jumpNumber;

	private int jumps;

	private bool moveForward;

	private Vector3 jumpOutShift;

	private float angle;

	private float initialAngle;

	private float angularSpeed;

	private bool angleWasLess;

	private float maxRotations;

	private float rotations;

	private float timeSpentInManeuver;

	private float maxTimeForManeuver;

	private float? priorMouthPosition;

	private float timeSpentAfterRest;

	private float restSkipTime;

	private const float MaxDistanceToBottom = 0.1f;

	private const float BoxPenetrationDepth = 0.3f;

	private int correctionAttempt;

	private int actionIndex = -1;

	private FishBitePart bitePart;

	private FishBiteKeepIn biteKeepIn;

	private int amountParts;

	private int iPart;

	private int amountTastes;

	private int countTastes;

	private float factorForce = 1f;

	private FishBiteDirection biteDirection;

	private int amountKeepIn;

	private int iKeepIn;

	private float keepInPeriod;

	private float shakePeriod;

	private float shakeFactorForce;

	private int shakeDirection;

	private Vector3 retreatDirection;

	private float maxBiteTime;

	private bool endOfBite;
}
