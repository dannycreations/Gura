using System;
using Cayman;
using ObjectModel;
using Phy;
using UnityEngine;

namespace Boats
{
	public class KayakController : BoatController<KayakController.KayakPlayerStates, KayakController.KayakSignals, KayakMouseController, Kayak>
	{
		protected KayakController(Transform spawn, BoatSettings settings, Kayak serverSettings, bool isOwner, ushort factoryID)
			: base(spawn, settings, serverSettings, isOwner, factoryID)
		{
			this.ParseServerSettings();
			this.InitializeSimulation();
			this._stamina = new StaminaController(this._serverSettings.StaminaSettings, this._settings.paddle.FastSpeedK);
			this.PutOar();
		}

		protected override float Vertical
		{
			get
			{
				return ControlsController.ControlsActions.Move.Y;
			}
		}

		protected Transform LPaddle
		{
			get
			{
				return this._settings.paddle.LeftPaddlePushPoint;
			}
		}

		protected Transform RPaddle
		{
			get
			{
				return this._settings.paddle.RightPaddlePushPoint;
			}
		}

		protected Transform Oar
		{
			get
			{
				return this._settings.paddle.transform;
			}
		}

		public override float Stamina
		{
			get
			{
				return this._stamina.Value;
			}
		}

		public override bool IsTrollingPossible
		{
			get
			{
				return true;
			}
		}

		public override bool IsRowing
		{
			get
			{
				return this._leftForceWeight != 0f || this._rightForceWeight != 0f || this._forwarForceWeight != 0f;
			}
		}

		public static KayakController Create(Transform spawn, BoatSettings settings, Kayak serverSettings, bool isOwner, ushort factoryID)
		{
			KayakController kayakController = new KayakController(spawn, settings, serverSettings, isOwner, factoryID);
			kayakController.InitFSM();
			kayakController.InitPhysics();
			return kayakController;
		}

		private void InitializeSimulation()
		{
			this.paddleTipLeft = new PointOnRigidBody(this.boatSim, base.Phyboat, base.Transform.InverseTransformPoint(this.LPaddle.position), Mass.MassType.Unknown)
			{
				MassValue = this._settings.paddle.OarTipMass,
				IgnoreEnvForces = true
			};
			this.paddleTipRight = new PointOnRigidBody(this.boatSim, base.Phyboat, base.Transform.InverseTransformPoint(this.RPaddle.position), Mass.MassType.Unknown)
			{
				MassValue = this._settings.paddle.OarTipMass,
				IgnoreEnvForces = true
			};
			this.boatSim.Masses.Add(this.paddleTipLeft);
			this.boatSim.Masses.Add(this.paddleTipRight);
			this.paddleTransmissionLeft = new PointToRigidBodyResistanceTransmission(this.paddleTipLeft)
			{
				Area = this._settings.paddle.OarArea,
				Radius = this._settings.paddle.OarLength * 0.5f,
				BodyResistance = this._settings.LongitudalResistance,
				SinkDutyRatio = 0.4f
			};
			this.paddleTransmissionRight = new PointToRigidBodyResistanceTransmission(this.paddleTipRight)
			{
				Area = this._settings.paddle.OarArea,
				Radius = this._settings.paddle.OarLength * 0.5f,
				BodyResistance = this._settings.LongitudalResistance,
				SinkDutyRatio = 0.4f
			};
			this.boatSim.Connections.Add(this.paddleTransmissionLeft);
			this.boatSim.Connections.Add(this.paddleTransmissionRight);
		}

		protected override void ResetSimulation()
		{
			base.ResetSimulation();
			this.InitializeSimulation();
			base.InitPhysics();
		}

		private void ParseServerSettings()
		{
			if (this._settings.paddle != null)
			{
				this._settings.paddle.OarArea *= this._serverSettings.OarAreaMultiplier;
			}
			this._settings.FakeMovementF = this._serverSettings.FakeMovementForce;
			this._settings.FakeTurnsF = this._serverSettings.FakeTurnForce;
		}

		protected override bool IsCanLeaveBoat
		{
			get
			{
				return (this._playerFsm.CurStateID == KayakController.KayakPlayerStates.Idle || (this._playerFsm.CurStateID == KayakController.KayakPlayerStates.Fishing && this._driver.CanLeaveBoat)) && base.IsCanLeaveBoat;
			}
		}

