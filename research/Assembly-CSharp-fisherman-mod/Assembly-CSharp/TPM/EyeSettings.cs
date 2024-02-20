using System;

namespace TPM
{
	[Serializable]
	public class EyeSettings
	{
		public float sideMaxAngleDelta;

		public float verticalMaxAngleDelta;

		public float angleSpeed;

		public float minDelay;

		public float maxDelay;

		public float maxDistToPlayer;

		public float maxYawOnPlayer;
	}
}
