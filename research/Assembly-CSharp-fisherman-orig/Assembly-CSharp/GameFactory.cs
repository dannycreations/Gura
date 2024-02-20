using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Boats;
using ObjectModel;
using Photon.Interfaces;
using Phy;
using UnityEngine;

public static class GameFactory
{
	static GameFactory()
	{
		GameFactory.OnChatInGameCreated = delegate
		{
		};
		GameFactory.Clear(true);
	}

	public static EventCode LoadingFishEvent { get; set; }

	public static bool GameIsPaused { get; set; }

	public static PlayerController Player { get; set; }

	public static bool IsPlayerInitialized { get; private set; }

	public static Collider FishingZonesPlayerCollider
	{
		get
		{
			return (!(GameFactory.Player != null)) ? null : GameFactory.Player.ZonesCollider;
		}
	}

	public static Transform DebugPlayer { get; set; }

	public static Transform PlayerTransform
	{
		get
		{
			return GameFactory.DebugPlayer ?? ((!(GameFactory.Player != null)) ? null : GameFactory.Player.transform);
		}
	}

	public static Transform PlayerRoot
	{
		get
		{
			Transform transform;
			if ((transform = GameFactory.DebugPlayer) == null)
			{
				transform = ((!(GameFactory.Player != null)) ? null : (GameFactory.Player.IsSailing ? ((!(GameFactory.Player.CameraController != null) || !(GameFactory.Player.CameraController.Camera != null)) ? null : GameFactory.Player.CameraController.Camera.transform) : GameFactory.Player.Collider));
			}
			return transform;
		}
	}

	public static FishSpawner FishSpawner { get; set; }

	public static BoatDock BoatDock { get; set; }

	public static MessageController Message { get; set; }

	public static IBobberIndicatorController BobberIndicator { get; set; }

	public static BottomFishingIndicator BottomIndicator
	{
		get
		{
			return GameFactory.Player.HudFishingHandler.BottomFishingIndicator;
		}
	}

	public static FeederFishingIndicator QuiverIndicator
	{
		get
		{
			return GameFactory.Player.HudFishingHandler.FeederFishingIndicator;
		}
	}

	public static IWaterController Water { get; set; }

	public static IWaterFlowController WaterFlow { get; set; }

	public static ChatListener ChatListener { get; set; }

	public static ISkyController SkyControllerInstance { get; set; }

