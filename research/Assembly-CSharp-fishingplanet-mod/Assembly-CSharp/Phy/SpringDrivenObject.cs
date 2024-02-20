using System;

namespace Phy
{
	public class SpringDrivenObject : PhyObject
	{
		public SpringDrivenObject(PhyObjectType type, ConnectedBodiesSystem sim, TackleBehaviour tackle = null)
			: base(type, sim)
		{
			this.Springs = new SpringDrivenObject.SpringsList(this);
			this.Tackle = tackle;
			this.swivelBody = null;
		}

		public SpringDrivenObject(ConnectedBodiesSystem sim, SpringDrivenObject source)
			: base(sim, source)
		{
			this.Springs = new SpringDrivenObject.SpringsList(this);
			this.swivelBody = source.swivelBody;
		}

		public TackleBehaviour Tackle { get; private set; }

		public SpringDrivenObject.SpringsList Springs { get; private set; }

		public void SyncSwivel()
		{
			if (this.Tackle != null && this.Tackle.SwivelObject != null && this.swivelBody != null)
			{
				this.Tackle.SwivelObject.transform.rotation = this.swivelBody.Rotation;
				this.Tackle.SwivelObject.transform.position += this.Tackle.Rod.PositionCorrection(this.swivelBody, true) - this.Tackle.Swivel.center.position;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} Masses: {1} Connections: {2}", Enum.GetName(typeof(PhyObjectType), base.Type), base.Masses.Count, this.Springs.Count);
		}

		public RigidBody swivelBody;

		public class SpringsList
		{
			public SpringsList(PhyObject obj)
			{
				this.obj = obj;
			}

			public Spring this[int index]
			{
				get
				{
					return this.obj.Connections[index] as Spring;
				}
				set
				{
					this.obj.Connections[index] = value;
				}
			}

			public int Count
			{
				get
				{
					return this.obj.Connections.Count;
				}
			}

			public void Add(Spring s)
			{
				this.obj.Connections.Add(s);
			}

			public void Insert(int index, Spring s)
			{
				this.obj.Connections.Insert(index, s);
			}

			public void RemoveAt(int index)
			{
				this.obj.Connections.RemoveAt(index);
			}

			public void Clear()
			{
				this.obj.Connections.Clear();
			}

			private PhyObject obj;
		}
	}
}
