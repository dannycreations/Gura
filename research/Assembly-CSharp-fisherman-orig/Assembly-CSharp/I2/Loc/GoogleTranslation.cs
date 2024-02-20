using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace I2.Loc
{
	public static class GoogleTranslation
	{
		public static bool CanTranslate()
		{
			return LocalizationManager.Sources.Count > 0 && !string.IsNullOrEmpty(LocalizationManager.Sources[0].Google_WebServiceURL);
		}

		public static void Translate(string text, string LanguageCodeFrom, string LanguageCodeTo, Action<string> OnTranslationReady)
		{
			WWW translationWWW = GoogleTranslation.GetTranslationWWW(text, LanguageCodeFrom, LanguageCodeTo);
			CoroutineManager.pInstance.StartCoroutine(GoogleTranslation.WaitForTranslation(translationWWW, OnTranslationReady, text));
		}

		private static IEnumerator WaitForTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
		{
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError(www.error);
				OnTranslationReady(string.Empty);
			}
			else
			{
				string text = GoogleTranslation.ParseTranslationResult(www.text, OriginalText);
				OnTranslationReady(text);
			}
			yield break;
		}

		public static string ForceTranslate(string text, string LanguageCodeFrom, string LanguageCodeTo)
		{
			WWW translationWWW = GoogleTranslation.GetTranslationWWW(text, LanguageCodeFrom, LanguageCodeTo);
			while (!translationWWW.isDone)
			{
			}
			if (!string.IsNullOrEmpty(translationWWW.error))
			{
				Debug.LogError("-- " + translationWWW.error);
				return string.Empty;
			}
			return GoogleTranslation.ParseTranslationResult(translationWWW.text, text);
		}

		public static WWW GetTranslationWWW(string text, string LanguageCodeFrom, string LanguageCodeTo)
		{
			LanguageCodeFrom = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeFrom);
			LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);
			if (GoogleTranslation.TitleCase(text) == text)
			{
				text = text.ToLower();
			}
			string text2 = string.Format("{0}?action=Translate&list={1}:{2}={3}", new object[]
			{
				LocalizationManager.Sources[0].Google_WebServiceURL,
				LanguageCodeFrom,
				LanguageCodeTo,
				Uri.EscapeUriString(text)
			});
			return new WWW(text2);
		}

		public static string ParseTranslationResult(string html, string OriginalText)
		{
			string text2;
			try
			{
				string text = html;
				if (GoogleTranslation.TitleCase(OriginalText) == OriginalText)
				{
					text = GoogleTranslation.TitleCase(text);
				}
				text2 = text;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				text2 = string.Empty;
			}
			return text2;
		}

		public static void Translate(List<TranslationRequest> requests, Action<List<TranslationRequest>> OnTranslationReady)
		{
			WWW translationWWW = GoogleTranslation.GetTranslationWWW(requests);
			CoroutineManager.pInstance.StartCoroutine(GoogleTranslation.WaitForTranslation(translationWWW, OnTranslationReady, requests));
		}

		public static WWW GetTranslationWWW(List<TranslationRequest> requests)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(LocalizationManager.Sources[0].Google_WebServiceURL);
			stringBuilder.Append("?action=Translate&list=");
			bool flag = true;
			foreach (TranslationRequest translationRequest in requests)
			{
				if (!flag)
				{
					stringBuilder.Append("<I2Loc>");
				}
				stringBuilder.Append(translationRequest.LanguageCode);
				stringBuilder.Append(":");
				for (int i = 0; i < translationRequest.TargetLanguagesCode.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(translationRequest.TargetLanguagesCode[i]);
				}
				stringBuilder.Append("=");
				string text = ((!(GoogleTranslation.TitleCase(translationRequest.Text) == translationRequest.Text)) ? translationRequest.Text : translationRequest.Text.ToLowerInvariant());
				stringBuilder.Append(Uri.EscapeUriString(text));
				flag = false;
			}
			string text2 = stringBuilder.ToString();
			return new WWW(text2);
		}

		private static IEnumerator WaitForTranslation(WWW www, Action<List<TranslationRequest>> OnTranslationReady, List<TranslationRequest> requests)
		{
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError(www.error);
				OnTranslationReady(requests);
			}
			else
			{
				GoogleTranslation.ParseTranslationResult(www.text, requests);
				OnTranslationReady(requests);
			}
			yield break;
		}

		public static string ParseTranslationResult(string html, List<TranslationRequest> requests)
		{
			if (html.StartsWith("<!DOCTYPE html>") || html.StartsWith("<HTML>"))
			{
				return "There was a problem contacting the WebService. Please try again later";
			}
			string[] array = html.Split(new string[] { "<I2Loc>" }, StringSplitOptions.None);
			string[] array2 = new string[] { "<i2>" };
			for (int i = 0; i < Mathf.Min(requests.Count, array.Length); i++)
			{
				TranslationRequest translationRequest = requests[i];
				translationRequest.Results = array[i].Split(array2, StringSplitOptions.None);
				if (GoogleTranslation.TitleCase(translationRequest.Text) == translationRequest.Text)
				{
					for (int j = 0; j < translationRequest.Results.Length; j++)
					{
						translationRequest.Results[j] = GoogleTranslation.TitleCase(translationRequest.Results[j]);
					}
				}
				requests[i] = translationRequest;
			}
			return string.Empty;
		}

		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] array = s.ToLower().ToCharArray();
			array[0] = char.ToUpper(array[0]);
			return new string(array);
		}

		public static string TitleCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(s);
			stringBuilder[0] = char.ToUpper(stringBuilder[0]);
			int i = 1;
			int length = s.Length;
			while (i < length)
			{
				if (char.IsWhiteSpace(stringBuilder[i - 1]))
				{
					stringBuilder[i] = char.ToUpper(stringBuilder[i]);
				}
				else
				{
					stringBuilder[i] = char.ToLower(stringBuilder[i]);
				}
				i++;
			}
			return stringBuilder.ToString();
		}
	}
}
