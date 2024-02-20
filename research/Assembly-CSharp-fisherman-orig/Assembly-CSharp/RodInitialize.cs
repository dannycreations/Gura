using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

public class RodInitialize
{
	public static MonoBehaviour AsyncHelper
	{
		get
		{
			return MenuHelpers.Instance.MenuPrefabsList;
		}
	}

	public static void InitRod(AssembledRod rod, PlayerController playerController, Action<AssembledRod> OnRodCreated, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		if (!PhotonConnectionFactory.Instance.Profile.Settings.ContainsKey("LastActiveRodSlot"))
		{
			PhotonConnectionFactory.Instance.Profile.Settings.Add("LastActiveRodSlot", rod.Rod.Slot.ToString());
		}
		else
		{
			PhotonConnectionFactory.Instance.Profile.Settings["LastActiveRodSlot"] = rod.Rod.Slot.ToString();
		}
		List<string> list = new List<string>();
		Inventory inventory = PhotonConnectionFactory.Instance.Profile.Inventory;
		for (int i = 0; i < inventory.Count; i++)
		{
			if (inventory[i] is Reel)
			{
				list.Add(inventory[i].InstanceId.ToString());
			}
		}
		ReelSettingsPersister.PackReelSettings(PhotonConnectionFactory.Instance.Profile.Settings, list);
		RodInitialize.AsyncHelper.StartCoroutine(RodInitialize.LoadItemsAsync(rod, playerController, OnRodCreated, transitionData));
	}

	public static void InitRodOnPod(RodPodController pod, GameFactory.RodSlot oldSlot, int rodPodSlot, RodOnPodBehaviour.TransitionData transitionData)
	{
		GameObject rodObject = transitionData.rodObject;
		GameObject reelObject = transitionData.reelObject;
		GameObject bellObject = transitionData.bellObject;
		GameObject tackleObject = transitionData.tackleObject;
		GameObject lineObject = transitionData.lineObject;
		GameObject underwaterItemObject = transitionData.underwaterItemObject;
		GameObject hookObject = transitionData.hookObject;
		GameFactory.RodSlot rodSlot = new GameFactory.RodSlot(oldSlot);
		RodInitialize.SetRodOnPodItems(pod, rodSlot, rodPodSlot, transitionData, rodObject, reelObject, tackleObject, bellObject, underwaterItemObject);
		GameFactory.RodSlots[rodSlot.Index] = rodSlot;
	}

	public static void DestroyRod(PlayerController playerController)
	{
		GameFactory.RodSlot rodSlot = playerController.RodSlot;
		if (playerController.Rod == null)
		{
			return;
		}
		if (playerController.Reel != null)
		{
			playerController.Reel.StopSounds();
			playerController.DestroyObject(playerController.Reel.gameObject);
		}
		if (playerController.Bell != null)
		{
			playerController.DestroyObject(playerController.Bell.gameObject);
		}
		if (playerController.Line != null)
		{
			if (playerController.Line.Sinkers != null)
			{
				foreach (GameObject gameObject in playerController.Line.Sinkers)
				{
					playerController.DestroyObject(gameObject);
				}
				playerController.Line.Sinkers = null;
			}
			playerController.DestroyObject(playerController.Line.gameObject);
		}
		if (playerController.RodObject != null)
		{
			playerController.DestroyObject(playerController.RodObject);
		}
		if (playerController.Tackle != null)
		{
			playerController.Tackle.Destroy();
			playerController.DestroyObject(playerController.Tackle.gameObject);
		}
		rodSlot.Clear();
	}

	public static IEnumerator LoadAndReplaceHQTextures(Renderer[] renderers)
	{
		for (int ri = 0; ri < renderers.Length; ri++)
		{
			Renderer r = renderers[ri];
			if (r == null)
			{
				string text = string.Format("renderers[{0}] is null", ri);
				LogHelper.Error(text, new object[0]);
				PhotonConnectionFactory.Instance.PinError(text, "RodInitialize.LoadAndReplaceHQTextures");
			}
			else
			{
				TexturesLocator locator = r.transform.parent.GetComponent<TexturesLocator>();
				if (locator == null)
				{
					LogHelper.Error("No TexturesLocator for {0}. Generate it throw Tools->Textures->Add TexturesLocator", new object[] { r.transform.parent.name });
				}
				else
				{
					string path = locator.Path;
					foreach (Material i in r.materials)
					{
						for (int j = 0; j < GlobalConsts.PossibleTextures.Length; j++)
						{
							Texture t = i.GetTexture(GlobalConsts.PossibleTextures[j]);
							if (t != null)
							{
								string hqPath = string.Format("{0}{1}_HQ", path, t.name);
								ResourceRequest request = Resources.LoadAsync<Texture2D>(hqPath);
								yield return request;
								Texture2D texture = request.asset as Texture2D;
								if (texture != null)
								{
									i.SetTexture(GlobalConsts.PossibleTextures[j], texture);
								}
							}
						}
					}
				}
			}
		}
		yield break;
	}