	public static IAudioController AudioController { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action OnChatInGameCreated;

	public static NewChatInGameController ChatInGameController { get; set; }

	public static InteractiveObjectController InteractiveObjectController { get; set; }

	public static bool Is3dViewVisible { get; set; }

	public static List<LocationBrief> PondLocationsInfo { get; private set; }

	public static void InGameChatCreated()
	{
		GameFactory.OnChatInGameCreated();
	}

	public static void ClearPondLocationsInfo()
	{
		GameFactory.PondLocationsInfo = null;
	}

	public static void SetPondLocationsInfo(IEnumerable<LocationBrief> locations)
	{
		GameFactory.PondLocationsInfo = ((locations == null) ? new List<LocationBrief>() : locations.ToList<LocationBrief>());
		LogHelper.Log("SetPondLocationsInfo Count:{0}", new object[] { GameFactory.PondLocationsInfo.Count });
	}

	public static bool IsRodAssembling
	{
		get
		{
			return GameFactory.RodSlot.AssemblingRodsCounter > 0;
		}
	}

	public static GameFactory.RodSlot SetSlot(int index)
	{
		if (index > 0 && index < GameFactory.RodSlots.Length)
		{
			return GameFactory.RodSlots[index] = new GameFactory.RodSlot(index);
		}
		return null;
	}

	public static GameFactory.RodSlot SetSlot(GameFactory.RodSlot newSlot)
	{
		if (newSlot.Index > 0 && newSlot.Index < GameFactory.RodSlots.Length)
		{
			GameFactory.RodSlots[newSlot.Index] = newSlot;
			return newSlot;
		}
		return null;
	}

	public static void SetupPlayer(PlayerController player)
	{
		GameFactory.Player = player;
		GameFactory.IsPlayerInitialized = true;
	}

	public static void Clear(bool reInitPhy = true)
	{
		for (int i = 0; i < GameFactory.RodSlots.Length; i++)
		{
			if (GameFactory.RodSlots[i] == null)
			{
				GameFactory.RodSlots[i] = new GameFactory.RodSlot(i);
			}
		}
		GameFactory.Player = null;
		GameFactory.IsPlayerInitialized = false;
		GameFactory.FishSpawner = null;
		GameFactory.Message = null;
		GameFactory.BobberIndicator = null;
		GameFactory.Water = null;
		GameFactory.WaterFlow = null;
		GameFactory.ChatListener = null;
		GameFactory.ChatInGameController = null;
		GameFactory.InteractiveObjectController = null;
	}

	public const int MaxRodSlots = 8;

	public static GameFactory.RodSlot[] RodSlots = new GameFactory.RodSlot[8];

	public class RodSlot
	{
		public RodSlot(int index)
		{
			this.Index = index;
			this.Clear();
			this.LineClips = new Stack<float>();
			this.uid = GameFactory.RodSlot.uidCounter++;
			Debug.LogWarning(string.Format("RodSlot uid={0} #{1} created as new", this.uid, this.Index));
			if (index >= 0)
			{
				this.initPhy();
			}
		}

		public RodSlot(GameFactory.RodSlot oldSlot)
		{
			this.Index = oldSlot.Index;
			this.Clear();
			this.LineClips = new Stack<float>(oldSlot.LineClips);
			this.uid = GameFactory.RodSlot.uidCounter++;
			if (this.Index >= 0)
			{
				this.initPhy();
			}
			Debug.LogWarning(string.Format("RodSlot uid={0} #{1} created as copy LineClip={2}", this.uid, this.Index, (this.LineClips.Count <= 0) ? (-1f) : this.LineClips.Peek()));
		}

		public int uid { get; private set; }

		public SimulationThread SimThread { get; private set; }

		public FishingRodSimulation Sim { get; private set; }

		public RodBehaviour Rod { get; private set; }

		public TackleBehaviour Tackle { get; private set; }

		public ReelBehaviour Reel { get; private set; }

		public LineBehaviour Line { get; private set; }

		public BellBehaviour Bell { get; private set; }

		public bool IsRodAssembling { get; private set; }

		public int RodItemId { get; private set; }

		public int ReelItemId { get; private set; }

		public int LineItemId { get; private set; }

		public bool PendingServerOp { get; private set; }

		public bool FishBiteConfirmRequestSent { get; private set; }

		public bool IsEmpty
		{
			get
			{
				return this.Rod == null && this.Reel == null && this.Line == null && this.Tackle == null;
			}
		}

		public int Index { get; private set; }

		public static int AssemblingRodsCounter { get; private set; }

		public void Clear()
		{
			if (this.SimThread != null)
			{
				this.SimThread.ForceStop();
			}
			this.SimThread = null;
			this.Sim = null;
			this.Rod = null;
			this.Tackle = null;
			this.Reel = null;
			this.Line = null;
			this.Bell = null;
			Debug.LogWarning(string.Format("RodSlot uid={0} #{1} Clear() LineClip={2}", this.uid, this.Index, (this.LineClips == null || this.LineClips.Count <= 0) ? (-1f) : this.LineClips.Peek()));
		}

		private void initPhy()
		{
			this.Sim = new FishingRodSimulation("RodMainSource" + this.Index, true);
			this.SimThread = new SimulationThread("Rod" + this.Index, this.Sim, new FishingRodSimulation("RodMainThread" + this.Index, false));
			this.Sim.PhyActionsListener = this.SimThread;
		}

		public void SetRod(RodBehaviour newRod)
		{
			Debug.LogWarning(string.Format("RodSlot uid={3} #{0} rod behaviour changed from {1} to {2}", new object[] { this.Index, this.Rod, newRod, this.uid }));
			this.Rod = newRod;
			this.RodItemId = this.Rod.RodAssembly.RodInterface.ItemId;
		}

		public void SetTackle(TackleBehaviour newTackle)
		{
			Debug.LogWarning(string.Format("RodSlot uid={3} #{0} tackle behaviour changed from {1} to {2}", new object[] { this.Index, this.Tackle, newTackle, this.uid }));
			this.Tackle = newTackle;
		}

		public void SetReel(ReelBehaviour newReel)
		{
			Debug.LogWarning(string.Format("RodSlot uid={3} #{0} reel behaviour changed from {1} to {2}", new object[] { this.Index, this.Reel, newReel, this.uid }));
			this.Reel = newReel;
			this.ReelItemId = this.Rod.RodAssembly.ReelInterface.ItemId;
		}

		public void SetLine(LineBehaviour newLine)
		{
			Debug.LogWarning(string.Format("RodSlot uid={3} #{0} line behaviour changed from {1} to {2}", new object[] { this.Index, this.Line, newLine, this.uid }));
			this.Line = newLine;
			this.LineItemId = this.Rod.RodAssembly.LineInterface.ItemId;
		}

		public void SetBell(BellBehaviour newBell)
		{
			Debug.LogWarning(string.Format("RodSlot uid={3} #{0} bell behaviour changed from {1} to {2}", new object[] { this.Index, this.Bell, newBell, this.uid }));
			this.Bell = newBell;
		}

		public void BeginRodAssembly()
		{
			this.IsRodAssembling = true;
			GameFactory.RodSlot.AssemblingRodsCounter++;
			Debug.LogWarning(string.Format("RodSlot #{0} starts rod assembly", this.Index));
		}

		public void FinishRodAssembly()
		{
			this.IsRodAssembling = false;
			if (this.Rod != null && this.Rod.RodAssembly != null)
			{
				this.RodItemId = this.Rod.RodAssembly.RodInterface.ItemId;
				this.ReelItemId = this.Rod.RodAssembly.ReelInterface.ItemId;
				this.LineItemId = this.Rod.RodAssembly.LineInterface.ItemId;
			}
			else
			{
				this.RodItemId = 0;
				this.ReelItemId = 0;
				this.LineItemId = 0;
			}
			GameFactory.RodSlot.AssemblingRodsCounter--;
			Debug.LogWarning(string.Format("RodSlot #{0} finishes rod assembly", this.Index));
		}

		public void BeginServerOp()
		{
			this.PendingServerOp = true;
			Debug.LogWarning(string.Format("RodSlot #{0} waits for server operation", this.Index));
		}

		public void FinishServerOp()
		{
			this.PendingServerOp = false;
			Debug.LogWarning(string.Format("RodSlot #{0} server operation finished", this.Index));
		}

		public void OnSendFishBiteConfirmRequest()
		{
			this.FishBiteConfirmRequestSent = true;
		}

		public void OnResetFishBiteConfirmRequest()
		{
			this.FishBiteConfirmRequestSent = false;
		}

		public void ClearLastReelClip()
		{
			if (this.LineClips.Count > 0)
			{
				this.Line.MaxLineLength = this.LineClips.Pop();
				this.Line.MaxLineLength = this.Line.AvailableLineLengthOnSpool;
			}
			else
			{
				this.Line.MaxLineLength = this.Line.AvailableLineLengthOnSpool;
			}
		}

		public void AddReelClip()
		{
			if (this.LineClips.Count < 1)
			{
				this.LineClips.Push(this.Sim.CurrentLineLength);
				this.Line.MaxLineLength = this.Sim.CurrentLineLength;
			}
		}

		public void SuspendReelClip()
		{
			this.Line.MaxLineLength = this.Line.AvailableLineLengthOnSpool;
		}

		public void RestoreReelClip()
		{
			if (this.LineClips.Count > 0)
			{
				this.Line.MaxLineLength = this.LineClips.Peek();
			}
		}

		public bool IsInHands;

		public Stack<float> LineClips;

		private static int uidCounter;

		public const int MaxReelClips = 1;
	}
}