		public override bool IsActiveMovement
		{
			get
			{
				return this._playerFsm.CurStateID == KayakController.KayakPlayerStates.MoveIn || this._playerFsm.CurStateID == KayakController.KayakPlayerStates.Forward || this._playerFsm.CurStateID == KayakController.KayakPlayerStates.Backward;
			}
		}

		protected override KayakController.KayakSignals BoardingSignal
		{
			get
			{
				return KayakController.KayakSignals.Boarded;
			}
		}

		protected override KayakController.KayakSignals UnboardingSignal
		{
			get
			{
				return KayakController.KayakSignals.Unboarding;
			}
		}

		protected override KayakController.KayakSignals HiddenUnboardingSignal
		{
			get
			{
				return KayakController.KayakSignals.HiddenUnboarding;
			}
		}

		protected override KayakController.KayakSignals CloseMapSignal
		{
			get
			{
				return KayakController.KayakSignals.CloseMap;
			}
		}

		protected override KayakController.KayakSignals RollSignal
		{
			get
			{
				return KayakController.KayakSignals.Roll;
			}
		}

		protected override KayakController.KayakSignals RollFisherSignal
		{
			get
			{
				return KayakController.KayakSignals.RollRecovering;
			}
		}

		protected override bool IsEnterFishingPromtActive
		{
			get
			{
				return this._enterFishingPromptTill > Time.time;
			}
		}

		protected override bool IsBackToNavigationPromptActive
		{
			get
			{
				return this._enterDrivingPromptTill > Time.time;
			}
		}

		protected override bool IsTakeAnchorPromptActive
		{
			get
			{
				return this._takeAnchorPromptTill > Time.time;
			}
		}

