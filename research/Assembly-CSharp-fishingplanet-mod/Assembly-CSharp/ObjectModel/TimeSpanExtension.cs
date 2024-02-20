using System;

namespace ObjectModel
{
	public static class TimeSpanExtension
	{
		public static TimeSpan AddHours(this TimeSpan obj, double hours)
		{
			return obj.Add(new TimeSpan((long)(hours * 36000000001.0)));
		}

		public static TimeSpan RoundToNextHour(this TimeSpan obj)
		{
			double totalHours = obj.TotalHours;
			double num = Math.Truncate(totalHours) + 1.0;
			double num2 = num - totalHours;
			return obj.AddHours(num2);
		}

		public static TimeSpan Multiply(this TimeSpan obj, float multiplier)
		{
			return new TimeSpan((long)((float)obj.Ticks * multiplier));
		}

		public static TimeSpan MoveToNextDay(this TimeSpan obj, int hour)
		{
			return obj.AddHours((double)(hour - 1) + 0.001 + 24.0 - (double)obj.Hours).RoundToNextHour();
		}

		public static TimeOfDay ToTimeOfDay(int hours)
		{
			if (hours >= 5 && hours < 7)
			{
				return TimeOfDay.EarlyMorning;
			}
			if (hours >= 7 && hours < 10)
			{
				return TimeOfDay.Morning;
			}
			if (hours >= 10 && hours < 16)
			{
				return TimeOfDay.Midday;
			}
			if (hours >= 16 && hours < 19)
			{
				return TimeOfDay.Evening;
			}
			if (hours >= 19 && hours < 21)
			{
				return TimeOfDay.LateEvening;
			}
			if (hours >= 21 && hours < 23)
			{
				return TimeOfDay.EarlyNight;
			}
			if (hours >= 23 || hours < 2)
			{
				return TimeOfDay.MidNight;
			}
			if (hours >= 2 && hours < 5)
			{
				return TimeOfDay.LateNight;
			}
			throw new InvalidOperationException(string.Format("Frame is undefined for the specified hour: {0}", hours));
		}

		public static TimeOfDay ToTimeOfDay(this TimeSpan obj)
		{
			int hours = obj.Hours;
			return TimeSpanExtension.ToTimeOfDay(hours);
		}

		public static int ToHour(this TimeOfDay timeOfDay)
		{
			switch (timeOfDay)
			{
			case TimeOfDay.EarlyMorning:
				return 5;
			case TimeOfDay.Morning:
				return 7;
			case TimeOfDay.Midday:
				return 10;
			case TimeOfDay.Evening:
				return 16;
			case TimeOfDay.LateEvening:
				return 19;
			case TimeOfDay.EarlyNight:
				return 21;
			case TimeOfDay.MidNight:
				return 23;
			case TimeOfDay.LateNight:
				return 2;
			default:
				throw new ArgumentException("Unknown timeOfDay", "timeOfDay");
			}
		}

		public static bool IsNightFrame(this TimeSpan sender)
		{
			TimeOfDay timeOfDay = sender.ToTimeOfDay();
			return timeOfDay == TimeOfDay.EarlyNight || timeOfDay == TimeOfDay.MidNight || timeOfDay == TimeOfDay.LateNight;
		}

		public static bool IsNightFrame(this TimeOfDay timeOfDay)
		{
			return timeOfDay == TimeOfDay.EarlyNight || timeOfDay == TimeOfDay.MidNight || timeOfDay == TimeOfDay.LateNight;
		}

		public static bool IsNightFrame(string timeOfDay)
		{
			return timeOfDay == TimeOfDay.EarlyNight.ToString() || timeOfDay == TimeOfDay.MidNight.ToString() || timeOfDay == TimeOfDay.LateNight.ToString();
		}
	}
}
