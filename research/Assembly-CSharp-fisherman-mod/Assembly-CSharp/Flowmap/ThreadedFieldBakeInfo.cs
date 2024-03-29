﻿using System;
using System.Threading;

namespace Flowmap
{
	internal class ThreadedFieldBakeInfo : ArrayThreadedInfo
	{
		public ThreadedFieldBakeInfo(int start, int length, ManualResetEvent resetEvent, FlowSimulationField[] fields, FlowmapGenerator generator)
			: base(start, length, resetEvent)
		{
			this.fields = fields;
			this.generator = generator;
		}

		public FlowSimulationField[] fields;

		public FlowmapGenerator generator;
	}
}
