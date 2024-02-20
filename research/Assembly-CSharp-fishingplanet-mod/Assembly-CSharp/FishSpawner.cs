using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using Photon.Interfaces;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
	public Dictionary<Guid, IFishController> Fish
	{
		get
		{
			return this.fish;
		}
	}

	public List<Box> Shelters { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnGamePaused = delegate(bool b)
	{
	};

	public bool IsGamePaused { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<FishCaughtEventArgs> FishCaught;

	private void Awake()
	{
		if (GameFactory.FishSpawner != null)
		{
			Debug.LogError("Fish spawner must be only one!");
		}
		GameFactory.FishSpawner = this;
	}

	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnGameActionResult += this.OnGameActionResult;
		PhotonConnectionFactory.Instance.OnGameEvent += this.OnGameEvent;
		PhotonConnectionFactory.Instance.OnGameHint += this.OnGameHint;
		PhotonConnectionFactory.Instance.OnLicensePenalty += this.OnLicensePenalty;
		PhotonConnectionFactory.Instance.OnLicensePenaltyWarning += this.OnLicensePenaltyWarning;
		PhotonConnectionFactory.Instance.OnItemGained += this.OnItemGained;
		PhotonConnectionFactory.Instance.OnItemLost += this.OnItemLost;
		PhotonConnectionFactory.Instance.OnBaitLost += this.OnBaitLost;
		PhotonConnectionFactory.Instance.OnBaitReplenished += this.OnBaitReplenished;
		PhotonConnectionFactory.Instance.OnRoomIsFull += this.OnRoomIsFull;
		PhotonConnectionFactory.Instance.OnUnableToEnterRoom += this.OnUnableToEnterRoom;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnProductDelivered += this.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnExpGained += this.OnExpGained;
		PhotonConnectionFactory.Instance.OnOperationFailed += this.OnOperationFailed;
		PhotonConnectionFactory.Instance.OnRefreshSecurityTokenFailed += this.OnRefreshSecurityTokenFailed;
		PhotonConnectionFactory.Instance.OnPondStayProlonged += this.OnPondStayProlonged;
		PhotonConnectionFactory.Instance.OnLevelGained += this.OnLevelGained;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnPondStayFinish += this.OnPondStayFinish;
		PhotonConnectionFactory.Instance.OnRodUnequiped += this.OnRodUnequiped;
		CacheLibrary.MapCache.OnGetPond += this.MapCache_OnGetPond;
		for (int i = 0; i < GameFactory.RodSlots.Length; i++)
		{
			this._rodEventsQueue[i] = new Queue<Action>();
		}
		this.IsGamePaused = false;
		this.OnGamePaused(this.IsGamePaused);
		if (PhotonConnectionFactory.Instance.CurrentPondId == null)
		{
			return;
		}
		if (!PhotonConnectionFactory.Instance.PondInfos.ContainsKey(PhotonConnectionFactory.Instance.CurrentPondId.Value))
		{
			return;
		}
		this.ReadHitchBoxes(PhotonConnectionFactory.Instance.PondInfos[PhotonConnectionFactory.Instance.CurrentPondId.Value]);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGameActionResult -= this.OnGameActionResult;
		PhotonConnectionFactory.Instance.OnGameEvent -= this.OnGameEvent;
		PhotonConnectionFactory.Instance.OnGameHint -= this.OnGameHint;
		PhotonConnectionFactory.Instance.OnLicensePenalty -= this.OnLicensePenalty;
		PhotonConnectionFactory.Instance.OnLicensePenaltyWarning -= this.OnLicensePenaltyWarning;
		PhotonConnectionFactory.Instance.OnItemGained -= this.OnItemGained;
		PhotonConnectionFactory.Instance.OnItemLost -= this.OnItemLost;
		PhotonConnectionFactory.Instance.OnBaitLost -= this.OnBaitLost;
		PhotonConnectionFactory.Instance.OnBaitReplenished -= this.OnBaitReplenished;
		PhotonConnectionFactory.Instance.OnRoomIsFull -= this.OnRoomIsFull;
		PhotonConnectionFactory.Instance.OnUnableToEnterRoom -= this.OnUnableToEnterRoom;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnExpGained -= this.OnExpGained;
		PhotonConnectionFactory.Instance.OnOperationFailed -= this.OnOperationFailed;
		PhotonConnectionFactory.Instance.OnRefreshSecurityTokenFailed -= this.OnRefreshSecurityTokenFailed;
		PhotonConnectionFactory.Instance.OnPondStayProlonged -= this.OnPondStayProlonged;
		PhotonConnectionFactory.Instance.OnLevelGained -= this.OnLevelGained;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnPondStayFinish -= this.OnPondStayFinish;
		PhotonConnectionFactory.Instance.OnRodUnequiped -= this.OnRodUnequiped;
		CacheLibrary.MapCache.OnGetPond -= this.MapCache_OnGetPond;
	}

	private void ReadHitchBoxes(Pond pond)
	{
		if (pond != null)
		{
			this.Shelters = new List<Box>();
			for (int i = 0; i < pond.HitchBoxes.Length; i++)
			{
				global::ObjectModel.BoxInfo boxInfo = pond.HitchBoxes[i];
				this.Shelters.Add(new Box(boxInfo.Position, boxInfo.Scale, boxInfo.Rotation));
			}
		}
	}

	private void MapCache_OnGetPond(object sender, GlobalMapPondCacheEventArgs e)
	{
		if (this.Shelters == null && PhotonConnectionFactory.Instance.CurrentPondId != null && e.Pond.PondId == PhotonConnectionFactory.Instance.CurrentPondId)
		{
			this.ReadHitchBoxes(e.Pond);
		}
	}

	public void ProcessWaitingEvents(int slotId)
	{
		if (this._rodEventsQueue.ContainsKey(slotId) && this._rodEventsQueue[slotId].Count > 0)
		{
			this._rodEventsQueue[slotId].Dequeue()();
		}
		if (!this._waitingEvents.ContainsKey(slotId))
		{
			return;
		}
		List<GameEvent> list = this._waitingEvents[slotId];
		for (int i = 0; i < list.Count; i++)
		{
			GameEvent gameEvent = list[i];
			LogHelper.Log("spawn[{1}] ProcessWaitingEvents({0})", new object[] { gameEvent.EventCode, gameEvent.RodSlot });
			this.OnGameEvent(list[i]);
		}
		list.Clear();
	}

	private void SaveFishCaughtOnPond(Fish fish)
	{
		this.sb.Length = 0;
		int num = ((!(ShowPondInfo.Instance != null)) ? StaticUserData.CurrentPond.PondId : ShowPondInfo.Instance.CurrentPond.PondId);
		this.sb.Append(num);
		if (fish.IsTrophy != null && fish.IsTrophy.Value)
		{
			this.sb.Append("t");
		}
		else if (fish.IsYoung != null && fish.IsYoung.Value)
		{
			this.sb.Append("y");
		}
		else if (fish.IsUnique != null && fish.IsUnique.Value)
		{
			this.sb.Append("u");
		}
		else
		{
			this.sb.Append("c");
		}
		this.sb.Append(fish.FishId);
		ObscuredPrefs.SetBool(this.sb.ToString(), true);
	}

	private void OnGameEvent(GameEvent gameEvent)
	{
		GameFactory.RodSlot rodSlot = GameFactory.RodSlots[gameEvent.RodSlot];
		if (rodSlot.IsRodAssembling)
		{
			LogHelper.Log("spawn[{1}] OnGameEvent({0}) saved", new object[] { gameEvent.EventCode, gameEvent.RodSlot });
			this._rodEventsQueue[gameEvent.RodSlot].Enqueue(delegate
			{
				this.OnGameEvent(gameEvent);
			});
			return;
		}
		if (gameEvent.EventCode == 193)
		{
			if (rodSlot.Rod is Rod1stBehaviour)
			{
				base.StartCoroutine(this.SpawnAttackingFish(gameEvent.Fish, rodSlot, UserBehaviours.FirstPerson, null));
			}
			else
			{
				RodOnPodBehaviour rodOnPodBehaviour = (RodOnPodBehaviour)rodSlot.Rod;
				rodOnPodBehaviour.SpawnFish(gameEvent.Fish, null);
			}
			TackleBehaviour tackle = rodSlot.Tackle;
			tackle.IsStrikeTimedOut = false;
			tackle.IsAttackFinished = false;
			tackle.IsFinishAttackRequested = false;
		}
		if (gameEvent.EventCode == 189)
		{
			if (rodSlot.Rod is Rod1stBehaviour)
			{
				IFishController fishController = this.FindFishByTemplate(gameEvent.Fish);
				if (fishController != null)
				{
					fishController.Behavior = FishBehavior.Hook;
					rodSlot.Reel.SetFightMode();
				}
			}
			else
			{
				RodOnPodBehaviour rodOnPodBehaviour2 = (RodOnPodBehaviour)rodSlot.Rod;
				rodOnPodBehaviour2.HookFish();
			}
			return;
		}
		if (gameEvent.EventCode == 191)
		{
			if (rodSlot.Rod is Rod1stBehaviour)
			{
				IFishController fishController2 = this.FindFishByTemplate(gameEvent.Fish);
				if (fishController2 != null)
				{
					fishController2.Escape();
					rodSlot.Rod.ResetAppliedForce();
					rodSlot.Tackle.EscapeFish();
				}
				else
				{
					this.EscapeAllFish(gameEvent.RodSlot);
				}
				if (rodSlot.Tackle.IsStrikeTimedOut)
				{
					GameFactory.Message.ShowStrikeTimeoutExpired(RodHelper.FindRodInSlot(gameEvent.RodSlot, null));
				}
			}
			else
			{
				RodOnPodBehaviour rodOnPodBehaviour3 = (RodOnPodBehaviour)rodSlot.Rod;
				rodOnPodBehaviour3.EscapeFish();
			}
			return;
		}
		if (gameEvent.EventCode == 195)
		{
			rodSlot.Tackle.IsHitched = true;
			this.EscapeAllFish(gameEvent.RodSlot);
			return;
		}
		if (gameEvent.EventCode == 194)
		{
			rodSlot.Tackle.IsHitched = false;
			return;
		}
		if (gameEvent.EventCode == 155)
		{
			string text = (string)gameEvent.RawEventData["iA"];
			string text2 = (string)gameEvent.RawEventData["iN"];
			int num = (int)gameEvent.RawEventData["iD"];
			string text3 = (string)gameEvent.RawEventData["iC"];
			rodSlot.Tackle.AddItemAsync(num, text, text2, text3);
			rodSlot.Reel.SetHighSpeedMode();
			return;
		}
		if (gameEvent.EventCode == 148)
		{
			rodSlot.Tackle.CaughtItem = gameEvent.Item;
		}
		if (gameEvent.EventCode == 199)
		{
			string text4 = (string)gameEvent.RawEventData[194];
			Debug.LogWarning(string.Format("SRV: ITEM {0} LOST", text4));
		}
		if (gameEvent.EventCode == 192)
		{
			object obj = gameEvent.RawEventData["IsIllegal"];
			object obj2 = gameEvent.RawEventData["CanTake"];
			object obj3 = gameEvent.RawEventData["CanRelease"];
			int num2 = ((!gameEvent.RawEventData.ContainsKey("Fine")) ? 0 : ((int)gameEvent.RawEventData["Fine"]));
			string text5 = ((gameEvent.Fish.IsTrophy == null || !gameEvent.Fish.IsTrophy.Value) ? "N" : "Y");
			string text6 = ((gameEvent.Fish.IsUnique == null || !gameEvent.Fish.IsUnique.Value) ? "N" : "Y");
			this.SaveFishCaughtOnPond(gameEvent.Fish);
			if (rodSlot.Tackle.Fish == null)
			{
				string text7 = "No fish on client when fish is caught";
				PhotonConnectionFactory.Instance.PinError(text7, "FishSpawner.OnGameEvent = FishCaught");
			}
			else
			{
				if (rodSlot.Tackle.Fish.InstanceGuid != gameEvent.Fish.InstanceId)
				{
					string text8 = string.Format("FishCaught: Fighting fish ({0}, {1}) does not match with caught fish ({2}, {3})", new object[]
					{
						rodSlot.Tackle.Fish.InstanceGuid,
						rodSlot.Tackle.Fish.FishTemplate.Name,
						gameEvent.Fish.InstanceId,
						gameEvent.Fish.Name
					});
					PhotonConnectionFactory.Instance.PinError(text8, "FishSpawner.OnGameEvent = FishCaught");
				}
				if (this.lastSpawnEventFish.ContainsKey(rodSlot.Index))
				{
					Guid? instanceId = this.lastSpawnEventFish[rodSlot.Index].InstanceId;
					bool flag = instanceId != null;
					Guid? instanceId2 = gameEvent.Fish.InstanceId;
					if (flag != (instanceId2 != null) || (instanceId != null && instanceId.GetValueOrDefault() != instanceId2.GetValueOrDefault()))
					{
						string text9 = string.Format("FishCaught: Previously spawned fish ({0}, {1}) does not match caught fish ({2}, {3})", new object[]
						{
							this.lastSpawnEventFish[rodSlot.Index].InstanceId,
							this.lastSpawnEventFish[rodSlot.Index].Name,
							gameEvent.Fish.InstanceId,
							gameEvent.Fish.Name
						});
						PhotonConnectionFactory.Instance.PinError(text9, "FishSpawner.OnGameEvent = FishCaught");
					}
				}
				rodSlot.Tackle.Fish.CaughtFish = gameEvent.Fish;
				if (this.FishCaught != null)
				{
					this.FishCaught(this, new FishCaughtEventArgs
					{
						CaughtFish = gameEvent.Fish,
						IsIllegal = (obj != null && (bool)obj),
						IsTrophy = (gameEvent.Fish.IsTrophy != null && gameEvent.Fish.IsTrophy.Value),
						IsUnique = (gameEvent.Fish.IsUnique != null && gameEvent.Fish.IsUnique.Value),
						CanRelease = (obj3 != null && (bool)obj3),
						CanTake = (obj2 != null && (bool)obj2),
						Penalty = num2
					});
				}
			}
			rodSlot.Reel.SetDragMode();
		}
	}

	internal void OnRodUnequiped(InventoryItem rod)
	{
		if (GameFactory.RodSlots[rod.Slot].IsRodAssembling)
		{
			this._rodEventsQueue[rod.Slot].Enqueue(delegate
			{
				this.OnRodUnequiped(rod);
			});
			return;
		}
		GameFactory.RodSlot rodSlot = GameFactory.RodSlots[rod.Slot];
		if (rodSlot != null && rodSlot.Rod != null && rodSlot.Rod.RodAssembly != null)
		{
			(rodSlot.Rod.RodAssembly as AssembledRod).RefreshRod(rod.InstanceId);
		}
	}

	internal void OnItemLost(InventoryItem item)
	{
		if (item is Bait && item.Storage == StoragePlaces.ParentItem)
		{
			if (!(PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemId == item.ItemId && x.Storage == StoragePlaces.Equipment && x.Count > 0) is Bait) && GameFactory.Message != null)
			{
				GameFactory.Message.ShowBaitDepleted(RodHelper.FindRodInSlot(item.ParentItem.Slot, null));
			}
		}
		else if (item is Chum && item.Storage == StoragePlaces.ParentItem)
		{
			if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null)
			{
				Chum chum = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault(delegate(InventoryItem x)
				{
					Guid? instanceId = x.InstanceId;
					bool flag = instanceId != null;
					Guid? splitFromInstanceId = (item as Chum).SplitFromInstanceId;
					return flag == (splitFromInstanceId != null) && (instanceId == null || instanceId.GetValueOrDefault() == splitFromInstanceId.GetValueOrDefault());
				}) as Chum;
				if (chum != null && !chum.IsExpired)
				{
					if (chum.Weight == null || item.Weight == null)
					{
						goto IL_1B4;
					}
					double? weight = item.Weight;
					if (!(chum.Weight.Value < weight))
					{
						goto IL_1B4;
					}
				}
				if (GameFactory.Message != null && PondControllers.Instance != null && !PondControllers.Instance.IsInMenu)
				{
					GameFactory.Message.ShowChumLost(RodHelper.FindRodInSlot(item.ParentItem.Slot, null));
				}
			}
			IL_1B4:;
		}
		else if (item.ItemSubType == ItemSubTypes.PvaFeeder && item.Storage == StoragePlaces.ParentItem && GameFactory.Message != null)
		{
			GameFactory.Message.ShowPVABagLost(RodHelper.FindRodInSlot(item.ParentItem.Slot, null));
		}
		Debug.LogWarning(string.Format("SRV: ITEM {0}({1}) LOST", item.Name, item.InstanceId));
	}

	internal void OnItemGained(IEnumerable<InventoryItem> items, bool announce)
	{
		foreach (InventoryItem inventoryItem in items)
		{
			Debug.Log(string.Format("SRV: ITEM {0}({1}) GAINED (ANNOUNCED: {2})", inventoryItem.Name, inventoryItem.InstanceId, (!announce) ? "N" : "Y"));
		}
	}

	private void OnGameActionResult(GameActionResult actionResult)
	{
		if (actionResult.ErrorCode == 32656)
		{
			return;
		}
		if (actionResult.ErrorCode != null)
		{
			return;
		}
		if (GameFactory.RodSlots[actionResult.RodSlot].IsRodAssembling)
		{
			this._rodEventsQueue[actionResult.RodSlot].Enqueue(delegate
			{
				this.OnGameActionResult(actionResult);
			});
			return;
		}
		if (actionResult.ActionCode == 6)
		{
			GameFactory.RodSlots[actionResult.RodSlot].Tackle.IsAttackFinished = true;
		}
		if (actionResult.ActionCode == 254)
		{
			this.IsGamePaused = true;
			this.OnGamePaused(this.IsGamePaused);
		}
		if (actionResult.ActionCode == 255)
		{
			this.IsGamePaused = false;
			this.OnGamePaused(this.IsGamePaused);
		}
	}

	private void OnOperationFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	internal void OnEncryptionFailedToEstablish()
	{
		Debug.LogError("Failed to establish encryption");
	}

	private void OnRefreshSecurityTokenFailed(Failure failure)
	{
	}

	private IFishController FindFishByTemplate(Fish template)
	{
		if (template == null || template.InstanceId == null)
		{
			return null;
		}
		return this.FindFishById(template.InstanceId.Value);
	}

	private IFishController FindFishById(Guid id)
	{
		if (this.fish.ContainsKey(id))
		{
			return this.fish[id];
		}
		return null;
	}

	private void OnGameHint(HintInfo hint)
	{
		this.ShowUserFriendlyHintMessage(hint);
		this.ShowHintMessage(hint);
	}

	private void ShowHintMessage(HintInfo hint)
	{
		HintCode code = hint.Code;
		if (code == 4)
		{
			GameFactory.Message.ShowFishLostIncorrectHook(RodHelper.FindRodInSlot(hint.RodSlot, null));
		}
	}

	private void OnBaitLost(bool onCatch, int rodSlotIndex)
	{
		if (GameFactory.RodSlots[rodSlotIndex].IsRodAssembling)
		{
			this._rodEventsQueue[rodSlotIndex].Enqueue(delegate
			{
				this.OnBaitLost(onCatch, rodSlotIndex);
			});
			return;
		}
		GameFactory.RodSlot rodSlot = GameFactory.RodSlots[rodSlotIndex];
		if (rodSlot.Tackle == null)
		{
			return;
		}
		Debug.Log("Bait was eaten or lost");
		if (!onCatch)
		{
			GameFactory.Message.ShowBaitLost(RodHelper.FindRodInSlot(rodSlotIndex, null));
		}
		if (rodSlot.Tackle.Hook != null)
		{
			rodSlot.Tackle.Hook.IsBaitShown = false;
		}
	}

	private void OnBaitReplenished(int rodSlotIndex)
	{
		if (GameFactory.RodSlots[rodSlotIndex].IsRodAssembling)
		{
			this._rodEventsQueue[rodSlotIndex].Enqueue(delegate
			{
				this.OnBaitReplenished(rodSlotIndex);
			});
			return;
		}
		GameFactory.RodSlot rodSlot = GameFactory.RodSlots[rodSlotIndex];
		if (rodSlot.Tackle == null)
		{
			return;
		}
		if (rodSlot.Tackle.Hook != null)
		{
			rodSlot.Tackle.Hook.IsBaitShown = true;
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		this.IsGamePaused = true;
		this.OnGamePaused(this.IsGamePaused);
	}

	private void OnRoomIsFull()
	{
		if (GameFactory.Message != null)
		{
			GameFactory.Message.ShowRoomIsFull();
		}
	}

	private void OnUnableToEnterRoom(Failure failure)
	{
		if (GameFactory.Message != null)
		{
			GameFactory.Message.ShowUnableToEnterRoom();
		}
	}

	private void ShowUserFriendlyHintMessage(HintInfo hint)
	{
		switch (hint.Code)
		{
		case 7:
			if (!string.IsNullOrEmpty(hint.Message))
			{
				if (hint.Message == "low")
				{
					GameFactory.Message.ShowFishEscapedLowTension();
				}
				else if (hint.Message == "high")
				{
					GameFactory.Message.ShowFishEscapedHighTension();
				}
			}
			break;
		}
	}

	private void OnExpGained(int experience, int rankExperience)
	{
		Debug.LogWarning(string.Concat(new object[] { "INFO: Experience gained! Regular: ", experience, " ranked: ", rankExperience }));
	}

	private void OnLevelGained(LevelInfo level)
	{
		Debug.LogWarning("CONGRATULATIONS! You've got new level: " + level.Level);
	}

	private void OnPondStayFinish(PondStayFinish finish)
	{
		this.IsGamePaused = true;
		this.OnGamePaused(this.IsGamePaused);
	}

	private void OnPondStayProlonged(ProlongInfo info)
	{
		this.IsGamePaused = false;
		this.OnGamePaused(this.IsGamePaused);
	}

	private void OnLicensePenalty(LicenseBreakingInfo info)
	{
		Debug.LogWarning("LICENSE BREACHED! You ARE fined " + info.Value.ToString("#0.0") + info.Currency);
	}

	private void OnLicensePenaltyWarning(LicenseBreakingInfo info)
	{
		Debug.LogWarning("LICENSE BREACHED! You may be fined " + info.Value.ToString("#0.0") + info.Currency);
	}

	private void OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		Debug.LogWarning(string.Concat(new object[] { "Product(s) delivered: ", product.Name, ", Count of products: ", count }));
	}

	public void DecodeFishForce(int slotId, float fishRelativeForce)
	{
		if (slotId > 0)
		{
			GameFactory.RodSlot rodSlot = GameFactory.RodSlots[slotId];
			if (rodSlot.IsRodAssembling)
			{
				this._rodEventsQueue[slotId].Enqueue(delegate
				{
					this.DecodeFishForce(slotId, fishRelativeForce);
				});
				return;
			}
			if (rodSlot.Tackle.Fish != null && rodSlot.Tackle.Fish.Behavior == FishBehavior.Hook)
			{
				rodSlot.Tackle.Fish.CurrentRelativeForce = fishRelativeForce;
			}
		}
	}

	public void EscapeAllFish(int slotId = -1)
	{
		if (slotId > 0)
		{
			GameFactory.RodSlot rodSlot = GameFactory.RodSlots[slotId];
			if (rodSlot.IsRodAssembling)
			{
				this._rodEventsQueue[slotId].Enqueue(delegate
				{
					this.EscapeAllFish(slotId);
				});
				return;
			}
		}
		foreach (IFishController fishController in this.Fish.Values)
		{
			if (fishController.State != typeof(FishHooked) && (slotId < 0 || fishController.SlotId == slotId))
			{
				fishController.Escape();
			}
		}
		if (slotId >= 0 && GameFactory.RodSlots[slotId].Rod != null)
		{
			GameFactory.RodSlots[slotId].Tackle.EscapeFish();
			GameFactory.RodSlots[slotId].Rod.ResetAppliedForce();
		}
	}

	public void FishSetOnPod(int slotId, bool onPod)
	{
		foreach (IFishController fishController in this.Fish.Values)
		{
			if (fishController.SlotId == slotId)
			{
				fishController.SetOnPod(onPod);
			}
		}
	}

	public void Add(IFishController fish2Add)
	{
		this.fish[fish2Add.InstanceGuid] = fish2Add;
	}

	public void Replace(IFishController fish2Add)
	{
		if (this.fish.ContainsKey(fish2Add.InstanceGuid))
		{
			this.fish[fish2Add.InstanceGuid] = fish2Add;
		}
		else
		{
			Debug.LogError("FishSpawner.Replace : fish " + fish2Add.InstanceGuid + " does not exist");
		}
	}

	public void Remove(IFishController fish2Remove)
	{
		IFishController fishController = null;
		if (this.fish.TryGetValue(fish2Remove.InstanceGuid, out fishController) && fishController.GetType() == fish2Remove.GetType())
		{
			GameFactory.Player.InformFishDestroyed(fish2Remove.tpmId);
			this.fish.Remove(fish2Remove.InstanceGuid);
			if (this.fishCamera != null && this.fishCamera.activeSelf)
			{
				this.fishCamera.transform.position = Vector3.zero;
				this.fishCamera.transform.rotation = Quaternion.identity;
				this.fishCamera.SetActive(false);
			}
		}
	}

	public IEnumerator SpawnAttackingFish(Fish template, GameFactory.RodSlot rodSlot, UserBehaviours userBehaviour, RodOnPodBehaviour.TransitionData td = null)
	{
		if (!(rodSlot.Rod is Rod1stBehaviour))
		{
			Debug.LogError(string.Concat(new object[] { "SpawnAttackingFish can be called for Rod1stBehaviour only! Rod = ", rodSlot.Rod, " Tackle = ", rodSlot.Tackle }));
			yield break;
		}
		if (rodSlot.Tackle == null)
		{
			throw new NullReferenceException("Tackle should not be NULL");
		}
		if (rodSlot.Tackle.FishIsLoading)
		{
			string text = string.Format("{0} SpawnAttackingFish(InstanceID={1} Name={2}): Abort loading previous fish (InstanceID={3} Name={4})", new object[]
			{
				DateTime.Now.ToString("hh:mm:ss.ff"),
				template.InstanceId,
				template.Name,
				this.lastSpawnEventFish[rodSlot.Index].InstanceId,
				this.lastSpawnEventFish[rodSlot.Index].Name
			});
			Debug.LogWarning(text);
			PhotonConnectionFactory.Instance.PinError(text, "FishSpawner.SpawnAttackingFish");
		}
		rodSlot.Tackle.FishIsLoading = true;
		if (td == null)
		{
			rodSlot.OnResetFishBiteConfirmRequest();
			this.EscapeAllFish(rodSlot.Index);
			rodSlot.Tackle.IsAttackFinished = false;
			rodSlot.Tackle.IsFinishAttackRequested = false;
		}
		else
		{
			rodSlot.Tackle.IsAttackFinished = td.tackleAttackFinished;
			rodSlot.Tackle.IsFinishAttackRequested = td.tackleAttackFinishedRequested;
		}
		Vector3 spawnVector = (rodSlot.Tackle.transform.position - rodSlot.Rod.CurrentTipPosition).normalized;
		if (template.AttackVector != null)
		{
			FishAttackStyle value = template.AttackVector.Value;
			if (value != FishAttackStyle.Fore)
			{
				if (value != FishAttackStyle.Side)
				{
					if (value != FishAttackStyle.Rare)
					{
					}
				}
				else if ((double)Random.Range(0f, 1f) < 0.5)
				{
					spawnVector = Quaternion.Euler(0f, 90f, 0f) * spawnVector;
				}
				else
				{
					spawnVector = Quaternion.Euler(0f, -90f, 0f) * spawnVector;
				}
			}
			else
			{
				spawnVector = -spawnVector;
			}
		}
		Vector3 spawnPosition = rodSlot.Sim.TackleTipMass.Position + spawnVector * 1f;
		spawnPosition = this.CorrectSpawnPosition(rodSlot.Sim.TackleTipMass.Position, spawnPosition, rodSlot.Tackle.transform.forward);
		if (GameFactory.Player.IsCurrentlyTrolling)
		{
			GameFactory.Player.OverwriteCatchedFishPos(rodSlot.Index, spawnPosition);
		}
		if (td != null)
		{
			spawnPosition = rodSlot.Sim.TackleTipMass.Position;
		}
		this.lastSpawnEventFish[rodSlot.Index] = template;
		FishWaterTile.DoingRenderWater = false;
		if (template.Asset.Length == 0)
		{
			throw new ArgumentException("Empty asset name");
		}
		ResourceRequest fishPrefabRequest = Resources.LoadAsync(template.Asset, typeof(GameObject));
		while (!fishPrefabRequest.isDone)
		{
			yield return null;
		}
		if (this.lastSpawnEventFish.ContainsKey(rodSlot.Index))
		{
			Guid? instanceId = this.lastSpawnEventFish[rodSlot.Index].InstanceId;
			bool flag = instanceId != null;
			Guid? instanceId2 = template.InstanceId;
			if (flag != (instanceId2 != null) || (instanceId != null && instanceId.GetValueOrDefault() != instanceId2.GetValueOrDefault()))
			{
				yield break;
			}
		}
		if (rodSlot.Tackle != null)
		{
			GameObject gameObject = fishPrefabRequest.asset as GameObject;
			if (gameObject == null)
			{
				throw new PrefabException(string.Concat(new object[] { "Can't instantiate fish '", template, "' from asset: ", template.Asset }));
			}
			FishWaterTile.DoingRenderWater = true;
			IFishController fishController = this.FindFishByTemplate(template);
			if (fishController != null && td != null)
			{
				td.fishInitialBehavior = fishController.Behavior;
			}
			FishBehaviour fishBehaviour = FishSpawner.GenerateFish(template, rodSlot, spawnPosition, Quaternion.identity, userBehaviour, gameObject, td);
			if (td == null)
			{
				fishBehaviour.tpmId = GameFactory.Player.InformAboutNewFish(template, spawnPosition);
			}
			rodSlot.Tackle.FishIsLoading = false;
		}
		yield break;
	}

	public void DestroyFishCam()
	{
		if (this.fishCam == null)
		{
			return;
		}
		Object.Destroy(this.fishCam.gameObject);
		this.fishCam = null;
	}

	public static FishBehaviour GenerateFish(IFish fishTemplate, GameFactory.RodSlot rodSlot, Vector3 spawnPosition, Quaternion spawnRotation, UserBehaviours userBehaviour, GameObject fishPrefab, RodOnPodBehaviour.TransitionData td = null)
	{
		if (fishPrefab == null)
		{
			throw new DbConfigException("Fish prefab with name " + fishTemplate.Asset + " not found");
		}
		GameObject gameObject = Object.Instantiate<GameObject>(fishPrefab);
		FishController component = gameObject.GetComponent<FishController>();
		gameObject.transform.position += spawnPosition - component.mouth.position;
		gameObject.transform.rotation = spawnRotation;
		return component.SetBehaviour(userBehaviour, fishTemplate, rodSlot, td);
	}

	private Vector3 CorrectSpawnPosition(Vector3 tacklePosition, Vector3 spawnPosition, Vector3 tackleForward)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 position = GameFactory.Player.transform.position;
			Vector3 vector = position - spawnPosition;
			if (vector.magnitude < this.minDistanceToPlayer)
			{
				spawnPosition -= vector.normalized * (this.minDistanceToPlayer - vector.magnitude);
			}
			if (spawnPosition.y > -this.minSpawnDepth)
			{
				spawnPosition..ctor(spawnPosition.x, -this.minSpawnDepth, spawnPosition.z);
			}
			bool flag = true;
			while (!Physics.Raycast(spawnPosition, Vector3.down, float.PositiveInfinity, GlobalConsts.FishMask))
			{
				spawnPosition..ctor(spawnPosition.x, spawnPosition.y + 0.5f, spawnPosition.z);
				if (spawnPosition.y > -this.minSpawnDepth)
				{
					Vector3 vector2 = Quaternion.Euler(0f, (float)Random.Range(-180, 180), 0f) * tackleForward;
					spawnPosition = tacklePosition + vector2 * 1f;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return spawnPosition;
			}
		}
		return tacklePosition;
	}

	public static GameObject LoadFishPrefab(string asset)
	{
		return (GameObject)Resources.Load(asset, typeof(GameObject));
	}

	public Vector3 GenerateRandomPoint()
	{
		Vector3 vector;
		vector..ctor(0f, 0f, 0f);
		vector.x = Random.Range(-0.5f, 0.5f);
		vector.z = Random.Range(-0.5f, 0.5f);
		Vector3 vector2 = this.fishingArea.TransformPoint(vector);
		vector2.y = -Random.value * Random.Range(this.minSpawnDepth, this.maxSpawnDepth);
		return vector2;
	}

	public void ShowBobberIndicator(Transform target)
	{
		if (GameFactory.BobberIndicator != null && SettingsManager.FishingIndicator)
		{
			GameFactory.BobberIndicator.Show();
		}
		if (this.bobberCamera == null)
		{
			return;
		}
		this.bobberCamera.SetActive(true);
		this.bobberCamera.GetComponent<SimpleFollow>().target = target;
	}

	public void HideBobberIndicator()
	{
		if (GameFactory.BobberIndicator != null)
		{
			GameFactory.BobberIndicator.Hide();
		}
		if (this.bobberCamera == null)
		{
			return;
		}
		this.bobberCamera.GetComponent<SimpleFollow>().target = null;
		this.bobberCamera.SetActive(false);
	}

	public float CheckDepth(float x, float z)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(new Vector3(x, 0f, z), Vector3.down, ref raycastHit, float.PositiveInfinity, GlobalConsts.FishMask))
		{
			return raycastHit.distance;
		}
		return 0f;
	}

	public Transform fishingArea;

	public GameObject fishCamera;

	public GameObject bobberCamera;

	public Transform hitchBoxesRoot;

	public float minSpawnDepth = 0.5f;

	public float maxSpawnDepth = 10f;

	public float minDistanceToPlayer = 3f;

	public FishCamController fishCam;

	private const float AttackSpawnDistance = 1f;

	private const float RaycastDepthStep = 0.5f;

	private Dictionary<Guid, IFishController> fish = new Dictionary<Guid, IFishController>();

	private Dictionary<int, Fish> lastSpawnEventFish = new Dictionary<int, Fish>();

	private Dictionary<int, List<GameEvent>> _waitingEvents = new Dictionary<int, List<GameEvent>>();

	private Dictionary<int, Queue<Action>> _rodEventsQueue = new Dictionary<int, Queue<Action>>();

	private StringBuilder sb = new StringBuilder();
}
