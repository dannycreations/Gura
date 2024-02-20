using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public static class Tokenizer
	{
		public static IEnumerable<Token> Tokenize(string expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			int line = 1;
			int col = 1;
			int i = 0;
			while (i < expression.Length)
			{
				char charCode = expression[i];
				Token current = default(Token);
				if (charCode == '\n')
				{
					line++;
					col = 0;
				}
				foreach (string text in Tokenizer.Symbols)
				{
					if (Tokenizer.Match(expression, i, text) && (!current.IsValid || text.Length >= current.TokenLength))
					{
						current = new Token(Tokenizer.TokensBySymbols[text], text, line, col, text.Length);
					}
				}
				if (current.IsValid)
				{
					goto IL_29B;
				}
				if (char.IsDigit(charCode) || charCode == '.')
				{
					current = Tokenizer.LookForNumber(expression, i, line, col);
					goto IL_29B;
				}
				if (charCode == '"' || charCode == '\'')
				{
					current = Tokenizer.LookForLiteral(expression, charCode, i, line, col);
					goto IL_29B;
				}
				if (char.IsLetter(charCode) || charCode == '_')
				{
					current = Tokenizer.LookForIdentifier(expression, i, line, col);
					goto IL_29B;
				}
				if (!char.IsWhiteSpace(charCode))
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_TOKENIZER_UNEXPECTEDSYMBOL, charCode, line, col), new Token(TokenType.None, charCode.ToString(), line, col, 1));
				}
				IL_2FF:
				i++;
				col++;
				continue;
				IL_29B:
				i += current.TokenLength - 1;
				col += current.TokenLength - 1;
				if (current.IsValid)
				{
					yield return current;
					goto IL_2FF;
				}
				goto IL_2FF;
			}
			yield break;
		}

		private static bool Match(string expression, int offset, string tokenToMatch)
		{
			if (tokenToMatch.Length + offset > expression.Length)
			{
				return false;
			}
			int num = 0;
			while (offset < expression.Length && num < tokenToMatch.Length)
			{
				if (expression[offset] != tokenToMatch[num])
				{
					return false;
				}
				offset++;
				num++;
			}
			return true;
		}

		private static Token LookForIdentifier(string expression, int offset, int line, int col)
		{
			int num = offset;
			int num2 = -1;
			while (offset < expression.Length)
			{
				char c = expression[offset];
				if (char.IsLetterOrDigit(c) || c == '_')
				{
					if (num2 < 0)
					{
						num2 = offset - num;
					}
				}
				else if (Tokenizer.TokensBySymbols.ContainsKey(expression[offset].ToString()) || c == '"' || num2 >= 0)
				{
					break;
				}
				offset++;
			}
			string text = expression.Substring(num, offset - num);
			bool flag = num2 < 0;
			string text2 = ((!flag) ? text.Substring(num2) : text);
			return new Token((!flag) ? TokenType.Identifier : TokenType.None, text2, line, (!flag) ? (col + num2) : col, text.Length - num2);
		}

		private static Token LookForLiteral(string expression, char termChar, int offset, int line, int col)
		{
			int num = offset;
			for (offset++; offset < expression.Length; offset++)
			{
				char c = expression[offset];
				if (c == termChar)
				{
					int num2 = 0;
					int num3 = offset - 1;
					while (num3 >= 0 && expression[num3] == '\\')
					{
						num2++;
						num3--;
					}
					if (num2 % 2 == 0)
					{
						offset++;
						break;
					}
				}
			}
			string text = expression.Substring(num, offset - num);
			return new Token(TokenType.Literal, text, line, col, text.Length);
		}

		private static Token LookForNumber(string expression, int offset, int line, int col)
		{
			int num = 0;
			int num2 = offset;
			while (offset < expression.Length)
			{
				if (num == 3)
				{
					break;
				}
				char c = expression[offset];
				if (c < '0' || c > '9')
				{
					char c2 = char.ToLowerInvariant(expression[offset]);
					switch (c2)
					{
					case '+':
					case '-':
						if (num != 2)
						{
							num = 3;
							offset--;
						}
						break;
					default:
						switch (c2)
						{
						case 'd':
						case 'f':
							break;
						case 'e':
							if (num != 2)
							{
								num = 2;
							}
							else
							{
								offset--;
								num = 3;
							}
							goto IL_146;
						default:
							if (c2 != 'l' && c2 != 'm' && c2 != 'u')
							{
								offset--;
								num = 3;
								goto IL_146;
							}
							break;
						}
						if (expression.Length > offset + 1 && ((char.ToLowerInvariant(expression[offset]) == 'u' && char.ToLowerInvariant(expression[offset + 1]) == 'l') || (char.ToLowerInvariant(expression[offset]) == 'l' && char.ToLowerInvariant(expression[offset + 1]) == 'u')))
						{
							offset++;
						}
						num = 3;
						break;
					case '.':
						if (num == 0)
						{
							num = 1;
						}
						else
						{
							num = 3;
							offset--;
						}
						break;
					}
				}
				IL_146:
				offset++;
			}
			string text = expression.Substring(num2, offset - num2).ToLowerInvariant();
			return new Token(TokenType.Number, text, line, col, text.Length);
		}

		private static readonly Dictionary<string, TokenType> TokensBySymbols = (from field in typeof(TokenType).GetFields()
			from tokenAttribute in field.GetCustomAttributes(typeof(TokenAttribute), true).Cast<TokenAttribute>()
			select new KeyValuePair<string, TokenType>(tokenAttribute.Value, (TokenType)Enum.Parse(typeof(TokenType), field.Name))).ToDictionary((KeyValuePair<string, TokenType> kv) => kv.Key, (KeyValuePair<string, TokenType> kv) => kv.Value);

		private static readonly string[] Symbols = Tokenizer.TokensBySymbols.Keys.ToArray<string>();
	}
}
