using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/Source")]
	public class LanguageSource : MonoBehaviour
	{
		public string Export_I2CSV(string Category, char Separator = ',')
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Key[*]Type[*]Desc");
			foreach (LanguageData languageData in this.mLanguages)
			{
				stringBuilder.Append("[*]");
				stringBuilder.Append(GoogleLanguages.GetCodedLanguage(languageData.Name, languageData.Code));
			}
			stringBuilder.Append("[ln]");
			int count = this.mLanguages.Count;
			foreach (TermData termData in this.mTerms)
			{
				string text;
				if (string.IsNullOrEmpty(Category) || (Category == LanguageSource.EmptyCategory && termData.Term.IndexOfAny(LanguageSource.CategorySeparators) < 0))
				{
					text = termData.Term;
				}
				else
				{
					if (!termData.Term.StartsWith(Category + "/") || !(Category != termData.Term))
					{
						continue;
					}
					text = termData.Term.Substring(Category.Length + 1);
				}
				LanguageSource.AppendI2Term(stringBuilder, count, text, termData, string.Empty, termData.Languages, termData.Languages_Touch, Separator, 1, 2);
				if (termData.HasTouchTranslations())
				{
					LanguageSource.AppendI2Term(stringBuilder, count, text, termData, "[touch]", termData.Languages_Touch, null, Separator, 2, 1);
				}
			}
			return stringBuilder.ToString();
		}

		private static void AppendI2Term(StringBuilder Builder, int nLanguages, string Term, TermData termData, string postfix, string[] aLanguages, string[] aSecLanguages, char Separator, byte FlagBitMask, byte SecFlagBitMask)
		{
			Builder.Append(Term);
			Builder.Append(postfix);
			Builder.Append("[*]");
			Builder.Append(termData.TermType.ToString());
			Builder.Append("[*]");
			Builder.Append(termData.Description);
			for (int i = 0; i < Mathf.Min(nLanguages, aLanguages.Length); i++)
			{
				Builder.Append("[*]");
				string text = aLanguages[i];
				bool flag = (termData.Flags[i] & FlagBitMask) > 0;
				if (string.IsNullOrEmpty(text) && aSecLanguages != null)
				{
					text = aSecLanguages[i];
					flag = (termData.Flags[i] & SecFlagBitMask) > 0;
				}
				if (flag)
				{
					Builder.Append("[i2auto]");
				}
				Builder.Append(text);
			}
			Builder.Append("[ln]");
		}

		public string Export_CSV(string Category, char Separator = ',')
		{
			StringBuilder stringBuilder = new StringBuilder();
			int count = this.mLanguages.Count;
			stringBuilder.AppendFormat("Key{0}Type{0}Desc", Separator);
			foreach (LanguageData languageData in this.mLanguages)
			{
				stringBuilder.Append(Separator);
				LanguageSource.AppendString(stringBuilder, GoogleLanguages.GetCodedLanguage(languageData.Name, languageData.Code), Separator);
			}
			stringBuilder.Append("\n");
			this.mTerms = this.mTerms.OrderBy((TermData x) => x.Term).ToList<TermData>();
			foreach (TermData termData in this.mTerms)
			{
				string text;
				if (string.IsNullOrEmpty(Category) || (Category == LanguageSource.EmptyCategory && termData.Term.IndexOfAny(LanguageSource.CategorySeparators) < 0))
				{
					text = termData.Term;
				}
				else
				{
					if (!termData.Term.StartsWith(Category + "/") || !(Category != termData.Term))
					{
						continue;
					}
					text = termData.Term.Substring(Category.Length + 1);
				}
				LanguageSource.AppendTerm(stringBuilder, count, text, termData, null, termData.Languages, termData.Languages_Touch, Separator, 1, 2);
				if (termData.HasTouchTranslations())
				{
					LanguageSource.AppendTerm(stringBuilder, count, text, termData, "[touch]", termData.Languages_Touch, null, Separator, 2, 1);
				}
			}
			return stringBuilder.ToString();
		}

		private static void AppendTerm(StringBuilder Builder, int nLanguages, string Term, TermData termData, string prefix, string[] aLanguages, string[] aSecLanguages, char Separator, byte FlagBitMask, byte SecFlagBitMask)
		{
			LanguageSource.AppendString(Builder, Term, Separator);
			if (!string.IsNullOrEmpty(prefix))
			{
				Builder.Append(prefix);
			}
			Builder.Append(Separator);
			Builder.Append(termData.TermType.ToString());
			Builder.Append(Separator);
			LanguageSource.AppendString(Builder, termData.Description, Separator);
			for (int i = 0; i < Mathf.Min(nLanguages, aLanguages.Length); i++)
			{
				Builder.Append(Separator);
				string text = aLanguages[i];
				bool flag = (termData.Flags[i] & FlagBitMask) > 0;
				if (string.IsNullOrEmpty(text) && aSecLanguages != null)
				{
					text = aSecLanguages[i];
					flag = (termData.Flags[i] & SecFlagBitMask) > 0;
				}
				LanguageSource.AppendTranslation(Builder, text, Separator, (!flag) ? string.Empty : "[i2auto]");
			}
			Builder.Append("\n");
		}

		private static void AppendString(StringBuilder Builder, string Text, char Separator)
		{
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}
			Text = Text.Replace("\\n", "\n");
			if (Text.IndexOfAny((Separator + "\n\"").ToCharArray()) >= 0)
			{
				Text = Text.Replace("\"", "\"\"");
				Builder.AppendFormat("\"{0}\"", Text);
			}
			else
			{
				Builder.Append(Text);
			}
		}

		private static void AppendTranslation(StringBuilder Builder, string Text, char Separator, string tags)
		{
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}
			Text = Text.Replace("\\n", "\n");
			if (Text.IndexOfAny((Separator + "\n\"").ToCharArray()) >= 0)
			{
				Text = Text.Replace("\"", "\"\"");
				Builder.AppendFormat("\"{0}{1}\"", tags, Text);
			}
			else
			{
				Builder.Append(tags);
				Builder.Append(Text);
			}
		}

		public WWW Export_Google_CreateWWWcall(eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace)
		{
			string text = this.Export_Google_CreateData();
			WWWForm wwwform = new WWWForm();
			wwwform.AddField("key", this.Google_SpreadsheetKey);
			wwwform.AddField("action", "SetLanguageSource");
			wwwform.AddField("data", text);
			wwwform.AddField("updateMode", UpdateMode.ToString());
			return new WWW(this.Google_WebServiceURL, wwwform);
		}

		private string Export_Google_CreateData()
		{
			List<string> categories = this.GetCategories(true, null);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (string text in categories)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append("<I2Loc>");
				}
				string text2 = this.Export_I2CSV(text, ',');
				stringBuilder.Append(text);
				stringBuilder.Append("<I2Loc>");
				stringBuilder.Append(text2);
			}
			return stringBuilder.ToString();
		}

		public string Import_CSV(string Category, string CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace, char Separator = ',')
		{
			List<string[]> list = LocalizationReader.ReadCSV(CSVstring, Separator);
			return this.Import_CSV(Category, list, UpdateMode);
		}

		public string Import_I2CSV(string Category, string I2CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace)
		{
			List<string[]> list = LocalizationReader.ReadI2CSV(I2CSVstring);
			return this.Import_CSV(Category, list, UpdateMode);
		}

		public string Import_CSV(string Category, List<string[]> CSV, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace)
		{
			string[] array = CSV[0];
			int num = 1;
			int num2 = -1;
			int num3 = -1;
			string[] array2 = new string[] { "Key" };
			string[] array3 = new string[] { "Type" };
			string[] array4 = new string[] { "Desc", "Description" };
			if (array.Length > 1 && this.ArrayContains(array[0], array2))
			{
				if (UpdateMode == eSpreadsheetUpdateMode.Replace)
				{
					this.ClearAllData();
				}
				if (array.Length > 2)
				{
					if (this.ArrayContains(array[1], array3))
					{
						num2 = 1;
						num = 2;
					}
					if (this.ArrayContains(array[1], array4))
					{
						num3 = 1;
						num = 2;
					}
				}
				if (array.Length > 3)
				{
					if (this.ArrayContains(array[2], array3))
					{
						num2 = 2;
						num = 3;
					}
					if (this.ArrayContains(array[2], array4))
					{
						num3 = 2;
						num = 3;
					}
				}
				int num4 = Mathf.Max(array.Length - num, 0);
				int[] array5 = new int[num4];
				for (int i = 0; i < num4; i++)
				{
					if (string.IsNullOrEmpty(array[i + num]))
					{
						array5[i] = -1;
					}
					else
					{
						string text;
						string text2;
						GoogleLanguages.UnPackCodeFromLanguageName(array[i + num], out text, out text2);
						int num5;
						if (!string.IsNullOrEmpty(text2))
						{
							num5 = this.GetLanguageIndexFromCode(text2);
						}
						else
						{
							num5 = this.GetLanguageIndex(text, true);
						}
						if (num5 < 0)
						{
							LanguageData languageData = new LanguageData();
							languageData.Name = text;
							languageData.Code = text2;
							this.mLanguages.Add(languageData);
							num5 = this.mLanguages.Count - 1;
						}
						array5[i] = num5;
					}
				}
				num4 = this.mLanguages.Count;
				int j = 0;
				int count = this.mTerms.Count;
				while (j < count)
				{
					TermData termData = this.mTerms[j];
					if (termData.Languages.Length < num4)
					{
						Array.Resize<string>(ref termData.Languages, num4);
						Array.Resize<string>(ref termData.Languages_Touch, num4);
						Array.Resize<byte>(ref termData.Flags, num4);
					}
					j++;
				}
				int k = 1;
				int count2 = CSV.Count;
				while (k < count2)
				{
					array = CSV[k];
					string text3 = ((!string.IsNullOrEmpty(Category)) ? (Category + "/" + array[0]) : array[0]);
					bool flag = false;
					if (text3.EndsWith("[touch]"))
					{
						text3 = text3.Remove(text3.Length - "[touch]".Length);
						flag = true;
					}
					LanguageSource.ValidateFullTerm(ref text3);
					if (!string.IsNullOrEmpty(text3))
					{
						TermData termData2 = this.GetTermData(text3, false);
						if (termData2 == null)
						{
							termData2 = new TermData();
							termData2.Term = text3;
							termData2.Languages = new string[this.mLanguages.Count];
							termData2.Languages_Touch = new string[this.mLanguages.Count];
							termData2.Flags = new byte[this.mLanguages.Count];
							for (int l = 0; l < this.mLanguages.Count; l++)
							{
								termData2.Languages[l] = (termData2.Languages_Touch[l] = string.Empty);
							}
							this.mTerms.Add(termData2);
							this.mDictionary.Add(text3, termData2);
						}
						else if (UpdateMode == eSpreadsheetUpdateMode.AddNewTerms)
						{
							goto IL_4A3;
						}
						if (num2 > 0)
						{
							termData2.TermType = LanguageSource.GetTermType(array[num2]);
						}
						if (num3 > 0)
						{
							termData2.Description = array[num3];
						}
						int num6 = 0;
						while (num6 < array5.Length && num6 < array.Length - num)
						{
							if (!string.IsNullOrEmpty(array[num6 + num]))
							{
								int num7 = array5[num6];
								if (num7 >= 0)
								{
									string text4 = array[num6 + num];
									bool flag2 = text4.Contains("[i2auto]");
									if (flag2)
									{
										text4 = text4.Replace("[i2auto]", string.Empty);
									}
									if (flag)
									{
										termData2.Languages_Touch[num7] = text4;
										if (flag2)
										{
											byte[] flags = termData2.Flags;
											int num8 = num7;
											flags[num8] |= 2;
										}
										else
										{
											byte[] flags2 = termData2.Flags;
											int num9 = num7;
											flags2[num9] &= 253;
										}
									}
									else
									{
										termData2.Languages[num7] = text4;
										if (flag2)
										{
											byte[] flags3 = termData2.Flags;
											int num10 = num7;
											flags3[num10] |= 1;
										}
										else
										{
											byte[] flags4 = termData2.Flags;
											int num11 = num7;
											flags4[num11] &= 254;
										}
									}
								}
							}
							num6++;
						}
					}
					IL_4A3:
					k++;
				}
				return string.Empty;
			}
			return "Bad Spreadsheet Format.\nFirst columns should be 'Key', 'Type' and 'Desc'";
		}

		private bool ArrayContains(string MainText, params string[] texts)
		{
			int i = 0;
			int num = texts.Length;
			while (i < num)
			{
				if (MainText.IndexOf(texts[i], StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
				i++;
			}
			return false;
		}

		public static eTermType GetTermType(string type)
		{
			int i = 0;
			int num = 7;
			while (i <= num)
			{
				eTermType eTermType = (eTermType)i;
				if (string.Equals(eTermType.ToString(), type, StringComparison.OrdinalIgnoreCase))
				{
					return (eTermType)i;
				}
				i++;
			}
			return eTermType.Text;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<LanguageSource> Event_OnSourceUpdateFromGoogle;

		public void Delayed_Import_Google()
		{
			this.Import_Google(false);
		}

		public void Import_Google_FromCache()
		{
			if (this.GoogleUpdateFrequency == LanguageSource.eGoogleUpdateFrequency.Never)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			string sourcePlayerPrefName = this.GetSourcePlayerPrefName();
			string @string = PlayerPrefs.GetString("I2Source_" + sourcePlayerPrefName, null);
			if (string.IsNullOrEmpty(@string))
			{
				return;
			}
			bool flag = false;
			string text = this.Google_LastUpdatedVersion;
			if (PlayerPrefs.HasKey("I2SourceVersion_" + sourcePlayerPrefName))
			{
				text = PlayerPrefs.GetString("I2SourceVersion_" + sourcePlayerPrefName, this.Google_LastUpdatedVersion);
				flag = this.IsNewerVersion(this.Google_LastUpdatedVersion, text);
			}
			if (!flag)
			{
				PlayerPrefs.DeleteKey("I2Source_" + sourcePlayerPrefName);
				PlayerPrefs.DeleteKey("I2SourceVersion_" + sourcePlayerPrefName);
				return;
			}
			if (text.Length > 19)
			{
				text = string.Empty;
			}
			this.Google_LastUpdatedVersion = text;
			Debug.Log("[I2Loc] Using Saved (PlayerPref) data in 'I2Source_" + sourcePlayerPrefName + "'");
			this.Import_Google_Result(@string, eSpreadsheetUpdateMode.Replace, false);
		}

		private bool IsNewerVersion(string currentVersion, string newVersion)
		{
			long num;
			long num2;
			return !string.IsNullOrEmpty(newVersion) && (string.IsNullOrEmpty(currentVersion) || (!long.TryParse(newVersion, out num) || !long.TryParse(currentVersion, out num2)) || num > num2);
		}

		public void Import_Google(bool ForceUpdate = false)
		{
			if (this.GoogleUpdateFrequency == LanguageSource.eGoogleUpdateFrequency.Never)
			{
				return;
			}
			string sourcePlayerPrefName = this.GetSourcePlayerPrefName();
			if (!ForceUpdate && this.GoogleUpdateFrequency != LanguageSource.eGoogleUpdateFrequency.Always)
			{
				string @string = PlayerPrefs.GetString("LastGoogleUpdate_" + sourcePlayerPrefName, string.Empty);
				DateTime dateTime;
				if (DateTime.TryParse(@string, out dateTime))
				{
					double totalDays = (DateTime.Now - dateTime).TotalDays;
					LanguageSource.eGoogleUpdateFrequency googleUpdateFrequency = this.GoogleUpdateFrequency;
					if (googleUpdateFrequency != LanguageSource.eGoogleUpdateFrequency.Daily)
					{
						if (googleUpdateFrequency != LanguageSource.eGoogleUpdateFrequency.Weekly)
						{
							if (googleUpdateFrequency == LanguageSource.eGoogleUpdateFrequency.Monthly)
							{
								if (totalDays < 31.0)
								{
									return;
								}
							}
						}
						else if (totalDays < 8.0)
						{
							return;
						}
					}
					else if (totalDays < 1.0)
					{
						return;
					}
				}
			}
			PlayerPrefs.SetString("LastGoogleUpdate_" + sourcePlayerPrefName, DateTime.Now.ToString());
			CoroutineManager.pInstance.StartCoroutine(this.Import_Google_Coroutine());
		}

		private string GetSourcePlayerPrefName()
		{
			if (Array.IndexOf<string>(LocalizationManager.GlobalSources, base.name) >= 0)
			{
				return base.name;
			}
			return SceneManager.GetActiveScene().name + "_" + base.name;
		}

		private IEnumerator Import_Google_Coroutine()
		{
			WWW www = this.Import_Google_CreateWWWcall(false);
			if (www == null)
			{
				yield break;
			}
			while (!www.isDone)
			{
				yield return null;
			}
			if (string.IsNullOrEmpty(www.error) && www.text != "\"\"")
			{
				string text = this.Import_Google_Result(www.text, eSpreadsheetUpdateMode.Replace, true);
				if (string.IsNullOrEmpty(text))
				{
					if (this.Event_OnSourceUpdateFromGoogle != null)
					{
						this.Event_OnSourceUpdateFromGoogle(this);
					}
					LocalizationManager.LocalizeAll(true);
					Debug.Log("Done Google Sync");
				}
				else
				{
					Debug.Log("Done Google Sync: source was up-to-date");
				}
			}
			else
			{
				Debug.Log("Language Source was up-to-date with Google Spreadsheet");
			}
			yield break;
		}

		public WWW Import_Google_CreateWWWcall(bool ForceUpdate = false)
		{
			if (!this.HasGoogleSpreadsheet())
			{
				return null;
			}
			string text = PlayerPrefs.GetString("I2SourceVersion_" + this.GetSourcePlayerPrefName(), this.Google_LastUpdatedVersion);
			if (text.Length > 19)
			{
				text = string.Empty;
			}
			if (this.IsNewerVersion(text, this.Google_LastUpdatedVersion))
			{
				this.Google_LastUpdatedVersion = text;
			}
			string text2 = string.Format("{0}?key={1}&action=GetLanguageSource&version={2}", this.Google_WebServiceURL, this.Google_SpreadsheetKey, (!ForceUpdate) ? this.Google_LastUpdatedVersion : "0");
			return new WWW(text2);
		}

		public bool HasGoogleSpreadsheet()
		{
			return !string.IsNullOrEmpty(this.Google_WebServiceURL) && !string.IsNullOrEmpty(this.Google_SpreadsheetKey);
		}

		public string Import_Google_Result(string JsonString, eSpreadsheetUpdateMode UpdateMode, bool saveInPlayerPrefs = false)
		{
			string empty = string.Empty;
			if (string.IsNullOrEmpty(JsonString) || JsonString == "\"\"")
			{
				return empty;
			}
			int num = JsonString.IndexOf("version=");
			int num2 = JsonString.IndexOf("script_version=");
			if (num < 0 || num2 < 0)
			{
				return "Invalid Response from Google, Most likely the WebService needs to be updated";
			}
			num += "version=".Length;
			num2 += "script_version=".Length;
			string text = JsonString.Substring(num, JsonString.IndexOf(",", num) - num);
			int num3 = int.Parse(JsonString.Substring(num2, JsonString.IndexOf(",", num2) - num2));
			if (text.Length > 19)
			{
				text = string.Empty;
			}
			if (num3 != LocalizationManager.GetRequiredWebServiceVersion())
			{
				return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
			}
			if (saveInPlayerPrefs && !this.IsNewerVersion(this.Google_LastUpdatedVersion, text))
			{
				return "LanguageSource is up-to-date";
			}
			if (saveInPlayerPrefs)
			{
				string sourcePlayerPrefName = this.GetSourcePlayerPrefName();
				PlayerPrefs.SetString("I2Source_" + sourcePlayerPrefName, JsonString);
				PlayerPrefs.SetString("I2SourceVersion_" + sourcePlayerPrefName, text);
				PlayerPrefs.Save();
			}
			this.Google_LastUpdatedVersion = text;
			if (UpdateMode == eSpreadsheetUpdateMode.Replace)
			{
				this.ClearAllData();
			}
			int i = JsonString.IndexOf("[i2category]");
			while (i > 0)
			{
				i += "[i2category]".Length;
				int num4 = JsonString.IndexOf("[/i2category]", i);
				string text2 = JsonString.Substring(i, num4 - i);
				num4 += "[/i2category]".Length;
				int num5 = JsonString.IndexOf("[/i2csv]", num4);
				string text3 = JsonString.Substring(num4, num5 - num4);
				i = JsonString.IndexOf("[i2category]", num5);
				this.Import_I2CSV(text2, text3, UpdateMode);
				if (UpdateMode == eSpreadsheetUpdateMode.Replace)
				{
					UpdateMode = eSpreadsheetUpdateMode.Merge;
				}
			}
			return empty;
		}

		public List<string> GetCategories(bool OnlyMainCategory = false, List<string> Categories = null)
		{
			if (Categories == null)
			{
				Categories = new List<string>();
			}
			foreach (TermData termData in this.mTerms)
			{
				string categoryFromFullTerm = LanguageSource.GetCategoryFromFullTerm(termData.Term, OnlyMainCategory);
				if (!Categories.Contains(categoryFromFullTerm))
				{
					Categories.Add(categoryFromFullTerm);
				}
			}
			Categories.Sort();
			return Categories;
		}

		public static string GetKeyFromFullTerm(string FullTerm, bool OnlyMainCategory = false)
		{
			int num = ((!OnlyMainCategory) ? FullTerm.LastIndexOfAny(LanguageSource.CategorySeparators) : FullTerm.IndexOfAny(LanguageSource.CategorySeparators));
			return (num >= 0) ? FullTerm.Substring(num + 1) : FullTerm;
		}

		public static string GetCategoryFromFullTerm(string FullTerm, bool OnlyMainCategory = false)
		{
			int num = ((!OnlyMainCategory) ? FullTerm.LastIndexOfAny(LanguageSource.CategorySeparators) : FullTerm.IndexOfAny(LanguageSource.CategorySeparators));
			return (num >= 0) ? FullTerm.Substring(0, num) : LanguageSource.EmptyCategory;
		}

		public static void DeserializeFullTerm(string FullTerm, out string Key, out string Category, bool OnlyMainCategory = false)
		{
			int num = ((!OnlyMainCategory) ? FullTerm.LastIndexOfAny(LanguageSource.CategorySeparators) : FullTerm.IndexOfAny(LanguageSource.CategorySeparators));
			if (num < 0)
			{
				Category = LanguageSource.EmptyCategory;
				Key = FullTerm;
			}
			else
			{
				Category = FullTerm.Substring(0, num);
				Key = FullTerm.Substring(num + 1);
			}
		}

		public static LanguageSource.eInputSpecialization GetCurrentInputType()
		{
			return (Input.GetJoystickNames().Length <= 0) ? LanguageSource.eInputSpecialization.PC : LanguageSource.eInputSpecialization.Controller;
		}

		private void Awake()
		{
			if (this.NeverDestroy)
			{
				if (this.ManagerHasASimilarSource())
				{
					Object.Destroy(this);
					return;
				}
				if (Application.isPlaying)
				{
					Object.DontDestroyOnLoad(base.gameObject);
				}
			}
			LocalizationManager.AddSource(this);
			this.UpdateDictionary(false);
		}

		public void UpdateDictionary(bool force = false)
		{
			if (!force && this.mDictionary != null && this.mDictionary.Count == this.mTerms.Count)
			{
				return;
			}
			StringComparer stringComparer = ((!this.CaseInsensitiveTerms) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
			if (this.mDictionary.Comparer != stringComparer)
			{
				this.mDictionary = new Dictionary<string, TermData>(stringComparer);
			}
			else
			{
				this.mDictionary.Clear();
			}
			int i = 0;
			int count = this.mTerms.Count;
			while (i < count)
			{
				LanguageSource.ValidateFullTerm(ref this.mTerms[i].Term);
				if (this.mTerms[i].Languages_Touch == null || this.mTerms[i].Languages_Touch.Length != this.mTerms[i].Languages.Length)
				{
					this.mTerms[i].Languages_Touch = new string[this.mTerms[i].Languages.Length];
				}
				this.mDictionary[this.mTerms[i].Term] = this.mTerms[i];
				this.mTerms[i].Validate();
				i++;
			}
		}

		public string GetSourceName()
		{
			string text = base.gameObject.name;
			Transform transform = base.transform.parent;
			while (transform)
			{
				text = transform.name + "_" + text;
				transform = transform.parent;
			}
			return text;
		}

		public int GetLanguageIndex(string language, bool AllowDiscartingRegion = true)
		{
			int i = 0;
			int count = this.mLanguages.Count;
			while (i < count)
			{
				if (string.Compare(this.mLanguages[i].Name, language, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return i;
				}
				i++;
			}
			if (AllowDiscartingRegion)
			{
				int num = -1;
				int num2 = 0;
				int j = 0;
				int count2 = this.mLanguages.Count;
				while (j < count2)
				{
					int commonWordInLanguageNames = LanguageSource.GetCommonWordInLanguageNames(this.mLanguages[j].Name, language);
					if (commonWordInLanguageNames > num2)
					{
						num2 = commonWordInLanguageNames;
						num = j;
					}
					j++;
				}
				if (num >= 0)
				{
					return num;
				}
			}
			return -1;
		}

		public int GetLanguageIndexFromCode(string Code)
		{
			int i = 0;
			int count = this.mLanguages.Count;
			while (i < count)
			{
				if (string.Compare(this.mLanguages[i].Code, Code, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return i;
				}
				i++;
			}
			return -1;
		}

		public static int GetCommonWordInLanguageNames(string Language1, string Language2)
		{
			if (string.IsNullOrEmpty(Language1) || string.IsNullOrEmpty(Language2))
			{
				return 0;
			}
			string[] array = (from x in Language1.Split("( )-/\\".ToCharArray())
				where !string.IsNullOrEmpty(x)
				select x).ToArray<string>();
			string[] array2 = (from x in Language2.Split("( )-/\\".ToCharArray())
				where !string.IsNullOrEmpty(x)
				select x).ToArray<string>();
			int num = 0;
			foreach (string text in array)
			{
				if (array2.Contains(text))
				{
					num++;
				}
			}
			foreach (string text2 in array2)
			{
				if (array.Contains(text2))
				{
					num++;
				}
			}
			return num;
		}

		public static bool AreTheSameLanguage(string Language1, string Language2)
		{
			Language1 = LanguageSource.GetLanguageWithoutRegion(Language1);
			Language2 = LanguageSource.GetLanguageWithoutRegion(Language2);
			return string.Compare(Language1, Language2, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public static string GetLanguageWithoutRegion(string Language)
		{
			int num = Language.IndexOfAny("(/\\[,{".ToCharArray());
			if (num < 0)
			{
				return Language;
			}
			return Language.Substring(0, num).Trim();
		}

		public void AddLanguage(string LanguageName, string LanguageCode)
		{
			if (this.GetLanguageIndex(LanguageName, false) >= 0)
			{
				return;
			}
			LanguageData languageData = new LanguageData();
			languageData.Name = LanguageName;
			languageData.Code = LanguageCode;
			this.mLanguages.Add(languageData);
			int count = this.mLanguages.Count;
			int i = 0;
			int count2 = this.mTerms.Count;
			while (i < count2)
			{
				Array.Resize<string>(ref this.mTerms[i].Languages, count);
				Array.Resize<string>(ref this.mTerms[i].Languages_Touch, count);
				Array.Resize<byte>(ref this.mTerms[i].Flags, count);
				i++;
			}
		}

		public void RemoveLanguage(string LanguageName)
		{
			int languageIndex = this.GetLanguageIndex(LanguageName, true);
			if (languageIndex < 0)
			{
				return;
			}
			int count = this.mLanguages.Count;
			int i = 0;
			int count2 = this.mTerms.Count;
			while (i < count2)
			{
				for (int j = languageIndex + 1; j < count; j++)
				{
					this.mTerms[i].Languages[j - 1] = this.mTerms[i].Languages[j];
					this.mTerms[i].Languages_Touch[j - 1] = this.mTerms[i].Languages_Touch[j];
					this.mTerms[i].Flags[j - 1] = this.mTerms[i].Flags[j];
				}
				Array.Resize<string>(ref this.mTerms[i].Languages, count - 1);
				Array.Resize<string>(ref this.mTerms[i].Languages_Touch, count - 1);
				Array.Resize<byte>(ref this.mTerms[i].Flags, count - 1);
				i++;
			}
			this.mLanguages.RemoveAt(languageIndex);
		}

		public List<string> GetLanguages()
		{
			List<string> list = new List<string>();
			int i = 0;
			int count = this.mLanguages.Count;
			while (i < count)
			{
				list.Add(this.mLanguages[i].Name);
				i++;
			}
			return list;
		}

		public string GetTermTranslation(string term)
		{
			int languageIndex = this.GetLanguageIndex(LocalizationManager.CurrentLanguage, true);
			if (languageIndex < 0)
			{
				return string.Empty;
			}
			TermData termData = this.GetTermData(term, false);
			if (termData != null)
			{
				return termData.GetTranslation(languageIndex);
			}
			return string.Empty;
		}

		public bool TryGetTermTranslation(string term, out string Translation)
		{
			int languageIndex = this.GetLanguageIndex(LocalizationManager.CurrentLanguage, true);
			if (languageIndex >= 0)
			{
				TermData termData = this.GetTermData(term, false);
				if (termData != null)
				{
					Translation = termData.GetTranslation(languageIndex);
					return true;
				}
			}
			Translation = string.Empty;
			return false;
		}

		public TermData AddTerm(string term)
		{
			return this.AddTerm(term, eTermType.Text, true);
		}

		public TermData GetTermData(string term, bool allowCategoryMistmatch = false)
		{
			if (string.IsNullOrEmpty(term))
			{
				return null;
			}
			if (this.mDictionary.Count == 0)
			{
				this.UpdateDictionary(false);
			}
			TermData termData;
			if (this.mDictionary.TryGetValue(term, out termData))
			{
				return termData;
			}
			TermData termData2 = null;
			if (allowCategoryMistmatch)
			{
				string keyFromFullTerm = LanguageSource.GetKeyFromFullTerm(term, false);
				foreach (KeyValuePair<string, TermData> keyValuePair in this.mDictionary)
				{
					if (keyValuePair.Value.IsTerm(keyFromFullTerm, true))
					{
						if (termData2 != null)
						{
							return null;
						}
						termData2 = keyValuePair.Value;
					}
				}
				return termData2;
			}
			return termData2;
		}

		public bool ContainsTerm(string term)
		{
			return this.GetTermData(term, false) != null;
		}

		public List<string> GetTermsList()
		{
			if (this.mDictionary.Count != this.mTerms.Count)
			{
				this.UpdateDictionary(false);
			}
			return new List<string>(this.mDictionary.Keys);
		}

		public TermData AddTerm(string NewTerm, eTermType termType, bool SaveSource = true)
		{
			LanguageSource.ValidateFullTerm(ref NewTerm);
			NewTerm = NewTerm.Trim();
			if (this.mLanguages.Count == 0)
			{
				this.AddLanguage("English", "en");
			}
			TermData termData = this.GetTermData(NewTerm, false);
			if (termData == null)
			{
				termData = new TermData();
				termData.Term = NewTerm;
				termData.TermType = termType;
				termData.Languages = new string[this.mLanguages.Count];
				termData.Languages_Touch = new string[this.mLanguages.Count];
				termData.Flags = new byte[this.mLanguages.Count];
				this.mTerms.Add(termData);
				this.mDictionary.Add(NewTerm, termData);
			}
			return termData;
		}

		public void RemoveTerm(string term)
		{
			int i = 0;
			int count = this.mTerms.Count;
			while (i < count)
			{
				if (this.mTerms[i].Term == term)
				{
					this.mTerms.RemoveAt(i);
					this.mDictionary.Remove(term);
					return;
				}
				i++;
			}
		}

		public static void ValidateFullTerm(ref string Term)
		{
			Term = Term.Replace('\\', '/');
			Term = Term.Trim();
			if (Term.StartsWith(LanguageSource.EmptyCategory) && Term.Length > LanguageSource.EmptyCategory.Length && Term[LanguageSource.EmptyCategory.Length] == '/')
			{
				Term = Term.Substring(LanguageSource.EmptyCategory.Length + 1);
			}
		}

		public bool IsEqualTo(LanguageSource Source)
		{
			if (Source.mLanguages.Count != this.mLanguages.Count)
			{
				return false;
			}
			int i = 0;
			int count = this.mLanguages.Count;
			while (i < count)
			{
				if (Source.GetLanguageIndex(this.mLanguages[i].Name, true) < 0)
				{
					return false;
				}
				i++;
			}
			if (Source.mTerms.Count != this.mTerms.Count)
			{
				return false;
			}
			for (int j = 0; j < this.mTerms.Count; j++)
			{
				if (Source.GetTermData(this.mTerms[j].Term, false) == null)
				{
					return false;
				}
			}
			return true;
		}

		internal bool ManagerHasASimilarSource()
		{
			int i = 0;
			int count = LocalizationManager.Sources.Count;
			while (i < count)
			{
				LanguageSource languageSource = LocalizationManager.Sources[i];
				if (languageSource != null && languageSource.IsEqualTo(this) && languageSource != this)
				{
					return true;
				}
				i++;
			}
			return false;
		}

		public void ClearAllData()
		{
			this.mTerms.Clear();
			this.mLanguages.Clear();
			this.mDictionary.Clear();
		}

		public Object FindAsset(string Name)
		{
			if (this.Assets != null)
			{
				int i = 0;
				int num = this.Assets.Length;
				while (i < num)
				{
					if (this.Assets[i] != null && this.Assets[i].name == Name)
					{
						return this.Assets[i];
					}
					i++;
				}
			}
			return null;
		}

		public bool HasAsset(Object Obj)
		{
			return Array.IndexOf<Object>(this.Assets, Obj) >= 0;
		}

		public void AddAsset(Object Obj)
		{
			Array.Resize<Object>(ref this.Assets, this.Assets.Length + 1);
			this.Assets[this.Assets.Length - 1] = Obj;
		}

		public string Google_WebServiceURL;

		public string Google_SpreadsheetKey;

		public string Google_SpreadsheetName;

		public string Google_LastUpdatedVersion;

		public LanguageSource.eGoogleUpdateFrequency GoogleUpdateFrequency = LanguageSource.eGoogleUpdateFrequency.Weekly;

		public float GoogleUpdateDelay = 5f;

		public static string EmptyCategory = "Default";

		public static char[] CategorySeparators = "/\\".ToCharArray();

		public List<TermData> mTerms = new List<TermData>();

		public List<LanguageData> mLanguages = new List<LanguageData>();

		public bool CaseInsensitiveTerms;

		[NonSerialized]
		public Dictionary<string, TermData> mDictionary = new Dictionary<string, TermData>(StringComparer.Ordinal);

		public Object[] Assets;

		public bool NeverDestroy = true;

		public bool UserAgreesToHaveItOnTheScene;

		public bool UserAgreesToHaveItInsideThePluginsFolder;

		public enum eGoogleUpdateFrequency
		{
			Always,
			Never,
			Daily,
			Weekly,
			Monthly
		}

		public enum eInputSpecialization
		{
			PC,
			Touch,
			Controller
		}
	}
}
