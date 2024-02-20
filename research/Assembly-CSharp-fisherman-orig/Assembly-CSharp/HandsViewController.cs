using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using RootMotion;
using RootMotion.FinalIK;
using TPM;
using UnityEngine;

public class HandsViewController : Player3dBehaviourController<RodAnimator>, IPositionCorrectors
{
	private Vector3 Position
	{
		get
		{
			return this._ikPartsRoot.transform.parent.position;
		}
	}

	public Transform Root
	{
		get
		{
			return this._ikPartsRoot.transform.parent;
		}
	}

	public Transform Bobber
	{
		get
		{
			return this._curRod.CurrentBobber;
		}
	}

	public Transform Boat
	{
		get
		{
			return this._curRod.CurrentBoat;
		}
	}

	public Transform Fish
	{
		get
		{
			return this._curRod.CurrentFish;
		}
	}

	public Vector3 ModelGroundCorrection
	{
		get
		{
			return (!this._boolParameters[7]) ? this.MODEL_GROUND_CORRECTION : this.BOAT_GROUND_CORRECTION;
		}
	}

	public virtual Vector3 DebugModelUpMovement
	{
		get
		{
			return Vector3.zero;
		}
	}

	public virtual Vector3 DebugHandsUpMovement
	{
		get
		{
			return Vector3.zero;
		}
	}

	public virtual float DebugForwardDist
	{
		get
		{
			return 0f;
		}
	}

	public Vector3 Forward
	{
		get
		{
			return this._charMovementAnimator.transform.forward;
		}
	}

	public Transform SceletonPartTransform
	{
		get
		{
			return this._charMovementAnimator.transform;
		}
	}

	public Vector3 PhotoModeFloatingLineDisplacement
	{
		get
		{
			return this._photoModeFloatingLineDisplacement;
		}
	}

	public Vector3 PhotoModeSpinningLineDisplacement
	{
		get
		{
			return this._photoModeSpinningLineDisplacement;
		}
	}

	public Vector3 PhotoModeCastingLineDisplacement
	{
		get
		{
			return this._photoModeCastingLineDisplacement;
		}
	}

	private float DistToDisappier
	{
		get
		{
			if (GameFactory.Player.IsSailing && this._boolParameters[7])
			{
				return this._distToDisappierWhenSailing;
			}
			return (!SettingsManager.ShowCharacterBubble) ? this._distToDisappier : 0.6f;
		}
	}

	private float DistToShow
	{
		get
		{
			if (GameFactory.Player.IsSailing && this._boolParameters[7])
			{
				return this._distToShowWhenSailing;
			}
			return (!SettingsManager.ShowCharacterBubble) ? this._distToShow : 0.75f;
		}
	}

	protected override string PlayerName
	{
		get
		{
			return this._player.UserName;
		}
	}

