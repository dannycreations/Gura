using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public class Parser
	{
		static Parser()
		{
			TokenType[][] array = new TokenType[][]
			{
				new TokenType[]
				{
					TokenType.Resolve,
					TokenType.NullResolve,
					TokenType.Call,
					TokenType.Typeof,
					TokenType.Default
				},
				new TokenType[] { TokenType.New },
				new TokenType[]
				{
					TokenType.Plus,
					TokenType.Minus,
					TokenType.Not,
					TokenType.Compl,
					TokenType.Convert
				},
				new TokenType[]
				{
					TokenType.Div,
					TokenType.Mul,
					TokenType.Pow,
					TokenType.Mod
				},
				new TokenType[]
				{
					TokenType.Add,
					TokenType.Subtract
				},
				new TokenType[]
				{
					TokenType.Lshift,
					TokenType.Rshift
				},
				new TokenType[]
				{
					TokenType.Gt,
					TokenType.Gte,
					TokenType.Lt,
					TokenType.Lte,
					TokenType.Is,
					TokenType.As
				},
				new TokenType[]
				{
					TokenType.Eq,
					TokenType.Neq
				},
				new TokenType[] { TokenType.And },
				new TokenType[] { TokenType.Xor },
				new TokenType[] { TokenType.Or },
				new TokenType[] { TokenType.AndAlso },
				new TokenType[] { TokenType.OrElse },
				new TokenType[] { TokenType.Coalesce },
				new TokenType[] { TokenType.Cond },
				new TokenType[]
				{
					TokenType.Colon,
					TokenType.Lambda
				}
			};
			for (int i = 0; i < array.Length; i++)
			{
				TokenType[] array2 = array[i];
				foreach (TokenType tokenType in array2)
				{
					Parser.TokenPrecedence.Add((int)tokenType, i);
				}
			}
		}

		private Parser(IEnumerable<Token> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}
			this.tokens = new List<Token>((tokens as List<Token>) ?? new List<Token>(tokens));
			this.stack = new Stack<ParseTreeNode>();
		}

		public static ParseTreeNode Parse(IEnumerable<Token> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}
			Parser parser = new Parser(tokens);
			parser.Expression(TokenType.None, null);
			if (parser.stack.Count == 0)
			{
				throw new ExpressionParserException(Resources.EXCEPTION_PARSER_EXPRESSIONISEMPTY, null);
			}
			return parser.stack.Pop();
		}

		private bool Expression(TokenType @operator = TokenType.None, TokenType[] terminator = null)
		{
			int num = 0;
			bool flag = false;
			while (this.tokens.Count > 0)
			{
				Token token = this.tokens.Dequeue<Token>();
				try
				{
					if (num == 0 && Parser.UnaryReplacement.ContainsKey((int)token.Type))
					{
						token = new Token((TokenType)Parser.UnaryReplacement[(int)token.Type], token.Value, token.LineNumber, token.ColumnNumber, token.TokenLength);
					}
					if ((token.Type == TokenType.Lparen && num > 0) || token.Type == TokenType.Lbracket || token.Type == TokenType.NullIndex)
					{
						Token token2 = new Token(TokenType.Call, token.Value, token.LineNumber, token.ColumnNumber, token.TokenLength);
						token = new Token(TokenType.Arguments, token.Value, token.LineNumber, token.ColumnNumber, token.TokenLength);
						this.tokens.Insert(0, token);
						this.tokens.Insert(0, token2);
					}
					else
					{
						if (terminator != null && Array.IndexOf<TokenType>(terminator, token.Type) >= 0)
						{
							this.tokens.Insert(0, token);
							break;
						}
						ParseTreeNode node = new ParseTreeNode(token);
						switch (token.Type)
						{
						case TokenType.Number:
						case TokenType.Literal:
						case TokenType.Identifier:
							this.stack.Push(node);
							flag = true;
							goto IL_902;
						case TokenType.Add:
						case TokenType.Subtract:
						case TokenType.Div:
						case TokenType.Mul:
						case TokenType.Pow:
						case TokenType.Mod:
						case TokenType.And:
						case TokenType.Or:
						case TokenType.Xor:
						case TokenType.Lshift:
						case TokenType.Rshift:
						case TokenType.AndAlso:
						case TokenType.OrElse:
						case TokenType.Gt:
						case TokenType.Gte:
						case TokenType.Lt:
						case TokenType.Lte:
						case TokenType.Eq:
						case TokenType.Neq:
						case TokenType.Is:
						case TokenType.As:
						case TokenType.Colon:
						case TokenType.Comma:
						case TokenType.Coalesce:
						case TokenType.Resolve:
						case TokenType.NullResolve:
						case TokenType.Lambda:
						case TokenType.Call:
							if (token.Type == TokenType.Lt && this.GenericArguments(token))
							{
								flag = true;
								continue;
							}
							if (@operator != TokenType.None && this.ComputePrecedence(node.Type, @operator) <= 0)
							{
								this.tokens.Insert(0, token);
								return flag;
							}
							this.stack.Push(node);
							if (!this.Expression(node.Type, terminator))
							{
								throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_OPREQUIRESSECONDOPERAND, token.Type), token);
							}
							this.CombineBinary(node.Lexeme);
							if (token.Type == TokenType.Call && node.Count == 2 && node[0].Type == TokenType.Identifier && (string.Equals(node[0].Value, "unchecked", StringComparison.Ordinal) || string.Equals(node[0].Value, "checked", StringComparison.Ordinal) || string.Equals(node[0].Value, "typeof", StringComparison.Ordinal) || string.Equals(node[0].Value, "default", StringComparison.Ordinal)) && node[1].Count == 1)
							{
								TokenType tokenType = ((!string.Equals(node[0].Value, "unchecked", StringComparison.Ordinal)) ? ((!string.Equals(node[0].Value, "checked", StringComparison.Ordinal)) ? ((!string.Equals(node[0].Value, "typeof", StringComparison.Ordinal)) ? ((!string.Equals(node[0].Value, "default", StringComparison.Ordinal)) ? TokenType.Call : TokenType.Default) : TokenType.Typeof) : TokenType.CheckedScope) : TokenType.UncheckedScope);
								ParseTreeNode parseTreeNode = node[1].ChangeType(tokenType);
								this.stack.Pop();
								this.stack.Push(parseTreeNode);
							}
							else if (token.Type == TokenType.Lambda)
							{
								ParseTreeNode parseTreeNode2 = this.stack.Pop();
								ParseTreeNode parseTreeNode3 = parseTreeNode2[0];
								TokenType type = parseTreeNode3.Type;
								switch (type)
								{
								case TokenType.Convert:
								case TokenType.Group:
								{
									ParseTreeNode parseTreeNode4 = parseTreeNode3.ChangeType(TokenType.Arguments);
									parseTreeNode2.RemoveAt(0);
									parseTreeNode2.Insert(0, parseTreeNode4);
									break;
								}
								default:
									if (type != TokenType.Identifier)
									{
										throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_UNEXPECTEDTOKEN, token), token);
									}
									parseTreeNode2.RemoveAt(0);
									parseTreeNode2.Insert(0, new ParseTreeNode(TokenType.Arguments, parseTreeNode3.Lexeme, "(") { parseTreeNode3 });
									break;
								}
								this.stack.Push(parseTreeNode2);
							}
							flag = true;
							goto IL_902;
						case TokenType.Plus:
						case TokenType.Minus:
						case TokenType.Compl:
						case TokenType.Not:
						case TokenType.Convert:
							this.stack.Push(node);
							if (!this.Expression(node.Type, terminator))
							{
								throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_OPREQUIRESOPERAND, token.Type), token);
							}
							this.CombineUnary(node.Lexeme);
							flag = true;
							goto IL_902;
						case TokenType.Cond:
						{
							if (@operator != TokenType.None && this.ComputePrecedence(node.Type, @operator) <= 0)
							{
								this.tokens.Insert(0, token);
								return flag;
							}
							if (this.NullableType(token))
							{
								flag = true;
								continue;
							}
							this.stack.Push(node);
							int num2 = this.FindCondClosingToken();
							if (num2 < 0)
							{
								throw new ExpressionParserException(Resources.EXCEPTION_PARSER_COLONISEXPRECTED, token);
							}
							TokenType[] array = Parser.CondTerm;
							this.Expression(TokenType.None, array);
							this.CheckAndConsumeToken(TokenType.Colon, TokenType.None);
							array = Parser.UnionTerminators(Parser.DefaultTerm, terminator).ToArray<TokenType>();
							this.Expression(TokenType.None, array);
							this.CombineTernary(node.Lexeme);
							flag = true;
							goto IL_902;
						}
						case TokenType.New:
						{
							this.stack.Push(node);
							if (!this.Expression(node.Type, Parser.UnionTerminators(terminator, Parser.NewTerm)))
							{
								throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_OPREQUIRESOPERAND, token.Type), token);
							}
							this.CombineUnary(node.Lexeme);
							this.CheckAndConsumeToken(TokenType.Call, TokenType.None);
							ParseTreeNode parseTreeNode5 = new ParseTreeNode(this.tokens.Dequeue<Token>());
							this.stack.Push(parseTreeNode5);
							while (this.Expression(parseTreeNode5.Type, Parser.DefaultTerm))
							{
								this.CombineUnary(parseTreeNode5.Lexeme);
								if (this.tokens.Count == 0 || this.tokens[0].Type != TokenType.Comma)
								{
									break;
								}
								this.CheckAndConsumeToken(TokenType.Comma, TokenType.None);
							}
							this.CheckAndConsumeToken(TokenType.Rparen, TokenType.Rbracket);
							this.CombineUnary(parseTreeNode5.Lexeme);
							flag = true;
							goto IL_902;
						}
						case TokenType.Lparen:
						case TokenType.Lbracket:
						case TokenType.NullIndex:
						case TokenType.Arguments:
							if (token.Type == TokenType.Lparen)
							{
								node = node.ChangeType(TokenType.Group);
							}
							this.stack.Push(node);
							while (this.Expression(node.Type, Parser.DefaultTerm))
							{
								this.CombineUnary(node.Lexeme);
								if (this.tokens.Count == 0 || this.tokens[0].Type != TokenType.Comma)
								{
									break;
								}
								this.CheckAndConsumeToken(TokenType.Comma, TokenType.None);
							}
							this.CheckAndConsumeToken(TokenType.Rparen, TokenType.Rbracket);
							if (token.Type == TokenType.Lparen && num == 0 && node.Count == 1 && (node[0].Type == TokenType.Identifier || node[0].Type == TokenType.Resolve) && this.Expression(TokenType.Convert, terminator) && this.stack.Any((ParseTreeNode n) => object.ReferenceEquals(n, node)))
							{
								this.CombineUnary(node.Lexeme);
								this.stack.Pop();
								this.stack.Push(node.ChangeType(TokenType.Convert));
							}
							flag = true;
							goto IL_902;
						}
						throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_UNEXPECTEDTOKEN, token), token);
						IL_902:
						num++;
					}
				}
				catch (ExpressionParserException)
				{
					throw;
				}
				catch (ThreadAbortException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new ExpressionParserException(ex.Message, ex, token);
				}
			}
			return flag;
		}

		private bool GenericArguments(Token currentToken)
		{
			if (this.stack.Count == 0 || this.stack.Peek().Type != TokenType.Identifier)
			{
				return false;
			}
			int num = this.FindGenericArgClosingToken();
			if (num < 0)
			{
				return false;
			}
			ParseTreeNode parseTreeNode = new ParseTreeNode(TokenType.Arguments, currentToken, currentToken.Value);
			this.stack.Push(parseTreeNode);
			Token token = default(Token);
			Token token2;
			for (;;)
			{
				bool flag = false;
				while (this.Expression(parseTreeNode.Type, Parser.GenArgTerm))
				{
					flag = true;
					this.CombineUnary(currentToken);
					if (this.tokens.Count == 0 || this.tokens[0].Type != TokenType.Comma)
					{
						break;
					}
					this.CheckAndConsumeToken(TokenType.Comma, TokenType.None);
					flag = false;
				}
				token2 = this.tokens.Dequeue<Token>();
				if (token2.Type == TokenType.Rshift)
				{
					this.tokens.Insert(0, new Token(TokenType.Gt, ">", token2.LineNumber, token2.ColumnNumber + 1, token2.TokenLength - 1));
				}
				else if (token2.Type != TokenType.Comma && token2.Type != TokenType.Gt)
				{
					break;
				}
				if (!flag)
				{
					this.stack.Push(new ParseTreeNode(TokenType.Identifier, token2, string.Empty));
					this.CombineUnary(currentToken);
				}
				if (token2.Type == TokenType.Gt || token2.Type == TokenType.Rshift)
				{
					goto IL_191;
				}
			}
			throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEOTHEREXPECTED, TokenType.Gt), token2);
			IL_191:
			this.CombineUnary(currentToken);
			return true;
		}

		private bool NullableType(Token currentToken)
		{
			if (this.stack.Count == 0)
			{
				return false;
			}
			ParseTreeNode parseTreeNode = this.stack.Peek();
			if ((parseTreeNode.Type != TokenType.Identifier && parseTreeNode.Type != TokenType.Resolve) || (this.tokens.Count != 0 && Array.IndexOf<TokenType>(Parser.NullableTerm, this.tokens[0].Type) < 0))
			{
				return false;
			}
			ParseTreeNode parseTreeNode2 = this.stack.Pop();
			ParseTreeNode parseTreeNode3 = new ParseTreeNode(TokenType.Arguments, currentToken, "<");
			ParseTreeNode parseTreeNode4 = new ParseTreeNode(TokenType.Identifier, currentToken, typeof(Nullable).Name);
			parseTreeNode3.Add(parseTreeNode2);
			parseTreeNode4.Add(parseTreeNode3);
			this.stack.Push(parseTreeNode4);
			return true;
		}

		private int FindCondClosingToken()
		{
			int num = -1;
			for (int i = 0; i < this.tokens.Count; i++)
			{
				if (this.tokens[i].Type == TokenType.Colon)
				{
					num = i + 1;
				}
				else if (this.tokens[i].Type == TokenType.Comma)
				{
					break;
				}
			}
			return num;
		}

		private int FindGenericArgClosingToken()
		{
			int num = 0;
			for (int i = 0; i < this.tokens.Count; i++)
			{
				TokenType type = this.tokens[i].Type;
				switch (type)
				{
				case TokenType.Gt:
					num--;
					if (num < 0)
					{
						return i;
					}
					break;
				default:
					switch (type)
					{
					case TokenType.Comma:
					case TokenType.Resolve:
						break;
					default:
						if (type != TokenType.Identifier)
						{
							if (type != TokenType.Rshift)
							{
								if (type != TokenType.Cond)
								{
									return -1;
								}
							}
							else
							{
								num -= 2;
								if (num < 0)
								{
									return i;
								}
							}
						}
						break;
					}
					break;
				case TokenType.Lt:
					num++;
					break;
				}
			}
			return -1;
		}

		private void CheckAndConsumeToken(TokenType expectedType1, TokenType expectedType2 = TokenType.None)
		{
			Token token = ((this.tokens.Count <= 0) ? default(Token) : this.tokens.Dequeue<Token>());
			TokenType type = token.Type;
			if (type == TokenType.None || (type != expectedType1 && type != expectedType2))
			{
				string text = ((expectedType2 == TokenType.None) ? expectedType1.ToString() : (expectedType1.ToString() + ", " + expectedType2.ToString()));
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEOTHEREXPECTED, text), token);
			}
		}

		private void CombineUnary(Token op)
		{
			if (this.stack.Count < 2)
			{
				throw new ExpressionParserException(Resources.EXCEPTION_PARSER_UNARYOPREQOPERAND, op);
			}
			ParseTreeNode parseTreeNode = this.stack.Pop();
			ParseTreeNode parseTreeNode2 = this.stack.Pop();
			parseTreeNode2.Add(parseTreeNode);
			this.stack.Push(parseTreeNode2);
		}

		private void CombineBinary(Token op)
		{
			if (this.stack.Count < 3)
			{
				throw new ExpressionParserException(Resources.EXCEPTION_PARSER_BINARYOPREQOPERAND, op);
			}
			ParseTreeNode parseTreeNode = this.stack.Pop();
			ParseTreeNode parseTreeNode2 = this.stack.Pop();
			ParseTreeNode parseTreeNode3 = this.stack.Pop();
			parseTreeNode2.Add(parseTreeNode3);
			parseTreeNode2.Add(parseTreeNode);
			this.stack.Push(parseTreeNode2);
		}

		private void CombineTernary(Token op)
		{
			if (this.stack.Count < 4)
			{
				throw new ExpressionParserException(Resources.EXCEPTION_PARSER_TERNARYOPREQOPERAND, op);
			}
			ParseTreeNode parseTreeNode = this.stack.Pop();
			ParseTreeNode parseTreeNode2 = this.stack.Pop();
			ParseTreeNode parseTreeNode3 = this.stack.Pop();
			ParseTreeNode parseTreeNode4 = this.stack.Pop();
			parseTreeNode3.Add(parseTreeNode4);
			parseTreeNode3.Add(parseTreeNode2);
			parseTreeNode3.Add(parseTreeNode);
			this.stack.Push(parseTreeNode3);
		}

		private static TokenType[] UnionTerminators(TokenType[] first, TokenType[] second)
		{
			if (first == null)
			{
				return second;
			}
			if (second == null)
			{
				return first;
			}
			if (object.ReferenceEquals(first, second))
			{
				return first;
			}
			int num = 0;
			foreach (TokenType tokenType in second)
			{
				num += ((Array.IndexOf<TokenType>(first, tokenType) < 0) ? 0 : 1);
			}
			if (num == second.Length)
			{
				return first;
			}
			int num2 = 0;
			foreach (TokenType tokenType2 in first)
			{
				num2 += ((Array.IndexOf<TokenType>(second, tokenType2) < 0) ? 0 : 1);
			}
			if (num2 == first.Length)
			{
				return second;
			}
			TokenType[] array = new TokenType[first.Length + second.Length];
			first.CopyTo(array, 0);
			second.CopyTo(array, first.Length);
			return array;
		}

		private int ComputePrecedence(TokenType tokenType1, TokenType tokenType2)
		{
			int num = 0;
			if (!Parser.TokenPrecedence.TryGetValue((int)tokenType1, out num))
			{
				num = int.MaxValue;
			}
			int num2 = 0;
			if (!Parser.TokenPrecedence.TryGetValue((int)tokenType2, out num2))
			{
				num2 = int.MaxValue;
			}
			return num2.CompareTo(num);
		}

		private static readonly Dictionary<int, int> UnaryReplacement = new Dictionary<int, int>
		{
			{ 4, 5 },
			{ 6, 7 }
		};

		private static readonly Dictionary<int, int> TokenPrecedence = new Dictionary<int, int>();

		private static readonly TokenType[] CondTerm = new TokenType[] { TokenType.Colon };

		private static readonly TokenType[] NewTerm = new TokenType[] { TokenType.Call };

		private static readonly TokenType[] GenArgTerm = new TokenType[]
		{
			TokenType.Comma,
			TokenType.Gt,
			TokenType.Rshift
		};

		private static readonly TokenType[] DefaultTerm = new TokenType[]
		{
			TokenType.Comma,
			TokenType.Rparen,
			TokenType.Rbracket
		};

		private static readonly TokenType[] NullableTerm = new TokenType[]
		{
			TokenType.Comma,
			TokenType.Rparen,
			TokenType.Gt,
			TokenType.Rshift
		};

		private readonly List<Token> tokens;

		private readonly Stack<ParseTreeNode> stack;
	}
}
