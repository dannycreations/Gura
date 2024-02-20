using System;
using LeaderboardSRIA.Models;
using LeaderboardSRIA.ViewsHolders;
using ObjectModel;

namespace Leaderboard
{
	public class FishLeaderboard : LeaderboardSRIA
	{
		protected override BaseModel CreateModelFromItem(TopPlayerBase entry, int index)
		{
			return new FishModel
			{
				Data = (entry as TopFish),
				id = index
			};
		}

		protected override BaseVH GetViewHodler()
		{
			return new FishItemVH();
		}
	}
}
