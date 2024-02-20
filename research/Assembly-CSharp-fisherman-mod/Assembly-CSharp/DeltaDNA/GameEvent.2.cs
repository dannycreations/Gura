using System;

namespace DeltaDNA
{
	public class GameEvent : GameEvent<GameEvent>
	{
		public GameEvent(string name)
			: base(name)
		{
		}
	}
}
