using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Mono.Simd;
using Mono.Simd.Math;
using Phy.Verlet;
using UnityEngine;

namespace Phy
{
	public class SimulationThread : IPhyActionsListener, IPhyProfiler
	{
		public SimulationThread(string name, ConnectedBodiesSystem source, ConnectedBodiesSystem threadSim)
		{
			this.Name = name;
			this.sim = threadSim;
			this.sim.RefreshObjectArrays(true);
			this.sim.StartHelpers();
			this.sourceSim = source;
			this.Actions = new SimulationThread.ParticleAction[1024];
			this.ExclusiveActions = new List<SimulationThread.ParticleAction>();
			this.ActionsCount = 0;
			this.Snapshot = null;
			this.ConnectionsNeedSync = new HashSet<int>();
			this.ObjectsNeedSync = new HashSet<int>();
			this.SyncLock = new object();
			this.thread = new Thread(new ThreadStart(this.ThreadProc))
			{
				IsBackground = true,
				Name = name + "SimThread"
			};
			this.MainThreadCycle = SimulationThread.ThreadCycleState.Busy;
			this.SimThreadCycle = SimulationThread.ThreadCycleState.Busy;
			this.PreviousSyncTimeStamp = Time.frameCount;
		}

		public object SyncLock { get; private set; }

