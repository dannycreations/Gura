using System;

namespace Phy
{
	public static class ParallelHelperFactory
	{
		public static ParallelHelper<Mass> MassHelper = new ParallelHelper<Mass>();

		public static ParallelHelper<ConnectionBase> ConnectionHelper = new ParallelHelper<ConnectionBase>();
	}
}
