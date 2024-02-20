using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using UnityEngine;

public class ChangeLanguage : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event EventHandler LanguageChanged;

	internal void Update()
	{
		if (ChangeLanguage._languageId != -1 && PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage._languageId, null, false);
			ChangeLanguage._languageId = -1;
		}
	}

	public static void ChangeLanguageAction(Language language)
	{
		PhotonConnectionFactory.Instance.OnSetAddPropsComplete += ChangeLanguage.OnSetAddPropsComplete;
		PhotonConnectionFactory.Instance.OnSetAddPropsFailed += ChangeLanguage.OnSetAddPropsFailed;
		PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.ChangeLanguageCommon(language), null, false);
	}

	private static void OnSetAddPropsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnSetAddPropsComplete -= ChangeLanguage.OnSetAddPropsComplete;
		PhotonConnectionFactory.Instance.OnSetAddPropsFailed -= ChangeLanguage.OnSetAddPropsFailed;
		if (ChangeLanguage.LanguageChanged != null)
		{
			ChangeLanguage.LanguageChanged(null, null);
		}
	}

	private static void OnSetAddPropsComplete()
	{
		PhotonConnectionFactory.Instance.OnSetAddPropsComplete -= ChangeLanguage.OnSetAddPropsComplete;
		PhotonConnectionFactory.Instance.OnSetAddPropsFailed -= ChangeLanguage.OnSetAddPropsFailed;
		if (ChangeLanguage.LanguageChanged != null)
		{
			ChangeLanguage.LanguageChanged(null, null);
		}
	}

	public static int ChangeLanguageActionWithoutProfile(Language language)
	{
		ChangeLanguage._languageId = ChangeLanguage.ChangeLanguageCommon(language);
		return ChangeLanguage._languageId;
	}

	private static int ChangeLanguageCommon(Language language)
	{
		LocalizationManager.CurrentLanguage = language.InLocalizeName;
		PlayerPrefs.SetInt("CurrentLanguage", language.Id);
		ChangeLanguage.LanguageWasChanged = true;
		return language.Id;
	}

	public static Language GetCurrentLanguage
	{
		get
		{
			if (!PlayerPrefs.HasKey("CurrentLanguage"))
			{
				return ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric];
			}
			return ChangeLanguage.LanguagesList.First((KeyValuePair<CustomLanguages, Language> x) => x.Value.Id == PlayerPrefs.GetInt("CurrentLanguage")).Value;
		}
	}

	public static Language GetLanguage(int comboBoxId, bool isMetric)
	{
		if (comboBoxId != ChangeLanguage.LanguagesList[CustomLanguages.English].ComboBoxId)
		{
			return ChangeLanguage.LanguagesList.First((KeyValuePair<CustomLanguages, Language> x) => x.Value.ComboBoxId == comboBoxId).Value;
		}
		if (isMetric)
		{
			return ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric];
		}
		return ChangeLanguage.LanguagesList[CustomLanguages.English];
	}

	public static void InitLanguage()
	{
		Language language = ChangeLanguage.LanguagesList.Values.FirstOrDefault((Language x) => x.Id == PlayerPrefs.GetInt("CurrentLanguage"));
		if (PlayerPrefs.HasKey("CurrentLanguage") && language != null && LocalizationManager.HasLanguage(language.InLocalizeName, true, true))
		{
			LocalizationManager.CurrentLanguage = language.InLocalizeName;
			MeasuringSystemManager.ChangeMeasuringSystem();
		}
		else
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			switch (systemLanguage)
			{
			case 9:
				if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Dutch].InLocalizeName, true, true))
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Dutch]);
				}
				else
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
				}
				break;
			case 10:
				ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.English]);
				break;
			default:
				switch (systemLanguage)
				{
				case 27:
					if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Polish].InLocalizeName, true, true))
					{
						ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Polish]);
					}
					else
					{
						ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
					}
					goto IL_491;
				case 28:
					if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Portuguese].InLocalizeName, true, true))
					{
						ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Portuguese]);
					}
					else
					{
						ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
					}
					goto IL_491;
				default:
					switch (systemLanguage)
					{
					case 38:
						if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Ukrainian].InLocalizeName, true, true))
						{
							ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Ukrainian]);
						}
						else
						{
							ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
						}
						goto IL_491;
					default:
						switch (systemLanguage)
						{
						case 3:
							break;
						default:
							if (systemLanguage != 21)
							{
								ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
								goto IL_491;
							}
							if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Italian].InLocalizeName, true, true))
							{
								ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Italian]);
							}
							else
							{
								ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
							}
							goto IL_491;
						case 6:
							if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Chinese].InLocalizeName, true, true))
							{
								ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Chinese]);
							}
							else
							{
								ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
							}
							goto IL_491;
						}
						break;
					case 40:
						if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Chinese].InLocalizeName, true, true))
						{
							ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Chinese]);
						}
						else
						{
							ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
						}
						goto IL_491;
					case 41:
						if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.ChineseTraditional].InLocalizeName, true, true))
						{
							ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.ChineseTraditional]);
						}
						else
						{
							ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
						}
						goto IL_491;
					}
					break;
				case 30:
					break;
				case 34:
					if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Spanish].InLocalizeName, true, true))
					{
						ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Spanish]);
					}
					else
					{
						ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
					}
					goto IL_491;
				}
				if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.Russian].InLocalizeName, true, true))
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.Russian]);
				}
				else
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
				}
				break;
			case 14:
				if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.French].InLocalizeName, true, true))
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.French]);
				}
				else
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
				}
				break;
			case 15:
				if (LocalizationManager.HasLanguage(ChangeLanguage.LanguagesList[CustomLanguages.German].InLocalizeName, true, true))
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.German]);
				}
				else
				{
					ChangeLanguage.ChangeLanguageCommon(ChangeLanguage.LanguagesList[CustomLanguages.EnglishMetric]);
				}
				break;
			}
			IL_491:
			MeasuringSystemManager.ChangeMeasuringSystem();
		}
	}

	public static CustomLanguagesShort GetCustomLanguagesShort(CustomLanguages customLanguage)
	{
		switch (customLanguage)
		{
		case CustomLanguages.English:
			return CustomLanguagesShort.EN;
		case CustomLanguages.Russian:
			return CustomLanguagesShort.RU;
		case CustomLanguages.EnglishMetric:
			return CustomLanguagesShort.EN;
		case CustomLanguages.German:
			return CustomLanguagesShort.DE;
		case CustomLanguages.Polish:
			return CustomLanguagesShort.PL;
		case CustomLanguages.French:
			return CustomLanguagesShort.FR;
		case CustomLanguages.Ukrainian:
			return CustomLanguagesShort.UA;
		case CustomLanguages.Spanish:
			return CustomLanguagesShort.ES;
		case CustomLanguages.Portuguese:
			return CustomLanguagesShort.PT;
		case CustomLanguages.Dutch:
			return CustomLanguagesShort.NL;
		case CustomLanguages.Chinese:
			return CustomLanguagesShort.CN;
		case CustomLanguages.ChineseTraditional:
			return CustomLanguagesShort.ZN;
		case CustomLanguages.Italian:
			return CustomLanguagesShort.IT;
		default:
			return CustomLanguagesShort.EN;
		}
	}

	public static bool LanguageWasChanged;

	private static int _languageId = -1;

	public static Dictionary<CustomLanguages, Language> LanguagesList = new Dictionary<CustomLanguages, Language>
	{
		{
			CustomLanguages.English,
			new Language
			{
				Lang = CustomLanguages.English,
				Id = 1,
				ComboBoxId = 0,
				InGameName = "English",
				InLocalizeName = "English (United States)"
			}
		},
		{
			CustomLanguages.Russian,
			new Language
			{
				Lang = CustomLanguages.Russian,
				Id = 2,
				ComboBoxId = 1,
				InGameName = "Русский",
				InLocalizeName = "Russian [ru-RU]"
			}
		},
		{
			CustomLanguages.EnglishMetric,
			new Language
			{
				Lang = CustomLanguages.EnglishMetric,
				Id = 3,
				ComboBoxId = 0,
				InGameName = "English",
				InLocalizeName = "English (metric)"
			}
		},
		{
			CustomLanguages.German,
			new Language
			{
				Lang = CustomLanguages.German,
				Id = 4,
				ComboBoxId = 2,
				InGameName = "Deutsch",
				InLocalizeName = "German (Germany)"
			}
		},
		{
			CustomLanguages.French,
			new Language
			{
				Lang = CustomLanguages.French,
				Id = 5,
				ComboBoxId = 3,
				InGameName = "Français",
				InLocalizeName = "French"
			}
		},
		{
			CustomLanguages.Chinese,
			new Language
			{
				Lang = CustomLanguages.Chinese,
				Id = 12,
				ComboBoxId = 4,
				InGameName = "简体中文",
				InLocalizeName = "Chinese (Simplified)"
			}
		},
		{
			CustomLanguages.ChineseTraditional,
			new Language
			{
				Lang = CustomLanguages.ChineseTraditional,
				Id = 13,
				ComboBoxId = 5,
				InGameName = "繁體中文",
				InLocalizeName = "Chinese (Traditional)"
			}
		},
		{
			CustomLanguages.Polish,
			new Language
			{
				Lang = CustomLanguages.Polish,
				Id = 6,
				ComboBoxId = 6,
				InGameName = "Polski",
				InLocalizeName = "Polish"
			}
		},
		{
			CustomLanguages.Ukrainian,
			new Language
			{
				Lang = CustomLanguages.Ukrainian,
				Id = 7,
				ComboBoxId = 7,
				InGameName = "Українська",
				InLocalizeName = "Ukrainian"
			}
		},
		{
			CustomLanguages.Spanish,
			new Language
			{
				Lang = CustomLanguages.Spanish,
				Id = 9,
				ComboBoxId = 8,
				InGameName = "Español",
				InLocalizeName = "Spanish (Spain)"
			}
		},
		{
			CustomLanguages.Portuguese,
			new Language
			{
				Lang = CustomLanguages.Portuguese,
				Id = 10,
				ComboBoxId = 9,
				InGameName = "Português",
				InLocalizeName = "Portuguese (Brazil)"
			}
		},
		{
			CustomLanguages.Dutch,
			new Language
			{
				Lang = CustomLanguages.Dutch,
				Id = 11,
				ComboBoxId = 10,
				InGameName = "Nederlands",
				InLocalizeName = "Dutch"
			}
		},
		{
			CustomLanguages.Italian,
			new Language
			{
				Lang = CustomLanguages.Italian,
				Id = 8,
				ComboBoxId = 11,
				InGameName = "Italiano",
				InLocalizeName = "Italian"
			}
		}
	};
}
