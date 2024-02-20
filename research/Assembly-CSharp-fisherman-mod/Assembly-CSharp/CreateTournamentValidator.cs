using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;

public class CreateTournamentValidator
{
	public void SetComponents(List<UgcConsts.Components> forCheck)
	{
		this._forCheck = forCheck;
	}

	public List<UgcConsts.Components> Check(UserCompetition competition)
	{
		List<UgcConsts.Components> errors = new List<UgcConsts.Components>();
		this._forCheck.ForEach(delegate(UgcConsts.Components p)
		{
			if (!this.Check(competition, p))
			{
				errors.Add(p);
			}
		});
		return errors;
	}

	public bool Check(UserCompetition competition, UgcConsts.Components p)
	{
		if (p == UgcConsts.Components.EntryFee && competition.EntranceFee == null)
		{
			return false;
		}
		if (p == UgcConsts.Components.InitialPrizePool && (competition.HostEntranceFee == null || competition.RewardScheme == UserCompetitionRewardScheme.None))
		{
			return false;
		}
		if (p == UgcConsts.Components.Level && (competition.MinLevel == null || competition.MaxLevel == null))
		{
			return false;
		}
		if (p == UgcConsts.Components.Duration && competition.Duration == UserCompetitionDuration.None)
		{
			return false;
		}
		if (p == UgcConsts.Components.TimeAdnWeather && (string.IsNullOrEmpty(competition.WeatherName) || competition.InGameStartHour == null))
		{
			return false;
		}
		if (p == UgcConsts.Components.Fish)
		{
			return this.CheckArray<string>(competition.FishScore);
		}
		if (p == UgcConsts.Components.Scoring && competition.FishSource == TournamentFishSource.Catch)
		{
			return false;
		}
		if (p == UgcConsts.Components.Equipment)
		{
			return this.CheckArray<UserCompetitionEquipmentAllowed>(competition.DollEquipment);
		}
		return p != UgcConsts.Components.Rulesets || competition.ScoringType != TournamentScoreType.TotalWeight;
	}

	private bool CheckArray<T>(T[] data)
	{
		return data != null && data.Length > 0;
	}

	private List<UgcConsts.Components> _forCheck;
}
