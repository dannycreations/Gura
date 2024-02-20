using System;
using ObjectModel;

public class TournamentFinalResult
{
	public int TournamentId { get; set; }

	public int TemplateId { get; set; }

	public string TournamentName { get; set; }

	public int? LogoBID { get; set; }

	public int? ImageBID { get; set; }

	public int KindId { get; set; }

	public int? StageTypeId { get; set; }

	public int PondId { get; set; }

	public int ParticipantsCount { get; set; }

	public PlayerFinalResult CurrentPlayerResult { get; set; }

	public PlayerFinalResult[] Winners { get; set; }

	public PlayerFinalResult[] SecondaryWinners { get; set; }

	public Tournament Tournament { get; set; }

	public UserCompetitionPublic UserCompetition { get; set; }
}
