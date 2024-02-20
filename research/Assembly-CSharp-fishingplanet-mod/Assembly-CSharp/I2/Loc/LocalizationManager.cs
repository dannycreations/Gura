using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ArabicSupport;
using UnityEngine;

namespace I2.Loc
{
	public static class LocalizationManager
	{
		public static string CurrentLanguage
		{
			get
			{
				LocalizationManager.InitializeIfNeeded();
				return LocalizationManager.mCurrentLanguage;
			}
			set
			{
				string supportedLanguage = LocalizationManager.GetSupportedLanguage(value);
				if (!string.IsNullOrEmpty(supportedLanguage) && LocalizationManager.mCurrentLanguage != supportedLanguage)
				{
					LocalizationManager.SetLanguageAndCode(supportedLanguage, LocalizationManager.GetLanguageCode(supportedLanguage), true, false);
				}
			}
		}

		public static string CurrentLanguageCode
		{
			get
			{
				LocalizationManager.InitializeIfNeeded();
				return LocalizationManager.mLanguageCode;
			}
			set
			{
				if (LocalizationManager.mLanguageCode != value)
				{
					string languageFromCode = LocalizationManager.GetLanguageFromCode(value);
					if (!string.IsNullOrEmpty(languageFromCode))
					{
						LocalizationManager.SetLanguageAndCode(languageFromCode, value, true, false);
					}
				}
			}
		}

		public static string CurrentRegion
		{
			get
			{
				string currentLanguage = LocalizationManager.CurrentLanguage;
				int num = currentLanguage.IndexOfAny("/\\".ToCharArray());
				if (num > 0)
				{
					return currentLanguage.Substring(num + 1);
				}
				num = currentLanguage.IndexOfAny("[(".ToCharArray());
				int num2 = currentLanguage.LastIndexOfAny("])".ToCharArray());
				if (num > 0 && num != num2)
				{
					return currentLanguage.Substring(num + 1, num2 - num - 1);
				}
				return string.Empty;
			}
			set
			{
				string text = LocalizationManager.CurrentLanguage;
				int num = text.IndexOfAny("/\\".ToCharArray());
				if (num > 0)
				{
					LocalizationManager.CurrentLanguage = text.Substring(num + 1) + value;
					return;
				}
				num = text.IndexOfAny("[(".ToCharArray());
				int num2 = text.LastIndexOfAny("])".ToCharArray());
				if (num > 0 && num != num2)
				{
					text = text.Substring(num);
				}
				LocalizationManager.CurrentLanguage = text + "(" + value + ")";
			}
		}

		public static string CurrentRegionCode
		{
			get
			{
				string currentLanguageCode = LocalizationManager.CurrentLanguageCode;
				int num = currentLanguageCode.IndexOfAny(" -_/\\".ToCharArray());
				return (num >= 0) ? currentLanguageCode.Substring(num + 1) : string.Empty;
			}
			set
			{
				string text = LocalizationManager.CurrentLanguageCode;
				int num = text.IndexOfAny(" -_/\\".ToCharArray());
				if (num > 0)
				{
					text = text.Substring(0, num);
				}
				LocalizationManager.CurrentLanguageCode = text + "-" + value;
			}
		}

		private static void InitializeIfNeeded()
		{
			if (string.IsNullOrEmpty(LocalizationManager.mCurrentLanguage))
			{
				LocalizationManager.UpdateSources();
				LocalizationManager.SelectStartupLanguage();
			}
		}

		public static void SetLanguageAndCode(string LanguageName, string LanguageCode, bool RememberLanguage = true, bool Force = false)
		{
			if (LocalizationManager.mCurrentLanguage != LanguageName || LocalizationManager.mLanguageCode != LanguageCode || Force)
			{
				if (RememberLanguage)
				{
					PlayerPrefs.SetString("I2 Language", LanguageName);
				}
				LocalizationManager.mCurrentLanguage = LanguageName;
				LocalizationManager.mLanguageCode = LanguageCode;
				LocalizationManager.IsRight2Left = LocalizationManager.IsRTL(LocalizationManager.mLanguageCode);
				LocalizationManager.LocalizeAll(Force);
			}
		}

