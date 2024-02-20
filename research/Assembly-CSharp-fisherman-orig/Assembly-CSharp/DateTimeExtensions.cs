using System;
using I2.Loc;

public static class DateTimeExtensions
{
	public static DateTime StartOfDay(this DateTime theDate)
	{
		return theDate.Date;
	}

	public static DateTime EndOfDay(this DateTime theDate)
	{
		return theDate.Date.AddDays(1.0).AddTicks(-1L);
	}

	public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
	{
		int num = dt.DayOfWeek - startOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return dt.AddDays((double)(-1 * num)).Date;
	}

	public static int GetTimestamp(this DateTime value)
	{
		return (int)value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
	}

	public static string GetTimeFinishInValue(this DateTime dateTime, bool isShortDays = false)
	{
		TimeSpan timeSpan = dateTime - TimeHelper.UtcTime();
		return timeSpan.GetFormated(isShortDays, true);
	}

	public static string GetFormated(this TimeSpan ts, bool isShortDays = false, bool isShowSeconds = true)
	{
		string text = string.Empty;
		if (isShowSeconds)
		{
			text = ((ts.TotalSeconds < 0.0) ? "00:00:00" : string.Format("{0}:{1}:{2}", ts.Hours.ToString("D2"), ts.Minutes.ToString("D2"), ts.Seconds.ToString("D2")));
		}
		else
		{
			text = ((ts.TotalSeconds < 0.0) ? "00:00" : string.Format("{0}:{1}", ts.Hours.ToString("D2"), ts.Minutes.ToString("D2")));
		}
		if (ts.Days <= 0)
		{
			return text;
		}
		int num = ts.Days;
		if (isShortDays && ts.Seconds > 0)
		{
			num++;
		}
		string text2 = string.Format("{0} {1}", num, DateTimeExtensions.GetDaysLocalization(num));
		return (!isShortDays) ? string.Format("{0} {1}", text2, text) : text2;
	}

	public static string GetFormatedMinSec(this TimeSpan ts)
	{
		return (ts.TotalSeconds < 0.0) ? "00:00" : string.Format("{0}:{1}", ts.Minutes.ToString("D2"), ts.Seconds.ToString("D2"));
	}

	public static string GetFormatedSecondsOnly(this TimeSpan ts)
	{
		return (ts.TotalSeconds < 0.0) ? "00" : string.Format("{0}", ts.Seconds.ToString("D2"));
	}

	public static string GetDaysLocalization(int days)
	{
		int num = days % 10;
		if (num == 0)
		{
			return ScriptLocalization.Get("[DaysTravelCaption]");
		}
		string text = days.ToString();
		if (text.Length > 2)
		{
			string text2 = text.Substring(text.Length - 2, 2);
			return DateTimeExtensions.GetDaysLocalization(int.Parse(text2));
		}
		if (text.Length == 2 && days > 20)
		{
			string text3 = text.Substring(text.Length - 1, 1);
			return DateTimeExtensions.GetDaysLocalization(int.Parse(text3));
		}
		if (days >= 2 && days <= 4)
		{
			return ScriptLocalization.Get("DaysTravelCaption2");
		}
		if (days >= 5 && days <= 19)
		{
			return ScriptLocalization.Get("[DaysTravelCaption]");
		}
		if (num == 1)
		{
			return ScriptLocalization.Get("DayCaption");
		}
		return ScriptLocalization.Get("LotDaysCaption");
	}
}
