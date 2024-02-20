using System;
using System.Collections.Generic;
using System.IO;
using BiteEditor.ObjectModel;
using ObjectModel;

namespace BiteEditor
{
	public class Settings
	{
		public static byte EVENING_DURATION
		{
			get
			{
				return 8;
			}
		}

		public static string GetImagesFolder(string pondName)
		{
			return string.Format("{0}{1}\\", "Assets\\BiteSystem\\Maps\\", pondName);
		}

		public static string GetHeightMapPath(string pondName)
		{
			return string.Format("{0}{1}\\heightmap.png", "Assets\\BiteSystem\\Maps\\", pondName);
		}

		public static string GetSplatMapPath(string pondName)
		{
			return string.Format("{0}{1}\\splatmap.png", "Assets\\BiteSystem\\Maps\\", pondName);
		}

		public static string GetFlowMapPath(string pondName)
		{
			return string.Format("{0}{1}\\flowmap.png", "Assets\\BiteSystem\\Maps\\", pondName);
		}

		public static string TrimAssetName(string name)
		{
			return (!string.IsNullOrEmpty(name)) ? name.Substring(name.IndexOf("Assets")) : null;
		}

		public static Dictionary<string, Settings.FishCodeNameExport> ParseFishTable()
		{
			Dictionary<char, FishForm> dictionary = new Dictionary<char, FishForm>
			{
				{
					'C',
					FishForm.Common
				},
				{
					'Y',
					FishForm.Young
				},
				{
					'T',
					FishForm.Trophy
				},
				{
					'U',
					FishForm.Unique
				}
			};
			Dictionary<string, Settings.FishCodeNameExport> dictionary2 = new Dictionary<string, Settings.FishCodeNameExport>();
			string[] array = File.ReadAllLines(string.Format("{0}\\Fish.txt", "Assets\\BiteSystem\\"));
			for (int i = 2; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length == 4)
				{
					int num = int.Parse(array2[2]);
					string text = array2[1];
					char c = array2[3][0];
					FishForm fishForm = dictionary[c];
					dictionary2[text] = new Settings.FishCodeNameExport((FishName)num, fishForm);
				}
			}
			return dictionary2;
		}

		public static void InitFish(FishData[] fishData)
		{
			Settings._fish.Clear();
			Settings._fishIdToFishName.Clear();
			foreach (FishData fishData2 in fishData)
			{
				FishName categoryId = (FishName)fishData2.CategoryId;
				Settings._fishIdToFishName[fishData2.Id] = categoryId;
				if (!Settings._fish.ContainsKey(categoryId))
				{
					Settings._fish[categoryId] = new Dictionary<FishForm, int>();
				}
				Settings._fish[categoryId][Settings._fishStatusToForm[fishData2.Form]] = fishData2.Id;
			}
		}

		public static FishName GetFishName(int formId)
		{
			return (!Settings._fishIdToFishName.ContainsKey(formId)) ? FishName.None : Settings._fishIdToFishName[formId];
		}

		public static int GetFishId(FishName name, FishForm form)
		{
			if (!Settings._fish.ContainsKey(name))
			{
				return -1;
			}
			Dictionary<FishForm, int> dictionary = Settings._fish[name];
			return (!dictionary.ContainsKey(form)) ? (-1) : dictionary[form];
		}

		public static float MinutesFromFirstHourToHours(int minutes)
		{
			int num = minutes + 300;
			int num2 = num / 60;
			return (float)num2 + (float)(num % 60) / 60f;
		}

		public const string BASE_PATH = "Assets\\BiteSystem\\";

		public const string MAPS_PATH = "Assets\\BiteSystem\\Maps\\";

		public const byte FIRST_HOUR = 5;

		public const byte EVENING_HOUR = 21;

		private static Dictionary<FishName, Dictionary<FishForm, int>> _fish = new Dictionary<FishName, Dictionary<FishForm, int>>();

		private static Dictionary<int, FishName> _fishIdToFishName = new Dictionary<int, FishName>();

		private static Dictionary<FishStatus, FishForm> _fishStatusToForm = new Dictionary<FishStatus, FishForm>
		{
			{
				FishStatus.Young,
				FishForm.Young
			},
			{
				FishStatus.Common,
				FishForm.Common
			},
			{
				FishStatus.Trophy,
				FishForm.Trophy
			},
			{
				FishStatus.Unique,
				FishForm.Unique
			}
		};

		public struct FishCodeNameExport
		{
			public FishCodeNameExport(FishName fishName, FishForm fishForm)
			{
				this = default(Settings.FishCodeNameExport);
				this.FishName = fishName;
				this.FishForm = fishForm;
			}

			public readonly FishName FishName;

			public readonly FishForm FishForm;
		}
	}
}