		private static void SelectStartupLanguage()
		{
			string @string = PlayerPrefs.GetString("I2 Language", string.Empty);
			string text = Application.systemLanguage.ToString();
			if (text == "ChineseSimplified")
			{
				text = "Chinese (Simplified)";
			}
			if (text == "ChineseTraditional")
			{
				text = "Chinese (Traditional)";
			}
			if (LocalizationManager.HasLanguage(@string, true, false))
			{
				LocalizationManager.CurrentLanguage = @string;
				return;
			}
			string supportedLanguage = LocalizationManager.GetSupportedLanguage(text);
			if (!string.IsNullOrEmpty(supportedLanguage))
			{
				LocalizationManager.SetLanguageAndCode(supportedLanguage, LocalizationManager.GetLanguageCode(supportedLanguage), false, false);
				return;
			}
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				if (LocalizationManager.Sources[i].mLanguages.Count > 0)
				{
					LocalizationManager.SetLanguageAndCode(LocalizationManager.Sources[i].mLanguages[0].Name, LocalizationManager.Sources[i].mLanguages[0].Code, false, false);
					return;
				}
				i++;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event LocalizationManager.OnLocalizeCallback OnLocalizeEvent;

		public static string GetTermTranslation(string Term)
		{
			return LocalizationManager.GetTermTranslation(Term, LocalizationManager.IsRight2Left, 0);
		}

		public static string GetTermTranslation(string Term, bool FixForRTL)
		{
			return LocalizationManager.GetTermTranslation(Term, FixForRTL, 0);
		}

		public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL)
		{
			string text;
			if (LocalizationManager.TryGetTermTranslation(Term, out text, FixForRTL, maxLineLengthForRTL))
			{
				return text;
			}
			return string.Empty;
		}

