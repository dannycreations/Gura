using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Boats;
using ObjectModel;
using RootMotion.FinalIK;
using UnityEngine;

namespace TPM
{
	public class RodAnimator : RodAnimatorBase
	{
		public RodAnimator(TPMAssembledRod rodAssembly, Transform charRoot, RodBones rodBones, Transform rootTransform, IPositionCorrectors positionCorrectors, GameObject gripPrefab, ChumBall pChumBall, LimbIK limbIk, string playerName = null)
			: base(null, rodBones.RightHand, rodBones.LeftHand, false)
		{
			this._pChumBall = pChumBall;
			this._playerName = playerName;
			this._ikCurveHashes[0] = Animator.StringToHash("IKWeightCurve");
			this._ikCurveHashes[1] = Animator.StringToHash("IKWeightCurveCasting");
			this._limbIk = limbIk;
			this._rootTransform = rootTransform;
			this._charRoot = charRoot;
			this._positionCorrectors = positionCorrectors;
			this._lastUpdateTime = Time.time - 1f;
			this._fishBehaviours = new List<Fish3rdBehaviour>(7);
			GameObject gameObject = Object.Instantiate<GameObject>(gripPrefab);
			if (gameObject != null)
			{
				this._grip = gameObject.GetComponent<GripSettings>();
			}
			this._boat = new TPMBoat(this._rightRoot.transform);
			this.ChangeRod(rodAssembly);
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ERodAssembled = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ERodStateChanged = delegate
		{
		};

		private Vector3 LineArcLocator
		{
			get
			{
				return this._curRod.Reel.ReelHandler.LineArcLocator.transform.position;
			}
		}

		protected override GameObject Rod
		{
			get
			{
				return (this._curRod.Rod == null) ? null : this._curRod.Rod.gameObject;
			}
		}

		public float GetDtPrc()
		{
			return Math.Min((Time.time - this._lastUpdateTime) / 0.1f, 1f);
		}

		public override void ChangeHand(bool isLeft)
		{
			base.ChangeHand(isLeft);
			this._grip.ChangeRoot((!this._isCurHandLeft) ? this._leftRoot.transform : this._rightRoot.transform);
		}

		public override void Activate(bool flag)
		{
			base.Activate(flag);
			if (this.Rod != null)
			{
				this._curRod.Line.SetActive(flag);
				this._curRod.Tackle.SetActive(flag);
			}
			this.SetVisibility(this._visibility);
		}

		public Transform CurrentBobber
		{
			get
			{
				if (this._curRod.State == RodAnimator.ResourcesState.Created && this._curRod.RodAssembly.RodTemplate == RodTemplate.Float)
				{
					return this._curRod.Tackle.transform;
				}
				return null;
			}
		}

		public Transform CurrentFish
		{
			get
			{
				if (this._fishBehaviours.Count > 0)
				{
					return this._fishBehaviours[0].transform;
				}
				return null;
			}
		}

		public Transform CurrentBoat
		{
			get
			{
				return this._boat.BoatModel;
			}
		}

		private void ChangeRod(TPMAssembledRod rodAssembly)
		{
			if (this._curRod.State == RodAnimator.ResourcesState.Created)
			{
				this.DestroyRod(this._curRod);
				this.ERodStateChanged(false);
			}
			if (rodAssembly == null)
			{
				this.SetVisibility(this._visibility);
				this._curRod.State = RodAnimator.ResourcesState.None;
			}
			else
			{
				RodInitialize.AsyncHelper.StartCoroutine(this.AsyncLoadRod(rodAssembly, this._curRod));
			}
		}

		private IEnumerator AsyncLoadRod(TPMAssembledRod rodAssembly, RodAnimator.RodData rodData)
		{
			if (rodAssembly != null)
			{
				rodData.RodAssembly.Clone(rodAssembly);
				rodData.State = RodAnimator.ResourcesState.Loading;
				FishWaterTile.DoingRenderWater = false;
				ResourceRequest request = Resources.LoadAsync<GameObject>(rodAssembly.RodInterface.Asset);
				yield return request;
				rodData.RodPrefab = request.asset as GameObject;
				request = Resources.LoadAsync<GameObject>(rodAssembly.ReelInterface.Asset);
				yield return request;
				rodData.ReelPrefab = request.asset as GameObject;
				request = Resources.LoadAsync<AnimatorOverrideController>("TPM/Animator/ReelPrototypeAnimator");
				yield return request;
				rodData.ReelAnimatorPrefab = request.asset as AnimatorOverrideController;
				if (rodAssembly.BellInterface != null)
				{
					request = Resources.LoadAsync<GameObject>(rodAssembly.BellInterface.Asset);
					yield return request;
					rodData.BellPrefab = request.asset as GameObject;
				}
				IEnumerator tackleRequest = RodInitialize.LoadTackleAsync(rodAssembly, delegate(GameObject asset)
				{
					rodData.TacklePrefab = asset;
				}, delegate(GameObject asset)
				{
					rodData.SecondaryTacklePrefab = asset;
				});
				yield return tackleRequest;
				FishWaterTile.DoingRenderWater = true;
				rodData.State = RodAnimator.ResourcesState.Loaded;
			}
			else
			{
				this.SetVisibility(this._visibility);
			}
			yield break;
		}

		private void CreateRod(RodAnimator.RodData rodData, bool isMain)
		{
			ReelTypes reelType = rodData.RodAssembly.ReelType;
			this._isBaitcasting = reelType == ReelTypes.Baitcasting;
			rodData.Slot = new GameFactory.RodSlot(-1);
			GameObject gameObject = rodData.RodPrefab;
			TPMAssembledRod tpmassembledRod = rodData.RodAssembly;
			GameFactory.RodSlot rodSlot = rodData.Slot;
			UserBehaviours userBehaviours = UserBehaviours.ThirdPerson;
			rodData.Rod = RodInitialize.CreateRod(gameObject, tpmassembledRod, rodSlot, userBehaviours, null, isMain, this._playerName, null).Behaviour as Rod3rdBehaviour;
			rodData.Reel = RodInitialize.CreateReel(rodData.ReelPrefab, rodData.RodAssembly, rodData.Slot, rodData.Rod.transform, UserBehaviours.ThirdPerson, null).Behaviour as Reel3rdBehaviour;
			if (rodData.BellPrefab != null)
			{
				rodData.Bell = RodInitialize.CreateBell(rodData.BellPrefab, rodData.RodAssembly, rodData.Rod.RodSlot, rodData.Rod.transform, UserBehaviours.ThirdPerson).Behaviour as Bell3rdBehaviour;
				rodData.Bell.Init(rodData.Rod);
			}
			Animator animator = rodData.Reel.gameObject.GetComponent<Animator>();
			if (isMain)
			{
				if (animator == null)
				{
					animator = rodData.Reel.gameObject.AddComponent<Animator>();
				}
				else
				{
					animator.enabled = true;
				}
				animator.runtimeAnimatorController = Object.Instantiate<AnimatorOverrideController>(rodData.ReelAnimatorPrefab);
				base.Reel = rodData.Reel.gameObject;
			}
			else if (animator != null)
			{
				animator.enabled = false;
			}
			rodData.Line = RodInitialize.CreateLine(rodData.RodAssembly, rodData.Slot, rodData.Reel.gameObject, this._rootTransform, UserBehaviours.ThirdPerson, true).Behaviour as Line3rdBehaviour;
			gameObject = rodData.TacklePrefab;
			tpmassembledRod = rodData.RodAssembly;
			rodSlot = rodData.Slot;
			userBehaviours = UserBehaviours.ThirdPerson;
			GameObject secondaryTacklePrefab = rodData.SecondaryTacklePrefab;
			rodData.Tackle = RodInitialize.CreateTackle(gameObject, tpmassembledRod, rodSlot, userBehaviours, null, secondaryTacklePrefab).Behaviour;
			rodData.Reel.Init();
			rodData.Line.EFinishThrowing += rodData.Tackle.OnFinishThrowing;
			rodData.Line.SyncInit(rodData.Rod);
			rodData.Slot.SetRod(rodData.Rod);
			rodData.Slot.SetTackle(rodData.Tackle);
			rodData.Slot.SetLine(rodData.Line);
			rodData.Slot.SetReel(rodData.Reel);
			rodData.Slot.SetBell(rodData.Bell);
			rodData.RodPrefab = null;
			rodData.ReelPrefab = null;
			rodData.ReelAnimatorPrefab = null;
			rodData.BellPrefab = null;
			rodData.TacklePrefab = null;
			rodData.SecondaryTacklePrefab = null;
			rodData.State = RodAnimator.ResourcesState.Created;
		}

		private void CreateMainRod()
		{
			this.CreateRod(this._curRod, true);
			this.ChangeHand(false);
			this._limbIk.solver.target = TransformHelper.FindDeepChild(base.Reel.transform, (!this._isBaitcasting) ? "handle" : "aim");
			Transform transform = this._limbIk.transform;
			if (this._isBaitcasting)
			{
				this._limbIk.solver.SetChain(TransformHelper.FindDeepChild(transform.transform, "RightArm"), TransformHelper.FindDeepChild(transform.transform, "RightHand"), TransformHelper.FindDeepChild(transform.transform, "RightHandThumb4"), TransformHelper.FindDeepChild(transform.transform, "Hips"));
				this._limbIk.solver.bendModifier = 3;
				this._limbIk.solver.goal = 3;
			}
			else
			{
				this._limbIk.solver.SetChain(TransformHelper.FindDeepChild(transform.transform, "LeftArm"), TransformHelper.FindDeepChild(transform.transform, "LeftHand"), TransformHelper.FindDeepChild(transform.transform, "LeftHandThumb4"), TransformHelper.FindDeepChild(transform.transform, "Hips"));
				this._limbIk.solver.bendModifier = 3;
				this._limbIk.solver.goal = 2;
			}
			this.Activate(this._isActive);
			this.ERodAssembled();
			this._lineWithFishDisplacement = this.GetLineDisplacement();
		}

		private Vector3 GetLineDisplacement()
		{
			return (this._curRod.RodAssembly.ReelType != ReelTypes.Baitcasting) ? ((this._curRod.RodAssembly.RodTemplate != RodTemplate.Float) ? this._positionCorrectors.PhotoModeSpinningLineDisplacement : this._positionCorrectors.PhotoModeFloatingLineDisplacement) : this._positionCorrectors.PhotoModeCastingLineDisplacement;
		}

		public void CatchFishAction(bool flag)
		{
			if (!flag || (this._lastServerData.ByteParameters[1] == 2 && this._lastServerData.ByteParameters[3] != 9))
			{
				this._grip.SetGameVisibility(flag);
			}
		}

		public void DestroyRod(RodAnimator.RodData rod)
		{
			if (rod.State == RodAnimator.ResourcesState.Created)
			{
				rod.State = RodAnimator.ResourcesState.None;
				rod.Line.EFinishThrowing -= rod.Tackle.OnFinishThrowing;
				if (rod.Bell != null)
				{
					rod.Bell.Destroy();
					rod.Bell = null;
				}
				rod.Reel.Destroy();
				rod.Rod.Destroy();
				rod.Line.Destroy();
				rod.Tackle.Destroy();
				rod.Rod = null;
			}
		}

		public void FirstUpdate(ThirdPersonData data)
		{
			this._lastServerData = null;
			this.ServerUpdate(data);
			this._lastServerData = null;
		}

		public void ServerUpdate(ThirdPersonData data)
		{
			bool flag = false;
			float num;
			if (this._lastServerData == null)
			{
				this._lastServerData = data;
				num = 1f;
				flag = true;
				this._boat.ServerUpdate(data, num, Vector3.zero);
				this._boat.SyncUpdate(num);
				this.UpdatePosition(1f);
			}
			else
			{
				num = this.GetDtPrc();
			}
			if (data.ByteParameters[0] == 6)
			{
				if (this._chumBall == null && this._pChumBall != null)
				{
					this._chumBall = Object.Instantiate<ChumBall>(this._pChumBall);
					this._chumBall.transform.parent = this._rightRoot.transform;
					this._chumBall.transform.localPosition = Vector3.zero;
				}
			}
			else if (this._chumBall != null && !this._chumBall.WasLaunched && !data.BoolParameters[5])
			{
				Object.Destroy(this._chumBall.gameObject);
			}
			if (this._chumBall != null && !this._chumBall.WasLaunched && data.ByteParameters[2] > 0 && this._nextChumBallAt < Time.time)
			{
				this._nextChumBallAt = Time.time + 5f;
				this._chumBall.Launch(0.1f, (float)data.ByteParameters[2], data.playerRotation * Vector3.forward, null, null);
			}
			this._prevPlayerPosition = Vector3.Lerp(this._prevPlayerPosition, this._lastServerData.playerPosition, num);
			this._prevPlayerRotation = Quaternion.Slerp(this._prevPlayerRotation, this._lastServerData.playerRotation, num);
			Vector3 vector = (data.playerPosition - this._lastServerData.playerPosition) / 0.1f;
			this._boat.ServerUpdate(data, num, vector);
			if (this._curRod.State != RodAnimator.ResourcesState.Loading && ((flag && data.IsRodAssembled) || (this._curRod.State != RodAnimator.ResourcesState.None && (!data.IsRodAssembled || this._curRod.RodAssembly.IsDifferent(data.RodAssembly))) || (this._curRod.State == RodAnimator.ResourcesState.None && data.IsRodAssembled)))
			{
				if (data.IsRodAssembled)
				{
				}
				this.ChangeRod(data.RodAssembly);
			}
			if (this._curRod.State == RodAnimator.ResourcesState.Loaded && data.IsRodAssembled)
			{
				this.CreateMainRod();
				this.ERodStateChanged(true);
			}
			if (this._curRod.State == RodAnimator.ResourcesState.Created && data.RodAssembly != null)
			{
				bool flag2 = data.FishesAndItems.Any((ThirdPersonData.FishData f) => f.state == TPMFishState.ShowSmall || f.state == TPMFishState.UnderwaterItemShowing);
				if (flag2)
				{
					Vector3 vector2 = this._curRod.Rod.ServerRodTransform.TransformDirection(this._lineWithFishDisplacement);
					Vector3[] mainAndLeaderPoints = data.RodAssembly.lineData.mainAndLeaderPoints;
					for (int i = 0; i < mainAndLeaderPoints.Length; i++)
					{
						mainAndLeaderPoints[i] += (float)i / (float)(mainAndLeaderPoints.Length - 1) * vector2;
					}
				}
				if (data.isLeftHandRod != this._isCurHandLeft)
				{
					this.ChangeHand(data.isLeftHandRod);
				}
				this._curRod.Rod.ServerUpdate(data.RodAssembly, num);
				this._curRod.Line.ServerUpdate(data.RodAssembly, data.isTackleThrown, flag2, data.BoolParameters[9], data.BoolParameters[0], data.IsPhotoMode, num);
				this._curRod.Tackle.ServerUpdate(data.RodAssembly.tackleData, data.isBaitVisibility, num);
			}
			this.ProcessRodPods(data);
			this.ProcessRods(data, num);
			this.ProcessFishes(data, num);
			this._lastServerData = data;
			this._lastUpdateTime = Time.time;
		}

		public void SetVisibility(bool flag)
		{
			this._visibility = flag;
			this._boat.SetVisibility(flag);
			if (this._curRod.Rod != null)
			{
				this._curRod.Rod.SetVisibility(flag);
				this._curRod.Reel.SetVisibility(flag);
				if (this._curRod.Bell != null)
				{
					this._curRod.Bell.SetVisibility(flag);
				}
				this._curRod.Line.SetVisibility(flag);
				this._curRod.Tackle.SetVisibility(flag);
				this._grip.SetPlayerVisibility(flag);
			}
			for (int i = 0; i < this._rodPods.Count; i++)
			{
				RodAnimator.RodPodData rodPodData = this._rodPods[i];
				if (rodPodData.State == RodAnimator.ResourcesState.Created)
				{
					rodPodData.Controller.SetVisibility(flag);
				}
			}
			for (int j = 0; j < this._rodsOnPods.Count; j++)
			{
				RodAnimator.RodData rodData = this._rodsOnPods[j];
				if (rodData.State == RodAnimator.ResourcesState.Created)
				{
					rodData.Rod.SetVisibility(flag);
					rodData.Reel.SetVisibility(flag);
					rodData.Line.SetVisibility(flag);
					rodData.Tackle.SetVisibility(flag);
					if (rodData.Bell != null)
					{
						rodData.Bell.SetVisibility(flag);
					}
				}
			}
			for (int k = 0; k < this._fishBehaviours.Count; k++)
			{
				this._fishBehaviours[k].SetVisibility(flag);
			}
			if (this._underwaterItem != null)
			{
				this._underwaterItem.SetVisibility(flag);
			}
			if (this._chumBall != null)
			{
				Renderer rendererForObject = RenderersHelper.GetRendererForObject<Renderer>(this._chumBall.transform);
				if (rendererForObject != null)
				{
					rendererForObject.enabled = flag;
				}
			}
		}

		public void SetOpaque(float prc)
		{
			if (this._curRod.State == RodAnimator.ResourcesState.Created)
			{
				this._curRod.Rod.SetOpaque(prc);
				this._curRod.Reel.SetOpaque(prc);
				if (this._curRod.Bell != null)
				{
					this._curRod.Bell.SetOpaque(prc);
				}
				this._curRod.Line.SetOpaque(prc);
				this._curRod.Tackle.SetOpaque(prc);
				this._grip.SetOpaque(prc);
			}
			for (int i = 0; i < this._rodPods.Count; i++)
			{
				RodAnimator.RodPodData rodPodData = this._rodPods[i];
				if (rodPodData.State == RodAnimator.ResourcesState.Created)
				{
					rodPodData.Controller.SetOpaque(prc);
				}
			}
			for (int j = 0; j < this._rodsOnPods.Count; j++)
			{
				RodAnimator.RodData rodData = this._rodsOnPods[j];
				if (rodData.State == RodAnimator.ResourcesState.Created)
				{
					rodData.Rod.SetOpaque(prc);
					rodData.Reel.SetOpaque(prc);
					rodData.Line.SetOpaque(prc);
					rodData.Tackle.SetOpaque(prc);
					if (rodData.Bell != null)
					{
						rodData.Bell.SetOpaque(prc);
					}
				}
			}
			for (int k = 0; k < this._fishBehaviours.Count; k++)
			{
				this._fishBehaviours[k].SetOpaque(prc);
			}
			if (this._underwaterItem != null)
			{
				this._underwaterItem.SetOpaque(prc);
			}
		}

		private IEnumerator AsyncLoadRodPod(RodAnimator.RodPodData rodPodData)
		{
			rodPodData.State = RodAnimator.ResourcesState.Loading;
			FishWaterTile.DoingRenderWater = false;
			ResourceRequest request = Resources.LoadAsync<GameObject>(rodPodData.AssetPath);
			yield return request;
			rodPodData.Prefab = request.asset as GameObject;
			FishWaterTile.DoingRenderWater = true;
			rodPodData.State = RodAnimator.ResourcesState.Loaded;
			yield break;
		}

		private void ProcessRodPods(ThirdPersonData data)
		{
			for (int i = 0; i < data.RodPods.Count; i++)
			{
				ThirdPersonData.RodPodData newPod = data.RodPods[i];
				if (this._rodPods.All((RodAnimator.RodPodData r) => r.Id != newPod.Id))
				{
					RodAnimator.RodPodData rodPodData = new RodAnimator.RodPodData
					{
						Id = newPod.Id,
						AssetId = newPod.AssetId
					};
					this._rodPods.Add(rodPodData);
					RodInitialize.AsyncHelper.StartCoroutine(this.AsyncLoadRodPod(rodPodData));
				}
			}
			int j = this._rodPods.Count - 1;
			while (j >= 0)
			{
				RodAnimator.RodPodData presentPod = this._rodPods[j];
				int num = data.RodPods.FindIndex((ThirdPersonData.RodPodData r) => r.Id == presentPod.Id);
				if (presentPod.State != RodAnimator.ResourcesState.Loaded)
				{
					goto IL_186;
				}
				presentPod.Controller = Object.Instantiate<GameObject>(presentPod.Prefab).GetComponent<RodPodController>();
				presentPod.Prefab = null;
				presentPod.State = RodAnimator.ResourcesState.Created;
				presentPod.Controller.transform.SetParent(this._rootTransform);
				if (num == -1)
				{
					goto IL_186;
				}
				ThirdPersonData.RodPodData rodPodData2 = data.RodPods[num];
				presentPod.Controller.PutTpmModel(rodPodData2.Position, rodPodData2.Yaw);
				IL_206:
				j--;
				continue;
				IL_186:
				if (num != -1)
				{
					if (presentPod.State == RodAnimator.ResourcesState.Created)
					{
						ThirdPersonData.RodPodData rodPodData3 = data.RodPods[num];
						presentPod.Controller.transform.position = rodPodData3.Position;
					}
					goto IL_206;
				}
				if (presentPod.State == RodAnimator.ResourcesState.Created)
				{
					Object.Destroy(presentPod.Controller.gameObject);
					this._rodPods.RemoveAt(j);
					goto IL_206;
				}
				goto IL_206;
			}
		}

		private void ProcessRods(ThirdPersonData data, float dtPrc)
		{
			for (int i = 0; i < data.RodsOnPods.Count; i++)
			{
				ThirdPersonData.RodData newRod = data.RodsOnPods[i];
				if (this._rodsOnPods.All((RodAnimator.RodData r) => r.Id != newRod.Id))
				{
					RodAnimator.RodData rodData = new RodAnimator.RodData
					{
						Id = newRod.Id
					};
					this._rodsOnPods.Add(rodData);
					RodInitialize.AsyncHelper.StartCoroutine(this.AsyncLoadRod(newRod.rodAssembly, rodData));
				}
			}
			for (int j = this._rodsOnPods.Count - 1; j >= 0; j--)
			{
				RodAnimator.RodData presentRod = this._rodsOnPods[j];
				if (presentRod.State == RodAnimator.ResourcesState.Loaded)
				{
					this.CreateRod(presentRod, false);
					presentRod.Rod.transform.SetParent(this._rootTransform);
				}
				int num = data.RodsOnPods.FindIndex((ThirdPersonData.RodData r) => r.Id == presentRod.Id);
				if (num != -1)
				{
					if (presentRod.State == RodAnimator.ResourcesState.Created)
					{
						ThirdPersonData.RodData rodData2 = data.RodsOnPods[num];
						this._rodsOnPods[j].Rod.ServerUpdate(rodData2.rodAssembly, dtPrc);
						this._rodsOnPods[j].Line.ServerUpdate(rodData2.rodAssembly, true, false, true, false, data.IsPhotoMode, dtPrc);
						this._rodsOnPods[j].Tackle.ServerUpdate(rodData2.rodAssembly.tackleData, true, dtPrc);
					}
				}
				else if (presentRod.State == RodAnimator.ResourcesState.Created)
				{
					this.DestroyRod(presentRod);
					this._rodsOnPods.RemoveAt(j);
				}
			}
		}

		private void ProcessFishes(ThirdPersonData data, float dtPrc)
		{
			ThirdPersonData.FishData fishData = null;
			for (int j = 0; j < data.FishesAndItems.Count; j++)
			{
				ThirdPersonData.FishData fishOrItem = data.FishesAndItems[j];
				if (fishOrItem.state == TPMFishState.UnderwaterItem || fishOrItem.state == TPMFishState.UnderwaterItemShowing)
				{
					fishData = fishOrItem;
					if (this._underwaterItem == null)
					{
						GameObject gameObject = Resources.Load<GameObject>(fishOrItem.template.ItemAsset);
						UnderwaterItemController component = Object.Instantiate<GameObject>(gameObject).GetComponent<UnderwaterItemController>();
						this._underwaterItem = component.Init(fishOrItem.template.FishId, Vector3.zero, UserBehaviours.ThirdPerson, null, null) as UnderwaterItem3rdBehaviour;
					}
				}
				else if (this._fishBehaviours.All((Fish3rdBehaviour f) => f.Id != fishOrItem.Id))
				{
					GameObject gameObject2 = FishSpawner.LoadFishPrefab(fishOrItem.template.Asset);
					if (gameObject2 == null)
					{
						throw new PrefabException("Can't instantiate fish or item from asset: " + fishOrItem.template.Asset);
					}
					Fish3rdBehaviour fish3rdBehaviour = FishSpawner.GenerateFish(fishOrItem.template, null, fishOrItem.position, Quaternion.identity, UserBehaviours.ThirdPerson, gameObject2, null) as Fish3rdBehaviour;
					fish3rdBehaviour.Id = data.FishesAndItems[j].Id;
					this._fishBehaviours.Add(fish3rdBehaviour);
					fish3rdBehaviour.SetVisibility(this._visibility);
				}
			}
			if (this._underwaterItem != null)
			{
				if (fishData == null)
				{
					this.DestroyUnderwaterItem();
				}
				else
				{
					this._underwaterItem.Update(fishData, dtPrc);
				}
			}
			int i;
			for (i = this._fishBehaviours.Count - 1; i >= 0; i--)
			{
				int num = data.FishesAndItems.FindIndex((ThirdPersonData.FishData f) => f.Id == this._fishBehaviours[i].Id);
				if (num != -1)
				{
					this._fishBehaviours[i].UpdateFish(data.FishesAndItems[num], dtPrc);
				}
				else
				{
					this._fishBehaviours[i].Destroy();
					this._fishBehaviours.RemoveAt(i);
				}
			}
		}

		public void LateUpdate(bool inGame)
		{
			if (this._boat != null && this._boat.BoatModel != null)
			{
				BoatShaderParametersController.SetBoatParameters(this._boat.MaskType, this._boat.BoatModel);
			}
			if (this._lastServerData != null)
			{
				float dtPrc = this.GetDtPrc();
				this._boat.SyncUpdate(dtPrc);
				this.UpdatePosition(dtPrc);
				for (int i = 0; i < this._rodsOnPods.Count; i++)
				{
					RodAnimator.RodData rodData = this._rodsOnPods[i];
					if (rodData.State == RodAnimator.ResourcesState.Created)
					{
						rodData.Rod.SyncUpdate(dtPrc);
						rodData.Line.RodSyncUpdate(rodData.Rod, null, rodData.Reel.ReelHandler.LineArcLocator.transform.position, dtPrc);
						rodData.Tackle.RodSyncUpdate(rodData.Line, dtPrc);
					}
				}
				float num = 0f;
				try
				{
					num = ((this._curRod.Rod == null) ? 0f : this._animator.GetFloat(this._ikCurveHashes[(this._curRod.RodAssembly.ReelType != ReelTypes.Baitcasting) ? 0 : 1]));
				}
				catch (Exception ex)
				{
				}
				this._limbIk.enabled = num > 0f && !this._lastServerData.IsPhotoMode;
				this._limbIk.solver.IKPositionWeight = num;
				this._limbIk.solver.IKRotationWeight = num;
				if (!inGame)
				{
					return;
				}
				if (this._curRod.State == RodAnimator.ResourcesState.Created)
				{
					this._curRod.Rod.SyncUpdate(dtPrc);
					this._curRod.Line.RodSyncUpdate(this._curRod.Rod, this._fishBehaviours, this.LineArcLocator, dtPrc);
					this._curRod.Tackle.RodSyncUpdate(this._curRod.Line, dtPrc);
					float num2 = ((!this._lastServerData.IsPhotoMode) ? 0f : Math3d.GetVectorsYaw(this._lastServerData.playerRotation * Vector3.forward, this._positionCorrectors.Forward));
					for (int j = 0; j < this._fishBehaviours.Count; j++)
					{
						this._fishBehaviours[j].SyncUpdate(this._curRod.Tackle, this._grip, num2, dtPrc);
					}
					if (this._underwaterItem != null)
					{
						this._underwaterItem.SyncUpdate(this._curRod.Tackle, dtPrc);
					}
				}
			}
		}

		private void DestroyUnderwaterItem()
		{
			if (this._underwaterItem != null)
			{
				Object.Destroy(this._underwaterItem.Owner.gameObject);
				this._underwaterItem = null;
			}
		}

		private void UpdatePosition(float dtPrc)
		{
			if (this._boat.BoatModel != null)
			{
				this._charRoot.rotation = this._boat.PlayerPivot.rotation;
				this._charRoot.localPosition = ((!this._lastServerData.BoolParameters[8] || !(this._boat.AnglerPivot != null)) ? this._boat.PlayerPivot.position : this._boat.AnglerPivot.position);
			}
			else
			{
				if ((this._prevPlayerPosition - this._lastServerData.playerPosition).magnitude > 1f)
				{
					dtPrc = 1f;
				}
				this._charRoot.localPosition = Vector3.Lerp(this._prevPlayerPosition, this._lastServerData.playerPosition, dtPrc) + this._positionCorrectors.ModelGroundCorrection;
				this._charRoot.rotation = Quaternion.identity;
			}
		}

		public override void OnDestroy()
		{
			this._lastServerData = null;
			this.DestroyRod(this._curRod);
			this._boat.OnDestroy();
			for (int i = this._fishBehaviours.Count - 1; i >= 0; i--)
			{
				this._fishBehaviours[i].Destroy();
			}
			this._fishBehaviours.Clear();
			for (int j = 0; j < this._rodPods.Count; j++)
			{
				RodAnimator.RodPodData rodPodData = this._rodPods[j];
				if (rodPodData.State == RodAnimator.ResourcesState.Created)
				{
					Object.Destroy(rodPodData.Controller.gameObject);
				}
			}
			this._rodPods.Clear();
			for (int k = 0; k < this._rodsOnPods.Count; k++)
			{
				RodAnimator.RodData rodData = this._rodsOnPods[k];
				if (rodData.State == RodAnimator.ResourcesState.Created)
				{
					this.DestroyRod(rodData);
				}
			}
			this._rodsOnPods.Clear();
			base.OnDestroy();
		}

		public const string REEL_ANIMATOR_PATH = "TPM/Animator/ReelPrototypeAnimator";

		private TPMBoat _boat;

		private readonly List<Fish3rdBehaviour> _fishBehaviours;

		private UnderwaterItem3rdBehaviour _underwaterItem;

		private ThirdPersonData _lastServerData;

		private float _lastUpdateTime;

		private readonly IPositionCorrectors _positionCorrectors;

		private Vector3 _prevPlayerPosition;

		private Quaternion _prevPlayerRotation;

		private Transform _charRoot;

		private Transform _rootTransform;

		private bool _visibility = true;

		private RodAnimator.RodData _curRod = new RodAnimator.RodData();

		private List<RodAnimator.RodData> _rodsOnPods = new List<RodAnimator.RodData>();

		private List<RodAnimator.RodPodData> _rodPods = new List<RodAnimator.RodPodData>();

		private GripSettings _grip;

		private LimbIK _limbIk;

		private int[] _ikCurveHashes = new int[2];

		private string _playerName;

		private ChumBall _pChumBall;

		private ChumBall _chumBall;

		private Vector3 _lineWithFishDisplacement;

		private float _nextChumBallAt = -1f;

		private const float CHUM_BALL_THROWING_DELAY = 5f;

		public enum ResourcesState
		{
			None,
			Loading,
			Loaded,
			Created
		}

		public class RodData
		{
			public int Id;

			public TPMAssembledRod RodAssembly = new TPMAssembledRod();

			public RodAnimator.ResourcesState State;

			public GameFactory.RodSlot Slot;

			public Rod3rdBehaviour Rod;

			public Reel3rdBehaviour Reel;

			public Line3rdBehaviour Line;

			public TackleBehaviour Tackle;

			public Bell3rdBehaviour Bell;

			public GameObject RodPrefab;

			public GameObject ReelPrefab;

			public AnimatorOverrideController ReelAnimatorPrefab;

			public GameObject TacklePrefab;

			public GameObject SecondaryTacklePrefab;

			public GameObject BellPrefab;
		}

		public class RodPodData
		{
			public string AssetPath
			{
				get
				{
					ItemAssetInfo itemAssetPath = CacheLibrary.AssetsCache.GetItemAssetPath(this.AssetId);
					if (itemAssetPath == null)
					{
						return null;
					}
					return itemAssetPath.Asset;
				}
			}

			public int Id;

			public RodAnimator.ResourcesState State;

			public RodPodController Controller;

			public int AssetId;

			public GameObject Prefab;
		}
	}
}
