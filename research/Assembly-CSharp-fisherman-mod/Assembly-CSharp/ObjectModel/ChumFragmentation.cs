using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ChumFragmentation
	{
		public float SpawnInitPortion { get; set; }

		public float SpawnInterval { get; set; }

		public float SpawnRepeats { get; set; }

		[JsonIgnore]
		public bool IsEmpty
		{
			get
			{
				return this.SpawnInitPortion == 0f && this.SpawnInterval == 0f && this.SpawnRepeats == 0f;
			}
		}

		[JsonIgnore]
		public bool IsInvalid
		{
			get
			{
				return this.SpawnInitPortion == 0f && this.SpawnRepeats == 0f;
			}
		}

		[JsonIgnore]
		public static ChumFragmentation BaseDefault
		{
			get
			{
				return new ChumFragmentation
				{
					SpawnInitPortion = 20f,
					SpawnInterval = 5f,
					SpawnRepeats = 4f
				};
			}
		}

		[JsonIgnore]
		public static ChumFragmentation ModifierDefault
		{
			get
			{
				return new ChumFragmentation
				{
					SpawnInitPortion = 1f,
					SpawnInterval = 1f,
					SpawnRepeats = 1f
				};
			}
		}
	}
}
