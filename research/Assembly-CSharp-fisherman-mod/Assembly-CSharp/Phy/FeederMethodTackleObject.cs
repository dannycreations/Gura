using System;

namespace Phy
{
	public class FeederMethodTackleObject : FeederTackleObject
	{
		public FeederMethodTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, normalMass, hangingMass, buoyancy, sim, tackle)
		{
		}

		public override void HookFish(float fishMass, float fishBuoyancy)
		{
		}

		public override void EscapeFish()
		{
		}

		public override void OnSetFilled(bool isFilled)
		{
			base.OnSetFilled(isFilled);
			if (!isFilled && this.hookStickingConnection != null)
			{
				this.Sim.RemoveConnection(this.hookStickingConnection);
				this.hookStickingConnection = null;
			}
		}

		public Spring hookStickingConnection;
	}
}
