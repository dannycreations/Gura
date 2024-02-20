using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using ExitGames.Client.Photon;
using ObjectModel;
using UnityEngine;

public class Game
{
	public Game(int slotIndex)
	{
		this.RodSlot = slotIndex;
		GameFactory.GameIsPaused = false;
		this.Adapter = new GameActionAdapter(this);
	}

	public int RodSlot { get; private set; }

	public GameActionAdapter Adapter { get; private set; }

	public TackleBehaviour Tackle
	{
		get
		{
			return GameFactory.RodSlots[this.RodSlot].Tackle;
		}
	}

	public RodBehaviour Rod
	{
		get
		{
			return GameFactory.RodSlots[this.RodSlot].Rod;
		}
	}

	public LineBehaviour Line
	{
		get
		{
			return GameFactory.RodSlots[this.RodSlot].Line;
		}
	}

	public ReelBehaviour Reel
	{
		get
		{
			return GameFactory.RodSlots[this.RodSlot].Reel;
		}
	}

	public IFishController CurrentFish
	{
		get
		{
			return GameFactory.RodSlots[this.RodSlot].Tackle.Fish;
		}
	}

	public void Reset()
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(253, hashtable);
		this.DebugFishing("Reset sent");
	}

	private void DebugFishing(string message)
	{
		string text = string.Format("fishing[{0}]: {1}", this.RodSlot, message);
		if (PhotonConnectionFactory.Instance.IsTest)
		{
			PhotonConnectionFactory.Instance.DebugFishing(text);
		}
	}

	public void Throw(ObscuredVector3 playerPosition, ObscuredFloat maxTerminalTackleForce, ObscuredInt usingBoatOfType, ObscuredVector3? throwTarget)
	{
		this.ResetPeriod();
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["hTf"] = ObscuredFloat.Decrypt(maxTerminalTackleForce.GetEncrypted());
		hashtable["b"] = ObscuredInt.Decrypt(usingBoatOfType.GetEncrypted());
		if (throwTarget != null)
		{
			hashtable["tT"] = throwTarget.Value.ToPoint3();
		}
		hashtable["D"] = ((AssembledRod)this.Rod.RodAssembly).Rod.InstanceId.ToString();
		PhotonConnectionFactory.Instance.SendGameAction(1, hashtable);
		this.DebugFishing("Throw sent");
	}

	public void Water(ObscuredVector3 terminalTacklePosition, ObscuredFloat castLength)
	{
		this.ResetPeriod();
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		hashtable["cL"] = ObscuredFloat.Decrypt(castLength.GetEncrypted());
		PhotonConnectionFactory.Instance.SendGameAction(2, hashtable);
		this.DebugFishing("Water sent");
	}

	public void Move(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition, ObscuredFloat lineLength)
	{
		if (!this.CheckUpdateTimeout())
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["rS"] = 4;
		hashtable["iR"] = true;
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		hashtable["lL"] = ObscuredFloat.Decrypt(lineLength.GetEncrypted());
		PhotonConnectionFactory.Instance.SendGameAction(3, hashtable);
	}

	public void Move(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition, ObscuredVector3? fishPosition, DragStyle style, ObscuredBool isBobberIdle, ObscuredBool isLyingOnBottom, ObscuredBool isFeederLyingOnBottom, ObscuredBool isMoving, ObscuredFloat terminalTackleForce, ObscuredFloat rodForce, ObscuredFloat reelForce, ObscuredFloat lineLength, ObscuredInt speed, ObscuredBool isReeling, ObscuredBool hasLineSlack, ObscuredBool isAnchorDown, ObscuredBool isRowing, ObscuredFloat boatStamina, bool forceSend = false)
	{
		this.UpdatePeriodData(isBobberIdle, isLyingOnBottom, isFeederLyingOnBottom, isMoving);
		this.UpdateForces(terminalTackleForce, rodForce, reelForce, 0f, true, hasLineSlack, false, speed, isReeling, false, false);
		if (!forceSend && !this.CheckUpdateTimeout())
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["rS"] = this.maxSpeed;
		hashtable["iR"] = this.wasReeling;
		hashtable["lL"] = ObscuredFloat.Decrypt(lineLength.GetEncrypted());
		if (this.hadLineSlack)
		{
			hashtable["l"] = this.hadLineSlack;
		}
		if (this.maxSpeed < 4 || !this.wasReeling)
		{
			Point3 point = terminalTacklePosition.ToPoint3();
			if (fishPosition != null)
			{
				Point3 point2 = fishPosition.Value.ToPoint3();
				hashtable["aFd"] = point.Distance(point2);
			}
			hashtable["pP"] = playerPosition.ToPoint3();
			hashtable["tP"] = point;
			if (this.wasBobberIdleForPeriod)
			{
				hashtable["iBi"] = true;
			}
			if (this.wasLyingOnBottom)
			{
				hashtable["tPs"] = true;
			}
			if (this.wasFeederLyingOnBottom)
			{
				hashtable["fPs"] = true;
			}
			if (this.wasMoving)
			{
				hashtable["tMs"] = true;
			}
			hashtable["d"] = (byte)style;
		}
		PhotonConnectionFactory.Instance.SendGameAction(3, hashtable);
		this.ResetPeriod();
		this.ResetBoatTravelingFlags();
	}

	public void FinishAttack(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition, ObscuredBool isStriking, ObscuredBool hasWrongStriking, ObscuredBool pulled, ObscuredFloat distanceToTackle)
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		if (isStriking)
		{
			hashtable["iS"] = true;
		}
		if (hasWrongStriking)
		{
			hashtable["wS"] = true;
		}
		if (pulled)
		{
			hashtable["iP"] = true;
		}
		if (distanceToTackle != 0f)
		{
			hashtable["d2t"] = ObscuredFloat.Decrypt(distanceToTackle.GetEncrypted());
		}
		hashtable["lTf"] = this.minTerminalTackleForceForPeriod;
		hashtable["hTf"] = this.maxTerminalTackleForceForPeriod;
		hashtable["hRdF"] = this.maxRodForceForPeriod;
		hashtable["hRlF"] = this.maxReelForceForPeriod;
		PhotonConnectionFactory.Instance.SendGameAction(6, hashtable);
		this.ResetPeriod();
		this.DebugFishing("FinishAttack sent");
	}

	public void ConfirmFishBite(Guid fishId)
	{
		if (!this.Rod.RodSlot.FishBiteConfirmRequestSent)
		{
			Hashtable hashtable = this.CreateActionDataHashtable();
			hashtable["f"] = fishId.ToString();
			PhotonConnectionFactory.Instance.SendGameAction(14, hashtable);
			this.DebugFishing(string.Format("Confirm fish bite sent; FishId = ({0})", fishId));
			this.Rod.RodSlot.OnSendFishBiteConfirmRequest();
		}
		else
		{
			this.DebugFishing(string.Format("Confirm fish bite is already sent; FishId = ({0})", fishId));
		}
	}

	public void FinishMove(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition)
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		PhotonConnectionFactory.Instance.SendGameAction(12, hashtable);
		this.DebugFishing("FinishMove sent");
	}

	public void UnHitch(ObscuredFloat terminalTackleForce, ObscuredFloat lineLength, ObscuredFloat rodForce, ObscuredFloat reelForce, ObscuredInt speed, ObscuredBool isReeling, ObscuredBool hasLineSlack, ObscuredBool isAnchorDown, ObscuredBool isRowing, ObscuredFloat boatStamina)
	{
		this.UpdateForces(terminalTackleForce, rodForce, reelForce, 0f, false, hasLineSlack, false, speed, isReeling, false, false);
		if (!this.CheckUpdateTimeout())
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["lTf"] = this.minTerminalTackleForceForPeriod;
		hashtable["hTf"] = this.maxTerminalTackleForceForPeriod;
		hashtable["hRdF"] = this.maxRodForceForPeriod;
		hashtable["hRlF"] = this.maxReelForceForPeriod;
		hashtable["lL"] = ObscuredFloat.Decrypt(lineLength.GetEncrypted());
		hashtable["rS"] = this.maxSpeed;
		hashtable["iR"] = this.wasReeling;
		if (this.hadLineSlack)
		{
			hashtable["l"] = this.hadLineSlack;
		}
		PhotonConnectionFactory.Instance.SendGameAction(5, hashtable);
		this.ResetPeriod();
		this.ResetBoatTravelingFlags();
	}

	public void BreakLine(float lineLength)
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(11, hashtable);
	}

	public void FightFish(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition, ObscuredFloat terminalTackleForce, ObscuredFloat rodForce, ObscuredFloat reelForce, ObscuredFloat frictionForce, ObscuredFloat lineLength, ObscuredBool isFishPassive, ObscuredBool hasLineSlack, ObscuredBool isForced, ObscuredInt speed, ObscuredBool isReeling, ObscuredBool canApproach, ObscuredBool isPoolingOrStriking, ObscuredBool isAnchorDown, ObscuredBool isRowing, ObscuredFloat boatStamina, bool forceSend)
	{
		this.UpdateForces(terminalTackleForce, rodForce, reelForce, frictionForce, isFishPassive, hasLineSlack, isForced, speed, isReeling, canApproach, isPoolingOrStriking);
		if (!forceSend && !this.CheckUpdateTimeout())
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		hashtable["lTf"] = this.minTerminalTackleForceForPeriod;
		hashtable["hTf"] = this.maxTerminalTackleForceForPeriod;
		hashtable["hRdF"] = this.maxRodForceForPeriod;
		hashtable["hRlF"] = this.maxReelForceForPeriod;
		if (this.wasFishPassive)
		{
			hashtable["p"] = this.wasFishPassive;
		}
		if (this.hadLineSlack)
		{
			hashtable["l"] = this.hadLineSlack;
		}
		if (this.wasForced)
		{
			hashtable["iF"] = this.wasForced;
		}
		hashtable["hRlF"] = this.maxReelForceForPeriod;
		hashtable["fF"] = this.maxFrictionForceForPeriod;
		hashtable["lL"] = ObscuredFloat.Decrypt(lineLength.GetEncrypted());
		hashtable["rS"] = this.maxSpeed;
		hashtable["iR"] = this.wasReeling;
		hashtable["cA"] = this.wasAbleToApproach;
		hashtable["iPS"] = this.wasPoolingOrStriking;
		PhotonConnectionFactory.Instance.SendGameAction(7, hashtable);
		this.ResetPeriod();
		this.ResetBoatTravelingFlags();
	}

	public void CatchFish(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition)
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		PhotonConnectionFactory.Instance.SendGameAction(13, hashtable);
		this.DebugFishing("CatchFish sent");
	}

	public void TakeFish(Fish fish)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			Debug.LogError("Profile is not available!");
			return;
		}
		if (profile.FishCage == null || profile.FishCage.Cage == null)
		{
			Debug.LogError("No fish cage!");
			return;
		}
		if (profile.Tournament != null && profile.Tournament.MaxFishInCage != null)
		{
			if (profile.FishCage.Cage.ItemSubType == ItemSubTypes.Stringer)
			{
				Debug.LogError("Catch&Release tournament: a try to take fish to Stringer");
				return;
			}
			int? maxFishInCage = profile.Tournament.MaxFishInCage;
			if (profile.FishCage.Count >= maxFishInCage)
			{
				Debug.LogErrorFormat("Catch&Release tournament: already having {0} fish in cage and trying to take more", new object[] { profile.FishCage.Count });
				return;
			}
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(9, hashtable);
		this.DebugFishing("TakeFish sent");
	}

	public void ReleaseFish()
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(10, hashtable);
		this.DebugFishing("ReleaseFish sent");
	}

	public void CatchItem(ObscuredVector3 playerPosition, ObscuredVector3 terminalTacklePosition)
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["tP"] = terminalTacklePosition.ToPoint3();
		PhotonConnectionFactory.Instance.SendGameAction(20, hashtable);
		this.DebugFishing("CatchItem sent");
	}

	public void TakeItem()
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(15, hashtable);
		this.DebugFishing("TakeItem sent");
	}

	public void ReleaseItem()
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(16, hashtable);
		this.DebugFishing("ReleaseItem sent");
	}

	public void Board(ObscuredVector3 playerPosition, ObscuredVector3 playerRotation, short boatType)
	{
		this._lastBoatUpdate = Time.time;
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["pR"] = playerRotation.ToPoint3();
		hashtable["bT"] = boatType;
		PhotonConnectionFactory.Instance.SendGameAction(17, hashtable);
	}

	public void UnBoard(ObscuredVector3 playerPosition, short boatType)
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["bT"] = boatType;
		PhotonConnectionFactory.Instance.SendGameAction(18, hashtable);
	}

	public void TurnOver()
	{
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["tO"] = true;
		PhotonConnectionFactory.Instance.SendGameAction(19, hashtable);
	}

	private void ResetBoatTravelingFlags()
	{
		this._wasRowing = false;
		this._wasEngineUsed = false;
		this._wasTrolling = false;
	}

	private void UpdateRowing(bool rowing, bool engine)
	{
		if (rowing)
		{
			this._wasRowing = true;
		}
		if (engine)
		{
			this._wasEngineUsed = true;
		}
	}

	private void UpdateTrolling(bool isTrolling)
	{
		if (isTrolling)
		{
			this._wasTrolling = true;
		}
	}

	public void TravelByBoat(ObscuredVector3 playerPosition, ObscuredVector3 playerRotation, ObscuredBool isAnchorDown, ObscuredBool isRowing, ObscuredBool isUsingBoatEngine, ObscuredFloat boatStamina, ObscuredBool isTrolling)
	{
		this.UpdateRowing(ObscuredBool.Decrypt(isRowing.GetEncrypted()), ObscuredBool.Decrypt(isUsingBoatEngine.GetEncrypted()));
		this.UpdateTrolling(ObscuredBool.Decrypt(isTrolling.GetEncrypted()));
		float num = Time.time - this._lastBoatUpdate;
		if (num > 2f)
		{
			Point3 point = playerPosition.ToPoint3();
			if (this._lastPosition != null)
			{
			}
			this._lastPosition = point;
			this._lastBoatUpdate = Time.time;
			Hashtable hashtable = this.CreateActionDataHashtable();
			hashtable["pP"] = playerPosition.ToPoint3();
			hashtable["pR"] = playerRotation.ToPoint3();
			hashtable["aD"] = ObscuredBool.Decrypt(isAnchorDown.GetEncrypted());
			hashtable["R"] = this._wasRowing;
			hashtable["nR"] = this._wasEngineUsed;
			hashtable["bS"] = ObscuredFloat.Decrypt(boatStamina.GetEncrypted());
			hashtable["tR"] = this._wasTrolling;
			PhotonConnectionFactory.Instance.SendGameAction(19, hashtable);
			this.ResetBoatTravelingFlags();
		}
	}

	public void RestoreBoatPosition()
	{
		PhotonConnectionFactory.Instance.SendGameAction(22, this.CreateActionDataHashtable());
	}

	public void Walk(ObscuredVector3 playerPosition, ObscuredVector3 playerRotation)
	{
		if (!this.CheckWalkUpdateTimeout())
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["pP"] = playerPosition.ToPoint3();
		hashtable["pR"] = playerRotation.ToPoint3();
		PhotonConnectionFactory.Instance.SendGameAction(21, hashtable);
	}

	public void Pause()
	{
		GameFactory.GameIsPaused = true;
		Hashtable hashtable = this.CreateActionDataHashtable();
		PhotonConnectionFactory.Instance.SendGameAction(254, hashtable);
	}

	public void Resume(bool sendChangeGameScreen = true)
	{
		GameFactory.GameIsPaused = false;
		if (PhotonConnectionFactory.Instance.IsPondStayFinished)
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		if (GameFactory.Player != null && GameFactory.Player.IsSailing)
		{
			this._lastBoatUpdate = Time.time;
		}
		PhotonConnectionFactory.Instance.SendGameAction(255, hashtable);
		if (sendChangeGameScreen)
		{
			UIStatsCollector.ChangeGameScreen(GameScreenType.Game, GameScreenTabType.Undefined, null, null, null, null, null);
		}
	}

	public void UpdateFeedings(IEnumerable<Feeding> feedings)
	{
		if (feedings == null)
		{
			return;
		}
		Hashtable hashtable = this.CreateActionDataHashtable();
		hashtable["f"] = SerializationHelper.SerializeFeedings(feedings);
		PhotonConnectionFactory.Instance.SendGameAction(252, hashtable);
	}

	public void AddPlayerDetractor(Vector3 position, float radius, float duration)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("pD", SerializationHelper.SerializePlayerDetractor(new PlayerDetractorData(new Vector3f(position), radius, duration)));
		Hashtable hashtable2 = hashtable;
		PhotonConnectionFactory.Instance.SendGameAction(251, hashtable2);
	}

	private Hashtable CreateActionDataHashtable()
	{
		Hashtable hashtable = new Hashtable();
		hashtable["sN"] = this.RodSlot;
		if (this.Rod is Rod1stBehaviour && GameFactory.Player.IsPullTriggered)
		{
			hashtable["iPt"] = true;
		}
		return hashtable;
	}

	private bool CheckUpdateTimeout()
	{
		this.timeSinceLastUpdate += Time.deltaTime;
		if (this.timeSinceLastUpdate > 0.2f)
		{
			this.timeSinceLastUpdate = 0f;
			return true;
		}
		return false;
	}

	private bool CheckWalkUpdateTimeout()
	{
		this.timeSinceLastWalkUpdate += Time.deltaTime;
		if (this.timeSinceLastWalkUpdate > 2f)
		{
			this.timeSinceLastWalkUpdate = 0f;
			return true;
		}
		return false;
	}

	private void UpdateForces(float terminalTackleForce, float maxRodForce, float maxReelForce, float maxFrictionForce, bool isFishPassive, bool hasLineSlack, bool isForced, int speed, bool isReeling, bool canApproach, bool isPoolingOrStriking)
	{
		if (terminalTackleForce > 0f && this.minTerminalTackleForceForPeriod > terminalTackleForce)
		{
			this.minTerminalTackleForceForPeriod = terminalTackleForce;
		}
		if (this.maxTerminalTackleForceForPeriod < terminalTackleForce)
		{
			this.maxTerminalTackleForceForPeriod = terminalTackleForce;
		}
		if (this.maxRodForceForPeriod < maxRodForce)
		{
			this.maxRodForceForPeriod = maxRodForce;
		}
		if (this.maxFrictionForceForPeriod < maxFrictionForce)
		{
			this.maxFrictionForceForPeriod = maxFrictionForce;
		}
		if (this.maxReelForceForPeriod < maxReelForce)
		{
			this.maxReelForceForPeriod = maxReelForce;
		}
		if (!this.wasFishPassive && isFishPassive)
		{
			this.wasFishPassive = true;
		}
		if (!this.hadLineSlack && hasLineSlack)
		{
			this.hadLineSlack = true;
		}
		if (!this.wasForced && isForced)
		{
			this.wasForced = true;
		}
		if (this.maxSpeed < speed)
		{
			this.maxSpeed = speed;
		}
		if (!this.wasReeling && isReeling)
		{
			this.wasReeling = true;
		}
		if (!this.wasAbleToApproach && canApproach)
		{
			this.wasAbleToApproach = true;
		}
		if (!this.wasPoolingOrStriking && isPoolingOrStriking)
		{
			this.wasPoolingOrStriking = true;
		}
	}

	private void UpdatePeriodData(bool isBobberIdle, bool isLyingOnBottom, bool isFeederLyingOnBottom, bool isMoving)
	{
		if (isBobberIdle && !this.wasBobberIdleForPeriod)
		{
			this.wasBobberIdleForPeriod = true;
		}
		if (!isLyingOnBottom && this.wasLyingOnBottom)
		{
			this.wasLyingOnBottom = false;
		}
		if (!isFeederLyingOnBottom && this.wasFeederLyingOnBottom)
		{
			this.wasFeederLyingOnBottom = false;
		}
		if (isMoving && !this.wasMoving)
		{
			this.wasMoving = true;
		}
	}

	public void ResetPeriod()
	{
		this.minTerminalTackleForceForPeriod = 0f;
		this.maxTerminalTackleForceForPeriod = 0f;
		this.maxRodForceForPeriod = 0f;
		this.maxReelForceForPeriod = 0f;
		this.maxFrictionForceForPeriod = 0f;
		this.wasBobberIdleForPeriod = false;
		this.wasLyingOnBottom = true;
		this.wasFeederLyingOnBottom = true;
		this.wasMoving = false;
		this.wasFishPassive = false;
		this.hadLineSlack = false;
		this.wasForced = false;
		this.maxSpeed = 0;
		this.wasReeling = false;
		this.wasAbleToApproach = false;
		this.wasPoolingOrStriking = false;
	}

	private const float MinUpdateTimeout = 0.2f;

	private const float MinBoatUpdateTimeout = 2f;

	private const float MinWalkUpdateTimeout = 2f;

	private const string SlotNumber = "sN";

	private const string PlayerPosition = "pP";

	private const string PlayerRotation = "pR";

	private const string TerminalTacklePosition = "tP";

	private const string LowTerminalTackleForce = "lTf";

	private const string HighTerminalTackleForce = "hTf";

	private const string ThrowTarget = "tT";

	private const string HighRodForce = "hRdF";

	private const string HighReelForce = "hRlF";

	public const string AnyBreaks = "b";

	private const string IsStriking = "iS";

	private const string WrongStriking = "wS";

	private const string IsPulled = "iP";

	private const string IsPullTriggered = "iPt";

	private const string IsBobberIdle = "iBi";

	private const string CastLegth = "cL";

	private const string DragStyle = "d";

	private const string TacklePositionStatus = "tPs";

	private const string FeederPositionStatus = "fPs";

	private const string TackleMoveStatus = "tMs";

	private const string IsFishPassive = "p";

	private const string HasLineSlack = "l";

	private const string IsForced = "iF";

	public const string FishId = "f";

	private const string ReelSpeed = "rS";

	private const string LineLength = "lL";

	private const string Deb = "D";

	private const string DistanceToTackle = "d2t";

	private const string IsReeling = "iR";

	private const string CanApproach = "cA";

	private const string FrictionForce = "fF";

	private const string AttackingFishDistance = "aFd";

	private const string IsPoolingOrStriking = "iPS";

	private const string BoatAnchorDown = "aD";

	private const string BoatWasRowing = "R";

	private const string BoatWasEngineUsed = "nR";

	private const string BoatType = "bT";

	private const string BoatStamina = "bS";

	private const string BoatTurnOver = "tO";

	private const string BoatTrolling = "tR";

	public const string ItemAsset = "iA";

	public const string ItemName = "iN";

	public const string ItemId = "iD";

	public const string ItemCategory = "iC";

	public const string PlayerDetractors = "pD";

	private bool _wasRowing;

	private bool _wasEngineUsed;

	private bool _wasTrolling;

	private Point3 _lastPosition;

	private float timeSinceLastUpdate;

	private float _lastBoatUpdate;

	private float timeSinceLastWalkUpdate;

	private float minTerminalTackleForceForPeriod;

	private float maxTerminalTackleForceForPeriod;

	private float maxRodForceForPeriod;

	private float maxReelForceForPeriod;

	private float maxFrictionForceForPeriod;

	private bool wasBobberIdleForPeriod;

	private bool wasLyingOnBottom;

	private bool wasFeederLyingOnBottom;

	private bool wasMoving;

	private bool wasFishPassive;

	private bool hadLineSlack;

	private bool wasForced;

	private bool wasReeling;

	private bool wasAbleToApproach;

	private bool wasPoolingOrStriking;

	private int maxSpeed;
}
