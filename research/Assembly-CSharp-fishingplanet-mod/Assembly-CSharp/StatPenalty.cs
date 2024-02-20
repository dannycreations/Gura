using System;
using System.Collections.Generic;

public class StatPenalty
{
	public int LicenseId { get; set; }

	public string LicenseName { get; set; }

	public double Amount { get; set; }

	public string Currency { get; set; }

	public static double GetSumOfPenalty(IList<StatPenalty> penalties, string currency = "SC")
	{
		double num = 0.0;
		for (int i = 0; i < penalties.Count; i++)
		{
			if (penalties[i].Currency == currency)
			{
				num += penalties[i].Amount;
			}
		}
		return num;
	}
}
