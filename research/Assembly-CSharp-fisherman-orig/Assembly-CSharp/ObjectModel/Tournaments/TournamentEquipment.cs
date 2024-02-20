using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel.Tournaments
{
	public class TournamentEquipment : TournamentRodEquipment
	{
		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] FishCageTypes { get; set; }

		public TackleLevelLimit[] TackleLevelLimits { get; set; }

		[JsonConverter(typeof(ArrayOfEnumsToStringConverterItemSubTypes))]
		public ItemSubTypes[] BoatTypes { get; set; }

		public int[] BoatIds { get; set; }

		public int? MaxRods { get; set; }

		public int? MaxRodTypes { get; set; }

		public bool AllowRodStands { get; set; }

		public int? MaxRodsOnStands { get; set; }

		public List<TournamentRodEquipment> Alternatives { get; set; }
	}
}
