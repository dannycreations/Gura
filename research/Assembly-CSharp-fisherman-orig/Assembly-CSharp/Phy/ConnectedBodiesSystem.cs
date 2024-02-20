using System;
using System.Collections.Generic;
using System.Text;
using Mono.Simd;
using Mono.Simd.Math;
using Phy.Verlet;
using UnityEngine;

namespace Phy
{
	public abstract class ConnectedBodiesSystem : Simulation
	{
		protected ConnectedBodiesSystem(string name)
		{
			this._name = name;
			this.Objects = new List<PhyObject>();
			this.DictObjects = new Dictionary<int, PhyObject>();
		}

		protected ConnectedBodiesSystem(string name, IEnumerable<Mass> masses, IEnumerable<ConnectionBase> springs)
			: base(masses)
		{
			this._name = name;
			base.Connections.AddRange(springs);
		}

		public List<PhyObject> Objects { get; private set; }

		public Dictionary<int, PhyObject> DictObjects { get; private set; }

		public int TraceScanCounter { get; private set; }

		public override void InitProfiler(IPhyProfiler PhyProfiler)
		{
			base.InitProfiler(PhyProfiler);
		}

		protected override void writeProfilerFrame()
		{
			base.writeProfilerFrame();
		}

		public override void Clear()
		{
			base.Clear();
			base.Connections.Clear();
			if (this.DictConnections != null)
			{
				this.DictConnections.Clear();
			}
			this.ArrayConnectionsLength = 0;
			this.ArrayImpulseBasedSpringsLength = 0;
			this.ArrayVerletConstraintsLength = 0;
			this.Objects.Clear();
			if (this.DictObjects != null)
			{
				this.DictObjects.Clear();
			}
		}

		public override void Update(float dt)
		{
			this.resetMassTracer();
			base.Update(dt);
			if (this.replayRecorder != null)
			{
				this.replayRecorder.StartNewSegment();
			}
		}

		public void EnableMassTracer(int traceScanPeriod, int[] tracedMasses)
		{
			this.traceScanPeriod = traceScanPeriod;
			this.TraceScanCounter = 0;
			this.skipCounter = 0;
			this.tracedMasses = tracedMasses;
			this.Traces = new Vector3[tracedMasses.Length][];
			for (int i = 0; i < this.Traces.Length; i++)
			{
				this.Traces[i] = new Vector3[150 / traceScanPeriod + 1];
			}
		}

		public void SyncMassTracer(ConnectedBodiesSystem sourceSim)
		{
			if (this.tracedMasses == null || sourceSim.Traces == null)
			{
				return;
			}
			for (int i = 0; i < this.Traces.Length; i++)
			{
				Array.Copy(sourceSim.Traces[i], this.Traces[i], this.Traces[i].Length);
			}
			this.TraceScanCounter = sourceSim.TraceScanCounter;
		}

		private void resetMassTracer()
		{
			this.TraceScanCounter = 0;
		}

		private void updateMassTracer()
		{
			if (this.tracedMasses == null)
			{
				return;
			}
			if (this.skipCounter == this.traceScanPeriod)
			{
				for (int i = 0; i < this.tracedMasses.Length; i++)
				{
					Mass mass = null;
					this.DictMasses.TryGetValue(this.tracedMasses[i], out mass);
					if (mass != null)
					{
						this.Traces[i][this.TraceScanCounter] = mass.Position;
					}
				}
				this.skipCounter = 0;
				this.TraceScanCounter++;
			}
			else
			{
				this.skipCounter++;
			}
		}

		protected override void Solve()
		{
			this.ApplyForcesToMasses();
			for (int i = 0; i < this.ArrayConnectionsLength; i++)
			{
				this.ArrayConnections[i].Solve();
			}
		}

