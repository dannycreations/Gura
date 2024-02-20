using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class Feeder : Sinker, IFeeder
	{
		public float Capacity { get; set; }

		public float CellSize { get; set; }

		public ChumFragmentation ChumFragmentation { get; set; }

		public float DissolveTime { get; set; }

		[JsonIgnore]
		public virtual float ChumCapacity
		{
			get
			{
				return this.Capacity;
			}
		}
	}
}