	private static void SetRodOnPodItems(RodPodController rodpodController, GameFactory.RodSlot slot, int rodPodSlot, RodOnPodBehaviour.TransitionData transitionData, GameObject rodAsset, GameObject reelAsset, GameObject tackleAsset, GameObject bellAsset = null, GameObject underwaterItemAsset = null)
	{
		slot.Sim.Clear();
		if (reelAsset != null)
		{
			reelAsset.transform.parent = null;
		}
		ReelController reelController = RodInitialize.CreateReel(reelAsset, transitionData.rodAssembly, slot, rodpodController.transform, UserBehaviours.None, transitionData);
		reelController.gameObject.SetActive(true);
		if (transitionData.bellObject != null)
		{
			transitionData.bellObject.transform.parent = null;
		}
		if (transitionData.baitObject != null)
		{
			transitionData.baitObject.transform.parent = null;
		}
		LineController lineController = RodInitialize.CreateLine(transitionData.rodAssembly, slot, reelController.gameObject, rodpodController.transform, UserBehaviours.None, false);
		TackleControllerBase tackleControllerBase = RodInitialize.CreateTackle(tackleAsset, transitionData.rodAssembly, slot, UserBehaviours.RodPod, transitionData, transitionData.secondaryTackleObject);
		tackleControllerBase.gameObject.SetActive(true);
		rodpodController.PrepareTransitionData(rodPodSlot, reelController.Behaviour, tackleControllerBase.Behaviour, lineController.Behaviour, transitionData);
		RodController rodController = RodInitialize.CreateRod(rodAsset, transitionData.rodAssembly, slot, UserBehaviours.RodPod, transitionData, false, null, null);
		BellBehaviour bellBehaviour = null;
		if (bellAsset != null)
		{
			BellController bellController = RodInitialize.CreateBell(bellAsset, transitionData.rodAssembly, slot, rodController.transform, UserBehaviours.RodPod);
			if (bellController != null)
			{
				bellBehaviour = bellController.Behaviour;
				bellController.gameObject.SetActive(true);
			}
		}
		rodController.gameObject.SetActive(true);
		Quaternion reelRotationByType = RodInitialize.GetReelRotationByType(transitionData.rodAssembly.ReelType);
		reelController.transform.parent = rodController.transform;
		reelController.transform.localPosition = Vector3.zero;
		reelController.transform.localRotation = reelRotationByType;
		reelController.transform.localScale = Vector3.one;
		rodpodController.OccupySlot(slot, rodPodSlot, rodController, reelController.Behaviour, tackleControllerBase.Behaviour, lineController.Behaviour, bellBehaviour, transitionData);
		rodController.Behaviour.Init(transitionData);
		reelController.Behaviour.Init();
		if (bellBehaviour != null)
		{
			bellBehaviour.Init();
		}
		lineController.Behaviour.Init(transitionData);
		tackleControllerBase.Behaviour.Init();
		tackleControllerBase.Behaviour.CreateTackle(Vector3.down);
	}

