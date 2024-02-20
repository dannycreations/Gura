using System;

namespace ObjectModel
{
	public enum LocalEventType : byte
	{
		Join = 1,
		Leave,
		FishCaught,
		TackleBroken,
		Level,
		Achivement,
		QuestCompleted,
		Chat,
		FireworkLaunched,
		BuoyShared,
		ItemCaught,
		MissionCompleted
	}
}
