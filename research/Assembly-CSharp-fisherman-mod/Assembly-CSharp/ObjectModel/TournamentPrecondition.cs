using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class TournamentPrecondition
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentPreconditionType PreconditionType { get; set; }

		public int? Level { get; set; }

		public int? TournamentTemplateId { get; set; }

		public int? TournamentPlace { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentTitles? Title { get; set; }

		public bool Match(TournamentTitles? title, int? tournamentPlace, int? level)
		{
			if (this.PreconditionType == TournamentPreconditionType.Title)
			{
				return this.Title == null || this.Title.Value == title;
			}
			if (this.PreconditionType == TournamentPreconditionType.Tournament)
			{
				return this.TournamentPlace == null || (tournamentPlace != null && this.TournamentPlace.Value >= tournamentPlace);
			}
			if (this.PreconditionType == TournamentPreconditionType.MinLevel)
			{
				bool flag2;
				if (this.Level != null)
				{
					bool flag = level != null;
					int? level2 = this.Level;
					flag2 = (flag & (level2 != null)) && level.GetValueOrDefault() >= level2.GetValueOrDefault();
				}
				else
				{
					flag2 = true;
				}
				return flag2;
			}
			if (this.PreconditionType == TournamentPreconditionType.MaxLevel)
			{
				bool flag4;
				if (this.Level != null)
				{
					bool flag3 = level != null;
					int? level3 = this.Level;
					flag4 = (flag3 & (level3 != null)) && level.GetValueOrDefault() <= level3.GetValueOrDefault();
				}
				else
				{
					flag4 = true;
				}
				return flag4;
			}
			return false;
		}
	}
}
