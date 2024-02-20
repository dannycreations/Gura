using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public abstract class Simulation : ISimulation
	{
		protected Simulation()
		{
			this.Masses = new List<Mass>();
			this.Connections = new List<ConnectionBase>();
			this.IterationsCounter = 0U;
			this.paddingMasses = new Mass[]
			{
				new Mass(this, 0f, Vector3.zero, Mass.MassType.Padding),
				new Mass(this, 0f, Vector3.zero, Mass.MassType.Padding),
				new Mass(this, 0f, Vector3.zero, Mass.MassType.Padding)
			};
		}

		protected Simulation(IEnumerable<Mass> masses)
		{
			this.Masses = new List<Mass>(masses);
			this.paddingMasses = new Mass[]
			{
				new Mass(this, 0f, Vector3.zero, Mass.MassType.Padding),
				new Mass(this, 0f, Vector3.zero, Mass.MassType.Padding),
				new Mass(this, 0f, Vector3.zero, Mass.MassType.Padding)
			};
		}

		public int NewUID
		{
			get
			{
				return this._uidcounter++;
			}
		}

		public List<Mass> Masses { get; private set; }

		public List<ConnectionBase> Connections { get; private set; }

		public float FrameProgress { get; private set; }

		public float FrameDeltaTime { get; set; }

		public int FrameIteration { get; private set; }

		public int NumOfIterations { get; private set; }

		public float InternalTime
		{
			get
			{
				return this.IterationsCounter * 0.0004f;
			}
		}

		public Vector3 VisualPositionOffset
		{
			get
			{
				return this._visualPositionOffset;
			}
			set
			{
				this._visualPositionOffset = value;
				foreach (Mass mass in this.Masses)
				{
					mass.VisualPositionOffset = value;
				}
			}
		}

		public virtual void InitProfiler(IPhyProfiler PhyProfiler)
		{
			this.PhyProfiler = PhyProfiler;
		}

		protected virtual void writeProfilerFrame()
		{
		}

		public virtual void Clear()
		{
			if (this.PhyActionsListener != null)
			{
				this.PhyActionsListener.StructureCleared();
			}
			this.ArrayMassesLength = 0;
			if (this.DictMasses != null)
			{
				this.DictMasses.Clear();
			}
			this.Masses.Clear();
		}

		protected virtual void Reset()
		{
			for (int i = 0; i < this.ArrayMassesLength; i++)
			{
				this.ArrayMasses[i].Reset();
			}
		}

		protected abstract void Solve();

		private void SimulateHelper()
		{
			while (!this.abortHelper)
			{
				this.mainHandle.WaitOne();
				for (int i = this.ArrayMassesLength / 2; i < this.ArrayMassesLength; i++)
				{
					this.ArrayMasses[i].Simulate();
				}
				this.helperHandle.Set();
			}
		}

		public void AbortHelpers()
		{
			this.abortHelper = true;
			this.mainHandle.Set();
		}

		public void StartHelpers()
		{
		}

		protected virtual void Simulate(float dt)
		{
			for (int i = 0; i < this.ArrayMassesLength; i++)
			{
				this.ArrayMasses[i].Simulate();
			}
		}

		protected virtual void Operate(float dt)
		{
			this.Reset();
			this.Solve();
			this.Simulate(dt);
			this.IterationsCounter += 1U;
		}

		public virtual void Update(float dt)
		{
			this.NumOfIterations = (int)(dt / 0.0004f) + 1;
			this.FrameDeltaTime = dt;
			if (this.NumOfIterations != 0)
			{
				dt /= (float)this.NumOfIterations;
			}
			for (int i = 0; i < this.NumOfIterations; i++)
			{
				this.FrameIteration = i;
				this.FrameProgress = (float)(i + 1) / (float)this.NumOfIterations;
				this.Operate(dt);
			}
		}

		private void refreshMassesArray()
		{
			if (this.ArrayMasses == null)
			{
				this.ArrayMasses = new Mass[1024];
			}
			if (this.ArrayMassesLength > this.Masses.Count)
			{
				for (int i = this.Masses.Count; i < this.ArrayMassesLength; i++)
				{
					this.ArrayMasses[i] = null;
				}
			}
			this.ArrayMassesLength = this.Masses.Count;
			int num = 0;
			for (int j = 0; j < this.Masses.Count; j++)
			{
				if (!this.Masses[j].DisableSimulation && (this.Masses[j].SourceMass == null || !this.Masses[j].SourceMass.DisableSimulation))
				{
					this.ArrayMasses[num] = this.Masses[j];
					num++;
				}
			}
			this.ArrayMassesLength = num;
			for (int k = 0; k < (4 - this.ArrayMassesLength % 4) % 4; k++)
			{
				this.ArrayMasses[this.ArrayMassesLength + k] = this.paddingMasses[k];
			}
		}

		public void ModifyObjectArrays(IList<Mass> createdMasses, IList<Mass> destroyedMasses)
		{
			this.refreshMassesArray();
			this.ModifyMassesDict(createdMasses, destroyedMasses);
		}

		public void ModifyMassesDict(IList<Mass> createdMasses, IList<Mass> destroyedMasses)
		{
			if (this.DictMasses == null)
			{
				this.DictMasses = new Dictionary<int, Mass>();
			}
			for (int i = 0; i < destroyedMasses.Count; i++)
			{
				this.DictMasses.Remove(destroyedMasses[i].UID);
			}
			for (int j = 0; j < createdMasses.Count; j++)
			{
				Mass mass = createdMasses[j];
				this.DictMasses[mass.UID] = mass;
			}
		}

		public virtual void RefreshObjectArrays(bool updateDict = true)
		{
			this.refreshMassesArray();
			if (this.DictMasses == null)
			{
				this.DictMasses = new Dictionary<int, Mass>();
			}
			if (updateDict)
			{
				this._dictRemoveMasses.Clear();
				foreach (int num in this.DictMasses.Keys)
				{
					this._dictRemoveMasses.Add(num, null);
				}
				for (int i = 0; i < this.ArrayMassesLength; i++)
				{
					if (!this.DictMasses.ContainsKey(this.ArrayMasses[i].UID))
					{
						this.DictMasses[this.ArrayMasses[i].UID] = this.ArrayMasses[i];
					}
					else
					{
						this._dictRemoveMasses.Remove(this.ArrayMasses[i].UID);
					}
				}
				foreach (int num2 in this._dictRemoveMasses.Keys)
				{
					this.DictMasses.Remove(num2);
				}
			}
		}

		public void RemoveMass(Mass m)
		{
			this.Masses.Remove(m);
			this.PhyActionsListener.MassDestroyed(m);
		}

		public virtual void SetVelocity(Vector3 velocity)
		{
		}

		public virtual void SetAngularVelocity(Vector3 velocity)
		{
		}

		public void SyncMasses()
		{
		}

		public virtual float CalculateSystemKineticEnergy()
		{
			float num = 0f;
			for (int i = 0; i < this.Masses.Count; i++)
			{
				num += 0.5f * this.Masses[i].MassValue * this.Masses[i].Velocity4f.SqrMagnitude();
			}
			return num;
		}

		public virtual float CalculateSystemPotentialEnergy()
		{
			float num = 0f;
			for (int i = 0; i < this.Connections.Count; i++)
			{
				num += this.Connections[i].CalculatePotentialEnergy();
			}
			return num;
		}

		public bool IsMain;

		public const int MassesMaxCount = 1024;

		public const float GroundCacheRadius = 125f;

		public const int GroundCacheInitDepth = 5;

		public const int GroundCacheMaxDepth = 8;

		public const float TimeQuant = 0.0004f;

		public static readonly Vector4f TimeQuant4f = new Vector4f(0.0004f);

		public static readonly Vector4f InvTimeQuant4f = new Vector4f(2500f);

		private int _uidcounter;

		public IPhyActionsListener PhyActionsListener;

		public IPhyProfiler PhyProfiler;

		protected Mass[] ArrayMasses;

		private Mass[] paddingMasses;

		protected int ArrayMassesLength;

		public Dictionary<int, Mass> DictMasses;

		public uint IterationsCounter;

		private Thread simulateHelperThread;

		private Vector3 _visualPositionOffset;

		private volatile bool abortHelper;

		private EventWaitHandle mainHandle = new AutoResetEvent(false);

		private EventWaitHandle helperHandle = new AutoResetEvent(false);

		private Dictionary<int, object> _dictRemoveMasses = new Dictionary<int, object>(1024);
	}
}
