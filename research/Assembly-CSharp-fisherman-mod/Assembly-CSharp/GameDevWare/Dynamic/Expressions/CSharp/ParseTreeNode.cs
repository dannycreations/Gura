using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public class ParseTreeNode : ILineInfo, IEnumerable<ParseTreeNode>, IEnumerable
	{
		private ParseTreeNode(TokenType type, ParseTreeNode otherNode)
		{
			if (otherNode == null)
			{
				throw new ArgumentNullException("otherNode");
			}
			this.Type = type;
			this.Lexeme = otherNode.Lexeme;
			this.Value = otherNode.Value;
			this.nodes = otherNode.nodes;
		}

		internal ParseTreeNode(TokenType type, Token lexeme, string value)
		{
			this.Type = type;
			this.Lexeme = lexeme;
			this.Value = value ?? lexeme.Value;
			this.nodes = default(ParseTreeNode.ParseTreeNodes);
		}

		internal ParseTreeNode(Token lexeme)
			: this(lexeme.Type, lexeme, lexeme.Value)
		{
		}

		public ParseTreeNode this[int index]
		{
			get
			{
				return this.nodes[index];
			}
		}

		public int Count
		{
			get
			{
				return this.nodes.Count;
			}
		}

		int ILineInfo.GetLineNumber()
		{
			return this.Lexeme.LineNumber;
		}

		int ILineInfo.GetColumnNumber()
		{
			return this.Lexeme.ColumnNumber;
		}

		int ILineInfo.GetTokenLength()
		{
			return this.Lexeme.TokenLength;
		}

		internal void Add(ParseTreeNode node)
		{
			ParseTreeNode.ParseTreeNodes.Add(ref this.nodes, node);
		}

		internal void Insert(int index, ParseTreeNode node)
		{
			ParseTreeNode.ParseTreeNodes.Insert(ref this.nodes, index, node);
		}

		internal void RemoveAt(int index)
		{
			ParseTreeNode.ParseTreeNodes.RemoveAt(ref this.nodes, index);
		}

		internal bool Remove(ParseTreeNode node)
		{
			return ParseTreeNode.ParseTreeNodes.Remove(ref this.nodes, node);
		}

		internal ParseTreeNode ChangeType(TokenType newType)
		{
			return new ParseTreeNode(newType, this);
		}

		public SyntaxTreeNode ToSyntaxTree(bool checkedScope = true)
		{
			SyntaxTreeNode syntaxTreeNode;
			try
			{
				string text = null;
				if (!ParseTreeNode.ExpressionTypeByToken.TryGetValue((int)this.Type, out text))
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENTYPE, this.Type), this);
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>
				{
					{
						"_pos",
						this.Lexeme.Position
					},
					{ "expressionType", text }
				};
				switch (this.Type)
				{
				case TokenType.Number:
				{
					bool flag = this.Value.IndexOf('f') >= 0;
					bool flag2 = this.Value.IndexOf('.') >= 0 || this.Value.IndexOf('d') >= 0;
					bool flag3 = this.Value.IndexOf('l') >= 0;
					bool flag4 = this.Value.IndexOf('u') >= 0;
					bool flag5 = this.Value.IndexOf('m') >= 0;
					if (flag5)
					{
						dictionary["type"] = typeof(decimal).FullName;
					}
					else if (flag)
					{
						dictionary["type"] = typeof(float).FullName;
					}
					else if (flag2)
					{
						dictionary["type"] = typeof(double).FullName;
					}
					else if (flag3 && !flag4)
					{
						dictionary["type"] = typeof(long).FullName;
					}
					else if (flag3)
					{
						dictionary["type"] = typeof(ulong).FullName;
					}
					else if (flag4)
					{
						dictionary["type"] = typeof(uint).FullName;
					}
					else
					{
						dictionary["type"] = typeof(int).FullName;
					}
					dictionary["value"] = this.Value.TrimEnd(new char[] { 'f', 'F', 'd', 'd', 'l', 'L', 'u', 'U', 'm', 'M' });
					goto IL_928;
				}
				case TokenType.Literal:
					dictionary["type"] = ((string.IsNullOrEmpty(this.Value) || this.Value[0] != '\'') ? typeof(string).FullName : typeof(char).FullName);
					dictionary["value"] = this.UnescapeAndUnquote(this.Value);
					goto IL_928;
				case TokenType.Identifier:
					if (this.nodes.Count == 0 && (this.Value == "true" || this.Value == "false" || this.Value == "null"))
					{
						dictionary["expressionType"] = ParseTreeNode.ExpressionTypeByToken[2];
						dictionary["type"] = ((!(this.Value == "null")) ? typeof(bool).FullName : typeof(object).FullName);
						dictionary["value"] = ((!(this.Value == "true")) ? ((!(this.Value == "false")) ? null : Constants.FalseObject) : Constants.TrueObject);
					}
					dictionary["expression"] = null;
					dictionary["arguments"] = ParseTreeNode.PrepareTypeArguments(this, 0);
					dictionary["propertyOrFieldName"] = this.Value;
					goto IL_928;
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
				case TokenType.Coalesce:
					ParseTreeNode.Ensure(this, 2, TokenType.None, TokenType.None, TokenType.None);
					dictionary["left"] = this.nodes[0].ToSyntaxTree(checkedScope);
					dictionary["right"] = this.nodes[1].ToSyntaxTree(checkedScope);
					if (checkedScope && (this.Type == TokenType.Add || this.Type == TokenType.Mul || this.Type == TokenType.Subtract))
					{
						Dictionary<string, object> dictionary2;
						(dictionary2 = dictionary)["expressionType"] = dictionary2["expressionType"] + "Checked";
					}
					goto IL_928;
				case TokenType.Plus:
				case TokenType.Minus:
				case TokenType.Compl:
				case TokenType.Not:
				case TokenType.Group:
					ParseTreeNode.Ensure(this, 1, TokenType.None, TokenType.None, TokenType.None);
					dictionary["expression"] = this.nodes[0].ToSyntaxTree(checkedScope);
					if (checkedScope && this.Type == TokenType.Minus)
					{
						Dictionary<string, object> dictionary2;
						(dictionary2 = dictionary)["expressionType"] = dictionary2["expressionType"] + "Checked";
					}
					goto IL_928;
				case TokenType.Cond:
					ParseTreeNode.Ensure(this, 3, TokenType.None, TokenType.None, TokenType.None);
					dictionary["test"] = this.nodes[0].ToSyntaxTree(checkedScope);
					dictionary["ifTrue"] = this.nodes[1].ToSyntaxTree(checkedScope);
					dictionary["ifFalse"] = this.nodes[2].ToSyntaxTree(checkedScope);
					goto IL_928;
				case TokenType.Is:
				case TokenType.As:
					ParseTreeNode.Ensure(this, 2, TokenType.None, TokenType.None, TokenType.None);
					dictionary["expression"] = this.nodes[0].ToSyntaxTree(checkedScope);
					dictionary["type"] = this.nodes[1].ToTypeName(ParseTreeNode.TypeNameOptions.All);
					goto IL_928;
				case TokenType.New:
					ParseTreeNode.Ensure(this, 2, TokenType.None, TokenType.Arguments, TokenType.None);
					if (this.nodes[1].Value == "[")
					{
						dictionary["expressionType"] = "NewArrayBounds";
					}
					dictionary["type"] = this.nodes[0].ToTypeName(ParseTreeNode.TypeNameOptions.All);
					dictionary["arguments"] = ParseTreeNode.PrepareArguments(this, 1, checkedScope);
					goto IL_928;
				case TokenType.Resolve:
				case TokenType.NullResolve:
					ParseTreeNode.Ensure(this, 2, TokenType.None, TokenType.Identifier, TokenType.None);
					dictionary["expression"] = this.nodes[0].ToSyntaxTree(checkedScope);
					dictionary["propertyOrFieldName"] = this.nodes[1].Value;
					dictionary["arguments"] = ParseTreeNode.PrepareTypeArguments(this.nodes[1], 0);
					dictionary["useNullPropagation"] = ((this.Type != TokenType.NullResolve) ? Constants.FalseObject : Constants.TrueObject);
					goto IL_928;
				case TokenType.Lambda:
					ParseTreeNode.Ensure(this, 2, TokenType.Arguments, TokenType.None, TokenType.None);
					dictionary["arguments"] = ParseTreeNode.PrepareArguments(this, 0, checkedScope);
					dictionary["expression"] = this.nodes[1].ToSyntaxTree(checkedScope);
					goto IL_928;
				case TokenType.Call:
				{
					ParseTreeNode.Ensure(this, 1, TokenType.None, TokenType.None, TokenType.None);
					bool flag6 = false;
					if (this.Value == "[" || this.Value == "?[")
					{
						dictionary["expressionType"] = ParseTreeNode.ExpressionTypeByToken[38];
						flag6 = this.Value == "?[";
					}
					dictionary["expression"] = this.nodes[0].ToSyntaxTree(checkedScope);
					dictionary["arguments"] = ParseTreeNode.PrepareArguments(this, 1, checkedScope);
					dictionary["useNullPropagation"] = ((!flag6) ? Constants.FalseObject : Constants.TrueObject);
					goto IL_928;
				}
				case TokenType.Convert:
					ParseTreeNode.Ensure(this, 2, TokenType.None, TokenType.None, TokenType.None);
					dictionary["type"] = this.nodes[0].ToTypeName(ParseTreeNode.TypeNameOptions.All);
					dictionary["expression"] = this.nodes[1].ToSyntaxTree(checkedScope);
					if (checkedScope)
					{
						Dictionary<string, object> dictionary2;
						(dictionary2 = dictionary)["expressionType"] = dictionary2["expressionType"] + "Checked";
					}
					goto IL_928;
				case TokenType.Typeof:
				case TokenType.Default:
					ParseTreeNode.Ensure(this, 1, TokenType.None, TokenType.None, TokenType.None);
					dictionary["type"] = this.nodes[0].ToTypeName(ParseTreeNode.TypeNameOptions.All);
					goto IL_928;
				case TokenType.CheckedScope:
					ParseTreeNode.Ensure(this, 1, TokenType.None, TokenType.None, TokenType.None);
					dictionary["expression"] = this.nodes[0].ToSyntaxTree(true);
					goto IL_928;
				case TokenType.UncheckedScope:
					ParseTreeNode.Ensure(this, 1, TokenType.None, TokenType.None, TokenType.None);
					dictionary["expression"] = this.nodes[0].ToSyntaxTree(false);
					goto IL_928;
				}
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEBUILDINGTREE, this.Type), this);
				IL_928:
				syntaxTreeNode = new SyntaxTreeNode(dictionary);
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
				throw new ExpressionParserException(ex.Message, ex, this);
			}
			return syntaxTreeNode;
		}

		private object ToTypeName(ParseTreeNode.TypeNameOptions options)
		{
			bool flag = (options & ParseTreeNode.TypeNameOptions.ShortNames) != ParseTreeNode.TypeNameOptions.None;
			bool flag2 = (options & ParseTreeNode.TypeNameOptions.Aliases) != ParseTreeNode.TypeNameOptions.None;
			bool flag3 = (options & ParseTreeNode.TypeNameOptions.Arrays) != ParseTreeNode.TypeNameOptions.None;
			if (this.Type == TokenType.Identifier && this.nodes.Count == 0 && flag)
			{
				string text = null;
				if (flag2 && ParseTreeNode.TypeAliases.TryGetValue(this.Value, out text))
				{
					return text;
				}
				return this.Value;
			}
			else
			{
				if (this.Type == TokenType.Call && this.nodes.Count == 2 && this.Value == "[" && this.nodes[1].Count == 0 && flag3)
				{
					return new ParseTreeNode(TokenType.Identifier, this.Lexeme, typeof(Array).Name)
					{
						new ParseTreeNode(TokenType.Arguments, this.Lexeme, "<") { this.nodes[0] }
					}.ToTypeName(ParseTreeNode.TypeNameOptions.None);
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>
				{
					{
						"_pos",
						this.Lexeme.Position
					},
					{ "expressionType", "PropertyOrField" }
				};
				if (this.Type == TokenType.Resolve)
				{
					ParseTreeNode.Ensure(this, 2, TokenType.None, TokenType.Identifier, TokenType.None);
					dictionary["expression"] = this.nodes[0].ToTypeName(ParseTreeNode.TypeNameOptions.None);
					dictionary["propertyOrFieldName"] = this.nodes[1].Value;
					dictionary["arguments"] = ParseTreeNode.PrepareTypeArguments(this.nodes[1], 0);
					dictionary["useNullPropagation"] = Constants.FalseObject;
				}
				else
				{
					if (this.Type != TokenType.Identifier)
					{
						throw new ExpressionParserException(Resources.EXCEPTION_PARSER_TYPENAMEEXPECTED, this);
					}
					string text2 = this.Value;
					if (flag2 && !ParseTreeNode.TypeAliases.TryGetValue(this.Value, out text2))
					{
						text2 = this.Value;
					}
					dictionary["expression"] = null;
					dictionary["propertyOrFieldName"] = text2;
					dictionary["useNullPropagation"] = Constants.FalseObject;
					dictionary["arguments"] = ParseTreeNode.PrepareTypeArguments(this, 0);
				}
				return new SyntaxTreeNode(dictionary);
			}
		}

		private static Dictionary<string, object> PrepareArguments(ParseTreeNode node, int argumentChildIndex, bool checkedScope)
		{
			if (argumentChildIndex >= node.Count || node[argumentChildIndex].Count == 0)
			{
				return null;
			}
			int num = 0;
			ParseTreeNode parseTreeNode = node[argumentChildIndex];
			Dictionary<string, object> dictionary = new Dictionary<string, object>(parseTreeNode.Count);
			for (int i = 0; i < parseTreeNode.Count; i++)
			{
				ParseTreeNode parseTreeNode2 = parseTreeNode[i];
				if (parseTreeNode2.Type == TokenType.Colon)
				{
					ParseTreeNode.Ensure(parseTreeNode2, 2, TokenType.Identifier, TokenType.None, TokenType.None);
					string value = parseTreeNode2[0].Value;
					dictionary[value] = parseTreeNode2[1].ToSyntaxTree(checkedScope);
				}
				else
				{
					string indexAsString = Constants.GetIndexAsString(num++);
					dictionary[indexAsString] = parseTreeNode2.ToSyntaxTree(checkedScope);
				}
			}
			return dictionary;
		}

		private static Dictionary<string, object> PrepareTypeArguments(ParseTreeNode node, int argumentChildIndex)
		{
			if (argumentChildIndex >= node.Count || node[argumentChildIndex].Count == 0)
			{
				return null;
			}
			int num = 0;
			ParseTreeNode parseTreeNode = node[argumentChildIndex];
			Dictionary<string, object> dictionary = new Dictionary<string, object>(parseTreeNode.Count);
			for (int i = 0; i < parseTreeNode.Count; i++)
			{
				ParseTreeNode parseTreeNode2 = parseTreeNode[i];
				string indexAsString = Constants.GetIndexAsString(num++);
				dictionary[indexAsString] = parseTreeNode2.ToTypeName(ParseTreeNode.TypeNameOptions.Aliases | ParseTreeNode.TypeNameOptions.Arrays);
			}
			return dictionary;
		}

		private static void Ensure(ParseTreeNode node, int childCount, TokenType childType0 = TokenType.None, TokenType childType1 = TokenType.None, TokenType childType2 = TokenType.None)
		{
			if (node.Count < childCount)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_INVALIDCHILDCOUNTOFNODE, node.Type, node.Count, childCount), node);
			}
			int i = 0;
			int num = Math.Min(3, childCount);
			while (i < num)
			{
				ParseTreeNode parseTreeNode = node[i];
				TokenType type = node[i].Type;
				if (i == 0 && childType0 != TokenType.None && childType0 != type)
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE, node.Type, type, childType0), parseTreeNode);
				}
				if (i == 1 && childType1 != TokenType.None && childType1 != type)
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE, node.Type, type, childType1), parseTreeNode);
				}
				if (i == 2 && childType2 != TokenType.None && childType2 != type)
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE, node.Type, type, childType2), parseTreeNode);
				}
				i++;
			}
		}

		private object UnescapeAndUnquote(string value)
		{
			if (value == null)
			{
				return null;
			}
			object obj;
			try
			{
				obj = StringUtils.UnescapeAndUnquote(value);
			}
			catch (InvalidOperationException ex)
			{
				throw new ExpressionParserException(ex.Message, ex, this.Lexeme);
			}
			return obj;
		}

		private void Write(StringBuilder sb, int depth)
		{
			sb.Append(' ', depth * 4).Append(this.Type).Append('\'')
				.Append(this.Value)
				.Append('\'');
			for (int i = 0; i < this.nodes.Count; i++)
			{
				sb.Append("\r\n").Append(' ', depth * 4);
				this.nodes[i].Write(sb, depth + 1);
			}
		}

		IEnumerator<ParseTreeNode> IEnumerable<ParseTreeNode>.GetEnumerator()
		{
			for (int i = 0; i < this.nodes.Count; i++)
			{
				yield return this.nodes[i];
			}
			yield break;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			this.Write(stringBuilder, 0);
			return stringBuilder.ToString();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ParseTreeNode>)this).GetEnumerator();
		}

		private static readonly Dictionary<int, string> ExpressionTypeByToken = new Dictionary<int, string>
		{
			{ 34, "PropertyOrField" },
			{ 35, "PropertyOrField" },
			{ 3, "PropertyOrField" },
			{ 2, "Constant" },
			{ 1, "Constant" },
			{ 44, "Convert" },
			{ 47, "Group" },
			{ 49, "UncheckedScope" },
			{ 48, "CheckedScope" },
			{ 5, "UnaryPlus" },
			{ 7, "Negate" },
			{ 20, "Not" },
			{ 15, "Complement" },
			{ 8, "Divide" },
			{ 9, "Multiply" },
			{ 10, "Power" },
			{ 11, "Modulo" },
			{ 4, "Add" },
			{ 6, "Subtract" },
			{ 16, "LeftShift" },
			{ 17, "RightShift" },
			{ 21, "GreaterThan" },
			{ 22, "GreaterThanOrEqual" },
			{ 23, "LessThan" },
			{ 24, "LessThanOrEqual" },
			{ 28, "TypeIs" },
			{ 29, "TypeAs" },
			{ 25, "Equal" },
			{ 26, "NotEqual" },
			{ 12, "And" },
			{ 13, "Or" },
			{ 14, "ExclusiveOr" },
			{ 18, "AndAlso" },
			{ 19, "OrElse" },
			{ 32, "Coalesce" },
			{ 27, "Condition" },
			{ 42, "Invoke" },
			{ 45, "TypeOf" },
			{ 46, "Default" },
			{ 33, "New" },
			{ 38, "Index" },
			{ 41, "Lambda" }
		};

		private static readonly Dictionary<string, string> TypeAliases = new Dictionary<string, string>
		{
			{
				"void",
				typeof(void).FullName
			},
			{
				"char",
				typeof(char).FullName
			},
			{
				"bool",
				typeof(bool).FullName
			},
			{
				"byte",
				typeof(byte).FullName
			},
			{
				"sbyte",
				typeof(sbyte).FullName
			},
			{
				"decimal",
				typeof(decimal).FullName
			},
			{
				"double",
				typeof(double).FullName
			},
			{
				"float",
				typeof(float).FullName
			},
			{
				"int",
				typeof(int).FullName
			},
			{
				"uint",
				typeof(uint).FullName
			},
			{
				"long",
				typeof(long).FullName
			},
			{
				"ulong",
				typeof(ulong).FullName
			},
			{
				"object",
				typeof(object).FullName
			},
			{
				"short",
				typeof(short).FullName
			},
			{
				"ushort",
				typeof(ushort).FullName
			},
			{
				"string",
				typeof(string).FullName
			}
		};

		private ParseTreeNode.ParseTreeNodes nodes;

		public readonly TokenType Type;

		public readonly Token Lexeme;

		public readonly string Value;

		[Flags]
		private enum TypeNameOptions
		{
			None = 0,
			Aliases = 1,
			ShortNames = 2,
			Arrays = 4,
			All = 7
		}

		private struct ParseTreeNodes
		{
			private ParseTreeNodes(ParseTreeNode item0, ParseTreeNode item1, ParseTreeNode item2)
			{
				if (item2 != null && item1 == null)
				{
					throw new ArgumentNullException("item1");
				}
				if (item1 != null && item0 == null)
				{
					throw new ArgumentNullException("item0");
				}
				this.item0 = item0;
				this.item1 = item1;
				this.item2 = item2;
				this.Count = ((item2 == null) ? ((item1 == null) ? ((item0 == null) ? 0 : 1) : 2) : 3);
				this.items = null;
			}

			private ParseTreeNodes(List<ParseTreeNode> items)
			{
				if (items == null)
				{
					throw new ArgumentNullException("items");
				}
				this.item0 = (this.item1 = (this.item2 = null));
				this.items = items;
				this.Count = items.Count;
			}

			public ParseTreeNode this[int index]
			{
				get
				{
					if (index >= this.Count || index < 0)
					{
						throw new ArgumentOutOfRangeException("index");
					}
					if (this.items != null)
					{
						return this.items[index];
					}
					switch (index)
					{
					case 0:
						return this.item0;
					case 1:
						return this.item1;
					case 2:
						return this.item2;
					default:
						throw new ArgumentOutOfRangeException("index");
					}
				}
			}

			public static void Add(ref ParseTreeNode.ParseTreeNodes nodes, ParseTreeNode node)
			{
				if (node == null)
				{
					throw new ArgumentNullException("node");
				}
				List<ParseTreeNode> list = nodes.items;
				if (list != null)
				{
					list.Add(node);
					nodes = new ParseTreeNode.ParseTreeNodes(list);
					return;
				}
				switch (nodes.Count)
				{
				case 0:
					nodes = new ParseTreeNode.ParseTreeNodes(node, null, null);
					break;
				case 1:
					nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, node, null);
					break;
				case 2:
					nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, nodes.item1, node);
					break;
				case 3:
					nodes = new ParseTreeNode.ParseTreeNodes(new List<ParseTreeNode> { nodes.item0, nodes.item1, nodes.item2, node });
					break;
				default:
					throw new ArgumentOutOfRangeException("node", "Unable to add new node. Tree is full.");
				}
			}

			public static void Insert(ref ParseTreeNode.ParseTreeNodes nodes, int index, ParseTreeNode node)
			{
				if (node == null)
				{
					throw new ArgumentNullException("node");
				}
				List<ParseTreeNode> list = nodes.items;
				if (list != null)
				{
					list.Insert(index, node);
					nodes = new ParseTreeNode.ParseTreeNodes(list);
					return;
				}
				switch (nodes.Count)
				{
				case 0:
					nodes = new ParseTreeNode.ParseTreeNodes(node, null, null);
					break;
				case 1:
					switch (index)
					{
					case 0:
						nodes = new ParseTreeNode.ParseTreeNodes(node, nodes.item0, null);
						break;
					case 1:
					case 2:
						nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, node, null);
						break;
					default:
						throw new ArgumentOutOfRangeException("index");
					}
					break;
				case 2:
					switch (index)
					{
					case 0:
						nodes = new ParseTreeNode.ParseTreeNodes(node, nodes.item0, nodes.item1);
						break;
					case 1:
						nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, node, nodes.item1);
						break;
					case 2:
						nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, nodes.item1, node);
						break;
					default:
						throw new ArgumentOutOfRangeException("index");
					}
					break;
				case 3:
					list = new List<ParseTreeNode> { nodes.item0, nodes.item1, nodes.item2 };
					list.Insert(index, node);
					nodes = new ParseTreeNode.ParseTreeNodes(list);
					break;
				default:
					throw new ArgumentOutOfRangeException("node", "Unable to add new node. Tree is full.");
				}
			}

			public static void RemoveAt(ref ParseTreeNode.ParseTreeNodes nodes, int index)
			{
				List<ParseTreeNode> list = nodes.items;
				if (list != null)
				{
					list.RemoveAt(index);
					nodes = new ParseTreeNode.ParseTreeNodes(list);
					return;
				}
				switch (index)
				{
				case 0:
					nodes = new ParseTreeNode.ParseTreeNodes(nodes.item1, nodes.item2, null);
					break;
				case 1:
					nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, nodes.item2, null);
					break;
				case 2:
					nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, nodes.item1, null);
					break;
				default:
					throw new ArgumentOutOfRangeException("index");
				}
			}

			public static bool Remove(ref ParseTreeNode.ParseTreeNodes nodes, ParseTreeNode node)
			{
				if (node == null)
				{
					throw new ArgumentNullException("node");
				}
				List<ParseTreeNode> list = nodes.items;
				if (list == null)
				{
					if (nodes.item2 == node)
					{
						nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, nodes.item1, null);
					}
					else if (nodes.item1 == node)
					{
						nodes = new ParseTreeNode.ParseTreeNodes(nodes.item0, nodes.item2, null);
					}
					else
					{
						if (nodes.item0 != node)
						{
							return false;
						}
						nodes = new ParseTreeNode.ParseTreeNodes(nodes.item1, nodes.item2, null);
					}
					return true;
				}
				if (!list.Remove(node))
				{
					return false;
				}
				nodes = new ParseTreeNode.ParseTreeNodes(list);
				return true;
			}

			public override string ToString()
			{
				return this.Count.ToString();
			}

			public readonly int Count;

			private readonly ParseTreeNode item0;

			private readonly ParseTreeNode item1;

			private readonly ParseTreeNode item2;

			private readonly List<ParseTreeNode> items;
		}
	}
}
