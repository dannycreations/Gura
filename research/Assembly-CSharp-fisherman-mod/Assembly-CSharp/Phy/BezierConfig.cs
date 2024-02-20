using System;
using System.Collections.Generic;

namespace Phy
{
	public class BezierConfig
	{
		public IList<Mass> OriginalMasses;

		public IList<Mass> Masses;

		public float FirstSegLength;

		public float LastSegLength;

		public float FullLength;

		public float LengthCorrectionMultiplier;

		public Mass FirstMass;

		public Mass SecondMass;

		public Mass PreLastMass;

		public Mass LastMass;
	}
}
