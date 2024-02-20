using System;

namespace ObjectModel
{
	public struct PlayerDetractorData
	{
		public PlayerDetractorData(Vector3f position, float radius, float duration)
		{
			this = default(PlayerDetractorData);
			this.Position = position;
			this.Radius = radius;
			this.Duration = duration;
		}

		public static int Size
		{
			get
			{
				return 16;
			}
		}

		public Vector3f Position;

		public float Radius;

		public float Duration;
	}
}
