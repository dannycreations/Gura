using System;
using LeaderboardSRIA.Models;
using LeaderboardSRIA.ViewsHolders;
using ObjectModel;

namespace Leaderboard
{
	public class PlayerLeaderboard : LeaderboardSRIA
	{
		protected override BaseModel CreateModelFromItem(TopPlayerBase entry, int index)
		{
			return new PlayerModel
			{
				Data = (entry as TopPlayers),
				id = index
			};
		}

		protected override BaseVH GetViewHodler()
		{
			return new PlayerItemVH();
		}
	}
}
