using System;

namespace ObjectModel.Tournaments
{
	public class TournamentGridItem
	{
		public int ItemId { get; set; }

		public int SerieId { get; set; }

		public int RowNumber { get; set; }

		public int ColumnNumber { get; set; }

		public string Type { get; set; }

		public int? TemplateId { get; set; }

		public int? ProductId { get; set; }

		public int? ImageBID { get; set; }

		public int? PanelId { get; set; }
	}
}
