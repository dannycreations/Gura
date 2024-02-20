using System;
using System.Collections.Generic;

public class FirstPageResource
{
	public int PondId { get; set; }

	public string HeaderImage { get; set; }

	public string FooterImage { get; set; }

	public string MainImage { get; set; }

	public FirstPageResource.RequestSet LeftPanelSet { get; set; }

	public FirstPageResource.RequestSet RightPanelSet { get; set; }

	public class RequestSet
	{
		public RequestSet()
		{
			this.Caption = "PremiumRightCaption";
		}

		public string Caption { get; set; }

		public RequestTypeSet TypeSet = RequestTypeSet.ItemIdsSet;

		public List<int> Ids;
	}
}
