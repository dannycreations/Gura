using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using I2.Loc;
using ObjectModel;
using UnitsNet;

public static class MeasuringSystemManager
{
	public static MeasuringSystem CurrentMeasuringSystem
	{
		get
		{
			return MeasuringSystemManager._currentMeasuringSystem;
		}
	}

	public static void ChangeMeasuringSystem()
	{
		Language getCurrentLanguage = ChangeLanguage.GetCurrentLanguage;
		switch (getCurrentLanguage.Id)
		{
		case 1:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.Imperial);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			break;
		case 2:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.RussianMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
			break;
		case 3:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			break;
		case 4:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			break;
		case 5:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
			break;
		case 6:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
			break;
		case 7:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.RussianMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("uk-UA");
			break;
		case 8:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
			break;
		case 9:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
			break;
		case 10:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
			break;
		case 11:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
			break;
		case 12:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
			break;
		case 13:
			MeasuringSystemManager.SetMeasuringSystem(MeasuringSystem.EnglishMetric);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-HK");
			break;
		}
	}

	private static void SetMeasuringSystem(MeasuringSystem system)
	{
		MeasuringSystemManager._currentMeasuringSystem = system;
	}

	public static float LineLength(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)Length.FromMeters((double)metricValue).Feet;
		}
		if (currentMeasuringSystem != MeasuringSystem.RussianMetric && currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return metricValue;
	}

	public static float LineBackLength(float value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)Length.FromFeet((double)value).Meters;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return (float)Length.FromMeters((double)value).Meters;
	}

	public static string LineLengthSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "ft";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "м";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "m";
	}

	public static float LineLeashLength(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)Length.FromMeters((double)metricValue).Inches;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return (float)Length.FromMeters((double)metricValue).Centimeters;
	}

	public static string ToStringCentimeters(float value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("0.00");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public static float LineLeashBackLength(float value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)Length.FromInches((double)value).Meters;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return (float)Length.FromCentimeters((double)value).Meters;
	}

	public static string LineLeashLengthSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "inch";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "см";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "cm";
	}

	public static string Seconds()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "с";
		}
		if (currentMeasuringSystem != MeasuringSystem.Imperial && currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "s";
	}

	public static float FishLength(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)Length.FromMeters((double)metricValue).Inches;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return (float)Length.FromMeters((double)metricValue).Centimeters;
	}

	public static string FishLengthSufix()
	{
		return MeasuringSystemManager.LineLeashLengthSufix();
	}

	public static float LineLeashMaxLength
	{
		get
		{
			MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
			if (currentMeasuringSystem == MeasuringSystem.Imperial)
			{
				return 98f;
			}
			if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
			{
				return 98f;
			}
			return 250f;
		}
	}

	public static float LineLeashMinLength
	{
		get
		{
			MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
			if (currentMeasuringSystem == MeasuringSystem.Imperial)
			{
				return 3f;
			}
			if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
			{
				return 3f;
			}
			return 10f;
		}
	}

	public static string[] FormatWeight(double[] weights)
	{
		return (from p in weights.ToList<double>()
			select string.Format("{0} {1}", MeasuringSystemManager.FishWeight((float)p), MeasuringSystemManager.FishWeightSufix())).ToArray<string>();
	}

	public static float FishWeight(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)Mass.FromKilograms((double)metricValue).Pounds;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return metricValue;
	}

	public static string FishWeightSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "lb";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "кг";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "kg";
	}

	public static float Kilograms2Oz(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return 35.274f * metricValue;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return 35.274f * metricValue;
	}

	public static float Kilograms2Grams(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return 35.274f * metricValue;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return metricValue * 1000f;
	}

	public static string ToStringGrams(float value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("0.00");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString("####");
	}

	public static float Grams2Kilograms(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return metricValue / 35.274f;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return metricValue / 1000f;
	}

	public static string GramsOzWeightSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "Oz";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "г";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "g";
	}

	public static string QuiverTipWeightSufix()
	{
		return ScriptLocalization.Get("OunceTranslation");
	}

	public static float Temperature(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)global::UnitsNet.Temperature.FromDegreesCelsius((double)metricValue).DegreesFahrenheit;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return metricValue;
	}

	public static string TemperatureSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "°F";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "°C";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "°C";
	}

	public static float Speed(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)global::UnitsNet.Speed.FromMetersPerSecond((double)metricValue).MilesPerHour;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return (float)global::UnitsNet.Speed.FromMetersPerSecond((double)metricValue).KilometersPerHour;
	}

	public static string SpeedSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "mph";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "км/ч";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "km/h";
	}

	public static float WindSpeed(float metricValue)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return (float)global::UnitsNet.Speed.FromMetersPerSecond((double)metricValue).MilesPerHour;
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric && currentMeasuringSystem != MeasuringSystem.RussianMetric)
		{
			throw new MeasuringConvertationException();
		}
		return metricValue;
	}

	public static string WindSpeedSufix()
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return "mph";
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return "м/с";
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return "m/s";
	}

	public static string GetPreassureIcon(WeatherDesc weather)
	{
		string text = string.Empty;
		string text2 = weather.Pressure.ToUpper();
		if (text2 != null)
		{
			if (!(text2 == "HIGH"))
			{
				if (!(text2 == "MEDIUM"))
				{
					if (text2 == "LOW")
					{
						text = "\ue616";
					}
				}
				else
				{
					text = "\ue615";
				}
			}
			else
			{
				text = "\ue614";
			}
		}
		return text;
	}

	public static string TimeString(DateTime value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return value.ToString("HH:mm");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString("HH:mm");
	}

	public static string LongTimeString(DateTime value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("h:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return value.ToString("HH:mm:ss");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString("HH:mm:ss");
	}

	public static string DateTimeString(DateTime value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("MM/dd/yyyy h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return value.ToString("dd/MM/yyyy HH:mm");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString("dd/MM/yyyy HH:mm");
	}

	public static string DateTimeShortString(DateTime value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("MM/dd/yyyy", CultureInfo.CreateSpecificCulture("en-US"));
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return value.ToString("dd/MM/yyyy");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString("dd/MM/yyyy");
	}

	public static string DateTimeShortWithoutYear(DateTime value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("MM/dd", CultureInfo.CreateSpecificCulture("en-US"));
		}
		if (currentMeasuringSystem == MeasuringSystem.RussianMetric)
		{
			return value.ToString("dd/MM");
		}
		if (currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		return value.ToString("dd/MM");
	}

	public static string GetShortDayCaption(DateTime date)
	{
		string text = string.Empty;
		switch (date.DayOfWeek)
		{
		case DayOfWeek.Sunday:
			text = ScriptLocalization.Get("SundayShortCaption");
			break;
		case DayOfWeek.Monday:
			text = ScriptLocalization.Get("MondayShortCaption");
			break;
		case DayOfWeek.Tuesday:
			text = ScriptLocalization.Get("TuesdayShortCaption");
			break;
		case DayOfWeek.Wednesday:
			text = ScriptLocalization.Get("WednesdayShortCaption");
			break;
		case DayOfWeek.Thursday:
			text = ScriptLocalization.Get("ThursdayShortCaption");
			break;
		case DayOfWeek.Friday:
			text = ScriptLocalization.Get("FridayShortCaption");
			break;
		case DayOfWeek.Saturday:
			text = ScriptLocalization.Get("SaturdayShortCaption");
			break;
		}
		return text;
	}

	public static string GetFullDayCaption(DateTime date)
	{
		string text = string.Empty;
		switch (date.DayOfWeek)
		{
		case DayOfWeek.Sunday:
			text = ScriptLocalization.Get("SundayFullCaption");
			break;
		case DayOfWeek.Monday:
			text = ScriptLocalization.Get("MondayFullCaption");
			break;
		case DayOfWeek.Tuesday:
			text = ScriptLocalization.Get("TuesdayFullCaption");
			break;
		case DayOfWeek.Wednesday:
			text = ScriptLocalization.Get("WednesdayFullCaption");
			break;
		case DayOfWeek.Thursday:
			text = ScriptLocalization.Get("ThursdayFullCaption");
			break;
		case DayOfWeek.Friday:
			text = ScriptLocalization.Get("FridayFullCaption");
			break;
		case DayOfWeek.Saturday:
			text = ScriptLocalization.Get("SaturdayFullCaption");
			break;
		}
		return text;
	}

	public static string GetHourString(DateTime value)
	{
		MeasuringSystem currentMeasuringSystem = MeasuringSystemManager._currentMeasuringSystem;
		if (currentMeasuringSystem == MeasuringSystem.Imperial)
		{
			return value.ToString("h tt", CultureInfo.CreateSpecificCulture("en-US"));
		}
		if (currentMeasuringSystem != MeasuringSystem.RussianMetric && currentMeasuringSystem != MeasuringSystem.EnglishMetric)
		{
			throw new MeasuringConvertationException();
		}
		if (value.Hour == 0)
		{
			return "24";
		}
		return value.ToString("HH");
	}

	public static double GetMaxCoins(string currency)
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return 0.0;
		}
		return (!(currency == "SC")) ? PhotonConnectionFactory.Instance.Profile.GoldCoins : PhotonConnectionFactory.Instance.Profile.SilverCoins;
	}

	public static string GetCurrencyIcon(string currencyName)
	{
		string text = string.Empty;
		if (currencyName == "SC")
		{
			text = "\ue62b";
		}
		if (currencyName == "GC")
		{
			text = "\ue62c";
		}
		return text;
	}

	public static bool ContainsIconFromWeb(string str)
	{
		return str.Contains(string.Format("${0}$", "SC")) || str.Contains(string.Format("${0}$", "GC")) || str.Contains("$EXP$");
	}

	public static string GetIconFromWeb(string str, int icoSize)
	{
		string text = "<size={0}>{1}</size>";
		string text2 = string.Format(text, icoSize, MeasuringSystemManager.GetCurrencyIcon("SC"));
		string text3 = string.Format(text, icoSize, MeasuringSystemManager.GetCurrencyIcon("GC"));
		string text4 = string.Format(text, icoSize, "\ue724");
		str = str.Replace(string.Format("${0}$", "SC"), text2);
		str = str.Replace(string.Format("${0}$", "GC"), text3);
		str = str.Replace("$EXP$", text4);
		return str;
	}

	public static string GetCurrencyName(string currencyName)
	{
		string text = string.Empty;
		if (currencyName == "SC")
		{
			text = ScriptLocalization.Get("MoneysButtonPopup");
		}
		if (currencyName == "GC")
		{
			text = ScriptLocalization.Get("GoldsButtonPopup");
		}
		return text;
	}

	public static string GetTournamentPrimaryScoreValueToString(TournamentBase tournament, TournamentResult originalValue)
	{
		return MeasuringSystemManager.GetTournamentScoreValueToString(tournament.PrimaryScoring.ScoringType, originalValue.Score, tournament.PrimaryScoring.TotalScoreKind, "3");
	}

	public static string GetTournamentSecondaryScoreValueToString(TournamentBase tournament, TournamentResult originalValue)
	{
		return MeasuringSystemManager.GetTournamentScoreValueToString(tournament.SecondaryScoring.ScoringType, originalValue.SecondaryScore, tournament.SecondaryScoring.TotalScoreKind, "3");
	}

	public static string GetTournamentScoreValueToString(TournamentScoreType scoringType, float? value, TournamentTotalScoreKind totalScoreKind, string formatNum = "3")
	{
		if (value == null)
		{
			return "-";
		}
		if (scoringType == TournamentScoreType.TotalLengthTop3 || scoringType == TournamentScoreType.LongestFish || scoringType == TournamentScoreType.TotalLength)
		{
			return MeasuringSystemManager.FishLength(value.Value / 100f).ToString(string.Format("N{0}", formatNum));
		}
		if (scoringType == TournamentScoreType.TotalScore)
		{
			return value.Value.ToString((totalScoreKind != TournamentTotalScoreKind.ScorePerFish) ? "F4" : "F0");
		}
		if (scoringType == TournamentScoreType.TotalWeightByLineMaxLoad)
		{
			return value.Value.ToString(string.Format("F{0}", formatNum));
		}
		bool flag = scoringType == TournamentScoreType.TotalWeight || scoringType == TournamentScoreType.BestWeightMatch || scoringType == TournamentScoreType.BiggestFish || scoringType == TournamentScoreType.SmallestFish || scoringType == TournamentScoreType.BiggestSizeDiff;
		return (!flag) ? value.Value.ToString("F0") : MeasuringSystemManager.FishWeight(value.Value).ToString(string.Format("F{0}", formatNum));
	}

	public const float LineLeashMinLengthCm = 10f;

	public const float LineLeashMaxLengthCm = 250f;

	public const float Oz = 35.274f;

	public const float Inches = 0.393701f;

	public const float Pound = 0.45359236f;

	private static MeasuringSystem _currentMeasuringSystem;
}
