using System;
using ObjectModel;
using ObjectModel.Tournaments;

public class PlayerFinalResult
{
	public string Id { get; set; }

	public string Name { get; set; }

	public int Level { get; set; }

	public bool HasPremium { get; set; }

	public int? AvatarBID { get; set; }

	public double? Score { get; set; }

	public double? SecondaryScore { get; set; }

	public int? Rank { get; set; }

	public int? Place { get; set; }

	public int? TeamPlace { get; set; }

	public int? Rating { get; set; }

	public TournamentTitles TitleGiven { get; set; }

	public TournamentTitles TitleProlonged { get; set; }

	public Reward Reward { get; set; }

	public double? SecondaryRewardScore { get; set; }

	public int? SecondaryRewardFishId { get; set; }

	public string SecondaryRewardFishName { get; set; }

	public SecondaryRewardType? SecondaryRewardType { get; set; }

	public Reward SecondaryReward { get; set; }

	public bool HasParticipated { get; set; }

	public bool IsDisqualified { get; set; }

	public bool IsAdmitedToNextStage { get; set; }
}
