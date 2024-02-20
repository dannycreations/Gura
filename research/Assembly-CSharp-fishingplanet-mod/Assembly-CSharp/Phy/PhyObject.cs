using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BiteEditor.ObjectModel;
using Mono.Simd;
using ObjectModel;
using UnityEngine;

namespace Phy
{
	public class PhyObject
	{
		public PhyObject(PhyObjectType type, ConnectedBodiesSystem sim)
		{
			this.Type = type;
			this.Masses = new List<Mass>();
			this.Connections = new List<ConnectionBase>();
			this.Sim = sim;
			this.UID = this.Sim.NewUID;
			this.IID = PhyObject.NewIID;
			this.Sim.Objects.Add(this);
			LogHelper.Log("Add PhyObject {0}.{1}", new object[] { this.UID, this.Type });
			this.Sim.DictObjects[this.UID] = this;
			this.CheckLayerMass = null;
			this.LayerNameUnderObject = SplatMap.LayerName.None;
			this.NeedCheckSurface = false;
			this.SurfaceUnderObject = SurfaceMaterial.NONE;
			this.Sim.PhyActionsListener.ObjectCreated(this);
		}

		public PhyObject(ConnectedBodiesSystem sim, PhyObject source)
		{
			this.Type = source.Type;
			this.Masses = new List<Mass>();
			this.Connections = new List<ConnectionBase>();
			this.Sim = sim;
			for (int i = 0; i < source.Masses.Count; i++)
			{
				this.Masses.Add(this.Sim.DictMasses[source.Masses[i].UID]);
			}
			for (int j = 0; j < source.Connections.Count; j++)
			{
				this.Connections.Add(this.Sim.DictConnections[source.Connections[j].UID]);
			}
			this.UID = source.UID;
			this.IID = -PhyObject.NewIID;
			this.NeedCheckSurface = source.NeedCheckSurface;
		}

		public PhyObjectType Type { get; set; }

		public List<Mass> Masses { get; private set; }

		public List<ConnectionBase> Connections { get; private set; }

		public int UID { get; private set; }

		public int IID { get; private set; }

		public static int NewIID
		{
			get
			{
				return Interlocked.Decrement(ref PhyObject.IIDCounter);
			}
		}

		public override int GetHashCode()
		{
			return this.IID;
		}

		public bool IsKinematic
		{
			get
			{
				return this.isKinematic;
			}
			set
			{
				this.isKinematic = value;
				foreach (Mass mass in this.Masses)
				{
					mass.IsKinematic = value;
					mass.StopMass();
				}
			}
		}

