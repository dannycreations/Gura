using System;
using LeaderboardSRIA.Models;
using LeaderboardSRIA.ViewsHolders;
using ObjectModel;

namespace Leaderboard
{
	public class CompetitionsPlayerLeaderboard : LeaderboardSRIA
	{
		protected override BaseModel CreateModelFromItem(TopPlayerBase entry, int index)
		{
			return new CompetitionPlayerModel
			{
				Data = (entry as TopTournamentPlayers),
				id = index
			};
		}

		protected override BaseVH GetViewHodler()
		{
			return new CompetitionPlayerItemVH();
		}
	}
}