		public string Name { get; private set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnThreadException;

		public ConnectedBodiesSystem InternalSim
		{
			get
			{
				return this.sim;
			}
		}

		public DebugPlotter ThreadProfiler
		{
			get
			{
				return this.phyProfiler;
			}
		}

		public void StartSegment(string name)
		{
		}

		public void StopSegment(string name)
		{
		}

		public void SaveSegments()
		{
		}

		public void Start()
		{
			if (this.thread.IsAlive)
			{
				return;
			}
			this.ActionsCount = 0;
			Debug.LogWarning("Sim thread '" + this.Name + "' is starting");
			this.thread.Start();
		}

		public void Restart()
		{
			this.thread.Abort();
			this.thread = new Thread(new ThreadStart(this.ThreadProc))
			{
				IsBackground = true,
				Name = this.Name + "SimThread"
			};
			this.MainThreadCycle = SimulationThread.ThreadCycleState.Busy;
			this.SimThreadCycle = SimulationThread.ThreadCycleState.Busy;
			this.PreviousSyncTimeStamp = Time.frameCount;
			Debug.LogWarning("Sim thread '" + this.Name + "' is restarting");
			this.thread.Start();
		}

		public void Reset()
		{
			object syncLock = this.SyncLock;
			lock (syncLock)
			{
				lock (this)
				{
					this.ActionsCount = 0;
					this.ConnectionsNeedSync.Clear();
					this.ObjectsNeedSync.Clear();
					this.ExclusiveActions.Clear();
				}
			}
		}

		private void ThreadProc()
		{
			Debug.LogWarning("Sim thread '" + this.Name + "' has started");
			while (!this.abortFlag)
			{
				try
				{
					lock (this)
					{
						if (this.MainThreadCycle == SimulationThread.ThreadCycleState.Waiting)
						{
							Monitor.Pulse(this);
						}
						this.SimThreadCycle = SimulationThread.ThreadCycleState.Waiting;
						Monitor.Wait(this);
						this.SimThreadCycle = SimulationThread.ThreadCycleState.Busy;
						this.cachedDeltaTime = Mathf.Clamp(Mathf.Min(this.DeltaTime, 0.0004f * (float)(this.MaxIterationsPerFrame - 1)), this.cachedDeltaTime - 0.0011999999f, this.cachedDeltaTime + 0.0011999999f);
						this.sourceSim.FrameDeltaTime = this.cachedDeltaTime;
					}
					FishingRodSimulation fishingRodSimulation = this.sim as FishingRodSimulation;
					if (fishingRodSimulation != null)
					{
						fishingRodSimulation.TackleMoveCompensationMode = (this.sourceSim as FishingRodSimulation).TackleMoveCompensationMode;
						fishingRodSimulation.ApplyMoveAndRotateToRod(this.rodRootPosition, this.rodRootRotation);
						fishingRodSimulation.Sync(this.sourceSim as FishingRodSimulation);
					}
					for (int i = 0; i < this.sim.Objects.Count; i++)
					{
						this.sim.Objects[i].BeforeSimulationUpdate();
					}
					this.sim.Update(this.cachedDeltaTime);
					this._updateProceduralBend();
					this._buildBezier5CatmullRomComposite();
				}
				catch (Exception ex)
				{
					PhotonConnectionFactory.Instance.PinError(string.Concat(new string[] { "Physics thread '", this.Name, "' exception: ", ex.Message, "\n", ex.Source }), ex.StackTrace);
					Debug.LogError(string.Concat(new string[] { "Sim thread '", this.Name, "' exception: ", ex.Message, "\n", ex.Source, "\n", ex.StackTrace }));
					this.Reset();
					this.StructureCleared();
					this.sim.ResetReplayRecorder();
					if (this.OnThreadException != null)
					{
						this.OnThreadException();
					}
				}
			}
			this.sim.AbortHelpers();
			Debug.LogWarning("Sim thread '" + this.Name + "' has finshed");
		}

		public void WaitForThreadSync()
		{
		}

		private void checkSyncLock(string sender)
		{
			bool flag = false;
			try
			{
				flag = Monitor.TryEnter(this.SyncLock);
				if (!flag)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"checkSyncLock ",
						sender,
						" : lock is taken ",
						this.InternalSim.IterationsCounter
					}));
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this.SyncLock);
				}
			}
		}

		private string strucstr()
		{
			return string.Format("\nMasses: source = {0}, sim = {1}, created = {2}, destroyed = {3},\nConnections: source = {4}, sim = {5}, created = {6}, destroyed = {7}", new object[]
			{
				this.sourceSim.Masses.Count,
				this.sim.Masses.Count,
				this.createdMasses.Count,
				this.destroyedMasses.Count,
				this.sourceSim.Connections.Count,
				this.sim.Connections.Count,
				this.createdConnections.Count,
				this.destroyedConnections.Count
			});
		}

		public void StructureCleared()
		{
			this.syncClearAll = true;
			this.destroyedMasses.Clear();
			this.destroyedConnections.Clear();
			this.destroyedObjects.Clear();
			this.createdMasses.Clear();
			this.createdConnections.Clear();
			this.createdObjects.Clear();
		}

		public void MassCreated(Mass m)
		{
			if (m == null)
			{
				Debug.LogError("MassCreated m = null");
			}
			this.createdMasses.Add(m);
		}

		public void MassDestroyed(Mass m)
		{
			if (m == null)
			{
				Debug.LogError("MassDestroyed m = null");
			}
			this.destroyedMasses.Add(m);
		}

		public void ConnectionCreated(ConnectionBase c)
		{
			if (c == null)
			{
				Debug.LogError("ConnectionCreated c = null");
			}
			this.createdConnections.Add(c);
		}

		public void ConnectionDestroyed(ConnectionBase c)
		{
			this.destroyedConnections.Add(c);
			this.ConnectionsNeedSync.Remove(c.UID);
		}

		public void ObjectCreated(PhyObject b)
		{
			this.createdObjects.Add(b);
		}

		public void ObjectDestroyed(PhyObject b)
		{
			this.destroyedObjects.Add(b);
			this.ObjectsNeedSync.Remove(b.UID);
		}

		private void StructureSync()
		{
			this.newMasses.Clear();
			this.deletedMasses.Clear();
			foreach (Mass mass in this.createdMasses)
			{
				if (!this.destroyedMasses.Contains(mass))
				{
					Type type = mass.GetType();
					Mass mass2;
					if (type == typeof(VerletMass))
					{
						mass2 = new VerletMass(this.sim, mass as VerletMass);
					}
					else if (type == typeof(RigidBody))
					{
						mass2 = new RigidBody(this.sim, mass as RigidBody);
					}
					else if (type == typeof(Wobbler))
					{
						mass2 = new Wobbler(this.sim, mass as Wobbler);
					}
					else if (type == typeof(PointOnRigidBody))
					{
						mass2 = new PointOnRigidBody(this.sim, mass as PointOnRigidBody);
					}
					else if (type == typeof(TopWaterLure))
					{
						mass2 = new TopWaterLure(this.sim, mass as TopWaterLure);
					}
					else if (type == typeof(ConstraintPointOnRigidBody))
					{
						mass2 = new ConstraintPointOnRigidBody(this.sim, mass as ConstraintPointOnRigidBody);
					}
					else if (type == typeof(FloatingSimplexComposite))
					{
						mass2 = new FloatingSimplexComposite(this.sim, mass as FloatingSimplexComposite);
					}
					else if (type == typeof(RigidLure))
					{
						mass2 = new RigidLure(this.sim, mass as RigidLure);
					}
					else
					{
						mass2 = new Mass(this.sim, mass);
					}
					this.sim.Masses.Add(mass2);
					this.newMasses.Add(mass2);
					if (this.PrintActions)
					{
						Debug.Log(string.Format("created mass {0} : {1}", mass2.UID, mass2.Type));
					}
				}
			}
			foreach (Mass mass3 in this.destroyedMasses)
			{
				if (!this.createdMasses.Contains(mass3))
				{
					Mass copyMass = mass3.CopyMass;
					if (copyMass == null)
					{
						Debug.LogError(string.Format("StructureSync: CopyMass == null {0}.{1}", mass3.Type, mass3.UID));
					}
					else if (!this.sim.DictMasses.ContainsKey(copyMass.UID))
					{
						Debug.LogError(string.Format("StructureSync: cannot destroy mass {0}.{1} in {2}", mass3.Type, mass3.UID, this.Name));
					}
					else
					{
						this.sim.Masses.Remove(copyMass);
						this.deletedMasses.Add(copyMass);
						mass3.CopyMass = null;
						copyMass.SourceMass = null;
						if (this.PrintActions)
						{
							Debug.Log(string.Format("deleted mass {0} : {1}", copyMass.UID, copyMass.Type));
						}
					}
				}
			}
			bool flag = this.createdMasses.Count + this.destroyedMasses.Count > 0;
			if (flag)
			{
				this.sim.ModifyMassesDict(this.newMasses, this.deletedMasses);
			}
			this.createdMasses.Clear();
			this.destroyedMasses.Clear();
			this.newConnections.Clear();
			this.deletedConnections.Clear();
			foreach (ConnectionBase connectionBase in this.createdConnections)
			{
				if (!this.destroyedConnections.Contains(connectionBase))
				{
					ConnectionBase connectionBase2 = null;
					Type type2 = connectionBase.GetType();
					if (type2 == typeof(Spring))
					{
						connectionBase2 = new Spring(this.sim, connectionBase as Spring);
					}
					else if (type2 == typeof(HingeVerletSpring))
					{
						connectionBase2 = new HingeVerletSpring(this.sim, connectionBase as HingeVerletSpring);
					}
					else if (type2 == typeof(VerletSpring))
					{
						connectionBase2 = new VerletSpring(this.sim, connectionBase as VerletSpring);
					}
					else if (type2 == typeof(Bend))
					{
						connectionBase2 = new Bend(this.sim, connectionBase as Bend);
					}
					else if (type2 == typeof(Magnet))
					{
						connectionBase2 = new Magnet(this.sim, connectionBase as Magnet);
					}
					else if (type2 == typeof(VerletFishBendStrain))
					{
						connectionBase2 = new VerletFishBendStrain(this.sim, connectionBase as VerletFishBendStrain);
					}
					else if (type2 == typeof(VerletFishThrustController))
					{
						connectionBase2 = new VerletFishThrustController(this.sim, connectionBase as VerletFishThrustController);
					}
					else if (type2 == typeof(MassToRigidBodySpring))
					{
						connectionBase2 = new MassToRigidBodySpring(this.sim, connectionBase as MassToRigidBodySpring);
					}
					else if (type2 == typeof(TetrahedronRollStabilizer))
					{
						connectionBase2 = new TetrahedronRollStabilizer(this.sim, connectionBase as TetrahedronRollStabilizer);
					}
					else if (type2 == typeof(TetrahedronBallJoint))
					{
						connectionBase2 = new TetrahedronBallJoint(this.sim, connectionBase as TetrahedronBallJoint);
					}
					else if (type2 == typeof(KinematicSpline))
					{
						connectionBase2 = new KinematicSpline(this.sim, connectionBase as KinematicSpline);
					}
					else if (type2 == typeof(KinematicVerticalParabola))
					{
						connectionBase2 = new KinematicVerticalParabola(this.sim, connectionBase as KinematicVerticalParabola);
					}
					else if (type2 == typeof(PointToRigidBodyResistanceTransmission))
					{
						connectionBase2 = new PointToRigidBodyResistanceTransmission(this.sim, connectionBase as PointToRigidBodyResistanceTransmission);
					}
					else if (type2 == typeof(MassToRigidBodyEulerSpring))
					{
						connectionBase2 = new MassToRigidBodyEulerSpring(this.sim, connectionBase as MassToRigidBodyEulerSpring);
					}
					else if (type2 == typeof(TetrahedronTorsionSpring))
					{
						connectionBase2 = new TetrahedronTorsionSpring(this.sim, connectionBase as TetrahedronTorsionSpring);
					}
					else if (type2 == typeof(EngineModel))
					{
						connectionBase2 = new EngineModel(this.sim, connectionBase as EngineModel);
					}
					this.sim.Connections.Add(connectionBase2);
					this.newConnections.Add(connectionBase2);
					if (this.PrintActions)
					{
						Debug.Log(string.Format("created connection {0} : {1}", connectionBase2.UID, connectionBase2.GetType()));
					}
				}
			}
			foreach (ConnectionBase connectionBase3 in this.destroyedConnections)
			{
				if (!this.createdConnections.Contains(connectionBase3))
				{
					ConnectionBase connectionBase4 = null;
					if (!this.sim.DictConnections.TryGetValue(connectionBase3.UID, out connectionBase4))
					{
						Debug.LogError(string.Format("StructureSync: cannot destroy connection {0}.{1} in {2}", connectionBase3.GetType(), connectionBase3.UID, this.Name));
					}
					else
					{
						this.sim.Connections.Remove(connectionBase4);
						this.deletedConnections.Add(connectionBase4);
						if (this.PrintActions)
						{
							Debug.Log(string.Format("deleted connection {0} : {1}", connectionBase4.UID, connectionBase4.GetType()));
						}
					}
				}
			}
			bool flag2 = this.createdConnections.Count + this.destroyedConnections.Count > 0;
			if (flag2)
			{
				this.sim.ModifyConnectionsDict(this.newConnections, this.deletedConnections);
			}
			this.createdConnections.Clear();
			this.destroyedConnections.Clear();
			this.newObjects.Clear();
			this.deletedObjects.Clear();
			foreach (PhyObject phyObject in this.createdObjects)
			{
				if (!this.destroyedObjects.Contains(phyObject))
				{
					Type type3 = phyObject.GetType();
					PhyObject phyObject2;
					if (type3 == typeof(SpringDrivenObject))
					{
						phyObject2 = new SpringDrivenObject(this.sim, phyObject as SpringDrivenObject);
					}
					else if (type3 == typeof(BasicLureTackleObject))
					{
						phyObject2 = new BasicLureTackleObject(this.sim, phyObject as BasicLureTackleObject);
					}
					else if (type3 == typeof(RigidBodyTackleObject))
					{
						phyObject2 = new RigidBodyTackleObject(this.sim, phyObject as RigidBodyTackleObject);
					}
					else if (type3 == typeof(WobblerTackleObject))
					{
						phyObject2 = new WobblerTackleObject(this.sim, phyObject as WobblerTackleObject);
					}
					else if (type3 == typeof(FeederTackleObject))
					{
						phyObject2 = new FeederTackleObject(this.sim, phyObject as FeederTackleObject);
					}
					else if (type3 == typeof(VerletObject))
					{
						phyObject2 = new VerletObject(this.sim, phyObject as VerletObject);
					}
					else if (type3 == typeof(VerletFishBody))
					{
						phyObject2 = new VerletFishBody(this.sim, phyObject as VerletFishBody);
					}
					else if (type3 == typeof(SimpleFishBody))
					{
						phyObject2 = new SimpleFishBody(this.sim, phyObject as SimpleFishBody);
					}
					else if (type3 == typeof(RodAndTackleOnPodObject))
					{
						phyObject2 = new RodAndTackleOnPodObject(this.sim, phyObject as RodAndTackleOnPodObject);
					}
					else if (type3 == typeof(RodObjectInHands))
					{
						phyObject2 = new RodObjectInHands(this.sim, phyObject as RodObjectInHands);
						if (this.sim is FishingRodSimulation)
						{
							((FishingRodSimulation)this.sim).rodObject = (RodObjectInHands)phyObject2;
						}
					}
					else if (type3 == typeof(UnderwaterItemObject))
					{
						phyObject2 = new UnderwaterItemObject(this.sim, phyObject as UnderwaterItemObject);
					}
					else if (type3 == typeof(PlantObject))
					{
						phyObject2 = new PlantObject(this.sim, phyObject as PlantObject);
					}
					else
					{
						phyObject2 = new PhyObject(this.sim, phyObject);
					}
					this.sim.Objects.Add(phyObject2);
					this.newObjects.Add(phyObject2);
				}
			}
			foreach (PhyObject phyObject3 in this.destroyedObjects)
			{
				if (!this.createdObjects.Contains(phyObject3))
				{
					PhyObject phyObject4 = null;
					if (!this.sim.DictObjects.TryGetValue(phyObject3.UID, out phyObject4))
					{
						Debug.LogError(string.Format("StructureSync: cannot destroy object {0}.{1} DictObjects = {2} in {3}", new object[]
						{
							phyObject3.GetType(),
							phyObject3.UID,
							SimulationThread.CollectionToString(this.sim.DictObjects.Values),
							this.Name
						}));
					}
					else
					{
						this.sim.Objects.Remove(phyObject4);
						this.deletedObjects.Add(phyObject4);
					}
				}
			}
			bool flag3 = this.createdObjects.Count + this.destroyedObjects.Count > 0;
			if (flag3)
			{
				this.sim.ModifyObjectsDict(this.newObjects, this.deletedObjects);
			}
			this.createdObjects.Clear();
			this.destroyedObjects.Clear();
			if (flag || flag2 || flag3)
			{
				this.sim.RefreshObjectArrays(false);
			}
			for (int i = 0; i < this.newMasses.Count; i++)
			{
				this.newMasses[i].UpdateReferences();
			}
			for (int j = 0; j < this.newConnections.Count; j++)
			{
				this.newConnections[j].UpdateReferences();
			}
			for (int k = 0; k < this.newObjects.Count; k++)
			{
				this.newObjects[k].UpdateReferences();
			}
		}

		public static string CollectionToString(IEnumerable<PhyObject> values)
		{
			if (values == null)
			{
				return "NULL";
			}
			StringBuilder stringBuilder = new StringBuilder(" [");
			foreach (PhyObject phyObject in values)
			{
				stringBuilder.Append(string.Format("{0}.{1}, ", phyObject.UID, phyObject.Type));
			}
			stringBuilder.Append(" ]");
			return stringBuilder.ToString();
		}

		private void DoStructureClear()
		{
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				this.sim.Masses[i].SourceMass = null;
				this.sim.Masses[i].CopyMass = null;
			}
			this.sim.Clear();
			this.syncClearAll = false;
		}

		private void SyncSim()
		{
			if (!this.DisableStructuralUpdate)
			{
				if (this.syncClearAll)
				{
					this.DoStructureClear();
				}
				this.StructureSync();
			}
			for (int i = 0; i < this.ActionsCount; i++)
			{
				SimulationThread.ParticleAction particleAction = this.Actions[i];
				if (particleAction.uid < 0)
				{
					switch (particleAction.type)
					{
					case SimulationThread.ParticleActionType.VelocityLimit:
						this.setVelocityLimit(particleAction.scalar);
						break;
					case SimulationThread.ParticleActionType.StopLine:
						this.doStopLine();
						break;
					case SimulationThread.ParticleActionType.UpdateLineParams:
						this.doUpdateLineParams(particleAction.data.x, particleAction.data.y);
						break;
					case SimulationThread.ParticleActionType.GlobalReset:
						this.doGlobalReset();
						break;
					case SimulationThread.ParticleActionType.VisualPositionOffsetGlobal:
						this.doVisualPositionOffsetGlobalChanged(particleAction.vector);
						break;
					}
				}
				if (this.sim.DictObjects.ContainsKey(particleAction.uid))
				{
					SimulationThread.ParticleActionType type = particleAction.type;
					if (type == SimulationThread.ParticleActionType.FishIsHooked)
					{
						(this.sim.DictObjects[particleAction.uid] as AbstractFishBody).HookState = (AbstractFishBody.HookStateEnum)particleAction.scalar;
					}
				}
				if (this.sim.DictMasses.ContainsKey(particleAction.uid))
				{
					switch (particleAction.type)
					{
					case SimulationThread.ParticleActionType.Position:
						this.sim.DictMasses[particleAction.uid].Position = particleAction.vector;
						break;
					case SimulationThread.ParticleActionType.LocalPosition:
						(this.sim.DictMasses[particleAction.uid] as PointOnRigidBody).LocalPosition = particleAction.vector;
						break;
					case SimulationThread.ParticleActionType.Rotation:
						this.sim.DictMasses[particleAction.uid].Rotation = particleAction.quaternion;
						break;
					case SimulationThread.ParticleActionType.VisualPositionOffset:
						this.sim.DictMasses[particleAction.uid].VisualPositionOffset = particleAction.vector;
						break;
					case SimulationThread.ParticleActionType.Mass:
						this.sim.DictMasses[particleAction.uid].MassValue = particleAction.scalar;
						break;
					case SimulationThread.ParticleActionType.Velocity:
						this.sim.DictMasses[particleAction.uid].Velocity = particleAction.vector;
						break;
					case SimulationThread.ParticleActionType.Force:
						this.sim.DictMasses[particleAction.uid].ApplyForce(particleAction.vector, false);
						break;
					case SimulationThread.ParticleActionType.Motor:
						this.sim.DictMasses[particleAction.uid].Motor = particleAction.vector;
						break;
					case SimulationThread.ParticleActionType.WaterMotor:
						this.sim.DictMasses[particleAction.uid].WaterMotor = particleAction.vector;
						break;
					case SimulationThread.ParticleActionType.IsKinematic:
						this.sim.DictMasses[particleAction.uid].IsKinematic = particleAction.flag;
						break;
					case SimulationThread.ParticleActionType.IsRef:
						this.sim.DictMasses[particleAction.uid].IsRef = particleAction.flag;
						break;
					case SimulationThread.ParticleActionType.StopMass:
						this.sim.DictMasses[particleAction.uid].StopMass();
						break;
					case SimulationThread.ParticleActionType.Reset:
						this.sim.DictMasses[particleAction.uid].Reset();
						break;
					case SimulationThread.ParticleActionType.KinematicTranslate:
						(this.sim.DictMasses[particleAction.uid] as VerletMass).KinematicTranslate(particleAction.vector.AsPhyVector(ref this.sim.DictMasses[particleAction.uid].Position4f));
						break;
					case SimulationThread.ParticleActionType.AirDragConstant:
						this.sim.DictMasses[particleAction.uid].AirDragConstant = particleAction.scalar;
						break;
					case SimulationThread.ParticleActionType.WaterDragConstant:
						this.sim.DictMasses[particleAction.uid].WaterDragConstant = particleAction.scalar;
						break;
					case SimulationThread.ParticleActionType.Buoyancy:
						this.sim.DictMasses[particleAction.uid].Buoyancy = particleAction.scalar;
						break;
					case SimulationThread.ParticleActionType.BuoyancySpeedMultiplier:
						this.sim.DictMasses[particleAction.uid].BuoyancySpeedMultiplier = particleAction.scalar;
						break;
					case SimulationThread.ParticleActionType.TopWaterStrikeSignal:
						(this.sim.DictMasses[particleAction.uid] as TopWaterLure).Strike = true;
						break;
					case SimulationThread.ParticleActionType.MassType:
						this.sim.DictMasses[particleAction.uid].Type = (Mass.MassType)Mathf.RoundToInt(particleAction.scalar);
						break;
					case SimulationThread.ParticleActionType.BoatInertia:
						(this.sim.DictMasses[particleAction.uid] as FloatingSimplexComposite).SyncBoatInertia(this.sourceSim.DictMasses[particleAction.uid] as FloatingSimplexComposite);
						break;
					case SimulationThread.ParticleActionType.DisableSimulation:
						this.sim.DictMasses[particleAction.uid].DisableSimulation = particleAction.scalar > 0f;
						break;
					case SimulationThread.ParticleActionType.CollisionType:
						this.sim.DictMasses[particleAction.uid].Collision = (Mass.CollisionType)Mathf.RoundToInt(particleAction.scalar);
						break;
					case SimulationThread.ParticleActionType.VelocityLimit:
						this.sim.DictMasses[particleAction.uid].CurrentVelocityLimit = particleAction.scalar;
						break;
					}
				}
				if (this.PrintActions)
				{
					Debug.Log(particleAction);
				}
			}
			if (this.PrintActions)
			{
				this.PrintActions = false;
				Debug.Break();
			}
			this.ActionsCount = 0;
			this.PreviousSyncTimeStamp = Time.frameCount;
			for (int j = 0; j < this.sim.Masses.Count; j++)
			{
				Mass mass = this.sim.Masses[j];
				if (mass.SourceMass != null)
				{
					Mass sourceMass = mass.SourceMass;
					mass.FlowVelocity = sourceMass.FlowVelocity;
					mass.WindVelocity = sourceMass.WindVelocity;
					mass.StaticFrictionFactor = sourceMass.StaticFrictionFactor;
					mass.SlidingFrictionFactor = sourceMass.SlidingFrictionFactor;
				}
				ConstraintPointOnRigidBody constraintPointOnRigidBody = mass as ConstraintPointOnRigidBody;
				if (constraintPointOnRigidBody != null && constraintPointOnRigidBody.SourceMass != null)
				{
					ConstraintPointOnRigidBody constraintPointOnRigidBody2 = (ConstraintPointOnRigidBody)constraintPointOnRigidBody.SourceMass;
					constraintPointOnRigidBody.SyncCollisionPlanes(constraintPointOnRigidBody2);
				}
				RigidBody rigidBody = mass as RigidBody;
				if (rigidBody != null && rigidBody.SourceMass != null)
				{
					RigidBody rigidBody2 = (RigidBody)rigidBody.SourceMass;
					rigidBody.SyncCollisionPlanes(rigidBody2);
					rigidBody.AxialWaterDragEnabled = rigidBody2.AxialWaterDragEnabled;
					rigidBody.AxialWaterDragFactors = rigidBody2.AxialWaterDragFactors;
				}
			}
			for (int k = 0; k < this.sim.Connections.Count; k++)
			{
				this.sim.Connections[k].SyncSimUpdate();
			}
			this.invalidateGroundHeightFlag = false;
			foreach (int num in this.ConnectionsNeedSync)
			{
				if (this.sim.DictConnections.ContainsKey(num) && this.sourceSim.DictConnections.ContainsKey(num))
				{
					this.sim.DictConnections[num].Sync(this.sourceSim.DictConnections[num]);
				}
			}
			this.ConnectionsNeedSync.Clear();
			foreach (int num2 in this.ObjectsNeedSync)
			{
				if (this.sim.DictObjects.ContainsKey(num2) && this.sourceSim.DictObjects.ContainsKey(num2))
				{
					this.sim.DictObjects[num2].Sync(this.sourceSim.DictObjects[num2]);
				}
			}
			this.ObjectsNeedSync.Clear();
		}

		private void writeSnapshot()
		{
			if (this.Snapshot == null || this.Snapshot.Length != this.sim.Masses.Count)
			{
				this.Snapshot = new SimulationThread.ParticleSnapshot[this.sim.Masses.Count];
			}
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				Mass mass = this.sim.Masses[i];
				this.Snapshot[i].uid = mass.UID;
				this.Snapshot[i].position = mass.Position;
				this.Snapshot[i].velocity = mass.Velocity;
			}
		}

		public void ApplyMoveAndRotateToRod(Vector3 rootPosition, Quaternion rootRotation)
		{
			this.rodRootPosition = rootPosition;
			this.rodRootRotation = rootRotation;
		}

		public void UpdateProceduralBend(RodBehaviour sender, int rodObjectUID, RodBehaviour.LocatorRing[] rings, Vector3[] FirstRingLocatorsUnbentPositions)
		{
			this.proceduralBendRodObjectUID = rodObjectUID;
			if (this.proceduralBendLocatorRings == null || this.proceduralBendLocatorRings.Length != rings.Length)
			{
				this.proceduralBendLocatorRings = new RodBehaviour.LocatorRing[rings.Length];
				for (int i = 0; i < rings.Length; i++)
				{
					this.proceduralBendLocatorRings[i] = new RodBehaviour.LocatorRing(rings[i]);
				}
			}
			this.proceduralBendFirstRingLocatorsUnbentPositions = FirstRingLocatorsUnbentPositions;
			if (this.proceduralBendlocatorsPos == null || this.proceduralBendlocatorsPos.Length != rings.Length)
			{
				this.proceduralBendlocatorsPos = new Vector3[rings.Length];
			}
			if (this.proceduralBendfirstRingPos == null || this.proceduralBendfirstRingPos.Length != FirstRingLocatorsUnbentPositions.Length)
			{
				this.proceduralBendfirstRingPos = new Vector3[FirstRingLocatorsUnbentPositions.Length];
			}
		}

		private void _updateProceduralBend()
		{
			PhyObject phyObject;
			if (this.sim.DictObjects.TryGetValue(this.proceduralBendRodObjectUID, out phyObject))
			{
				RodObjectInHands rodObjectInHands = phyObject as RodObjectInHands;
				int pointsCount = rodObjectInHands.PointsCount;
				if (this.proceduralBendrodPoints == null || this.proceduralBendrodPoints.Length != pointsCount)
				{
					this.proceduralBendrodPoints = new Vector3[pointsCount];
				}
				for (int i = 0; i < this.proceduralBendrodPoints.Length; i++)
				{
					this.proceduralBendrodPoints[i] = Quaternion.Inverse(this.rodRootRotation) * (rodObjectInHands.Masses[i].Position - this.rodRootPosition);
				}
				BezierCurve bezierCurve = new BezierCurve(pointsCount - 1);
				for (int j = 0; j < pointsCount; j++)
				{
					if (j < 3)
					{
						bezierCurve.AnchorPoints[j] = new Vector3(0f, 0f, this.proceduralBendrodPoints[j].z);
					}
					else
					{
						bezierCurve.AnchorPoints[j] = this.proceduralBendrodPoints[j];
					}
				}
				for (int k = 0; k < this.proceduralBendLocatorRings.Length; k++)
				{
					bezierCurve.SetT(this.proceduralBendLocatorRings[k].tparam);
					this.proceduralBendlocatorsPos[k] = bezierCurve.CurvedCylinderTransform(this.proceduralBendLocatorRings[k].locatorUnbentPosition);
				}
				bezierCurve.SetT(this.proceduralBendLocatorRings[0].tparam);
				for (int l = 0; l < this.proceduralBendFirstRingLocatorsUnbentPositions.Length; l++)
				{
					this.proceduralBendfirstRingPos[l] = bezierCurve.CurvedCylinderTransform(this.proceduralBendFirstRingLocatorsUnbentPositions[l]);
				}
			}
		}

		public void ResetBezier5CatmullRomComposite()
		{
			this.splineRodTipMassUID = -1;
			this.splineTackleObjectUID = -1;
		}

		public int BuildBezier5CatmullRomComposite(int rodTipMassUID, int tackleObjectUID, float noiseAmp, float alpha, ref Vector3[] spline, int bezierPoints, int catmullPointsPerSegment, Vector3 lineLocatorCorrection)
		{
			this.splineRodTipMassUID = rodTipMassUID;
			this.splineTackleObjectUID = tackleObjectUID;
			this.splineNoiseAmp = noiseAmp;
			this.splineAlpha = alpha;
			this.splineBezierPoints = bezierPoints;
			this.splineCatmullPointsPerSegment = catmullPointsPerSegment;
			if (this.splinePointsCount > 0)
			{
				spline = this.splineCacheA;
			}
			this.lineLocatorCorrection = lineLocatorCorrection;
			return this.splinePointsCount;
		}

		private void _syncBezier5CatmullRomComposite()
		{
			Vector3[] array = this.splineCacheB;
			this.splineCacheB = this.splineCacheA;
			this.splineCacheA = array;
		}

		private void _buildBezier5CatmullRomComposite()
		{
			if (this.splineRodTipMassUID < 0 || this.splineTackleObjectUID < 0)
			{
				return;
			}
			Mass mass;
			PhyObject phyObject;
			if (this.sim.DictMasses.TryGetValue(this.splineRodTipMassUID, out mass) && this.sim.DictObjects.TryGetValue(this.splineTackleObjectUID, out phyObject))
			{
				int num = 1;
				Vector3 zero = Vector3.zero;
				List<Vector3> list = new List<Vector3>();
				list.Add(mass.Position + this.lineLocatorCorrection);
				int i = 0;
				Mass mass2 = null;
				Mass mass3 = mass;
				while (mass3.NextSpring != null)
				{
					if (mass3.Type == Mass.MassType.Leader || mass3.Type == Mass.MassType.Leash || mass3.Type == Mass.MassType.Swivel || mass3.Type == Mass.MassType.Wobbler)
					{
						break;
					}
					if (i >= num && (mass2.Position - mass3.Position).sqrMagnitude > 0.0001f)
					{
						list.Add(mass3.Position);
					}
					mass2 = mass3;
					i++;
					if (mass3.Type == Mass.MassType.Bobber || mass3.Type == Mass.MassType.Lure || mass3.Type == Mass.MassType.Hook || mass3.Type == Mass.MassType.Feeder || mass3.IsKinematic)
					{
						break;
					}
					mass3 = mass3.NextSpring.Mass2;
				}
				float num2 = 1f / (float)(list.Count - 1);
				for (i = 1; i < list.Count; i++)
				{
					List<Vector3> list2;
					int num3;
					(list2 = list)[num3 = i] = list2[num3] + this.lineLocatorCorrection * (1f - (float)i * num2);
				}
				this.splinePointsCount = SplineBuilder.BuildBezier5CatmullRomComposite(list, this.splineNoiseAmp, this.splineAlpha, this.splineCacheB, this.splineBezierPoints, this.splineCatmullPointsPerSegment);
			}
		}

		public bool AddAction(int UID, SimulationThread.ParticleActionType type)
		{
			this.WaitForThreadSync();
			if (this.ActionsCount < this.Actions.Length)
			{
				this.actionsBufferOverflow = false;
				this.Actions[this.ActionsCount].type = type;
				this.Actions[this.ActionsCount].uid = UID;
				this.ActionsCount++;
				return true;
			}
			if (!this.actionsBufferOverflow)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"SimulationThread.Actions: actions per frame limit (",
					this.Actions.Length,
					") exceeded: ",
					type,
					" uid: ",
					UID
				}));
				this.actionsBufferOverflow = true;
			}
			return false;
		}

		public void MassChanged(int UID, float newMass)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Mass))
			{
				this.Actions[this.ActionsCount - 1].scalar = newMass;
			}
		}

		public void PositionChanged(int UID, Vector3 newPosition)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Position))
			{
				this.Actions[this.ActionsCount - 1].vector = newPosition;
			}
		}

		public void LocalPositionChanged(int UID, Vector3 newLocalPosition)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.LocalPosition))
			{
				this.Actions[this.ActionsCount - 1].vector = newLocalPosition;
			}
		}

		public void RotationChanged(int UID, Quaternion newRotation)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Rotation))
			{
				this.Actions[this.ActionsCount - 1].quaternion = newRotation;
			}
		}

		public void VelocityChanged(int UID, Vector3 newVelocity)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Velocity))
			{
				this.Actions[this.ActionsCount - 1].vector = newVelocity;
			}
		}

		public void ForceApplied(int UID, Vector3 force)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Force))
			{
				this.Actions[this.ActionsCount - 1].vector = force;
			}
		}

		public void WaterMotorChanged(int UID, Vector3 watermotor)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.WaterMotor))
			{
				this.Actions[this.ActionsCount - 1].vector = watermotor;
			}
		}

		public void MotorChanged(int UID, Vector3 motor)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Motor))
			{
				this.Actions[this.ActionsCount - 1].vector = motor;
			}
		}

		public void IsKinematicChanged(int UID, bool isKinematic)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.IsKinematic))
			{
				this.Actions[this.ActionsCount - 1].flag = isKinematic;
			}
		}

		public void IsRefChanged(int UID, bool isRef)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.IsRef))
			{
				this.Actions[this.ActionsCount - 1].flag = isRef;
			}
		}

		public void VisualPositionOffsetChanged(int UID, Vector3 visualPositionOffset)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.VisualPositionOffset))
			{
				this.Actions[this.ActionsCount - 1].vector = visualPositionOffset;
			}
		}

		public void StopMassCalled(int UID)
		{
			this.AddAction(UID, SimulationThread.ParticleActionType.StopMass);
		}

		public void ResetCalled(int UID)
		{
			this.AddAction(UID, SimulationThread.ParticleActionType.Reset);
		}

		public void KinematicTranslateCalled(int UID, Vector3 translate)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.KinematicTranslate))
			{
				this.Actions[this.ActionsCount - 1].vector = translate;
			}
		}

		public void AirDragConstantChanged(int UID, float airDrag)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.AirDragConstant))
			{
				this.Actions[this.ActionsCount - 1].scalar = airDrag;
			}
		}

		public void WaterDragConstantChanged(int UID, float waterDrag)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.WaterDragConstant))
			{
				this.Actions[this.ActionsCount - 1].scalar = waterDrag;
			}
		}

		public void TopWaterStrikeSignal(int UID)
		{
			this.AddAction(UID, SimulationThread.ParticleActionType.TopWaterStrikeSignal);
		}

		public void ConnectionNeedSyncMark(int UID)
		{
			this.WaitForThreadSync();
			this.ConnectionsNeedSync.Add(UID);
		}

		public void ObjectNeedSyncMark(int UID)
		{
			this.WaitForThreadSync();
			this.ObjectsNeedSync.Add(UID);
		}

		public void FishIsHookedChanged(int UID, int hookState)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.FishIsHooked))
			{
				this.Actions[this.ActionsCount - 1].scalar = (float)hookState;
			}
		}

		public void GravityOverrideChanged(int UID, float gravity)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.GravityOverride))
			{
				this.Actions[this.ActionsCount - 1].scalar = gravity;
			}
		}

		public void BuoyancyChanged(int UID, float buoyancy)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.Buoyancy))
			{
				this.Actions[this.ActionsCount - 1].scalar = buoyancy;
			}
		}

		public void BuoyancySpeedMultiplierChanged(int UID, float mult)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.BuoyancySpeedMultiplier))
			{
				this.Actions[this.ActionsCount - 1].scalar = mult;
			}
		}

		public void VelocityLimitChanged(int UID, float limit)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.VelocityLimit))
			{
				this.Actions[this.ActionsCount - 1].scalar = limit;
			}
		}

		public void MassTypeChanged(int UID, Mass.MassType newMassType)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.MassType))
			{
				this.Actions[this.ActionsCount - 1].scalar = (float)newMassType;
			}
		}

		public void CollisionTypeChanged(int UID, Mass.CollisionType newCollisionType)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.CollisionType))
			{
				this.Actions[this.ActionsCount - 1].scalar = (float)newCollisionType;
			}
		}

		public void DisableSimualtionChanged(int UID, bool disabled)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.DisableSimulation))
			{
				this.Actions[this.ActionsCount - 1].scalar = ((!disabled) ? 0f : 1f);
			}
		}

		public void BoatInertiaChanged(int UID)
		{
			this.AddAction(UID, SimulationThread.ParticleActionType.BoatInertia);
		}

		public void VelocityLimitChanged(float limit)
		{
			if (this.AddAction(-1, SimulationThread.ParticleActionType.VelocityLimit))
			{
				this.Actions[this.ActionsCount - 1].scalar = limit;
			}
		}

		public void HorizontalVelocityLimitChanged(int UID, float limit)
		{
			if (this.AddAction(UID, SimulationThread.ParticleActionType.HorizontalVelocityLimit))
			{
				this.Actions[this.ActionsCount - 1].scalar = limit;
			}
		}

		private void setVelocityLimit(float limit)
		{
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				this.sim.Masses[i].CurrentVelocityLimit = limit;
			}
		}

		public void StopLine()
		{
			this.AddAction(-1, SimulationThread.ParticleActionType.StopLine);
		}

		private void doStopLine()
		{
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				Mass mass = this.sim.Masses[i];
				if (mass.Type == Mass.MassType.Line && mass.NextSpring != null)
				{
					Mass mass2 = mass.NextSpring.Mass2;
					Vector4f vector4f = (mass2.Position4f - mass.Position4f).Normalized();
					mass.Velocity4f *= new Vector4f(1f - Math.Abs(Vector4fExtensions.Dot(vector4f, mass.Velocity4f.Normalized())));
				}
			}
		}

		public void UpdateLineParams(float lineMass, float lineSpringConstant)
		{
			if (this.AddAction(-1, SimulationThread.ParticleActionType.UpdateLineParams))
			{
				this.Actions[this.ActionsCount - 1].data = new Vector4(lineMass, lineSpringConstant, 0f, 0f);
			}
		}

		private void doUpdateLineParams(float lineMass, float lineSpringConstant)
		{
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				Mass mass = this.sim.Masses[i];
				if (mass.Type == Mass.MassType.Line || mass.Type == Mass.MassType.Leader)
				{
					mass.MassValue = lineMass;
					if (mass.NextSpring != null)
					{
						mass.NextSpring.SpringConstant = lineSpringConstant;
					}
				}
			}
		}

		public void GlobalReset()
		{
			this.AddAction(-1, SimulationThread.ParticleActionType.GlobalReset);
		}

		public void doGlobalReset()
		{
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				Mass mass = this.sim.Masses[i];
				mass.StopMass();
			}
			this.invalidateGroundHeightFlag = true;
		}

		public void VisualPositionOffsetGlobalChanged(Vector3 vpo)
		{
			this.sim.VisualPositionOffset = vpo;
			if (this.AddAction(-1, SimulationThread.ParticleActionType.VisualPositionOffsetGlobal))
			{
				this.Actions[this.ActionsCount - 1].vector = vpo;
			}
		}

		private void doVisualPositionOffsetGlobalChanged(Vector3 vpo)
		{
			this.sim.VisualPositionOffset = vpo;
		}

		public void EnableMassTracer(int traceScanPeriod, int[] tracedMasses)
		{
			this.InternalSim.EnableMassTracer(traceScanPeriod, tracedMasses);
			this.sourceSim.EnableMassTracer(traceScanPeriod, tracedMasses);
		}

		public void SyncMain()
		{
			lock (this)
			{
				this.sourceSim.RefreshObjectArrays(true);
				if (this.SimThreadCycle == SimulationThread.ThreadCycleState.Busy)
				{
					this.MainThreadCycle = SimulationThread.ThreadCycleState.Waiting;
					if (!Monitor.Wait(this, 3000) || this.DebugTrigger)
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Sim thread timeout: main = ",
							this.MainThreadCycle,
							" sim = ",
							this.SimThreadCycle,
							" thread.IsAlive = ",
							this.thread.IsAlive
						}));
						this.DebugTrigger = false;
						this.Restart();
					}
				}
				this.MainThreadCycle = SimulationThread.ThreadCycleState.Busy;
				this.DeltaTime = Time.deltaTime;
				this.sourceSim.IterationsCounter = this.sim.IterationsCounter;
				for (int i = 0; i < this.sim.Masses.Count; i++)
				{
					Mass mass = this.sim.Masses[i];
					if (this.sourceSim.DictMasses.ContainsKey(mass.UID))
					{
						this.sourceSim.DictMasses[mass.UID].SyncMain(mass);
						mass.ResetAvgForce();
						mass.IsLimitBreached = false;
					}
				}
				for (int j = 0; j < this.sim.Objects.Count; j++)
				{
					PhyObject phyObject = this.sim.Objects[j];
					PhyObject phyObject2;
					if (this.sourceSim.DictObjects.TryGetValue(phyObject.UID, out phyObject2))
					{
						phyObject2.SyncMain(phyObject);
					}
				}
				this._syncBezier5CatmullRomComposite();
				this.sourceSim.SyncMassTracer(this.sim);
				if (!this.DisableSyncSim)
				{
					this.SyncSim();
				}
				Monitor.Pulse(this);
				this.MainThreadCycle = SimulationThread.ThreadCycleState.Busy;
				if (this.PrintPhySnapshotLog)
				{
					Debug.LogWarning("Snapshot MAIN:::");
					this.sourceSim.DebugSnapshotLog();
					Debug.LogWarning("Snapshot SIM:::");
					this.sim.DebugSnapshotLog();
					Debug.Break();
					this.PrintPhySnapshotLog = false;
				}
				if (this.DetectRefLeaks)
				{
					Debug.LogWarning("RefLeaks MAIN::");
					this.sourceSim.DetectRefLeaks();
					Debug.LogWarning("RefLeaks SIM::");
					this.sim.DetectRefLeaks();
					this.DetectRefLeaks = false;
				}
			}
		}

		public void ForceStop()
		{
			Debug.LogWarning(string.Format("Signalling sim thread {0} to stop", this.Name));
			this.InternalSim.SaveReplayRecorder();
			lock (this)
			{
				this.abortFlag = true;
				if (this.SimThreadCycle == SimulationThread.ThreadCycleState.Waiting)
				{
					Monitor.Pulse(this);
				}
			}
		}

		~SimulationThread()
		{
			this.ForceStop();
		}

		public const int WaitTimeout = -1;

		public const int DefaultMaxIterationsPerFrame = 150;

		public const int MaxIterationsChangePerFrame = 3;

		public const float MaxDeltaTimeChangePerFrame = 0.0011999999f;

		public bool DebugTrigger;

		public bool DisableSyncSim;

		public bool PrintActions;

		public bool PrintPhySnapshotLog;

		public bool DisableStructuralUpdate;

		public bool DetectRefLeaks;

		public volatile SimulationThread.ThreadCycleState MainThreadCycle;

		public volatile SimulationThread.ThreadCycleState SimThreadCycle;

		private volatile int PreviousSyncTimeStamp;

		public volatile float DeltaTime;

		private SimulationThread.ParticleAction[] Actions;

		public List<SimulationThread.ParticleAction> ExclusiveActions;

		private int ActionsCount;

		private HashSet<int> ConnectionsNeedSync;

		private HashSet<int> ObjectsNeedSync;

		public SimulationThread.ParticleSnapshot[] Snapshot;

		public int MaxIterationsPerFrame = 150;

		private Thread thread;

		private ConnectedBodiesSystem sourceSim;

		private ConnectedBodiesSystem sim;

		private volatile bool abortFlag;

		private float cachedDeltaTime;

		private Vector3 cachedLinePositionInLeftHand;

		private bool cachedFishControllerIsHooked;

		private Mass kinematicLineMass;

		private bool invalidateGroundHeightFlag;

		private DebugPlotter phyProfiler;

		private bool syncClearAll;

		private List<Mass> newMasses = new List<Mass>();

		private List<Mass> deletedMasses = new List<Mass>();

		private List<ConnectionBase> newConnections = new List<ConnectionBase>();

		private List<ConnectionBase> deletedConnections = new List<ConnectionBase>();

		private List<PhyObject> newObjects = new List<PhyObject>();

		private List<PhyObject> deletedObjects = new List<PhyObject>();

		private HashSet<Mass> createdMasses = new HashSet<Mass>();

		private HashSet<Mass> destroyedMasses = new HashSet<Mass>();

		private HashSet<ConnectionBase> createdConnections = new HashSet<ConnectionBase>();

		private HashSet<ConnectionBase> destroyedConnections = new HashSet<ConnectionBase>();

		private HashSet<PhyObject> createdObjects = new HashSet<PhyObject>();

		private HashSet<PhyObject> destroyedObjects = new HashSet<PhyObject>();

		private Vector3 rodRootPosition;

		private Quaternion rodRootRotation;

		private int proceduralBendRodObjectUID = -1;

		private RodBehaviour.LocatorRing[] proceduralBendLocatorRings;

		private Vector3[] proceduralBendFirstRingLocatorsUnbentPositions;

		private Vector3[] proceduralBendlocatorsPos;

		private Vector3[] proceduralBendfirstRingPos;

		private Vector3[] proceduralBendrodPoints;

		private int splineRodTipMassUID = -1;

		private int splineTackleObjectUID = -1;

		private float splineNoiseAmp;

		private float splineAlpha;

		private int splineBezierPoints;

		private int splineCatmullPointsPerSegment;

		private Vector3[] splineCacheA = new Vector3[1000];

		private Vector3[] splineCacheB = new Vector3[1000];

		private int splinePointsCount;

		private Vector3 lineLocatorCorrection;

		private bool actionsBufferOverflow;

		public enum ThreadCycleState
		{
			Busy,
			Syncing,
			Waiting
		}

		public enum ParticleActionType
		{
			Position,
			LocalPosition,
			Rotation,
			VisualPositionOffset,
			Mass,
			Velocity,
			Force,
			Motor,
			WaterMotor,
			IsKinematic,
			IsRef,
			StopMass,
			Reset,
			KinematicTranslate,
			AirDragConstant,
			WaterDragConstant,
			FishIsHooked,
			GravityOverride,
			Buoyancy,
			BuoyancySpeedMultiplier,
			HorizontalVelocityLimit,
			TopWaterStrikeSignal,
			MassType,
			BoatInertia,
			DisableSimulation,
			CollisionType,
			VelocityLimit,
			StopLine,
			UpdateLineParams,
			GlobalReset,
			VisualPositionOffsetGlobal
		}

		public struct ParticleAction
		{
			public ParticleAction(SimulationThread.ParticleActionType type, int uid, float data)
			{
				this.type = type;
				this.uid = uid;
				this.data = new Vector4(data, 0f, 0f, 0f);
			}

			public ParticleAction(SimulationThread.ParticleActionType type, int uid, Vector3 data)
			{
				this.type = type;
				this.uid = uid;
				this.data = data;
			}

			public ParticleAction(SimulationThread.ParticleActionType type, int uid, Quaternion data)
			{
				this.type = type;
				this.uid = uid;
				this.data = new Vector4(data.x, data.y, data.z, data.w);
			}

			public ParticleAction(SimulationThread.ParticleActionType type, int uid, bool data)
			{
				this.type = type;
				this.uid = uid;
				this.data = new Vector4((!data) ? 0f : 1f, 0f, 0f, 0f);
			}

			public float scalar
			{
				get
				{
					return this.data.x;
				}
				set
				{
					this.data = new Vector4(value, 0f, 0f, 0f);
				}
			}

			public Vector3 vector
			{
				get
				{
					return new Vector3(this.data.x, this.data.y, this.data.z);
				}
				set
				{
					this.data = value;
				}
			}

			public Quaternion quaternion
			{
				get
				{
					return new Quaternion(this.data.x, this.data.y, this.data.z, this.data.w);
				}
				set
				{
					this.data = new Vector4(value.x, value.y, value.z, value.w);
				}
			}

			public bool flag
			{
				get
				{
					return this.data.x > 0f;
				}
				set
				{
					this.data = new Vector4((!value) ? 0f : 1f, 0f, 0f, 0f);
				}
			}

			public override string ToString()
			{
				return string.Format("[{0}] {1}: data = ({2} {3} {4} {5})", new object[]
				{
					this.uid,
					this.type,
					this.data.x,
					this.data.y,
					this.data.z,
					this.data.w
				});
			}

			public SimulationThread.ParticleActionType type;

			public int uid;

			public Vector4 data;
		}

		public struct ParticleSnapshot
		{
			public int uid;

			public Vector3 position;

			public Vector3 velocity;
		}

		public class ProfilingSegment
		{
			public ProfilingSegment(string name)
			{
				this.name = name;
				this.ticksHist = new int[10000];
				this.ticksGraph = new long[100000];
				this.stopwatch = new Stopwatch();
			}

			public const int TicksMax = 100000;

			public const int TicksResolution = 10;

			public const int CallsAverage = 50;

			public string name;

			public Stopwatch stopwatch;

			public DebugPlotter.Value ticksHistPlot;

			public DebugPlotter.Value ticksGraphPlot;

			public int calls;

			public long totalTicks;

			public int[] ticksHist;

			public long[] ticksGraph;

			public int graphPos;
		}
	}
}
