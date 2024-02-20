using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Phy
{
	public abstract class ConnectionBase
	{
		protected ConnectionBase(Mass mass1, Mass mass2, int forceUID = -1)
		{
			this.UID = ((forceUID >= 0) ? forceUID : mass1.Sim.NewUID);
			if (forceUID < 0)
			{
				this.IID = ConnectionBase.NewIID;
				mass1.Sim.PhyActionsListener.ConnectionCreated(this);
			}
			else
			{
				this.IID = -ConnectionBase.NewIID;
			}
			this.SetMasses(mass1, mass2);
		}

		public Mass Mass1
		{
			get
			{
				return this._mass1;
			}
			set
			{
				this._mass1 = value;
				this.ConnectionNeedSyncMark();
			}
		}

		public Mass Mass2
		{
			get
			{
				return this._mass2;
			}
			set
			{
				this._mass2 = value;
				this.ConnectionNeedSyncMark();
			}
		}

		public int UID { get; protected set; }

		public int IID { get; private set; }

		public static int NewIID
		{
			get
			{
				return Interlocked.Decrement(ref ConnectionBase.IIDCounter);
			}
		}

		public override int GetHashCode()
		{
			return this.IID;
		}

		public abstract void Solve();

		public float MaxForce { get; set; }

		public virtual void UpdateReferences()
		{
		}

		public virtual void SyncSimUpdate()
		{
		}

		public virtual void DetectRefLeaks(List<Mass> masses, List<ConnectionBase> connections, List<PhyObject> objects, StringBuilder msg)
		{
			if (this.Mass1 != null && !this.Mass1.Sim.DictMasses.ContainsKey(this.Mass1.UID))
			{
				msg.AppendFormat("{0}.{1}: [{2}.{3} {4} --- {5}.{6} {7}] Mass1 is not present in the system.\n", new object[]
				{
					this.UID,
					base.GetType(),
					this.Mass1.UID,
					this.Mass1.Type,
					this.Mass1.GetType(),
					this.Mass2.UID,
					this.Mass2.Type,
					this.Mass2.GetType()
				});
				masses.Add(this.Mass1);
			}
			if (this.Mass2 != null && !this.Mass2.Sim.DictMasses.ContainsKey(this.Mass2.UID))
			{
				msg.AppendFormat("{0}.{1}: [{2}.{3} {4} --- {5}.{6} {7}] Mass2 is not present in the system.\n", new object[]
				{
					this.UID,
					base.GetType(),
					this.Mass1.UID,
					this.Mass1.Type,
					this.Mass1.GetType(),
					this.Mass2.UID,
					this.Mass2.Type,
					this.Mass2.GetType()
				});
				masses.Add(this.Mass2);
			}
			if (this.Mass1 != null && this.Mass2 != null && this.Mass1.Sim != this.Mass2.Sim)
			{
				msg.AppendFormat("{0}.{1}: [{2}.{3} {4} --- {5}.{6} {7}] Mass1 and Mass2 do not belong to a single system.\n", new object[]
				{
					this.UID,
					base.GetType(),
					this.Mass1.UID,
					this.Mass1.Type,
					this.Mass1.GetType(),
					this.Mass2.UID,
					this.Mass2.Type,
					this.Mass2.GetType()
				});
				masses.Add(this.Mass1);
				masses.Add(this.Mass2);
			}
		}

		public virtual void Sync(ConnectionBase source)
		{
			if (this.Mass1.UID != source.Mass1.UID)
			{
				this._mass1 = this._mass1.Sim.DictMasses[source.Mass1.UID];
			}
			if (this.Mass2.UID != source.Mass2.UID)
			{
				this._mass2 = this._mass1.Sim.DictMasses[source.Mass2.UID];
			}
		}

		public void ConnectionNeedSyncMark()
		{
			if (this.Mass1.Sim.PhyActionsListener != null)
			{
				this.Mass1.Sim.PhyActionsListener.ConnectionNeedSyncMark(this.UID);
			}
		}

		public virtual void SetMasses(Mass mass1, Mass mass2 = null)
		{
			if (mass1 != null)
			{
				this._mass1 = mass1;
			}
			if (mass2 != null)
			{
				this._mass2 = mass2;
			}
			this.ConnectionNeedSyncMark();
		}

		public override string ToString()
		{
			return string.Format("#{0} - #{1}", this.Mass1.UID, this.Mass2.UID);
		}

		public virtual float CalculatePotentialEnergy()
		{
			return 0f;
		}

		protected Mass _mass1;

		protected Mass _mass2;

		private static int IIDCounter;

		public Color? TraceColor;

		public float CurrentForceMagnitude;

		public float DeltaLength;
	}
}
