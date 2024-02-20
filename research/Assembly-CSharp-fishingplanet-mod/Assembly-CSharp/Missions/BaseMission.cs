using System;

namespace Missions
{
	public abstract class BaseMission
	{
		public virtual void Init(MissionsController controller)
		{
			this._controller = controller;
		}

		public abstract bool Update();

		public bool SomeCondition()
		{
			return false;
		}

		public void HighligtSomething(string arg)
		{
			this._controller.HighlightSomething(arg);
		}

		public void HighligtSomething2(int id, bool flag)
		{
			this._controller.HighlightSomething2(id, flag);
		}

		private MissionsController _controller;
	}
}
