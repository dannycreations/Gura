using System;
using System.Collections.Generic;
using Mono.Simd.Math;
using ObjectModel;
using Phy;
using UnityEngine;

public class RodOnPodBehaviour : RodBehaviour
{
	public RodOnPodBehaviour(RodController controller, RodOnPodBehaviour.TransitionData transitionData, GameFactory.RodSlot slot)
		: base(controller, transitionData.rodAssembly, slot)
	{
		this._tpmId = transitionData.tpmId;
		this.gameInstance = PhotonConnectionFactory.Instance.GetGameSlot(transitionData.rodAssembly.Slot);
		this.TackleType = transitionData.tackleType;
		this.tackleObject = transitionData.tackleObject;
		this.tackleController = this.tackleObject.GetComponent<TackleControllerBase>();
		this.hookObject = transitionData.hookObject;
		this.mainLineObject = transitionData.mainLineObject;
		this.leaderLineObject = transitionData.leaderLineObject;
		this.lineOnRodObject = transitionData.lineOnRodObject;
		this.signalizerObject = transitionData.Signalizer;
		this.LeaderLength = transitionData.leaderLength;
		this.reelFrictionForce = transitionData.reelFrictionForce;
		this.tdBackup = new RodOnPodBehaviour.TransitionData
		{
			tackleMass = transitionData.tackleMass,
			tackleBuoyancy = transitionData.tackleBuoyancy,
			tackleSize = transitionData.tackleSize,
			tackleType = transitionData.tackleType,
			rootPosition = transitionData.rootPosition,
			rodLength = transitionData.rodLength,
			rodAssembly = transitionData.rodAssembly,
			baitMass = transitionData.baitMass,
			baitBuoyancy = transitionData.baitBuoyancy,
			bobberMass = transitionData.bobberMass,
			leaderLength = transitionData.leaderLength,
			solidFactor = transitionData.solidFactor,
			bounceFactor = transitionData.bounceFactor,
			staticFrictionFactor = transitionData.staticFrictionFactor,
			slidingFrictionFactor = transitionData.slidingFrictionFactor,
			extrudeFactor = transitionData.extrudeFactor,
			tacklePosition = transitionData.tacklePosition,
			fishPosition = transitionData.fishPosition
		};
		this.tackleObject.transform.parent = base.gameObject.transform.parent;
		if (this.hookObject != null)
		{
			this.hookObject.transform.parent = base.gameObject.transform.parent;
		}
		this.mainLineObject.transform.parent.parent = base.gameObject.transform.parent;
		if (this.leaderLineObject != null)
		{
			this.leaderLineObject.transform.parent.parent = base.gameObject.transform.parent;
		}
		this.lineOnRodObject.transform.parent.parent = base.gameObject.transform.parent;
		this.mainLineSpline = transitionData.LineSpline;
		this.lrMainLine = this.mainLineObject.GetComponent<LineRenderer>();
		this.lrMainLine.positionCount = 101;
		this.lrLineOnRodObject = this.lineOnRodObject.GetComponent<LineRenderer>();
		if (this.leaderLineObject != null)
		{
			this.lrLeaderLine = this.leaderLineObject.GetComponent<LineRenderer>();
			this.lrLeaderLine.positionCount = 2;
		}
		this.lineTransitionStartTime = Time.time;
		if (RodOnPodBehaviour.RodPodSim == null)
		{
			RodOnPodBehaviour.InitRodPodSim();
		}
		this.rootPosition = transitionData.rootPosition;
		this.rootDirection = (transitionData.tipPosition - transitionData.rootPosition).normalized;
		transitionData.rootDirection = this.rootDirection;
		this.rodLength = transitionData.rodLength;
		RodOnPodBehaviour.RodPodSimThread.OnThreadException += this.OnSimThreadException;
	}

	public FishingRodSimulation.TackleType TackleType { get; private set; }

	public bool IsFloatTackle
	{
		get
		{
			return this.TackleType == FishingRodSimulation.TackleType.Float;
		}
	}

	public bool IsLureTackle
	{
		get
		{
			return this.TackleType == FishingRodSimulation.TackleType.Lure || this.TackleType == FishingRodSimulation.TackleType.Topwater || this.TackleType == FishingRodSimulation.TackleType.Wobbler;
		}
	}

	public static SimplifiedMultiRodSimulation RodPodSim { get; protected set; }

	public static SimulationThread RodPodSimThread { get; protected set; }