	private static IEnumerator LoadItemsAsync(AssembledRod rodAssembly, PlayerController playerController, Action<AssembledRod> OnRodCreated, RodOnPodBehaviour.TransitionData transitionData)
	{
		float startTimestamp = Time.time;
		Debug.LogWarning(string.Format("LoadItemsAsync #{0} started. Timestamp = {1}", rodAssembly.Slot, startTimestamp));
		if (PhotonConnectionFactory.Instance.Profile.PondId != 2)
		{
			FishWaterTile.DoingRenderWater = false;
		}
		RodInitialize.DestroyRod(playerController);
		GameFactory.RodSlot oldSlot = GameFactory.RodSlots[rodAssembly.Slot];
		if (oldSlot.IsRodAssembling)
		{
			LogHelper.Error("LoadItemsAsync is called on RodSlot {0} when another LoadItemsAsync is still running on that slot.", new object[] { rodAssembly.Slot });
			yield return null;
		}
		GameFactory.RodSlot slot = new GameFactory.RodSlot(GameFactory.RodSlots[rodAssembly.Slot]);
		playerController.SetRodSlot(slot);
		if (rodAssembly.Rod.ItemId != oldSlot.RodItemId || rodAssembly.Reel.ItemId != oldSlot.ReelItemId || rodAssembly.Line.ItemId != oldSlot.LineItemId)
		{
			slot.LineClips.Clear();
		}
		slot.BeginRodAssembly();
		GameFactory.RodSlots[slot.Index] = slot;
		slot.Sim.Clear();
		GameObject bellAsset = null;
		GameObject tackleAsset = null;
		GameObject secondaryTackleAsset = null;
		GameObject underwaterItemAsset = null;
		GameObject pPatchAnimationsList = null;
		Vector3 rodPos = Vector3.zero;
		Quaternion rodRot = Quaternion.identity;
		GameObject rodAsset;
		GameObject reelAsset;
		if (transitionData == null)
		{
			ResourceRequest request = Resources.LoadAsync<GameObject>(rodAssembly.RodInterface.Asset);
			yield return request;
			rodAsset = request.asset as GameObject;
			request = Resources.LoadAsync<GameObject>(rodAssembly.ReelInterface.Asset);
			yield return request;
			reelAsset = request.asset as GameObject;
			if (rodAssembly.BellInterface != null)
			{
				request = Resources.LoadAsync<GameObject>(rodAssembly.BellInterface.Asset);
				yield return request;
				bellAsset = request.asset as GameObject;
			}
			request = Resources.LoadAsync<GameObject>("pPatchAnimationsList");
			yield return request;
			pPatchAnimationsList = request.asset as GameObject;
			IEnumerator tackleRequest = RodInitialize.LoadTackleAsync(rodAssembly, delegate(GameObject asset)
			{
				tackleAsset = asset;
			}, delegate(GameObject asset)
			{
				secondaryTackleAsset = asset;
			});
			yield return tackleRequest;
		}
		else
		{
			rodAsset = transitionData.rodObject;
			rodPos = rodAsset.transform.position;
			rodRot = rodAsset.transform.rotation;
			reelAsset = transitionData.reelObject;
			if (reelAsset != null)
			{
				reelAsset.transform.parent = null;
			}
			tackleAsset = transitionData.tackleObject;
			bellAsset = transitionData.bellObject;
			if (bellAsset != null)
			{
				bellAsset.transform.parent = null;
			}
			underwaterItemAsset = transitionData.underwaterItemObject;
			if (underwaterItemAsset != null)
			{
				underwaterItemAsset.transform.parent = null;
			}
			secondaryTackleAsset = transitionData.secondaryTackleObject;
			transitionData.OnCreated();
		}
		Transform handRoot = playerController.GetRodRootBone(PlayerController.Hand.Right);
		GameObject go = new GameObject("!!!" + handRoot.name + Time.frameCount);
		go.transform.parent = null;
		go.transform.position = handRoot.position;
		go.transform.rotation = handRoot.rotation;
		Transform rt = ((transitionData != null) ? null : playerController.GetRodRootBone(PlayerController.Hand.Right));
		GameObject gameObject = rodAsset;
		GameFactory.RodSlot rodSlot = slot;
		UserBehaviours userBehaviours = UserBehaviours.FirstPerson;
		Transform transform = rt;
		RodBehaviour rodBehaviour = RodInitialize.CreateRod(gameObject, rodAssembly, rodSlot, userBehaviours, transitionData, false, null, transform).Behaviour;
		slot.SetRod(rodBehaviour);
		playerController.RodObject.SetActive(playerController.State != typeof(PlayerEmpty));
		slot.SetReel(RodInitialize.CreateReel(reelAsset, rodAssembly, slot, playerController.RodObject.transform, UserBehaviours.FirstPerson, transitionData).Behaviour);
		if (slot.Reel == null)
		{
			FishWaterTile.DoingRenderWater = true;
			throw new NullReferenceException("Reel '" + rodAssembly.Reel.Asset + "' does not have ReelController on it!");
		}
		if (bellAsset != null)
		{
			slot.SetBell(RodInitialize.CreateBell(bellAsset, rodAssembly, slot, slot.Rod.gameObject.transform, UserBehaviours.FirstPerson).Behaviour);
		}
		slot.SetLine(RodInitialize.CreatePlayerLine(rodAssembly, slot.Reel.gameObject, playerController, slot).Behaviour);
		slot.SetTackle(RodInitialize.CreateTackle(tackleAsset, rodAssembly, slot, UserBehaviours.FirstPerson, transitionData, secondaryTackleAsset).Behaviour);
		playerController.refreshRodSlotReferences();
		RodInitialize._renderers[0] = RenderersHelper.GetRendererForObject<Renderer>(slot.Rod.gameObject.transform);
		if (RodInitialize._renderers[0] == null)
		{
			LogHelper.Error("RodInitialize: Cannot find Renderer in Rod {0}", new object[] { slot.Rod.gameObject.name });
		}
		RodInitialize._renderers[1] = RenderersHelper.GetRendererForObject<Renderer>(slot.Reel.gameObject.transform);
		if (RodInitialize._renderers[1] == null)
		{
			LogHelper.Error("RodInitialize: Cannot find Renderer in Reel {0}", new object[] { slot.Reel.gameObject.name });
		}
		IEnumerator hqTexturesRequest = RodInitialize.LoadAndReplaceHQTextures(RodInitialize._renderers);
		yield return hqTexturesRequest;
		if (pPatchAnimationsList != null)
		{
			AnimClips component = pPatchAnimationsList.GetComponent<AnimClips>();
			Animation component2 = slot.Reel.gameObject.GetComponent<Animation>();
			for (int i = 0; i < component.ReelClips.Length; i++)
			{
				AnimationClip animationClip = component.ReelClips[i];
				component2.AddClip(animationClip, animationClip.name);
			}
		}
		else if (transitionData != null)
		{
			Animation component3 = reelAsset.GetComponent<Animation>();
			Animation component4 = slot.Reel.gameObject.GetComponent<Animation>();
			List<string> list = new List<string>();
			IEnumerator enumerator = component3.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					AnimationState animationState = (AnimationState)obj;
					list.Add(animationState.name);
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
			for (int j = 0; j < list.Count; j++)
			{
				component4.AddClip(component3.GetClip(list[j]), list[j]);
			}
		}
		if (slot.Tackle.RodTemplate == RodTemplate.Float && GameFactory.BobberIndicator != null)
		{
			GameFactory.BobberIndicator.Sensitivity = rodAssembly.Bobber.Sensitivity;
		}
		StaticUserData.RodInHand = rodAssembly;
		if (transitionData != null)
		{
			slot.Rod.transform.rotation = rodRot;
			slot.Rod.transform.position = rodPos;
			slot.Rod.Controller.enabled = true;
			playerController.DestroyObject(rodAsset);
			playerController.DestroyObject(reelAsset);
			playerController.DestroyObject(bellAsset);
			playerController.DestroyObject(tackleAsset);
			playerController.DestroyObject(transitionData.lineObject);
			playerController.DestroyObject(secondaryTackleAsset);
			if (transitionData.hookObject != null)
			{
				playerController.DestroyObject(transitionData.hookObject);
			}
			for (int k = 0; k < transitionData.sinkers.Count; k++)
			{
				playerController.DestroyObject(transitionData.sinkers[k]);
			}
		}
		slot.Rod.Init(transitionData);
		slot.Reel.Init();
		slot.Line.Init(transitionData);
		slot.Tackle.Init();
		if (slot.Bell != null)
		{
			slot.Bell.Init();
		}
		if (slot.Rod.IsQuiver)
		{
			GameFactory.QuiverIndicator.Init(slot.Rod as Rod1stBehaviour);
		}
		LogHelper.Log("playerController.ChangeRodHandler({0})", new object[] { rodAssembly.Rod.Name });
		FishWaterTile.DoingRenderWater = true;
		slot.FinishRodAssembly();
		GameFactory.FishSpawner.ProcessWaitingEvents(rodAssembly.Slot);
		if (OnRodCreated != null)
		{
			OnRodCreated(rodAssembly);
		}
		Debug.LogWarning(string.Format("LoadItemsAsync finishes on {0}", rodAssembly.Slot));
		yield break;
	}

