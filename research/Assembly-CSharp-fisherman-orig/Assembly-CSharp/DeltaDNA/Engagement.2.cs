using System;

namespace DeltaDNA
{
	public class Engagement : Engagement<Engagement>
	{
		public Engagement(string decisionPoint)
			: base(decisionPoint)
		{
		}
	}
}
