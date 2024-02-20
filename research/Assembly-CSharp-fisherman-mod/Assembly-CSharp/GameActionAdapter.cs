using System;
using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using ExitGames.Client.Photon;
using ObjectModel;
using UnityEngine;

public class GameActionAdapter
{
	public GameActionAdapter(Game owner)
	{
		this.Game = owner;
	}

	public static GameActionAdapter Instance
	{
		get
		{
			return PhotonConnectionFactory.Instance.Game.Adapter;
		}
	}

	public Game Game { get; private set; }

	private Vector3 PlayerPosition
	{
		get
		{
			return GameFactory.Player.Position;
		}
	}

	private Vector3 PlayerRotation
	{
		get
		{
			return GameFactory.Player.Rotation;
		}
	}

	private Vector3 TacklePosition
	{
		get
		{
			return this.Game.Tackle.Position;
		}
	}

	private Vector3? AttackingFishPosition
	{
		get
		{
			if (this.Game.Tackle.AttackingFish != null)
			{
				return new Vector3?(this.Game.Tackle.AttackingFish.MouthPosition);
			}
			return null;
		}
	}

	private ObscuredBool IsFishPassive
	{
		get
		{
			return this.Game.CurrentFish.IsPassive;
		}
	}

	private ObscuredBool IsFishBig
	{
		get
		{
			return this.Game.CurrentFish != null && this.Game.CurrentFish.IsBig;
		}
	}

	private ObscuredBool HasLineSlack
	{
		get
		{
			return this.Game.Line.IsSlacked;
		}
	}

	private ObscuredBool IsForced
	{
		get
		{
			return this.Game.Rod.IsFishingForced && this.IsFishBig;
		}
	}

	private TackleBehaviour Tackle
	{
		get
		{
			return this.Game.Tackle;
		}
	}

	private ObscuredBool IsTackleLying
	{
		get
		{
			return this.Game.Tackle.IsLying;
		}
	}

	private ObscuredBool IsFeederLying
	{
		get
		{
			IFeederBehaviour feederBehaviour = this.Game.Tackle as IFeederBehaviour;
			return feederBehaviour != null && feederBehaviour.IsLying;
		}
	}

	private ObscuredBool IsTackleMoving
	{
		get
		{
			return this.Game.Tackle.IsMoving;
		}
	}

	private ObscuredFloat LineLength
	{
		get
		{
			return this.Game.Line.SecuredLineLength + this.Game.Rod.LineOnRodLength;
		}
	}

	private ObscuredFloat LineForce
	{
		get
		{
			return this.Game.Line.AppliedForce / 9.81f;
		}
	}

	private ObscuredFloat RodForce
	{
		get
		{
			return this.Game.Rod.AppliedForce / 9.81f;
		}
	}

	private ObscuredFloat ReelForce
	{
		get
		{
			return this.Game.Reel.AppliedForce / 9.81f;
		}
	}

	private ObscuredFloat FrictionForce
	{
		get
		{
			return this.Game.Reel.LineAdjustedFrictionForce / 9.81f;
		}
	}

	private DragStyle TackleDragStyle
	{
		get
		{
			return this.Game.Tackle.DragStyle;
		}
	}

	private ObscuredBool IsAnchorDown
	{
		get
		{
			return GameFactory.Player.IsAnchored;
		}
	}

	private ObscuredBool IsRowing
	{
		get
		{
			return GameFactory.Player.IsRowing;
		}
	}

	private ObscuredBool IsUsingBoatEngine
	{
		get
		{
			return GameFactory.Player.IsUsingBoatEngine;
		}
	}

	private ObscuredFloat BoatStamina
	{
		get
		{
			return GameFactory.Player.Stamina;
		}
	}

	private ObscuredBool IsTrolling
	{
		get
		{
			return GameFactory.Player.CurrentBoat != null && GameFactory.Player.CurrentBoat.IsTrolling;
		}
	}

	private bool CanInteractWithServer
	{
		get
		{
			return PhotonConnectionFactory.Instance != null && this.Game != null;
		}
	}

	private bool CanTrow
	{
		get
		{
			return this.CanInteractWithServer && this.Game.Rod != null && !this.Game.Rod.RodAssembly.IsRodDisassembled && !GameFactory.FishSpawner.IsGamePaused;
		}
	}

	private bool CanSendGameActions
	{
		get
		{
			return this.CanTrow && !this.isGameActionFinished;
		}
	}

	private ObscuredBool IsReeling
	{
		get
		{
			return this.Game.Reel.IsReeling;
		}
	}