	public static RodController CreateRod(GameObject rodPrefab, IAssembledRod rodAssembly, GameFactory.RodSlot slot, UserBehaviours userBehaviour, RodOnPodBehaviour.TransitionData td = null, bool isMain = false, string playerName = null, Transform rootTransform = null)
	{
		if (rodPrefab == null)
		{
			throw new PrefabException(string.Format("rodAssembly: {0} prefab can't instantiate", rodAssembly.RodInterface.Asset));
		}
		GameObject gameObject = Object.Instantiate<GameObject>(rodPrefab);
		gameObject.name = rodPrefab.name;
		if (rootTransform != null)
		{
			gameObject.transform.parent = rootTransform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
		}
		else if (td != null)
		{
			gameObject.transform.position = td.rootPosition;
			gameObject.transform.rotation = td.rootRotation;
		}
		RodController component = gameObject.GetComponent<RodController>();
		QuiverTip quiverTip = ((!(rodAssembly is AssembledRod)) ? null : (rodAssembly as AssembledRod).Quiver);
		if (quiverTip != null)
		{
			component.segment.quiverLength = 0.3f;
			component.segment.quiverMass = 0.007f;
			component.segment.quiverTest = quiverTip.Test;
		}
		else
		{
			component.segment.quiverLength = 0f;
			component.segment.quiverMass = 0f;
			component.segment.quiverTest = 1f;
		}
		component.SetBehaviour(userBehaviour, rodAssembly, slot, isMain, td, playerName);
		if (quiverTip != null)
		{
			component.Behaviour.QuiverTipColorName = quiverTip.Color;
		}
		return component;
	}

