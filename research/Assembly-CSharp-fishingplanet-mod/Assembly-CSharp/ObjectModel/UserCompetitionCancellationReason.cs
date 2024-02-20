using System;

namespace ObjectModel
{
	public enum UserCompetitionCancellationReason
	{
		ReasonCompetitionRemovedBecauseInactivity,
		ReasonCompetitionRemovedByHost,
		ReasonPlayerRemovedByHost,
		ReasonCompetitionUnpublishedByHost,
		ReasonPlayerNotReady,
		ReasonCompetitionCantStartOutdated,
		ReasonCompetitionCantStartParticipantsCount,
		ReasonCompetitionCantStartUpcomingRelease,
		ReasonCompetitionCantStartGeneral,
		ReasonCompetitionReverted,
		ReasonCompetitionCanceled
	}
}