		public override void InitFSM()
		{
			this._playerFsm = new TFSM<KayakController.KayakPlayerStates, KayakController.KayakSignals>("Kayak", KayakController.KayakPlayerStates.None);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.None, new StateSwitchingDelegate(this.ReleaseControll), null, null, 0f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.Idle, delegate
			{
				this._driver.ShowSleeves = true;
				base.SetServerBoatNavigation(true);
				this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, 0f);
				this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.RightWeight, 0f);
				this._driver.PlayAnimation("KayakOarIdle", 1f, 1f, 0.5f);
				this._enterFishingPromptFrom = Time.time + 5f;
				this._enterFishingPromptTill = -1f;
				this._couldTakeRodAt = Time.time + 1f;
				this._idleFrom = Time.time + 1f;
				this._takeRodRequest = false;
			}, delegate
			{
				this._driver.transform.localRotation = Quaternion.identity;
				this._enterFishingPromptFrom = -1f;
				this._enterFishingPromptTill = -1f;
			}, delegate
			{
				if (base.SwitchingRodHandler())
				{
					this._playerFsm.SendSignal(KayakController.KayakSignals.Standup);
				}
			}, 0.5f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.MoveIn, delegate
			{
				if (CaymanGenerator.Instance != null)
				{
					CaymanGenerator.Instance.OnScareAction();
				}
				string text = "KayakOarRowIn";
				this._lastSideForce = 0f;
				if (base.Horizontal > 0f)
				{
					text = "KayakOarRowRightIn";
					this._lastSideForce = 1f;
				}
				else if (base.Horizontal < 0f)
				{
					text = "KayakOarRowLeftIn";
					this._lastSideForce = -1f;
				}
				base.PlayTransitAnimation(text, (this.Vertical >= 0f) ? KayakController.KayakSignals.ReadyToMoveForward : KayakController.KayakSignals.ReadyToMoveBackward, 1f, 1);
			}, null, null, 0f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.Forward, delegate
			{
				if (this._rowingReverse)
				{
					this._rowingReverse = false;
					this._driver.ReverseAnimation("KayakOarRow");
					this._driver.ReverseAnimation("KayakOarRowLeft");
					this._driver.ReverseAnimation("KayakOarRowRight");
				}
				else
				{
					this._driver.PlayAnimationBlended("KayakOarRow", 1f, 0f, 0.5f);
					this._driver.PlayAnimationBlended("KayakOarRowLeft", 1f, 0f, 0.5f);
					this._driver.PlayAnimationBlended("KayakOarRowRight", 1f, 0f, 0.5f);
				}
			}, null, null, 0f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.Backward, delegate
			{
				if (this._rowingReverse)
				{
					this._rowingReverse = false;
					this._driver.ReverseAnimation("KayakOarRow");
					this._driver.ReverseAnimation("KayakOarRowLeft");
					this._driver.ReverseAnimation("KayakOarRowRight");
				}
				else
				{
					this._driver.PlayAnimationBlended("KayakOarRow", -1f, 0f, 0.5f);
					this._driver.PlayAnimationBlended("KayakOarRowLeft", -1f, 0f, 0.5f);
					this._driver.PlayAnimationBlended("KayakOarRowRight", -1f, 0f, 0.5f);
				}
			}, null, null, 0f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.Unboarding, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this._driver.PlayAnimation("KayakOarDraw", 1f, 1f, 0.5f);
				this._mouseController.PrepareDriverForUnboarding();
			}, null, null, 0f, KayakController.KayakSignals.Unboarded, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.PrepareFishing, delegate
			{
				this._mouseController.EnterFishingMode();
				this.PutOar();
				base.IsFishing = true;
				this._driver.PlayAnimation("empty", 1f, 1f, 0.5f);
			}, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, true);
				this._driver.PrepareFishingFromBoat();
			}, null, 0f, KayakController.KayakSignals.ReadyToFish, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.Fishing, delegate
			{
				this._enterDrivingPromptFrom = Time.time + 5f;
				this._acceptRollBackFrom = Time.time + 3f;
				base.SetServerBoatNavigation(false);
				base.CheckForRequestedRod();
			}, delegate
			{
				this._enterDrivingPromptTill = -1f;
			}, null, 0f, KayakController.KayakSignals.None, null, delegate
			{
				base.IsFishing = false;
				this._mouseController.ClearFishingMode();
			});
			this._playerFsm.AddState(KayakController.KayakPlayerStates.EndFishing, delegate
			{
				this._driver.InitFishingToDriving(delegate
				{
					this._mouseController.LeaveFishingMode(delegate
					{
						this._playerFsm.SendSignal(KayakController.KayakSignals.ReadyToDrive);
					});
				});
			}, delegate
			{
				base.IsFishing = false;
				this.TakeOar();
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
				this._driver.FinishFishingFromBoat();
			}, null, 0f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.UnboardFisher, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
			}, delegate
			{
				base.IsFishing = false;
				this._mouseController.ClearFishingMode();
			}, null, 0f, KayakController.KayakSignals.Unboarded, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.StartRecovery, delegate
			{
				base.SetServerBoatNavigation(false);
				BlackScreenHandler.Show(true, null);
				GameActionAdapter.Instance.TurnOver();
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, KayakController.KayakSignals.RollRecovering, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.StartRecoveryFishing, delegate
			{
				BlackScreenHandler.Show(true, null);
				if (this._acceptRollBackFrom > 0f && this._acceptRollBackFrom < Time.time)
				{
					GameActionAdapter.Instance.TurnOver();
				}
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, KayakController.KayakSignals.RollRecovering, null, null);
			this._playerFsm.AddState(KayakController.KayakPlayerStates.InGameMap, delegate
			{
				base.SetServerBoatNavigation(false);
				this._mouseController.EnterMapMode();
				this.Oar.gameObject.SetActive(false);
				this._driver.PrepareOpenMap();
			}, delegate
			{
				this.Oar.gameObject.SetActive(true);
				this._mouseController.LeaveMapMode();
			}, null, 0f, KayakController.KayakSignals.None, null, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.None, KayakController.KayakPlayerStates.Idle, KayakController.KayakSignals.Boarded, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Idle, KayakController.KayakPlayerStates.MoveIn, KayakController.KayakSignals.Force, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Idle, KayakController.KayakPlayerStates.MoveIn, KayakController.KayakSignals.NegForce, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.MoveIn, KayakController.KayakPlayerStates.Forward, KayakController.KayakSignals.ReadyToMoveForward, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.MoveIn, KayakController.KayakPlayerStates.Backward, KayakController.KayakSignals.ReadyToMoveBackward, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Forward, KayakController.KayakPlayerStates.Idle, KayakController.KayakSignals.NoForce, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Forward, KayakController.KayakPlayerStates.Backward, KayakController.KayakSignals.NegForce, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Backward, KayakController.KayakPlayerStates.Forward, KayakController.KayakSignals.Force, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Backward, KayakController.KayakPlayerStates.Idle, KayakController.KayakSignals.NoForce, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Idle, KayakController.KayakPlayerStates.Unboarding, KayakController.KayakSignals.Unboarding, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Fishing, KayakController.KayakPlayerStates.UnboardFisher, KayakController.KayakSignals.Unboarding, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.UnboardFisher, KayakController.KayakPlayerStates.None, KayakController.KayakSignals.Unboarded, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Unboarding, KayakController.KayakPlayerStates.None, KayakController.KayakSignals.Unboarded, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Idle, KayakController.KayakPlayerStates.PrepareFishing, KayakController.KayakSignals.Standup, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.PrepareFishing, KayakController.KayakPlayerStates.Fishing, KayakController.KayakSignals.ReadyToFish, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Fishing, KayakController.KayakPlayerStates.EndFishing, KayakController.KayakSignals.Sitdown, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.EndFishing, KayakController.KayakPlayerStates.Idle, KayakController.KayakSignals.ReadyToDrive, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Idle, KayakController.KayakPlayerStates.StartRecovery, KayakController.KayakSignals.Roll, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Forward, KayakController.KayakPlayerStates.StartRecovery, KayakController.KayakSignals.Roll, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Backward, KayakController.KayakPlayerStates.StartRecovery, KayakController.KayakSignals.Roll, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.StartRecovery, KayakController.KayakPlayerStates.Idle, KayakController.KayakSignals.RollRecovering, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Fishing, KayakController.KayakPlayerStates.StartRecoveryFishing, KayakController.KayakSignals.Roll, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.StartRecoveryFishing, KayakController.KayakPlayerStates.Fishing, KayakController.KayakSignals.RollRecovering, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.Idle, KayakController.KayakPlayerStates.InGameMap, KayakController.KayakSignals.UseMap, null);
			this._playerFsm.AddTransition(KayakController.KayakPlayerStates.InGameMap, KayakController.KayakPlayerStates.Idle, KayakController.KayakSignals.CloseMap, null);
			this._playerFsm.AddAnyStateTransition(KayakController.KayakPlayerStates.None, KayakController.KayakSignals.HiddenUnboarding, true);
		}

		protected override void ToDrivingPrompt()
		{
			ShowHudElements.Instance.ShowTakeOarPrompt();
		}

		protected override AnimationState PlayAnimation(string name, float animSpeed = 1f, int animWeight = 1)
		{
			return this._driver.PlayAnimation(name, animSpeed, (float)animWeight, 0f);
		}

		public override void OnTimeChanged()
		{
			this._stamina.Restore();
		}

		protected override void Board()
		{
			base.Board();
			base.PlayTransitAnimation("KayakOarDraw", KayakController.KayakSignals.Boarded, 1f, 1);
			this.TakeOar();
			this._fakeLegs.SetActive(true);
			this.resetSpline = true;
		}

		protected override void ReleaseControll()
		{
			base.SetServerBoatNavigation(false);
			if (this._driver != null)
			{
				this._fakeLegs.SetActive(false);
				this.PutOar();
				base.ReleaseControll();
			}
		}

		protected override void UpdateInput()
		{
			base.UpdateInput();
			this._forwarForceWeight = 0f;
			this._leftForceWeight = 0f;
			this._rightForceWeight = 0f;
			bool flag = Mathf.Approximately(this.Vertical, 0f);
			bool flag2 = Mathf.Approximately(base.Horizontal, 0f);
			KayakController.KayakPlayerStates curStateID = this._playerFsm.CurStateID;
			if (curStateID != KayakController.KayakPlayerStates.Idle)
			{
				if (curStateID == KayakController.KayakPlayerStates.Forward || curStateID == KayakController.KayakPlayerStates.Backward)
				{
					if (flag && flag2)
					{
						this._driver.SetAnimationTargetWeight("KayakOarRow", 0f, 0.5f, true, 1f);
						this._driver.SetAnimationTargetWeight("KayakOarRowLeft", 0f, 0.5f, true, 1f);
						this._driver.SetAnimationTargetWeight("KayakOarRowRight", 0f, 0.5f, true, 1f);
						this._fakeLegs.SetState(FakeLegsController.State.IDLE);
						this._playerFsm.SendSignal(KayakController.KayakSignals.NoForce);
					}
					else
					{
						this._lastSideForce = base.Horizontal;
						this._fakeLegs.SetState(FakeLegsController.State.MOVEMENT);
						if (this._playerFsm.CurStateID == KayakController.KayakPlayerStates.Forward)
						{
							if (this.Vertical < 0f)
							{
								this._rowingReverse = true;
								this._playerFsm.SendSignal(KayakController.KayakSignals.NegForce);
								return;
							}
						}
						else if (this.Vertical >= 0f)
						{
							this._rowingReverse = true;
							this._playerFsm.SendSignal(KayakController.KayakSignals.Force);
							return;
						}
						Vector2 vector;
						vector..ctor(this.Vertical, base.Horizontal);
						float num = Mathf.Abs(vector.x) + Mathf.Abs(vector.y);
						this._forwarForceWeight = Mathf.Abs(vector.x) / num * 100f;
						if (vector.x > 0f || Mathf.Approximately(vector.x, 0f))
						{
							if (vector.y < 0f)
							{
								this._leftForceWeight = Mathf.Abs(vector.y) / num * 100f;
							}
							else
							{
								this._rightForceWeight = Mathf.Abs(vector.y) / num * 100f;
							}
						}
						else
						{
							if (!Mathf.Approximately(vector.y, 0f))
							{
								this._forwarForceWeight = 0f;
							}
							if (vector.y > 0f)
							{
								this._leftForceWeight = Mathf.Abs(vector.y) * 100f;
							}
							else
							{
								this._rightForceWeight = Mathf.Abs(vector.y) * 100f;
							}
						}
					}
					this.UpdateMovement();
					this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, (this.Vertical <= 0f || Mathf.Approximately(base.Horizontal, 0f)) ? this.Vertical : 0f);
					this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.RightWeight, (this.Vertical < 0f) ? 0f : base.Horizontal);
				}
			}
			else
			{
				if (!flag || !flag2)
				{
					this._playerFsm.SendSignal((this.Vertical >= 0f) ? KayakController.KayakSignals.Force : KayakController.KayakSignals.NegForce);
				}
				this.UpdateMovement();
			}
			switch (this._playerFsm.CurStateID)
			{
			case KayakController.KayakPlayerStates.Idle:
				base.UpdateFishingPrompt();
				if (this._couldTakeRodAt < Time.time)
				{
					if (this._takeRodRequest || ControlsController.ControlsActions.StartFishing.WasPressed)
					{
						this._takeRodRequest = false;
						this._playerFsm.SendSignal(KayakController.KayakSignals.Standup);
					}
				}
				else if (ControlsController.ControlsActions.StartFishing.WasPressed)
				{
					this._takeRodRequest = true;
				}
				if (base.IsOpenMap)
				{
					this._playerFsm.SendSignal(KayakController.KayakSignals.UseMap);
					return;
				}
				if (ControlsController.ControlsActions.UseAnchor.WasPressed)
				{
					this.SwitchAnchored();
				}
				break;
			case KayakController.KayakPlayerStates.Forward:
			case KayakController.KayakPlayerStates.Backward:
				this._rowingAnimSpeed = 1f;
				if (base.BoatVelocity > this._settings.VelocityToChangeRowingAnimSpeed)
				{
					float num2 = Mathf.Clamp01((base.BoatVelocity - this._settings.VelocityToChangeRowingAnimSpeed) / (this._settings.VelocityWithMaxRowingAnimSpeed - this._settings.VelocityToChangeRowingAnimSpeed));
					this._rowingAnimSpeed = 1f + num2 * (this._settings.MaxRowingAnimSpeedK - 1f);
				}
				if (base.IsAnchored)
				{
					this._takeAnchorPromptTill = Time.time + 1f;
				}
				else if (this._takeAnchorPromptTill > -1f && this._takeAnchorPromptTill < Time.time)
				{
					this._takeAnchorPromptTill = -1f;
				}
				break;
			case KayakController.KayakPlayerStates.Fishing:
				base.UpdateDrivingPrompt();
				if (ControlsController.ControlsActions.StartFishing.WasPressed && (this._driver.State == typeof(PlayerIdle) || this._driver.State == typeof(PlayerIdlePitch) || this._driver.State == typeof(PlayerEmpty) || this._driver.State == typeof(HandIdle)))
				{
					this._playerFsm.SendSignal(KayakController.KayakSignals.Sitdown);
				}
				break;
			}
			this._stamina.Update(!this.IsRowing, Time.time > this._idleFrom);
			if (this._driver != null && !base.IsFishing)
			{
				this.UpdateOarRotation();
			}
		}

		private void UpdateOarRotation()
		{
			this.Oar.rotation = Quaternion.LookRotation(this._hand2.position - this._hand.position, this._settings.transform.forward);
		}

		private void UpdateMovement()
		{
			this.paddleTipLeft.LocalPosition = base.Transform.InverseTransformPoint(this.LPaddle.position);
			this.paddleTipRight.LocalPosition = base.Transform.InverseTransformPoint(this.RPaddle.position);
			float num = this._settings.paddle.RowingVelocity * this._stamina.RowingK;
			if (base.IsAnchored)
			{
				num *= this._settings.AnchoredForceK;
			}
			this.paddleTransmissionLeft.ThrustVelocity = Mathf.Clamp(this.Vertical + base.Horizontal, -1f, 1f) * num;
			this.paddleTransmissionLeft.ThrustLocalDirection = Vector3.Slerp(Vector3.back, Vector3.left, base.Horizontal * 0.5f);
			this.paddleTransmissionLeft.ConnectionNeedSyncMark();
			this.paddleTransmissionRight.ThrustVelocity = Mathf.Clamp(this.Vertical - base.Horizontal, -1f, 1f) * num;
			this.paddleTransmissionRight.ThrustLocalDirection = Vector3.Slerp(Vector3.back, Vector3.right, -base.Horizontal * 0.5f);
			this.paddleTransmissionRight.ConnectionNeedSyncMark();
		}

		protected override void UpdateFSM()
		{
			base.UpdateFSM();
			KayakController.KayakPlayerStates curStateID = this._playerFsm.CurStateID;
			if (curStateID != KayakController.KayakPlayerStates.Forward)
			{
				if (curStateID == KayakController.KayakPlayerStates.Backward)
				{
					this._driver.SetAnimationTargetWeight("KayakOarRow", this._forwarForceWeight, 0.5f, false, 1f);
					this._driver.SetAnimationTargetWeight("KayakOarRowLeft", this._rightForceWeight, 0.5f, false, 1f);
					this._driver.SetAnimationTargetWeight("KayakOarRowRight", this._leftForceWeight, 0.5f, false, 1f);
				}
			}
			else
			{
				PlayerController driver = this._driver;
				string text = "KayakOarRow";
				float num = this._forwarForceWeight;
				float num2 = 0.5f;
				float num3 = this._rowingAnimSpeed;
				driver.SetAnimationTargetWeight(text, num, num2, false, num3);
				PlayerController driver2 = this._driver;
				text = "KayakOarRowLeft";
				num3 = this._leftForceWeight;
				num2 = 0.5f;
				num = this._rowingAnimSpeed;
				driver2.SetAnimationTargetWeight(text, num3, num2, false, num);
				PlayerController driver3 = this._driver;
				text = "KayakOarRowRight";
				num = this._rightForceWeight;
				num2 = 0.5f;
				num3 = this._rowingAnimSpeed;
				driver3.SetAnimationTargetWeight(text, num, num2, false, num3);
			}
		}

		protected override void UpdatePhysics()
		{
			base.UpdatePhysics();
			float num = 0f;
			float num2 = 0f;
			KayakController.KayakPlayerStates curStateID = this._playerFsm.CurStateID;
			if (curStateID == KayakController.KayakPlayerStates.Idle || curStateID == KayakController.KayakPlayerStates.Forward || curStateID == KayakController.KayakPlayerStates.Backward)
			{
				float vectorsYaw = Math3d.GetVectorsYaw(base.Transform.forward, this._mouseController.CameraDirection);
				num2 = Mathf.Sign(vectorsYaw);
				float num3 = Math.Abs(vectorsYaw);
				if (num3 >= 360f)
				{
					num3 = 360f - num3;
				}
				if (num3 > this._settings.paddle.YawToStartHandsRotation)
				{
					num = 1f - (this._mouseController.maximumX - num3) / (this._mouseController.maximumX - this._settings.paddle.YawToStartHandsRotation);
					this._driver.transform.localRotation = Quaternion.Euler(0f, num2 * num * this._settings.paddle.HandsMaxYaw, 0f);
				}
				else
				{
					this._driver.transform.localRotation = Quaternion.identity;
				}
			}
			bool flag = this._leftForceWeight > 0f || this._rightForceWeight > 0f;
			bool flag2 = this._forwarForceWeight > 0f;
			if (flag || flag2)
			{
				this._bowForcePoint.Motor = num2 * num * this._settings.paddle.MaxForceFromHandsRotation * this._settings.transform.right;
				if (flag)
				{
					int num4 = ((this._leftForceWeight <= 0f) ? 1 : (-1));
					this._bowForcePoint.Motor += (float)num4 * this._settings.FakeTurnsF * this._settings.transform.right;
					this._sternForcePoint.Motor = -this._bowForcePoint.Motor;
				}
				else
				{
					this._sternForcePoint.Motor = Vector3.zero;
				}
				if (flag2)
				{
					Vector3 vector = (float)((this._playerFsm.CurStateID != KayakController.KayakPlayerStates.Forward) ? (-1) : 1) * this._settings.FakeMovementF * this._settings.transform.forward;
					this._bowForcePoint.Motor += vector;
				}
			}
			else
			{
				this._bowForcePoint.Motor = Vector3.zero;
				this._sternForcePoint.Motor = Vector3.zero;
			}
		}

		protected override void StopBoat()
		{
			this._bowForcePoint.Motor = Vector3.zero;
			this._sternForcePoint.Motor = Vector3.zero;
			this.paddleTransmissionLeft.ThrustVelocity = 0f;
			this.paddleTransmissionRight.ThrustVelocity = 0f;
			this.paddleTransmissionLeft.ConnectionNeedSyncMark();
			this.paddleTransmissionRight.ConnectionNeedSyncMark();
			base.StopBoat();
		}

		private void TakeOar()
		{
			this.Oar.SetParent(this._hand);
			this.Oar.localPosition = Vector3.zero;
			this.Oar.localRotation = Quaternion.identity;
		}

		private void PutOar()
		{
			this.Oar.SetParent(base.Transform);
			this.Oar.localPosition = this._settings.OarAnchor.localPosition;
			this.Oar.localRotation = this._settings.OarAnchor.localRotation;
		}

		private const float STOP_ROWING_BLEND_TIME = 1.5f;

		private const float DELAY_TO_TAKE_ROD = 1f;

		private const float STAMINA_IDLE_DELAY = 1f;

		protected PointOnRigidBody paddleMotorLeft;

		protected PointOnRigidBody paddleMotorRight;

		protected PointOnRigidBody paddleTipLeft;

		protected PointOnRigidBody paddleTipRight;

		protected PointToRigidBodyResistanceTransmission paddleTransmissionLeft;

		protected PointToRigidBodyResistanceTransmission paddleTransmissionRight;

		protected bool resetSpline = true;

		private float paddleSide;

		private float paddleRow;

		private Vector3 paddleRestPosition;

		private float _forwarForceWeight;

		private float _leftForceWeight;

		private float _rightForceWeight;

		private StaminaController _stamina;

		private float _rowingAnimSpeed = 1f;

		private float _couldTakeRodAt = -1f;

		private bool _takeRodRequest;

		private float _idleFrom = -1f;

		private float _takeAnchorPromptTill = -1f;

		private float _lastSideForce;

		private const float ACCEPT_ROLL_BACK_DELAY = 3f;

		private float _verticalRowingTime;

		private float _horizontalRowingTime;

		private bool _rowingReverse;

		public enum KayakPlayerStates
		{
			None,
			Idle,
			MoveIn,
			Forward,
			Backward,
			Unboarding,
			PrepareFishing,
			Fishing,
			EndFishing,
			UnboardFisher,
			StartRecovery,
			StartRecoveryFishing,
			Recovery,
			InGameMap
		}

		public enum KayakSignals
		{
			None,
			Boarded,
			Force,
			NegForce,
			NoForce,
			Unboarding,
			Unboarded,
			Standup,
			ReadyToFish,
			Sitdown,
			ReadyToDrive,
			Roll,
			RollRecovering,
			HiddenUnboarding,
			ReadyToMoveForward,
			ReadyToMoveBackward,
			UseMap,
			CloseMap
		}
	}
}
