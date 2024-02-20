using System;

namespace ObjectModel
{
	public interface IPlayer : ILevelRank
	{
		string UserId { get; }

		string UserName { get; }

		TPMCharacterModel TpmCharacterModel { get; }

		bool IsReplay { get; }
	}
}