		public bool IsLying
		{
			get
			{
				foreach (Mass mass in this.Masses)
				{
					if (mass.IsLying)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Mass.CollisionType Collision
		{
			get
			{
				return this.Masses[0].Collision;
			}
			set
			{
				foreach (Mass mass in this.Masses)
				{
					mass.Collision = value;
				}
			}
		}

		public void StopMasses()
		{
			foreach (Mass mass in this.Masses)
			{
				mass.StopMass();
			}
		}

		public Vector3 VisualPositionOffset
		{
			get
			{
				return this.Masses[0].VisualPositionOffset;
			}
			set
			{
				foreach (Mass mass in this.Masses)
				{
					mass.VisualPositionOffset = value;
				}
			}
		}

		public virtual void KinematicTranslate(Vector4f offset)
		{
			foreach (Mass mass in this.Masses)
			{
				mass.KinematicTranslate(offset);
			}
		}

		public bool IgnoreEnvForces
		{
			set
			{
				foreach (Mass mass in this.Masses)
				{
					mass.IgnoreEnvForces = value;
				}
			}
		}

		public void SetMotionDamping(float damping)
		{
			foreach (Mass mass in this.Masses)
			{
				mass.MotionDamping = damping;
			}
		}

		public float CurrentVelocityLimit
		{
			get
			{
				return this.Masses[0].CurrentVelocityLimit;
			}
			set
			{
				foreach (Mass mass in this.Masses)
				{
					mass.CurrentVelocityLimit = value;
				}
			}
		}

		public SplatMap.LayerName LayerNameUnderObject { get; protected set; }

		public void CheckLayer()
		{
			if (this.CheckLayerMass != null && this.CheckLayerMass.IsLying && Init3D.SceneSettings != null && Init3D.SceneSettings.SplatMap != null)
			{
				Vector3f vector3f = new Vector3f(this.CheckLayerMass.Position.x, this.CheckLayerMass.Position.y, this.CheckLayerMass.Position.z);
				SplatMap.Layer layer = Init3D.SceneSettings.SplatMap.GetLayer(vector3f);
				if (layer.Name != this.LayerNameUnderObject)
				{
					foreach (Mass mass in this.Masses)
					{
						mass.StaticFrictionFactor = layer.StaticFriction;
						mass.SlidingFrictionFactor = layer.SlidingFriction;
					}
					this.LayerNameUnderObject = layer.Name;
				}
			}
		}

		public SurfaceMaterial SurfaceUnderObject { get; protected set; }

		public virtual void CheckSurface()
		{
			if (this.NeedCheckSurface)
			{
				SurfaceMaterial surfaceMaterial = SurfaceMaterial.NONE;
				foreach (Mass mass in this.Masses)
				{
					if (mass.IsLying)
					{
						if (surfaceMaterial == SurfaceMaterial.NONE)
						{
							RaycastHit raycastHit;
							Physics.Raycast(mass.Position + Vector3.up, Vector3.down, ref raycastHit, 1.1f);
							surfaceMaterial = SurfaceSettings.Instance.GetMaterial(raycastHit, 5);
						}
						if (surfaceMaterial != this.SurfaceUnderObject)
						{
							mass.SetFrictionFactor(surfaceMaterial);
						}
					}
				}
				this.SurfaceUnderObject = surfaceMaterial;
			}
		}

		public virtual void DetectRefLeaks(List<Mass> masses, List<ConnectionBase> connections, List<PhyObject> objects, StringBuilder msg)
		{
			for (int i = 0; i < this.Masses.Count; i++)
			{
				if (!this.Sim.DictMasses.ContainsKey(this.Masses[i].UID))
				{
					masses.Add(this.Masses[i]);
					msg.AppendFormat("{0}.{1} {2}: Masses[{3}] {4}.{5} {6} is not present in the system.\n", new object[]
					{
						this.UID,
						this.Type,
						base.GetType(),
						i,
						this.Masses[i].UID,
						this.Masses[i].Type,
						this.Masses[i].GetType()
					});
				}
			}
			for (int j = 0; j < this.Connections.Count; j++)
			{
				if (!this.Sim.DictConnections.ContainsKey(this.Connections[j].UID))
				{
					connections.Add(this.Connections[j]);
					msg.AppendFormat("{0}.{1} {2}: Connections[{3}] {4}.{5} is not present in the system.\n", new object[]
					{
						this.UID,
						this.Type,
						base.GetType(),
						j,
						this.Connections[j].UID,
						this.Connections[j].GetType()
					});
				}
			}
		}

		public virtual void Sync(PhyObject source)
		{
			this.Masses.Clear();
			this.Connections.Clear();
			for (int i = 0; i < source.Masses.Count; i++)
			{
				this.Masses.Add(this.Sim.DictMasses[source.Masses[i].UID]);
			}
			for (int j = 0; j < source.Connections.Count; j++)
			{
				this.Connections.Add(this.Sim.DictConnections[source.Connections[j].UID]);
			}
		}

		public virtual void UpdateReferences()
		{
		}

		public virtual void SyncMain(PhyObject source)
		{
		}

		public override string ToString()
		{
			return string.Format("{0} Masses: {1} Connections: {2}", Enum.GetName(typeof(PhyObjectType), this.Type), this.Masses.Count, this.Connections.Count);
		}

		public virtual void UpdateSim()
		{
			this.Sim.Masses.AddRange(this.Masses);
			this.Sim.Connections.AddRange(this.Connections);
			this.Sim.RefreshObjectArrays(true);
		}

		public virtual void Remove()
		{
			for (int i = 0; i < this.Masses.Count; i++)
			{
				this.Sim.RemoveMass(this.Masses[i]);
			}
			for (int j = 0; j < this.Connections.Count; j++)
			{
				this.Sim.RemoveConnection(this.Connections[j]);
			}
			this.Masses.Clear();
			this.Connections.Clear();
			this.Sim.RemoveObject(this);
			this.Sim.RefreshObjectArrays(true);
		}

		public virtual void Simulate(float dt)
		{
		}

		public virtual void BeforeSimulationUpdate()
		{
		}

		public float Project(Vector3 v, Vector3 direct)
		{
			float magnitude = Vector3.Project(v, direct).magnitude;
			if (Vector3.Dot(v, direct) >= 0f)
			{
				return magnitude;
			}
			return -magnitude;
		}

		public ConnectedBodiesSystem Sim;

		private static int IIDCounter;

		private bool isKinematic;

		public Mass CheckLayerMass;

		public bool NeedCheckSurface;
	}
}