	public static ReelController CreateReel(GameObject reelPrefab, IAssembledRod rodAssembly, GameFactory.RodSlot slot, Transform rodTransform, UserBehaviours userBehaviour, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		if (reelPrefab == null)
		{
			throw new PrefabException(string.Format("reel: {0} prefab can't instantiate", rodAssembly.ReelInterface.Asset));
		}
		GameObject gameObject = Object.Instantiate<GameObject>(reelPrefab);
		gameObject.name = reelPrefab.name;
		Quaternion reelRotationByType = RodInitialize.GetReelRotationByType(rodAssembly.ReelType);
		gameObject.transform.parent = rodTransform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = reelRotationByType;
		gameObject.transform.localScale = Vector3.one;
		ReelController component = gameObject.GetComponent<ReelController>();
		component.SetBehaviour(userBehaviour, rodAssembly, slot, transitionData);
		return component;
	}

	public static BellController CreateBell(GameObject bellPrefab, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot, Transform rodTransform, UserBehaviours userBehaviour)
	{
		if (bellPrefab == null)
		{
			throw new PrefabException(string.Format("bell: {0} prefab can't instantiate", rodAssembly.BellInterface.Asset));
		}
		GameObject gameObject = Object.Instantiate<GameObject>(bellPrefab);
		gameObject.name = bellPrefab.name;
		gameObject.transform.parent = rodTransform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		BellController component = gameObject.GetComponent<BellController>();
		component.SetBehaviour(userBehaviour, rodAssembly, rodSlot);
		return component;
	}

