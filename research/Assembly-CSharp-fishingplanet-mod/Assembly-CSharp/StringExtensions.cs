using System;
using System.Text.RegularExpressions;

public static class StringExtensions
{
	public static string ReplaceNonLatin(this string str)
	{
		string text = "[^\\x20-\\x7e]";
		return Regex.Replace(str, text, string.Empty, RegexOptions.IgnoreCase);
	}
}