		protected void SatisfyVerletConstraints()
		{
			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < this.ArrayVerletConstraintsLength; j++)
				{
					if (j % 3 == this.verletCycle)
					{
						this.ArrayVerletConstraints[j].Satisfy();
					}
				}
			}
		}

		protected override void Operate(float dt)
		{
			base.Operate(dt);
			for (int i = 0; i < this.Objects.Count; i++)
			{
				this.Objects[i].Simulate(dt);
			}
			this.SatisfyVerletConstraints();
			this.verletCycle = (this.verletCycle + 1) % 3;
		}

		public virtual void ApplyForcesToMasses()
		{
			for (int i = 0; i < this.ArrayMassesLength; i += 4)
			{
				this.mcache[0] = this.ArrayMasses[i];
				this.mcache[1] = this.ArrayMasses[i + 1];
				this.mcache[2] = this.ArrayMasses[i + 2];
				this.mcache[3] = this.ArrayMasses[i + 3];
				Vector4f vector4f;
				vector4f..ctor(this.mcache[0].Weight, this.mcache[1].Weight, this.mcache[2].Weight, this.mcache[3].Weight);
				bool flag = ((ulong)this.IterationsCounter + (ulong)((long)this.mcache[0].UID)) % 3UL == 0UL;
				if (flag)
				{
					Vector4f vector4f2;
					vector4f2..ctor(this.mcache[0].Position4f.Y, this.mcache[1].Position4f.Y, this.mcache[2].Position4f.Y, this.mcache[3].Position4f.Y);
					Vector4f vector4f3;
					vector4f3..ctor(this.mcache[0].VisualPositionOffset.y, this.mcache[1].VisualPositionOffset.y, this.mcache[2].VisualPositionOffset.y, this.mcache[3].VisualPositionOffset.y);
					Vector4f vector4f4 = vector4f2 + vector4f3;
					vector4f4 = VectorOperations.Min(vector4f4, ConnectedBodiesSystem.WaterLevelEpsillon4f);
					vector4f4 = VectorOperations.Max(vector4f4, ConnectedBodiesSystem.WaterLevelEpsillon4f.Negative());
					vector4f4 = (Vector4fExtensions.one - vector4f4 / ConnectedBodiesSystem.WaterLevelEpsillon4f) * Vector4fExtensions.half;
					Vector4f vector4f5 = Vector4f.Zero;
					Vector4f vector4f6 = Vector4f.Zero;
					Vector4f vector4f7;
					vector4f7..ctor((!this.mcache[0].IgnoreEnvForces) ? 1f : 0f, (!this.mcache[1].IgnoreEnvForces) ? 1f : 0f, (!this.mcache[2].IgnoreEnvForces) ? 1f : 0f, (!this.mcache[3].IgnoreEnvForces) ? 1f : 0f);
					Vector4f vector4f8 = VectorOperations.CompareLessEqual(vector4f2, ConnectedBodiesSystem.WaterLevelEpsillon4f);
					vector4f8 &= Vector4fExtensions.one;
					for (int j = 0; j < 4; j++)
					{
						if (!this.mcache[j].IgnoreEnvForces)
						{
							this.relvelcache[j] = this.mcache[j].Velocity4f - this.mcache[j].FlowVelocity;
							this.relvelcache[j].W = 0f;
							vector4f5.set_Component(j, this.relvelcache[j].SqrMagnitude());
							Vector4f vector4f9 = VectorOperations.DuplicateLow(this.relvelcache[j]);
							vector4f9 *= vector4f9;
							vector4f9 = VectorOperations.Shuffle(vector4f9, 57);
							vector4f6.set_Component(j, VectorOperations.HorizontalAdd(vector4f9, vector4f9).X);
						}
					}
					vector4f5 = VectorOperations.Max(VectorOperations.Sqrt(vector4f5), ConnectedBodiesSystem.WaterSpeedLaminar4f);
					vector4f6 = VectorOperations.Sqrt(vector4f6);
					Vector4f vector4f10;
					vector4f10..ctor(this.mcache[0].CompoundWaterResistance, this.mcache[1].CompoundWaterResistance, this.mcache[2].CompoundWaterResistance, this.mcache[3].CompoundWaterResistance);
					vector4f10 *= vector4f5 * vector4f8;
					Vector4f vector4f11;
					vector4f11..ctor(this.mcache[0].Buoyancy, this.mcache[1].Buoyancy, this.mcache[2].Buoyancy, this.mcache[3].Buoyancy);
					vector4f11 += Vector4fExtensions.one;
					Vector4f vector4f12;
					vector4f12..ctor(this.mcache[0].ScaledBuoyancySpeedMultiplier, this.mcache[1].ScaledBuoyancySpeedMultiplier, this.mcache[2].ScaledBuoyancySpeedMultiplier, this.mcache[3].ScaledBuoyancySpeedMultiplier);
					vector4f12 *= vector4f6;
					vector4f12 = VectorOperations.Max(VectorOperations.Min(vector4f12, ConnectedBodiesSystem.MaxDynamicBuoyancy4f), ConnectedBodiesSystem.MaxDynamicBuoyancy4f.Negative());
					Vector4f vector4f13 = vector4f * (vector4f11 + vector4f12) * vector4f4 * vector4f7 - vector4f;
					Vector4f vector4f14;
					vector4f14..ctor(this.mcache[0].AirDragConstant, this.mcache[1].AirDragConstant, this.mcache[2].AirDragConstant, this.mcache[3].AirDragConstant);
					vector4f14 *= Vector4fExtensions.one - vector4f4;
					for (int k = 0; k < 4; k++)
					{
						this.mcache[k].perPeriodForcesCache4f = ((!(this.mcache[k] is VerletMass)) ? Vector4f.Zero : (new Vector4f(-vector4f10.get_Component(k)) * this.relvelcache[k])) + new Vector4f(0f, vector4f13.get_Component(k), 0f, 0f) - (this.mcache[k].Velocity4f - this.mcache[k].WindVelocity) * new Vector4f(vector4f14.get_Component(k));
					}
				}
				for (int l = 0; l < 4; l++)
				{
					Vector4f vector4f15 = this.mcache[l].perPeriodForcesCache4f + ((this.mcache[l].Position.y > 0.0125f) ? Vector4f.Zero : this.mcache[l].WaterMotor4f) + this.mcache[l].Motor4f;
					this.mcache[l].ApplyForce(vector4f15, false);
				}
			}
		}

		public virtual void ApplyForcesToMass(Mass mass)
		{
			if (mass._isKinematic)
			{
				return;
			}
			if (base.FrameIteration == 0)
			{
				mass.perFrameForcesCache4f = Vector4fExtensions.down * mass.MassValue4f * ConnectedBodiesSystem.GravityAcceleration4f;
			}
			bool flag = base.FrameIteration % 3 == 0;
			if (flag)
			{
				mass.perPeriodForcesCache4f = Vector4f.Zero;
			}
			Vector4f vector4f = mass.perFrameForcesCache4f;
			if (!mass.IgnoreEnvForces)
			{
				if (mass.Position.y <= mass.WaterHeight + 0.0125f)
				{
					Vector4f vector4f2 = mass.Velocity4f - mass.FlowVelocity;
					vector4f2.W = 0f;
					float num = 0.5f * (1f - Mathf.Clamp(mass.Position.y, -0.0125f, 0.0125f) / 0.0125f);
					float num2 = vector4f2.Magnitude();
					if (num2 < 1f)
					{
						num2 = 1f;
					}
					vector4f -= vector4f2 * mass.MassValue4f * (mass.WaterDragConstant4f + new Vector4f(mass.BuoyancySpeedMultiplier * 9.81f)) * new Vector4f(num2);
					if (flag)
					{
						float num3 = (mass.Buoyancy + 1f) * num;
						float num4 = 0f;
						if (mass.BuoyancySpeedMultiplier != 0f && mass.Position.y < 0f)
						{
							Vector2 vector;
							vector..ctor(mass.Velocity4f.X - mass.FlowVelocity.X, mass.Velocity4f.Z - mass.FlowVelocity.Z);
							num4 += vector.magnitude * mass.BuoyancySpeedMultiplier * 1.3f * num;
						}
						num4 = Mathf.Clamp(num4, -1000f, 1000f);
						mass.perPeriodForcesCache4f += Vector4fExtensions.up * (mass.MassValue4f * ConnectedBodiesSystem.GravityAcceleration4f * new Vector4f(num3 + num4));
					}
				}
				if (mass.Position.y >= 0f && flag)
				{
					mass.perPeriodForcesCache4f -= (mass.Velocity4f - mass.WindVelocity) * mass.AirDragConstant4f;
				}
				vector4f += mass.perPeriodForcesCache4f;
			}
			vector4f += mass.Motor4f;
			if (mass.Position.y <= mass.WaterHeight + 0.0125f)
			{
				vector4f += mass.WaterMotor4f;
			}
			mass.ApplyForce(vector4f, false);
		}

		private void refreshConnectionsAndObjectsArrays()
		{
			if (this.ArrayConnections == null)
			{
				this.ArrayConnections = new ConnectionBase[1024];
			}
			if (this.ArrayVerletConstraints == null)
			{
				this.ArrayVerletConstraints = new VerletConstraint[1024];
			}
			if (this.ArrayImpulseBasedSprings == null)
			{
				this.ArrayImpulseBasedSprings = new IImpulseConstraint[1024];
			}
			if (this.ArrayConnectionsLength > base.Connections.Count)
			{
				for (int i = base.Connections.Count; i < this.ArrayConnectionsLength; i++)
				{
					this.ArrayConnections[i] = null;
				}
			}
			this.ArrayConnectionsLength = base.Connections.Count;
			int arrayVerletConstraintsLength = this.ArrayVerletConstraintsLength;
			this.ArrayVerletConstraintsLength = 0;
			int arrayImpulseBasedSpringsLength = this.ArrayImpulseBasedSpringsLength;
			this.ArrayImpulseBasedSpringsLength = 0;
			int num = 0;
			for (int j = 0; j < base.Connections.Count; j++)
			{
				if (!base.Connections[j].Mass1.DisableSimulation && (base.Connections[j].Mass1.SourceMass == null || !base.Connections[j].Mass1.SourceMass.DisableSimulation) && !base.Connections[j].Mass2.DisableSimulation && (base.Connections[j].Mass2.SourceMass == null || !base.Connections[j].Mass2.SourceMass.DisableSimulation))
				{
					this.ArrayConnections[num] = base.Connections[j];
					num++;
					if (base.Connections[j] is VerletConstraint)
					{
						this.ArrayVerletConstraints[this.ArrayVerletConstraintsLength] = base.Connections[j] as VerletConstraint;
						this.ArrayVerletConstraintsLength++;
					}
					if (base.Connections[j] is IImpulseConstraint)
					{
						this.ArrayImpulseBasedSprings[this.ArrayImpulseBasedSpringsLength] = base.Connections[j] as IImpulseConstraint;
						this.ArrayImpulseBasedSpringsLength++;
					}
				}
			}
			this.ArrayConnectionsLength = num;
			if (arrayVerletConstraintsLength > this.ArrayVerletConstraintsLength)
			{
				for (int k = this.ArrayVerletConstraintsLength; k < arrayVerletConstraintsLength; k++)
				{
					this.ArrayVerletConstraints[k] = null;
				}
			}
			if (arrayImpulseBasedSpringsLength > this.ArrayImpulseBasedSpringsLength)
			{
				for (int l = this.ArrayImpulseBasedSpringsLength; l < arrayImpulseBasedSpringsLength; l++)
				{
					this.ArrayImpulseBasedSprings[l] = null;
				}
			}
		}

		public void ModifyObjectArrays(IList<Mass> createdMasses, IList<Mass> destroyedMasses, IList<ConnectionBase> createdConnections, IList<ConnectionBase> destroyedConnections, IList<PhyObject> createdObjects, IList<PhyObject> destroyedObjects)
		{
			base.ModifyObjectArrays(createdMasses, destroyedMasses);
			this.refreshConnectionsAndObjectsArrays();
			this.ModifyConnectionsDict(createdConnections, destroyedConnections);
			this.ModifyObjectsDict(createdObjects, destroyedObjects);
		}

		public void ModifyConnectionsDict(IList<ConnectionBase> createdConnections, IList<ConnectionBase> destroyedConnections)
		{
			if (this.DictConnections == null)
			{
				this.DictConnections = new Dictionary<int, ConnectionBase>();
			}
			for (int i = 0; i < destroyedConnections.Count; i++)
			{
				this.DictConnections.Remove(destroyedConnections[i].UID);
			}
			for (int j = 0; j < createdConnections.Count; j++)
			{
				ConnectionBase connectionBase = createdConnections[j];
				this.DictConnections[connectionBase.UID] = connectionBase;
			}
		}

		public void ModifyObjectsDict(IList<PhyObject> createdObjects, IList<PhyObject> destroyedObjects)
		{
			for (int i = 0; i < destroyedObjects.Count; i++)
			{
				this.DictObjects.Remove(destroyedObjects[i].UID);
			}
			for (int j = 0; j < createdObjects.Count; j++)
			{
				PhyObject phyObject = createdObjects[j];
				this.DictObjects[phyObject.UID] = phyObject;
			}
		}

		public override void RefreshObjectArrays(bool updateDict = true)
		{
			base.RefreshObjectArrays(updateDict);
			this.refreshConnectionsAndObjectsArrays();
			if (this.DictConnections == null)
			{
				this.DictConnections = new Dictionary<int, ConnectionBase>();
			}
			if (updateDict)
			{
				this._dictRemoveKeys.Clear();
				foreach (int num in this.DictConnections.Keys)
				{
					this._dictRemoveKeys.Add(num, null);
				}
				for (int i = 0; i < base.Connections.Count; i++)
				{
					if (!this.DictConnections.ContainsKey(base.Connections[i].UID))
					{
						this.DictConnections[base.Connections[i].UID] = base.Connections[i];
					}
					else
					{
						this._dictRemoveKeys.Remove(base.Connections[i].UID);
					}
				}
				foreach (int num2 in this._dictRemoveKeys.Keys)
				{
					this.DictConnections.Remove(num2);
				}
				this._dictRemoveKeys.Clear();
				foreach (int num3 in this.DictObjects.Keys)
				{
					this._dictRemoveKeys.Add(num3, null);
				}
				for (int j = 0; j < this.Objects.Count; j++)
				{
					if (!this.DictObjects.ContainsKey(this.Objects[j].UID))
					{
						this.DictObjects[this.Objects[j].UID] = this.Objects[j];
					}
					else
					{
						this._dictRemoveKeys.Remove(this.Objects[j].UID);
					}
				}
				foreach (int num4 in this._dictRemoveKeys.Keys)
				{
					this.DictObjects.Remove(num4);
				}
			}
		}

		public void RemoveConnection(ConnectionBase c)
		{
			base.Connections.Remove(c);
			this.PhyActionsListener.ConnectionDestroyed(c);
		}

		public void RemoveObject(PhyObject obj)
		{
			this.Objects.Remove(obj);
			this.DictObjects.Remove(obj.UID);
			this.PhyActionsListener.ObjectDestroyed(obj);
		}

		public override float CalculateSystemPotentialEnergy()
		{
			float num = 0f;
			for (int i = 0; i < base.Masses.Count; i++)
			{
				num += base.Masses[i].MassValue * 9.81f * base.Masses[i].Position4f.Y;
			}
			return base.CalculateSystemPotentialEnergy() + num;
		}

		public void DrawHeightFieldTree<T>(HeightFieldQuadTree<T> tree) where T : class, IPlaneXZResident
		{
			if (tree.ChildrenCount == 0)
			{
				Vector3 vector = tree.hd[0].Current(0f);
				Vector3 vector2 = tree.hd[1].Current(0f);
				Vector3 vector3 = tree.hd[2].Current(0f);
				Vector3 vector4 = tree.hd[3].Current(0f);
				Debug.DrawLine(vector, vector2, Color.green);
				Debug.DrawLine(vector2, vector4, Color.green);
				Debug.DrawLine(vector4, vector3, Color.green);
				Debug.DrawLine(vector3, vector, Color.green);
			}
			for (int i = 0; i < tree.ChildrenCount; i++)
			{
				this.DrawHeightFieldTree<T>(tree.GetChild(i) as HeightFieldQuadTree<T>);
			}
		}

		public void TriangulationDebugDraw()
		{
		}

		public void DebugDraw()
		{
			for (int i = 0; i < base.Masses.Count; i++)
			{
				Vector3 position = base.Masses[i].Position;
				Color color = ((!base.Masses[i]._isKinematic) ? Color.blue : Color.green);
				Debug.DrawLine(position - Vector3.right * 0.01f, position + Vector3.right * 0.01f, color);
				Debug.DrawLine(position - Vector3.forward * 0.01f, position + Vector3.forward * 0.01f, color);
				Debug.DrawLine(position - Vector3.up * 0.01f, position + Vector3.up * 0.01f, color);
			}
			for (int j = 0; j < base.Connections.Count; j++)
			{
				ConnectionBase connectionBase = base.Connections[j];
				if (!(connectionBase is TetrahedronBallJoint))
				{
					if (connectionBase.Mass1 != null && connectionBase.Mass2 != null)
					{
						Vector3 position2 = connectionBase.Mass1.Position;
						Vector3 vector = connectionBase.Mass2.Position;
						if (connectionBase is MassToRigidBodySpring)
						{
							vector = (connectionBase as MassToRigidBodySpring).AttachmentPointWorld();
						}
						Color color2 = ((j % 2 != 0) ? Color.magenta : Color.yellow);
						if (connectionBase.DeltaLength <= 0f)
						{
							color2 *= 0.35f;
						}
						Debug.DrawLine(position2, vector, color2);
					}
				}
			}
		}

		public void DebugSnapshotLog()
		{
			StringBuilder stringBuilder = new StringBuilder("----- Masses -----\n");
			for (int i = 0; i < base.Masses.Count; i++)
			{
				stringBuilder.AppendLine(base.Masses[i].ToString());
			}
			stringBuilder.AppendLine();
			Debug.Log(stringBuilder.ToString());
			stringBuilder = new StringBuilder("----- Connections -----\n");
			for (int j = 0; j < base.Connections.Count; j++)
			{
				stringBuilder.AppendLine(base.Connections[j].ToString());
			}
			Debug.Log(stringBuilder.ToString());
		}

		private void showListDiffMass(IEnumerable<Mass> m1, IEnumerable<Mass> m2, string set1, string set2)
		{
			HashSet<Mass> hashSet = new HashSet<Mass>(m1);
			HashSet<Mass> hashSet2 = new HashSet<Mass>(m2);
			StringBuilder stringBuilder = new StringBuilder("showListDiffMass:");
			foreach (Mass mass in m1)
			{
				if (mass != null)
				{
					if (!hashSet2.Contains(mass))
					{
						stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {3} but is absent from {4}.", new object[] { mass.Type, mass.UID, mass.IID, set1, set2 });
					}
				}
			}
			foreach (Mass mass2 in m2)
			{
				if (mass2 != null)
				{
					if (!hashSet.Contains(mass2))
					{
						stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {4} but is absent from {3}.", new object[] { mass2.Type, mass2.UID, mass2.IID, set1, set2 });
					}
				}
			}
			Debug.LogError(stringBuilder.ToString());
		}

		private void showListDiffConnections(IEnumerable<ConnectionBase> c1, IEnumerable<ConnectionBase> c2, string set1, string set2)
		{
			HashSet<ConnectionBase> hashSet = new HashSet<ConnectionBase>(c1);
			HashSet<ConnectionBase> hashSet2 = new HashSet<ConnectionBase>(c2);
			StringBuilder stringBuilder = new StringBuilder("showListDiffConnections:");
			foreach (ConnectionBase connectionBase in c1)
			{
				if (connectionBase != null)
				{
					if (!hashSet2.Contains(connectionBase))
					{
						stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {3} but is absent from {4}.", new object[]
						{
							connectionBase.GetType(),
							connectionBase.UID,
							connectionBase.IID,
							set1,
							set2
						});
					}
				}
			}
			foreach (ConnectionBase connectionBase2 in c2)
			{
				if (connectionBase2 != null)
				{
					if (!hashSet.Contains(connectionBase2))
					{
						stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {4} but is absent from {3}.", new object[]
						{
							connectionBase2.GetType(),
							connectionBase2.UID,
							connectionBase2.IID,
							set1,
							set2
						});
					}
				}
			}
			Debug.LogError(stringBuilder.ToString());
		}

		private void showListDiffObjects(IEnumerable<PhyObject> c1, IEnumerable<PhyObject> c2, string set1, string set2)
		{
			HashSet<PhyObject> hashSet = new HashSet<PhyObject>(c1);
			HashSet<PhyObject> hashSet2 = new HashSet<PhyObject>(c2);
			StringBuilder stringBuilder = new StringBuilder("showListDiffObjects:");
			foreach (PhyObject phyObject in c1)
			{
				if (phyObject != null)
				{
					if (!hashSet2.Contains(phyObject))
					{
						stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {3} but is absent from {4}.", new object[]
						{
							phyObject.GetType(),
							phyObject.UID,
							phyObject.IID,
							set1,
							set2
						});
					}
				}
			}
			foreach (PhyObject phyObject2 in c2)
			{
				if (phyObject2 != null)
				{
					if (!hashSet.Contains(phyObject2))
					{
						stringBuilder.AppendFormat("\n{0} {1}:{2} is present in {4} but is absent from {3}.", new object[]
						{
							phyObject2.GetType(),
							phyObject2.UID,
							phyObject2.IID,
							set1,
							set2
						});
					}
				}
			}
			Debug.LogError(stringBuilder.ToString());
		}

		public void DetectRefLeaks()
		{
			if (this.ArrayMassesLength != base.Masses.Count || this.ArrayMassesLength != this.DictMasses.Count)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"DetectRefLeaks FAIL: masses containers do not match: ArrayMassesLength = ",
					this.ArrayMassesLength,
					" DictMasses.Count = ",
					this.DictMasses.Count,
					" Masses.Count = ",
					base.Masses.Count
				}));
				if (this.ArrayMassesLength != base.Masses.Count)
				{
					this.showListDiffMass(this.ArrayMasses, base.Masses, "ArrayMasses", "Masses");
				}
				if (this.ArrayMassesLength != this.DictMasses.Count)
				{
					this.showListDiffMass(this.ArrayMasses, this.DictMasses.Values, "ArrayMasses", "DictMasses");
				}
			}
			if (this.ArrayConnectionsLength != base.Connections.Count || this.ArrayConnectionsLength != this.DictConnections.Count)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"DetectRefLeaks FAIL: connections containers do not match: ArrayConnectionsLength = ",
					this.ArrayConnectionsLength,
					" DictConnections.Count = ",
					this.DictConnections.Count,
					" Connections.Count = ",
					base.Connections.Count
				}));
				if (this.ArrayConnectionsLength != base.Connections.Count)
				{
					this.showListDiffConnections(this.ArrayConnections, base.Connections, "ArrayConnections", "Connections");
				}
				if (this.ArrayConnectionsLength != this.DictConnections.Count)
				{
					this.showListDiffConnections(this.ArrayConnections, this.DictConnections.Values, "ArrayConnections", "DictConnections");
				}
			}
			if (this.DictObjects.Count != this.Objects.Count)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"DetectRefLeaks FAIL: objects containers do not match: Objects.Count = ",
					this.Objects.Count,
					" DictObjects.Count = ",
					this.DictObjects.Count
				}));
				this.showListDiffObjects(this.DictObjects.Values, this.Objects, "DictObjects", "Objects");
			}
			for (int i = 0; i < this.ArrayMassesLength; i++)
			{
				if (!this.DictMasses.ContainsKey(this.ArrayMasses[i].UID))
				{
					Debug.LogError("DetectRefLeaks FAIL: Mass_" + this.ArrayMasses[i].UID + " does not present in the dict");
				}
			}
			for (int j = 0; j < this.ArrayConnectionsLength; j++)
			{
				if (!this.DictConnections.ContainsKey(this.ArrayConnections[j].UID))
				{
					Debug.LogError("DetectRefLeaks FAIL: Connection_" + this.ArrayConnections[j].UID + " does not present in the dict");
				}
			}
			foreach (PhyObject phyObject in this.Objects)
			{
				if (!this.DictObjects.ContainsKey(phyObject.UID))
				{
					Debug.LogError("DetectRefLeaks FAIL: PhyObject_" + phyObject.UID + " does not present in the dict");
				}
			}
			for (int k = 0; k < base.Masses.Count; k++)
			{
				if (base.Masses[k].Sim != this)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"DetectRefLeaks FAIL: [Masses] Mass ",
						base.Masses[k].Type,
						" ",
						base.Masses[k].UID,
						" has invalid Sim reference"
					}));
				}
			}
			for (int l = 0; l < this.ArrayMassesLength; l++)
			{
				if (this.ArrayMasses[l].Sim != this)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"DetectRefLeaks FAIL: [ArrayMasses] Mass ",
						this.ArrayMasses[l].Type,
						" ",
						this.ArrayMasses[l].UID,
						" has invalid Sim reference"
					}));
				}
			}
			foreach (Mass mass in this.DictMasses.Values)
			{
				if (mass.Sim != this)
				{
					Debug.LogError(string.Concat(new object[] { "DetectRefLeaks FAIL: [DictMasses] Mass ", mass.Type, " ", mass.UID, " has invalid Sim reference" }));
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<Mass> list = new List<Mass>();
			List<ConnectionBase> list2 = new List<ConnectionBase>();
			List<PhyObject> list3 = new List<PhyObject>();
			for (int m = 0; m < base.Masses.Count; m++)
			{
				base.Masses[m].DetectRefLeaks(list, list2, list3, stringBuilder);
			}
			for (int n = 0; n < base.Connections.Count; n++)
			{
				base.Connections[n].DetectRefLeaks(list, list2, list3, stringBuilder);
			}
			for (int num = 0; num < this.Objects.Count; num++)
			{
				this.Objects[num].DetectRefLeaks(list, list2, list3, stringBuilder);
			}
			if (stringBuilder.Length > 0)
			{
				Debug.LogError(stringBuilder);
			}
			if (list.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder("DetectRefLeaks FAIL: Following referenced masses do not present in the system:\n");
				foreach (Mass mass2 in list)
				{
					stringBuilder2.AppendLine(mass2.Type + " " + mass2.UID);
				}
				Debug.LogError(stringBuilder2);
			}
			if (list2.Count > 0)
			{
				StringBuilder stringBuilder3 = new StringBuilder("DetectRefLeaks FAIL: Following referenced connections do not present in the system:\n");
				foreach (ConnectionBase connectionBase in list2)
				{
					stringBuilder3.AppendLine(connectionBase.GetType().ToString() + " " + connectionBase.UID);
				}
				Debug.LogError(stringBuilder3);
			}
			if (list3.Count > 0)
			{
				StringBuilder stringBuilder4 = new StringBuilder("DetectRefLeaks FAIL: Following referenced objects do not present in the system:\n");
				foreach (PhyObject phyObject2 in list3)
				{
					stringBuilder4.AppendLine(phyObject2.Type + " " + phyObject2.UID);
				}
				Debug.LogError(stringBuilder4);
			}
		}

		public void EnableReplayRecorder(string fileName, string relativePath = "/monitor_output/phyreplays")
		{
			if (this.replayRecorder == null)
			{
				Debug.LogWarning("Starting phy replay recording #" + ConnectedBodiesSystem.replayCounter);
				this.replayRecorder = new PhyReplay(this, string.Concat(new object[]
				{
					ConnectedBodiesSystem.appDataPath,
					"/../",
					relativePath,
					"/",
					fileName,
					ConnectedBodiesSystem.replayCounter++,
					".phy"
				}), false);
			}
		}

		public void SaveReplayRecorder()
		{
			if (this.replayRecorder != null)
			{
				Debug.LogWarning("Stopping phy replay recording #" + (ConnectedBodiesSystem.replayCounter - 1));
				this.replayRecorder.Close();
				this.replayRecorder = null;
			}
		}

		public void ResetReplayRecorder()
		{
			if (this.replayRecorder != null)
			{
				this.replayRecorder.StartNewSegment();
			}
		}

		public float MassToMassSpringDistance(Mass lowerMass, Mass upperMass, float load = 0f)
		{
			float num = 0f;
			Mass mass = lowerMass;
			while (mass.NextSpring != null && mass != upperMass)
			{
				if (load == 0f)
				{
					num += mass.NextSpring.SpringLength;
				}
				else
				{
					num += mass.NextSpring.EquilibrantLength(load * 9.81f);
				}
				mass = mass.NextSpring.Mass2;
			}
			return (mass != upperMass) ? (-1f) : num;
		}

		public const int ConnectionsMaxCount = 1024;

		public const int VerletConstraintsMaxCount = 1024;

		public const int ObjectsMaxCount = 128;

		protected ConnectionBase[] ArrayConnections;

		protected int ArrayConnectionsLength;

		protected VerletConstraint[] ArrayVerletConstraints;

		protected int ArrayVerletConstraintsLength;

		protected IImpulseConstraint[] ArrayImpulseBasedSprings;

		protected int ArrayImpulseBasedSpringsLength;

		public Dictionary<int, ConnectionBase> DictConnections;

		public const int MaxVerletConstraintIterations = 1;

		public const float GravityAcceleration = 9.81f;

		public static readonly Vector4f GravityAcceleration4f = new Vector4f(9.81f);

		public const float WaterDragConstant = 10f;

		public static readonly Vector4f WaterDragConstant4f = new Vector4f(10f);

		public const float WaterLevelEpsillon = 0.0125f;

		public static readonly Vector4f WaterLevelEpsillon4f = new Vector4f(0.0125f);

		public const float BuoyancySpeedMultiplierFactor = 1.3f;

		public const float WaterSpeedLaminar = 1f;

		public static readonly Vector4f WaterSpeedLaminar4f = new Vector4f(1f);

		public const float MaxDynamicBuoyancy = 1000f;

		public static readonly Vector4f MaxDynamicBuoyancy4f = new Vector4f(1000f);

		public const float WaterFlowVelocityFactor = 0.8f;

		public const float AirDragConstant = 0.001f;

		public static readonly Vector4f AirDragConstant4f = new Vector4f(0.001f);

		public const float GroundRepulsionConstant = 2f;

		public const float GroundDragConstant = 0.2f;

		public static readonly Vector4f GroundDragConstant4f = new Vector4f(0.2f);

		public const float GroundAbsorptionConstant = 0.995f;

		public static readonly Vector4f GroundAbsorptionConstant4f = new Vector4f(0.995f);

		public const float GroundFrictionDelta = 0.035f;

		protected PhyReplay replayRecorder;

		private int[] tracedMasses;

		public Vector3[][] Traces;

		private int traceScanPeriod;

		private int skipCounter;

		protected string _name;

		private int verletCycle;

		private Mass[] mcache = new Mass[4];

		private Vector4f[] relvelcache = new Vector4f[4];

		private Dictionary<int, object> _dictRemoveKeys = new Dictionary<int, object>(1024);

		public static int replayCounter = 0;

		private static readonly string appDataPath = Application.dataPath;
	}
}
