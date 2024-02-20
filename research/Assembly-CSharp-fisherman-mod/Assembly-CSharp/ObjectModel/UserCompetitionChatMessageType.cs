using System;

namespace ObjectModel
{
	public enum UserCompetitionChatMessageType
	{
		UgcPlayerRegistered,
		UgcPlayerUnregistered,
		UgcPlayerRemovedByHost,
		UgcPlayerApproved,
		UgcPlayerUnapproved,
		UgcPlayerCaughtFish,
		UgcCompetitionRemoved,
		UgcPlayerExchanged,
		UgcPlayerMovedByHost,
		UgcPlayerLockedInTeam,
		UgcPlayerUnlockedInTeam,
		UgcCompetitionAutoArranged,
		UgcCompetitionStartRequested,
		UgcCompetitionStartUnrequested,
		UgcPlayerMoved
	}
}
