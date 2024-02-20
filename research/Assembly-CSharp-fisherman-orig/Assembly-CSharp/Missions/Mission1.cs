using System;

namespace Missions
{
	public class Mission1 : BaseMission
	{
		public override bool Update()
		{
			if (base.SomeCondition())
			{
				base.HighligtSomething("arg example");
				return false;
			}
			return true;
		}
	}
}
