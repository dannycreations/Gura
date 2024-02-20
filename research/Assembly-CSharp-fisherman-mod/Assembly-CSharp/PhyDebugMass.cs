using System;
using Mono.Simd.Math;
using Phy;
using UnityEngine;

public class PhyDebugMass : MonoBehaviour
{
	public virtual void Init(Mass phyMass, SimulationThread simThread, PhyDebugTool tool)
	{
		this.phyMass = phyMass;
		base.transform.position = phyMass.Position;
		base.name = "Mass_" + phyMass.UID;
		this.tool = tool;
		this.UID = phyMass.UID;
		this.IID = phyMass.IID;
		this.CollisionQuadY = new float[4];
		this.simThread = simThread;
	}

	public virtual void Init(PhyReplay.ReplayParticleState state, int UID, PhyDebugTool tool)
	{
		this.replayState = state;
		this.CollisionQuadY = new float[4];
		this.OnUpdateReplay(state);
		this.UID = UID;
		base.name = "Mass_" + UID;
		this.tool = tool;
	}

	public virtual void OnUpdate()
	{
		if (this.phyMass != null)
		{
			base.transform.position = this.phyMass.Position;
			base.transform.rotation = this.phyMass.Rotation;
			this.Value = this.phyMass.MassValue;
			this.IsKinematic = this.phyMass.IsKinematic;
			this.ActualPosition = this.phyMass.Position4f.AsVector3();
			this.Velocity = this.phyMass.Velocity;
			this.Force = this.phyMass.Force;
			this.Motor = this.phyMass.Motor;
			this.WaterMotor = this.phyMass.WaterMotor;
			this.Buoyancy = this.phyMass.Buoyancy;
			this.WaterDragConstant = this.phyMass.WaterDragConstant;
			this.BuoyancySpeedMultiplier = this.phyMass.BuoyancySpeedMultiplier;
			this.GroundHeight = this.phyMass.GroundHeight;
			this.FlowVelocity = this.phyMass.FlowVelocity.AsVector3();
			this.WindVelocity = this.phyMass.WindVelocity.AsVector3();
			this.IgnoreEnvironment = this.phyMass.IgnoreEnvironment;
			this.IgnoreEnvForces = this.phyMass.IgnoreEnvForces;
			this.IsLying = this.phyMass.IsLying;
			this.IsTensioned = this.phyMass.IsTensioned;
			this.PriorSpring = this.tool.FindConnection(this.phyMass.PriorSpring);
			this.NextSpring = this.tool.FindConnection(this.phyMass.NextSpring);
			this.SpringsStr = ((this.phyMass.PriorSpring == null) ? "null" : this.phyMass.PriorSpring.UID.ToString()) + " <-> " + ((this.phyMass.NextSpring == null) ? "null" : this.phyMass.NextSpring.UID.ToString());
			this.Type = this.phyMass.Type;
			this.AirDragConstant = this.phyMass.AirDragConstant4f.X;
			this.VelocityLimit = this.phyMass.CurrentVelocityLimit;
			if (this.EnableMotionMonitor)
			{
				this.phyMass.EnableMotionMonitor(base.name + "_PhyDebugTool");
				this.EnableMotionMonitor = false;
			}
			if (this.TraceMass)
			{
			}
		}
		this.destroyedFlag = true;
	}

	public void AddKinematicExclusiveAction(bool flag)
	{
		SimulationThread.ParticleAction particleAction = new SimulationThread.ParticleAction(SimulationThread.ParticleActionType.IsKinematic, this.phyMass.UID, flag);
		this.simThread.ExclusiveActions.Add(particleAction);
		if (flag)
		{
			particleAction = new SimulationThread.ParticleAction(SimulationThread.ParticleActionType.StopMass, this.phyMass.UID, 0f);
			this.simThread.ExclusiveActions.Add(particleAction);
		}
	}

