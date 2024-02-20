using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Boats;
using Cayman;
using CodeStage.AdvancedFPSCounter;
using DebugCamera;
using InControl;
using ObjectModel;
using Phy;
using RootMotion.FinalIK;
using TPM;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerController : MonoBehaviour
{
	public PlayerController()
	{
		Dictionary<BrokenTackleType, Action> dictionary = new Dictionary<BrokenTackleType, Action>();
		dictionary.Add(BrokenTackleType.Line, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowLineBroken();
			}
		});
		dictionary.Add(BrokenTackleType.LineCut, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowLineBrokenCut();
			}
		});
		dictionary.Add(BrokenTackleType.Reel, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowReelBroken();
			}
		});
		dictionary.Add(BrokenTackleType.Rod, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowRodBroken();
			}
		});
		dictionary.Add(BrokenTackleType.Quiver, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowQuiverBroken();
			}
		});
		dictionary.Add(BrokenTackleType.Bobber, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowBobberIsBroken();
			}
		});
		dictionary.Add(BrokenTackleType.Sinker, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowSinkerIsBroken();
			}
		});
		dictionary.Add(BrokenTackleType.Leader, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowLeaderIsBroken();
			}
		});
		dictionary.Add(BrokenTackleType.LeaderCut, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowLeaderIsBrokenCut();
			}
		});
		dictionary.Add(BrokenTackleType.Hook, delegate
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowLureBroken();
			}
		});
		dictionary.Add(BrokenTackleType.FishCage, delegate
		{
			if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.FishCage != null)
			{
				PhotonConnectionFactory.Instance.Profile.FishCage.Clear();
			}
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowFishCageBroken();
			}
		});
		this._brokenItemActions = dictionary;
		this.handsDamper = 40f;
		this.handsWithFishDamper = 80f;
		this.cameraIn3dViewPosition = new Vector3(0f, 0.5f, -2f);
		this._letSitToTheWater = true;
		this.feedingsToSend = new List<Feeding>();
		this._independentChumController = new WaterChumController();
		this.pausedAnimsSpeed = new Dictionary<string, Dictionary<string, float>>();
		this._changingAnimations = new List<PlayerController.SmoothAnimationWeight>();
		this.OnActivateTarget = delegate
		{
		};
		this.OnUpdateTargetLocalPos = delegate
		{
		};
		this.OnDeactivateTarget = delegate
		{
		};
		this.rodLocalPoints = new Vector3[]
		{
			new Vector3(-0.02f, 0.05f, 1f),
			new Vector3(-0.01f, 0f, 0.9f)
		};
		base..ctor();
	}

	public void InitThrownPos()
	{
		if (!this.IsSailing)
		{
			this.EnableMovement();
		}
		if (this.Rod != null)
		{
			this._initThrownPos[this.Rod.RodSlot.Index] = this.Tackle.HookAnchor.position;
		}
		if (this.IsBoatFishing && this._rodPods.Count > 0)
		{
			this._isTrollingRodThrown = true;
		}
	}

	public void OverwriteCatchedFishPos(int slot, Vector3 position)
	{
		if (this._initThrownPos.ContainsKey(slot))
		{
			this._initThrownPos[slot] = position;
		}
	}

	public void SaveCatchedFishPos()
	{
		this._catchedFishPos = new Dictionary<int, Vector3>(this._initThrownPos);
		if (this.Rod != null)
		{
			this._lastCatchedFishSlot = this.Rod.RodSlot.Index;
		}
	}

	public Vector3 CatchedFishPos
	{
		get
		{
			return (!this._catchedFishPos.ContainsKey(this._lastCatchedFishSlot)) ? Vector3.zero : this._catchedFishPos[this._lastCatchedFishSlot];
		}
	}

	public GripSettings Grip { get; private set; }

	public Collider ZonesCollider
	{
		get
		{
			return this._zonesCollider;
		}
	}

	public bool IsRequestedRodDestroying
	{
		get
		{
			return this._isRequestedRodDestroying;
		}
	}

	public bool IsRodActive
	{
		get
		{
			return this._isRodActive;
		}
	}

	public bool InFishingZone
	{
		get
		{
			return (PhotonConnectionFactory.Instance.IsFreeRoamingOn && !StaticUserData.IS_IN_TUTORIAL) || this.EnteredFishingZonesCount > 0;
		}
	}

	public bool IsReadyForRod
	{
		get
		{
			if (this.IsSailing)
			{
				return this.CurrentBoat.IsReadyForRod;
			}
			return this.InFishingZone;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Type, bool> OnPlayerActiveState = delegate(Type type, bool active)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<AssembledRod> OnChangeRod = delegate(AssembledRod rod)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnBoarded = delegate
	{
	};

	public IBoatController CurrentBoat
	{
		get
		{
			return this._currentBoat;
		}
		set
		{
			this._currentBoat = value;
			this.OnBoarded(this.IsSailing);
		}
	}

	public bool IsCameraRoutingOn { get; private set; }

	public bool IsSailing
	{
		get
		{
			return this._currentBoat != null;
		}
	}

	public bool IsBoatFishing { get; private set; }

	public bool IsCurrentlyTrolling
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsTrolling;
		}
	}

	public bool IsTrollingRodThrown
	{
		get
		{
			return this._isTrollingRodThrown;
		}
	}

	public bool IsAnchored
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsAnchored;
		}
	}

	public float BoatVelocity
	{
		get
		{
			return (!this.IsSailing) ? 0f : this.CurrentBoat.BoatVelocity;
		}
	}

	public float Stamina
	{
		get
		{
			return (!this.IsSailing) ? 0f : this.CurrentBoat.Stamina;
		}
	}

	public bool IsOarPresent
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsOarPresent;
		}
	}

	public bool IsEnginePresent
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsEnginePresent;
		}
	}

	public bool IsTrollingPossible
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsTrollingPossible;
		}
	}

	public bool IsRowing
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsRowing;
		}
	}

	public bool IsUsingBoatEngine
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.IsEngineForceActive;
		}
	}

	public Vector3 Position
	{
		get
		{
			return (!this.IsSailing) ? base.transform.position : this.CurrentBoat.Position;
		}
	}

	public Vector3 Rotation
	{
		get
		{
			return (!this.IsSailing) ? base.transform.rotation.eulerAngles : this.CurrentBoat.Rotation.eulerAngles;
		}
	}

	public int BoatTypeInUse
	{
		get
		{
			return (int)((!this.IsSailing) ? ItemSubTypes.All : this.CurrentBoat.Category);
		}
	}

	public bool CantOpenInventory
	{
		get
		{
			return this.IsSailing && this.CurrentBoat.CantOpenInventory;
		}
	}

	public bool IsStriking
	{
		get
		{
			return this.pullValue.isActive() || (this.Rod != null && this.Rod.RodTipMoveSpeed > 12f / this.Rod.Length && !this.Rod.IsOngoingRodPodTransition);
		}
	}

	public bool StrikeMovesBack
	{
		get
		{
			return !this.pullValue.isActive() && this.pullValue.ValueRatio >= 0.1f;
		}
	}

	public bool HasFullStrike
	{
		get
		{
			return this.pullValue.ValueRatio >= 0.9f;
		}
	}

	public bool IsPulling
	{
		get
		{
			return this.pullValue.ValueRatio >= 0.2f;
		}
	}

	public bool IsPullTriggered
	{
		get
		{
			return this.pullTriggered;
		}
	}

	public bool IsCurrentlyPullingOrStriking
	{
		get
		{
			return this.Rod.RodTipMoveSpeed > 0.4f;
		}
	}

	public bool IsReeling
	{
		get
		{
			return this.Reel != null && this.Reel.IsReeling;
		}
	}

	public void Update3dCharMecanimParameter(TPMMecanimIParameter name, byte value)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateMecanimParameter(name, value);
		}
	}

	public void Update3dCharMecanimParameter(TPMMecanimFParameter name, float value)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateMecanimParameter(name, value);
		}
	}

	public void Update3dCharMecanimParameter(TPMMecanimBParameter name, bool value)
	{
		if (StaticUserData.IS_TPM_ENABLED && this._cache != null)
		{
			this._cache.CurFraction.UpdateMecanimParameter(name, value);
		}
	}

	public void UpdateRod(Transform rodTransform, List<Vector3> points)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateRod(rodTransform, points);
		}
	}

	public void UpdateFakeRod(int id, Transform rodTransform, IList<Vector3> points, Vector3[] mainAndLeaderPoints, Vector3 sinkersFirstPoint, bool isLeaderVisible, Transform tackleTransfrom, Transform hookTransform)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateFakeRod(id, rodTransform, points, mainAndLeaderPoints, sinkersFirstPoint, isLeaderVisible, tackleTransfrom, hookTransform);
		}
	}

	public void ChangeRodHandler(AssembledRod rodAssembly)
	{
		if (this.Reel != null)
		{
			this.UpdateReelIkController();
		}
		this.CurRodSessionSettings.IsOnPod = false;
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.TakeRod((this.Rod != null) ? rodAssembly : null);
		}
		this.OnChangeRod(rodAssembly);
	}

	public void UpdateLinePoints(Vector3[] mainAndLeaderPoints, Vector3 sinkersFirstPoint, bool isLeaderVisible, bool isLineContactGround)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsLineContactGround, isLineContactGround);
			this._cache.CurFraction.UpdateLinePoints(mainAndLeaderPoints, sinkersFirstPoint, isLeaderVisible);
		}
	}

	public void UpdateTackleThrowData(Vector3? position, float? startAngle)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateTackleThrowData(position, startAngle);
		}
	}

	public void UpdateTackle(Transform tackleTransfrom, Transform hookTransform)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateTackle(tackleTransfrom, hookTransform);
		}
	}

	public void ChangeBaitVisibility(bool flag)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.isBaitVisibility = flag;
		}
	}

	public void OnChangeLeftHandRod(bool flag)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.isLeftHandRod = flag;
		}
	}

	public int InformAboutNewFish(Fish fishTemplate, Vector3 spawnPosition)
	{
		return (!StaticUserData.IS_TPM_ENABLED) ? 0 : this._cache.CurFraction.AddFish(fishTemplate, spawnPosition);
	}

	public void UpdateFish(int instanceId, Vector3 position, Vector3 fishBackward, Vector3 fishBackward2, Vector3 fishRight, TPMFishState state)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateFish(instanceId, position, fishBackward, fishBackward2, fishRight, state);
		}
	}

	public void InformFishDestroyed(int instanceId)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.DelFish(instanceId);
		}
	}

	public int AddItem(int itemId, Vector3 position)
	{
		return (!StaticUserData.IS_TPM_ENABLED) ? 0 : this._cache.CurFraction.AddItem(itemId, position);
	}

	public void UpdateFakeFish(int itemId, Vector3 position)
	{
		this._cache.CurFraction.UpdateItem(itemId, position, TPMFishState.None);
	}

	public void UpdateItem(int itemId, Vector3 position, TPMFishState state)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdateItem(itemId, position, state);
		}
	}

	public void DestroyItem(int itemId)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.DestroyItem(itemId);
		}
	}

	public void OnNewAnimation(string clipName)
	{
		if (!StaticUserData.IS_TPM_ENABLED)
		{
			return;
		}
		int hashForAnimateAnimation = AnimateToMecanimAdaptor.GetHashForAnimateAnimation(clipName);
		if (hashForAnimateAnimation != 0)
		{
			this._cache.CurFraction.currentClipHash = hashForAnimateAnimation;
		}
	}

	public void UpdateFirework(int itemId)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.fireworkID = itemId;
		}
	}

	public void SetLabelsVisibility(bool flag)
	{
		if (this._tpmCharacters != null)
		{
			this._tpmCharacters.SetLabelsVisibility(flag);
		}
	}

	public GameObject Map
	{
		get
		{
			return this._map;
		}
	}

	public bool IsTransitionToMap { get; set; }

	public bool IsDrawRodPodRequest { get; set; }

	public CameraController CameraController
	{
		get
		{
			return this.cameraController;
		}
	}

	public Transform Root
	{
		get
		{
			return this._root;
		}
	}

	public PlayerTargetCloser TargetCloser
	{
		get
		{
			return this._targetCloser;
		}
	}

	public string SleevesPrefabName
	{
		get
		{
			return this._sleevesPrefabName;
		}
		set
		{
			GameObject gameObject = (GameObject)Resources.Load(value, typeof(GameObject));
			if (gameObject == null)
			{
				LogHelper.Error("PlayerController Sleeves: {0} prefab can't instantiate", new object[] { value });
				return;
			}
			this._sleevesPrefabName = value;
			this.DestroyObject(this._sleevesData.gameObject);
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, this.Arms.transform.position, this.Arms.transform.rotation);
			gameObject2.transform.parent = this.Arms.transform;
			gameObject2.SetActive(true);
			this._sleevesData = gameObject2.GetComponent<SleevesHelper>();
			SkinnedMeshRenderer componentInChildren = this._sleevesData.GetComponentInChildren<SkinnedMeshRenderer>();
			this._sleevesData.sleevesMaterial = componentInChildren.material;
			this.RetargetSleevesSkeleton(this._sleevesData.GetComponentInChildren<SkinnedMeshRenderer>());
			this.InitFurMaterial(this._sleevesData.sleevesMeshObject);
		}
	}

	public ReelHandler ReelHandlerProp
	{
		get
		{
			return this.Reel.ReelHandler;
		}
	}

	public GameFactory.RodSlot RodSlot { get; private set; }

	[HideInInspector]
	public GameObject RodObject
	{
		get
		{
			return (this.RodSlot.Rod == null) ? null : this.RodSlot.Rod.gameObject;
		}
	}

	[HideInInspector]
	public Rod1stBehaviour Rod { get; private set; }

	[HideInInspector]
	public TackleBehaviour Tackle { get; private set; }

	[HideInInspector]
	public Reel1stBehaviour Reel { get; private set; }

	[HideInInspector]
	public Line1stBehaviour Line { get; private set; }

	[HideInInspector]
	public Bell1stBehaviour Bell { get; private set; }

	public FeederBehaviour Feeder
	{
		get
		{
			return this.Tackle as FeederBehaviour;
		}
	}

	public TackleThrowData ThrowData
	{
		get
		{
			return this.throwData;
		}
	}

	public ReelTypes ReelType
	{
		get
		{
			return this._reelType;
		}
		set
		{
			this._reelType = value;
			this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBaitcasting, this._reelType == ReelTypes.Baitcasting);
		}
	}

	public float MovedDown
	{
		get
		{
			return this._m_moveddown;
		}
		set
		{
			this._m_moveddown = value;
		}
	}

	public float BowDown
	{
		get
		{
			return this._m_bowdown;
		}
		set
		{
			this._m_bowdown = value;
		}
	}

	public FootStepsHelper FootStepsHelper
	{
		get
		{
			return this._fpController.FootStepsHelper;
		}
	}

	public float DebugBobberStartForce
	{
		get
		{
			return this._debugBobberStartForce;
		}
	}

	public float DebugBobberEndForce
	{
		get
		{
			return this._debugBobberEndForce;
		}
	}

	public float DebugBobberForceApplyTimePrc
	{
		get
		{
			return this._debugBobberForceApplyTimePrc;
		}
	}

	public float DebugBobberForceReleaseTimePrc
	{
		get
		{
			return this._debugBobberForceReleaseTimePrc;
		}
	}

	public float DebugBobberDuration
	{
		get
		{
			return this._debugBobberDuration;
		}
	}

	public float DebugBobberDisturbSize
	{
		get
		{
			return this._debugBobberDisturbSize;
		}
	}

	public bool ShowSleeves
	{
		get
		{
			return this._m_showSleevesFlag;
		}
		set
		{
			this._m_showSleevesFlag = value;
			float num = ((!value) ? 0.5f : 0f);
			if (this._sleevesData != null && this._sleevesData.sleevesMaterial != null)
			{
				this._sleevesData.sleevesMaterial.SetFloat("_Cutoff", num);
			}
		}
	}

	public bool IsFailThrowing { get; set; }

	public bool IsPitching
	{
		get
		{
			RodSessionSettings curRodSessionSettings = this.CurRodSessionSettings;
			bool flag;
			if (curRodSessionSettings != null)
			{
				bool? isPitching = curRodSessionSettings.IsPitching;
				flag = ((isPitching == null) ? (StaticUserData.RodInHand.Rod.ItemSubType == ItemSubTypes.TelescopicRod) : isPitching.Value);
			}
			else
			{
				flag = false;
			}
			return flag;
		}
		set
		{
			RodSessionSettings curRodSessionSettings = this.CurRodSessionSettings;
			if (curRodSessionSettings != null)
			{
				this.CurRodSessionSettings.IsPitching = new bool?(value);
				this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsPitching, value);
			}
		}
	}

	public RodSessionSettings CurRodSessionSettings
	{
		get
		{
			if (StaticUserData.RodInHand == null)
			{
				return new RodSessionSettings();
			}
			if (this._rodsSavedSettings.ContainsKey(StaticUserData.RodInHand.Rod.ItemId))
			{
				return this._rodsSavedSettings[StaticUserData.RodInHand.Rod.ItemId];
			}
			RodSessionSettings rodSessionSettings = new RodSessionSettings();
			this._rodsSavedSettings[StaticUserData.RodInHand.Rod.ItemId] = rodSessionSettings;
			return rodSessionSettings;
		}
	}

	public Type State
	{
		get
		{
			return (this.fsm != null) ? this.fsm.CurrentStateType : null;
		}
	}

	public Transform LinePositionInRightHand
	{
		get
		{
			return this.linePositionInRightHand;
		}
	}

	public Transform LinePositionInLeftHand
	{
		get
		{
			return this.linePositionInLeftHand;
		}
	}

	public bool HasAdvancedLicense
	{
		get
		{
			if (StaticUserData.CurrentPond == null || PhotonConnectionFactory.Instance.Profile == null || PhotonConnectionFactory.Instance.Profile.Licenses == null)
			{
				return false;
			}
			return PhotonConnectionFactory.Instance.Profile.ActiveLicenses.Any((PlayerLicense l) => l.StateId == StaticUserData.CurrentPond.State.StateId && l.IsAdvanced);
		}
	}

	public bool IsBreakLineAvailable
	{
		get
		{
			return !StaticUserData.IS_IN_TUTORIAL && this.Tackle != null && ((this.Tackle.IsFishHooked && this.State != typeof(PlayerShowFishLineIn)) || this.Tackle.IsHitched);
		}
	}

	public int LineLength
	{
		get
		{
			return (this.Rod == null || this.Line == null) ? 0 : ((int)MeasuringSystemManager.LineLength(this.Rod.LineOnRodLength + this.Line.MaxLineLength));
		}
	}

	public int LineLengthAvailable
	{
		get
		{
			return (this.Rod == null || this.Line == null) ? 0 : ((int)MeasuringSystemManager.LineLength(this.Rod.LineOnRodLength + this.Line.AvailableLineLengthOnSpool));
		}
	}

	public Transform HandTransform
	{
		get
		{
			ReelTypes reelType = GameFactory.Player.ReelType;
			if (reelType == ReelTypes.Spinning)
			{
				return GameFactory.Player.LinePositionInLeftHand;
			}
			if (reelType != ReelTypes.Baitcasting)
			{
				return null;
			}
			return GameFactory.Player.LinePositionInRightHand;
		}
	}

	public Transform Collider
	{
		get
		{
			return this.Player.transform.parent;
		}
	}

	public bool WasFirstZoneEntered { get; set; }

	public Transform LastPin
	{
		get
		{
			return this._lastPin;
		}
	}

	public void Move(Transform newTransform)
	{
		this.WasFirstZoneEntered = false;
		this._lastPin = newTransform;
		bool flag = false;
		if (GameFactory.BoatDock != null)
		{
			for (int i = 0; i < GameFactory.BoatDock.Boats.Count; i++)
			{
				if (GameFactory.BoatDock.Boats[i].HiddenLeave())
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.CleanRodPods();
		}
		this.ShowSleeves = false;
		this.CameraController.CameraMouseLook.ResetOriginalRotation(newTransform.rotation);
		Vector3 localPosition = newTransform.localPosition;
		Vector3? groundCollision = Math3d.GetGroundCollision(new Vector3(localPosition.x, 0f, localPosition.z));
		this.Collider.position = ((groundCollision == null) ? newTransform.localPosition : (groundCollision.Value + new Vector3(0f, 1.6f, 0f)));
		this.EmptyOnMoveSwitch = this.Rod != null || this.IsHandThrowMode;
		if (LazyManualUpdater.Instance != null)
		{
			LazyManualUpdater.Instance.ForceUpdate();
		}
		if (this.Rod != null)
		{
			this.Rod.InvalidateSimulation();
		}
	}

	public void OnExternalMove(Vector3 pos, Quaternion rotation, float prc)
	{
		if (this.IsSailing)
		{
			this._currentBoat.SetExternalGlobalRotation(rotation);
		}
		else
		{
			this.Collider.position = pos;
			this.CameraController.CameraMouseLook.ResetOriginalRotation(rotation);
		}
	}

	public void OnBoardingMove(Vector3 pos, Quaternion rotation, float prc)
	{
		this.Collider.position = pos;
		this.CameraController.CameraMouseLook.ResetOriginalRotation(rotation);
		this.CameraController.Camera.transform.localPosition = Vector3.Lerp(this.BoardingFromLocalCameraPosition, this.BoardingToLocalCameraPosition, prc);
	}

	public bool EmptyOnMoveSwitch { get; private set; }

	public void ResetEmptyOnMoveSwitch()
	{
		this.EmptyOnMoveSwitch = false;
	}

	public bool CanLeaveRoom
	{
		get
		{
			return this.CanLeaveBoat;
		}
	}

	public FirstPersonControllerFP FPController
	{
		get
		{
			return this._fpController;
		}
	}

	public void MoveToBoat()
	{
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsInGame, false);
		this._isReadyForBoarding = false;
		this.CurrentBoat = GameFactory.BoatDock.Boats.FirstOrDefault((IBoatController b) => b.Category == this._boardingCategory);
		this.DisableMovement();
		this._fpController.enabled = false;
		this.ActivateRod(false);
		this.CurrentBoat.TakeControll(this);
		this.CleanRodPods();
		this._savedRodPods.Clear();
		this._savedRodPods.AddRange(this._rodPods);
		this._rodPods.Clear();
		if (this.CurrentBoat.RodSlots != null)
		{
			this._rodPods.Add(this.CurrentBoat.RodSlots);
		}
	}

	public bool CanLeaveBoat
	{
		get
		{
			return this._canLeaveBoatStates.Contains(this.fsm.CurrentStateType);
		}
	}

	public Transform UnboardingObject
	{
		get
		{
			return this._unboardingObject;
		}
	}

	public Vector3? BoardingFrom { get; set; }

	public void MoveFromBoat(Vector3 currentPosition, Vector3 unboardingPosition)
	{
		this.SetHandsVisibility(false);
		this.BoardingFrom = new Vector3?(currentPosition);
		this._unboardingObject = new GameObject("unboarding").transform;
		this._unboardingObject.position = unboardingPosition;
		this._unboardingObject.rotation = Quaternion.Euler(0f, this.CameraController.Camera.transform.eulerAngles.y, 0f);
		this.IsBoatFishing = false;
		this.PlayAnimation("empty", 1f, 1f, 0f);
		this.ShowSleeves = false;
	}

	public void OnMoveFromBoatFinished()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			this._finishUnboardingRequest = true;
			return;
		}
		this._finishUnboardingRequest = false;
		this.CleanRodPods();
		this.SetHandsVisibility(true);
		if (this._unboardingObject != null)
		{
			Object.Destroy(this._unboardingObject.gameObject);
		}
		this._unboardingObject = null;
		this.PlayAnimation("empty", 1f, 1f, 0f);
		this._isReadyForBoarding = false;
		sbyte savedFishingSlot = this.CurrentBoat.SavedFishingSlot;
		this.CurrentBoat = null;
		this.IsBoatFishing = false;
		this.ShowSleeves = false;
		this.EnableMovement();
		this._fpController.enabled = true;
		if (this._refreshBoatsRequest)
		{
			this._refreshBoatsRequest = false;
			this.RecreateAllBoats();
		}
		this._rodPods.Clear();
		this._rodPods.AddRange(this._savedRodPods);
		this._savedRodPods.Clear();
		this.CameraController.enabled = true;
		this.CameraController.CameraMouseLook.enabled = true;
		if (this.IsReadyForRod && StaticUserData.RodInHand.IsRodDisassembled && !this.IsEmptyHandsMode && (int)savedFishingSlot >= 0)
		{
			Rod rod = RodHelper.FindRodInSlot((int)savedFishingSlot, null);
			if (rod != null)
			{
				this.SaveTakeRodRequest(rod, false);
			}
		}
	}

	public void PrepareFishingFromBoat()
	{
		this.IsBoatFishing = true;
		this.ShowSleeves = false;
		if (!this.IsEmptyHandsMode && this.IsReadyForRod && StaticUserData.RodInHand.IsRodDisassembled && this.RequestedRod == null && !GameFactory.IsRodAssembling)
		{
			GameFactory.Message.ShowNoAssembledRods();
		}
	}

	public void InitFishingToDriving(Action action)
	{
		this._fishingToDrivingAction = action;
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsInGame, false);
		this.IsBoatFishing = false;
		this.DisableMovement();
		this._fpController.enabled = false;
	}

	public void OnInitFishingToDrivingDone()
	{
		if (this._fishingToDrivingAction != null)
		{
			this._fishingToDrivingAction();
			this._fishingToDrivingAction = null;
		}
	}

	public void FinishFishingFromBoat()
	{
		this.ShowSleeves = true;
		this.ActivateRod(false);
		this.Hands.SetActive(true);
	}

	public int EnteredFishingZonesCount
	{
		get
		{
			return this.enteredFishingZones.Count;
		}
	}

	public void OnEnterFishingZone(GameObject zone)
	{
		this.enteredFishingZones.Add(zone);
		if (this.EnteredFishingZonesCount == 1)
		{
			this.OnEnterFishingZone();
		}
	}

	public void OnExitFishingZone(GameObject zone)
	{
		this.enteredFishingZones.Remove(zone);
	}

	public void OnEnterFishingZone()
	{
		this.WasFirstZoneEntered = true;
		if (this.CurFirework != null || this.IsSailing)
		{
			return;
		}
		if (ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.OnEnterFishingZone();
		}
		if (!this.IsEmptyHandsMode && !GameFactory.IsRodAssembling && StaticUserData.RodInHand.IsRodDisassembled && this.RequestedRod == null && !RodHelper.IsInventorySlotOccupiedByRodStand(PhotonConnectionFactory.Instance.PendingGameSlotIndex))
		{
			this.IsEmptyHandsMode = true;
			if (this.IsReadyForRod)
			{
				ControlsController.ControlsActions.SetInFishingZoneMappings();
			}
			GameFactory.Message.ShowNoAssembledRods();
		}
	}

	public void PrepareOpenMap()
	{
		this.IsTransitionToMap = true;
		this.ShowSleeves = false;
	}

	public bool IsBoatMapClosing
	{
		get
		{
			return this._isBoatMapClosing;
		}
	}

	public void OnCloseMap()
	{
		if (this.IsSailing)
		{
			this._isBoatMapClosing = true;
			this.CurrentBoat.OnCloseMap();
		}
	}

	public void FinishBoatMapClosing()
	{
		this._isBoatMapClosing = false;
	}

	public void FreezeCamera(bool flag)
	{
		if (this.IsBoatFishing)
		{
			this.CurrentBoat.FreezeCamera(flag);
		}
		else if (flag)
		{
			this.CameraController.CameraFreeze();
		}
		else
		{
			this.CameraController.CameraUnFreeze();
		}
	}

	public bool IsLookWithFishMode
	{
		get
		{
			return this._isLookWithFishMode;
		}
	}

	public void SetLookWithFishMode(bool flag)
	{
		this._isLookWithFishMode = flag;
		if (this.IsBoatFishing)
		{
			this.CurrentBoat.SetFishPhotoMode(flag);
		}
		else
		{
			CameraMouseLook component = this.CameraController.Camera.gameObject.GetComponent<CameraMouseLook>();
			if (flag)
			{
				component.enabled = true;
				component.SmoothStart();
				this.CameraController.CameraMouseLook.axes = CameraMouseLook.RotationAxes.MouseX;
			}
			else
			{
				component.SmoothResetAndStop(Quaternion.identity);
				this.CameraController.CameraMouseLook.axes = CameraMouseLook.RotationAxes.MouseXAndY;
			}
		}
	}

	public void OnColliderEnterFishZone(Collider visitor)
	{
	}

	public void OnColliderExitFishZone(Collider visitor)
	{
	}

	public void SetBoatInExitZone(bool flag)
	{
		this.CurrentBoat.SetBoatInExitZone(flag);
	}

	public bool IsTPMInitialized
	{
		get
		{
			return this._tpmCharacters != null;
		}
	}

	public TPMCharactersController CharCtr
	{
		get
		{
			return this._tpmCharacters;
		}
	}

	public ItemSubTypes BoardingCategory
	{
		get
		{
			return this._boardingCategory;
		}
	}

	public bool IsCouldBoardBoat()
	{
		if (this._isReadyForBoarding)
		{
			return true;
		}
		if (!this.IsSailing && ControlsController.ControlsActions.InteractObject.WasPressed && this._selectedBoat != null)
		{
			this._boardingCategory = this._selectedBoat.Category;
			if (this._selectedBoat.State != BoatState.FOR_RENT)
			{
				if (!this.HasAdvancedLicense)
				{
					this.ShowLicenseMissingDialog();
					return false;
				}
				this._isReadyForBoarding = true;
				return true;
			}
			else
			{
				this.ShowInitRentDialog(GameFactory.BoatDock.BoatsForRentDescriptions.FirstOrDefault((BoatDesc b) => (ItemSubTypes)b.BoatCategoryId == this._selectedBoat.Category));
			}
		}
		return false;
	}

	public void UpdateSelectedBoat()
	{
		Vector3 position = this.cameraController.Camera.transform.position;
		Vector3 vector = position + this.cameraController.Camera.transform.forward * 7.5f;
		RaycastHit selectedBoatHit = Math3d.GetMaskedRayHit(position, vector, GlobalConsts.BoatAZMask);
		this._selectedBoat = ((!(selectedBoatHit.collider != null)) ? null : GameFactory.BoatDock.Boats.FirstOrDefault((IBoatController b) => b.IsBeingLookedAt(selectedBoatHit.collider)));
		if (this._selectedBoat != null)
		{
			if (this._selectedBoat.State != BoatState.FOR_RENT)
			{
				ShowHudElements.Instance.ShowBoatBoarding();
			}
			else if (!HudTournamentHandler.IsWarningOfEnd)
			{
				ShowHudElements.Instance.ShowBoatRent();
			}
		}
	}

	private void ShowLicenseMissingDialog()
	{
		this._boatRentController.ShowBoatFishingLicenseIsMissing(new Action(this.OnAcceptFishingWithoutLicense));
	}

	private void OnAcceptFishingWithoutLicense()
	{
		this._isReadyForBoarding = true;
	}

	public IBoatController GetBoatByCategory(int categoryId)
	{
		return GameFactory.BoatDock.Boats.FirstOrDefault((IBoatController b) => (int)b.Category == categoryId);
	}

	private void RentExpired(BoatDesc boatDesc)
	{
		PhotonConnectionFactory.Instance.OutdateRent();
		if (this.CurrentBoat != null && this.CurrentBoat.Category == (ItemSubTypes)boatDesc.BoatCategoryId)
		{
			this.CurrentBoat.OnRentExpired();
			Transform playerRespawnPos = GameFactory.BoatDock.PlayerRespawnPos;
			if (playerRespawnPos != null)
			{
				this.Collider.position = playerRespawnPos.position;
				this.CameraController.CameraMouseLook.ResetOriginalRotation(playerRespawnPos.rotation);
			}
		}
		else
		{
			IBoatController boatByCategory = this.GetBoatByCategory(boatDesc.BoatCategoryId);
			if (boatByCategory != null)
			{
				boatByCategory.OnRentExpired();
			}
		}
		this.CheckNextBoatRentExtention();
	}

	private void RentBoat(BoatDesc description, int daysCount)
	{
		if (this._rentWaitingCategory != -1)
		{
			LogHelper.Error("Request to rent {0} when previous {1} was not processed", new object[]
			{
				(ItemSubTypes)description.BoatCategoryId,
				(ItemSubTypes)this._rentWaitingCategory
			});
		}
		else
		{
			this._rentWaitingCategory = description.BoatCategoryId;
			PhotonConnectionFactory.Instance.RentBoat(description.BoatId, daysCount);
			PhotonConnectionFactory.Instance.OnBoatRented += this.OnRentAccepted;
			PhotonConnectionFactory.Instance.OnErrorRentingBoat += this.OnRentDeclined;
		}
	}

	private void OnRentAccepted()
	{
		IBoatController boatByCategory = this.GetBoatByCategory(this._rentWaitingCategory);
		if (boatByCategory.State == BoatState.FOR_RENT)
		{
			if (!this.HasAdvancedLicense)
			{
				this.ShowLicenseMissingDialog();
			}
			else
			{
				this._isReadyForBoarding = true;
			}
		}
		boatByCategory.Rent();
		this.UnsubscribeFromRenting();
	}

	private void OnRentDeclined(Failure f)
	{
		this.UnsubscribeFromRenting();
	}

	private void CheckNextBoatRentExtention()
	{
		if (this._boatsWithFinishedRent.Count > 0)
		{
			this.ShowExtendRentDialog();
		}
	}

	public void OnMovedTimeForward()
	{
		if (GameFactory.BoatDock != null)
		{
			for (int i = 0; i < GameFactory.BoatDock.Boats.Count; i++)
			{
				GameFactory.BoatDock.Boats[i].OnTimeChanged();
			}
		}
	}

	private void UnsubscribeFromRenting()
	{
		this._rentWaitingCategory = -1;
		PhotonConnectionFactory.Instance.OnBoatRented -= this.OnRentAccepted;
		PhotonConnectionFactory.Instance.OnErrorRentingBoat -= this.OnRentDeclined;
		this.CheckNextBoatRentExtention();
	}

	public void ShowCatchedItemDialog(string itemName, string itemCategory, int itemId)
	{
		ShowHudElements.Instance.ShowCatchedItemWindow(itemName, itemCategory, this.RodSlot);
	}

	private void OnCloseCatchedItemWindow(bool wasTaken)
	{
		if (wasTaken)
		{
			GameActionAdapter.Instance.TakeItem();
		}
		else
		{
			GameActionAdapter.Instance.ReleaseItem();
		}
		this.Tackle.FinishWithItem();
	}

	private void ShowInitRentDialog(BoatDesc description)
	{
		this._boatRentController.ShowRentPopup(description, new Action<BoatDesc, int>(this.RentBoat));
	}

	private void ShowExtendRentDialog()
	{
		if (StaticUserData.CurrentPond != null && StaticUserData.CurrentLocation != null && this._boatsWithFinishedRent.Count > 0)
		{
			short category = this._boatsWithFinishedRent[this._boatsWithFinishedRent.Count - 1];
			this._boatsWithFinishedRent.RemoveAt(this._boatsWithFinishedRent.Count - 1);
			BoatDesc boatDesc = GameFactory.BoatDock.BoatsForRentDescriptions.FirstOrDefault((BoatDesc b) => b.BoatCategoryId == (int)category);
			if (boatDesc != null)
			{
				this._boatRentController.ShowExtendRentPopup(boatDesc, new Action<BoatDesc, int>(this.RentBoat), new Action<BoatDesc>(this.RentExpired));
			}
		}
	}

	public List<RodPodController> RodPods
	{
		get
		{
			return this._rodPods;
		}
	}

	public List<RodPodController> SavedRodPods
	{
		get
		{
			return this._savedRodPods;
		}
	}

	public bool IsWithRodPodMode { get; set; }

	public RodPodController CurrentRodPod
	{
		get
		{
			return this._curRodPod;
		}
	}

	public void CreateRodPod()
	{
		RodStand rodStand = RodPodHelper.FindPodOnDoll();
		GameObject gameObject = Resources.Load<GameObject>(rodStand.Asset);
		this._curRodPod = Object.Instantiate<GameObject>(gameObject).GetComponent<RodPodController>();
		this._curRodPod.SetItemId(rodStand.ItemId);
		this._curRodPod.SetItem(rodStand);
		InGameMap.Instance.AddRodPod(rodStand.InstanceId.ToString(), this.Position);
	}

	public void DestroyRodPod()
	{
		InGameMap.Instance.RemoveRodPod(this._curRodPod.Item.InstanceId.ToString());
		if (this._curRodPod != null)
		{
			Object.Destroy(this._curRodPod.gameObject);
		}
	}

	public void RodPodsSetActive(bool active)
	{
		for (int i = 0; i < this._rodPods.Count; i++)
		{
			this._rodPods[i].SetActive(active);
		}
	}

	public RodPodController CurrentLookAtPod
	{
		get
		{
			return this._lookAtPod;
		}
	}

	public void UpdateRodPods()
	{
		if (this._rodPods.Count == 0)
		{
			return;
		}
		if (this.Tackle != null && this.Tackle.IsFishHooked && this.IsSailing)
		{
			if (this._lookAtPod != null)
			{
				this.ClearRodPodsHighlighting();
			}
			this.CommonUpdateRodPods();
			return;
		}
		if (this._curRodPod != null)
		{
			this._lookAtPod = this.FindLookAtRodPod();
			this.CommonUpdateRodPods();
			return;
		}
		this._lookAtPod = this.FindLookAtPodSlot(out this._lookAtPodSlot);
		if (this._lookAtPod != null)
		{
			if (!this._lookAtPod.IsSlotOccupied(this._lookAtPodSlot) && !this.IsTackleThrown)
			{
				this._lookAtPodSlot = -1;
				if (!this._lookAtPod.IsFree || !this._lookAtPod.CouldBeTaken)
				{
					this._lookAtPod = null;
				}
			}
		}
		else if (this.IsTackleThrown)
		{
			this._lookAtPod = this.FindLookAtRodPod();
			if (this._lookAtPod != null)
			{
				if (this._lookAtPod.HasFreeSlots)
				{
					this._lookAtPodSlot = this._lookAtPod.FindFreeSlot();
				}
				else
				{
					this._lookAtPod = null;
				}
			}
		}
		else
		{
			this._lookAtPod = this.FindLookAtRodPod();
			if (this._lookAtPod != null && (!this._lookAtPod.IsFree || !this._lookAtPod.CouldBeTaken))
			{
				this._lookAtPod = null;
			}
		}
		this.CommonUpdateRodPods();
	}

	private void CommonUpdateRodPods()
	{
		ShowHudElements.Instance.ActivateCrossHair(this._lookAtPod != null);
		for (int i = 0; i < this._rodPods.Count; i++)
		{
			this._rodPods[i].UpdateDeployed(this._lookAtPod == this._rodPods[i], this._lookAtPodSlot);
		}
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.UpdatePods(this._rodPods);
		}
	}

	public void ClearRodPodsHighlighting()
	{
		ShowHudElements.Instance.ActivateCrossHair(false);
		this._lookAtPod = null;
		this._lookAtPodSlot = -1;
		for (int i = 0; i < this._rodPods.Count; i++)
		{
			this._rodPods[i].UpdateDeployed(false, -1);
		}
	}

	private RodPodController FindLookAtPodSlot(out int selectedPodSlot)
	{
		selectedPodSlot = -1;
		Transform transform = this.CameraController.Camera.transform;
		RaycastHit raycastHit;
		if (!Physics.Raycast(transform.position, transform.forward, ref raycastHit, 5f, GlobalConsts.RodOnPodMask))
		{
			return null;
		}
		for (int i = 0; i < this._rodPods.Count; i++)
		{
			selectedPodSlot = this._rodPods[i].FindSlotByCollidedTransform(raycastHit.collider.transform);
			if (selectedPodSlot != -1)
			{
				return this._rodPods[i];
			}
		}
		return null;
	}

	private RodPodController FindLookAtRodPod()
	{
		Transform transform = this.CameraController.Camera.transform;
		RaycastHit hit;
		return (!Physics.Raycast(transform.position, transform.forward, ref hit, 5f, GlobalConsts.RodPodMask)) ? null : this._rodPods.FirstOrDefault((RodPodController p) => p.transform == hit.transform);
	}

	private Vector3 RodPodPutBasePoint
	{
		get
		{
			Vector3 normalized = Math3d.ProjectOXZ(base.transform.forward).normalized;
			return base.transform.position + normalized * 1f - new Vector3(0f, 1f, 0f);
		}
	}

	public void UpdateCurRodPod()
	{
		this._curRodPod.UpdatePosition(this.RodPodPutBasePoint, base.transform.rotation.eulerAngles.y);
		this._curRodPod.UpdateIdle(this._rodPods.All((RodPodController c) => c.IsFarEnough(this._curRodPod.transform.position)));
	}

	public RodPodPutState CuRodPodPutState
	{
		get
		{
			return (!(this._curRodPod == null)) ? this._curRodPod.PutState : RodPodPutState.Invalid;
		}
	}

	public void PutRodPod()
	{
		this._rodPods.Add(this._curRodPod);
		PhotonConnectionFactory.Instance.PutRodStand(this._curRodPod.Item, true);
		if (StaticUserData.IS_TPM_ENABLED)
		{
			int num = this._cache.CurFraction.OnPutPod(this._curRodPod);
			this._curRodPod.SetTpmId(num);
		}
		this._curRodPod.OnPut(true);
		this._curRodPod = null;
	}

	public bool IsOneMoreRodPodPresent()
	{
		return RodPodHelper.GetUnusedCount() > 0;
	}

	public RodPodController RodPodToPickUp
	{
		get
		{
			return this._rodPodToPickUp;
		}
	}

	public bool CanPickUpRodPod()
	{
		return this.IsRodPodVisible() && this._lookAtPod.CouldBeTaken && this._lookAtPod.IsFree && this._lookAtPodSlot == -1;
	}

	public bool InitPickUpRodPod()
	{
		if (this.CanPickUpRodPod())
		{
			this._rodPodToPickUp = this._lookAtPod;
			return true;
		}
		return false;
	}

	public void PickUpRodPod()
	{
		if (this._rodPodToPickUp != null)
		{
			this._curRodPod = this._rodPodToPickUp;
			PhotonConnectionFactory.Instance.PutRodStand(this._curRodPod.Item, false);
			if (StaticUserData.IS_TPM_ENABLED)
			{
				this._cache.CurFraction.TakePod(this._curRodPod.TpmId);
			}
			this._rodPods.Remove(this._rodPodToPickUp);
			this._rodPodToPickUp = null;
			this._curRodPod.OnPickUp();
		}
		else
		{
			LogHelper.Error("No rod to pickup", new object[0]);
		}
	}

	public float RodPodInteractionHeight
	{
		get
		{
			return this._rodPodInteractionHeight;
		}
	}

	public void CreatePutTargetForCurrentRod()
	{
		FABRIK ik = this.CurHand.IK;
		ik.solver.target = this._rodPodToPutRod.CreatePutRodTarget((byte)this._rodPodSlotToPutRod, this.Rod.DistFromRootToBackTip);
		ik.solver.IKPositionWeight = 0f;
		ik.enabled = !this.IsSailing;
		this._rodPodInteractionHeight = this._rodPodToPutRod.GetPlayerInteractionHeight((byte)this._rodPodSlotToPutRod);
	}

	public bool InitPutRodOnPod()
	{
		if (this._lookAtPod != null && !this._lookAtPod.IsSlotOccupied(this._lookAtPodSlot))
		{
			this._rodPodToPutRod = this._lookAtPod;
			this._rodPodSlotToPutRod = this._lookAtPodSlot;
			return true;
		}
		return false;
	}

	public void PutRodOnPod()
	{
		this.CurRodSessionSettings.IsOnPod = true;
		this.Rod.Controller.ResetLocators();
		this.Rod.SetOnPod(this._rodPodToPutRod, this._rodPodSlotToPutRod);
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this.RodSlot.Rod.RodOnPodTpmId = this._cache.CurFraction.PutRodOnPod();
		}
		this._rodPodToPutRod.OccupySlot(this.RodSlot, this._rodPodSlotToPutRod, this.RodSlot.Rod.Controller, this.RodSlot.Reel, this.RodSlot.Tackle, this.RodSlot.Line, this.RodSlot.Bell, null);
		GameFactory.FishSpawner.HideBobberIndicator();
		BottomFishingIndicator bottomFishingIndicator = GameFactory.Player.HudFishingHandler.BottomFishingIndicator;
		if (bottomFishingIndicator != null)
		{
			bottomFishingIndicator.Hide();
		}
		FeederFishingIndicator feederFishingIndicator = GameFactory.Player.HudFishingHandler.FeederFishingIndicator;
		if (feederFishingIndicator != null)
		{
			feederFishingIndicator.Hide();
		}
		this._isRodActive = false;
		this._rodPodToPutRod = null;
		if (this.RodSlot.Rod.Bell != null && this.RodSlot.Rod.IsFishHooked)
		{
			this.RodSlot.Rod.Bell.Voice(true);
		}
		this.SetRodSlot(GameFactory.RodSlots[0]);
	}

	public PlayerController.HandData CurHand
	{
		get
		{
			return this._handsData[(int)((byte)this._curHandType)];
		}
	}

	public PlayerController.HandData TakingHand
	{
		get
		{
			return this._handsData[(int)((byte)this._takingHandType)];
		}
	}

	public PlayerController.HandData LeftHand
	{
		get
		{
			return this._handsData[0];
		}
	}

	public PlayerController.HandData RightHand
	{
		get
		{
			return this._handsData[1];
		}
	}

	public RodController TakingRod
	{
		get
		{
			return (this._takingRodSlot == null) ? this.Rod.Controller : this._takingRodSlot.Value.Rod;
		}
	}

	public bool WasLastRodCasting
	{
		get
		{
			return this._wasLastRodCasting;
		}
	}

	public int RodSlotToTakeFromPod
	{
		get
		{
			return this._rodSlotToTakeFromPod;
		}
	}

	public int CurrentLookAtSlot
	{
		get
		{
			return this._lookAtPodSlot;
		}
	}

	public bool IsRodPodVisible()
	{
		return this._lookAtPod != null;
	}

	public bool CanPutRodOnPod()
	{
		return this.IsRodPodVisible() && !this._lookAtPod.IsSlotOccupied(this._lookAtPodSlot);
	}

	public bool CanTakeOrReplaceRodOnStand(bool isEmptyHands)
	{
		return this.IsRodPodVisible() && this._lookAtPod.IsSlotOccupied(this._lookAtPodSlot);
	}

	public bool InitTakeRodFromPod(bool isEmptyHands)
	{
		if (this._lookAtPod != null && this._lookAtPod.IsSlotOccupied(this._lookAtPodSlot))
		{
			if (this.CurrentBoat != null)
			{
				this.CurrentBoat.OnTakeRodFromPod();
			}
			this._rodSlotToTakeFromPod = this._lookAtPodSlot;
			this._rodPodToTakeRod = this._lookAtPod;
			this._takingRodSlot = new PodSlotData?(this._rodPodToTakeRod.LeaveSlot(this._rodSlotToTakeFromPod));
			if (!isEmptyHands)
			{
				this._rodPodToPutRod = this._lookAtPod;
				this._rodPodSlotToPutRod = this._lookAtPodSlot;
				this._wasLastRodCasting = this.ReelType == ReelTypes.Baitcasting;
				this._takingHandType = ((!this._wasLastRodCasting) ? PlayerController.Hand.Left : PlayerController.Hand.Right);
			}
			else
			{
				this._takingHandType = ((this.TakingRod.ReelType != ReelTypes.Baitcasting) ? PlayerController.Hand.Right : PlayerController.Hand.Left);
			}
			return true;
		}
		return false;
	}

	public bool IsPositionAdjusted
	{
		get
		{
			return this._isPositionAdjusted;
		}
	}

	public void AdjustPositionWithPod()
	{
		this._isPositionAdjusted = false;
		if (this._rodPodToTakeRod != null)
		{
			this._rodPodToTakeRod.AdjustPlayerPosition(this._rodSlotToTakeFromPod);
		}
		else if (this._rodPodToPutRod != null)
		{
			this._rodPodToPutRod.AdjustPlayerPosition(this._rodPodSlotToPutRod);
		}
	}

	public void OnPositionAdjusted()
	{
		this._isPositionAdjusted = true;
		if (this.IsSailing)
		{
			this.CurrentBoat.OnPlayerExternalControllReleased();
		}
	}

	public void PrepareHandForTakingRod(FABRIK handIk)
	{
		RodController takingRod = this.TakingRod;
		Transform transform = new GameObject("takeRodIkTarget").transform;
		transform.parent = this._rodPodToTakeRod.transform;
		transform.rotation = takingRod.transform.rotation;
		transform.position = takingRod.transform.position;
		handIk.solver.target = transform;
		handIk.solver.IKPositionWeight = 0f;
		handIk.enabled = !this.IsSailing;
		this._rodPodInteractionHeight = this._rodPodToTakeRod.GetPlayerInteractionHeight((byte)this._rodSlotToTakeFromPod);
	}

	public void SetRodSlot(GameFactory.RodSlot newRodSlot)
	{
		if (this.RodSlot != null)
		{
			Debug.LogWarning(string.Format("PlayerController.SetRodSlot #{0} ({1}) -> #{2} ({3})", new object[]
			{
				this.RodSlot.Index,
				this.RodSlot.Tackle,
				newRodSlot.Index,
				newRodSlot.Tackle
			}));
		}
		else
		{
			Debug.LogWarning(string.Format("PlayerController.SetRodSlot null -> #{0} ({1})", newRodSlot.Index, newRodSlot.Tackle));
		}
		this.RodSlot = newRodSlot;
		this.refreshRodSlotReferences();
	}

	private void restoreTackleIndicators()
	{
		if (this.Tackle.Fish != null || this.Tackle.IsFishHooked)
		{
			return;
		}
		if (this.Tackle is Float1stBehaviour && this.Tackle.State == typeof(FloatFloating))
		{
			GameFactory.FishSpawner.ShowBobberIndicator(this.Tackle.transform);
		}
		Feeder1stBehaviour feeder1stBehaviour = this.Tackle as Feeder1stBehaviour;
		if (feeder1stBehaviour != null && feeder1stBehaviour.State == typeof(FeederFloating) && feeder1stBehaviour.RigidBody.GroundHeight < 0f && this.Rod.TackleTipMass.GroundHeight < 0f)
		{
			if (this.Rod.IsQuiver && GameFactory.QuiverIndicator != null && SettingsManager.FishingIndicator)
			{
				GameFactory.QuiverIndicator.HighSensitivity(false);
				GameFactory.QuiverIndicator.Show();
			}
			if (GameFactory.BottomIndicator != null && SettingsManager.FishingIndicator && this.Rod.AssembledRod.RodTemplate != RodTemplate.Spod)
			{
				GameFactory.BottomIndicator.Show();
			}
		}
	}

	public void refreshRodSlotReferences()
	{
		this.Rod = this.RodSlot.Rod as Rod1stBehaviour;
		this.Line = this.RodSlot.Line as Line1stBehaviour;
		this.Reel = this.RodSlot.Reel as Reel1stBehaviour;
		this.Bell = this.RodSlot.Bell as Bell1stBehaviour;
		this.Tackle = this.RodSlot.Tackle;
	}

	public void MakePuttingRodRealRequest()
	{
		if (this.Rod == null || this.Rod.AssembledRod == null || this.Rod.AssembledRod.Rod == null)
		{
			return;
		}
		Rod rod = this.Rod.AssembledRod.Rod;
		PhotonConnectionFactory.Instance.PutRodOnStand(rod.Slot, true);
	}

	public void MakeTakingRodRealRequest()
	{
		RodController takingRod = this.TakingRod;
		RodBehaviour behaviour = takingRod.Behaviour;
		AssembledRod assembledRod = behaviour.RodAssembly as AssembledRod;
		takingRod.ResetLocators();
		(behaviour as Rod1stBehaviour).SetOnPod(null, 0);
		PodSlotData value = this._takingRodSlot.Value;
		Rod rod = RodHelper.FindRodInHands();
		if (rod != null && this._isTackleThrown)
		{
			PhotonConnectionFactory.Instance.PutRodOnStand(rod.Slot, true);
		}
		Rod rod2 = RodHelper.FindRodInSlot(behaviour.RodSlot.Index, null);
		RodHelper.MoveRodToHands(rod2, true, false);
		this.SetRodSlot(behaviour.RodSlot);
		StaticUserData.RodInHand = assembledRod;
		this.CalcRodAngles();
		this.ChangeRodHandler(assembledRod);
		this.SwitchRodInitialized(assembledRod);
		this.restoreTackleIndicators();
		PhotonConnectionFactory.Instance.PutRodOnStand(rod2.Slot, false);
	}

	public void OnTakingRodReadyToBeInstantiated()
	{
		this.OnTakeRodFromPod(this._takingRodSlot.Value.Rod.Behaviour.RodOnPodTpmId);
		if (this.ReelType == ReelTypes.Baitcasting)
		{
			this.SetRodRootLeft();
		}
		else
		{
			this.SetRodRootRight();
		}
		this._takingRodSlot = null;
		this._rodPodToTakeRod = null;
		this._rodSlotToTakeFromPod = -1;
		this._isTackleThrown = true;
		this._reelIkController.enabled = true;
		this.IsEmptyHandsMode = false;
	}

	public void OnTakeRodFromPod(int tpmId)
	{
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.OnTakeRodFromPod(tpmId);
		}
	}

	public void CreateChumBall()
	{
		GameFactory.Player.HudFishingHandler.LineHandler.LineInWater = 0;
		GameFactory.Player.HudFishingHandler.LineHandler.LineLength = (int)MeasuringSystemManager.LineLength(Inventory.ChumHandMaxCastLength);
		this.throwData = new TackleThrowData();
		Chum chum = FeederHelper.FindPreparedChumInHand();
		this._curChumBall = Object.Instantiate<ChumBall>((chum.Ingredients[0].SpecialItem != InventorySpecialItem.Snow) ? this._pChumBall : this._pSnowBall);
		this._curChumBall.transform.parent = this.RightHand.RodBone;
		this._curChumBall.transform.localRotation = Quaternion.identity;
		this._curChumBall.transform.localPosition = Vector3.zero;
		this._curChumBall.SetupTexture(chum.HeaviestChumBase.Color);
	}

	public void DestroyChumBall()
	{
		if (this._curChumBall != null)
		{
			Object.Destroy(this._curChumBall.gameObject);
		}
	}

	public void ThrowChumByHand()
	{
		if (this._curChumBall != null)
		{
			Chum chum = FeederHelper.FindPreparedChumInHand();
			PhotonConnectionFactory.Instance.OnThrowChumDone += this.Instance_OnThrowChumDone;
			PhotonConnectionFactory.Instance.OnThrowChumFailed += this.Instance_OnThrowChumFailed;
			PhotonConnectionFactory.Instance.ThrowChum(new Chum[] { chum });
			this.Update3dCharMecanimParameter(TPMMecanimIParameter.SubState, (byte)this.ThrowData.CastLength);
			this._curChumBall.Launch(this._chumLaunchDelay, this.ThrowData.CastLength, base.transform.forward, chum, new Func<Vector3, Vector3, Vector3?>(this.CheckSnowballIntersection));
			this._curChumBall = null;
		}
	}

	private Vector3? CheckSnowballIntersection(Vector3 from, Vector3 to)
	{
		TPMCharactersController.HitResult hitPoint = this._tpmCharacters.GetHitPoint(from, to);
		if (hitPoint.HasValue)
		{
			PhotonConnectionFactory.Instance.FlagSnowballHit(hitPoint.UserId);
			return new Vector3?(hitPoint.Pos);
		}
		return null;
	}

	private void Instance_OnThrowChumDone(Chum[] chums)
	{
		PhotonConnectionFactory.Instance.OnThrowChumDone -= this.Instance_OnThrowChumDone;
		PhotonConnectionFactory.Instance.OnThrowChumFailed -= this.Instance_OnThrowChumFailed;
	}

	private void Instance_OnThrowChumFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnThrowChumDone -= this.Instance_OnThrowChumDone;
		PhotonConnectionFactory.Instance.OnThrowChumFailed -= this.Instance_OnThrowChumFailed;
	}

	public Chum BeginHandLoading()
	{
		Chum chum = FeederHelper.FindPreparedChumInHand();
		if (!chum.BeginFillRequested)
		{
			PhotonConnectionFactory.Instance.BeginFillChum(new Chum[] { chum });
		}
		chum.BeginFillRequested = true;
		return chum;
	}

	public Chum CancelHandLoading()
	{
		Chum chum = FeederHelper.FindPreparedChumInHand();
		PhotonConnectionFactory.Instance.OnCancelFillChumDone += this.Instance_OnCancelFillChumDone;
		PhotonConnectionFactory.Instance.OnCancelFillChumFailed += this.Instance_OnCancelFillChumFailed;
		PhotonConnectionFactory.Instance.CancelFillChum(new Chum[] { chum });
		chum.CancelRequested = true;
		return chum;
	}

	public Chum FinishHandLoading()
	{
		Chum chum = FeederHelper.FindPreparedChumInHand();
		if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanFinishFillChum(chum))
		{
			Debug.LogError("Can't finish Chum Hand loading: " + PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError);
			return null;
		}
		PhotonConnectionFactory.Instance.FinishFillChum(new Chum[] { chum });
		chum.FinishFillRequested = true;
		return chum;
	}

	public Chum[] BeginFeederLoading()
	{
		Chum[] array = (from chum in FeederHelper.FindPreparedChumActiveRodAll()
			where !chum.HasWeight || !chum.WasFilled
			select chum).ToArray<Chum>();
		PhotonConnectionFactory.Instance.BeginFillChum(array);
		foreach (Chum chum2 in array)
		{
			chum2.BeginFillRequested = true;
		}
		return array;
	}

	public Chum[] CancelFeederLoading()
	{
		Chum[] array = FeederHelper.FindPreparedChumActiveRodAll();
		PhotonConnectionFactory.Instance.CancelFillChum(array);
		PhotonConnectionFactory.Instance.OnCancelFillChumDone += this.Instance_OnCancelFillChumDone;
		PhotonConnectionFactory.Instance.OnCancelFillChumFailed += this.Instance_OnCancelFillChumFailed;
		foreach (Chum chum in array)
		{
			chum.CancelRequested = true;
		}
		return array;
	}

	private void Instance_OnCancelFillChumDone(Chum[] chums)
	{
		foreach (Chum chum in chums)
		{
			chum.BeginFillRequested = false;
			chum.FinishFillRequested = false;
			chum.CancelRequested = false;
		}
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Hands && x.ItemType == ItemTypes.Rod);
		if (inventoryItem != null)
		{
			StaticUserData.RodInHand.RefreshRod(inventoryItem.InstanceId);
		}
		PhotonConnectionFactory.Instance.OnCancelFillChumDone -= this.Instance_OnCancelFillChumDone;
		PhotonConnectionFactory.Instance.OnCancelFillChumFailed -= this.Instance_OnCancelFillChumFailed;
	}

	private void Instance_OnCancelFillChumFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnCancelFillChumDone -= this.Instance_OnCancelFillChumDone;
		PhotonConnectionFactory.Instance.OnCancelFillChumFailed -= this.Instance_OnCancelFillChumFailed;
	}

	public Chum[] FinishFeederLoading()
	{
		Chum[] array = (from chum in FeederHelper.FindPreparedChumActiveRodAll()
			where !chum.HasWeight || !chum.WasFilled
			select chum).ToArray<Chum>();
		foreach (Chum chum3 in array)
		{
			if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanFinishFillChum(chum3))
			{
				Debug.LogError("Can't finish Chum loading: " + PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError);
				return null;
			}
		}
		this.Feeder.SetFilled(true);
		PhotonConnectionFactory.Instance.FinishFillChum(array);
		PhotonConnectionFactory.Instance.OnFinishFillChumDone += this.Instance_OnFinishFillChumDone;
		PhotonConnectionFactory.Instance.OnFinishFillChumFailed += this.Instance_OnFinishFillChumFailed;
		foreach (Chum chum2 in array)
		{
			chum2.FinishFillRequested = true;
		}
		return array;
	}

	private void Instance_OnFinishFillChumDone(Chum[] newChum)
	{
		Guid? parentItemInstanceId = newChum.First<Chum>().ParentItemInstanceId;
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Hands && x.ItemType == ItemTypes.Rod);
		if (inventoryItem != null && inventoryItem.InstanceId == parentItemInstanceId)
		{
			StaticUserData.RodInHand.RefreshRod(inventoryItem.InstanceId);
		}
		PhotonConnectionFactory.Instance.OnFinishFillChumDone -= this.Instance_OnFinishFillChumDone;
		PhotonConnectionFactory.Instance.OnFinishFillChumFailed -= this.Instance_OnFinishFillChumFailed;
	}

	private void Instance_OnFinishFillChumFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnFinishFillChumDone -= this.Instance_OnFinishFillChumDone;
		PhotonConnectionFactory.Instance.OnFinishFillChumFailed -= this.Instance_OnFinishFillChumFailed;
	}

	public void SetCameraFollowMode(bool flag)
	{
		if (flag)
		{
			this.CameraController.CameraMouseLook.EnableFolowBoneController(this.Root);
			this.CameraController.enabled = false;
		}
		else
		{
			this.CameraController.CameraMouseLook.DisableFollowBoneController();
			this.CameraController.enabled = true;
		}
	}

	private void PatchAnimations(Animation anims)
	{
		if (this._pPatchAnimationsList != null)
		{
			AnimClips component = this._pPatchAnimationsList.GetComponent<AnimClips>();
			for (int i = 0; i < component.HandsClips.Length; i++)
			{
				AnimationClip animationClip = component.HandsClips[i];
				anims.AddClip(animationClip, animationClip.name);
			}
		}
	}

	public void OnEmpty(bool wasMoving)
	{
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsFireworkMode, false);
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsInGame, false);
		this._isRodActive = false;
		this.ShowSleeves = false;
		this.PlayAnimation((!wasMoving) ? "empty" : "Walk", 1f, 1f, 0f);
		this.StopThrowState();
	}

	public void OnHandMode(bool flag)
	{
		ShowHudElements.Instance.ShowingFishingHUD.SetActive(flag);
		this.ShowSleeves = !flag;
	}

	public void OnDrawIn()
	{
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsInGame, true);
		ShowHudElements.Instance.ShowingFishingHUD.SetActive(true);
		this._isRodActive = true;
		this.ReelAnimation = this.Reel.Animation;
		this.Tackle.SetVisibility(true);
		this.ActivateRod(true);
		this.ShowSleeves = false;
		this.InitGrip();
	}

	public void OnEnterIdle()
	{
		this.RodSlot.RestoreReelClip();
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsThrowFinished, false);
		ShowHudElements.Instance.ShowingFishingHUD.SetActive(true);
		ShowHudElements.Instance.LureInfoHandler.Refresh();
		this.StopThrowState();
		this.ShowSleeves = false;
	}

	public void OnDrawOut()
	{
		ShowHudElements.Instance.ShowingFishingHUD.SetActive(false);
		this.ReelAnimation = null;
		this.ShowSleeves = false;
		if (this.Rod != null)
		{
			this.Rod.InvalidateSimulation();
		}
	}

	public void OnToolDrawIn()
	{
		this.Update3dCharMecanimParameter(TPMMecanimBParameter.IsFireworkMode, true);
		this.Update3dCharMecanimParameter(TPMMecanimIParameter.SubState, 0);
		this.CreateFirework(this.RequestedFireworkItem);
	}

	public void OnStartEngineIgnition()
	{
		this.HudFishingHandler.State = HudState.EngineIgnition;
		ShowHudElements.Instance.ShowingFishingHUD.SetActive(true);
		ShowHudElements.Instance.LureInfoHandler.gameObject.SetActive(false);
	}

	public void OnFinishEngineIgnition()
	{
		this.HudFishingHandler.State = HudState.CastSimple;
		ShowHudElements.Instance.ShowingFishingHUD.SetActive(false);
		ShowHudElements.Instance.LureInfoHandler.gameObject.SetActive(true);
	}

	private void RetargetSleevesSkeleton(SkinnedMeshRenderer renderer)
	{
		Object.Destroy(renderer.transform.parent.GetComponent<Animation>());
		renderer.updateWhenOffscreen = true;
		TransformHelper.CopyBoneTransforms(renderer, this._srcBonesMap, this._bonesCache);
		Object.Destroy(renderer.rootBone.gameObject);
	}

	public bool WasFeederReadyForStrike
	{
		get
		{
			return ControlsController.ControlsActions.Fire2.IsPressed;
		}
	}

	private void OnUpdateFBAZones(HashSet<ushort> activeZones)
	{
		for (int i = 0; i < this._fishBoxActivityZones.Count; i++)
		{
			this._fishBoxActivityZones[i].SetServerVisibility(activeZones.Contains(this._fishBoxActivityZones[i].ServerId));
		}
	}

	private LimbIK AddLimbIk(GameObject go)
	{
		LimbIK limbIK = go.GetComponent<LimbIK>();
		if (limbIK == null)
		{
			limbIK = go.AddComponent<LimbIK>();
		}
		limbIK.solver.IKPositionWeight = 0f;
		limbIK.solver.IKRotationWeight = 0f;
		if (this.Reel != null)
		{
			this.SetupLimbIk(limbIK, null);
		}
		return limbIK;
	}

	private void SetupLimbIk(LimbIK ik, Transform reelAim = null)
	{
		ik.solver.target = reelAim ?? TransformHelper.FindDeepChild(this.Reel.gameObject.transform, "handle");
		Transform transform = ik.transform;
		if (this.ReelType == ReelTypes.Spinning)
		{
			ik.solver.SetChain(TransformHelper.FindDeepChild(transform.transform, "L_sholder"), TransformHelper.FindDeepChild(transform.transform, "L_arm"), TransformHelper.FindDeepChild(transform.transform, "L_thumb4"), TransformHelper.FindDeepChild(transform.transform, "root"));
			ik.solver.bendModifier = 2;
			ik.solver.goal = 2;
		}
		else
		{
			ik.solver.SetChain(TransformHelper.FindDeepChild(transform.transform, "R_sholder"), TransformHelper.FindDeepChild(transform.transform, "R_arm"), TransformHelper.FindDeepChild(transform.transform, "R_thumb4"), TransformHelper.FindDeepChild(transform.transform, "root"));
			ik.solver.bendModifier = 2;
			ik.solver.goal = 3;
		}
		ik.solver.IKPositionWeight = 0f;
		ik.solver.IKRotationWeight = 0f;
		ik.solver.bendModifierWeight = 1f;
		ik.enabled = false;
	}

	public void UpdateReelIkController()
	{
		Transform transform = TransformHelper.FindDeepChild(this.Reel.gameObject.transform, (this.ReelType != ReelTypes.Spinning) ? "aim" : "handle");
		this.SetupLimbIk(this._reelIkController, transform);
	}

	internal void Awake()
	{
		List<GameObject> list = TransformHelper.FindObjectsByPath("SKY/LIGHTS/Directional light");
		if (list.Count > 0)
		{
			SunShafts component = this.CameraController.Camera.GetComponent<SunShafts>();
			if (component != null)
			{
				component.sunTransform = list[0].transform;
			}
		}
		ClientDebugHelper.LogIfEnabled(ProfileFlag.GameLogic, "Player::Awake()");
		GameFactory.SetupPlayer(this);
		if (GameFactory.BoatDock != null)
		{
			if (GameFactory.BoatDock.IsRefreshRequest)
			{
				this.RecreateAllBoats();
			}
			GameFactory.BoatDock.OnPlayerReady();
		}
		this._photoModeLight = this.CameraController.Camera.GetComponent<Light>();
		this._flashLight = this._photoModeLight.transform.Find("light").GetComponent<FlashLightController>();
		this._shadow = this.Collider.GetChild(1).GetComponent<HandsShadowController>();
		this._targetCloser = base.GetComponent<PlayerTargetCloser>();
		GameObject gameObject = GameObject.Find("fishActiveZones");
		if (gameObject != null)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				FishBoxActivity component2 = gameObject.transform.GetChild(i).GetComponent<FishBoxActivity>();
				if (component2 != null && component2.ServerId != 0)
				{
					this._fishBoxActivityZones.Add(component2);
				}
			}
		}
		this._ikCurves = base.GetComponent<AnimationCurves>();
		this._reelIkController = this.AddLimbIk(this.Hands);
		this._handsData[0] = new PlayerController.HandData(this.RodLeftRoot.transform, this._leftIK);
		this._handsData[1] = new PlayerController.HandData(this.RodRoot.transform, this._rightIK);
		this._bonesCache = new Transform[this._handsRenderer.bones.Length];
		this._srcBonesMap = TransformHelper.BuildSrcBonesMap(this._handsRenderer);
		this.ZoomCamera(false);
		this._boatRentController = new BoatRentController();
		Object @object = Resources.Load("Equipment/Grip01/pGrip_Type01");
		GameObject gameObject2 = Object.Instantiate(@object) as GameObject;
		if (gameObject2 != null)
		{
			this.Grip = gameObject2.GetComponent<GripSettings>();
			this.Grip.SetPlayerVisibility(true);
		}
		else
		{
			LogHelper.Error("Grip object Equipment/Grip01/pGrip_Type01 is not found", new object[0]);
		}
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache = new TPMSenderDataCache();
		}
		Transform collider = this.Collider;
		this._motor = collider.GetComponent<CharacterMotorCS>();
		this._fpController = collider.GetComponent<FirstPersonControllerFP>();
		this._fpController.SetControllable(true);
		this.SetRodSlot(GameFactory.RodSlots[0]);
		GameFactory.Message = new MenuHelpers().MenuPrefabsList.gameObject.GetComponent<MessageController>();
		this._sleevesData = this.Sleeves.GetComponent<SleevesHelper>();
		SkinnedMeshRenderer componentInChildren = this._sleevesData.GetComponentInChildren<SkinnedMeshRenderer>();
		this._sleevesData.sleevesMaterial = componentInChildren.material;
		this.RetargetSleevesSkeleton(componentInChildren);
		this.ArmsAnimation = this.Arms.GetComponent<Animation>();
		PlayerController.SmoothAnimationWeight.ArmsAnimation = this.ArmsAnimation;
		this._pPatchAnimationsList = Resources.Load("pPatchAnimationsList") as GameObject;
		this.PatchAnimations(this.ArmsAnimation);
		this.ThrowSounds = new RandomSounds("Sounds/Actions/Throw/", this);
		this.fsm = new Fsm<PlayerController>("PlayerFSM", this, true);
		this.fsm.RegisterState<PlayerEmpty>();
		this.fsm.RegisterState<PlayerDrawIn>();
		this.fsm.RegisterState<PlayerIdle>();
		this.fsm.RegisterState<PlayerIdlePitch>();
		this.fsm.RegisterState<PlayerIdleThrown>();
		this.fsm.RegisterState<PlayerRoll>();
		this.fsm.RegisterState<PlayerDrawOut>();
		this.fsm.RegisterState<PlayerSimpleThrow>();
		this.fsm.RegisterState<PlayerIdleTarget>();
		this.fsm.RegisterState<PlayerThrowPitch>();
		this.fsm.RegisterState<PlayerThrowInOneHand>();
		this.fsm.RegisterState<PlayerThrowInOneHandRight>();
		this.fsm.RegisterState<PlayerThrowInOneHandLeft>();
		this.fsm.RegisterState<PlayerThrowInTwoHands>();
		this.fsm.RegisterState<PlayerThrowInTwoHandsOvercast>();
		this.fsm.RegisterState<PlayerThrowOutOneHand>();
		this.fsm.RegisterState<PlayerThrowOutOneHandRight>();
		this.fsm.RegisterState<PlayerThrowOutOneHandLeft>();
		this.fsm.RegisterState<PlayerThrowOutTwoHands>();
		this.fsm.RegisterState<PlayerThrowOutWait>();
		this.fsm.RegisterState<PlayerThrowToIdleThrown>();
		this.fsm.RegisterState<PlayerShowFishIn>();
		this.fsm.RegisterState<PlayerShowFishIdle>();
		this.fsm.RegisterState<PlayerShowFishOut>();
		this.fsm.RegisterState<PlayerShowFishLineIn>();
		this.fsm.RegisterState<PlayerShowFishLineIdle>();
		this.fsm.RegisterState<PlayerShowFishLineOut>();
		this.fsm.RegisterState<PlayerIdleThrownToIdle>();
		this.fsm.RegisterState<PlayerIdleThrownToIdlePitch>();
		this.fsm.RegisterState<PlayerIdleToIdlePitch>();
		this.fsm.RegisterState<PlayerIdlePitchToIdle>();
		this.fsm.RegisterState<PlayerPitchToIdleThrown>();
		this.fsm.RegisterState<ToolDrawIn>();
		this.fsm.RegisterState<ToolDrawOut>();
		this.fsm.RegisterState<ToolIdle>();
		this.fsm.RegisterState<ToolSetup>();
		this.fsm.RegisterState<PlayerOnBoat>();
		this.fsm.RegisterState<PlayerPhotoMode>();
		this.fsm.RegisterState<ShowMap>();
		this.fsm.RegisterState<RodPodIn>();
		this.fsm.RegisterState<RodPodIdle>();
		this.fsm.RegisterState<RodPodOut>();
		this.fsm.RegisterState<PutRodPodIn>();
		this.fsm.RegisterState<PutRodPodOut>();
		this.fsm.RegisterState<PickUpRodPodIn>();
		this.fsm.RegisterState<PickUpRodPodOut>();
		this.fsm.RegisterState<PutRodOnPodIn>();
		this.fsm.RegisterState<PutRodOnPodOut>();
		this.fsm.RegisterState<TakeRodFromPodIn>();
		this.fsm.RegisterState<TakeRodFromPodOut>();
		this.fsm.RegisterState<ReplaceRodOnPodLean>();
		this.fsm.RegisterState<ReplaceRodOnPodTakeAndPut>();
		this.fsm.RegisterState<ReplaceRodOnPodOut>();
		this.fsm.RegisterState<FeederLoadingIdle>();
		this.fsm.RegisterState<FeederSkipLoading>();
		this.fsm.RegisterState<FeederLoadingOut>();
		this.fsm.RegisterState<DebugDelay>();
		this.fsm.RegisterState<HandDrawIn>();
		this.fsm.RegisterState<HandIdle>();
		this.fsm.RegisterState<HandIdleToLoading>();
		this.fsm.RegisterState<HandDrawOut>();
		this.fsm.RegisterState<HandThrowIn>();
		this.fsm.RegisterState<HandThrowOut>();
		this.fsm.RegisterState<HandTrowToLoading>();
		this.fsm.RegisterState<HandLoadingIdle>();
		this.fsm.RegisterState<HandSkipLoading>();
		this.fsm.RegisterState<HandLoadingOut>();
		this.fsm.RegisterState<PlayerCameraRouting>();
		this.fsm.Start<PlayerEmpty>();
		if (base.transform.parent != null)
		{
			this.lastRotation = base.transform.parent.rotation;
		}
		this.pullValue = new ValueGainTrigger(0f, 1f, 10f)
		{
			isBack = true,
			Damping = 250f,
			Stiffness = 1500f
		};
		this.isHookedValue = new ValueGainTrigger(0f, 1f, 25f);
		this.isHookedValue.Damping = 300f;
		this.strikeValue = new ValueChanger(0f, 1f, 10f, new float?(3f));
		base.transform.localPosition = new Vector3(0f, -this.localHandsOffset, 0f);
		this._rodLocator = this.RodRoot;
		if (this._rodLocator == null)
		{
			this._rodLocator = GameObject.Find("FakeRod");
		}
		GameObject gameObject3 = GameObject.Find("CatchingWalls");
		if (gameObject3 != null)
		{
			gameObject3.SetActive(false);
		}
		this._catchingWalls = gameObject3;
		this._weather.Activate();
		this.lastTimeSendPlayerPositionAndRotation = Time.realtimeSinceStartup;
		this.InitiateRod();
	}

	private void InitiateRod()
	{
		StaticUserData.RodInHand.ClearRod();
		Rod rod = RodHelper.FindRodInHands();
		if (rod == null || !RodHelper.IsRodEquipped(rod))
		{
			rod = RodHelper.FindFirstEquippedRod();
			if (rod != null)
			{
				RodHelper.MoveRodToHands(rod, true, false);
			}
		}
	}

	private void OnEnable()
	{
		if (this._finishUnboardingRequest)
		{
			this.OnMoveFromBoatFinished();
		}
		this.RefreshSleeves();
		ControlsController.ControlsActions.UnBlockInput();
		if (this.CurrentBoat != null)
		{
			this.CurrentBoat.OnPlayerEnabled();
			this.CurrentBoat.DisableSound(false);
		}
		if (this.IsSailing && (!this.IsBoatFishing || !this.IsTrollingPossible))
		{
			this.RequestedRod = null;
			return;
		}
		if (this.CurFirework != null)
		{
			return;
		}
		this.RequestPendingRod();
	}

	private void RequestPendingRod()
	{
		Rod rod = ((PhotonConnectionFactory.Instance.PendingGameSlotIndex <= 0) ? null : RodHelper.FindRodInSlot(PhotonConnectionFactory.Instance.PendingGameSlotIndex, null));
		if (RodHelper.IsInventorySlotOccupiedByRodStand(PhotonConnectionFactory.Instance.PendingGameSlotIndex))
		{
			return;
		}
		if ((this.RequestedRod == null || this.RequestedRod == rod) && (rod == null || !RodHelper.IsRodEquipped(rod)))
		{
			GameFactory.Message.ShowNoAssembledRods();
			this._isRequestedRodDestroying = true;
		}
		else
		{
			if (this.RequestedRod != null)
			{
				return;
			}
			if (StaticUserData.RodInHand.Rod != rod)
			{
				this.SaveTakeRodRequest(rod, false);
			}
			else
			{
				AssembledRod assembledRod = new AssembledRod(rod.InstanceId);
				if (!StaticUserData.RodInHand.Equals(assembledRod))
				{
					this.SaveTakeRodRequest(rod, false);
				}
			}
		}
	}

	public int GetRequestedSlotId()
	{
		int num;
		if (ControlsController.ControlsActions.RodPanel.IsPressed)
		{
			num = Array.FindIndex<CustomPlayerAction>(ControlsController.ControlsActions.RodStandSlots, (CustomPlayerAction r) => r.WasReleased);
		}
		else
		{
			num = Array.FindIndex<CustomPlayerAction>(ControlsController.ControlsActions.Rods, (CustomPlayerAction r) => r.WasPressed);
		}
		int num2 = num;
		return (num2 == -1) ? (-1) : (num2 + 1);
	}

	public bool FastUseRodPod
	{
		get
		{
			return this._fastUseRodPodSlotId != -1;
		}
	}

	private void SwitchingRodHandler()
	{
		if (this.IsSailing && this.CurrentBoat.IsInTransitionState)
		{
			return;
		}
		int requestedSlotId = this.GetRequestedSlotId();
		if (requestedSlotId != -1)
		{
			this.WasFirstZoneEntered = true;
		}
		bool flag = this.IsReadyForRod;
		if (!this.IsSailing)
		{
			flag = (flag | ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier.IsPressed) || ControlsController.ControlsActions.RodPanel.IsPressed;
		}
		if (flag && !GameFactory.IsRodAssembling && requestedSlotId != -1)
		{
			if (!ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier.IsPressed && !ControlsController.ControlsActions.RodPanel.IsPressed)
			{
				this.TryToTakeRodFromSlot(requestedSlotId, true);
			}
			else if (this._selectRodReadyStates.Contains(this.fsm.CurrentStateType))
			{
				this.TryToTakeRodFromPodSlot(requestedSlotId - 1);
			}
			else if (this._fastUseRodPodStates.Contains(this.fsm.CurrentStateType))
			{
				this._fastUseRodPodSlotId = requestedSlotId;
			}
		}
	}

	public void RequestUseRodPod()
	{
		this.TryToTakeRodFromPodSlot(this._fastUseRodPodSlotId - 1);
		this._fastUseRodPodSlotId = -1;
	}

	public void TryToTakeRodFromPodSlot(int slotId)
	{
		RodPodController rodPodByPodSlotId = RodPodHelper.GetRodPodByPodSlotId(slotId);
		if (rodPodByPodSlotId != null && rodPodByPodSlotId.IsSlotOccupied(slotId))
		{
			this._lookAtPod = rodPodByPodSlotId;
			this._lookAtPodSlot = slotId;
			this.InitTakeRodFromPod(true);
		}
	}

	public bool IsEmptyHandsMode { get; set; }

	public bool IsHandThrowMode { get; set; }

	public bool TryToPutRodOnPod(int slotId)
	{
		slotId--;
		RodPodController rodPodByPodSlotId = RodPodHelper.GetRodPodByPodSlotId(slotId);
		if (rodPodByPodSlotId != null && !rodPodByPodSlotId.IsSlotOccupied(slotId))
		{
			this._rodPodToPutRod = rodPodByPodSlotId;
			this._rodPodSlotToPutRod = slotId;
			return true;
		}
		return false;
	}

	public bool TryToReplaceRod(int slotId)
	{
		if (ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier.IsPressed || ControlsController.ControlsActions.RodPanel.IsPressed)
		{
			slotId--;
			RodPodController rodPodByPodSlotId = RodPodHelper.GetRodPodByPodSlotId(slotId);
			if (rodPodByPodSlotId != null && rodPodByPodSlotId.IsSlotOccupied(slotId))
			{
				this._lookAtPod = rodPodByPodSlotId;
				this._lookAtPodSlot = slotId;
				this.InitTakeRodFromPod(false);
				return true;
			}
		}
		else
		{
			RodPodController rodPodByRodSlotId = RodHelper.GetRodPodByRodSlotId(slotId);
			if (rodPodByRodSlotId != null)
			{
				if ((rodPodByRodSlotId.transform.position - this.Position).magnitude < 5f)
				{
					this._lookAtPod = rodPodByRodSlotId;
					this._lookAtPodSlot = this._lookAtPod.FindSlotByRodSlot(slotId);
					this.InitTakeRodFromPod(false);
					return true;
				}
			}
		}
		return false;
	}

	public void TryToTakeRodFromSlot(int slotId, bool takeFromPod = true)
	{
		Rod rod = RodHelper.FindRodInSlot(slotId, null);
		if (rod != null && (!this.IsSailing || !this.IsReadyForRod))
		{
			AssembledRod assembledRod = new AssembledRod(rod.InstanceId);
			if (assembledRod == null)
			{
				return;
			}
			bool flag = StaticUserData.RodInHand.Equals(assembledRod) && StaticUserData.RodInHand.Slot == slotId;
			if ((!this._selectRodReadyStates.Contains(this.fsm.CurrentStateType) && !this.IsBoatFishing) || (flag && !this.IsHandThrowMode && !this.IsWithRodPodMode && !this.IsEmptyHandsMode && this.fsm.CurrentStateType != typeof(PlayerEmpty) && this.CurFirework == null))
			{
				return;
			}
		}
		if (!RodHelper.IsInventorySlotOccupiedByRodStand(slotId))
		{
			if (rod == null)
			{
				GameFactory.Message.ShowHaventRodInSlot(slotId.ToString(CultureInfo.InvariantCulture));
				return;
			}
			if (!RodHelper.IsRodEquipped(rod))
			{
				PlayerController.InformRodUnequipped(slotId, rod);
				return;
			}
			this.SaveTakeRodRequest(rod, false);
		}
		else if (takeFromPod)
		{
			RodPodController rodPodByRodSlotId = RodHelper.GetRodPodByRodSlotId(slotId);
			if (!(rodPodByRodSlotId != null))
			{
				return;
			}
			if ((rodPodByRodSlotId.transform.position - this.Position).magnitude >= 5f)
			{
				return;
			}
			this._lookAtPod = rodPodByRodSlotId;
			this._lookAtPodSlot = this._lookAtPod.FindSlotByRodSlot(slotId);
			this.InitTakeRodFromPod(true);
		}
		this.IsEmptyHandsMode = false;
		this.IsWithRodPodMode = false;
	}

	public void SaveTakeRodRequest(Rod rod, bool fastChange = false)
	{
		Debug.LogWarning(string.Format("SaveTakeRodRequest #{0}", rod.Slot));
		this.RequestedRod = rod;
		this.NewRodFastChange = fastChange;
	}

	public void CreateRequestedRod()
	{
		Rod requestedRod = this.RequestedRod;
		bool newRodFastChange = this.NewRodFastChange;
		RodHelper.MoveRodToHands(requestedRod, true, newRodFastChange);
		StaticUserData.RodInHand = new AssembledRod(this.RequestedRod.InstanceId);
		this.RequestedRod = null;
		this.NewRodFastChange = false;
		this.ReelType = StaticUserData.RodInHand.ReelType;
		RodInitialize.InitRod(StaticUserData.RodInHand, this, delegate(AssembledRod rodAssembly)
		{
			this.refreshRodSlotReferences();
			this.SetRodRootRight();
			this.CalcRodAngles();
			this.ChangeRodHandler(rodAssembly);
		}, null);
		this.SwitchRodInitialized(StaticUserData.RodInHand);
		this.InitGrip();
	}

	public void DestroyRod(bool clearRodInHands = true)
	{
		this._isRequestedRodDestroying = false;
		RodInitialize.DestroyRod(this);
		if (clearRodInHands)
		{
			StaticUserData.RodInHand.ClearRod();
		}
		this.SetRodSlot(GameFactory.RodSlots[0]);
	}

	public void ReplaceBaitInCurrentRod()
	{
		this.SaveTakeRodRequest(StaticUserData.RodInHand.Rod, false);
	}

	private void InstanceOnOnCharacterUpdated(CharacterInfo character)
	{
		if (this._tpmCharacters != null)
		{
			this._tpmCharacters.OnUpdate(character.Id, character.Package, this.fsm.CurrentStateType == typeof(PlayerPhotoMode));
		}
	}

	private void InstanceOnOnCharacterDestroyed(Player player)
	{
		if (this._tpmCharacters != null)
		{
			this._tpmCharacters.OnPlayerModelLeave(player.UserId);
		}
	}

	public bool IsAnyPlayerCloseEnough(Vector3 pos, float distance, bool ignorePlayer = false)
	{
		return (!ignorePlayer && (base.transform.position - pos).magnitude < distance) || (this._tpmCharacters != null && this._tpmCharacters.IsAnyPlayerCloseEnough(pos, distance));
	}

	public bool IsAnyTackleCloseEnough(Vector3 pos, float distance)
	{
		for (int i = 0; i < GameFactory.RodSlots.Length; i++)
		{
			TackleBehaviour tackle = GameFactory.RodSlots[i].Tackle;
			if (tackle != null && (tackle.Hook != null || tackle is Lure1stBehaviour) && tackle.transform != null && (tackle.Position - pos).magnitude < distance)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsActiveSailing
	{
		get
		{
			return this._currentBoat != null && this._currentBoat.IsActiveMovement;
		}
	}

	public void OnFishCaugh()
	{
		if (CaymanGenerator.Instance != null && this.Tackle.Fish != null && this.Tackle.Fish.FishTemplate.Weight < 5f)
		{
			Vector3 vector = Math3d.ProjectOXZ(this.Tackle.Position - base.transform.position);
			Vector3 vector2 = Math3d.ProjectOXZ(this.Rod.TipPosition - base.transform.position);
			float num = Vector3.Angle(vector, vector2);
			if (vector.magnitude > CaymanGenerator.Instance.JUMPING_MIN_DIST_TO_PLAYER && num < CaymanGenerator.Instance.JUMPING_DIR_GOOD_ANGLE)
			{
				CaymanGenerator.Instance.OnFishCaugh();
			}
		}
	}

	public void CleanRodPods()
	{
		if (this._rodPods != null)
		{
			List<RodPodController> list = this._rodPods.ToList<RodPodController>();
			this._rodPods.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				RodPodController rodPodController = list[i];
				if (rodPodController.CouldBeTaken)
				{
					rodPodController.RemoveAllRods();
					Object.Destroy(rodPodController.gameObject);
				}
				else
				{
					rodPodController.RemoveAllRods();
					this._rodPods.Add(rodPodController);
				}
				InGameMap.Instance.RemoveRodPod(rodPodController.Item.InstanceId.ToString());
			}
			if (this._cache != null && this._cache.CurFraction != null)
			{
				this._cache.CurFraction.ClearPods();
			}
		}
	}

	public void StartMoveToNewLocation()
	{
		this.CleanRodPods();
		if (this._tpmCharacters != null)
		{
			this._tpmCharacters.StartMoveToNewLocation();
		}
	}

	public void FinishMoveToNewLocation(bool isTeleportation)
	{
		if (this.Rod != null && isTeleportation)
		{
			this.Rod.SetVisibility(false);
			this.Reel.SetVisibility(false);
			this.Tackle.SetVisibility(false);
			this.Line.SetVisibility(false);
			this.SetHandsVisibility(false);
		}
		if (this._tpmCharacters != null)
		{
			this._tpmCharacters.FinishMoveToNewLocation();
		}
		if (GameFactory.BoatDock != null)
		{
			if (!GameFactory.IsPlayerInitialized)
			{
				GameFactory.BoatDock.TrySpawnBoat(false);
			}
			else
			{
				GameFactory.BoatDock.RefreshBoats((!this.IsSailing) ? null : new ItemSubTypes?(this._currentBoat.Category));
			}
		}
		Transform component = GameObject.Find(StaticUserData.CurrentLocation.Asset).GetComponent<Transform>();
		if (PhotonConnectionFactory.Instance.IsSavePlayerStateOn && !isTeleportation)
		{
			PersistentData persistentData = PhotonConnectionFactory.Instance.Profile.PersistentData;
			if (persistentData != null)
			{
				LogHelper.Log("PersistentData: PlayerPosition: {0}, BoatPosition: {1}, IsBoarded: {2}, LastBoatType: {3}, LastPinId: {4}", new object[] { persistentData.Position, persistentData.BoatPosition, persistentData.IsBoarded, persistentData.LastBoatType, persistentData.LastPinId });
				ItemSubTypes boatType = persistentData.LastBoatType;
				if (boatType != ItemSubTypes.All)
				{
					IBoatController boatController = null;
					bool flag = GameFactory.BoatDock.IsBoatTypeAllowed(boatType);
					if (flag)
					{
						boatController = GameFactory.BoatDock.Boats.FirstOrDefault((IBoatController b) => b.Category == boatType);
						if (boatController == null && boatType == ItemSubTypes.MotorBoat)
						{
							boatController = GameFactory.BoatDock.Boats.FirstOrDefault((IBoatController b) => b.Category == ItemSubTypes.BassBoat);
						}
						if (boatController != null)
						{
							boatController.Teleport(persistentData.BoatPosition.ToVector3(), persistentData.BoatRotation.ToQuaternion());
							boatType = boatController.Category;
						}
					}
					if (persistentData.IsBoarded)
					{
						if (!flag)
						{
							LogHelper.Log("Restore on the ground because {0} is not allowed", new object[] { boatType });
							this.Move(component);
						}
						else if (boatController == null)
						{
							LogHelper.Log("Restore on the ground because {0} is not found in the dock (hope impossible case)", new object[] { boatType });
							this.Move(component);
						}
						else
						{
							this._boardingCategory = boatType;
							this._isReadyForBoarding = true;
							this.ImmediateBoarding = true;
							LogHelper.Log("Restore on the Boat");
						}
					}
					else
					{
						this.RestoreOnTheGround(persistentData, component);
					}
				}
				else
				{
					this.RestoreOnTheGround(persistentData, component);
				}
			}
			this._lastPin = component;
		}
		else
		{
			this.Move(component);
		}
		PhotonConnectionFactory.Instance.Profile.PersistentData = null;
		this.Player.SetActive(true);
	}

	private void RestoreOnTheGround(PersistentData data, Transform pinTransform)
	{
		LogHelper.Log("Restore on the ground. Position = {0}, rotation = {1}", new object[] { data.Position, data.Rotation });
		if (data.Position != null)
		{
			if (data.Rotation != null)
			{
				this.CameraController.CameraMouseLook.ResetOriginalRotation(Quaternion.Euler(0f, data.Rotation.Y, 0f));
			}
			this.Collider.position = data.Position.ToVector3();
		}
		else
		{
			this.Move(pinTransform);
		}
	}

	internal void Start()
	{
		this._targetCloser.Setup(this.Collider, this.CameraController.Camera.transform, new UpdatePlayerPositionAndRotationDelegate(this.OnBoardingMove));
		GameObject gameObject = GameObject.Find("pods");
		if (gameObject != null)
		{
			Transform transform = gameObject.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				RodPodController component = transform.GetChild(i).GetComponent<RodPodController>();
				if (component != null)
				{
					this._rodPods.Add(component);
					component.OnPut(false);
				}
			}
		}
		ShowHudElements.Instance.ECloseCatchedItemWindow += this.OnCloseCatchedItemWindow;
		if (StaticUserData.IS_TPM_VISIBLE && !StaticUserData.IS_IN_TUTORIAL)
		{
			this._tpmCharacters = new TPMCharactersController(this.cameraController.Camera);
		}
		this.strikeValue.target = 0f;
		this.ToolsOffset = new Vector3(0f, 0f, 0f);
		this.PlayAnimation("empty", 1f, 1f, 0f);
		PhotonConnectionFactory.Instance.OnFBAZonesUpdate += this.OnUpdateFBAZones;
		PhotonConnectionFactory.Instance.OnGameActionResult += this.OnGameActionResult;
		PhotonConnectionFactory.Instance.OnItemBroken += this.OnItemBroken;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnBoatsRentFinished += this.OnBoatsRentFinished;
		PhotonConnectionFactory.Instance.OnItemGained += this.OnItemGained;
		PhotonConnectionFactory.Instance.OnItemBought += this.OnItemBought;
		PhotonConnectionFactory.Instance.OnItemLost += this.OnItemLost;
		if (this._tpmCharacters != null)
		{
			PhotonConnectionFactory.Instance.OnCharacterUpdated += this.InstanceOnOnCharacterUpdated;
			PhotonConnectionFactory.Instance.OnCharacterDestroyed += this.InstanceOnOnCharacterDestroyed;
			GameFactory.ChatListener.OnGotIncomeMessage += this._tpmCharacters.OnIncomingChatMessage;
		}
		this.Init3DCharParameters();
		if (!StaticUserData.IS_IN_TUTORIAL)
		{
			this._map = Object.Instantiate<GameObject>(this._pMap, this.RodLeftRoot.transform);
			this._map.transform.localRotation = Quaternion.identity;
			this._map.transform.localPosition = Vector3.zero;
			this._map.SetActive(false);
		}
		if (AFPSCounter.Instance != null)
		{
			AFPSCounter.Instance.fpsCounter.ResetValues();
			AFPSLogger.LogMemoryUsage("Scene loaded");
		}
	}

	private void OnItemGained(IEnumerable<InventoryItem> items, bool announce)
	{
		if (items.Any((InventoryItem i) => i.ItemType == ItemTypes.Boat))
		{
			this.OnNewBoat();
		}
	}

	private void OnItemLost(InventoryItem item)
	{
		if (StaticUserData.RodInHand.Rod != null && !RodHelper.IsRodEquipped(StaticUserData.RodInHand.Rod))
		{
			this.IsEmptyHandsMode = true;
		}
		if (this.RequestedRod != null && item.Storage == StoragePlaces.ParentItem)
		{
			Guid? instanceId = this.RequestedRod.InstanceId;
			bool flag = instanceId != null;
			Guid? parentItemInstanceId = item.ParentItemInstanceId;
			if (flag == (parentItemInstanceId != null) && (instanceId == null || instanceId.GetValueOrDefault() == parentItemInstanceId.GetValueOrDefault()) && !RodHelper.IsRodEquipped(this.RequestedRod))
			{
				this.RequestedRod = null;
			}
		}
	}

	private void OnItemBought(InventoryItem item)
	{
		if (item.ItemType == ItemTypes.Boat)
		{
			this.OnNewBoat();
		}
	}

	private void OnNewBoat()
	{
		if (this.IsSailing)
		{
			this._refreshBoatsRequest = true;
		}
		else
		{
			this.RecreateAllBoats();
		}
	}

	private void OnBoatsRentFinished(short[] boatsWithFinishedRent)
	{
		this._boatsWithFinishedRent = new List<short>(boatsWithFinishedRent);
		if (!this.IsTackleThrown)
		{
			this.ShowExtendRentDialog();
		}
	}

	private void Init3DCharParameters()
	{
		this.PlayerSpeedParameters = new PlayerSpeedParameters
		{
			ReelingForwardSpeed = 0.25f,
			ReelingSidewaysSpeed = 0.15f,
			ThrowingForwardSpeed = 0.7f,
			ThrowingSidewaysSpeed = 0.25f
		};
	}

	internal void OnDestroy()
	{
		this._independentChumController.Unsubscribe();
		this._shadow = null;
		this._weather = null;
		this._zonesCollider = null;
		PlayerController.SmoothAnimationWeight.ArmsAnimation = null;
		this._motor = null;
		this._fpController = null;
		this._fishBoxActivityZones.Clear();
		PhotonConnectionFactory.Instance.OnFBAZonesUpdate -= this.OnUpdateFBAZones;
		ShowHudElements.Instance.ECloseCatchedItemWindow -= this.OnCloseCatchedItemWindow;
		GameFactory.Clear(false);
		PhotonConnectionFactory.Instance.OnGameActionResult -= this.OnGameActionResult;
		PhotonConnectionFactory.Instance.OnItemBroken -= this.OnItemBroken;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnBoatsRentFinished -= this.OnBoatsRentFinished;
		PhotonConnectionFactory.Instance.OnItemGained -= this.OnItemGained;
		PhotonConnectionFactory.Instance.OnItemBought -= this.OnItemBought;
		PhotonConnectionFactory.Instance.OnItemLost -= this.OnItemLost;
		if (this._tpmCharacters != null)
		{
			PhotonConnectionFactory.Instance.OnCharacterUpdated -= this.InstanceOnOnCharacterUpdated;
			PhotonConnectionFactory.Instance.OnCharacterDestroyed -= this.InstanceOnOnCharacterDestroyed;
			if (GameFactory.ChatListener != null && this._tpmCharacters != null)
			{
				GameFactory.ChatListener.OnGotIncomeMessage -= this._tpmCharacters.OnIncomingChatMessage;
			}
			this._tpmCharacters.Clean();
		}
		for (int i = 0; i < this._rodPods.Count; i++)
		{
			if (this._rodPods[i] != null)
			{
				this._rodPods[i].Clean();
			}
		}
		for (int j = 0; j < this._savedRodPods.Count; j++)
		{
			if (this._savedRodPods[j] != null)
			{
				this._savedRodPods[j].Clean();
			}
		}
		AFPSLogger.LogMemoryUsage("Scene unloading");
		AFPSLogger.LogFps("Scene performance");
	}

	private void OnDisable()
	{
		if (this.CurrentBoat != null)
		{
			this.CurrentBoat.DisableSound(true);
		}
		ControlsController.ControlsActions.BlockInput(new List<string> { "UICancel", "Inventory", "InventoryAdditional", "Map", "MapAdditional" });
	}

	public static void SetupMixingAnimation(AnimationState state, int layer = 1)
	{
		state.layer = layer;
		state.blendMode = 1;
	}

	private void RefreshSleeves()
	{
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.Waistcoat);
		if (inventoryItem != null && this.SleevesPrefabName != inventoryItem.Asset && !string.IsNullOrEmpty(inventoryItem.Asset))
		{
			this.SleevesPrefabName = inventoryItem.Asset;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		Dictionary<string, string> settings = profile.Settings;
		Color color = ((!settings.ContainsKey("TpmSkinColorR")) ? new Color(0.34901962f, 0.3137255f, 0.23137255f) : new Color(this.ParseFloat(settings["TpmSkinColorR"]), this.ParseFloat(settings["TpmSkinColorG"]), this.ParseFloat(settings["TpmSkinColorB"])));
		SkinnedMeshRenderer rendererForObject = RenderersHelper.GetRendererForObject(this.Hands.transform);
		TPMCharacterCustomization.SetMeshColor(rendererForObject, color);
	}

	private float ParseFloat(string v)
	{
		return float.Parse(v.Replace(',', '.'), CultureInfo.InvariantCulture);
	}

	public void CalcRodAngles()
	{
		float length = this.Rod.Length;
		if (length < 1.7f)
		{
			this.pullMaxAngle = -40f;
			this.pullValue.Speed = 1f;
			this.speedLengthAjust = 1f;
		}
		else
		{
			float num = 1.7f;
			float num2 = Mathf.Abs(Mathf.Asin(1.092f / length) * 57.29578f) / 40f;
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			this.pullValue.Speed = num2;
			this.speedLengthAjust = num2;
			if ((double)this.Rod.Length > 1.7)
			{
				num += (length - num) * 0.1f;
			}
			this.pullMaxAngle = -1f * Mathf.Abs(Mathf.Asin(1.092f / num) * 57.29578f);
		}
	}

	private void UpdateRotSpeed()
	{
		float num = this.SmoothForce(this.RodForce.magnitude);
		float rotationAngle = this.GetRotationAngle();
		float num2 = Mathf.Abs(rotationAngle) - Mathf.Abs(this.oldRotationalAngel);
		this.oldRotationalAngel = this.GetRotationAngle();
		num *= num2 * 5f * this.speedLengthAjust;
		float num3 = 12f * this.speedLengthAjust * 2.75f;
		num = Mathf.Clamp(num, -3f, 9f);
		num3 = Mathf.Clamp(num3, 9f, 12f);
		this._dataSpeedQ.Add(num);
		this.Speed = Mathf.Clamp(num3 - this._dataSpeedQ.MiddleValue, 3f, 12f);
	}

	internal void Update()
	{
		if (!Mathf.Approximately(this.CameraController.Camera.fieldOfView, this.TargetFov))
		{
			this.CameraController.Camera.fieldOfView = Mathf.Lerp(this.TargetFov, this.CameraController.Camera.fieldOfView, Mathf.Exp(-10f * Time.deltaTime));
		}
		if (this.RodSlot.SimThread != null)
		{
			this.RodSlot.SimThread.WaitForThreadSync();
		}
		for (int i = this._changingAnimations.Count - 1; i >= 0; i--)
		{
			if (this._changingAnimations[i].Update())
			{
				this._changingAnimations.RemoveAt(i);
			}
		}
		if (GameFactory.BoatDock != null)
		{
			for (int j = 0; j < GameFactory.BoatDock.Boats.Count; j++)
			{
				GameFactory.BoatDock.Boats[j].Update();
			}
		}
		this.fsm.Update();
		if (GameFactory.IsRodAssembling || this.RodSlot.PendingServerOp)
		{
			return;
		}
		this.UpdateRotSpeed();
		this.UpdateIndicator();
		this.SwitchingRodHandler();
		this.UpdateDynWater();
		this._independentChumController.UpdatePositions(Time.deltaTime);
		this.UpdateFeedings();
		if (GameFactory.InteractiveObjectController != null)
		{
			GameFactory.InteractiveObjectController.CheckInteraction(this.CameraController.Camera.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f)));
		}
		if (!this.IsSailing && PhotonConnectionFactory.HasPhotonServerConnection && PhotonConnectionFactory.Instance.Game != null)
		{
			GameActionAdapter.Instance.Walk();
		}
		if (SettingsManager.InputType != InputModuleManager.InputType.Mouse && InputManager.IsInputDeviceActivated(InputModuleManager.InputType.Mouse))
		{
			ShowHudElements.Instance.OnInputDeviceActivated(InputModuleManager.InputType.Mouse);
		}
		else if (SettingsManager.InputType != InputModuleManager.InputType.GamePad && InputManager.IsInputDeviceActivated(InputModuleManager.InputType.GamePad))
		{
			ShowHudElements.Instance.OnInputDeviceActivated(InputModuleManager.InputType.GamePad);
		}
	}

	private void UpdateDynWater()
	{
		if (GameFactory.Water == null)
		{
			return;
		}
		if (this._currentDynWaterPosition.y < -100f || Vector3.Distance(this._currentDynWaterPosition, base.transform.position) > 5f)
		{
			this._currentDynWaterPosition = base.transform.position;
			GameFactory.Water.SetDynWaterPosition(this._currentDynWaterPosition.x, this._currentDynWaterPosition.z);
		}
		GameFactory.Water.PlayerPosition = base.transform.position;
		GameFactory.Water.PlayerForward = base.transform.forward;
	}

	private static void InformRodUnequipped(int slotId, Rod newRodInst)
	{
		Profile profile = ((PhotonConnectionFactory.Instance == null) ? null : PhotonConnectionFactory.Instance.Profile);
		Action<string> action = new Action<string>(GameFactory.Message.ShowRodIsNotAssembled);
		if (profile != null)
		{
			List<InventoryItem> rodEquipment = profile.Inventory.GetRodEquipment(newRodInst);
			Dictionary<ItemTypes, bool> dictionary = new Dictionary<ItemTypes, bool>
			{
				{
					ItemTypes.Reel,
					false
				},
				{
					ItemTypes.Line,
					false
				}
			};
			for (int i = 0; i < rodEquipment.Count; i++)
			{
				InventoryItem inventoryItem = rodEquipment[i];
				if (dictionary.ContainsKey(inventoryItem.ItemType))
				{
					dictionary[inventoryItem.ItemType] = true;
				}
			}
			if (!dictionary[ItemTypes.Reel])
			{
				action = new Action<string>(GameFactory.Message.ShowRodHasNoReel);
			}
			else if (!dictionary[ItemTypes.Line])
			{
				action = new Action<string>(GameFactory.Message.ShowRodHasNoLine);
			}
			else
			{
				action = new Action<string>(GameFactory.Message.ShowRodHasNoTackle);
			}
		}
		action(slotId.ToString(CultureInfo.InvariantCulture));
		Debug.Log("Rod is unequiped");
	}

	private void InitGrip()
	{
		GameObject gameObject = ((StaticUserData.RodInHand.ReelType != ReelTypes.Baitcasting) ? this.RodLeftRoot : this.RodRoot);
		this.Grip.ChangeRoot(gameObject.transform);
	}

	private void OnItemBroken(BrokenTackleType code, int rodSlotIndex)
	{
		if (this._brokenItemActions.ContainsKey(code))
		{
			LogHelper.Log("OnItemBroken({0})", new object[] { code });
			this._brokenItemActions[code]();
			GameFactory.RodSlot rodSlot = GameFactory.RodSlots[rodSlotIndex];
			(rodSlot.Rod.RodAssembly as AssembledRod).IsRodDisassembled = true;
			rodSlot.Reel.OnItemBroken();
			if (rodSlot.Rod is Rod1stBehaviour)
			{
				GameFactory.FishSpawner.EscapeAllFish(rodSlotIndex);
			}
			else
			{
				(rodSlot.Rod as RodOnPodBehaviour).EscapeFish();
			}
		}
	}

	public Vector3 RodForce
	{
		get
		{
			return (this.Rod == null) ? Vector3.zero : this.Rod.GetRodForce();
		}
	}

	public Vector3 RodForceHand
	{
		get
		{
			return (this.Rod == null) ? Vector3.zero : this.Rod.GetRodForceHand();
		}
	}

	public float GetRotationAngleToTackle()
	{
		float num = 0f;
		if (this.Tackle == null || this.Line == null || this.Rod == null)
		{
			return num;
		}
		float num2 = this.Line.SecuredLineLength;
		float length = this.Rod.Length;
		if (num2 < length)
		{
			return num;
		}
		float num3 = base.transform.eulerAngles.x;
		Vector3 vector = base.transform.InverseTransformPoint(this.RodSlot.Sim.TackleTipMass.Position);
		float x = vector.x;
		float z = vector.z;
		float num4 = Mathf.Sqrt(x * x + z * z);
		float num5 = Mathf.Atan2(x, z) * 57.29578f * -1f;
		float num6 = 1f;
		if (num5 > 120f || num5 < -120f)
		{
			num6 = Mathf.Clamp01((60f - (Mathf.Abs(num5) - 120f)) / 60f);
		}
		if (num5 > 180f)
		{
			num5 -= 360f;
		}
		if (num5 > 90f)
		{
			num5 = 90f;
		}
		if (num5 < -90f)
		{
			num5 = -90f;
		}
		num5 = Mathf.Sin(num5 * 0.017453292f);
		num5 = Mathf.Lerp(0f, num5, num6);
		if (num3 >= 180f)
		{
			num3 = 180f - (num3 - 180f);
		}
		if (num3 >= 35f)
		{
			if (num3 >= 80f)
			{
				num3 = 80f;
			}
			float num7 = Mathf.Clamp01((80f - num3) / 45f);
			num5 = Mathf.Lerp(0f, num5, num7);
		}
		if (num4 - length < 10f)
		{
			num5 = Mathf.Lerp(0f, num5, (num4 - length) / 10f);
		}
		return num5 * Mathf.Exp(-1f / (2f * num4 + 0.1f));
	}

	public float GetRotationAngle()
	{
		return (this.Rod == null) ? 0f : this.GetRotationAngleToTackle();
	}

	public float GetRotationForceAngle()
	{
		return (this.Rod == null) ? 0f : this.Rod.GetRotationalDamper();
	}

	public float GetRodAccel()
	{
		return (this.Rod == null) ? 0f : this.Rod.GetRodAccel();
	}

	public float GetRodAccelHand()
	{
		return (this.Rod == null) ? 0f : this.Rod.GetRodAccel();
	}

	protected void SetPullSpeed(float speed)
	{
		this.pullSpeed = speed;
	}

	public void SetPullAngel(float angle, float weight)
	{
		float num = angle * Mathf.Abs(this.pullMaxAngle) * 0.7f - this.pullSideAngle;
		if (num > 0.1f)
		{
			num = 0.1f;
		}
		else if (num < -0.1f)
		{
			num = -0.1f;
		}
		this.pullSideAngle += num;
		this.pullAngle = this.pullMaxAngle + Mathf.Abs(this.pullSideAngle);
		if (this.pullAngle > -5f)
		{
			this.pullAngle = -5f;
		}
	}

	public float SmoothForce(float a)
	{
		float num = a;
		float num2 = 100f;
		float num3 = 100f;
		if (num > 100f)
		{
			num = 100f;
		}
		num /= num2;
		if (this.Rod != null)
		{
			num3 = this.Rod.MaxLoad * 1.25f;
		}
		return Mathf.Sin(num * 3.1415927f * 0.75f) * num2 * (100f / num3);
	}

	protected Vector3 GetHandShift()
	{
		float rodAccel = this.GetRodAccel();
		float rodAccelHand = this.GetRodAccelHand();
		float num = this.SmoothForce(this.RodForceHand.magnitude);
		Vector3 vector = base.transform.InverseTransformDirection(this.RodForce);
		Vector3 vector2;
		vector2..ctor(0f, 0f, 0f);
		float rotationAngle = this.GetRotationAngle();
		float num2 = Mathf.Sign(rodAccel);
		float num3 = Mathf.Lerp(0.2f, 1f, 1f - Mathf.Abs(rotationAngle));
		float num4 = Mathf.Clamp(Mathf.Abs(rodAccelHand), 0f, 1f);
		float num5 = Mathf.Lerp(1f, 1.25f, num / 100f);
		float num6 = num4 * 0.01f * num5 * num3;
		if (rodAccelHand > 0.01f || rodAccelHand < -0.01f)
		{
			vector2 = Vector3.Lerp(vector2, vector.normalized * num2, num6);
		}
		this._dataShiftQ.Add(vector2);
		return this._dataShiftQ.MiddleValue;
	}

	public float GetHandRotation()
	{
		float rodAccel = this.GetRodAccel();
		float num = this.SmoothForce(this.RodForce.magnitude);
		float rotationAngle = this.GetRotationAngle();
		float num2 = Mathf.Sign(rodAccel);
		float num3 = Mathf.Clamp(Mathf.Abs(rodAccel), 0f, 1f);
		float num4 = Mathf.Lerp(1f, 1.25f, num / 100f);
		float num5 = num3 * 0.1f * rotationAngle * -1f * num2 * num4;
		this._dataRotQ.Add(num5);
		return this._dataRotQ.MiddleValue;
	}

	private void UpdateStrikeAndPull()
	{
		Type currentStateType = this.fsm.CurrentStateType;
		bool flag = currentStateType == typeof(PlayerIdleThrown) || currentStateType == typeof(PlayerRoll) || currentStateType == typeof(PlayerThrowOutWait);
		if (!this.pullFlag && !this.strikeFlag && flag && ControlsController.ControlsActions.Fire2.IsPressed)
		{
			if (!this.pullValue.isActive())
			{
				this.SendPullTrigger();
			}
			this.pullValue.setTriggered(true);
		}
		else
		{
			if (this.pullValue.isActive())
			{
				this.SendPullTriggerOff();
			}
			this.pullValue.setTriggered(false);
		}
		if (this.Tackle != null && this.Tackle.IsFishHooked)
		{
			this.isHookedValue.setTriggered(true);
		}
		else
		{
			this.isHookedValue.setTriggered(false);
		}
		this.pullValue.dumpingForce = this.RodSlot.Sim.RodAppliedForce;
		this.pullValue.update(Time.deltaTime);
		this.isHookedValue.update(Time.deltaTime);
		this.strikeValue.update(Time.deltaTime);
	}

	private void SendPullTrigger()
	{
		this.pullTriggered = true;
	}

	private void SendPullTriggerOff()
	{
		this.pullTriggered = false;
	}

	protected void ProceduralRotation()
	{
		Quaternion rotation = base.transform.parent.rotation;
		Quaternion quaternion = Quaternion.Inverse(rotation);
		float num = Quaternion.Angle(base.transform.rotation, rotation);
		float num2 = Mathf.Lerp(1f, 6f, num / 70f);
		this.angleAccum = Mathf.Lerp(num, this.angleAccum, Mathf.Exp(-Time.deltaTime * 5f));
		float num3;
		if (num < 20f)
		{
			num3 = Mathf.Exp(-Time.deltaTime * this.handsDamper);
		}
		else
		{
			num3 = Mathf.Exp(-this.angleAccum * 0.1f);
		}
		Quaternion quaternion2 = Quaternion.Slerp(rotation, this.lastRotation, num3);
		Vector3 vector;
		vector..ctor(0f, -this.localHandsOffset, 0f);
		vector += this.ToolsOffset;
		Quaternion quaternion3;
		quaternion3..ctor(0f, 0f, 0f, 1f);
		Vector3 vector2;
		vector2..ctor(0f, -this.localHandsOffset - 0.2f, 0f);
		Vector3 vector3;
		vector3..ctor(0f, 0f, 0f);
		float num4 = this.pullSideAngle * 0.85f;
		Quaternion quaternion4 = Quaternion.AngleAxis(this.pullAngle, Vector3.right) * Quaternion.AngleAxis(num4, Vector3.up);
		if (this.ReelType == ReelTypes.Spinning || (this.IsSailing && !this.IsBoatFishing))
		{
			float num5 = -Mathf.Abs(this.pullMaxAngle) * 0.005f + Mathf.Abs(this.pullSideAngle / Mathf.Abs(this.pullMaxAngle) * Mathf.Abs(this.pullMaxAngle) * 0.5f * 0.01f);
			float num6 = -Mathf.Abs(this.pullMaxAngle * 0.75f) * 0.005f + Mathf.Abs(this.pullSideAngle / Mathf.Abs(this.pullMaxAngle) * Mathf.Abs(this.pullMaxAngle) * 0.4f * 0.01f);
			vector2.y = -this.localHandsOffset + num5;
			vector3.y = num6;
		}
		else
		{
			vector..ctor(0f, -this.localHandsOffset + ((this.State == typeof(ShowMap)) ? 0f : (-0.02f)), 0f);
			vector += this.ToolsOffset;
			vector2..ctor(0f, -this.localHandsOffset - 0.22f, -0.015f);
			float num7 = -Mathf.Abs(this.pullMaxAngle) * 0.005f + Mathf.Abs(this.pullSideAngle / Mathf.Abs(this.pullMaxAngle) * Mathf.Abs(this.pullMaxAngle) * 0.5f * 0.01f);
			float num8 = -Mathf.Abs(this.pullMaxAngle * 0.75f) * 0.005f + Mathf.Abs(this.pullSideAngle / Mathf.Abs(this.pullMaxAngle) * Mathf.Abs(this.pullMaxAngle) * 0.4f * 0.01f);
			vector2.y = -this.localHandsOffset - 0.02f + num7;
			vector3.y = num8;
		}
		float num9 = this.strikeValue.value;
		if (num9 <= 0f)
		{
			num9 = this.pullValue.getValue();
		}
		Vector3 handShift;
		handShift..ctor(0f, 0f, 0f);
		Quaternion quaternion5 = Quaternion.identity;
		Type currentStateType = this.fsm.CurrentStateType;
		if (currentStateType == typeof(PlayerIdleThrown) || currentStateType == typeof(PlayerRoll))
		{
			handShift = this.GetHandShift();
			float num10 = Mathf.Clamp(this.GetHandRotation() * 8f, -1f, 1f);
			quaternion5 = Quaternion.AngleAxis(num10, Vector3.up);
		}
		Vector3 vector4;
		vector4..ctor(0f, 0f, 0f);
		Quaternion quaternion6 = Quaternion.identity;
		if ((currentStateType == typeof(PlayerIdleThrown) || currentStateType == typeof(PlayerRoll) || currentStateType == typeof(PlayerShowFishLineIn) || currentStateType == typeof(PlayerShowFishIn)) && !this.IsPitching)
		{
			float num11 = ((currentStateType != typeof(PlayerShowFishLineIn) && currentStateType != typeof(PlayerShowFishIn)) ? 1f : (1f - this.ArmsAnimation[this._lastAnimation].normalizedTime));
			float value = this.isHookedValue.getValue();
			quaternion6 = Quaternion.AngleAxis(this.pullAngle * 0.75f * num11, Vector3.right);
			vector4 = vector3 * value * num11;
		}
		Quaternion quaternion7 = Quaternion.identity;
		Vector3 vector5;
		vector5..ctor(0f, 0f, 0f);
		if (!this.IsEmptyHandsMode && this.ReelType == ReelTypes.Baitcasting)
		{
			if (num4 > 0f)
			{
				vector5.y += Mathf.Abs(num4) * 0.0047f;
				quaternion7 = Quaternion.AngleAxis(Mathf.Abs(num4) * 0.75f, Vector3.right);
			}
			else
			{
				quaternion7 = Quaternion.AngleAxis(Mathf.Abs(num4) * 0.8f, Vector3.right);
				vector5.y += Mathf.Abs(num4) * 0.0018f;
			}
		}
		base.transform.localPosition = handShift + Vector3.Lerp(vector, vector2, num9) + vector4 + vector5;
		base.transform.localRotation = quaternion7 * quaternion5 * quaternion * quaternion2 * Quaternion.Slerp(quaternion3, quaternion4, num9) * Quaternion.Slerp(quaternion3, quaternion6, this.isHookedValue.getValue());
		this.lastRotation = quaternion2;
		float num12 = num9 * num9 * num9 * num9;
		this.Update3dCharMecanimParameter(TPMMecanimFParameter.PullValue, (num12 <= 0.9f) ? num12 : 1f);
	}

	internal void FixedUpdate()
	{
		this.UpdateStrikeAndPull();
	}

	private Vector3 ColliderPosition
	{
		get
		{
			if (this.BoardingFrom != null)
			{
				return this.BoardingFrom.Value;
			}
			Vector3 vector;
			if (this.IsSailing)
			{
				vector = this.CurrentBoat.TPMPlayerPosition;
			}
			else
			{
				Vector3? savedPosition = this.SavedPosition;
				vector = ((savedPosition == null) ? this._motor.ColliderPosition : savedPosition.Value);
			}
			return vector;
		}
	}

	public void SetViewPause(bool flag)
	{
		IPhotonServerConnection instance = PhotonConnectionFactory.Instance;
		if (this._cache != null)
		{
			Package package = this._cache.SetViewPause(flag, this.ColliderPosition, this.CameraController.Camera.transform, this.CurrentBoat);
			if (package != null)
			{
				this.SendCharacterPackage(package);
			}
			this.SendPlayerPositionAndRotation(false);
		}
	}

	public Vector3 MainCameraDebugOffset { get; set; }

	internal void LateUpdate()
	{
		this.ProceduralRotation();
		this.UpdateTarget();
		if (this.DebugFreezePhysics)
		{
			this.FreezePhysics();
		}
		else
		{
			this.UnfreezePhysics();
		}
		if (GameFactory.BoatDock != null)
		{
			for (int i = 0; i < GameFactory.BoatDock.Boats.Count; i++)
			{
				GameFactory.BoatDock.Boats[i].LateUpdate();
			}
		}
		if (this._rodBoneAdjuster.StartedAt > 0f)
		{
			RodController takingRod = this.TakingRod;
			Transform transform = takingRod.transform;
			float num = Mathf.Clamp01((Time.time - this._rodBoneAdjuster.StartedAt) / this._rodBoneAdjuster.Duration);
			transform.localRotation = Quaternion.Slerp(this._rodBoneAdjuster.FromRotation, this._rodBoneAdjuster.ToRotation, num);
			transform.localPosition = Vector3.zero;
			if (Time.time - this._rodBoneAdjuster.StartedAt > this._rodBoneAdjuster.Duration)
			{
				this._rodBoneAdjuster = this._rodBoneAdjuster.Clear();
			}
		}
		this.fsm.LateUpdate();
		if (this.Rod != null && this.Rod.IsInitialized && (this.State != typeof(PlayerEmpty) || (this.Tackle != null && this.Tackle.IsActive)) && this.State != typeof(ShowMap) && !this.physicsFreezed)
		{
			this.Rod.OnLateUpdate();
			this.RodInHandsUpdate();
		}
		PodSlotData? takingRodSlot = this._takingRodSlot;
		if (takingRodSlot != null && this.TakingRod != null && this.TakingRod.Behaviour != this.Rod)
		{
			(this.TakingRod.Behaviour as Rod1stBehaviour).OnLateUpdate();
		}
		for (int j = 0; j < this._rodPods.Count; j++)
		{
			this._rodPods[j].UpdateRodsBeforeSim();
		}
		PodSlotData? takingRodSlot2 = this._takingRodSlot;
		if (takingRodSlot2 != null)
		{
			this._takingRodSlot.Value.Rod.Behaviour.LateUpdateBeforeSim();
		}
		for (int k = 0; k < this._rodPods.Count; k++)
		{
			this._rodPods[k].UpdateSim();
		}
		for (int l = 0; l < this._rodPods.Count; l++)
		{
			this._rodPods[l].UpdateRodsAfterSim();
		}
		PodSlotData? takingRodSlot3 = this._takingRodSlot;
		if (takingRodSlot3 != null)
		{
			this._takingRodSlot.Value.Rod.Behaviour.LateUpdateAfterSim();
		}
		if (this.sleevesFurMaterial != null)
		{
			this.UpdateFurMovement();
		}
		if (this._updatePhotoModeRequest)
		{
			this._updatePhotoModeRequest = false;
			this.UpdatePhotoModel(this._phmModel);
		}
		if (!string.IsNullOrEmpty(this._lastAnimation))
		{
			float curveValueForLocalTime = this._ikCurves.GetCurveValueForLocalTime((this._reelType != ReelTypes.Baitcasting) ? "IKWeightCurve" : "IKWeightCurveCasting", this._lastAnimation, Mathf.Repeat(this.ArmsAnimation[this._lastAnimation].time, this.ArmsAnimation[this._lastAnimation].length));
			this._reelIkController.solver.IKPositionWeight = curveValueForLocalTime;
		}
		if (StaticUserData.IS_TPM_ENABLED)
		{
			Package package = this._cache.Update(this.ColliderPosition, this.CameraController.Camera.transform, this.CurrentBoat);
			if (package != null)
			{
				this.SendCharacterPackage(package);
				this.SendPlayerPositionAndRotation(false);
			}
			if (this._tpmCharacters != null)
			{
				this._tpmCharacters.Update();
			}
		}
		if (!this.IsSailing && !this.IsTackleThrown && this.fsm.CurrentStateType != typeof(PlayerPhotoMode) && this.CameraController.Camera.transform.position.y <= 0.75f && !this.ImmediateBoarding)
		{
			LogHelper.Log("last pin teleportation");
			this.Move(this._lastPin);
		}
	}

	public void RodInHandsUpdate()
	{
		this.UpdateLinePoints(this.Line.ServerMainAndLeaderPoints, this.Line.SinkersFirstPoint, this.Line.LeaderObj != null, this.Line.TerrainHit);
		this.HudFishingHandler.LineHandler.LineInWater = (int)MeasuringSystemManager.LineLength(this.Line.IndicatedLineLength);
		this.HudFishingHandler.LineHandler.LineLength = (int)MeasuringSystemManager.LineLength((float)StaticUserData.RodInHand.Line.Length.Value);
	}

	public bool IsReplayMode
	{
		get
		{
			return false;
		}
	}

	public GameObject Create3DPlayerFromProfile(string modelName, string modelID, MeshBakersController.ModelReadyDelegate onModelCreated)
	{
		MeshBakersController baker = PlayerController.GetBaker();
		baker.EModelReady += onModelCreated;
		GameObject gameObject = new GameObject(modelName);
		baker.AskToCreateModelParts(gameObject, modelID, PlayerController.CurrentPlayerModel, true);
		return gameObject;
	}

	public static TPMCharacterModel CurrentPlayerModel
	{
		get
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			Dictionary<string, string> settings = profile.Settings;
			Faces faces = (Faces)((!settings.ContainsKey("TpmHeadType")) ? 3 : Convert.ToInt32(settings["TpmHeadType"]));
			Hair hair = (Hair)((!settings.ContainsKey("TpmHairType")) ? 2 : Convert.ToInt32(settings["TpmHairType"]));
			Pants pants = (Pants)((!settings.ContainsKey("TpmPantsType")) ? 0 : Convert.ToInt32(settings["TpmPantsType"]));
			TPMCharacterCustomization instance = TPMCharacterCustomization.Instance;
			InventoryItem inventoryItemGameObject = PlayerController.GetInventoryItemGameObject(ItemSubTypes.Hat);
			Gender gender = instance.GetHead(faces).gender;
			Hats hats = ((inventoryItemGameObject == null) ? Hats.None : instance.GetHatByPath(gender, (gender != Gender.Male) ? inventoryItemGameObject.FemaleAsset3rdPerson : inventoryItemGameObject.MaleAsset3rdPerson));
			InventoryItem inventoryItemGameObject2 = PlayerController.GetInventoryItemGameObject(ItemSubTypes.Waistcoat);
			Shirts shirts = ((inventoryItemGameObject2 == null) ? Shirts.Default : instance.GetShirtByPath(gender, (gender != Gender.Male) ? inventoryItemGameObject2.FemaleAsset3rdPerson : inventoryItemGameObject2.MaleAsset3rdPerson));
			TPMCharacterModel tpmcharacterModel = new TPMCharacterModel(faces, hair, pants, hats, shirts, Shoes.BOOTS);
			if (settings.ContainsKey("TpmSkinColorR"))
			{
				float num = Math3d.ParseFloat(settings["TpmSkinColorR"]);
				float num2 = Math3d.ParseFloat(settings["TpmSkinColorG"]);
				float num3 = Math3d.ParseFloat(settings["TpmSkinColorB"]);
				tpmcharacterModel.SetSkinColor(num, num2, num3);
			}
			return tpmcharacterModel;
		}
	}

	private static InventoryItem GetInventoryItemGameObject(ItemSubTypes itemType)
	{
		return PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == itemType);
	}

	private Renderer FindRenderer(Transform root)
	{
		for (int i = 0; i < root.childCount; i++)
		{
			Transform child = root.GetChild(i);
			Renderer component = child.GetComponent<Renderer>();
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}

	public void Set3DViewMode()
	{
		GameFactory.Is3dViewVisible = false;
		this.Switch3DViewVisibility(true);
	}

	public bool IsNewFish { get; set; }

	public bool IsPhotoModeRequested { get; set; }

	public void SwitchPhotoModeFlashlight()
	{
		this._isPHMFlashLightEnabled = !this._isPHMFlashLightEnabled;
	}

	public bool SetupPhotoMode(HandsViewController model)
	{
		bool flag = false;
		if (this.IsSailing && !this.CurrentBoat.IsAnchored)
		{
			this._wasPhotoModeAnchored = true;
			this.CurrentBoat.IsAnchored = true;
		}
		Transform transform = new GameObject("cameraTarget").transform;
		transform.parent = model.Root.parent;
		this._isHandsHoldCondition = this.Tackle.Fish.IsHandsHoldCondition;
		if (this._isHandsHoldCondition && !this.IsSailing)
		{
			Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(base.transform.position, base.transform.position + new Vector3(0f, -3f, 0f), GlobalConsts.FishMask);
			this._isHandsHoldCondition = maskedRayContactPoint != null && maskedRayContactPoint.Value.y > -0.25f;
			flag = !this._isHandsHoldCondition;
		}
		this._phmModel = model;
		this._isPHMFlashLightEnabled = this._flashLight.HandLightEnabled;
		this.UpdatePhotoModel(model);
		if (this._isHandsHoldCondition)
		{
			Fish caughtFish = this.Tackle.Fish.CaughtFish;
			this.CreatePhotoModeFish(caughtFish.FishId, caughtFish.Length);
			if (this.IsSailing)
			{
				transform.position = model.Root.position + new Vector3(0f, 0.4f, 0f);
			}
			else
			{
				transform.position = this.ColliderPosition + new Vector3(0f, -0.75f, 0f);
			}
			this._camera3dView.SetActive(true, transform, this._motor.transform.rotation.eulerAngles.y, true);
		}
		else
		{
			if (this.IsSailing)
			{
				transform.position = model.Root.position + new Vector3(0f, 1.2f, 0f);
			}
			else
			{
				transform.position = this.ColliderPosition + new Vector3(0f, 0.25f, 0f);
			}
			this._camera3dView.SetActive(true, transform, this._motor.transform.rotation.eulerAngles.y, true);
		}
		this.FreezeCamera(true);
		return flag;
	}

	public void LeavePhotoMode()
	{
		if (this._wasPhotoModeAnchored)
		{
			this._wasPhotoModeAnchored = false;
			this.CurrentBoat.IsAnchored = false;
		}
		this._phmModel = null;
		this.SetLabelsVisibility(ShowHudElements.Instance.CurrentState != ShowHudStates.HideAll);
		this.FreezeCamera(false);
		this.ClearRodPodsHighlighting();
	}

	public void UpdatePhotoModeRequest()
	{
		this._updatePhotoModeRequest = true;
	}

	private void CreatePhotoModeFish(int fishId, float fishLength)
	{
		Fish caughtFish = this.Tackle.Fish.CaughtFish;
		caughtFish.FishId = fishId;
		caughtFish.Asset = CacheLibrary.AssetsCache.GetFishAssetPath(fishId);
		caughtFish.Length = fishLength;
		GameObject gameObject = Resources.Load<GameObject>(caughtFish.Asset);
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
		Object.Destroy(gameObject2.GetComponent<FishAnimationController>());
		Animation component = gameObject2.GetComponent<Animation>();
		component.playAutomatically = false;
		component.Play("idle");
		FishController component2 = gameObject2.GetComponent<FishController>();
		component2.SetBehaviour(UserBehaviours.Photo, caughtFish, this.RodSlot, null);
		this._phmModel.SetCaughtFish(component2);
	}

	public void UpdatePhotoModel(HandsViewController model)
	{
		ThirdPersonData photoModeData = this._cache.GetPhotoModeData(this.ColliderPosition, this._motor.transform.rotation, this.CurrentBoat, this._isHandsHoldCondition);
		model.ProcessPhotoModeData(photoModeData, this._isPHMFlashLightEnabled);
	}

	public CameraWetness CameraWetness
	{
		get
		{
			return this.Player.GetComponent<CameraWetness>();
		}
	}

	public void Switch3DViewVisibility(bool couldDisableCamera = true)
	{
		GameFactory.Is3dViewVisible = !GameFactory.Is3dViewVisible;
		this.Update3DViewVisibility(GameFactory.Is3dViewVisible);
		if (this._camera3dView == null)
		{
			this.MainCameraDebugOffset = ((!GameFactory.Is3dViewVisible) ? Vector3.zero : this.cameraIn3dViewPosition);
		}
		else
		{
			if (!this.IsSailing)
			{
				this.CameraController.enabled = !GameFactory.Is3dViewVisible;
				this.CameraController.CameraMouseLook.enabled = !GameFactory.Is3dViewVisible;
			}
			else
			{
				this._currentBoat.CameraSetActive(!GameFactory.Is3dViewVisible);
			}
			if (!GameFactory.Is3dViewVisible && couldDisableCamera)
			{
				this._camera3dView.SetActive(false, null, 0f, false);
			}
		}
		if (GameFactory.Is3dViewVisible)
		{
			this.CameraWetness.DisableCameraDrops();
		}
		else
		{
			this.CameraWetness.EnableCameraDrops();
			this.CameraWetness.DelayedStart();
		}
	}

	public void SetHandsVisibility(bool flag)
	{
		this.FindRenderer(this.Hands.transform).enabled = flag;
		this.FindRenderer(this._sleevesData.transform).enabled = flag;
	}

	public Camera SetCameraRouting()
	{
		this._savedCameraPosition = this.cameraController.Camera.transform.localPosition;
		this._savedCameraRotation = this.cameraController.Camera.transform.localRotation;
		this._savedCameraParent = this.cameraController.Camera.transform.parent;
		this.Update3DViewVisibility(true);
		this.cameraController.Camera.transform.parent = null;
		return this.cameraController.Camera;
	}

	public void ClearCameraRouting()
	{
		this.Update3DViewVisibility(false);
		this.cameraController.Camera.transform.parent = this._savedCameraParent;
		this.cameraController.Camera.transform.localRotation = this._savedCameraRotation;
		this.cameraController.Camera.transform.localPosition = this._savedCameraPosition;
	}

	private void Update3DViewVisibility(bool isFirstPersonHidden)
	{
		this.SetHandsVisibility(!isFirstPersonHidden);
		if (this.Rod != null)
		{
			this.Rod.SetVisibility(!isFirstPersonHidden);
			this.Reel.SetVisibility(!isFirstPersonHidden);
			this.Line.SetVisibility(!isFirstPersonHidden);
			this.Tackle.SetVisibility(!isFirstPersonHidden);
			if (this.Bell != null)
			{
				this.Bell.SetVisibility(!isFirstPersonHidden);
			}
			foreach (IFishController fishController in GameFactory.FishSpawner.Fish.Values)
			{
				fishController.SetVisibility(!isFirstPersonHidden);
			}
			this.Grip.SetGameVisibility(!isFirstPersonHidden && this.Tackle != null && this.Tackle.Fish != null && this.Tackle.Fish.IsBig);
		}
		if (this._curChumBall != null)
		{
			Renderer rendererForObject = RenderersHelper.GetRendererForObject<Renderer>(this._curChumBall.transform);
			if (rendererForObject != null)
			{
				rendererForObject.enabled = !isFirstPersonHidden;
			}
		}
		if (this.IsSailing)
		{
			this.CurrentBoat.SetVisibility(!isFirstPersonHidden);
		}
		if (this._player3dView != null)
		{
			this._player3dView.SetModelVisibility(isFirstPersonHidden);
		}
		this._photoModeLight.enabled = isFirstPersonHidden && FlashLightController.IsDarkTime;
		if (isFirstPersonHidden)
		{
			this._flashLight.PushLightDisabling();
		}
		else
		{
			this._flashLight.PopLightDisabling();
		}
		this._shadow.SetActive(!isFirstPersonHidden);
	}

	public static MeshBakersController GetBaker()
	{
		GameObject gameObject = GameObject.Find("bakers");
		return gameObject.GetComponent<MeshBakersController>();
	}

	private void BakerOnModelCreated(string objId, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, TPMCharacterModel modelSettings, SkinnedMeshRenderer bakedRenderer)
	{
		if (objId == "p3dv")
		{
			PlayerController.GetBaker().EModelReady -= this.BakerOnModelCreated;
			GameObject gameObject = GameObject.Find("Main player 3D view");
			GameObject gameObject2 = Object.Instantiate<GameObject>(TPMCharacterCustomization.Instance.MainPlayerPrefab);
			gameObject2.name = gameObject.name;
			gameObject.transform.parent = gameObject2.transform;
			this._player3dView = gameObject2.GetComponent<HandsViewController>();
			Player player = new Player
			{
				TpmCharacterModel = modelSettings
			};
			this._player3dView.Initialize(null, player, parts, bakedRenderer, false, true, null, true);
			this.Set3DViewMode();
		}
	}

	private void SendCharacterPackage(Package package)
	{
		if (!TournamentHelper.IS_IN_TOURNAMENT && !StaticUserData.IS_IN_TUTORIAL)
		{
			PhotonConnectionFactory.Instance.RaiseUpdateCharacter(package);
		}
	}

	public void SendPlayerPositionAndRotation(bool force = false)
	{
		bool flag = this.Tackle != null && this.Tackle.IsInitialized && this.Tackle.IsInWater;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!force)
		{
			if (!flag)
			{
				Vector3? vector = this.oldColliderPosition;
				if (vector == null || Vector3.Distance(this.ColliderPosition, this.oldColliderPosition.Value) > 1.2f)
				{
					goto IL_F0;
				}
			}
			if (this.ThrowTargetPoint != null)
			{
				Vector3? vector2 = this.oldThrowTargetPoint;
				if (vector2 == null || Vector3.Distance(this.ThrowTargetPoint.Value, this.oldThrowTargetPoint.Value) > 1.2f)
				{
					goto IL_F0;
				}
			}
			if (this.ThrowTargetPoint != null)
			{
				return;
			}
			Vector3? vector3 = this.oldThrowTargetPoint;
			if (vector3 == null)
			{
				return;
			}
		}
		IL_F0:
		this.oldColliderPosition = new Vector3?(this.ColliderPosition);
		this.oldThrowTargetPoint = this.ThrowTargetPoint;
		this.lastTimeSendPlayerPositionAndRotation = realtimeSinceStartup;
		PhotonConnectionFactory.Instance.SendPlayerPositionAndRotation(this.ColliderPosition, this.CameraController.Camera.transform.rotation, this.ThrowTargetPoint);
	}

	private void UpdateFurMovement()
	{
		Vector3 vector;
		vector..ctor(0f, 0f, GameFactory.Player.CameraController.CameraMouseLook.RotationXDelta);
		this.furDisplacement = Vector3.Lerp(-0.003f * vector / Time.deltaTime + 0.35f * SplineBuilder.PerlinVector(Vector3.forward * Time.time, 1f), this.furDisplacement, Mathf.Pow(0.5f, Time.deltaTime * 20f));
		if (this.furDisplacement.magnitude > 1f)
		{
			this.furDisplacement.Normalize();
		}
		this.sleevesFurMaterial.SetVector(this.furDisplacementPropertyID.Value, this.furDisplacement);
	}

	private void InitFurMaterial(GameObject sleevesMeshObject)
	{
		if (sleevesMeshObject == null)
		{
			return;
		}
		SkinnedMeshRenderer component = sleevesMeshObject.GetComponent<SkinnedMeshRenderer>();
		if (component != null && component.sharedMaterials != null && component.sharedMaterials.Length >= 2)
		{
			this.sleevesFurMaterial = component.sharedMaterials[1];
			this.furDisplacementPropertyID = new int?(Shader.PropertyToID("Displacement"));
			if (!this.sleevesFurMaterial.HasProperty(this.furDisplacementPropertyID.Value))
			{
				this.sleevesFurMaterial = null;
			}
		}
	}

	public FishingHandler HudFishingHandler
	{
		get
		{
			return ShowHudElements.Instance.FishingHndl;
		}
	}

	public void SetRodRootLeft()
	{
		this.SetRodRootBone(PlayerController.Hand.Left);
	}

	public void SetRodRootRight()
	{
		this.SetRodRootBone(PlayerController.Hand.Right);
	}

	public Transform GetRodRootBone(PlayerController.Hand hand)
	{
		return this._handsData[(int)((byte)hand)].RodBone;
	}

	private void SetRodRootBone(PlayerController.Hand hand)
	{
		this._curHandType = hand;
		this._takingHandType = this._curHandType;
		Transform transform = this.TakingRod.transform;
		transform.parent = this.CurHand.RodBone;
		transform.localRotation = ((this.TakingRod.ReelType != ReelTypes.Baitcasting) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f));
		transform.localPosition = Vector3.zero;
		this.OnChangeLeftHandRod(this._curHandType == PlayerController.Hand.Left);
	}

	public void SmoothSetRodRootLeft()
	{
		this.SmoothSetRodBone(PlayerController.Hand.Left);
	}

	public void SmoothSetRodRootRight()
	{
		this.SmoothSetRodBone(PlayerController.Hand.Right);
	}

	private void SmoothSetRodBone(PlayerController.Hand hand)
	{
		this._curHandType = hand;
		this._takingHandType = this._curHandType;
		this.OnChangeLeftHandRod(hand == PlayerController.Hand.Left);
		RodController takingRod = this.TakingRod;
		Transform transform = takingRod.transform;
		transform.parent = this.CurHand.RodBone;
		this._rodBoneAdjuster = new PlayerController.SmoothRodBoneAdjustingData(transform.localRotation, (takingRod.ReelType != ReelTypes.Baitcasting) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f), Time.time, 0.5f);
	}

	public void AddIndependentChum(Chum chum, Vector3 position, Vector3 velocity)
	{
		this._independentChumController.AddChumOnPosition(chum, position, velocity);
	}

	public void AddFeeding(Feeding feeding)
	{
		if (feeding.IsExpired)
		{
			return;
		}
		Feeding feeding2 = this.feedingsToSend.FirstOrDefault((Feeding f) => f.ItemId == feeding.ItemId);
		if (feeding2 != null)
		{
			if (feeding2.IsDestroyed)
			{
				Debug.LogError("Chum " + feeding.ItemId + " still updating, but it is going to destroy.");
				PhotonConnectionFactory.Instance.PinError("Chum " + feeding.ItemId + " still updating, but it is going to destroy.", null);
			}
			if (feeding.IsNew)
			{
				Debug.LogError("Chum " + feeding.ItemId + " is claimed to be new, but it is already presented in update set.");
				PhotonConnectionFactory.Instance.PinError("Chum " + feeding.ItemId + " is claimed to be new, but it is already presented in update set.", null);
			}
			if (feeding2.IsNew && feeding.IsDestroyed)
			{
				this.feedingsToSend.Remove(feeding2);
				return;
			}
			if (feeding2.IsUpdated && feeding.IsDestroyed)
			{
				feeding2.IsDestroyed = true;
				return;
			}
			feeding2.Position = feeding.Position;
		}
		else
		{
			this.feedingsToSend.Add(feeding.Clone());
		}
	}

	public void UpdateFeedings()
	{
		if (GameFactory.GameIsPaused)
		{
			return;
		}
		if (Time.time > this.prevUpdateFeedingsTimeStamp + 1f && this.feedingsToSend.Any<Feeding>())
		{
			this.prevUpdateFeedingsTimeStamp = Time.time;
			if (this.feedingsToSend.Any<Feeding>())
			{
				GameActionAdapter.Instance.UpdateFeedings(this.feedingsToSend);
				this.feedingsToSend.Clear();
			}
		}
	}

	public void ThrowTackle()
	{
		if (!this.Tackle.ThrowData.IsOvercasting)
		{
			GameActionAdapter.Instance.Throw(this.Tackle.ThrowData.ThrowForce);
		}
		Vector3 forward = base.transform.forward;
		forward.y += 0.2f;
		Vector3.Normalize(forward);
		this.Tackle.ThrowTackle(forward);
		this.Rod.ResetAppliedForce();
	}

	public void ThrowPitchTackle()
	{
		GameActionAdapter.Instance.Throw(0f);
		Vector3 forward = base.transform.forward;
		forward.y += 0.2f;
		Vector3.Normalize(forward);
		this.Tackle.ThrowTackle(forward);
		this.Rod.ResetAppliedForce();
	}

	public void BeginThrowing()
	{
		this.BeginThrowTime = 0f;
		this.isThrowingV = true;
		this.canThrowValue = false;
		this.CanThrowOvercast = false;
		this.DisableMovement();
	}

	public void FinishThrowing()
	{
		this.isThrowingV = false;
	}

	public bool IsThrowing
	{
		get
		{
			return this.isThrowingV;
		}
	}

	public void BeginThrowPowerGainProcess()
	{
		this.BeginThrowTime = Time.time;
		this.powerGainInProcess = true;
		if (!StaticUserData.IS_IN_TUTORIAL)
		{
			PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Swing, true, null);
		}
	}

	public void FinishThrowPowerGainProcess()
	{
		this.EndThrowTime = Time.time;
		this.powerGainInProcess = false;
	}

	public void FinishThrowPowerGainProcessSwing()
	{
		if (!StaticUserData.IS_IN_TUTORIAL)
		{
			PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Swing, false, null);
		}
	}

	public float GetThrowPowerGainProgress()
	{
		if (this.BeginThrowTime == 0f)
		{
			return 0f;
		}
		float num = ((!this.powerGainInProcess) ? this.EndThrowTime : Time.time);
		return Mathf.Min(1f, (num - this.BeginThrowTime) / 1f);
	}

	public float GetThrowPowerGainProgress(float startSpeedCoef, float finishSpeedCoef)
	{
		if (this.BeginThrowTime == 0f)
		{
			return 0f;
		}
		float num = ((!this.powerGainInProcess) ? this.EndThrowTime : Time.time);
		float num2 = Mathf.Lerp(startSpeedCoef, finishSpeedCoef, (num - this.BeginThrowTime) / 6f);
		return Mathf.Lerp(0f, 1f, (num - this.BeginThrowTime) / 6f * num2);
	}

	public float GetThrowPowerGainProgressBack(float startSpeedCoef, float finishSpeedCoef)
	{
		if (this.BeginThrowTime == 0f)
		{
			return 0f;
		}
		float num = ((!this.powerGainInProcess) ? this.EndThrowTime : Time.time);
		float num2 = Mathf.Lerp(startSpeedCoef, finishSpeedCoef, (num - this.BeginThrowTime) / 6f);
		return Mathf.Lerp(1f, 0f, (num - this.BeginThrowTime) / 6f * num2);
	}

	public void RaiseCanThrow()
	{
		this.canThrowValue = true;
	}

	public void RaiseCanNotThrow()
	{
		this.canThrowValue = false;
	}

	public bool CanThrow
	{
		get
		{
			return this.canThrowValue;
		}
	}

	public void FreezePhysics()
	{
		if (!this.physicsFreezed)
		{
			this.physicsFreezed = true;
			this.PauseAnimation(this.ArmsAnimation);
			this.PauseAnimation(this.ReelAnimation);
		}
	}

	public void UnfreezePhysics()
	{
		if (this.physicsFreezed)
		{
			this.physicsFreezed = false;
			this.ResumeAnimation(this.ArmsAnimation);
			this.ResumeAnimation(this.ReelAnimation);
		}
	}

	public void PauseAnimation(Animation anim)
	{
		if (!this.pausedAnimsSpeed.ContainsKey(anim.name))
		{
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			IEnumerator enumerator = anim.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					AnimationState animationState = (AnimationState)obj;
					dictionary[animationState.name] = animationState.speed;
					animationState.speed = 0f;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			this.pausedAnimsSpeed[anim.name] = dictionary;
		}
		else
		{
			Debug.LogError("PlayerController.PauseAnimation: " + anim.name + " is already paused");
		}
	}

	public void ResumeAnimation(Animation anim)
	{
		if (this.pausedAnimsSpeed.ContainsKey(anim.name))
		{
			Dictionary<string, float> dictionary = this.pausedAnimsSpeed[anim.name];
			IEnumerator enumerator = anim.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					AnimationState animationState = (AnimationState)obj;
					animationState.speed = dictionary[animationState.name];
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			this.pausedAnimsSpeed.Remove(anim.name);
		}
		else
		{
			Debug.LogError("PlayerController.ResumeAnimation: " + anim.name + " is not paused");
		}
	}

	public void StopAnimation(string animName)
	{
		this.ArmsAnimation.Stop(animName);
		if (this.ReelAnimation != null)
		{
			this.ReelAnimation.Stop(animName);
		}
	}

	public void SetAnimationTargetWeight(string animName, float weight, float time, bool automaticallyStopAnimation = false, float animSpeed = 1f)
	{
		if (this.ArmsAnimation[animName] == null)
		{
			return;
		}
		PlayerController.SmoothAnimationWeight smoothAnimationWeight = this._changingAnimations.FirstOrDefault((PlayerController.SmoothAnimationWeight r) => r.Animation == animName);
		if (smoothAnimationWeight == null)
		{
			this._changingAnimations.Add(new PlayerController.SmoothAnimationWeight(animName, weight, time, automaticallyStopAnimation));
		}
		else
		{
			smoothAnimationWeight.Change(weight, time, automaticallyStopAnimation);
		}
	}

	public void ReverseAnimation(string animName)
	{
		this.ArmsAnimation[animName].speed = -this.ArmsAnimation[animName].speed;
	}

	public AnimationState PlayAnimationBlended(string animName, float animSpeed = 1f, float blendWeight = 1f, float blendTime = 1f)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.normalizedTime = (float)((animSpeed <= 0f) ? 1 : 0);
			animationState.speed = animSpeed;
		}
		this.ArmsAnimation.Blend(animName, blendWeight, blendTime);
		if (this.ReelAnimation != null)
		{
			animationState = this.ReelAnimation[animName];
			if (animationState != null)
			{
				animationState.normalizedTime = (float)((animSpeed <= 0f) ? 1 : 0);
				animationState.speed = animSpeed;
				animationState.weight = 0f;
			}
		}
		if (animationState == null)
		{
			Debug.Log("PlayAnimationBlended:" + animName + " - cant find animation");
		}
		return animationState;
	}

	public AnimationState PlayBlendedLeftRoll(string animName, float animSpeed, float blendWeight = 1f, float blendTime = 1f, float startTime = 0f)
	{
		AnimationState animationState;
		if (this.ReelAnimation != null)
		{
			animationState = this.ReelAnimation[animName];
			if (animationState != null)
			{
				animationState.speed = animSpeed;
				animationState.time = startTime;
				animationState.weight = 100f;
			}
			this.ReelAnimation.CrossFade(animName, 0.2f);
		}
		animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.speed = animSpeed;
			animationState.time = startTime;
			animationState.AddMixingTransform(this.blendLeftHand, true);
		}
		this.ArmsAnimation.Blend(animName, blendWeight, blendTime);
		if (animationState == null)
		{
			Debug.Log("PlayBlendedLeftRoll:" + animName + " - cant find animation");
		}
		return animationState;
	}

	public AnimationState PlayBlendedLeftRollNoReel(string animName, float animSpeed, float blendWeight = 1f, float blendTime = 1f, float startTime = 0f)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.speed = animSpeed;
			animationState.time = startTime;
			animationState.AddMixingTransform(this.blendLeftHand, true);
		}
		this.ArmsAnimation.Blend(animName, blendWeight, blendTime);
		if (animationState == null)
		{
			Debug.Log("PlayBlendedLeftRollNoReel:" + animName + " - cant find animation");
		}
		return animationState;
	}

	public AnimationState PlayBlendedRightRoll(string animName, float animSpeed, float blendWeight = 1f, float blendTime = 1f, float startTime = 0f)
	{
		AnimationState animationState;
		if (this.ReelAnimation != null)
		{
			animationState = this.ReelAnimation[animName];
			if (animationState != null)
			{
				animationState.speed = animSpeed;
				animationState.time = startTime;
				animationState.weight = 100f;
			}
			this.ReelAnimation.CrossFade(animName, 0.2f);
		}
		animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.speed = animSpeed;
			animationState.time = startTime;
			animationState.AddMixingTransform(this.blendRightHand, true);
		}
		this.ArmsAnimation.Blend(animName, blendWeight, blendTime);
		if (animationState == null)
		{
			Debug.Log("PlayBlendedRightRoll:" + animName + " - cant find animation");
		}
		return animationState;
	}

	public AnimationState PlayBlendedRightRollNoReel(string animName, float animSpeed, float blendWeight = 1f, float blendTime = 1f, float startTime = 0f)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.speed = animSpeed;
			animationState.time = startTime;
			animationState.AddMixingTransform(this.blendRightHand, true);
		}
		this.ArmsAnimation.Blend(animName, blendWeight, blendTime);
		if (animationState == null)
		{
			Debug.Log("PlayBlendedRightRollNoReel:" + animName + " - cant find animation");
		}
		return animationState;
	}

	public AnimationState PlayHandAnimation(string clipName, float animSpeed = 1f, float time = 0f, float blendTime = 0f)
	{
		AnimationState animationState = this.ArmsAnimation[clipName];
		if (animationState != null)
		{
			this._lastAnimation = clipName;
			if (animSpeed < 0f && time < 0f)
			{
				time = animationState.length;
			}
			animationState.speed = animSpeed;
			animationState.time = time;
			this.ArmsAnimation.CrossFade(clipName, blendTime);
			if (this.CurrentBoat != null && this.CurrentBoat.BoatAnimation != null)
			{
				AnimationState animationState2 = this.CurrentBoat.BoatAnimation[clipName];
				if (animationState2 != null)
				{
					animationState2.speed = animSpeed;
					animationState2.time = time;
					this.CurrentBoat.BoatAnimation.CrossFade(clipName, blendTime);
				}
			}
		}
		else
		{
			LogHelper.Error("Can't find animation {0}", new object[] { clipName });
		}
		return animationState;
	}

	public void UpdateAdditiveHandAnimation(string clipName, float prc)
	{
		bool flag = !Mathf.Approximately(prc, 0f);
		AnimationState animationState = this.ArmsAnimation[clipName];
		if (animationState != null)
		{
			animationState.normalizedTime = prc;
			animationState.weight = 1f;
			animationState.enabled = flag;
			if (this.CurrentBoat != null && this.CurrentBoat.BoatAnimation != null)
			{
				animationState = this.CurrentBoat.BoatAnimation[clipName];
				if (animationState != null)
				{
					animationState.normalizedTime = prc;
					animationState.weight = 1f;
					animationState.enabled = flag;
				}
			}
		}
	}

	public AnimationState BlendHandAnimation(string clipName, float animSpeed = 1f, float time = 0f, float blendTime = 0f)
	{
		if (string.IsNullOrEmpty(clipName))
		{
			return null;
		}
		AnimationState animationState = this.ArmsAnimation[clipName];
		if (animationState != null)
		{
			if (animSpeed < 0f && time < 0f)
			{
				time = animationState.length;
			}
			animationState.speed = animSpeed;
			animationState.time = time;
			animationState.weight = 1f;
			this.ArmsAnimation.Blend(clipName, 1f, blendTime);
			if (this.CurrentBoat != null && this.CurrentBoat.BoatAnimation != null)
			{
				AnimationState animationState2 = this.CurrentBoat.BoatAnimation[clipName];
				if (animationState2 != null)
				{
					animationState2.speed = animSpeed;
					animationState2.time = time;
					animationState2.weight = 1f;
					this.CurrentBoat.BoatAnimation.Blend(clipName, 1f, blendTime);
				}
			}
		}
		return animationState;
	}

	public AnimationState PlayAnimation(string animName, float animSpeed = 1f, float animWeight = 1f, float blendTime = 0f)
	{
		this.OnNewAnimation(animName);
		this._lastAnimation = animName;
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.speed = animSpeed;
			animationState.time = 0f;
			animationState.weight = animWeight;
		}
		this.ArmsAnimation.CrossFade(animName, blendTime);
		if (this.ReelAnimation != null)
		{
			string text = animName;
			if (animName == "empty" || animName == "Walk")
			{
				text = ((this.ReelType != ReelTypes.Baitcasting) ? "Empty" : "BaitEmpty");
			}
			AnimationState animationState2 = this.ReelAnimation[text];
			if (animationState2 != null)
			{
				animationState2.speed = animSpeed;
				animationState2.time = 0f;
				animationState2.weight = animWeight;
				this.ReelAnimation.CrossFade(text, blendTime);
			}
		}
		if (animationState == null)
		{
			DebugUtility.Missing.Trace("PlayAnimation: " + animName + " - cant find animation", new object[0]);
		}
		return animationState;
	}

	public AnimationState PlayAnimationInSeconds(string animName, float animDuration, float animWeight = 1f)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		return (!(animationState != null)) ? null : this.PlayAnimation(animName, animationState.length / animDuration, animWeight, 0f);
	}

	public AnimationState GetAnimation(string animName, float animSpeed = 1f, float blendTime = 0f)
	{
		return this.ArmsAnimation[animName];
	}

	public AnimationState PlayAnimationCheckPlayed(string animName, float animSpeed = 1f, float animWeight = 1f, float blendTime = 0f)
	{
		if (!this.ArmsAnimation.IsPlaying(animName))
		{
			this._lastAnimation = animName;
			AnimationState animationState = this.ArmsAnimation[animName];
			if (animationState != null)
			{
				animationState.speed = animSpeed;
				animationState.time = 0f;
				animationState.weight = animWeight;
			}
			this.ArmsAnimation.CrossFade(animName, blendTime);
			if (this.ReelAnimation != null)
			{
				animationState = this.ReelAnimation[animName];
				if (animationState != null)
				{
					animationState.speed = animSpeed;
					animationState.time = 0f;
					animationState.weight = animWeight;
				}
				this.ReelAnimation.CrossFade(animName, blendTime);
			}
			if (animationState == null)
			{
				Debug.Log("PlayAnimationCheckPlayed:" + animName + " - cant find animation");
			}
			return animationState;
		}
		return null;
	}

	public void SetAnimationSpeed(string animName, float animSpeed)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.speed = animSpeed;
		}
		if (this.ReelAnimation != null)
		{
			animationState = this.ReelAnimation[animName];
			if (animationState != null)
			{
				animationState.speed = animSpeed;
			}
		}
		if (animationState == null)
		{
			Debug.Log("SetAnimationSpeed:" + animName + " - cant find animation");
		}
	}

	public void SetAnimationWeight(string animName, float animWeight)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.weight = animWeight;
		}
	}

	public void SetAnimationWeightWithReel(string animName, float animWeight)
	{
		AnimationState animationState = this.ArmsAnimation[animName];
		if (animationState != null)
		{
			animationState.weight = animWeight;
		}
		if (this.ReelAnimation != null)
		{
			animationState = this.ReelAnimation[animName];
			if (animationState != null)
			{
				animationState.weight = animWeight;
			}
		}
		if (animationState == null)
		{
			Debug.Log("SetAnimationWeightWithReel:" + animName + " - cant find animation");
		}
	}

	public AnimationState PlayReverseAnimation(string animName, float animSpeed = 1f)
	{
		this.OnNewAnimation("Back_" + animName);
		this._lastAnimation = animName;
		AnimationState animationState = this.ArmsAnimation[animName];
		animationState.speed = -animSpeed;
		animationState.time = animationState.length;
		this.ArmsAnimation.CrossFade(animName);
		if (this.ReelAnimation != null)
		{
			AnimationState animationState2 = this.ReelAnimation[animName];
			if (animationState2 != null)
			{
				animationState2.speed = -animSpeed;
				animationState2.time = animationState2.length;
				this.ReelAnimation.CrossFade(animName);
			}
		}
		return animationState;
	}

	public bool IsCameraZoomed
	{
		get
		{
			return this._isCameraZoomed;
		}
	}

	public float TargetFov { get; set; }

	public void ZoomCamera(bool flag)
	{
		this._isCameraZoomed = flag;
		this.TargetFov = ((!flag) ? 60f : 35f);
	}

	internal void CreateAndActivateTarget()
	{
		GameObject gameObject = (GameObject)Resources.Load("HUD/TargetSpot/Prefabs/TargetMark", typeof(GameObject));
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
		this.UpdateTarget();
		Quaternion quaternion = default(Quaternion);
		quaternion.eulerAngles = new Vector3(0f, 0f, 0f);
		Quaternion quaternion2 = quaternion;
		gameObject2.transform.localRotation = quaternion2;
		gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
		this.targetObject = gameObject2.GetComponent<TargetHandler>();
		this.ZoomCamera(true);
	}

	public Vector3? ThrowTargetPoint { get; set; }

	public Vector3 CameraForward
	{
		get
		{
			return this.CameraController.Camera.transform.forward;
		}
	}

	internal void UpdateTarget()
	{
		if (this.targetObject == null)
		{
			return;
		}
		Vector3 position = this.CameraController.Camera.transform.position;
		Vector3 vector = this.CameraController.Camera.transform.forward;
		if (this.IsSailing)
		{
			vector = Quaternion.AngleAxis(this._boatTargetAddAngle, this.CameraController.Camera.transform.right) * vector;
		}
		Vector3 vector2 = position + 500f * vector;
		RaycastHit raycastHit;
		Physics.Linecast(position, vector2, ref raycastHit, 273);
		if (raycastHit.collider == null)
		{
			this.ThrowTargetPoint = null;
			this.targetObject.SetActive(false);
			return;
		}
		this.targetObject.SetActive(true);
		Vector3 vector3 = raycastHit.point;
		Vector3 vector4;
		vector4..ctor(base.transform.position.x, 0f, base.transform.position.z);
		float num = (vector3 - vector4).magnitude;
		bool flag = raycastHit.collider.gameObject.layer == 4;
		if (num > this.Rod.MaxCastLength)
		{
			num = this.Rod.MaxCastLength;
			Vector3 vector5;
			vector5..ctor(vector.x, 0f, vector.z);
			Vector3 normalized = vector5.normalized;
			Vector3 vector6 = vector4 + num * normalized;
			Vector3 normalized2 = (vector6 - position).normalized;
			Physics.Linecast(position, position + 500f * normalized2, ref raycastHit, 273);
			vector3 = raycastHit.point;
			flag = raycastHit.collider.gameObject.layer == 4;
		}
		this.targetObject.SetType(flag);
		this.targetObject.transform.localPosition = vector3;
		this.ThrowTargetPoint = new Vector3?(vector3);
		this.targetObject.Value = (float)((double)(StaticUserData.RodInHand.Rod.Action / 40f) + 0.025) * num;
		GameFactory.Player.HudFishingHandler.CastTargetHandler.MaxValue = this.Rod.MaxCastLength;
		GameFactory.Player.HudFishingHandler.CastTargetHandler.TargetValue = num;
	}

	internal void DestroyTarget(bool resetTargetPoint = true)
	{
		if (resetTargetPoint)
		{
			this.ThrowTargetPoint = null;
		}
		this.targetObject.SetActive(false);
		Object.Destroy(this.targetObject.gameObject);
		this.ZoomCamera(false);
	}

	internal void DestroyObject(GameObject destroyObject)
	{
		if (destroyObject != null)
		{
			destroyObject.SetActive(false);
			Object.Destroy(destroyObject);
		}
	}

	private void OnGameActionResult(GameActionResult actionResult)
	{
		this.CanThrowOvercast = true;
	}

	public void UpdateIndicator()
	{
		if (ShowHudElements.Instance == null || this.HudFishingHandler == null)
		{
			Debug.LogError("PlayerControllers::UpdateIndicator() Hud is null, wtf???");
			return;
		}
		if (!this.lurePositionHandler)
		{
			this.lurePositionHandler = this.HudFishingHandler.LurePositionHandlerContinuous;
		}
		if (this.lurePositionHandler == null)
		{
			Debug.LogError("PlayerControllers::UpdateIndicator() lurePositionHandler is null, wtf???");
			return;
		}
		if (SettingsManager.FishingIndicator && this.Tackle != null && string.IsNullOrEmpty(this.Tackle.UnderwaterItemName) && !StaticUserData.RodInHand.IsRodDisassembled && !this.Tackle.IsHitched && this.Tackle.IsInWater && !this.Tackle.IsFishHooked && this.Tackle is LureBehaviour)
		{
			this.lurePositionHandler.gameObject.SetActive(true);
		}
		else
		{
			this.lurePositionHandler.gameObject.SetActive(false);
		}
	}

	public bool ResetZoneEnabler { get; set; }

	public bool CanMovement { get; set; }

	public bool IsTackleThrown
	{
		get
		{
			return this._isTackleThrown;
		}
	}

	public bool WithAnyRod
	{
		get
		{
			return this.Rod != null;
		}
	}

	public bool IsCatchedSomething
	{
		get
		{
			return (ShowHudElements.Instance != null && ShowHudElements.Instance.IsCatchedWindowActive) || this.State == typeof(PlayerShowFishLineIn) || (this.Tackle != null && this.Tackle.Fish != null && (this.Tackle.Fish.State == typeof(FishShowSmall) || this.Tackle.Fish.State == typeof(FishShowBig)));
		}
	}

	public bool IsInteractionWithRodStand
	{
		get
		{
			return this.State == typeof(TakeRodFromPodIn) || this.State == typeof(TakeRodFromPodOut) || this.State == typeof(ReplaceRodOnPodLean) || this.State == typeof(ReplaceRodOnPodTakeAndPut) || this.State == typeof(ReplaceRodOnPodOut) || this.State == typeof(PutRodOnPodIn) || this.State == typeof(PutRodOnPodOut);
		}
	}

	public bool IsIdle
	{
		get
		{
			return this.State == typeof(PlayerIdlePitch) || this.State == typeof(PlayerIdle);
		}
	}

	public void StopThrowState()
	{
		if (this.IsReadyForRod)
		{
			ControlsController.ControlsActions.SetInFishingZoneMappings();
			ControlsController.ControlsActions.SetRodSpecificInFishingZoneMappings(this.ReelType == ReelTypes.Spinning);
		}
		else if (!this.IsSailing)
		{
			ControlsController.ControlsActions.SetNotInFishingZoneMappings();
		}
		this._isTrollingRodThrown = false;
		this._isTackleThrown = false;
		this._reelIkController.enabled = false;
		if (this._catchingWalls != null)
		{
			if (!PhotonConnectionFactory.Instance.IsFreeRoamingOn || StaticUserData.IS_IN_TUTORIAL)
			{
				this._catchingWalls.SetActive(false);
			}
			this._fpController.SetCatchingMode(false);
		}
		this.EnableMovement();
		this._motor.SetControllable(true);
		this.UpdateThrownFlag(false);
		if (this._boatsWithFinishedRent.Count > 0)
		{
			this.ShowExtendRentDialog();
		}
		if (this.Bell != null)
		{
			this.Bell.Voice(false);
			this.Bell.Show(true);
		}
	}

	public void StartThrowState()
	{
		this.RodSlot.RestoreReelClip();
		this._isTackleThrown = true;
		this._reelIkController.enabled = true;
		if (this._catchingWalls != null)
		{
			if (!PhotonConnectionFactory.Instance.IsFreeRoamingOn || StaticUserData.IS_IN_TUTORIAL)
			{
				this._catchingWalls.SetActive(true);
			}
			this._fpController.SetCatchingMode(true);
		}
		else
		{
			this.DisableMovement();
		}
		this.UpdateThrownFlag(true);
	}

	public float ReelingPos { get; set; }

	public void UpdateThrownFlag(bool flag)
	{
		if (flag)
		{
			this.ReelingPos = 0f;
		}
		if (StaticUserData.IS_TPM_ENABLED)
		{
			this._cache.CurFraction.isTackleThrown = flag;
		}
	}

	public void EnableMovement()
	{
		if (!this.IsSailing)
		{
			this._fpController.SetControllable(true);
		}
	}

	public void DisableMovement()
	{
		this._fpController.SetControllable(false);
	}

	private void OnMoved(TravelDestination destination)
	{
		if (StaticUserData.CurrentLocation != null)
		{
			PhotonConnectionFactory.Instance.Game.Resume(true);
		}
	}

	private void RecreateAllBoats()
	{
		this.CurrentBoat = null;
		if (GameFactory.BoatDock != null)
		{
			GameFactory.BoatDock.RecreateAllBoats();
		}
	}

	public PlayerSpeedParameters PlayerSpeedParameters { get; private set; }

	private void ActivateRod(bool flag)
	{
		if (this.Tackle != null)
		{
			this.Tackle.SetActive(flag);
		}
		if (this.Line != null)
		{
			this.Line.SetActive(flag);
		}
	}

	public InventoryItem RequestedFireworkItem { get; private set; }

	public ToolController CurFirework { get; private set; }

	public List<InventoryItem> FireWorkItems
	{
		get
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem x) => x.ItemSubType == ItemSubTypes.Firework && x.Storage == StoragePlaces.Equipment).ToList<InventoryItem>();
		}
	}

	private void CreateFirework(InventoryItem itemToCreate)
	{
		this.RequestedFireworkItem = null;
		this.UpdateFirework(itemToCreate.ItemId);
		GameObject gameObject = Resources.Load<GameObject>(itemToCreate.Asset);
		if (gameObject == null)
		{
			throw new PrefabException(string.Format("firework: {0} prefab can't instantiate", itemToCreate.Asset));
		}
		this.CurFirework = Object.Instantiate<GameObject>(gameObject).GetComponent<ToolController>();
		this.CurFirework.Item = itemToCreate;
		this.CurFirework.transform.parent = this.RodRoot.transform.parent;
		this.CurFirework.transform.localRotation = Quaternion.identity;
		this.CurFirework.transform.localPosition = Vector3.zero;
	}

	public void ReplaceFireWork(InventoryItem item)
	{
		this.RequestedFireworkItem = item;
	}

	public bool RequestFirework()
	{
		if (this.IsSailing || GameFactory.IsRodAssembling)
		{
			return false;
		}
		List<InventoryItem> fireWorkItems = this.FireWorkItems;
		if (fireWorkItems.Count > 0)
		{
			this.RequestedFireworkItem = fireWorkItems[0];
			return true;
		}
		return false;
	}

	public bool CanPutFirework()
	{
		RaycastHit raycastHit;
		Physics.Raycast(base.transform.position + Math3d.ProjectOXZ(base.transform.forward) * 1.4f, Vector3.down, ref raycastHit, 10f, GlobalConsts.GroundObstacleMask);
		if (raycastHit.point.y >= 0f)
		{
			return true;
		}
		GameFactory.Message.ShowCantSetupFirework();
		return false;
	}

	public bool RequestNextFirework()
	{
		List<InventoryItem> fireWorkItems = this.FireWorkItems;
		if (fireWorkItems.Count < 2)
		{
			return false;
		}
		int num = fireWorkItems.IndexOf(this.CurFirework.Item) + 1;
		if (num == fireWorkItems.Count)
		{
			num = 0;
		}
		this.RequestedFireworkItem = fireWorkItems[num];
		return true;
	}

	public bool RequestPrevFirework()
	{
		List<InventoryItem> fireWorkItems = this.FireWorkItems;
		if (fireWorkItems.Count < 2)
		{
			return false;
		}
		int num = fireWorkItems.IndexOf(this.CurFirework.Item) - 1;
		if (num < 0)
		{
			num = fireWorkItems.Count - 1;
		}
		this.RequestedFireworkItem = fireWorkItems[num];
		return true;
	}

	public void DestroyFirework()
	{
		if (this.CurFirework != null)
		{
			this.DestroyObject(this.CurFirework.gameObject);
			this.CurFirework = null;
		}
	}

	public void PutFirework()
	{
		if (this.CurFirework.Item.Count > 1)
		{
			this.RequestedFireworkItem = this.CurFirework.Item;
		}
		else if (this.FireWorkItems.Count > 1)
		{
			this.RequestNextFirework();
		}
		this.CurFirework.PutOnTheGround();
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event PlayerController.ActivateTargetDelegate OnActivateTarget;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event PlayerController.UpdateTargetLocalPosDelegate OnUpdateTargetLocalPos;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event PlayerController.DeactivateTargetDelegate OnDeactivateTarget;

	private Vector3 CurRodLocalPoint
	{
		get
		{
			return this.rodLocalPoints[(this.ReelType != ReelTypes.Baitcasting) ? 0 : 1];
		}
	}

	public void EUpdateTargetState(bool flag)
	{
		if (flag)
		{
			this.OnActivateTarget(PlayerTargetType.ROD, this.RodObject.transform, this.CurRodLocalPoint);
		}
		else
		{
			this.OnDeactivateTarget(PlayerTargetType.ROD);
		}
	}

	public void EUpdateRodLocalPos()
	{
		this.OnUpdateTargetLocalPos(PlayerTargetType.ROD, this.CurRodLocalPoint);
	}

	public void InitTransitionToZeroXRotation()
	{
		if (this.IsBoatFishing)
		{
			this.CurrentBoat.InitTransitionToZeroXRotation();
		}
		else
		{
			this.CameraController.InitTransitionToZeroXRotation(this.CameraController.CameraMouseLook);
		}
	}

	public void TransitToZeroXRotation()
	{
		if (this.IsBoatFishing)
		{
			this.CurrentBoat.TransitToZeroXRotation();
		}
		else
		{
			this.CameraController.TransitToZeroXRotation();
		}
	}

	public void FinalizeTransitionToZeroXRotation()
	{
		if (this.IsBoatFishing)
		{
			this.CurrentBoat.FinalizeTransitionToZeroXRotation();
		}
		else
		{
			this.CameraController.FinalizeTransitionToZeroXRotation();
		}
	}

	public bool IsTransitionMapClosing { get; set; }

	public void OnActiveState(Type getType, bool isActive)
	{
		this.OnPlayerActiveState(getType, isActive);
	}

	public void CameraRoutingTurnOn()
	{
		LogHelper.Log("___kocha CameraRouting: Turn On");
		this.fsm.EnterState(typeof(PlayerCameraRouting), null);
	}

	public bool CanBreakStateMachine
	{
		get
		{
			return this.fsm == null || (!this.CantOpenInventory && !(this.fsm.CurrentState as PlayerStateBase).CantOpenInventory && this.fsm.CurrentStateType != typeof(ShowMap) && this.fsm.CurrentStateType != typeof(RodPodIdle) && this.fsm.CurrentStateType != typeof(ToolIdle));
		}
	}

	public void CameraRoutingTurnOff()
	{
		LogHelper.Log("___kocha CameraRouting: Turn Off");
		this.LeaveCameraRouting = true;
		if (this.fsm != null)
		{
			this.fsm.Update();
		}
	}

	private readonly Dictionary<int, Vector3> _initThrownPos = new Dictionary<int, Vector3>();

	private Dictionary<int, Vector3> _catchedFishPos = new Dictionary<int, Vector3>();

	private int _lastCatchedFishSlot;

	[SerializeField]
	private WeatherController _weather;

	private Dictionary<int, RodSessionSettings> _rodsSavedSettings = new Dictionary<int, RodSessionSettings>();

	[SerializeField]
	private Collider _zonesCollider;

	private const string SPIN_REEL_IK_WEIGTH_CURVE_NAME = "IKWeightCurve";

	private const string CASTING_REEL_IK_WEIGTH_CURVE_NAME = "IKWeightCurveCasting";

	private AnimationCurves _ikCurves;

	private LimbIK _reelIkController;

	private const float LINE_LENGTH_TO_REMOVE_HANDS_TILT = 10f;

	private const float MinStrikeSpeed = 12f;

	private const float MinPoolSpeed = 0.4f;

	private bool _isRequestedRodDestroying;

	private bool _isRodActive;

	private IBoatController _currentBoat;

	public bool IsPutTrollingRod;

	public bool _isTrollingRodThrown;

	public GameObject Player;

	public GameObject Hands;

	public Transform Arms;

	public Transform Sleeves;

	[SerializeField]
	private GameObject _pMap;

	private GameObject _map;

	public CameraController cameraController;

	public GameObject RodRoot;

	public GameObject RodLeftRoot;

	[SerializeField]
	private Transform _root;

	public Transform linePositionInRightHand;

	public Transform linePositionInLeftHand;

	public Transform blendLeftHand;

	public Transform blendRightHand;

	[SerializeField]
	private SkinnedMeshRenderer _handsRenderer;

	[SerializeField]
	private PlayerTargetCloser _targetCloser;

	private Material sleevesFurMaterial;

	private Transform[] _bonesCache;

	private Dictionary<string, Transform> _srcBonesMap;

	protected SleevesHelper _sleevesData;

	private string _sleevesPrefabName;

	protected float pullAngle = -40f;

	protected float pullSideAngle;

	protected float pullSpeed = 1f;

	protected float pullMaxAngle = -40f;

	protected float oldRotationalAngel;

	internal TackleThrowData throwData;

	private ReelTypes _reelType;

	[HideInInspector]
	public CastTypes CastType = CastTypes.Twohands;

	[HideInInspector]
	private float _angleOfRod;

	[HideInInspector]
	public float throwFinishTime;

	[HideInInspector]
	public float localHandsOffset = 0.15f;

	private float _m_moveddown;

	public Vector3 ToolsOffset;

	private float _m_bowdown;

	[SerializeField]
	private float _debugBobberStartForce = 0.1f;

	[SerializeField]
	private float _debugBobberEndForce = 0.085f;

	[SerializeField]
	private float _debugBobberForceApplyTimePrc = 0.5f;

	[SerializeField]
	private float _debugBobberForceReleaseTimePrc = 0.5f;

	[SerializeField]
	private float _debugBobberDuration = 1f;

	[SerializeField]
	private float _debugBobberDisturbSize = 0.25f;

	private Fsm<PlayerController> fsm;

	private float Speed = 12f;

	private float speedLengthAjust = 1f;

	private const float maxSpeed = 12f;

	private Quaternion lastRotation;

	private ValueGainTrigger pullValue;

	private ValueGainTrigger isHookedValue;

	private ValueChanger strikeValue;

	private Vector3 _currentDynWaterPosition = new Vector3(0f, -100f, 0f);

	private const float _distanceChangesDynWater = 5f;

	private bool strikeFlag;

	private bool pullFlag;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private GameObject _rodLocator;

	public const float ThrowSoundVolume = 0.05f;

	internal RandomSounds ThrowSounds;

	internal Animation ArmsAnimation;

	internal Animation ReelAnimation;

	internal Rod RequestedRod;

	internal bool NewRodFastChange;

	private bool _m_showSleevesFlag;

	private int? furDisplacementPropertyID;

	public Vector3 furDisplacement;

	private const float FurDisplacementMovementWeight = 0.003f;

	private const float FurDisplacementNoiseWeight = 0.35f;

	private const float FurDisplacementDampingMultiplier = 20f;

	private BoatRentController _boatRentController;

	public bool HasReel = true;

	private QueueVector3Filter _dataShiftQ = new QueueVector3Filter(14);

	private QueueFloatFilter _dataRotQ = new QueueFloatFilter(14);

	private QueueFloatFilter _dataSpeedQ = new QueueFloatFilter(7);

	private FightHandler fightHandler;

	private DamageHandler damageHandler;

	private LurePositionHandlerContinuous lurePositionHandler;

	private GameObject _catchingWalls;

	private Transform _lastPin;

	public const float playerHeigh = 1.6f;

	public Vector3 BoardingFromLocalCameraPosition;

	public Vector3 BoardingToLocalCameraPosition;

	private readonly Type[] _canLeaveBoatStates = new Type[]
	{
		typeof(PlayerIdle),
		typeof(PlayerEmpty),
		typeof(PlayerIdlePitch),
		typeof(PlayerOnBoat),
		typeof(HandIdle)
	};

	private Transform _unboardingObject;

	private bool _finishUnboardingRequest;

	private Action _fishingToDrivingAction;

	private HashSet<GameObject> enteredFishingZones = new HashSet<GameObject>();

	private bool _isBoatMapClosing;

	private bool _isLookWithFishMode;

	private TPMSenderDataCache _cache;

	private TPMCharactersController _tpmCharacters;

	private bool _isReadyForBoarding;

	private ItemSubTypes _boardingCategory;

	private IBoatController _selectedBoat;

	private int _rentWaitingCategory = -1;

	private readonly List<RodPodController> _rodPods = new List<RodPodController>();

	private readonly List<RodPodController> _savedRodPods = new List<RodPodController>();

	private RodPodController _curRodPod;

	private RodPodController _lookAtPod;

	private int _lookAtPodSlot = -1;

	private const float DIST_WHERE_PUT_ROD_POD = 1f;

	public const float MAX_DIST_TO_INTERACT_WITH_POD = 5f;

	private RodPodController _rodPodToPickUp;

	private RodPodController _rodPodToPutRod;

	private int _rodPodSlotToPutRod;

	public Vector3? SavedPosition;

	private float _rodPodInteractionHeight;

	[SerializeField]
	private FABRIK _leftIK;

	[SerializeField]
	private FABRIK _rightIK;

	private PlayerController.HandData[] _handsData = new PlayerController.HandData[2];

	private PlayerController.Hand _curHandType = PlayerController.Hand.Right;

	private PlayerController.Hand _takingHandType = PlayerController.Hand.Right;

	private PodSlotData? _takingRodSlot;

	private bool _wasLastRodCasting;

	private int _rodSlotToTakeFromPod = -1;

	private RodPodController _rodPodToTakeRod;

	private bool _isPositionAdjusted;

	public Action<AssembledRod> SwitchRodInitialized = delegate
	{
	};

	[SerializeField]
	private ChumBall _pChumBall;

	[SerializeField]
	private ChumBall _pSnowBall;

	private ChumBall _curChumBall;

	[SerializeField]
	private float _chumLaunchDelay = 0.32f;

	private GameObject _pPatchAnimationsList;

	private List<FishBoxActivity> _fishBoxActivityZones = new List<FishBoxActivity>();

	private HandsShadowController _shadow;

	[SerializeField]
	private ReplayPlayerUI _pReplayPlayer;

	private Light _photoModeLight;

	private FlashLightController _flashLight;

	private int _fastUseRodPodSlotId = -1;

	private readonly List<Type> _fastUseRodPodStates = new List<Type>
	{
		typeof(PlayerShowFishOut),
		typeof(PlayerShowFishLineOut),
		typeof(PlayerIdleThrownToIdle),
		typeof(HandLoadingIdle),
		typeof(FeederLoadingIdle)
	};

	private readonly List<Type> _selectRodReadyStates = new List<Type>
	{
		typeof(PlayerIdle),
		typeof(PlayerEmpty),
		typeof(PlayerIdlePitch),
		typeof(RodPodIdle),
		typeof(ToolIdle),
		typeof(HandIdle)
	};

	public bool ImmediateBoarding;

	private bool _refreshBoatsRequest;

	private List<short> _boatsWithFinishedRent = new List<short>();

	private const int MIXING_ANIMATIONS_LAYER = 1;

	private Dictionary<BrokenTackleType, Action> _brokenItemActions;

	private bool pullTriggered;

	private float angleAccum;

	public float handsDamper;

	public float handsWithFishDamper;

	private bool _isPlayer3dViewRequested;

	private HandsViewController _player3dView;

	[SerializeField]
	private Vector3 cameraIn3dViewPosition;

	[SerializeField]
	private DebugCameraControler _camera3dView;

	[SerializeField]
	private OperatorCamera _replayCamera;

	[SerializeField]
	private bool _isCharacterFromProfile;

	public const string DEBUG_MODEL_ID = "p3dv";

	[SerializeField]
	private bool _letSitToTheWater;

	private bool _isHandsHoldCondition;

	private HandsViewController _phmModel;

	private bool _updatePhotoModeRequest;

	private bool _wasPhotoModeAnchored;

	private bool _isPHMFlashLightEnabled;

	private Vector3 _savedCameraPosition;

	private Quaternion _savedCameraRotation;

	private Transform _savedCameraParent;

	private float lastTimeSendPlayerPositionAndRotation;

	private Vector3? oldColliderPosition;

	private Vector3? oldThrowTargetPoint;

	private const float delta = 1.2f;

	public Quaternion TakenRodRotation;

	private PlayerController.SmoothRodBoneAdjustingData _rodBoneAdjuster;

	private float prevUpdateFeedingsTimeStamp;

	private const float MinFeedingUpdateTimeout = 1f;

	private List<Feeding> feedingsToSend;

	private WaterChumController _independentChumController;

	internal float BeginThrowTime;

	internal float EndThrowTime;

	internal bool CanThrowOvercast;

	private bool powerGainInProcess;

	private bool canThrowValue;

	private bool isThrowingV;

	private const float GainPowerTime = 6f;

	private const float SimpleGainPowerTime = 1f;

	public bool DebugFreezePhysics;

	private bool physicsFreezed;

	private Dictionary<string, Dictionary<string, float>> pausedAnimsSpeed;

	private List<PlayerController.SmoothAnimationWeight> _changingAnimations;

	private string _lastAnimation;

	private TargetHandler targetObject;

	private const float MAX_CAST_DIST = 500f;

	private const int CAST_MASK = 273;

	private const float ZOOMED_FOV = 35f;

	private const float NORMAL_FOV = 60f;

	private bool _isCameraZoomed;

	[SerializeField]
	private float _boatTargetAddAngle;

	private bool _isTackleThrown;

	private CharacterMotorCS _motor;

	private FirstPersonControllerFP _fpController;

	public Vector3[] rodLocalPoints;

	public bool LeaveCameraRouting;

	public class HandData
	{
		public HandData(Transform rodBone, FABRIK ik)
		{
			this.RodBone = rodBone;
			this.IK = ik;
		}

		public readonly Transform RodBone;

		public readonly FABRIK IK;
	}

	public enum Hand
	{
		Left,
		Right
	}

	private struct SmoothRodBoneAdjustingData
	{
		public SmoothRodBoneAdjustingData(Quaternion fromRotation, Quaternion toRotation, float startedAt, float duration)
		{
			this = default(PlayerController.SmoothRodBoneAdjustingData);
			this.FromRotation = fromRotation;
			this.ToRotation = toRotation;
			this.StartedAt = startedAt;
			this.Duration = duration;
		}

		public PlayerController.SmoothRodBoneAdjustingData Clear()
		{
			return default(PlayerController.SmoothRodBoneAdjustingData);
		}

		public readonly Quaternion FromRotation;

		public readonly Quaternion ToRotation;

		public readonly float StartedAt;

		public readonly float Duration;
	}

	private class SmoothAnimationWeight
	{
		public SmoothAnimationWeight(string animation, float weight, float time, bool automaticallyStopAnimation = false)
		{
			this._animation = animation;
			this.Change(weight, time, automaticallyStopAnimation);
		}

		public string Animation
		{
			get
			{
				return this._animation;
			}
		}

		public void Change(float weight, float time, bool automaticallyStopAnimation)
		{
			this._startTime = Time.time;
			this._startWeight = PlayerController.SmoothAnimationWeight.ArmsAnimation[this._animation].weight;
			this._targetWeight = weight;
			this._duration = time;
			this._automaticallyStopAnimation = automaticallyStopAnimation && Mathf.Approximately(this._targetWeight, 0f);
		}

		public bool Update()
		{
			float num = Time.time - this._startTime;
			if (num >= this._duration)
			{
				if (this._automaticallyStopAnimation)
				{
					PlayerController.SmoothAnimationWeight.ArmsAnimation.Stop(this._animation);
				}
				return true;
			}
			float num2 = num / this._duration;
			float num3 = this._startWeight * (1f - num2) + this._targetWeight * num2;
			PlayerController.SmoothAnimationWeight.ArmsAnimation[this._animation].weight = num3;
			return false;
		}

		public static Animation ArmsAnimation;

		private string _animation;

		private float _startWeight;

		private float _targetWeight;

		private float _startTime;

		private float _duration;

		private bool _automaticallyStopAnimation;
	}

	public delegate void ActivateTargetDelegate(PlayerTargetType targetType, Transform rodTransform, Vector3 rodLocalPoint);

	public delegate void UpdateTargetLocalPosDelegate(PlayerTargetType targetType, Vector3 newLocalPoint);

	public delegate void DeactivateTargetDelegate(PlayerTargetType targetType);
}
