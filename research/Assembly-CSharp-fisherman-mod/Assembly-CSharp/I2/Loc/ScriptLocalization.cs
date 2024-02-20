using System;
using System.Collections.Generic;

namespace I2.Loc
{
	public static class ScriptLocalization
	{
		public static string Get(string Term)
		{
			return ScriptLocalization.Get(Term, false, 0);
		}

		public static string Get(string Term, bool FixForRTL)
		{
			return ScriptLocalization.Get(Term, FixForRTL, 0);
		}

		public static string Get(string Term, bool FixForRTL, int maxLineLengthForRTL)
		{
			if (string.IsNullOrEmpty(Term))
			{
				return Term;
			}
			string termTranslation;
			if (ScriptLocalization._dict.TryGetValue(Term, out termTranslation))
			{
				return termTranslation;
			}
			termTranslation = LocalizationManager.GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL);
			ScriptLocalization._dict[Term] = termTranslation;
			return termTranslation;
		}

		public static void OnLanguageChanged()
		{
			ScriptLocalization._dict.Clear();
		}

		private static Dictionary<string, string> _dict = new Dictionary<string, string>();
	}
}
