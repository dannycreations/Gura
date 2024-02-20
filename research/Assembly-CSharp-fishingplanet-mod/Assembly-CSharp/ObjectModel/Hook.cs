using System;

namespace ObjectModel
{
	public class Hook : TerminalTackleItem, IHook
	{
		[JsonConfig]
		public float Visibility { get; set; }

		[JsonConfig]
		public float HookSize { get; set; }

		[JsonConfig]
		public float HookingMultiplier { get; set; }

		[JsonConfig]
		public float AutoStrikeChance { get; set; }

		[JsonConfig]
		public float HitchChance { get; set; }

		[JsonConfig]
		public bool HasBarb { get; set; }
	}
}