	private ObscuredInt ReelSpeed
	{
		get
		{
			return this.Game.Reel.CurrentReelSpeedSection;
		}
	}

	private ObscuredBool CanApproach
	{
		get
		{
			return this.Game.CurrentFish.IsPassive || this.Game.CurrentFish.IsGoingTo;
		}
	}

	private ObscuredBool IsCurrentlyPullingOrStriking
	{
		get
		{
			return GameFactory.Player.IsCurrentlyPullingOrStriking;
		}
	}

	private ObscuredInt UsingBoatOfType
	{
		get
		{
			return GameFactory.Player.BoatTypeInUse;
		}
	}

	private Vector3? ThrowTarget
	{
		get
		{
			return this.Game.Tackle.ThrowData.Target;
		}
	}

	public void Throw(ObscuredFloat tackleForce)
	{
		if (!this.CanInteractWithServer || !this.CanTrow)
		{
			return;
		}
		this.ResetGameState();
		GameFactory.Player.ThrowTargetPoint = null;
		Game game = this.Game;
		ObscuredVector3 obscuredVector = this.PlayerPosition;
		ObscuredInt usingBoatOfType = this.UsingBoatOfType;
		Vector3? throwTarget = this.ThrowTarget;
		game.Throw(obscuredVector, tackleForce, usingBoatOfType, (throwTarget == null) ? null : new ObscuredVector3?(throwTarget.GetValueOrDefault()));
		this.Game.Tackle.ThrowData.Target = null;
		this.isGameActionFinished = false;
	}