		public static bool TryGetTermTranslation(string Term, out string Translation)
		{
			return LocalizationManager.TryGetTermTranslation(Term, out Translation, false, 0);
		}

		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL)
		{
			return LocalizationManager.TryGetTermTranslation(Term, out Translation, FixForRTL, 0);
		}

		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL, int maxLineLengthForRTL)
		{
			Translation = string.Empty;
			if (string.IsNullOrEmpty(Term))
			{
				return false;
			}
			LocalizationManager.InitializeIfNeeded();
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				if (LocalizationManager.Sources[i].TryGetTermTranslation(Term, out Translation))
				{
					if (LocalizationManager.IsRight2Left && FixForRTL)
					{
						Translation = LocalizationManager.ApplyRTLfix(Translation, maxLineLengthForRTL);
					}
					return true;
				}
				i++;
			}
			return false;
		}

		public static string ApplyRTLfix(string line)
		{
			return LocalizationManager.ApplyRTLfix(line, 0);
		}

		public static string ApplyRTLfix(string line, int maxCharacters)
		{
			bool flag = true;
			MatchCollection matchCollection = null;
			if (flag)
			{
				Regex regex = new Regex("(?></?\\w+)(?>(?:[^>'\"]+|'[^']*'|\"[^\"]*\")*)>|\\[.*?\\]");
				matchCollection = regex.Matches(line);
				line = regex.Replace(line, "¶");
			}
			if (maxCharacters <= 0)
			{
				line = ArabicFixer.Fix(line);
			}
			else
			{
				Regex regex2 = new Regex(".{0," + maxCharacters + "}(\\s+|$)", RegexOptions.Multiline);
				line = regex2.Replace(line, "$0\n");
				if (line.EndsWith("\n\n"))
				{
					line = line.Substring(0, line.Length - 2);
				}
				string[] array = line.Split(new char[] { '\n' });
				int i = 0;
				int num = array.Length;
				while (i < num)
				{
					array[i] = ArabicFixer.Fix(array[i]);
					i++;
				}
				line = string.Join("\n", array);
			}
			if (flag && matchCollection != null)
			{
				int count = matchCollection.Count;
				int num2 = 0;
				for (int j = 0; j < count; j++)
				{
					num2 = line.IndexOf('¶', num2);
					line = line.Remove(num2, 1).Insert(num2, matchCollection[j].Value);
				}
			}
			return line;
		}

		public static string FixRTL_IfNeeded(string text, int maxCharacters = 0)
		{
			if (LocalizationManager.IsRight2Left)
			{
				return LocalizationManager.ApplyRTLfix(text, maxCharacters);
			}
			return text;
		}

		internal static void LocalizeAll(bool Force = false)
		{
			Localize[] array = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));
			int i = 0;
			int num = array.Length;
			while (i < num)
			{
				Localize localize = array[i];
				localize.OnLocalize(Force);
				i++;
			}
			if (LocalizationManager.OnLocalizeEvent != null)
			{
				LocalizationManager.OnLocalizeEvent();
			}
			ResourceManager.pInstance.CleanResourceCache();
		}

		internal static void ApplyLocalizationParams(ref string translation, Localize localizeCmp)
		{
			Regex regex = new Regex("{\\[(.*?)\\]}");
			MatchCollection matchCollection = regex.Matches(translation);
			int i = 0;
			int count = matchCollection.Count;
			while (i < count)
			{
				Match match = matchCollection[i];
				string value = match.Groups[match.Groups.Count - 1].Value;
				string localizationParam = LocalizationManager.GetLocalizationParam(value, localizeCmp);
				if (localizationParam != null)
				{
					translation = translation.Replace(match.Value, localizationParam);
				}
				i++;
			}
		}

		internal static string GetLocalizationParam(string ParamName, Localize localizeCmp)
		{
			if (localizeCmp)
			{
				MonoBehaviour[] components = localizeCmp.GetComponents<MonoBehaviour>();
				int i = 0;
				int num = components.Length;
				while (i < num)
				{
					ILocalizationParamsManager localizationParamsManager = components[i] as ILocalizationParamsManager;
					if (localizationParamsManager != null)
					{
						string text = localizationParamsManager.GetParameterValue(ParamName);
						if (text != null)
						{
							return text;
						}
					}
					i++;
				}
			}
			int j = 0;
			int count = LocalizationManager.ParamManagers.Count;
			while (j < count)
			{
				string text = LocalizationManager.ParamManagers[j].GetParameterValue(ParamName);
				if (text != null)
				{
					return text;
				}
				j++;
			}
			return null;
		}

		public static bool UpdateSources()
		{
			LocalizationManager.UnregisterDeletededSources();
			LocalizationManager.RegisterSourceInResources();
			LocalizationManager.RegisterSceneSources();
			return LocalizationManager.Sources.Count > 0;
		}

		private static void UnregisterDeletededSources()
		{
			for (int i = LocalizationManager.Sources.Count - 1; i >= 0; i--)
			{
				if (LocalizationManager.Sources[i] == null)
				{
					LocalizationManager.RemoveSource(LocalizationManager.Sources[i]);
				}
			}
		}

		private static void RegisterSceneSources()
		{
			LanguageSource[] array = (LanguageSource[])Resources.FindObjectsOfTypeAll(typeof(LanguageSource));
			int i = 0;
			int num = array.Length;
			while (i < num)
			{
				if (!LocalizationManager.Sources.Contains(array[i]))
				{
					LocalizationManager.AddSource(array[i]);
				}
				i++;
			}
		}

		private static void RegisterSourceInResources()
		{
			foreach (string text in LocalizationManager.GlobalSources)
			{
				GameObject asset = ResourceManager.pInstance.GetAsset<GameObject>(text);
				LanguageSource languageSource = ((!asset) ? null : asset.GetComponent<LanguageSource>());
				if (languageSource && !LocalizationManager.Sources.Contains(languageSource))
				{
					LocalizationManager.AddSource(languageSource);
				}
			}
		}

		internal static void AddSource(LanguageSource Source)
		{
			if (LocalizationManager.Sources.Contains(Source))
			{
				return;
			}
			LocalizationManager.Sources.Add(Source);
			Source.Import_Google_FromCache();
			if (Source.GoogleUpdateDelay > 0f)
			{
				Source.Invoke("Delayed_Import_Google", Source.GoogleUpdateDelay);
			}
			else
			{
				Source.Import_Google(false);
			}
			if (Source.mDictionary.Count == 0)
			{
				Source.UpdateDictionary(true);
			}
		}

		internal static void RemoveSource(LanguageSource Source)
		{
			LocalizationManager.Sources.Remove(Source);
		}

		public static bool IsGlobalSource(string SourceName)
		{
			return Array.IndexOf<string>(LocalizationManager.GlobalSources, SourceName) >= 0;
		}

		public static bool HasLanguage(string Language, bool AllowDiscartingRegion = true, bool Initialize = true)
		{
			if (Initialize)
			{
				LocalizationManager.InitializeIfNeeded();
			}
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				if (LocalizationManager.Sources[i].GetLanguageIndex(Language, false) >= 0)
				{
					return true;
				}
				i++;
			}
			if (AllowDiscartingRegion)
			{
				int j = 0;
				int count2 = LocalizationManager.Sources.Count;
				while (j < count2)
				{
					if (LocalizationManager.Sources[j].GetLanguageIndex(Language, true) >= 0)
					{
						return true;
					}
					j++;
				}
			}
			return false;
		}

		public static string GetSupportedLanguage(string Language)
		{
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				int languageIndex = LocalizationManager.Sources[i].GetLanguageIndex(Language, false);
				if (languageIndex >= 0)
				{
					return LocalizationManager.Sources[i].mLanguages[languageIndex].Name;
				}
				i++;
			}
			int j = 0;
			int count2 = LocalizationManager.Sources.Count;
			while (j < count2)
			{
				int languageIndex2 = LocalizationManager.Sources[j].GetLanguageIndex(Language, true);
				if (languageIndex2 >= 0)
				{
					return LocalizationManager.Sources[j].mLanguages[languageIndex2].Name;
				}
				j++;
			}
			return string.Empty;
		}

		public static string GetLanguageCode(string Language)
		{
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				int languageIndex = LocalizationManager.Sources[i].GetLanguageIndex(Language, true);
				if (languageIndex >= 0)
				{
					return LocalizationManager.Sources[i].mLanguages[languageIndex].Code;
				}
				i++;
			}
			return string.Empty;
		}

		public static string GetLanguageFromCode(string Code)
		{
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				int languageIndexFromCode = LocalizationManager.Sources[i].GetLanguageIndexFromCode(Code);
				if (languageIndexFromCode >= 0)
				{
					return LocalizationManager.Sources[i].mLanguages[languageIndexFromCode].Name;
				}
				i++;
			}
			return string.Empty;
		}

		public static List<string> GetAllLanguages()
		{
			List<string> list = new List<string>();
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				int j = 0;
				int count2 = LocalizationManager.Sources[i].mLanguages.Count;
				while (j < count2)
				{
					if (!list.Contains(LocalizationManager.Sources[i].mLanguages[j].Name))
					{
						list.Add(LocalizationManager.Sources[i].mLanguages[j].Name);
					}
					j++;
				}
				i++;
			}
			return list;
		}

		public static List<string> GetCategories()
		{
			List<string> list = new List<string>();
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				LocalizationManager.Sources[i].GetCategories(false, list);
				i++;
			}
			return list;
		}

		public static List<string> GetTermsList()
		{
			if (LocalizationManager.Sources.Count == 0)
			{
				LocalizationManager.UpdateSources();
			}
			if (LocalizationManager.Sources.Count == 1)
			{
				return LocalizationManager.Sources[0].GetTermsList();
			}
			HashSet<string> hashSet = new HashSet<string>();
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				hashSet.UnionWith(LocalizationManager.Sources[i].GetTermsList());
				i++;
			}
			return new List<string>(hashSet);
		}

		public static TermData GetTermData(string term)
		{
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				TermData termData = LocalizationManager.Sources[i].GetTermData(term, false);
				if (termData != null)
				{
					return termData;
				}
				i++;
			}
			return null;
		}

		public static LanguageSource GetSourceContaining(string term, bool fallbackToFirst = true)
		{
			if (!string.IsNullOrEmpty(term))
			{
				int i = 0;
				int count = LocalizationManager.Sources.Count;
				while (i < count)
				{
					if (LocalizationManager.Sources[i].GetTermData(term, false) != null)
					{
						return LocalizationManager.Sources[i];
					}
					i++;
				}
			}
			return (!fallbackToFirst || LocalizationManager.Sources.Count <= 0) ? null : LocalizationManager.Sources[0];
		}

		public static Object FindAsset(string value)
		{
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				Object @object = LocalizationManager.Sources[i].FindAsset(value);
				if (@object)
				{
					return @object;
				}
				i++;
			}
			return null;
		}

		public static string GetVersion()
		{
			return "2.6.7 f6";
		}

		public static int GetRequiredWebServiceVersion()
		{
			return 4;
		}

		private static bool IsRTL(string Code)
		{
			return Array.IndexOf<string>(LocalizationManager.LanguagesRTL, Code) >= 0;
		}

		private static string mCurrentLanguage;

		private static string mLanguageCode;

		public static bool IsRight2Left = false;

		public static List<LanguageSource> Sources = new List<LanguageSource>();

		public static string[] GlobalSources = new string[] { "I2Languages" };

		public static List<ILocalizationParamsManager> ParamManagers = new List<ILocalizationParamsManager>();

		private static string[] LanguagesRTL = new string[]
		{
			"ar-DZ", "ar", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA",
			"ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "he", "ur", "ji"
		};

		public delegate void OnLocalizeCallback();
	}
}