	public static TackleControllerBase CreateTackle(GameObject prefab, IAssembledRod rodAssembly, GameFactory.RodSlot slot, UserBehaviours userBehaviour, RodOnPodBehaviour.TransitionData transitionData = null, GameObject secondaryPrefab = null)
	{
		if (rodAssembly.RodTemplate == RodTemplate.Lure || rodAssembly.RodTemplate.IsTails() || rodAssembly.RodTemplate.IsSilicon())
		{
			if (prefab == null)
			{
				throw new PrefabException(string.Format("lure: {0} prefab can't be instantiated", rodAssembly.HookInterface.Asset));
			}
			GameObject gameObject = Object.Instantiate<GameObject>(prefab);
			gameObject.name = prefab.name;
			LureController component = gameObject.GetComponent<LureController>();
			if (component == null)
			{
				throw new PrefabConfigException(string.Format("Prefab {0} has no LureController on it", rodAssembly.HookInterface.Asset));
			}
			component.SetBehaviour(userBehaviour, rodAssembly, slot, transitionData);
			return component;
		}
		else if (rodAssembly.RodTemplate == RodTemplate.Float)
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity);
			gameObject2.name = prefab.name;
			FloatController component2 = gameObject2.GetComponent<FloatController>();
			if (component2 == null)
			{
				throw new PrefabConfigException(string.Format("Prefab {0} has no FloatController on it", rodAssembly.BobberInterface.Asset));
			}
			component2.SetBehaviour(userBehaviour, rodAssembly, slot, transitionData);
			return component2;
		}
		else if (rodAssembly.RodTemplate.IsChumFishingTemplate())
		{
			GameObject gameObject3 = Object.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity);
			gameObject3.name = prefab.name;
			GameObject gameObject4 = null;
			if (secondaryPrefab != null)
			{
				gameObject4 = Object.Instantiate<GameObject>(secondaryPrefab, Vector3.zero, Quaternion.identity);
			}
			FeederController component3 = gameObject3.GetComponent<FeederController>();
			if (component3 == null)
			{
				throw new PrefabConfigException(string.Format("Prefab {0} has no FeederController on it", rodAssembly.SinkerInterface.Asset));
			}
			component3.SecondaryTackleObject = gameObject4;
			component3.SetBehaviour(userBehaviour, rodAssembly, slot, transitionData);
			return component3;
		}
		else
		{
			if (rodAssembly.RodTemplate != RodTemplate.Jig)
			{
				return null;
			}
			IBait baitInterface = rodAssembly.BaitInterface;
			if (prefab == null)
			{
				throw new PrefabException(string.Format("jig: {0} prefab can't be instantiated", baitInterface.Asset));
			}
			GameObject gameObject5 = Object.Instantiate<GameObject>(prefab);
			gameObject5.name = prefab.name;
			LureController component4 = gameObject5.GetComponent<LureController>();
			if (component4 == null)
			{
				throw new PrefabConfigException(string.Format("Prefab {0} has no LureController on it", baitInterface.Asset));
			}
			component4.SetBehaviour(userBehaviour, rodAssembly, slot, transitionData);
			return component4;
		}
	}

	public static IEnumerator LoadTackleAsync(IAssembledRod rodAssembly, Action<GameObject> callback, Action<GameObject> callbackSecondary = null)
	{
		if (rodAssembly.RodTemplate == RodTemplate.Lure || rodAssembly.RodTemplate.IsTails() || rodAssembly.RodTemplate.IsSilicon())
		{
			ResourceRequest request = Resources.LoadAsync<GameObject>(rodAssembly.HookInterface.Asset);
			yield return request;
			callback(request.asset as GameObject);
		}
		else if (rodAssembly.RodTemplate == RodTemplate.Float)
		{
			ResourceRequest request2 = Resources.LoadAsync<GameObject>(rodAssembly.BobberInterface.Asset);
			yield return request2;
			callback(request2.asset as GameObject);
		}
		else if (rodAssembly.RodTemplate.IsChumFishingTemplate())
		{
			ResourceRequest request3 = Resources.LoadAsync<GameObject>(rodAssembly.SinkerInterface.Asset);
			yield return request3;
			callback(request3.asset as GameObject);
			if (callbackSecondary != null && rodAssembly.FeederInterface != null && rodAssembly.FeederInterface.ItemId != rodAssembly.SinkerInterface.ItemId)
			{
				request3 = Resources.LoadAsync<GameObject>(rodAssembly.FeederInterface.Asset);
				yield return request3;
				callbackSecondary(request3.asset as GameObject);
			}
		}
		else if (rodAssembly.RodTemplate == RodTemplate.Jig)
		{
			ResourceRequest request4 = Resources.LoadAsync<GameObject>(rodAssembly.BaitInterface.Asset);
			yield return request4;
			callback(request4.asset as GameObject);
		}
		else
		{
			Debug.LogError("Rod with equipment of type " + Enum.GetName(typeof(RodTemplate), rodAssembly.RodTemplate) + " is not implemented!");
		}
		yield break;
	}

	private static LineController CreatePlayerLine(IAssembledRod rodAssembly, GameObject reelObject, PlayerController playerController, GameFactory.RodSlot slot)
	{
		if (playerController.Line != null)
		{
			if (playerController.Line.Sinkers != null)
			{
				foreach (GameObject gameObject in playerController.Line.Sinkers)
				{
					playerController.DestroyObject(gameObject);
				}
			}
			if (playerController.Line.gameObject != null)
			{
				playerController.DestroyObject(playerController.Line.gameObject);
			}
		}
		return RodInitialize.CreateLine(rodAssembly, slot, reelObject, playerController.Player.transform, UserBehaviours.FirstPerson, false);
	}

	public static LineController CreateLine(IAssembledRod rodAssembly, GameFactory.RodSlot slot, GameObject reelObject, Transform ownerTransform, UserBehaviours userBehaviour, bool activate)
	{
		ILine lineInterface = rodAssembly.LineInterface;
		ReelTypes reelType = rodAssembly.ReelType;
		GameObject gameObject = (GameObject)Resources.Load(lineInterface.Asset, typeof(GameObject));
		Debug.Log("Line name:" + lineInterface.Asset);
		if (gameObject == null)
		{
			throw new PrefabException(string.Format("line: {0} prefab can't instantiate", lineInterface.Asset));
		}
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
		gameObject2.name = gameObject.name;
		gameObject2.transform.parent = ownerTransform;
		gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
		Quaternion quaternion = default(Quaternion);
		quaternion.eulerAngles = new Vector3(0f, 0f, 0f);
		Quaternion quaternion2 = quaternion;
		gameObject2.transform.localRotation = quaternion2;
		gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
		LineController component = gameObject2.GetComponent<LineController>();
		LineBehaviour lineBehaviour = component.SetBehaviour(userBehaviour, rodAssembly, slot);
		if (reelObject == null)
		{
			return component;
		}
		if (string.IsNullOrEmpty(lineInterface.LineOnBaitcastSpoolAsset) && string.IsNullOrEmpty(lineInterface.LineOnSpoolAsset))
		{
			return component;
		}
		GameObject gameObject3 = null;
		Transform transform = null;
		if (reelType == ReelTypes.Baitcasting && !string.IsNullOrEmpty(lineInterface.LineOnBaitcastSpoolAsset))
		{
			gameObject3 = (GameObject)Resources.Load(lineInterface.LineOnBaitcastSpoolAsset, typeof(GameObject));
			transform = reelObject.transform.Find("root_baitcaster");
		}
		if (reelType == ReelTypes.Spinning && !string.IsNullOrEmpty(lineInterface.LineOnSpoolAsset))
		{
			gameObject3 = (GameObject)Resources.Load(lineInterface.LineOnSpoolAsset, typeof(GameObject));
			transform = reelObject.transform.Find("root");
		}
		if (gameObject3 == null || transform == null)
		{
			throw new PrefabException(string.Format("line: {0} prefab can't instantiate", lineInterface.Asset));
		}
		Transform transform2 = transform.Find("bobbin");
		GameObject gameObject4 = Object.Instantiate<GameObject>(gameObject3);
		gameObject4.name = gameObject3.name;
		gameObject4.transform.parent = transform2;
		gameObject4.transform.localPosition = new Vector3(0f, 0f, 0f);
		gameObject4.transform.localRotation = quaternion2;
		gameObject4.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject4.GetComponentInChildren<SkinnedMeshRenderer>().material.color = LineBehaviour.HexToColor(lineInterface.Color, LineBehaviour.DefaultLineColor);
		gameObject4.SetActive(true);
		return component;
	}

	private static Quaternion GetReelRotationByType(ReelTypes reelType)
	{
		if (reelType == ReelTypes.Spinning)
		{
			return Quaternion.identity;
		}
		if (reelType == ReelTypes.Baitcasting)
		{
			return Quaternion.Euler(0f, 0f, 180f);
		}
		throw new InvalidOperationException("Can't instatiate reel of type: " + Enum.GetName(typeof(ReelTypes), reelType));
	}

	public static Quaternion GetIdentityRotation(bool isBaitcasting)
	{
		return RodInitialize.GetReelRotationByType((!isBaitcasting) ? ReelTypes.Spinning : ReelTypes.Baitcasting);
	}

	public const string SinkerPrefab = "Tackle/Sinkers/DefaultSinker/pDefaultSinker";

	public const string BeadPrefab = "Tackle/Sinkers/RigBead/pRigBead_Red";

	public const string ShacklePrefab = "Tackle/Sinkers/Swivels/pShackle";

	public const string SwivelPrefab = "Tackle/Sinkers/Swivels/pSwivel";

	public const string StopperPrefab = "Tackle/Bobbers/Stoppers/pStopper_01";

	private static readonly Renderer[] _renderers = new Renderer[2];
}
