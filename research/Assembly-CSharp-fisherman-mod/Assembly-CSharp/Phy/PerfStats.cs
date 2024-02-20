using System;
using System.Collections.Generic;

namespace Phy
{
	public static class PerfStats
	{
		public static void Start(string counter)
		{
			PerfStats.starts[counter] = DateTime.Now;
		}

		public static void Finish(string counter)
		{
			float num = (float)DateTime.Now.Subtract(PerfStats.starts[counter]).TotalSeconds;
			if (PerfStats.Stats.ContainsKey(counter))
			{
				Dictionary<string, float> stats;
				(stats = PerfStats.Stats)[counter] = stats[counter] + num;
			}
			else
			{
				PerfStats.Stats[counter] = num;
			}
		}

		private static Dictionary<string, DateTime> starts = new Dictionary<string, DateTime>();

		public static Dictionary<string, float> Stats = new Dictionary<string, float>();
	}
}