	public RodAndTackleOnPodObject RodOnPodObject
	{
		get
		{
			return base.RodObject as RodAndTackleOnPodObject;
		}
	}

	public FishOnPodBehaviour FishBehaviour { get; protected set; }

	public override Mass TackleTipMass
	{
		get
		{
			return this.RodOnPodObject.hookMass;
		}
	}

	public override Vector3 CurrentTipPosition
	{
		get
		{
			return this.RodOnPodObject.TipMass.Position;
		}
	}

	public bool DetectLineSlack
	{
		get
		{
			return this.lineSlackDetectionStartTimeStamp > 0f && Time.time > this.lineSlackDetectionStartTimeStamp;
		}
	}

	public Game gameInstance { get; private set; }

	public int TpmId
	{
		get
		{
			return this._tpmId;
		}
	}

	public float LeaderLength { get; private set; }

	public float LineLength { get; private set; }

	public float RodTipToTackleDistance
	{
		get
		{
			return (this.RodOnPodObject.TipMass.Position - this.RodOnPodObject.tackleMass.Position).magnitude;
		}
	}

	public Fish SpawnedFish { get; private set; }

	public RodOnPodBehaviour.PodFishState FishState { get; private set; }

	public static void InitRodPodSim()
	{
		RodOnPodBehaviour.RodPodSim = new SimplifiedMultiRodSimulation("RodPodSource");
		RodOnPodBehaviour.RodPodSimThread = new SimulationThread("RodPod", RodOnPodBehaviour.RodPodSim, new SimplifiedMultiRodSimulation("RodPodThread"));
		RodOnPodBehaviour.RodPodSim.PhyActionsListener = RodOnPodBehaviour.RodPodSimThread;
		RodOnPodBehaviour.RodPodSimThread.Start();
	}

	public static void UpdateRodPodSim()
	{
		if (RodOnPodBehaviour.RodPodSimThread != null)
		{
			foreach (Mass mass in RodOnPodBehaviour.RodPodSim.Masses)
			{
				if (!mass.IsKinematic)
				{
					Vector3 position = mass.Position;
					Vector3 up = Vector3.up;
					Math3d.GetGroundCollision(position, out position, out up, 0.5f);
					mass.GroundPoint = position;
					mass.GroundNormal = up;
				}
				RodBehaviour.UpdateMassEnvironment(mass);
			}
			RodOnPodBehaviour.RodPodSimThread.SyncMain();
		}
	}

