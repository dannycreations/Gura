using System;

namespace Missions
{
	public class Mission2 : BaseMission
	{
		public override bool Update()
		{
			if (base.SomeCondition())
			{
				base.HighligtSomething2(10, true);
				return false;
			}
			return true;
		}
	}
}
