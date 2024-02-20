using System;

namespace Phy
{
	public class WobblerTackleObject : RigidBodyTackleObject
	{
		public WobblerTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
		}

		public WobblerTackleObject(ConnectedBodiesSystem sim, WobblerTackleObject source)
			: base(sim, source)
		{
		}

		public override void Sync(PhyObject source)
		{
			base.Sync(source);
		}

		public override Mass HookMass
		{
			get
			{
				return base.Masses[1];
			}
		}

		public Wobbler WobblerMass
		{
			get
			{
				return (base.Masses[0] as Wobbler) ?? (this.cachedRigidBody as Wobbler);
			}
		}

		public override void HookFish(float fishMass, float fishBuoyancy)
		{
			base.HookFish(fishMass, fishBuoyancy);
			this.WobblerMass.DivingEnabled = false;
			this.WobblerMass.WobblingEnabled = false;
		}

		public override void EscapeFish()
		{
			base.EscapeFish();
			this.WobblerMass.DivingEnabled = true;
			this.WobblerMass.WobblingEnabled = true;
		}
	}
}
