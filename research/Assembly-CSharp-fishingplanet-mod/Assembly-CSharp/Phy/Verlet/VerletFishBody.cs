using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletFishBody : AbstractFishBody
	{
		public VerletFishBody(ConnectedBodiesSystem sim, float mass, Vector3 tipPosition, Vector3 right, Vector3 up, Vector3 forward, float edge, int segmentsCount, float waveAmp, float waveFreq, Vector3 waveAxis, float[] massDistribution, float[] jointsStiffness, float[] jointsFriction)
			: base(sim, mass)
		{
			this.segmentsCount = segmentsCount;
			float num = 1f / Mathf.Sqrt(2f);
			float num2 = edge * (Mathf.Sqrt(6f) / 3f);
			float num3 = edge / Mathf.Sqrt(3f);
			float num4 = Mathf.Sqrt(3f) * 0.5f * edge;
			base.Edge = edge;
			this.SegmentHeight = num2;
			this.BendWaveAxis = waveAxis;
			Vector3 normalized = (right * (num3 - num4) + up * edge * 0.5f).normalized;
			Vector3 normalized2 = (right * (num3 - num4) - up * edge * 0.5f).normalized;
			this.bottomAxis = new Vector2(0f, -1f);
			this.leftAxis = new Vector2(-0.5f, Mathf.Sqrt(3f) / 6f);
			this.rightAxis = new Vector2(0.5f, Mathf.Sqrt(3f) / 6f);
			Vector3 vector = tipPosition;
			float num5 = 0f;
			for (int i = 0; i < massDistribution.Length; i++)
			{
				num5 += massDistribution[i];
			}
			this.joints = new TetrahedronBallJoint[segmentsCount - 1];
			for (int j = 0; j < segmentsCount; j++)
			{
				int num6 = base.addTetrahedron(mass * massDistribution[j] / num5, vector, vector + forward * num2 + right * num3, vector + forward * num2 + normalized * num3, vector + forward * num2 + normalized2 * num3, -1f);
				vector += forward * num2;
				if (j > 0)
				{
					TetrahedronBallJoint tetrahedronBallJoint = base.addBallJoint(num6 - 4, num6, jointsStiffness[j - 1] * 0.5f, jointsFriction[j - 1]);
					tetrahedronBallJoint.WaveDeviationPhase = (float)j * (6.2831855f / (float)(segmentsCount - 1));
					if (j > 1)
					{
						tetrahedronBallJoint.WaveDeviationAmp = waveAmp * Mathf.Pow((float)j / (float)(segmentsCount - 1), 1f);
					}
					else
					{
						tetrahedronBallJoint.WaveDeviationAmp = 0f;
					}
					tetrahedronBallJoint.WaveDeviationFreq = waveFreq;
					tetrahedronBallJoint.WaveAxis = waveAxis;
					tetrahedronBallJoint.FirstChordMass = base.Masses[0] as VerletMass;
					tetrahedronBallJoint.SecondChordMass = base.Masses[4] as VerletMass;
					this.joints[j - 1] = tetrahedronBallJoint;
				}
			}
			this.rollstab = new TetrahedronRollStabilizer[1];
			for (int k = 0; k < 1; k++)
			{
				this.rollstab[k] = new TetrahedronRollStabilizer(new Mass[]
				{
					base.Masses[k * 4],
					base.Masses[k * 4 + 1],
					base.Masses[k * 4 + 2],
					base.Masses[k * 4 + 3]
				}.Cast<VerletMass>().ToArray<VerletMass>(), 0.003f, Vector3.down);
				base.Connections.Add(this.rollstab[k]);
			}
			this.strain = new VerletFishBendStrain(this);
			base.Connections.Add(this.strain);
			base.thrust = new VerletFishThrustController(this);
			base.Connections.Add(base.thrust);
			this.UpdateSim();
			Vector4f currentGillPosition = this.GetCurrentGillPosition();
			this.cachedGillDistances = new float[this.gillAttachments.Length];
			for (int l = 0; l < this.gillAttachments.Length; l++)
			{
				this.cachedGillDistances[l] = (currentGillPosition - base.Masses[this.gillAttachments[l]].Position4f).Magnitude();
			}
			Vector4f zero = Vector4f.Zero;
			this.groundAlign(ref zero);
			this.KinematicTranslate(zero);
		}

		public VerletFishBody(ConnectedBodiesSystem sim, VerletFishBody source)
			: base(sim, source)
		{
			this.segmentsCount = source.segmentsCount;
			this.SegmentHeight = source.SegmentHeight;
			this.rightAxis = source.rightAxis;
			this.leftAxis = source.leftAxis;
			this.bottomAxis = source.bottomAxis;
			this.BendWaveAxis = source.BendWaveAxis;
			this.joints = new TetrahedronBallJoint[source.joints.Length];
			for (int i = 0; i < this.joints.Length; i++)
			{
				this.joints[i] = this.Sim.DictConnections[source.joints[i].UID] as TetrahedronBallJoint;
			}
			this.cachedGillDistances = source.cachedGillDistances;
			this.gillAttachments = source.gillAttachments;
			this.rollstab = new TetrahedronRollStabilizer[source.rollstab.Length];
			for (int j = 0; j < this.rollstab.Length; j++)
			{
				this.rollstab[j] = this.Sim.DictConnections[source.rollstab[j].UID] as TetrahedronRollStabilizer;
			}
			this.strain = this.Sim.DictConnections[source.strain.UID] as VerletFishBendStrain;
			this.BendMaxAngle = source.BendMaxAngle;
			this.BendReboundPoint = source.BendReboundPoint;
			this.BendStiffnessMultiplier = source.BendStiffnessMultiplier;
		}

		public override Mass Mouth
		{
			get
			{
				return base.Masses[0];
			}
		}

		public Mass Neck
		{
			get
			{
				return base.Masses[4];
			}
		}

		public override Mass Root
		{
			get
			{
				return base.Masses[12];
			}
		}

		public float SegmentHeight { get; private set; }

		public override void SyncMain(PhyObject source)
		{
			base.SyncMain(source);
			VerletFishBody verletFishBody = source as VerletFishBody;
			verletFishBody.ChordRotation = base.ChordRotation;
			verletFishBody.BendStiffnessMultiplier = this.BendStiffnessMultiplier;
			verletFishBody.BendMaxAngle = this.BendMaxAngle;
			verletFishBody.BendReboundPoint = this.BendReboundPoint;
		}

		public override float UnderwaterRatio
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < base.Masses.Count; i++)
				{
					if (base.Masses[i].Position.y < base.Masses[i].WaterHeight)
					{
						num += 1f;
					}
				}
				return num / (float)base.Masses.Count;
			}
		}

		public void SetWaveAxis(Vector3 axis)
		{
			for (int i = 0; i < this.joints.Length; i++)
			{
				this.joints[i].WaveAxis = axis;
				this.joints[i].ConnectionNeedSyncMark();
			}
		}

		public override void VisualChordRotation(float newChordRotation)
		{
			base.ChordRotation = newChordRotation;
			this.SetWaveAxis(Quaternion.AngleAxis(-base.ChordRotation, Vector3.up) * Vector3.right);
		}

		public override void groundAlign(ref Vector4f currentTranslate)
		{
			float num = 0f;
			for (int i = 0; i < base.Masses.Count; i++)
			{
				Vector3 vector = base.Masses[i].Position + currentTranslate.AsVector3();
				float num2 = Math3d.GetGroundHight(vector);
				num2 += Mass.collisionEpsilon4f.X;
				if (num2 > vector.y)
				{
					float num3 = num2 - vector.y;
					if (num3 > num)
					{
						num = num3;
					}
				}
			}
			currentTranslate += new Vector4f(0f, num, 0f, 0f);
		}

		private void obstaclesAlign()
		{
			Vector3 vector = Vector3.zero;
			float num = 0f;
			bool flag = false;
			for (int i = 0; i < base.Masses.Count; i++)
			{
				Vector3 position = base.Masses[i].Position;
				RaycastHit raycastHit;
				RaycastHit raycastHit2;
				if (Physics.Raycast(position, Vector3.down, ref raycastHit, 5f, GlobalConsts.TerrainMask) && Physics.Raycast(position + Vector3.up * 5f, Vector3.down, ref raycastHit2, 5f, GlobalConsts.ObstaclesExceptTerrainMask) && raycastHit2.point.y > raycastHit.point.y)
				{
					flag = true;
					vector = GameFactory.Player.Tackle.transform.position - position;
					num = vector.magnitude;
					vector /= num;
					vector.y = 0f;
					vector.Normalize();
					break;
				}
			}
			if (flag)
			{
				for (int j = 0; j < base.Masses.Count; j++)
				{
					Vector3 position2 = base.Masses[j].Position;
					base.Masses[j].Position += vector * (1.5f * (num + this.SegmentHeight * (float)this.segmentsCount));
					base.Masses[j].StopMass();
					Debug.DrawLine(position2, base.Masses[j].Position, Color.yellow, 10f);
				}
			}
		}

		public override void SetBendParameters(float maxAngle, float reboundPoint, float stiffnessMult)
		{
			this.BendMaxAngle = maxAngle;
			this.BendReboundPoint = reboundPoint;
			this.BendStiffnessMultiplier = stiffnessMult;
		}

		public override void SetLocomotionWaveMult(float mult)
		{
			for (int i = 0; i < this.joints.Length; i++)
			{
				if (!Mathf.Approximately(this.joints[i].WaveDeviationMult, mult))
				{
					this.joints[i].WaveDeviationMult = mult;
					this.joints[i].ConnectionNeedSyncMark();
				}
			}
		}

		public override Mass ReplaceFirstMass(Mass newMass)
		{
			VerletMass verletMass = newMass as VerletMass;
			newMass.MassValue = base.Masses[0].MassValue;
			newMass.Buoyancy = base.Masses[0].Buoyancy;
			newMass.IsRef = true;
			newMass.Position = base.Masses[0].Position;
			newMass.Collision = Mass.CollisionType.Full;
			newMass.WaterDragConstant = base.Masses[0].WaterDragConstant;
			newMass.Type = Mass.MassType.Fish;
			newMass.SlidingFrictionFactor = 0.01f;
			newMass.StaticFrictionFactor = 0.01f;
			Mass mass = base.Masses[0];
			base.Masses.RemoveAt(0);
			base.Masses.Insert(0, newMass);
			base.Connections[0].SetMasses(newMass, null);
			base.Connections[1].SetMasses(newMass, null);
			base.Connections[2].SetMasses(newMass, null);
			this.joints[0].Tetrahedron1[0] = verletMass;
			this.joints[0].Mass1 = newMass;
			for (int i = 0; i < this.joints.Length; i++)
			{
				this.joints[i].FirstChordMass = verletMass;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.ConnectionNeedSyncMark(this.joints[i].UID);
				}
			}
			this.rollstab[0].Tetrahedron[0] = verletMass;
			this.rollstab[0].Mass1 = newMass;
			this.strain.Mass1 = newMass;
			base.thrust.SetMasses(newMass, null);
			this.Sim.RefreshObjectArrays(true);
			if (this.Sim.PhyActionsListener != null)
			{
				this.Sim.PhyActionsListener.ConnectionNeedSyncMark(base.thrust.UID);
				this.Sim.PhyActionsListener.ConnectionNeedSyncMark(this.rollstab[0].UID);
				this.Sim.PhyActionsListener.ObjectNeedSyncMark(base.UID);
			}
			return mass;
		}

		public override float TetrahedronHeight
		{
			get
			{
				return base.Edge * (Mathf.Sqrt(6f) / 3f);
			}
		}

		public override Vector3[] GetIdealTetrahedronPositions(Vector3 baseCenter, Vector3 right, Vector3 up, Vector3 forward)
		{
			float tetrahedronHeight = this.TetrahedronHeight;
			float num = base.Edge / Mathf.Sqrt(3f);
			float num2 = Mathf.Sqrt(3f) * 0.5f * base.Edge;
			Vector3 normalized = (right * (num - num2) + up * base.Edge * 0.5f).normalized;
			Vector3 normalized2 = (right * (num - num2) - up * base.Edge * 0.5f).normalized;
			return new Vector3[]
			{
				baseCenter + forward * tetrahedronHeight,
				baseCenter + right * num,
				baseCenter + normalized * num,
				baseCenter + normalized2 * num
			};
		}

		public Vector4f GetCurrentGillPosition()
		{
			return (base.Masses[3].Position4f * Vector4fExtensions.half3 + base.Masses[7].Position4f * Vector4fExtensions.half3 + base.Masses[4].Position4f) * Vector4fExtensions.half3;
		}

		public VerletMass AddAttachedMass(Vector3 position, float mass, int[] attachToMasses)
		{
			VerletMass verletMass = base.addMass(mass, position, Mass.MassType.Fish);
			this.Sim.Masses.Add(verletMass);
			List<ConnectionBase> list = new List<ConnectionBase>();
			foreach (int num in attachToMasses)
			{
				list.Add(base.addSpring(verletMass, base.Masses[num] as VerletMass, 0.1f, -1f, false));
			}
			this.Sim.Connections.AddRange(list);
			this.Sim.RefreshObjectArrays(true);
			return verletMass;
		}

		public VerletMass CreateGillMass()
		{
			VerletMass verletMass = base.addMass(base.TotalMass * 0.1f, this.GetCurrentGillPosition().AsVector3(), Mass.MassType.Fish);
			this.Sim.Masses.Add(verletMass);
			List<ConnectionBase> list = new List<ConnectionBase>();
			for (int i = 0; i < this.gillAttachments.Length; i++)
			{
				list.Add(base.addSpring(verletMass, base.Masses[this.gillAttachments[i]] as VerletMass, 0.1f, (i != 3 && i != 4 && i != 7) ? (-1f) : this.cachedGillDistances[i], false));
			}
			this.Sim.Connections.AddRange(list);
			this.Sim.RefreshObjectArrays(true);
			return verletMass;
		}

		public float TriAdjustedDot(Vector2 lhs, Vector2 rhs)
		{
			return Mathf.Max((Vector2.Dot(lhs, rhs) + 0.5f) / 1.5f, 0f);
		}

		public override void StartBendStrain(float stiffnessMult = -1f)
		{
			if (stiffnessMult > 0f)
			{
				this.strain.StiffnessMultiplier = stiffnessMult;
			}
			else
			{
				this.strain.StiffnessMultiplier = this.BendStiffnessMultiplier;
			}
			this.strain.StartBendStrain();
		}

		public override void StopBendStrain()
		{
			this.strain.StopBendStrain();
		}

		public override void RollStabilizer(Vector3 destUp, float intensity = 1f)
		{
			for (int i = 0; i < this.rollstab.Length; i++)
			{
				this.rollstab[i].StiffnessMultiplier = intensity;
				this.rollstab[i].TargetDirection = Vector3.Cross(destUp, base.Masses[i * 4].Position - base.Masses[i * 4 + 4].Position).normalized;
				this.rollstab[i].ConnectionNeedSyncMark();
				Debug.DrawLine(base.Masses[i * 4].Position, base.Masses[i * 4].Position + this.rollstab[i].TargetDirection, Color.cyan);
			}
		}

		public override float GetSegmentAxialOrientation(int segment)
		{
			int num = segment * 4;
			Vector4f vector4f = Vector4fExtensions.Cross(base.Masses[num + 3].Position4f - base.Masses[num + 1].Position4f, base.Masses[num + 2].Position4f - base.Masses[num + 1].Position4f);
			Vector4f vector4f2 = base.Masses[num].Position4f - (base.Masses[num + 1].Position4f + base.Masses[num + 2].Position4f + base.Masses[num + 3].Position4f) * Vector4fExtensions.onethird3;
			return (float)Math.Sign(Vector4fExtensions.Dot(vector4f, vector4f2));
		}

		public override Vector3 GetSegmentRight(int segment)
		{
			int num = segment * 4;
			Vector3 segmentUp = this.GetSegmentUp(segment);
			Quaternion quaternion = Quaternion.AngleAxis(base.ChordRotation, segmentUp);
			return quaternion * ((base.Masses[num + 1].Position4f + base.Masses[num + 2].Position4f + base.Masses[num + 3].Position4f) * Vector4fExtensions.onethird3 - base.Masses[num + 3].Position4f).Normalized().AsVector3();
		}

		public override Vector3 GetSegmentUp(int segment)
		{
			int num = segment * 4;
			return (base.Masses[num].Position4f - (base.Masses[num + 1].Position4f + base.Masses[num + 2].Position4f + base.Masses[num + 3].Position4f) * Vector4fExtensions.onethird3).Normalized().AsVector3();
		}

		public override Vector3 GetSegmentBendAxis(int segment)
		{
			int num = segment * 4;
			Vector3 segmentUp = this.GetSegmentUp(segment);
			Vector3 segmentRight = this.GetSegmentRight(segment);
			Vector3 normalized = Vector3.Cross(segmentRight, segmentUp).normalized;
			return this.BendWaveAxis.x * segmentRight + this.BendWaveAxis.y * segmentUp + this.BendWaveAxis.z * normalized;
		}

		public override Vector3 GetSegmentBendDirection(int segment)
		{
			int num = segment * 4;
			Vector3 segmentUp = this.GetSegmentUp(segment);
			Vector3 segmentRight = this.GetSegmentRight(segment);
			Vector3 normalized = Vector3.Cross(segmentRight, segmentUp).normalized;
			Vector3 vector = this.BendWaveAxis.x * segmentRight + this.BendWaveAxis.y * segmentUp + this.BendWaveAxis.z * normalized;
			return Vector3.Cross(segmentUp, vector).normalized;
		}

		public override void BeforeSimulationUpdate()
		{
		}

		public override void DebugDraw()
		{
			for (int i = 0; i < this.segmentsCount; i++)
			{
				int num = i * 4;
				Vector4f vector4f = (base.Masses[num + 1].Position4f + base.Masses[num + 2].Position4f + base.Masses[num + 3].Position4f) * Vector4fExtensions.onethird3;
				Vector3 vector = vector4f.AsVector3();
				Color color = ((this.GetSegmentAxialOrientation(i) <= 0f) ? Color.red : Color.green);
				Debug.DrawLine(base.Masses[num].Position4f.AsVector3(), vector, color);
				Debug.DrawLine(vector, base.Masses[num + 3].Position4f.AsVector3(), color);
			}
		}

		private int segmentsCount;

		private Vector2 rightAxis;

		private Vector2 leftAxis;

		private Vector2 bottomAxis;

		public TetrahedronBallJoint[] joints;

		private float[] cachedGillDistances;

		private int[] gillAttachments = new int[] { 1, 2, 3, 4, 5, 6, 7 };

		private TetrahedronRollStabilizer[] rollstab;

		private VerletFishBendStrain strain;

		public float BendMaxAngle;

		public float BendReboundPoint;

		public Vector3 BendWaveAxis;
	}
}
