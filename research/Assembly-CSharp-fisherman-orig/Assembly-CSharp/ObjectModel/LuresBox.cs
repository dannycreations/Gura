using System;

namespace ObjectModel
{
	public class LuresBox : Hat
	{
		[JsonConfig]
		public int LineCount { get; set; }

		[JsonConfig]
		public int ReelCount { get; set; }

		[JsonConfig]
		public int ChumCount { get; set; }
	}
}