	public void OnUpdateReplay(PhyReplay.ReplayParticleState state)
	{
		this.replayState = state;
		base.transform.position = this.replayState.position + this.replayState.visualpositionoffset;
		this.Value = this.replayState.massvalue;
		this.IsKinematic = this.replayState.iskinematic;
		this.ActualPosition = this.replayState.position;
		this.Velocity = this.replayState.velocity;
		this.Force = this.replayState.force;
		this.GroundHeight = this.replayState.groundheight;
		this.CollisionQuadMinXZ = state.quadMinXZ;
		this.CollisionQuadMaxXZ = state.quadMaxXZ;
		this.CollisionQuadY[0] = state.quadY.x;
		this.CollisionQuadY[1] = state.quadY.y;
		this.CollisionQuadY[2] = state.quadY.z;
		this.CollisionQuadY[3] = state.quadY.w;
	}

	private void Start()
	{
	}

	private void Update()
	{
		this.OnUpdate();
	}

	public virtual void PhyMonitorIteration(int iteration)
	{
		this.FrameTrajectory[iteration] = new PhyDebugMass.TrajectorySample
		{
			position = this.phyMass.Position,
			velocity = this.phyMass.Velocity,
			force = this.phyMass.Force,
			watermotor = this.phyMass.WaterMotor
		};
	}

	public virtual void PhyMonitorPrepareFrame(int numberOfIterations)
	{
		this.FrameTrajectory = new PhyDebugMass.TrajectorySample[numberOfIterations];
	}

	public virtual void DrawGizmos()
	{
	}

	private static string Vector3ToString(Vector3 v)
	{
		return string.Concat(new object[] { "| (", v.x, "  ", v.y, "  ", v.z, ") | = ", v.magnitude });
	}

	public virtual string[] IterationString()
	{
		if (this.tool != null && this.FrameTrajectory != null && this.FrameTrajectory.Length > 1)
		{
			return new string[]
			{
				"   Iteration " + this.tool.trajectoryIteration,
				"   Position = " + PhyDebugMass.Vector3ToString(this.FrameTrajectory[this.tool.trajectoryIteration].position),
				"   Velocity = " + PhyDebugMass.Vector3ToString(this.FrameTrajectory[this.tool.trajectoryIteration].velocity),
				"   Force = " + PhyDebugMass.Vector3ToString(this.FrameTrajectory[this.tool.trajectoryIteration].force),
				"   WaterMotor = " + PhyDebugMass.Vector3ToString(this.FrameTrajectory[this.tool.trajectoryIteration].watermotor)
			};
		}
		return new string[] { "---" };
	}

	public Mass phyMass;

	public PhyReplay.ReplayParticleState replayState;

	public int UID;

	public int IID;

	public float Value;

	public bool IsKinematic;

	public Vector3 ActualPosition;

	public Vector3 Velocity;

	public Vector3 Force;

	public Vector3 Motor;

	public Vector3 WaterMotor;

	public float Buoyancy;

	public float WaterDragConstant;

	public float BuoyancySpeedMultiplier;

	public float GroundHeight;

	public float GravityOverride;

	public float AirDragConstant;

	public Vector3 FlowVelocity;

	public Vector3 WindVelocity;

	public bool IgnoreEnvironment;

	public bool IgnoreEnvForces;

	public bool IsLying;

	public bool IsTensioned;

	public PhyDebugConnection PriorSpring;

	public PhyDebugConnection NextSpring;

	public int PriorSpringUID;

	public int NextSpringUID;

	public string SpringsStr;

	public Mass.MassType Type;

	public float VelocityLimit;

	public Vector2 CollisionQuadMinXZ;

	public Vector2 CollisionQuadMaxXZ;

	public float[] CollisionQuadY;

	public bool StoreFrameTrajectory;

	public bool DrawFrameTrajectory;

	public PhyDebugMass.TrajectorySample[] FrameTrajectory;

	public bool EnableMotionMonitor;

	public bool TraceMass;

	public Color[] TraceColors;

	public bool DrawTrace;

	public PhyDebugTool tool;

	public bool destroyedFlag;

	private SimulationThread simThread;

	public struct TrajectorySample
	{
		public Vector3 position;

		public Vector3 velocity;

		public Vector3 force;

		public Vector3 watermotor;
	}
}