	public string UserId
	{
		get
		{
			return (this._player != null) ? this._player.UserId : string.Empty;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPlayerActive = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> ETargetFrameReached = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> EFrameUpdate = delegate
	{
	};

	private ReplayEngine PlaybackControll
	{
		get
		{
			return this._cache as ReplayEngine;
		}
	}

	public int ReplayFramesCount
	{
		get
		{
			return (this.PlaybackControll == null) ? 0 : this.PlaybackControll.FramesCount;
		}
	}

	public int ReplayCurrentFrame
	{
		get
		{
			return (this.PlaybackControll == null) ? 0 : this.PlaybackControll.CurrentFrame;
		}
	}

	public void Initialize(Camera playerCamera, IPlayer player, Dictionary<CustomizedParts, TPMModelLayerSettings> constructedParts, SkinnedMeshRenderer bakedRenderer, bool labelsVisibility, bool isForceVisibilityMode = false, Transform lookAtTarget = null, bool isLightConeVisible = true)
	{
		this._isModelVisible = true;
		if (player.IsReplay)
		{
			this._cache = new ReplayEngine();
			this.PlaybackControll.EPlaybackFinished += this.OnPlaybackFinished;
			this.PlaybackControll.ETargetFrameReached += this.OnTargetFrameReached;
		}
		else
		{
			this._cache = new TPMRecieverDataCache(this._minCacheSize);
		}
		this._isForceVisibilityMode = isForceVisibilityMode || player.IsReplay;
		this._labelsVisibility = labelsVisibility && !player.IsReplay;
		this._player = player;
		this._playerCamera = playerCamera;
		if (lookAtTarget != null)
		{
			this._ikTarget = lookAtTarget;
		}
		TPMSetupIKModel.CreateIkParts(this._ikPartsRoot, player.TpmCharacterModel, this._ikTarget, constructedParts, false);
		Transform transform = constructedParts[MeshBakersController.SKELETON_SRC_PART].transform;
		this._collidersSystem = this._ikPartsRoot.AddComponent<TPMColliders>();
		this._collidersSystem.Init(transform);
		Transform parent = transform.parent;
		this._charMovementAnimator = parent.GetComponent<TPMPositionDrivenAnimator>();
		this._charMovementAnimator.enabled = true;
		this._charMovementAnimator.Init(this._isControlTarget, transform, this._bodyDefaultWeight, this._headDefaultWeight, this._headClamp, this, false);
		base.RegisterNewAnimator(new ObjWithAnimator(new AnimatedObject
		{
			obj = parent.gameObject,
			isAllTimeActiveObject = true,
			isLogEnabled = this._isAnimatorLogEnabled,
			layersCount = 2,
			syncLayerIndex = 1
		}));
		TPMFullIKDebugSettings iksettings = TPMCharacterCustomization.Instance.IKSettings;
		this._leftFoot = transform.Find(iksettings.bodyIKSettings.leftFoot);
		this._rightFoot = transform.Find(iksettings.bodyIKSettings.rightFoot);
		LookAtIK component = parent.GetComponent<LookAtIK>();
		this.solvers.Add(component);
		component.enabled = true;
		this._footStepsAudioSource = parent.GetComponent<AudioSource>();
		this._footStepsAudioSource.enabled = true;
		this.rodBones.Init(parent);
		if (this.rodBones.RightHand == null)
		{
			LogHelper.Error("Can't find bone {0} for initial bones object!", new object[] { this.rodBones.rightHandName });
		}
		if (this.rodBones.LeftHand == null)
		{
			LogHelper.Error("Can't find bone {0} for initial bones object!", new object[] { this.rodBones.leftHandName });
		}
		this._fireworkController = new TPMFireworkController(this.rodBones);
		this._gearAnimator = parent.GetComponent<Animator>();
		this._handsLayersController = new HandsLayersController(this._gearAnimator);
		this.InitLabels();
		if (bakedRenderer == null)
		{
			this.CollectRenderers(constructedParts);
		}
		else
		{
			this._renderers.Add(bakedRenderer);
		}
		TPMModelLayerSettings tpmmodelLayerSettings = constructedParts.Values.FirstOrDefault((TPMModelLayerSettings p) => p.FlashlightContoller != null);
		if (tpmmodelLayerSettings != null)
		{
			this._light = tpmmodelLayerSettings.FlashlightContoller;
			this._light.SetLightConeVisibility(isLightConeVisible);
		}
		this._curRod = new RodAnimator(null, this.Root, this.rodBones, base.transform, this, this._gripPrefab, this._pChumBall, parent.GetComponent<LimbIK>(), this.PlayerName);
		this._curRod.ERodAssembled += this.OnRodAssembled;
		this._curRod.ERodStateChanged += this.OnRodStateChanged;
		this.SetHiddenState(HandsViewController.HiddenState.Initialization);
	}

	public Vector3? GetHitPoint(Ray ray, float dist)
	{
		return this._collidersSystem.GetHitPoint(ray, dist);
	}

	private void OnPlaybackFinished()
	{
		LogHelper.Log("Playback finished");
		this.TogglePause();
	}

	private void OnTargetFrameReached(float speed)
	{
		this.SetPlaybackSpeed(speed);
		this.Pause();
		this.ETargetFrameReached(this.PlaybackControll.CurrentFrame);
	}

	public void PushData(Stream stream, float dt)
	{
		this.PlaybackControll.PushData(stream, dt);
	}

	public bool IsPaused
	{
		get
		{
			return this.PlaybackControll.IsPaused;
		}
	}

	public void Pause()
	{
		this.PlaybackControll.Pause();
		Time.timeScale = 0f;
	}

	public void TogglePause()
	{
		this.PlaybackControll.TogglePause();
		float num = ((!this.PlaybackControll.IsPaused) ? this.PlaybackControll.SpeedK : 0f);
		Time.timeScale = num;
	}

	public void SetPlaybackSpeed(float speed)
	{
		this.PlaybackControll.SetPlaybackSpeed(speed);
		if (!this.PlaybackControll.IsPaused)
		{
			Time.timeScale = speed;
		}
	}

	public void PlayReplayToFrame(int frame, float speed = 5f)
	{
		this.PlaybackControll.PlayToFrame(frame, speed);
		Time.timeScale = speed;
	}

	public void PlayReplayFastBackwardTo(int frame, float speed = 5f)
	{
		ThirdPersonData thirdPersonData = this.PlaybackControll.Restart();
		for (int i = 0; i < this._animators.Count; i++)
		{
			this._animators[i].Animator.enabled = false;
			this._animators[i].Animator.Rebind();
			this._animators[i].Animator.enabled = true;
		}
		this._prevPlayerPosition = null;
		this._wasRestarted = true;
		this._boolParameters[2] = false;
		this._charMovementAnimator.Restart();
		this._handsLayersController.Restart();
		this._curRod.FirstUpdate(thirdPersonData);
		this.PlayReplayToFrame(frame, speed);
	}

	private void SetAnimationsSpeed(float speed)
	{
		for (int i = 0; i < this._animators.Count; i++)
		{
			this._animators[i].Animator.speed = speed;
		}
	}

	private void OnRodStateChanged(bool isRodAssembled)
	{
		base.UpdateParameter(TPMMecanimBParameter.WasRodAssembled, isRodAssembled, true);
		if (isRodAssembled)
		{
			this.Activate(true);
		}
		else
		{
			this._curRod.ERodAssembled += this.OnRodAssembled;
			this._animators.RemoveAt(this._animators.Count - 1);
		}
	}

	private void OnRodAssembled()
	{
		this._curRod.ERodAssembled -= this.OnRodAssembled;
		base.RegisterNewAnimator(this._curRod);
	}

	private void CollectRenderers(Dictionary<CustomizedParts, TPMModelLayerSettings> constructedParts)
	{
		foreach (KeyValuePair<CustomizedParts, TPMModelLayerSettings> keyValuePair in constructedParts)
		{
			if (keyValuePair.Value != null)
			{
				LayerPair[] layers = keyValuePair.Value.Layers;
				for (int i = 0; i < layers.Length; i++)
				{
					this._renderers.Add(layers[i].renderer);
				}
			}
		}
	}

	public void CheckCollisions(List<PlayerRecord> players)
	{
		if (!this._isForceVisibilityMode && this._prevPlayerPosition != null && GameFactory.Player != null)
		{
			if (this._isModelVisible)
			{
				for (int i = 0; i < players.Count; i++)
				{
					if (players[i].Controller != this && players[i].Controller != null)
					{
						float magnitude = (this.Position - players[i].Controller.Position).magnitude;
						if (magnitude < this.DistToDisappier)
						{
							this.SetModelVisibility(false);
							return;
						}
					}
				}
				float magnitude2 = (this._prevPlayerPosition.Value - GameFactory.Player.transform.position).magnitude;
				if (magnitude2 < this.DistToDisappier)
				{
					this.SetModelVisibility(false);
					return;
				}
			}
			else
			{
				bool flag = false;
				for (int j = 0; j < players.Count; j++)
				{
					if (players[j].Controller != this && players[j].Controller != null)
					{
						float magnitude3 = (this.Position - players[j].Controller.Position).magnitude;
						if (magnitude3 < this.DistToShow)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					float magnitude4 = (this._prevPlayerPosition.Value - GameFactory.Player.transform.position).magnitude;
					if (magnitude4 > this.DistToShow)
					{
						this.SetModelVisibility(true);
						return;
					}
				}
			}
			if (this._visibilityActionFrom >= 0f)
			{
				float num = Mathf.Min((Time.time - this._visibilityActionFrom) / this._visibilityChangingTime, 1f);
				bool flag2 = Time.time - this._visibilityActionFrom > this._visibilityChangingTime;
				if (!this._isModelVisible)
				{
					num = 1f - num;
				}
				for (int k = 0; k < this._renderers.Count; k++)
				{
					this._renderers[k].material.SetFloat("_CharacterOpaque", num);
				}
				this._curRod.SetOpaque(num);
				if (flag2)
				{
					this._visibilityActionFrom = -1f;
					if (!this._isModelVisible)
					{
						this._fireworkController.SetVisibility(false);
						for (int l = 0; l < this._renderers.Count; l++)
						{
							this._renderers[l].enabled = false;
						}
						this._curRod.SetVisibility(false);
						if (this._light != null)
						{
							this._light.SetActive(false);
						}
					}
				}
			}
		}
	}

	private void SetHiddenState(HandsViewController.HiddenState state)
	{
		this._curHiddenState = state;
		this.ApplyModelVisibility(true);
	}

	public void SetModelVisibility(bool flag)
	{
		this._isModelVisible = flag;
		if (this._curHiddenState == HandsViewController.HiddenState.None)
		{
			this._visibilityActionFrom = Time.time;
			this.ApplyModelVisibility(false);
		}
	}

	private void ApplyModelVisibility(bool immediately)
	{
		bool flag = this._isModelVisible && this._curHiddenState == HandsViewController.HiddenState.None;
		if (immediately || flag)
		{
			for (int i = 0; i < this._renderers.Count; i++)
			{
				this._renderers[i].enabled = flag;
			}
			this._curRod.SetVisibility(flag);
			if (this._light != null)
			{
				this._light.SetActive(flag);
			}
			this._fireworkController.SetVisibility(flag);
		}
	}

	private void InitLabels()
	{
		Transform transform = this._labelsRoot.Find("pBillboardMessage/textMesh");
		if (transform != null)
		{
			this._chatMessageObject = transform.GetComponent<BillboardText>();
			if (this._playerCamera != null)
			{
				this._chatMessageObject.cameraToLookAt = this._playerCamera;
			}
			transform.gameObject.SetActive(false);
		}
		Transform transform2 = this._labelsRoot.Find("pBillboardTarget/textMesh");
		if (transform2 != null)
		{
			BillboardText component = transform2.GetComponent<BillboardText>();
			if (this._playerCamera != null)
			{
				component.cameraToLookAt = this._playerCamera;
			}
			transform2.gameObject.SetActive(false);
		}
		Transform transform3 = this._labelsRoot.Find("pBillboardLabel/textMesh");
		if (transform3 != null)
		{
			this._playerNameObject = transform3.GetComponent<BillboardText>();
			if (this._playerCamera != null)
			{
				this._playerNameObject.cameraToLookAt = this._playerCamera;
			}
			transform3.gameObject.SetActive(false);
			this.UpdatePlayerLabel();
		}
	}

	private void _SetLabelsVisibility(bool flag)
	{
		bool flag2 = true;
		Transform transform = this._labelsRoot.Find("pBillboardMessage/textMesh");
		if (transform != null)
		{
			transform.gameObject.SetActive(this._labelsVisibility && flag && this._showText && flag2);
		}
		Transform transform2 = this._labelsRoot.Find("pBillboardTarget/textMesh");
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(this._labelsVisibility && flag && flag2);
		}
		Transform transform3 = this._labelsRoot.Find("pBillboardLabel/textMesh");
		if (transform3 != null)
		{
			transform3.gameObject.SetActive(this._labelsVisibility && flag && flag2);
		}
	}

	public void SetLabelsVisibility(bool flag)
	{
		this._labelsVisibility = flag;
		if (this.WasControllerActivated)
		{
			this._SetLabelsVisibility(flag);
		}
	}

	protected override void SubscribeAnimatorEvents(ObjWithAnimator animator)
	{
		base.SubscribeAnimatorEvents(animator);
		if (this._fireworkHandler == null)
		{
			this._fireworkHandler = animator.GameObject.GetComponent<FireworkHandler>();
			if (this._fireworkHandler != null)
			{
				this._fireworkHandler.OnPut += this.PutFireworkAction;
			}
		}
		if (this._footStepsHandler == null)
		{
			this._footStepsHandler = animator.GameObject.GetComponent<FootStepsHandler>();
			if (this._footStepsHandler != null)
			{
				this._footStepsHandler.OnHitGround += this.FootHitGroundAction;
			}
		}
		if (this._eyesLockerHandler == null)
		{
			this._eyesLockerHandler = animator.GameObject.GetComponent<EyesLockerHandler>();
			if (this._eyesLockerHandler != null)
			{
				this._eyesLockerHandler.EChangeState += this.EyesLockerHandlerOnChangeState;
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> EEyesLockerChangeState = delegate
	{
	};

	public void EyesLockerHandlerOnChangeState(bool flag)
	{
		this.EEyesLockerChangeState(flag);
	}

	private void UpdatePlayerLabel()
	{
		if (this._player.Level != this._lastUpdatedLevel || this._lastUpdatedRank != this._player.Rank)
		{
			this._lastUpdatedRank = this._player.Rank;
			this._lastUpdatedLevel = this._player.Level;
			this._playerNameObject.SetText(PlayerProfileHelper.GetPlayerNameLevelRank(this._player));
		}
	}

	protected virtual void Start()
	{
		this._prevPlayerPosition = null;
		this.Activate(false);
	}

	public void ProcessPackage(Package package, bool suppressVisibility)
	{
		if (suppressVisibility)
		{
			if (this._curHiddenState != HandsViewController.HiddenState.Suppressed)
			{
				this.SetHiddenState(HandsViewController.HiddenState.Suppressed);
			}
		}
		else if (this._curHiddenState == HandsViewController.HiddenState.Suppressed)
		{
			this.SetHiddenState(HandsViewController.HiddenState.None);
		}
		this.UpdatePlayerLabel();
		this._cache.AppendData(package);
	}

	protected override void CatchFishAction(bool flag)
	{
		base.CatchFishAction(flag);
		this._curRod.CatchFishAction(flag);
	}

	private void PutFireworkAction()
	{
		this._fireworkController.OnPutAction();
	}

	private void FootHitGroundAction()
	{
		if (GameFactory.Player != null)
		{
			Transform transform = ((this._leftFoot.position.y >= this._rightFoot.position.y) ? this._rightFoot : this._leftFoot);
			GameFactory.Player.FootStepsHelper.PlayFootStepAudio(transform.position, transform.position + Vector3.down * 1f, this._footStepsAudioSource);
		}
	}

	public void SetCaughtFish(FishController fish)
	{
		if (this._fishInHands != null)
		{
			Object.Destroy(this._fishInHands.gameObject);
		}
		this._fishInHands = fish;
		this.UpdateCaughtFish();
		this._fishInHands.UpdateLength(this._fishInHands.Length);
	}

	public void UpdateCaughtFishLength(float length)
	{
		if (this._fishInHands != null)
		{
			this._fishInHands.UpdateLength(length);
		}
	}

	private void UpdateCaughtFish()
	{
		if (this._fishInHands != null)
		{
			this._fishInHands.LeanToHands(this.rodBones.LeftHand.transform, this.rodBones.RightHand.transform);
			if (this._fishInHands.updateLengthCounter == 0)
			{
				this._fishInHands.updateLengthCounter++;
			}
			else if (this._fishInHands.updateLengthCounter == 1)
			{
				this._fishInHands.UpdateLength(this._fishInHands.Length);
				this._fishInHands.updateLengthCounter++;
			}
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		for (int i = 0; i < this.solvers.Count; i++)
		{
			this.solvers[i].UpdateSolverExternal();
		}
		this.UpdateCaughtFish();
		this._curRod.LateUpdate(this._boolParameters[2]);
		ThirdPersonData thirdPersonData = this._cache.Update();
		if (thirdPersonData != null)
		{
			this.ProcessDataPortion(thirdPersonData);
			if (this.PlaybackControll != null)
			{
				this.EFrameUpdate(this.PlaybackControll.CurrentFrame);
			}
		}
		if (this._cache.IsPaused && !this._cache.IsPlayer)
		{
			this._stopSwitchingStateAt = Time.time + 1f;
			this.SetHiddenState(HandsViewController.HiddenState.Paused);
		}
		if (this._stopSwitchingStateAt > 0f && this._stopSwitchingStateAt < Time.time)
		{
			this.SetHiddenState(HandsViewController.HiddenState.None);
			this._stopSwitchingStateAt = -1f;
		}
		float dtPrc = this._curRod.GetDtPrc();
		for (int j = 0; j < this._floatParameters.Length; j++)
		{
			if (j != 5)
			{
				float num = Mathf.Lerp(this._prevFloats[j], this._floatParameters[j], dtPrc);
				int num2 = this._animatorFHashes[j];
				for (int k = 0; k < this._animators.Count; k++)
				{
					this._animators[k].SetFloat(num2, num);
				}
			}
		}
		this._handsLayersController.Update();
	}

	public void ProcessPhotoModeData(ThirdPersonData data, bool isFlashLightEnabled)
	{
		this.ProcessDataPortion(data);
		if (this._light != null)
		{
			this._light.SetLightVisibility(isFlashLightEnabled);
		}
	}

	public void ProcessDataPortion(ThirdPersonData data)
	{
		Vector3? prevPlayerPosition = this._prevPlayerPosition;
		bool flag = prevPlayerPosition == null && !this._wasRestarted;
		this.OnNewServerDataPortion(data);
		bool flag2 = this._boolParameters[2];
		this._curRod.ServerUpdate(data);
		if (data.BoolParameters[7] != this._boolParameters[7])
		{
			this._stopSwitchingStateAt = Time.time + 3f;
			this.SetHiddenState((!this._boolParameters[7]) ? HandsViewController.HiddenState.StartSailing : HandsViewController.HiddenState.StopSailing);
		}
		if (data.BoolParameters[6] != this._boolParameters[6])
		{
			for (int i = 0; i < this.solvers.Count; i++)
			{
				this.solvers[i].enabled = !data.BoolParameters[6];
			}
		}
		for (int j = 0; j < data.ByteParameters.Length; j++)
		{
			this.UpdateParameter((TPMMecanimIParameter)j, data.ByteParameters[j], true);
		}
		for (int k = 0; k < data.FloatParameters.Length; k++)
		{
			this.UpdateParameter((TPMMecanimFParameter)k, data.FloatParameters[k], false);
		}
		for (int l = 0; l < data.BoolParameters.Length; l++)
		{
			base.UpdateParameter((TPMMecanimBParameter)l, data.BoolParameters[l], false);
		}
		this.UpdateParameter(TPMMecanimFParameter.FishFight, (float)((!data.FishesAndItems.Any((ThirdPersonData.FishData f) => f.state == TPMFishState.Hooked)) ? 0 : 1), true);
		this._fireworkController.ServerUpdate(data.fireworkID, data.BoolParameters[6]);
		if (flag2 != this._boolParameters[2])
		{
			this._handsLayersController.SetRodActive(!flag2);
			this.Activate(this._boolParameters[2]);
			if (this._boolParameters[2] && (!this._boolParameters[7] || this._boolParameters[8]))
			{
				base.SetAnimation(data.currentClipHash);
			}
		}
		bool flag3 = data.BoolParameters[7] && !data.BoolParameters[8] && data.ByteParameters[2] == 4;
		this._gearAnimator.SetLayerWeight(3, (float)((!flag3) ? 0 : 1));
		if (flag)
		{
			this.OnPlayerActive();
		}
		this._wasRestarted = false;
	}

	protected override void UpdateParameter(TPMMecanimIParameter name, byte value, bool logEnabled = true)
	{
		byte b = this._byteParameters[(int)((byte)name)];
		if (name == TPMMecanimIParameter.MovieAction)
		{
			if (b == 0)
			{
				if (value != 0)
				{
					for (int i = 0; i < this._animators.Count; i++)
					{
						this._animators[i].SaveAnimatorStates();
					}
				}
			}
			else if (value == 0)
			{
				for (int j = 0; j < this._animators.Count; j++)
				{
					this._animators[j].RestoreAnimatorStates();
				}
			}
		}
		base.UpdateParameter(name, value, logEnabled);
	}

	public bool WasControllerActivated
	{
		get
		{
			Vector3? prevPlayerPosition = this._prevPlayerPosition;
			return prevPlayerPosition != null;
		}
	}

	protected void OnNewServerDataPortion(ThirdPersonData data)
	{
		Vector3? prevPlayerPosition = this._prevPlayerPosition;
		Vector3 vector = ((prevPlayerPosition != null) ? (data.playerPosition - this._prevPlayerPosition.Value) : Vector3.zero);
		if (this.WasControllerActivated)
		{
			for (int i = 0; i < this._floatParameters.Length; i++)
			{
				this._prevFloats[i] = this._floatParameters[i];
			}
		}
		else
		{
			this.SetHiddenState(HandsViewController.HiddenState.None);
			this._SetLabelsVisibility(true);
			for (int j = 0; j < data.FloatParameters.Length; j++)
			{
				this._prevFloats[j] = data.FloatParameters[j];
			}
		}
		if (this._charMovementAnimator != null)
		{
			bool flag = this._boolParameters[16] || (this._boolParameters[7] && (this._byteParameters[2] == 1 || !this._boolParameters[8]));
			this._charMovementAnimator.MoveAndLook(vector, data.playerPosition, data.playerRotation, 0.1f, flag, this._boolParameters[7]);
		}
		byte b = this._byteParameters[3];
		this._handsLayersController.UpdateData(vector != Vector3.zero, data.isTackleThrown || data.BoolParameters[6], data.BoolParameters[7], b == 11 || b == 12, data.ByteParameters[0] == 6);
		this._prevPlayerPosition = new Vector3?(data.playerPosition);
	}

	protected override void UpdateParameter(TPMMecanimFParameter name, float value, bool logEnabled = true)
	{
		this._floatParameters[(int)((byte)name)] = value;
	}

	public void OnIncomingChatMessage(string message)
	{
		if (this._chatMessageObject != null && this._prevPlayerPosition != null)
		{
			bool flag = (this._prevPlayerPosition.Value - GameFactory.Player.transform.position).magnitude > (float)this._textMaxDist;
			this._chatMessageObject.SetText((!flag) ? HandsViewController.AdjustText(message, this._textMaxLength, this._textMaxWidth, '\n') : "...");
		}
	}

	public static string AdjustText(string message, int maxLength, int maxWidth, char delimiter = '\n')
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num2;
		for (int i = 0; i < message.Length; i = num2 + 1)
		{
			int num = Math.Min(i + maxWidth - 1, message.Length - 1);
			num2 = message.LastIndexOf(' ', num);
			if (num2 < i)
			{
				num2 = message.IndexOf(' ', num);
				if (num2 == -1)
				{
					num2 = message.Length - 1;
				}
			}
			string text = message.Substring(i, num2 - i + 1).Trim();
			if (text.Length > 0)
			{
				if (i > 0)
				{
					stringBuilder.Append(delimiter);
				}
				stringBuilder.Append(text);
			}
		}
		string text2 = stringBuilder.ToString();
		if (text2.Length > maxLength)
		{
			text2 = text2.Substring(0, maxLength) + "...";
		}
		return text2;
	}

	protected override void OnDestroy()
	{
		this._player = null;
		if (this._cache == null)
		{
			return;
		}
		if (this.PlaybackControll != null)
		{
			this.PlaybackControll.EPlaybackFinished -= this.OnPlaybackFinished;
			this.PlaybackControll.ETargetFrameReached -= this.OnTargetFrameReached;
		}
		this._fireworkController.Destroy();
		this._curRod.ERodStateChanged -= this.OnRodStateChanged;
		this._curRod.OnDestroy();
		if (this._fireworkHandler != null)
		{
			this._fireworkHandler.OnPut -= this.PutFireworkAction;
			this._fireworkHandler = null;
		}
		if (this._footStepsHandler != null)
		{
			this._footStepsHandler.OnHitGround -= this.FootHitGroundAction;
			this._footStepsHandler = null;
		}
		if (this._eyesLockerHandler != null)
		{
			this._eyesLockerHandler.EChangeState -= this.EyesLockerHandlerOnChangeState;
			this._eyesLockerHandler = null;
		}
		if (this._fishInHands != null)
		{
			Object.Destroy(this._fishInHands.gameObject);
			this._fishInHands = null;
		}
		base.OnDestroy();
	}

	private const float GROUND_RAYCAST_MAX_DIST = 1f;

	protected IDataCache _cache;

	private Vector3? _prevPlayerPosition;

	public Vector3 MODEL_GROUND_CORRECTION = new Vector3(0f, -1.5f, 0f);

	public Vector3 BOAT_GROUND_CORRECTION = Vector3.zero;

	private List<SolverManager> solvers = new List<SolverManager>();

	[SerializeField]
	private GameObject _ikPartsRoot;

	[SerializeField]
	private Transform _labelsRoot;

	[SerializeField]
	private Transform _ikTarget;

	[Tooltip("Model on this dist became invisible")]
	[SerializeField]
	private float _distToDisappier = 2f;

	[Tooltip("Model on boat from this dist became invisible")]
	[SerializeField]
	private float _distToDisappierWhenSailing = 4.5f;

	[Tooltip("You need to go out from invisible model on this distance to show it againe")]
	[SerializeField]
	private float _distToShow = 4f;

	[Tooltip("You need to go out from invisible model on boat to this distance to show it againe")]
	[SerializeField]
	private float _distToShowWhenSailing = 5f;

	[SerializeField]
	private float _visibilityChangingTime = 0.15f;

	[Tooltip("Size of reciever data cache (seconds)")]
	[SerializeField]
	private float _minCacheSize = 1f;

	[SerializeField]
	private bool _showText;

	[SerializeField]
	private int _textMaxWidth = 40;

	[SerializeField]
	private int _textMaxLength = 200;

	[SerializeField]
	private int _textMaxDist = 50;

	[SerializeField]
	private float _headDefaultWeight = 0.8f;

	[SerializeField]
	private float _bodyDefaultWeight = 0.3f;

	[SerializeField]
	private float _headClamp = 0.5f;

	[SerializeField]
	private bool _isControlTarget = true;

	[SerializeField]
	private bool _isAnimatorLogEnabled;

	[SerializeField]
	private float _holdingFishDy;

	[SerializeField]
	private Vector3 _photoModeFloatingLineDisplacement = new Vector3(-0.12f, -0.1f, 0f);

	[SerializeField]
	private Vector3 _photoModeSpinningLineDisplacement = new Vector3(-0.3f, -0.2f, 0f);

	[SerializeField]
	private Vector3 _photoModeCastingLineDisplacement = new Vector3(0f, 0f, -0.03f);

	private TPMPositionDrivenAnimator _charMovementAnimator;

	private BillboardText _playerNameObject;

	private BillboardText _chatMessageObject;

	private Camera _playerCamera;

	private IPlayer _player;

	private int _lastUpdatedLevel;

	private int _lastUpdatedRank;

	private TPMFireworkController _fireworkController;

	private FireworkHandler _fireworkHandler;

	private FootStepsHandler _footStepsHandler;

	private EyesLockerHandler _eyesLockerHandler;

	private TPMAnimatorHandler _animatorHandler;

	private Transform _leftFoot;

	private Transform _rightFoot;

	private AudioSource _footStepsAudioSource;

	private Animator _gearAnimator;

	private const byte GEAR_LAYER_INDEX = 3;

	private float[] _prevFloats = new float[7];

	private List<SkinnedMeshRenderer> _renderers = new List<SkinnedMeshRenderer>();

	private bool _isModelVisible;

	private HandsViewController.HiddenState _curHiddenState;

	private float _stopSwitchingStateAt = -1f;

	private bool _labelsVisibility;

	private float _visibilityActionFrom = -1f;

	private bool _isForceVisibilityMode;

	private HandsLayersController _handsLayersController;

	private TPMFlashLightController _light;

	private TPMColliders _collidersSystem;

	private FishController _fishInHands;

	private enum HiddenState
	{
		None,
		StartSailing,
		StopSailing,
		Paused,
		Initialization,
		Suppressed
	}
}
