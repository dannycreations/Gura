using System;
using System.Text;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	internal class StringUtils
	{
		public static string UnescapeAndUnquote(string stringToUnescape)
		{
			if (stringToUnescape == null)
			{
				throw new ArgumentNullException("stringToUnescape");
			}
			int num = 0;
			int num2 = stringToUnescape.Length;
			bool flag = false;
			string text = stringToUnescape;
			if (stringToUnescape.Length > 0 && (stringToUnescape[0] == '"' || stringToUnescape[0] == '\''))
			{
				num++;
				num2 -= 2;
				flag = stringToUnescape[0] == '\'';
			}
			if (num != 0 || num2 != stringToUnescape.Length || stringToUnescape.IndexOf('\\') >= 0)
			{
				StringBuilder stringBuilder = new StringBuilder(num2);
				int num3 = num;
				int num4 = 0;
				int num5 = num + num2;
				for (int i = num; i < num5; i++)
				{
					char c = stringToUnescape[i];
					if (c == '\\')
					{
						int num6 = 1;
						if (num4 != 0)
						{
							stringBuilder.Append(stringToUnescape, num3, num4);
							num4 = 0;
						}
						char c2 = stringToUnescape[i + 1];
						switch (c2)
						{
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
							stringBuilder.Append((char)StringUtils.StringToInt32(stringToUnescape, i + 1, 3, null));
							num6 = 3;
							break;
						default:
							switch (c2)
							{
							case 'r':
								stringBuilder.Append('\r');
								break;
							default:
								if (c2 != '"')
								{
									if (c2 != '\'')
									{
										if (c2 != '\\')
										{
											if (c2 != 'b')
											{
												if (c2 != 'f')
												{
													if (c2 != 'n')
													{
														throw new InvalidOperationException(string.Format(Resources.EXCEPTION_STRINGUTILS_UNEXPECTEDESCAPESEQ, "//" + c2));
													}
													stringBuilder.Append('\n');
												}
												else
												{
													stringBuilder.Append('\f');
												}
											}
											else
											{
												stringBuilder.Append('\b');
											}
										}
										else
										{
											stringBuilder.Append('\\');
										}
									}
									else
									{
										stringBuilder.Append('\'');
									}
								}
								else
								{
									stringBuilder.Append('"');
								}
								break;
							case 't':
								stringBuilder.Append('\t');
								break;
							case 'u':
								stringBuilder.Append((char)StringUtils.HexStringToUInt32(stringToUnescape, i + 2, 4));
								num6 = 5;
								break;
							case 'x':
								stringBuilder.Append((char)StringUtils.HexStringToUInt32(stringToUnescape, i + 2, 2));
								num6 = 3;
								break;
							}
							break;
						}
						num3 = i + num6 + 1;
						i += num6;
					}
					else
					{
						num4++;
					}
				}
				if (num4 != 0)
				{
					stringBuilder.Append(stringToUnescape, num3, num4);
				}
				text = stringBuilder.ToString();
			}
			if (flag && string.IsNullOrEmpty(text) && text.Length != 1)
			{
				throw new InvalidOperationException(Resources.EXCEPTION_TOKENIZER_INVALIDCHARLITERAL);
			}
			return text;
		}

		private static int StringToInt32(string value, int offset, int count, IFormatProvider formatProvider = null)
		{
			uint num = 0U;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				char c = value[offset + i];
				if (i == 0 && c == '-')
				{
					flag = true;
				}
				else
				{
					if (c < '0' || c > '9')
					{
						throw new FormatException();
					}
					num = checked(10U * num + (uint)(c - '0'));
				}
			}
			if (flag)
			{
				return (int)(-(int)num);
			}
			return (int)num;
		}

		private static uint HexStringToUInt32(string value, int offset, int count)
		{
			uint num = 0U;
			for (int i = 0; i < count; i++)
			{
				char c = value[offset + i];
				uint num2;
				if (c >= '0' && c <= '9')
				{
					num2 = (uint)(c - '0');
				}
				else if (c >= 'a' && c <= 'f')
				{
					num2 = (uint)('\n' + (c - 'a'));
				}
				else
				{
					if (c < 'A' || c > 'F')
					{
						throw new FormatException();
					}
					num2 = (uint)('\n' + (c - 'A'));
				}
				num = 16U * num + num2;
			}
			return num;
		}
	}
}
