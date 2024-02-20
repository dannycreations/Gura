using System;

namespace ObjectModel
{
	public class FishPortrait
	{
		public FishTrail[] Trails { get; set; }

		public FishBait[] FishBaits { get; set; }

		public bool IsScript { get; set; }

		public FishBiteTemplate ResultFishBiteTemplate { get; set; }

		public static FishPortrait Default = new FishPortrait
		{
			Trails = new FishTrail[]
			{
				new FishTrail
				{
					Behavior = FishAiBehavior.Hide,
					Probability = 0.3f
				},
				new FishTrail
				{
					Behavior = FishAiBehavior.Escape,
					Probability = 0.1f
				},
				new FishTrail
				{
					Behavior = FishAiBehavior.Float,
					Probability = 0.05f
				},
				new FishTrail
				{
					Behavior = FishAiBehavior.Sink,
					Probability = 0.2f
				},
				new FishTrail
				{
					Behavior = FishAiBehavior.StopFight,
					Probability = 0.5f
				}
			}
		};
	}
}
