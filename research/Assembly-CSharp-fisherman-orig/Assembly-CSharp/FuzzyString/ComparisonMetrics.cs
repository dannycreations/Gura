using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuzzyString
{
	public static class ComparisonMetrics
	{
		public static double GetFuzzyEqualityScore(this string source, string target, params FuzzyStringComparisonOptions[] options)
		{
			List<double> list = new List<double>();
			if (!options.Contains(FuzzyStringComparisonOptions.CaseSensitive))
			{
				source = source.Capitalize();
				target = target.Capitalize();
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseHammingDistance) && source.Length == target.Length)
			{
				list.Add((double)(source.HammingDistance(target) / target.Length));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseJaccardDistance))
			{
				list.Add(source.JaccardDistance(target));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseJaroDistance))
			{
				list.Add(source.JaroDistance(target));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseJaroWinklerDistance))
			{
				list.Add(source.JaroWinklerDistance(target));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseNormalizedLevenshteinDistance))
			{
				list.Add(Convert.ToDouble(source.NormalizedLevenshteinDistance(target)) / Convert.ToDouble(Math.Max(source.Length, target.Length) - source.LevenshteinDistanceLowerBounds(target)));
			}
			else if (options.Contains(FuzzyStringComparisonOptions.UseLevenshteinDistance))
			{
				list.Add(Convert.ToDouble(source.LevenshteinDistance(target)) / Convert.ToDouble(source.LevenshteinDistanceUpperBounds(target)));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseLongestCommonSubsequence))
			{
				list.Add(1.0 - Convert.ToDouble((double)source.LongestCommonSubsequence(target).Length / Convert.ToDouble(Math.Min(source.Length, target.Length))));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseLongestCommonSubstring))
			{
				list.Add(1.0 - Convert.ToDouble((double)source.LongestCommonSubstring(target).Length / Convert.ToDouble(Math.Min(source.Length, target.Length))));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseSorensenDiceDistance))
			{
				list.Add(source.SorensenDiceDistance(target));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseOverlapCoefficient))
			{
				list.Add(1.0 - source.OverlapCoefficient(target));
			}
			if (options.Contains(FuzzyStringComparisonOptions.UseRatcliffObershelpSimilarity))
			{
				list.Add(1.0 - source.RatcliffObershelpSimilarity(target));
			}
			return list.Average();
		}

		public static bool ApproximatelyEquals(this string source, string target, FuzzyStringComparisonTolerance tolerance, params FuzzyStringComparisonOptions[] options)
		{
			if (options.Length == 0)
			{
				return false;
			}
			double fuzzyEqualityScore = source.GetFuzzyEqualityScore(target, options);
			if (tolerance == FuzzyStringComparisonTolerance.Strong)
			{
				return fuzzyEqualityScore < 0.25;
			}
			if (tolerance == FuzzyStringComparisonTolerance.Normal)
			{
				return fuzzyEqualityScore < 0.5;
			}
			if (tolerance == FuzzyStringComparisonTolerance.Weak)
			{
				return fuzzyEqualityScore < 0.75;
			}
			return tolerance == FuzzyStringComparisonTolerance.Manual && fuzzyEqualityScore > 0.6;
		}

		public static int HammingDistance(this string source, string target)
		{
			int num = 0;
			if (source.Length == target.Length)
			{
				for (int i = 0; i < source.Length; i++)
				{
					if (!source[i].Equals(target[i]))
					{
						num++;
					}
				}
				return num;
			}
			return 99999;
		}

		public static double JaccardDistance(this string source, string target)
		{
			return 1.0 - source.JaccardIndex(target);
		}

		public static double JaccardIndex(this string source, string target)
		{
			return Convert.ToDouble(source.Intersect(target).Count<char>()) / Convert.ToDouble(source.Union(target).Count<char>());
		}

		public static double JaroDistance(this string source, string target)
		{
			int num = source.Intersect(target).Count<char>();
			if (num == 0)
			{
				return 0.0;
			}
			string text = string.Empty;
			string text2 = string.Empty;
			IEnumerable<char> enumerable = source.Intersect(target);
			IEnumerable<char> enumerable2 = target.Intersect(source);
			foreach (char c in enumerable)
			{
				text += c;
			}
			foreach (char c2 in enumerable2)
			{
				text2 += c2;
			}
			double num2 = (double)(text.LevenshteinDistance(text2) / 2);
			return ((double)(num / source.Length + num / target.Length) + ((double)num - num2) / (double)num) / 3.0;
		}

		public static double JaroWinklerDistance(this string source, string target)
		{
			double num = source.JaroDistance(target);
			double num2 = ComparisonMetrics.CommonPrefixLength(source, target);
			return num + num2 * 0.1 * (1.0 - num);
		}

		public static double JaroWinklerDistanceWithPrefixScale(string source, string target, double p)
		{
			double num;
			if (p > 0.25)
			{
				num = 0.25;
			}
			else if (p < 0.0)
			{
				num = 0.0;
			}
			else
			{
				num = p;
			}
			double num2 = source.JaroDistance(target);
			double num3 = ComparisonMetrics.CommonPrefixLength(source, target);
			return num2 + num3 * num * (1.0 - num2);
		}

		private static double CommonPrefixLength(string source, string target)
		{
			int num = 4;
			int num2 = 0;
			if (source.Length <= 4 || target.Length <= 4)
			{
				num = Math.Min(source.Length, target.Length);
			}
			for (int i = 0; i < num; i++)
			{
				if (!source[i].Equals(target[i]))
				{
					return (double)num2;
				}
				num2++;
			}
			return (double)num2;
		}

		public static int LevenshteinDistance(this string source, string target)
		{
			if (source.Length == 0)
			{
				return target.Length;
			}
			if (target.Length == 0)
			{
				return source.Length;
			}
			int num;
			if (source[source.Length - 1] == target[target.Length - 1])
			{
				num = 0;
			}
			else
			{
				num = 1;
			}
			return Math.Min(Math.Min(source.Substring(0, source.Length - 1).LevenshteinDistance(target) + 1, source.LevenshteinDistance(target.Substring(0, target.Length - 1))) + 1, source.Substring(0, source.Length - 1).LevenshteinDistance(target.Substring(0, target.Length - 1)) + num);
		}

		public static double NormalizedLevenshteinDistance(this string source, string target)
		{
			int num = source.LevenshteinDistance(target);
			return (double)(num - source.LevenshteinDistanceLowerBounds(target));
		}

		public static int LevenshteinDistanceUpperBounds(this string source, string target)
		{
			if (source.Length == target.Length)
			{
				return source.HammingDistance(target);
			}
			if (source.Length > target.Length)
			{
				return source.Length;
			}
			if (target.Length > source.Length)
			{
				return target.Length;
			}
			return 9999;
		}

		public static int LevenshteinDistanceLowerBounds(this string source, string target)
		{
			if (source.Length == target.Length)
			{
				return 0;
			}
			return Math.Abs(source.Length - target.Length);
		}

		public static string LongestCommonSubsequence(this string source, string target)
		{
			int[,] array = ComparisonMetrics.LongestCommonSubsequenceLengthTable(source, target);
			return ComparisonMetrics.Backtrack(array, source, target, source.Length, target.Length);
		}

		private static int[,] LongestCommonSubsequenceLengthTable(string source, string target)
		{
			int[,] array = new int[source.Length + 1, target.Length + 1];
			for (int i = 0; i < source.Length + 1; i++)
			{
				array[i, 0] = 0;
			}
			for (int j = 0; j < target.Length + 1; j++)
			{
				array[0, j] = 0;
			}
			for (int k = 1; k < source.Length + 1; k++)
			{
				for (int l = 1; l < target.Length + 1; l++)
				{
					if (source[k - 1].Equals(target[l - 1]))
					{
						array[k, l] = array[k - 1, l - 1] + 1;
					}
					else
					{
						array[k, l] = Math.Max(array[k, l - 1], array[k - 1, l]);
					}
				}
			}
			return array;
		}

		private static string Backtrack(int[,] C, string source, string target, int i, int j)
		{
			if (i == 0 || j == 0)
			{
				return string.Empty;
			}
			if (source[i - 1].Equals(target[j - 1]))
			{
				return ComparisonMetrics.Backtrack(C, source, target, i - 1, j - 1) + source[i - 1];
			}
			if (C[i, j - 1] > C[i - 1, j])
			{
				return ComparisonMetrics.Backtrack(C, source, target, i, j - 1);
			}
			return ComparisonMetrics.Backtrack(C, source, target, i - 1, j);
		}

		public static string LongestCommonSubstring(this string source, string target)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
			{
				return null;
			}
			int[,] array = new int[source.Length, target.Length];
			int num = 0;
			int num2 = 0;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < source.Length; i++)
			{
				for (int j = 0; j < target.Length; j++)
				{
					if (source[i] != target[j])
					{
						array[i, j] = 0;
					}
					else
					{
						if (i == 0 || j == 0)
						{
							array[i, j] = 1;
						}
						else
						{
							array[i, j] = 1 + array[i - 1, j - 1];
						}
						if (array[i, j] > num)
						{
							num = array[i, j];
							int num3 = i - array[i, j] + 1;
							if (num2 == num3)
							{
								stringBuilder.Append(source[i]);
							}
							else
							{
								num2 = num3;
								stringBuilder.Length = 0;
								stringBuilder.Append(source.Substring(num2, i + 1 - num2));
							}
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		public static double OverlapCoefficient(this string source, string target)
		{
			return Convert.ToDouble(source.Intersect(target).Count<char>()) / Convert.ToDouble(Math.Min(source.Length, target.Length));
		}

		public static double RatcliffObershelpSimilarity(this string source, string target)
		{
			return 2.0 * Convert.ToDouble(source.Intersect(target).Count<char>()) / Convert.ToDouble(source.Length + target.Length);
		}

		public static double SorensenDiceDistance(this string source, string target)
		{
			return 1.0 - source.SorensenDiceIndex(target);
		}

		public static double SorensenDiceIndex(this string source, string target)
		{
			return 2.0 * Convert.ToDouble(source.Intersect(target).Count<char>()) / Convert.ToDouble(source.Length + target.Length);
		}

		public static double TanimotoCoefficient(this string source, string target)
		{
			double num = (double)source.Length;
			double num2 = (double)target.Length;
			double num3 = (double)source.Intersect(target).Count<char>();
			return num3 / (num + num2 - num3);
		}
	}
}
