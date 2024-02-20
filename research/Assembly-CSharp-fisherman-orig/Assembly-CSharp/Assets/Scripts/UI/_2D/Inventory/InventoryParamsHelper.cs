using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using I2.Loc;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Inventory
{
	public class InventoryParamsHelper
	{
		public static string ParseDesc(InventoryItem ii)
		{
			string text = ii.Desc;
			Chum chum = ii as Chum;
			if (chum != null)
			{
				double? weight = chum.Weight;
				double num = ((weight == null) ? 0.0 : weight.Value);
				int num2 = 0;
				string[] array = MeasuringSystemManager.FormatWeight(new double[]
				{
					num,
					(double)num2
				});
				if (chum.RemainTime.TotalSeconds > 0.0 || num > 0.0 || num2 > 0)
				{
					text += "\n;";
				}
				if (chum.RemainTime.TotalSeconds > 0.0)
				{
					text += string.Format("{0}: {1}; ", ScriptLocalization.Get("TimeLeftCaption"), chum.RemainTime.GetFormated(true, true));
				}
				if (num > 0.0)
				{
					text += string.Format("{0}: {1}; ", ScriptLocalization.Get("WeightCaption"), array[0]);
				}
				if (num2 > 0)
				{
					text += string.Format("{0}: {1}; ", ScriptLocalization.Get("WaterWeightCaption"), array[1]);
				}
				return InventoryParamsHelper.Split(text);
			}
			return text;
		}

		public static string SplitWithCaptionOutline(string paramz)
		{
			InventoryParamsHelper._sb.Length = 0;
			string[] array = InventoryParamsHelper.Split(paramz).Split(new char[] { '\n' });
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(InventoryParamsHelper.paramsDividers);
				if (array2[0].Trim(new char[] { ' ' }).Length != 0)
				{
					array2[0] = string.Format("<color={1}>{0}</color>", array2[0], "#AAAAAAFF");
					for (int j = 0; j < array2.Length; j++)
					{
						string text = array2[j];
						InventoryParamsHelper._sb.Append(text);
						if (j != array2.Length - 1)
						{
							InventoryParamsHelper._sb.Append(InventoryParamsHelper.paramsDividers[0]);
						}
					}
					InventoryParamsHelper._sb.Append('\n');
				}
			}
			return InventoryParamsHelper._sb.ToString();
		}

		public static string SplitWithCaptionOutlineForBoats(string paramz, string except)
		{
			InventoryParamsHelper._sb.Length = 0;
			string[] array = InventoryParamsHelper.Split(paramz).Split(new char[] { '\n' });
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(InventoryParamsHelper.paramsDividers);
				string text = array2[0].ToLower().Trim(InventoryParamsHelper.trimmerChars);
				if (text.Length != 0 && (string.IsNullOrEmpty(except) || !text.Contains(except.ToLower())))
				{
					array2[0] = string.Format("<color=#7C7C7CFF>{0}</color>: <pos=60%><b>", array2[0]);
					foreach (string text2 in array2)
					{
						InventoryParamsHelper._sb.Append(text2);
					}
					InventoryParamsHelper._sb.Append("</b>\n");
				}
			}
			return InventoryParamsHelper._sb.ToString();
		}

		public static string FixReelParams(string paramz)
		{
			int num = paramz.IndexOfAny(InventoryParamsHelper.splitterChars);
			if (num > 0 && num < paramz.Length)
			{
				return paramz.Insert(num, ":1");
			}
			return paramz;
		}

		public static string ParseParamsInfo(InventoryItem ii, bool addDurability = true)
		{
			string text = ((!(ii is Reel)) ? ii.Params : InventoryParamsHelper.FixReelParams(ii.Params));
			Chum chum = ii as Chum;
			string text2 = string.Empty;
			if (chum != null)
			{
				text2 += InventoryParamsHelper.ParseChumIngredient<ChumBase>(chum.ChumBase, "ChumBasesCaption");
				text2 += InventoryParamsHelper.ParseChumIngredient<ChumAroma>(chum.ChumAroma, "ChumAromasCaption");
				text2 += InventoryParamsHelper.ParseChumIngredient<ChumParticle>(chum.ChumParticle, "ChumParticlesCaption");
				text2 += string.Format("{0}: {1} {2}; ", ScriptLocalization.Get("WeightCaption"), MeasuringSystemManager.Kilograms2Grams((chum.Weight == null) ? 0f : ((float)chum.Weight.Value)), MeasuringSystemManager.GramsOzWeightSufix());
			}
			else if (!ii.IsUnwearable && addDurability)
			{
				int? maxDurability = ii.MaxDurability;
				int num = ((maxDurability == null) ? 0 : maxDurability.Value);
				float num2 = ((num <= 0) ? 0f : ((float)ii.Durability / (float)num * 100f));
				string text3 = ((num2 >= 33f) ? ((num2 > 66f) ? "#ffffffff" : "#ffc300ff") : "#ff0000ff");
				text2 = string.Format("{0}: <color={3}>{1}</color>/{2}; ", new object[]
				{
					ScriptLocalization.Get("DurabilityCaption"),
					ii.Durability,
					num,
					text3
				});
			}
			if (chum == null)
			{
				text2 += text;
			}
			return text2;
		}

		public static string Split(string paramsInfo)
		{
			return paramsInfo.Split(InventoryParamsHelper.splitterChars).Aggregate(string.Empty, (string current, string s) => current + s.Trim(InventoryParamsHelper.trimmerChars) + "\n").Trim(InventoryParamsHelper.trimmerChars);
		}

		private static string ParseChumIngredient<T>(List<T> ingredient, string ingredientLoc) where T : ChumIngredient
		{
			string text = string.Empty;
			if (ingredient.Count > 0)
			{
				text += string.Format("{0}; ", ScriptLocalization.Get(ingredientLoc));
				for (int i = 0; i < ingredient.Count; i++)
				{
					T t = ingredient[i];
					text += string.Format("{0}: {1}%; ", t.Name, t.Percentage);
				}
			}
			return text;
		}

		public const string NormalColor = "#ffffffff";

		public const string Low33PrcColor = "#ff0000ff";

		public const string Low66PrcColor = "#ffc300ff";

		public const string DescriptionHeaderColor = "#AAAAAAFF";

		private const string BoatParamsFormatStr = "<color=#7C7C7CFF>{0}</color>: <pos=60%><b>";

		private const string reelRatioFixStr = ":1";

		private static char[] trimmerChars = new char[] { ' ', '\n' };

		private static char[] splitterChars = new char[] { ';', '；' };

		private static char[] paramsDividers = new char[] { ':', '：' };

		private static StringBuilder _sb = new StringBuilder();
	}
}
