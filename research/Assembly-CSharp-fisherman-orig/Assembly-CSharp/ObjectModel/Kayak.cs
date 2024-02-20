using System;

namespace ObjectModel
{
	public class Kayak : Boat
	{
		[JsonConfig]
		public StaminaSettings StaminaSettings { get; set; }

		[JsonConfig]
		public float OarAreaMultiplier { get; set; }

		[JsonConfig]
		public float FakeMovementForce { get; set; }

		[JsonConfig]
		public float FakeTurnForce { get; set; }
	}
}
