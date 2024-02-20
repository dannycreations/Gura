using System;
using Mono.Simd;
using Mono.Simd.Math;

namespace Phy.Verlet
{
	public sealed class VerletFishThrustController : ConnectionBase
	{
		public VerletFishThrustController(AbstractFishBody body)
			: base(body.Mouth, body.Root, -1)
		{
			this.mouth = base.Mass1 as VerletMass;
			this.root = base.Mass2 as VerletMass;
			this.body = body;
		}

		public VerletFishThrustController(Simulation sim, VerletFishThrustController source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.body = source.body;
			this.Sync(source);
		}

		public Vector4f Force
		{
			get
			{
				return this._force;
			}
			set
			{
				this._force = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public bool Reverse
		{
			get
			{
				return this._reverse;
			}
			set
			{
				this._reverse = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			VerletFishThrustController verletFishThrustController = source as VerletFishThrustController;
			if (this.mouth.UID != verletFishThrustController.mouth.UID)
			{
				this.mouth = base.Mass1.Sim.DictMasses[verletFishThrustController.mouth.UID] as VerletMass;
			}
			if (this.root.UID != verletFishThrustController.root.UID)
			{
				this.root = base.Mass1.Sim.DictMasses[verletFishThrustController.root.UID] as VerletMass;
			}
			this.Force = verletFishThrustController.Force;
			this.Reverse = verletFishThrustController.Reverse;
		}

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			this.body = (base.Mass1.Sim as ConnectedBodiesSystem).DictObjects[this.body.UID] as AbstractFishBody;
		}

		public override void SetMasses(Mass mass1, Mass mass2 = null)
		{
			if (mass1 != null)
			{
				base.Mass1 = mass1;
				this.mouth = mass1 as VerletMass;
			}
			if (mass2 != null)
			{
				base.Mass2 = mass2;
				this.root = mass2 as VerletMass;
			}
			base.ConnectionNeedSyncMark();
		}

		public override void Solve()
		{
			Vector4f vector4f;
			vector4f..ctor(this.body.UnderwaterRatio);
			Vector4f vector4f2 = this.Force * vector4f;
			Vector4f vector4f3 = vector4f2 * this.forceDistributeFactor;
			if (!this.Reverse)
			{
				this.mouth.WaterMotor4f = vector4f2 + vector4f3;
				this.root.WaterMotor4f = vector4f3.Negative();
			}
			else
			{
				this.root.WaterMotor4f = vector4f2 + vector4f3;
				this.mouth.WaterMotor4f = vector4f3.Negative();
			}
		}

		private Vector4f _force;

		private bool _reverse;

		public VerletMass mouth;

		public VerletMass root;

		private readonly Vector4f forceDistributeFactor = new Vector4f(0.1f);

		private AbstractFishBody body;
	}
}