	public void Water(ObscuredFloat castLength)
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		this.Game.Water(this.TacklePosition, castLength);
	}

	public void Move(ObscuredBool isBobberIdle)
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		if (this.Game.Reel.IsReeling && this.Game.Reel.CurrentReelSpeedSection == 4)
		{
			this.Game.Move(this.PlayerPosition, this.TacklePosition, this.LineLength);
		}
		else
		{
			Game game = this.Game;
			ObscuredVector3 obscuredVector = this.PlayerPosition;
			ObscuredVector3 obscuredVector2 = this.TacklePosition;
			Vector3? attackingFishPosition = this.AttackingFishPosition;
			game.Move(obscuredVector, obscuredVector2, (attackingFishPosition == null) ? null : new ObscuredVector3?(attackingFishPosition.GetValueOrDefault()), this.TackleDragStyle, isBobberIdle, this.IsTackleLying, this.IsFeederLying, this.IsTackleMoving, this.LineForce, this.RodForce, this.ReelForce, this.LineLength, this.ReelSpeed, this.IsReeling, this.HasLineSlack, this.IsAnchorDown, this.IsRowing, this.BoatStamina, false);
		}
	}

	public void FinishAttack(ObscuredBool striking, ObscuredBool wrongStriking, ObscuredBool pulled, ObscuredBool sendForce, ObscuredFloat distanceToTackle)
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		if (this.Tackle.IsFinishAttackRequested)
		{
			return;
		}
		this.Tackle.IsFinishAttackRequested = true;
		if (sendForce)
		{
			Game game = this.Game;
			ObscuredVector3 obscuredVector = this.PlayerPosition;
			ObscuredVector3 obscuredVector2 = this.TacklePosition;
			Vector3? attackingFishPosition = this.AttackingFishPosition;
			game.Move(obscuredVector, obscuredVector2, (attackingFishPosition == null) ? null : new ObscuredVector3?(attackingFishPosition.GetValueOrDefault()), DragStyle.Undefined, true, false, false, false, this.LineForce, this.RodForce, this.ReelForce, this.LineLength, this.ReelSpeed, false, this.HasLineSlack, this.IsAnchorDown, this.IsRowing, this.BoatStamina, false);
		}
		else
		{
			this.ResetPeriod();
		}
		this.Game.FinishAttack(this.PlayerPosition, this.TacklePosition, striking, wrongStriking, pulled, distanceToTackle);
	}

	public void FinishAttackOnPod(ObscuredBool striking, ObscuredBool wrongStriking, ObscuredBool pulled, ObscuredBool sendForce, ObscuredFloat distanceToTackle)
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		if (this.Tackle.IsFinishAttackRequested)
		{
			return;
		}
		if (!this.Game.Rod.RodSlot.FishBiteConfirmRequestSent)
		{
			return;
		}
		this.Tackle.IsFinishAttackRequested = true;
		if (sendForce)
		{
			Game game = this.Game;
			ObscuredVector3 obscuredVector = this.PlayerPosition;
			ObscuredVector3 obscuredVector2 = this.TacklePosition;
			Vector3? attackingFishPosition = this.AttackingFishPosition;
			game.Move(obscuredVector, obscuredVector2, (attackingFishPosition == null) ? null : new ObscuredVector3?(attackingFishPosition.GetValueOrDefault()), DragStyle.Undefined, true, false, false, false, this.LineForce, this.RodForce, this.ReelForce, this.LineLength, this.ReelSpeed, this.IsReeling, this.HasLineSlack, this.IsAnchorDown, this.IsRowing, this.BoatStamina, false);
		}
		else
		{
			this.ResetPeriod();
		}
		this.Game.FinishAttack(this.PlayerPosition, this.TacklePosition, striking, wrongStriking, pulled, distanceToTackle);
	}

	public void FightFish()
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		this.Game.FightFish(this.PlayerPosition, this.TacklePosition, this.LineForce, this.RodForce, this.ReelForce, this.FrictionForce, this.LineLength, this.IsFishPassive, this.HasLineSlack, this.IsForced, this.ReelSpeed, this.IsReeling, this.CanApproach, this.IsCurrentlyPullingOrStriking, this.IsAnchorDown, this.IsRowing, this.BoatStamina, false);
	}

	public void FightFishOnPod()
	{
		this.Game.FightFish(this.PlayerPosition, this.TacklePosition, this.LineForce, this.RodForce, this.ReelForce, this.FrictionForce, this.LineLength, this.IsFishPassive, this.HasLineSlack, this.IsForced, this.ReelSpeed, this.IsReeling, this.CanApproach, false, false, false, 0f, false);
	}

	public void CatchFish()
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		this.Game.CatchFish(this.PlayerPosition, this.TacklePosition);
	}

	public void TakeFish()
	{
		if (!this.CanInteractWithServer)
		{
			return;
		}
		TutorialController.FishCatchedCount++;
		if (this.Game.CurrentFish == null || this.Game.CurrentFish.CaughtFish == null)
		{
			throw new InvalidOperationException(string.Format("No fish to take when taking fish, Game.CurrentFish == null: {0}, Game.CurrentFish.CaughtFish==null: {1}", this.Game.CurrentFish == null, this.Game.CurrentFish != null && this.Game.CurrentFish.CaughtFish == null));
		}
		this.Game.TakeFish(this.Game.CurrentFish.CaughtFish);
	}

	public void ReleaseFish()
	{
		if (!this.CanInteractWithServer)
		{
			return;
		}
		TutorialController.FishCatchedCount++;
		this.Game.ReleaseFish();
	}

	public void CatchItem()
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		PhotonConnectionFactory.Instance.Game.CatchItem(this.PlayerPosition, this.TacklePosition);
	}

	public void TakeItem()
	{
		if (this.CanInteractWithServer)
		{
			this.Game.TakeItem();
		}
	}

	public void ReleaseItem()
	{
		if (this.CanInteractWithServer)
		{
			this.Game.ReleaseItem();
		}
	}

	public void Board(ItemSubTypes boatType)
	{
		if (this.CanInteractWithServer)
		{
			GameFactory.Player.SendPlayerPositionAndRotation(true);
			this.Game.Board(this.PlayerPosition, this.PlayerRotation, (short)boatType);
		}
	}

	public void RestoreBoatPosition()
	{
		if (this.CanInteractWithServer)
		{
			this.Game.RestoreBoatPosition();
		}
	}

	public void UnBoard(ItemSubTypes boatType)
	{
		if (this.CanInteractWithServer)
		{
			this.Game.UnBoard(this.PlayerPosition, (short)boatType);
		}
	}

	public void TravelByBoat()
	{
		if (this.CanInteractWithServer)
		{
			this.Game.TravelByBoat(this.PlayerPosition, this.PlayerRotation, this.IsAnchorDown, this.IsRowing, this.IsUsingBoatEngine, this.BoatStamina, this.IsTrolling);
		}
	}

	public void TurnOver()
	{
		if (this.CanInteractWithServer)
		{
			this.Game.TurnOver();
		}
	}

	public void Walk()
	{
		if (this.CanInteractWithServer)
		{
			this.Game.Walk(this.PlayerPosition, this.PlayerRotation);
		}
	}

	public void FinishGameAction()
	{
		if (!this.CanInteractWithServer)
		{
			return;
		}
		if (this.isGameActionFinished)
		{
			return;
		}
		GameFactory.FishSpawner.EscapeAllFish(this.Game.RodSlot);
		if (this.Tackle.Fish == null)
		{
			this.Game.FinishMove(this.PlayerPosition, this.TacklePosition);
		}
		else
		{
			if (this.Tackle.Fish.Behavior == FishBehavior.Undefind)
			{
				this.Game.FinishMove(this.PlayerPosition, this.TacklePosition);
				this.SetGameActionFinished();
				this.Game.Reset();
				this.Tackle.EscapeFish();
				return;
			}
			if (this.Tackle.Fish.State == typeof(FishBite) || this.Tackle.Fish.State == typeof(FishSwimAway) || this.Tackle.Fish.State == typeof(FishPredatorSwim))
			{
				this.FinishAttack(false, true, false, false, 0f);
				this.Game.FinishMove(this.PlayerPosition, this.TacklePosition);
				this.Tackle.EscapeFish();
			}
		}
		this.Game.Reset();
		this.Game.Rod.RodSlot.OnResetFishBiteConfirmRequest();
		this.SetGameActionFinished();
	}

	public void SetGameActionFinished()
	{
		this.isGameActionFinished = true;
	}

	public void UnHitch()
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		this.Game.UnHitch(this.LineForce, this.LineLength, this.RodForce, this.ReelForce, this.ReelSpeed, this.IsReeling, this.HasLineSlack, this.IsAnchorDown, this.IsRowing, this.BoatStamina);
	}

	public void BreakLine()
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		this.Game.BreakLine(this.LineLength);
		this.SetGameActionFinished();
	}

	public void ConfirmFishBite(Guid fishId)
	{
		if (!this.CanSendGameActions)
		{
			return;
		}
		this.Game.ConfirmFishBite(fishId);
	}

	public void ResetPeriod()
	{
		this.Game.ResetPeriod();
	}

	private void ResetGameState()
	{
		this.Game.Rod.RodSlot.OnResetFishBiteConfirmRequest();
		this.ResetPeriod();
		this.Game.Tackle.IsHitched = false;
		this.Game.Tackle.IsAttackFinished = false;
		this.Game.Tackle.IsFinishAttackRequested = false;
		this.Game.Tackle.IsPitchTooShort = false;
	}

	public void UpdateFeedings(IEnumerable<Feeding> feedings)
	{
		if (!this.CanInteractWithServer)
		{
			return;
		}
		this.Game.UpdateFeedings(feedings);
	}

	public static WearInfo DecodeAndApplyWearInfo(GameActionResult gameActionResult, Profile profile)
	{
		Hashtable actionData = gameActionResult.ActionData;
		if (profile == null || actionData == null)
		{
			return null;
		}
		WearInfo wearInfo = new WearInfo();
		if (actionData.ContainsKey("rU") && actionData["rU"] is string)
		{
			Guid rodId = new Guid((string)actionData["rU"]);
			wearInfo.RodUnequiped = profile.Inventory.OfType<Rod>().FirstOrDefault((Rod r) => r.InstanceId == rodId);
		}
		if (actionData.ContainsKey("bI") && actionData["bI"] is byte)
		{
			wearInfo.BrokenItem = new BrokenTackleType?((BrokenTackleType)actionData["bI"]);
		}
		if (actionData.ContainsKey("iR") && actionData["iR"] is string)
		{
			Guid guid = new Guid((string)actionData["iR"]);
			for (int i = 0; i < profile.Inventory.Count; i++)
			{
				InventoryItem inventoryItem = profile.Inventory[i];
				if (inventoryItem.InstanceId == guid)
				{
					wearInfo.Rod = inventoryItem as Rod;
					wearInfo.RodWear = (int)actionData["wR"];
					inventoryItem.Durability = (int)actionData["dR"];
					FeederRod feederRod = inventoryItem as FeederRod;
					if (feederRod != null && actionData.ContainsKey("iQ") && actionData["iQ"] is string)
					{
						int quiverId = int.Parse((string)actionData["iQ"]);
						QuiverTip quiverTip = feederRod.QuiverTips.First((QuiverTip q) => q.ItemId == quiverId);
						int num = (int)actionData["dQ"];
						quiverTip.IsBroken = num == 0;
						wearInfo.Quiver = quiverTip;
						wearInfo.QuiverWear = wearInfo.RodWear;
					}
					break;
				}
			}
		}
		if (actionData.ContainsKey("ir"))
		{
			Guid guid2 = new Guid((string)actionData["ir"]);
			for (int j = 0; j < profile.Inventory.Count; j++)
			{
				InventoryItem inventoryItem2 = profile.Inventory[j];
				if (inventoryItem2.InstanceId == guid2)
				{
					wearInfo.Reel = inventoryItem2 as Reel;
					wearInfo.ReelWear = (int)actionData["wr"];
					inventoryItem2.Durability = (int)actionData["dr"];
					break;
				}
			}
		}
		if (actionData.ContainsKey("iL"))
		{
			Guid guid3 = new Guid((string)actionData["iL"]);
			for (int k = 0; k < profile.Inventory.Count; k++)
			{
				InventoryItem inventoryItem3 = profile.Inventory[k];
				if (inventoryItem3.InstanceId == guid3)
				{
					wearInfo.Line = inventoryItem3 as Line;
					wearInfo.LineWear = (int)actionData["wL"];
					inventoryItem3.Durability = (int)actionData["dL"];
					break;
				}
			}
		}
		if (actionData.ContainsKey("iB"))
		{
			Guid guid4 = new Guid((string)actionData["iB"]);
			for (int l = 0; l < profile.Inventory.Count; l++)
			{
				InventoryItem inventoryItem4 = profile.Inventory[l];
				if (inventoryItem4.InstanceId == guid4)
				{
					wearInfo.Bobber = inventoryItem4 as Bobber;
					wearInfo.BobberWear = (int)actionData["wB"];
					inventoryItem4.Durability = (int)actionData["dB"];
					break;
				}
			}
		}
		if (actionData.ContainsKey("iSn"))
		{
			Guid guid5 = new Guid((string)actionData["iSn"]);
			for (int m = 0; m < profile.Inventory.Count; m++)
			{
				InventoryItem inventoryItem5 = profile.Inventory[m];
				if (inventoryItem5.InstanceId == guid5)
				{
					wearInfo.Sinker = inventoryItem5 as Sinker;
					wearInfo.SinkerWear = (int)actionData["wSn"];
					inventoryItem5.Durability = (int)actionData["dSn"];
					break;
				}
			}
		}
		if (actionData.ContainsKey("iLd"))
		{
			Guid guid6 = new Guid((string)actionData["iLd"]);
			for (int n = 0; n < profile.Inventory.Count; n++)
			{
				InventoryItem inventoryItem6 = profile.Inventory[n];
				if (inventoryItem6.InstanceId == guid6)
				{
					wearInfo.Leader = inventoryItem6 as Leader;
					wearInfo.LeaderWear = (int)actionData["wLd"];
					inventoryItem6.Durability = (int)actionData["dLd"];
					break;
				}
			}
		}
		if (actionData.ContainsKey("iH"))
		{
			Guid guid7 = new Guid((string)actionData["iH"]);
			for (int num2 = 0; num2 < profile.Inventory.Count; num2++)
			{
				InventoryItem inventoryItem7 = profile.Inventory[num2];
				if (inventoryItem7.InstanceId == guid7)
				{
					wearInfo.Hook = inventoryItem7 as Hook;
					wearInfo.HookWear = (int)actionData["wH"];
					inventoryItem7.Durability = (int)actionData["dH"];
					break;
				}
			}
		}
		return wearInfo;
	}

	public static void DecodeFishForce(GameActionResult gameActionResult)
	{
		Hashtable actionData = gameActionResult.ActionData;
		if (actionData != null && actionData.ContainsKey("fRf"))
		{
			GameFactory.FishSpawner.DecodeFishForce(gameActionResult.RodSlot, (float)actionData["fRf"]);
		}
	}

	private bool isGameActionFinished = true;

	private const string RodId = "iR";

	private const string RodWear = "wR";

	private const string RodDurability = "dR";

	private const string QuiverId = "iQ";

	private const string QuiverDurability = "dQ";

	private const string ReelId = "ir";

	private const string ReelWear = "wr";

	private const string ReelDurability = "dr";

	private const string LineId = "iL";

	private const string LineWear = "wL";

	private const string LineDurability = "dL";

	private const string BobberId = "iB";

	private const string BobberWear = "wB";

	private const string BobberDurability = "dB";

	private const string SinkerId = "iSn";

	private const string SinkerWear = "wSn";

	private const string SinkerDurability = "dSn";

	private const string LeaderId = "iLd";

	private const string LeaderWear = "wLd";

	private const string LeaderDurability = "dLd";

	private const string HookId = "iH";

	private const string HookWear = "wH";

	private const string HookDurability = "dH";

	private const string RodUnequiped = "rU";

	private const string BrokenItem = "bI";

	private const string FishRelativeForce = "fRf";
}