	public static void StopRodPodSim()
	{
		if (RodOnPodBehaviour.RodPodSimThread != null)
		{
			RodOnPodBehaviour.RodPodSimThread.ForceStop();
			RodOnPodBehaviour.RodPodSimThread = null;
			RodOnPodBehaviour.RodPodSim.Clear();
			RodOnPodBehaviour.RodPodSim.RefreshObjectArrays(true);
			RodOnPodBehaviour.RodPodSim = null;
		}
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Init(RodOnPodBehaviour.TransitionData transitionData = null)
	{
		base.Init(transitionData);
		base.RodObject = RodOnPodBehaviour.RodPodSim.AddRod(base.Controller.segment, this, base.Tackle, base.RodSlot, transitionData);
		this.RodOnPodObject.TipMass.IsRef = true;
		this.RodOnPodObject.UpdateSim();
		this._rodPoints = new Vector3[base.RodPointsCount];
		this.updateRodPoints();
		base.transform.position = transitionData.rootPosition;
		base.InitProceduralBend(this._rodPoints);
		this._lineBezierCurve = new BezierCurve(2);
		this.LineLength = this.RodOnPodObject.lineSpring.SpringLength;
		base.Line.MinLineLengthWithFish = transitionData.rodLength * 0.85f;
		if (transitionData.spawnedFish != null)
		{
			this.SpawnFish(transitionData.spawnedFish, transitionData);
		}
	}

	public void SpawnFish(Fish fishTemplate, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		if (this.SpawnedFish != null)
		{
			this.EscapeFish();
		}
		this.SpawnedFish = fishTemplate;
		this.FishState = RodOnPodBehaviour.PodFishState.Attacking;
		if (transitionData != null && transitionData.fishInitialState is FishHooked)
		{
			this.FishState = RodOnPodBehaviour.PodFishState.Hooked;
		}
		this.FishBehaviour = new FishOnPodBehaviour(this, this.RodOnPodObject.hookMass.Position + Vector3.up * 0.25f, fishTemplate, transitionData);
		if (transitionData == null || transitionData.spawnedFish == null)
		{
			this.FishBehaviour.tpmId = GameFactory.Player.InformAboutNewFish(fishTemplate, this.RodOnPodObject.hookMass.Position);
		}
		if (GameFactory.Player.IsCurrentlyTrolling)
		{
			GameFactory.Player.OverwriteCatchedFishPos(base.RodSlot.Index, this.RodOnPodObject.hookMass.Position);
		}
	}

	public void OnSimThreadException()
	{
		this.SimThreadExceptionThrown = true;
	}

	public void ResetSimulation()
	{
		this.SimThreadExceptionThrown = false;
		Vector3? vector = this.lastValidTacklePosition;
		if (vector != null)
		{
			this.tdBackup.tacklePosition = this.lastValidTacklePosition.Value;
		}
		Vector3? vector2 = this.lastValidFishPosition;
		if (vector2 != null)
		{
			this.tdBackup.fishPosition = this.lastValidFishPosition.Value;
		}
		if (this.FishBehaviour != null)
		{
			this.tdBackup.fishInitialBehavior = this.FishBehaviour.Behavior;
			this.tdBackup.fishInitialState = this.FishBehaviour.StateInstance;
			this.tdBackup.fishTpmId = this.FishBehaviour.tpmId;
		}
		this.Init(this.tdBackup);
		if (this.SpawnedFish != null)
		{
			RodOnPodBehaviour.RodPodSim.AddFish(this.RodOnPodObject, this.tdBackup.fishPosition, this.SpawnedFish.Weight, this.SpawnedFish.Force.Value * 9.81f, this.SpawnedFish.Speed.Value, this.SpawnedFish.Length);
			this.FishBehaviour.ai.RefreshFishObject(this.RodOnPodObject.fishObject);
			if (this.FishBehaviour.State == typeof(FishHooked))
			{
				RodOnPodBehaviour.RodPodSim.HookFish(this.RodOnPodObject);
			}
			else if (this.FishBehaviour.State == typeof(FishBite))
			{
				this.FishBehaviour.Bite();
			}
		}
	}

	public void HookFish()
	{
		if (this.SpawnedFish != null)
		{
			this.FishState = RodOnPodBehaviour.PodFishState.Hooked;
			this.lineSlackDetectionStartTimeStamp = Time.time + Random.Range(60f, 60f);
			this.FishBehaviour.Behavior = FishBehavior.Hook;
			base.Tackle.Fish = this.FishBehaviour;
			base.Line.MaxLineLength = base.Line.AvailableLineLengthOnSpool;
		}
	}

	public void EscapeFish()
	{
		if (this.SpawnedFish != null)
		{
			this.lineSlackDetectionStartTimeStamp = 0f;
			this.FishBehaviour.Escape();
			this.FishBehaviour.Update();
			if (this.SpawnedFish != null)
			{
				this.FishBehaviour.Update();
			}
		}
	}

	public void OnFishDestroy()
	{
		this.FishBehaviour = null;
		this.SpawnedFish = null;
		this.FishState = RodOnPodBehaviour.PodFishState.None;
		this.lastValidFishPosition = null;
	}

	private Vector3 rodBendPosition()
	{
		return this.rootPosition + 0.65f * this.rootDirection * this.rodLength;
	}

	private void updateRodPoints()
	{
		this.rootPositionCorrection = this._owner.segment.firstTransform.position - this.RodOnPodObject.RootMass.Position;
		Quaternion quaternion = Quaternion.FromToRotation(this.rootDirection, this._owner.segment.firstTransform.forward);
		for (int i = 0; i < base.RodPointsCount; i++)
		{
			Vector3 vector = this._owner.segment.firstTransform.position + quaternion * (this.RodOnPodObject.Masses[i].Position - this.RodOnPodObject.RootMass.Position);
			this._rodPoints[i] = base.transform.InverseTransformPoint(vector);
		}
		this.rootPosition = this._owner.segment.firstTransform.position;
		this.rootDirection = this._owner.segment.firstTransform.forward;
		this.RodOnPodObject.RootKinematicSpline.SetNextPointAndRotation(this.rootPosition, Quaternion.FromToRotation(Vector3.forward, this.rootDirection));
	}

	private void updateLineOnRod()
	{
		bool flag = this.signalizerObject != null && (GameFactory.Player == null || (GameFactory.Player.State != typeof(ReplaceRodOnPodTakeAndPut) && GameFactory.Player.State != typeof(ReplaceRodOnPodOut)));
		Vector3 reelLineArcLocatorPos = base.Reel.ReelHandler.LineArcLocator.transform.position;
		this.LinePoints.Clear();
		this.LinePoints.Add(reelLineArcLocatorPos);
		for (int i = 0; i < base.LineLocatorsProp.Count; i++)
		{
			this.LinePoints.Add(base.LineLocatorsProp[i].position);
		}
		if (flag)
		{
			this.LinePoints.Add(this.signalizerObject.ClipPosition);
			this.LinePoints.Add(this.signalizerObject.NestPosition);
			this.LinePoints.Sort((Vector3 a, Vector3 b) => (a - reelLineArcLocatorPos).sqrMagnitude.CompareTo((b - reelLineArcLocatorPos).sqrMagnitude));
		}
		this.lrLineOnRodObject.SetPositions(this.LinePoints.ToArray());
		this.lrLineOnRodObject.positionCount = this.LinePoints.Count - 1;
	}

	private void updateMainLine()
	{
		this._lineBezierCurve.AnchorPoints[0] = base.LineLocatorsProp[base.LineLocatorsProp.Count - 1].position;
		this._lineBezierCurve.AnchorPoints[2] = this.RodOnPodObject.tackleMass.Position;
		this._lineBezierCurve.AnchorPoints[1] = (this._lineBezierCurve.AnchorPoints[0] + this._lineBezierCurve.AnchorPoints[2]) * 0.5f;
		float magnitude = (this.RodOnPodObject.TipMass.Position - this.RodOnPodObject.tackleMass.Position).magnitude;
		float y = this._lineBezierCurve.AnchorPoints[0].y;
		float y2 = this._lineBezierCurve.AnchorPoints[2].y;
		this._lineBezierCurve.AnchorPoints[1].y = Mathf.Lerp(y2, (y + y2) * 0.5f, Mathf.Min(1f, Mathf.Exp(magnitude - this.RodOnPodObject.lineSpring.SpringLength)));
		this.lrMainLine.SetPosition(0, base.LineLocatorsProp[base.LineLocatorsProp.Count - 1].position);
		for (int i = 0; i < 100; i++)
		{
			float num = (float)(i + 1) / 99f;
			this.lrMainLine.SetPosition(i + 1, Vector3.Lerp(this._lineBezierCurve.Point(num), this.mainLineSpline[i] + (base.LineLocatorsProp[base.LineLocatorsProp.Count - 1].position - this.mainLineSpline[0]), Mathf.Exp(this.lineTransitionStartTime - Time.time)));
		}
	}

	private void updateLeaderLine()
	{
		if (this.lrLeaderLine != null)
		{
			this.lrLeaderLine.SetPosition(0, this.RodOnPodObject.tackleMass.Position);
			this.lrLeaderLine.SetPosition(1, this.RodOnPodObject.hookMass.Position);
		}
	}

	private void updateAppliedForces()
	{
		float num = Vector3.Angle(this._owner.segment.firstTransform.forward, base.RodObject.TipMass.AvgForce.normalized);
		base.ReelDamper = base.CalcReelDamper(num);
		this.CalculateAppliedForce();
		base.Reel.CalculateAppliedForce();
		base.Line.CalculateAppliedForce();
		float num2 = this.AdjustedAppliedForce * base.ReelDamper;
		float num3 = num2 / this.reelFrictionForce;
		if (num2 > this.reelFrictionForce)
		{
			float num4;
			if (this.reelFrictionForce < 0.001f)
			{
				num4 = base.Reel.Owner.speed * 4f;
			}
			else
			{
				num4 = base.Reel.Owner.speed * 4f * Mathf.Clamp01(num3 - 1f);
			}
			this.LineLength += num4 * Time.deltaTime;
			this.LineLength = Mathf.Min(this.LineLength, base.Line.MaxLineLength);
			this.RodOnPodObject.lineSpring.SpringLength = this.LineLength;
		}
		base.Reel.UpdateFrictionState(num3);
	}

	private void debugDraw()
	{
		Color color;
		color..ctor(1f, 1f, 0.3f);
		Color color2;
		color2..ctor(1f, 0.3f, 1f);
		Color color3;
		color3..ctor(0.3f, 0.3f, 1f);
		Color color4;
		color4..ctor(1f, 0.3f, 0.3f);
		Debug.DrawLine(this.RodOnPodObject.RootMass.Position, this.RodOnPodObject.TipMass.Position, color);
		Debug.DrawLine(this.RodOnPodObject.TipMass.Position, this.RodOnPodObject.tackleMass.Position, color2);
		Debug.DrawLine(this.RodOnPodObject.tackleMass.Position, this.RodOnPodObject.hookMass.Position, color3);
		if (this.RodOnPodObject.fishObject != null)
		{
			Debug.DrawLine(this.RodOnPodObject.fishObject.Mouth.Position, this.RodOnPodObject.fishObject.Root.Position, color4);
		}
	}

	public override void LateUpdateBeforeSim()
	{
		RodOnPodBehaviour.RodPodSimThread.DebugTrigger = this._owner.SimThreadDebugTrigger || RodOnPodBehaviour.RodPodSimThread.DebugTrigger;
		this._owner.SimThreadDebugTrigger = false;
		if (this.SimThreadExceptionThrown)
		{
			this.ResetSimulation();
		}
		this.updateRodPoints();
		if (this.TackleTipMass != null)
		{
			this.TackleTipMass.CheckLayer();
		}
	}

	public override void LateUpdateAfterSim()
	{
		base.UpdateProceduralBend(this._rodPoints);
		if (this.IsFloatTackle)
		{
			this.tackleObject.transform.localScale = FloatBehaviour.GetBobberScale(this.RodOnPodObject.tackleMass.Position);
		}
		if (this.hookObject != null)
		{
			this.hookObject.transform.position = this.RodOnPodObject.hookMass.Position;
		}
		this.updateMainLine();
		this.updateLeaderLine();
		this.updateLineOnRod();
		this.updateAppliedForces();
		if (this.FishBehaviour != null)
		{
			GameFactory.Player.UpdateFakeFish(this.FishBehaviour.tpmId, this.FishBehaviour.MouthPosition);
			this.FishBehaviour.Update();
		}
		if (base.Tackle != null)
		{
			base.Tackle.OnFsmUpdate();
			base.Tackle.OnLateUpdate();
		}
		if (base.Bell != null)
		{
			base.Bell.OnLateUpdate();
			base.Bell.SoundUpdate();
		}
		Vector3[] array = new Vector3[]
		{
			this._lineBezierCurve.Point(0.5f),
			this.RodOnPodObject.tackleMass.Position,
			this.RodOnPodObject.hookMass.Position
		};
		Vector3 vector = (this.RodOnPodObject.tackleMass.Position + this.RodOnPodObject.hookMass.Position) * 0.5f;
		bool flag = this.TackleType == FishingRodSimulation.TackleType.Float || this.TackleType == FishingRodSimulation.TackleType.Feeder || this.TackleType == FishingRodSimulation.TackleType.CarpClassic || this.TackleType == FishingRodSimulation.TackleType.CarpMethod || this.TackleType == FishingRodSimulation.TackleType.CarpPVABag || this.TackleType == FishingRodSimulation.TackleType.CarpPVAStick;
		GameFactory.Player.UpdateFakeRod(this._tpmId, base.transform, this._rodPoints, array, vector, flag, (!(this.tackleObject != null)) ? null : this.tackleObject.transform, (!(this.hookObject != null)) ? null : this.hookObject.transform);
		if (this.RodOnPodObject.tackleMass.Position.y > this.RodOnPodObject.tackleMass.GroundHeight && !Vector3Extension.IsNaN(this.RodOnPodObject.tackleMass.Position) && !Vector3Extension.IsInfinity(this.RodOnPodObject.tackleMass.Position))
		{
			this.lastValidTacklePosition = new Vector3?(this.RodOnPodObject.tackleMass.Position);
		}
		if (this.RodOnPodObject.fishObject != null)
		{
			Vector3 position = this.RodOnPodObject.fishObject.Mouth.Position;
			if (position.y > this.RodOnPodObject.fishObject.Mouth.GroundHeight && !Vector3Extension.IsNaN(position) && !Vector3Extension.IsInfinity(position))
			{
				this.lastValidFishPosition = new Vector3?(position);
			}
		}
	}

	public void OnTakeRodFromPod()
	{
		base.Reel.StopSounds();
		if (this.FishBehaviour != null && (this.FishBehaviour.State == typeof(FishSwimAway) || this.FishBehaviour.State == typeof(FishHooked)))
		{
			this.gameInstance.Adapter.FinishAttackOnPod(true, false, false, true, 0f);
		}
	}

	public override void Clean()
	{
		Debug.LogWarning("RodOnPodBehaviour.Clean " + ((!(this._owner != null)) ? "null" : base.InstanceID.ToString()));
		if (this._owner == null)
		{
			return;
		}
		base.Reel.StopSounds();
		base.Clean();
		this.RodOnPodObject.Remove();
		if (this.FishBehaviour != null)
		{
			this.FishBehaviour.Clean();
		}
		if (RodOnPodBehaviour.RodPodSim != null)
		{
			RodOnPodBehaviour.RodPodSim.RefreshObjectArrays(true);
			RodOnPodBehaviour.RodPodSimThread.OnThreadException -= this.OnSimThreadException;
		}
	}

	public const int MainLineSplinePointsCount = 100;

	private const float LineSlackDetectionCooldownMin = 60f;

	private const float LineSlackDetectionCooldownMax = 60f;

	private Vector3[] _rodPoints;

	private BezierCurve _lineBezierCurve;

	private GameObject tackleObject;

	private TackleControllerBase tackleController;

	private GameObject hookObject;

	private GameObject mainLineObject;

	private GameObject leaderLineObject;

	private GameObject lineOnRodObject;

	private RodStandSignalizer signalizerObject;

	private LineRenderer lrMainLine;

	private LineRenderer lrLeaderLine;

	private LineRenderer lrLineOnRodObject;

	private Vector3[] mainLineSpline;

	private float lineTransitionStartTime;

	private float lineSlackDetectionStartTimeStamp;

	private Vector3 rootPosition;

	private Vector3 rootDirection;

	private float rodLength;

	private float reelFrictionForce;

	private int _tpmId;

	private RodOnPodBehaviour.TransitionData tdBackup;

	private Vector3? lastValidFishPosition;

	private Vector3? lastValidTacklePosition;

	private bool SimThreadExceptionThrown;

	private Vector3 rootPositionCorrection;

	private List<Vector3> LinePoints = new List<Vector3>();

	public enum PodFishState
	{
		None,
		Attacking,
		Hooked
	}

	public class TransitionData
	{
		public AssembledRod rodAssembly;

		public Vector3 rootPosition;

		public Quaternion rootRotation;

		public Vector3 rootDirection;

		public Vector3 tipPosition;

		public Vector3 tacklePosition;

		public Quaternion tackleRotation;

		public float tackleGroundHeight;

		public float rodLength;

		public float tackleMass;

		public float tackleBuoyancy;

		public float tackleBuoyancySpeedMult;

		public float tackleWaterResistance;

		public float tackleSize;

		public float baitMass;

		public float baitBuoyancy;

		public float bobberMass;

		public float solidFactor;

		public Vector3 axialDragFactors;

		public float bounceFactor;

		public float staticFrictionFactor;

		public float slidingFrictionFactor;

		public float extrudeFactor;

		public float lineLength;

		public float leaderLength;

		public bool isBaitcasting;

		public Type tackleInitialState;

		public bool tackleAttackFinished;

		public bool tackleAttackFinishedRequested;

		public float tackleStrikeTimeEnd;

		public FishingRodSimulation.TackleType tackleType;

		public int reelSpeedSection;

		public int reelFrictionSection;

		public float reelFrictionForce;

		public float reelMaxLineLength;

		public GameObject rodObject;

		public GameObject reelObject;

		public GameObject bellObject;

		public GameObject tackleObject;

		public GameObject secondaryTackleObject;

		public GameObject lineObject;

		public GameObject hookObject;

		public GameObject baitObject;

		public GameObject mainLineObject;

		public GameObject leaderLineObject;

		public GameObject lineOnRodObject;

		public List<GameObject> sinkers;

		public Fish spawnedFish;

		public FsmBaseState<IFishController> fishInitialState;

		public FishBehavior fishInitialBehavior;

		public Vector3 fishPosition;

		public Action OnCreated;

		public Vector3 InitialPosition;

		public Quaternion InitialRotation;

		public RodStandSignalizer Signalizer;

		public Vector3[] LineSpline;

		public int tpmId;

		public int fishTpmId;

		public int underwaterItemId;

		public GameObject underwaterItemObject;

		public ResourceRequest underwaterItemAsyncRequest;

		public string underwaterItemName;

		public string underwaterItemCategory;
	}
}
