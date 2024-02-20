using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public static class CSharpExpressionRenderer
	{
		public static string Render(this SyntaxTreeNode node, bool checkedScope = true)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			StringBuilder stringBuilder = new StringBuilder();
			CSharpExpressionRenderer.Render(node, stringBuilder, true, checkedScope);
			return stringBuilder.ToString();
		}

		public static string Render(this Expression expression, bool checkedScope = true)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			StringBuilder stringBuilder = new StringBuilder();
			CSharpExpressionRenderer.Render(expression, stringBuilder, true, checkedScope);
			return stringBuilder.ToString();
		}

		private static void Render(SyntaxTreeNode node, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object obj = null;
			if (!node.TryGetValue("expressionType", out obj) || !(obj is string))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expressionType"));
			}
			try
			{
				string text = (string)obj;
				if (text != null)
				{
					if (CSharpExpressionRenderer.<>f__switch$map3 == null)
					{
						CSharpExpressionRenderer.<>f__switch$map3 = new Dictionary<string, int>(45)
						{
							{ "Invoke", 0 },
							{ "Index", 0 },
							{ "Enclose", 1 },
							{ "UncheckedScope", 1 },
							{ "CheckedScope", 1 },
							{ "Group", 1 },
							{ "Constant", 2 },
							{ "PropertyOrField", 3 },
							{ "TypeOf", 4 },
							{ "Default", 5 },
							{ "NewArrayBounds", 6 },
							{ "New", 6 },
							{ "UnaryPlus", 7 },
							{ "Negate", 7 },
							{ "NegateChecked", 7 },
							{ "Not", 7 },
							{ "Complement", 7 },
							{ "Divide", 8 },
							{ "Multiply", 8 },
							{ "MultiplyChecked", 8 },
							{ "Modulo", 8 },
							{ "Add", 8 },
							{ "AddChecked", 8 },
							{ "Subtract", 8 },
							{ "SubtractChecked", 8 },
							{ "LeftShift", 8 },
							{ "RightShift", 8 },
							{ "GreaterThan", 8 },
							{ "GreaterThanOrEqual", 8 },
							{ "LessThan", 8 },
							{ "LessThanOrEqual", 8 },
							{ "Equal", 8 },
							{ "NotEqual", 8 },
							{ "And", 8 },
							{ "Or", 8 },
							{ "ExclusiveOr", 8 },
							{ "AndAlso", 8 },
							{ "OrElse", 8 },
							{ "Coalesce", 8 },
							{ "Condition", 9 },
							{ "Convert", 10 },
							{ "ConvertChecked", 10 },
							{ "TypeIs", 10 },
							{ "TypeAs", 10 },
							{ "Lambda", 11 }
						};
					}
					int num;
					if (CSharpExpressionRenderer.<>f__switch$map3.TryGetValue(text, out num))
					{
						switch (num)
						{
						case 0:
							CSharpExpressionRenderer.RenderInvokeOrIndex(node, builder, checkedScope);
							break;
						case 1:
							CSharpExpressionRenderer.RenderGroup(node, builder, checkedScope);
							break;
						case 2:
							CSharpExpressionRenderer.RenderConstant(node, builder);
							break;
						case 3:
							CSharpExpressionRenderer.RenderPropertyOrField(node, builder, checkedScope);
							break;
						case 4:
							CSharpExpressionRenderer.RenderTypeOf(node, builder);
							break;
						case 5:
							CSharpExpressionRenderer.RenderDefault(node, builder);
							break;
						case 6:
							CSharpExpressionRenderer.RenderNew(node, builder, checkedScope);
							break;
						case 7:
							CSharpExpressionRenderer.RenderUnary(node, builder, wrapped, checkedScope);
							break;
						case 8:
							CSharpExpressionRenderer.RenderBinary(node, builder, wrapped, checkedScope);
							break;
						case 9:
							CSharpExpressionRenderer.RenderCondition(node, builder, wrapped, checkedScope);
							break;
						case 10:
							CSharpExpressionRenderer.RenderTypeBinary(node, builder, wrapped, checkedScope);
							break;
						case 11:
							CSharpExpressionRenderer.RenderLambda(node, builder, wrapped, checkedScope);
							break;
						case 12:
							goto IL_38C;
						default:
							goto IL_38C;
						}
						return;
					}
				}
				IL_38C:
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, text));
			}
			catch (InvalidOperationException)
			{
				throw;
			}
			catch (ThreadAbortException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_RENDERFAILED, obj, ex.Message), ex);
			}
		}

		private static void RenderTypeBinary(SyntaxTreeNode node, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			string expressionType = node.GetExpressionType(true);
			object typeName = node.GetTypeName(true);
			SyntaxTreeNode expression = node.GetExpression(true);
			Convert.ToString(typeName, Constants.DefaultFormatProvider);
			bool flag = expressionType == "ConvertChecked" || (!(expressionType == "Convert") && checkedScope);
			bool flag2 = false;
			if (flag && !checkedScope)
			{
				builder.Append("checked(");
				checkedScope = true;
				flag2 = true;
			}
			else if (!flag && checkedScope)
			{
				builder.Append("unchecked(");
				checkedScope = false;
				flag2 = true;
			}
			else if (!wrapped)
			{
				builder.Append("(");
				flag2 = true;
			}
			if (expressionType != null)
			{
				if (!(expressionType == "ConvertChecked") && !(expressionType == "Convert"))
				{
					if (!(expressionType == "TypeIs"))
					{
						if (!(expressionType == "TypeAs"))
						{
							goto IL_18C;
						}
						CSharpExpressionRenderer.Render(expression, builder, false, checkedScope);
						builder.Append(" as ");
						CSharpExpressionRenderer.RenderTypeName(typeName, builder);
					}
					else
					{
						CSharpExpressionRenderer.Render(expression, builder, false, checkedScope);
						builder.Append(" is ");
						CSharpExpressionRenderer.RenderTypeName(typeName, builder);
					}
				}
				else
				{
					builder.Append("(");
					CSharpExpressionRenderer.RenderTypeName(typeName, builder);
					builder.Append(")");
					CSharpExpressionRenderer.Render(expression, builder, false, flag);
				}
				if (flag2)
				{
					builder.Append(")");
				}
				return;
			}
			IL_18C:
			throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expressionType));
		}

		private static void RenderCondition(SyntaxTreeNode node, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object obj = null;
			if (!node.TryGetValue("test", out obj) || !(obj is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "test", node.GetTypeName(true)));
			}
			object obj2 = null;
			if (!node.TryGetValue("ifTrue", out obj2) || !(obj2 is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "ifTrue", node.GetTypeName(true)));
			}
			object obj3 = null;
			if (!node.TryGetValue("ifFalse", out obj3) || !(obj3 is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "ifFalse", node.GetTypeName(true)));
			}
			SyntaxTreeNode syntaxTreeNode = (SyntaxTreeNode)obj;
			SyntaxTreeNode syntaxTreeNode2 = (SyntaxTreeNode)obj2;
			SyntaxTreeNode syntaxTreeNode3 = (SyntaxTreeNode)obj3;
			if (!wrapped)
			{
				builder.Append("(");
			}
			CSharpExpressionRenderer.Render(syntaxTreeNode, builder, true, checkedScope);
			builder.Append(" ? ");
			CSharpExpressionRenderer.Render(syntaxTreeNode2, builder, true, checkedScope);
			builder.Append(" : ");
			CSharpExpressionRenderer.Render(syntaxTreeNode3, builder, true, checkedScope);
			if (!wrapped)
			{
				builder.Append(")");
			}
		}

		private static void RenderBinary(SyntaxTreeNode node, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			string expressionType = node.GetExpressionType(true);
			object obj = null;
			if (!node.TryGetValue("left", out obj) || !(obj is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "left", expressionType));
			}
			object obj2 = null;
			if (!node.TryGetValue("right", out obj2) || !(obj2 is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "right", expressionType));
			}
			SyntaxTreeNode syntaxTreeNode = (SyntaxTreeNode)obj;
			SyntaxTreeNode syntaxTreeNode2 = (SyntaxTreeNode)obj2;
			bool flag = expressionType == "MultiplyChecked" || expressionType == "AddChecked" || expressionType == "SubtractChecked" || (!(expressionType == "Multiply") && !(expressionType == "Add") && !(expressionType == "Subtract") && checkedScope);
			bool flag2 = false;
			if (flag && !checkedScope)
			{
				builder.Append("checked(");
				checkedScope = true;
				flag2 = true;
			}
			else if (!flag && checkedScope)
			{
				builder.Append("unchecked(");
				checkedScope = false;
				flag2 = true;
			}
			else if (!wrapped)
			{
				builder.Append("(");
				flag2 = true;
			}
			CSharpExpressionRenderer.Render(syntaxTreeNode, builder, false, flag);
			if (expressionType != null)
			{
				if (CSharpExpressionRenderer.<>f__switch$map4 == null)
				{
					CSharpExpressionRenderer.<>f__switch$map4 = new Dictionary<string, int>(23)
					{
						{ "Divide", 0 },
						{ "MultiplyChecked", 1 },
						{ "Multiply", 1 },
						{ "Modulo", 2 },
						{ "AddChecked", 3 },
						{ "Add", 3 },
						{ "SubtractChecked", 4 },
						{ "Subtract", 4 },
						{ "LeftShift", 5 },
						{ "RightShift", 6 },
						{ "GreaterThan", 7 },
						{ "GreaterThanOrEqual", 8 },
						{ "LessThan", 9 },
						{ "LessThanOrEqual", 10 },
						{ "Equal", 11 },
						{ "NotEqual", 12 },
						{ "And", 13 },
						{ "Or", 14 },
						{ "ExclusiveOr", 15 },
						{ "Power", 16 },
						{ "AndAlso", 17 },
						{ "OrElse", 18 },
						{ "Coalesce", 19 }
					};
				}
				int num;
				if (CSharpExpressionRenderer.<>f__switch$map4.TryGetValue(expressionType, out num))
				{
					switch (num)
					{
					case 0:
						builder.Append(" / ");
						break;
					case 1:
						builder.Append(" * ");
						break;
					case 2:
						builder.Append(" % ");
						break;
					case 3:
						builder.Append(" + ");
						break;
					case 4:
						builder.Append(" - ");
						break;
					case 5:
						builder.Append(" << ");
						break;
					case 6:
						builder.Append(" >> ");
						break;
					case 7:
						builder.Append(" > ");
						break;
					case 8:
						builder.Append(" >= ");
						break;
					case 9:
						builder.Append(" < ");
						break;
					case 10:
						builder.Append(" <= ");
						break;
					case 11:
						builder.Append(" == ");
						break;
					case 12:
						builder.Append(" != ");
						break;
					case 13:
						builder.Append(" & ");
						break;
					case 14:
						builder.Append(" | ");
						break;
					case 15:
						builder.Append(" ^ ");
						break;
					case 16:
						builder.Append(" ** ");
						break;
					case 17:
						builder.Append(" && ");
						break;
					case 18:
						builder.Append(" || ");
						break;
					case 19:
						builder.Append(" ?? ");
						break;
					case 20:
						goto IL_498;
					default:
						goto IL_498;
					}
					CSharpExpressionRenderer.Render(syntaxTreeNode2, builder, false, flag);
					if (flag2)
					{
						builder.Append(")");
					}
					return;
				}
			}
			IL_498:
			throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expressionType));
		}

		private static void RenderUnary(SyntaxTreeNode node, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object obj = null;
			if (!node.TryGetValue("expressionType", out obj) || !(obj is string))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expressionType"));
			}
			string text = (string)obj;
			object obj2 = null;
			if (!node.TryGetValue("expression", out obj2) || !(obj2 is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expression", text));
			}
			SyntaxTreeNode syntaxTreeNode = (SyntaxTreeNode)obj2;
			bool flag = text == "NegateChecked" || (!(text == "Negate") && checkedScope);
			bool flag2 = false;
			if (flag && !checkedScope)
			{
				builder.Append("checked(");
				checkedScope = true;
				flag2 = true;
			}
			else if (!flag && checkedScope)
			{
				builder.Append("unchecked(");
				checkedScope = false;
				flag2 = true;
			}
			else if (!wrapped)
			{
				builder.Append("(");
				flag2 = true;
			}
			if (text != null)
			{
				if (!(text == "UnaryPlus"))
				{
					if (!(text == "NegateChecked") && !(text == "Negate"))
					{
						if (!(text == "Not"))
						{
							if (!(text == "Complement"))
							{
								goto IL_1C7;
							}
							builder.Append("~");
						}
						else
						{
							builder.Append("!");
						}
					}
					else
					{
						builder.Append("-");
					}
				}
				else
				{
					builder.Append("+");
				}
				CSharpExpressionRenderer.Render(syntaxTreeNode, builder, false, flag);
				if (flag2)
				{
					builder.Append(")");
				}
				return;
			}
			IL_1C7:
			throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, text));
		}

		private static void RenderNew(SyntaxTreeNode node, StringBuilder builder, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			string expressionType = node.GetExpressionType(true);
			object typeName = node.GetTypeName(true);
			ArgumentsTree arguments = node.GetArguments(false);
			builder.Append("new ");
			CSharpExpressionRenderer.RenderTypeName(typeName, builder);
			if (expressionType == "NewArrayBounds")
			{
				builder.Append("[");
			}
			else
			{
				builder.Append("(");
			}
			CSharpExpressionRenderer.RenderArguments(arguments, builder, checkedScope);
			if (expressionType == "NewArrayBounds")
			{
				builder.Append("]");
			}
			else
			{
				builder.Append(")");
			}
		}

		private static void RenderDefault(SyntaxTreeNode node, StringBuilder builder)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object typeName = node.GetTypeName(true);
			builder.Append("default(");
			CSharpExpressionRenderer.RenderTypeName(typeName, builder);
			builder.Append(")");
		}

		private static void RenderTypeOf(SyntaxTreeNode node, StringBuilder builder)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object typeName = node.GetTypeName(true);
			builder.Append("typeof(");
			CSharpExpressionRenderer.RenderTypeName(typeName, builder);
			builder.Append(")");
		}

		private static void RenderPropertyOrField(SyntaxTreeNode node, StringBuilder builder, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			SyntaxTreeNode expression = node.GetExpression(false);
			string propertyOrFieldName = node.GetPropertyOrFieldName(true);
			bool useNullPropagation = node.GetUseNullPropagation(false);
			ArgumentsTree arguments = node.GetArguments(false);
			if (expression != null)
			{
				CSharpExpressionRenderer.Render(expression, builder, false, checkedScope);
				if (useNullPropagation)
				{
					builder.Append("?.");
				}
				else
				{
					builder.Append(".");
				}
			}
			builder.Append(propertyOrFieldName);
			if (arguments != null && arguments.Count > 0)
			{
				builder.Append("<");
				for (int i = 0; i < arguments.Count; i++)
				{
					if (i != 0)
					{
						builder.Append(",");
					}
					SyntaxTreeNode syntaxTreeNode = null;
					if (arguments.TryGetValue(i, out syntaxTreeNode))
					{
						CSharpExpressionRenderer.Render(syntaxTreeNode, builder, true, checkedScope);
					}
				}
				builder.Append(">");
			}
		}

		private static void RenderConstant(SyntaxTreeNode node, StringBuilder builder)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object obj = null;
			object obj2 = null;
			if (!node.TryGetValue("type", out obj) || !(obj is string))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "type", node.GetExpressionType(true)));
			}
			if (!node.TryGetValue("value", out obj2))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "value", node.GetExpressionType(true)));
			}
			if (obj2 == null)
			{
				builder.Append("null");
				return;
			}
			string text = Convert.ToString(obj, Constants.DefaultFormatProvider);
			string text2 = Convert.ToString(obj2, Constants.DefaultFormatProvider);
			switch (text)
			{
			case "System.Char":
			case "Char":
			case "char":
				CSharpExpressionRenderer.RenderTextLiteral(text2, builder, true);
				return;
			case "System.String":
			case "String":
			case "string":
				CSharpExpressionRenderer.RenderTextLiteral(text2, builder, false);
				return;
			case "UInt16":
			case "System.UInt16":
			case "ushort":
			case "UInt32":
			case "System.UInt32":
			case "uint":
				builder.Append(text2);
				builder.Append("u");
				return;
			case "UInt64":
			case "System.UInt64":
			case "ulong":
				builder.Append(text2);
				builder.Append("ul");
				return;
			case "Int64":
			case "System.Int64":
			case "long":
				builder.Append(text2);
				builder.Append("l");
				return;
			case "Single":
			case "System.Single":
			case "float":
				builder.Append(text2);
				builder.Append("f");
				return;
			case "Double":
			case "System.Double":
			case "double":
				builder.Append(text2);
				if (text2.IndexOf('.') == -1)
				{
					builder.Append("d");
				}
				return;
			case "Decimal":
			case "System.Decimal":
			case "decimal":
				builder.Append(text2);
				builder.Append("m");
				return;
			case "Boolean":
			case "System.Boolean":
			case "bool":
				builder.Append(text2.ToLowerInvariant());
				return;
			}
			builder.Append(text2);
		}

		private static void RenderGroup(SyntaxTreeNode node, StringBuilder builder, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			object obj = null;
			if (!node.TryGetValue("expressionType", out obj) || !(obj is string))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expressionType"));
			}
			string text = (string)obj;
			object obj2 = null;
			if (!node.TryGetValue("expression", out obj2) || !(obj2 is SyntaxTreeNode))
			{
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expression", text));
			}
			SyntaxTreeNode syntaxTreeNode = (SyntaxTreeNode)obj2;
			if (text == "UncheckedScope")
			{
				builder.Append("unchecked");
			}
			if (text == "CheckedScope")
			{
				builder.Append("checked");
			}
			builder.Append("(");
			CSharpExpressionRenderer.Render(syntaxTreeNode, builder, true, checkedScope);
			builder.Append(")");
		}

		private static void RenderInvokeOrIndex(SyntaxTreeNode node, StringBuilder builder, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			string expressionType = node.GetExpressionType(true);
			SyntaxTreeNode expression = node.GetExpression(true);
			ArgumentsTree arguments = node.GetArguments(false);
			bool useNullPropagation = node.GetUseNullPropagation(false);
			CSharpExpressionRenderer.Render(expression, builder, false, checkedScope);
			builder.Append((!(expressionType == "Invoke")) ? ((!useNullPropagation) ? "[" : "?[") : "(");
			CSharpExpressionRenderer.RenderArguments(arguments, builder, checkedScope);
			builder.Append((!(expressionType == "Invoke")) ? "]" : ")");
		}

		private static void RenderLambda(SyntaxTreeNode node, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (node == null)
			{
				throw new ArgumentException("node");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			if (!wrapped)
			{
				builder.Append("(");
			}
			ArgumentsTree arguments = node.GetArguments(false);
			SyntaxTreeNode expression = node.GetExpression(true);
			if (arguments.Count != 1)
			{
				builder.Append("(");
			}
			bool flag = true;
			foreach (SyntaxTreeNode syntaxTreeNode in arguments.Values)
			{
				if (!flag)
				{
					builder.Append(", ");
				}
				CSharpExpressionRenderer.Render(syntaxTreeNode, builder, true, checkedScope);
				flag = false;
			}
			if (arguments.Count != 1)
			{
				builder.Append(")");
			}
			builder.Append(" => ");
			CSharpExpressionRenderer.Render(expression, builder, false, checkedScope);
			if (!wrapped)
			{
				builder.Append(")");
			}
		}

		private static void RenderArguments(ArgumentsTree arguments, StringBuilder builder, bool checkedScope)
		{
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			bool flag = true;
			foreach (string text in arguments.Keys)
			{
				SortedDictionary<int, SyntaxTreeNode> sortedDictionary = new SortedDictionary<int, SyntaxTreeNode>();
				SortedDictionary<string, SyntaxTreeNode> sortedDictionary2 = new SortedDictionary<string, SyntaxTreeNode>();
				int num = 0;
				if (int.TryParse(text, out num))
				{
					sortedDictionary[num] = arguments[text];
				}
				else
				{
					sortedDictionary2[text] = arguments[text];
				}
				foreach (SyntaxTreeNode syntaxTreeNode in sortedDictionary.Values)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.Render(syntaxTreeNode, builder, true, checkedScope);
					flag = false;
				}
				foreach (KeyValuePair<string, SyntaxTreeNode> keyValuePair in sortedDictionary2)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					builder.Append(keyValuePair.Key).Append(": ");
					CSharpExpressionRenderer.Render(keyValuePair.Value, builder, true, checkedScope);
					flag = false;
				}
			}
		}

		private static void RenderTypeName(object typeName, StringBuilder builder)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (typeName is SyntaxTreeNode)
			{
				CSharpExpressionRenderer.Render((SyntaxTreeNode)typeName, builder, true, true);
			}
			else
			{
				builder.Append(Convert.ToString(typeName, Constants.DefaultFormatProvider));
			}
		}

		private static void Render(Expression expression, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			switch (expression.NodeType)
			{
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.Coalesce:
			case ExpressionType.Divide:
			case ExpressionType.Equal:
			case ExpressionType.ExclusiveOr:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.LeftShift:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.Modulo:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.NotEqual:
			case ExpressionType.Or:
			case ExpressionType.OrElse:
			case ExpressionType.Power:
			case ExpressionType.RightShift:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
				CSharpExpressionRenderer.RenderBinary((BinaryExpression)expression, builder, wrapped, checkedScope);
				break;
			case ExpressionType.ArrayLength:
				CSharpExpressionRenderer.Render(((UnaryExpression)expression).Operand, builder, false, checkedScope);
				builder.Append(".Length");
				break;
			case ExpressionType.ArrayIndex:
				CSharpExpressionRenderer.RenderArrayIndex(expression, builder, checkedScope);
				break;
			case ExpressionType.Call:
				CSharpExpressionRenderer.RenderCall((MethodCallExpression)expression, builder, checkedScope);
				break;
			case ExpressionType.Conditional:
				CSharpExpressionRenderer.RenderCondition((ConditionalExpression)expression, builder, wrapped, checkedScope);
				break;
			case ExpressionType.Constant:
				CSharpExpressionRenderer.RenderConstant((ConstantExpression)expression, builder);
				break;
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
				CSharpExpressionRenderer.RenderConvert((UnaryExpression)expression, builder, wrapped, checkedScope);
				break;
			case ExpressionType.Invoke:
			{
				InvocationExpression invocationExpression = (InvocationExpression)expression;
				CSharpExpressionRenderer.Render(invocationExpression.Expression, builder, false, checkedScope);
				builder.Append("(");
				CSharpExpressionRenderer.RenderArguments(invocationExpression.Arguments, builder, checkedScope);
				builder.Append(")");
				break;
			}
			case ExpressionType.Lambda:
				CSharpExpressionRenderer.RenderLambda((LambdaExpression)expression, builder, wrapped, checkedScope);
				break;
			case ExpressionType.ListInit:
				CSharpExpressionRenderer.RenderListInit((ListInitExpression)expression, builder, checkedScope);
				break;
			case ExpressionType.MemberAccess:
				CSharpExpressionRenderer.RenderMemberAccess((MemberExpression)expression, builder, checkedScope);
				break;
			case ExpressionType.MemberInit:
				CSharpExpressionRenderer.RenderMemberInit((MemberInitExpression)expression, builder, checkedScope);
				break;
			case ExpressionType.Negate:
			case ExpressionType.UnaryPlus:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
				CSharpExpressionRenderer.RenderUnary((UnaryExpression)expression, builder, wrapped, checkedScope);
				break;
			case ExpressionType.New:
				CSharpExpressionRenderer.RenderNew((NewExpression)expression, builder, checkedScope);
				break;
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				CSharpExpressionRenderer.RenderNewArray((NewArrayExpression)expression, builder, checkedScope);
				break;
			case ExpressionType.Parameter:
			{
				ParameterExpression parameterExpression = (ParameterExpression)expression;
				builder.Append(parameterExpression.Name);
				break;
			}
			case ExpressionType.Quote:
				CSharpExpressionRenderer.Render(((UnaryExpression)expression).Operand, builder, true, checkedScope);
				break;
			case ExpressionType.TypeAs:
			{
				UnaryExpression unaryExpression = (UnaryExpression)expression;
				CSharpExpressionRenderer.Render(unaryExpression.Operand, builder, false, checkedScope);
				builder.Append(" as ");
				CSharpExpressionRenderer.RenderType(unaryExpression.Type, builder);
				break;
			}
			case ExpressionType.TypeIs:
			{
				TypeBinaryExpression typeBinaryExpression = (TypeBinaryExpression)expression;
				CSharpExpressionRenderer.Render(typeBinaryExpression.Expression, builder, false, checkedScope);
				builder.Append(" is ");
				CSharpExpressionRenderer.RenderType(typeBinaryExpression.TypeOperand, builder);
				break;
			}
			default:
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expression.NodeType));
			}
		}

		private static void RenderCondition(ConditionalExpression expression, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (!wrapped)
			{
				builder.Append("(");
			}
			CSharpExpressionRenderer.Render(expression.Test, builder, true, checkedScope);
			builder.Append(" ? ");
			CSharpExpressionRenderer.Render(expression.IfTrue, builder, true, checkedScope);
			builder.Append(" : ");
			CSharpExpressionRenderer.Render(expression.IfFalse, builder, true, checkedScope);
			if (!wrapped)
			{
				builder.Append(")");
			}
		}

		private static void RenderConvert(UnaryExpression expression, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (!expression.Type.IsInterface && expression.Type.IsAssignableFrom(expression.Operand.Type))
			{
				CSharpExpressionRenderer.Render(expression.Operand, builder, true, checkedScope);
				return;
			}
			bool flag = false;
			bool flag2 = expression.NodeType == ExpressionType.ConvertChecked;
			if (flag2 && !checkedScope)
			{
				builder.Append("checked(");
				checkedScope = true;
				flag = true;
			}
			else if (!flag2 && checkedScope)
			{
				builder.Append("unchecked(");
				checkedScope = false;
				flag = true;
			}
			else if (!wrapped)
			{
				builder.Append("(");
				flag = true;
			}
			builder.Append("(");
			CSharpExpressionRenderer.RenderType(expression.Type, builder);
			builder.Append(")");
			CSharpExpressionRenderer.Render(expression.Operand, builder, false, checkedScope);
			if (flag)
			{
				builder.Append(")");
			}
		}

		private static void RenderNewArray(NewArrayExpression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			if (expression.NodeType == ExpressionType.NewArrayBounds)
			{
				builder.Append("new ");
				CSharpExpressionRenderer.RenderType(expression.Type.GetElementType(), builder);
				builder.Append("[");
				bool flag = true;
				foreach (Expression expression2 in expression.Expressions)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.Render(expression2, builder, false, checkedScope);
					flag = false;
				}
				builder.Append("]");
			}
			else
			{
				builder.Append("new ");
				CSharpExpressionRenderer.RenderType(expression.Type.GetElementType(), builder);
				builder.Append("[] { ");
				bool flag2 = true;
				foreach (Expression expression3 in expression.Expressions)
				{
					if (!flag2)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.Render(expression3, builder, false, checkedScope);
					flag2 = false;
				}
				builder.Append(" }");
			}
		}

		private static void RenderMemberInit(MemberInitExpression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			CSharpExpressionRenderer.RenderNew(expression.NewExpression, builder, checkedScope);
			if (expression.Bindings.Count > 0)
			{
				builder.Append(" { ");
				bool flag = true;
				foreach (MemberBinding memberBinding in expression.Bindings)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.RenderMemberBinding(memberBinding, builder, checkedScope);
					flag = false;
				}
				builder.Append(" }");
			}
		}

		private static void RenderListInit(ListInitExpression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			CSharpExpressionRenderer.RenderNew(expression.NewExpression, builder, checkedScope);
			if (expression.Initializers.Count > 0)
			{
				builder.Append(" { ");
				bool flag = true;
				foreach (ElementInit elementInit in expression.Initializers)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.RenderListInitializer(elementInit, builder, checkedScope);
					flag = false;
				}
				builder.Append(" }");
			}
		}

		private static void RenderLambda(LambdaExpression expression, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			builder.Append("new ");
			CSharpExpressionRenderer.RenderType(expression.Type, builder);
			builder.Append(" (");
			if (expression.Parameters.Count != 1)
			{
				builder.Append("(");
			}
			bool flag = true;
			foreach (ParameterExpression parameterExpression in expression.Parameters)
			{
				if (!flag)
				{
					builder.Append(", ");
				}
				builder.Append(parameterExpression.Name);
				flag = false;
			}
			if (expression.Parameters.Count != 1)
			{
				builder.Append(")");
			}
			builder.Append(" => ");
			CSharpExpressionRenderer.Render(expression.Body, builder, false, checkedScope);
			builder.Append(")");
		}

		private static void RenderNew(NewExpression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			ReadOnlyCollection<Expression> readOnlyCollection = expression.Arguments;
			if (expression.Members != null && expression.Members.Count > 0)
			{
				readOnlyCollection = readOnlyCollection.Take(expression.Constructor.GetParameters().Length).ToList<Expression>().AsReadOnly();
			}
			builder.Append("new ");
			CSharpExpressionRenderer.RenderType(expression.Constructor.DeclaringType, builder);
			builder.Append("(");
			CSharpExpressionRenderer.RenderArguments(readOnlyCollection, builder, checkedScope);
			builder.Append(")");
			if (expression.Members != null && expression.Members.Count > 0)
			{
				bool flag = true;
				int num = readOnlyCollection.Count;
				builder.Append(" { ");
				foreach (MemberInfo memberInfo in expression.Members)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					builder.Append(memberInfo.Name).Append(" = ");
					CSharpExpressionRenderer.Render(expression.Arguments[num], builder, true, checkedScope);
					flag = false;
					num++;
				}
				builder.Append(" }");
			}
		}

		private static void RenderMemberAccess(MemberExpression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			PropertyInfo propertyInfo = expression.Member as PropertyInfo;
			FieldInfo fieldInfo = expression.Member as FieldInfo;
			Type declaringType = expression.Member.DeclaringType;
			bool flag = (fieldInfo != null && fieldInfo.IsStatic) || (propertyInfo != null && propertyInfo.GetGetMethod(true) != null && propertyInfo.GetGetMethod(true).IsStatic);
			if (expression.Expression != null)
			{
				CSharpExpressionRenderer.Render(expression.Expression, builder, false, checkedScope);
				builder.Append(".");
			}
			else if (flag && declaringType != null)
			{
				CSharpExpressionRenderer.RenderType(declaringType, builder);
				builder.Append(".");
			}
			builder.Append(expression.Member.Name);
		}

		private static void RenderCall(MethodCallExpression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			bool flag = expression.NodeType == ExpressionType.ArrayIndex;
			if (expression.Method.IsStatic)
			{
				Type declaringType = expression.Method.DeclaringType;
				if (declaringType != null)
				{
					CSharpExpressionRenderer.RenderType(declaringType, builder);
				}
			}
			else
			{
				CSharpExpressionRenderer.Render(expression.Object, builder, false, checkedScope);
			}
			if (flag)
			{
				builder.Append("[");
				CSharpExpressionRenderer.RenderArguments(expression.Arguments, builder, checkedScope);
				builder.Append("]");
			}
			else
			{
				MethodInfo method = expression.Method;
				builder.Append(".");
				builder.Append(method.Name);
				if (method.IsGenericMethod)
				{
					builder.Append("<");
					foreach (Type type in method.GetGenericArguments())
					{
						CSharpExpressionRenderer.RenderType(type, builder);
						builder.Append(',');
					}
					builder.Length--;
					builder.Append(">");
				}
				builder.Append("(");
				CSharpExpressionRenderer.RenderArguments(expression.Arguments, builder, checkedScope);
				builder.Append(")");
			}
		}

		private static void RenderArrayIndex(Expression expression, StringBuilder builder, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			BinaryExpression binaryExpression = expression as BinaryExpression;
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			if (binaryExpression != null)
			{
				CSharpExpressionRenderer.Render(binaryExpression.Left, builder, false, checkedScope);
				builder.Append("[");
				CSharpExpressionRenderer.Render(binaryExpression.Right, builder, false, checkedScope);
				builder.Append("]");
			}
			else
			{
				if (methodCallExpression == null)
				{
					throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_INVALIDCONSTANTEXPRESSION, expression.NodeType));
				}
				if (methodCallExpression.Method.IsStatic)
				{
					Type declaringType = methodCallExpression.Method.DeclaringType;
					if (declaringType != null)
					{
						CSharpExpressionRenderer.RenderType(declaringType, builder);
						builder.Append(".");
					}
				}
				else
				{
					CSharpExpressionRenderer.Render(methodCallExpression.Object, builder, false, checkedScope);
				}
				builder.Append("[");
				CSharpExpressionRenderer.RenderArguments(methodCallExpression.Arguments, builder, checkedScope);
				builder.Append("]");
			}
		}

		private static void RenderConstant(ConstantExpression expression, StringBuilder builder)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			if (expression.Value == null)
			{
				builder.Append("default(");
				CSharpExpressionRenderer.RenderType(expression.Type, builder);
				builder.Append(")");
				return;
			}
			string text = Convert.ToString(expression.Value, Constants.DefaultFormatProvider);
			if (expression.Type == typeof(string))
			{
				CSharpExpressionRenderer.RenderTextLiteral(text, builder, false);
			}
			else if (expression.Type == typeof(char))
			{
				CSharpExpressionRenderer.RenderTextLiteral(text, builder, true);
			}
			else if (expression.Type == typeof(Type))
			{
				builder.Append("typeof(");
				CSharpExpressionRenderer.RenderType((Type)expression.Value, builder);
				builder.Append(")");
			}
			else if (expression.Type == typeof(ushort) || expression.Type == typeof(uint))
			{
				builder.Append(text).Append("u");
			}
			else if (expression.Type == typeof(ulong))
			{
				builder.Append(text).Append("ul");
			}
			else if (expression.Type == typeof(long))
			{
				builder.Append(text).Append("l");
			}
			else if (expression.Type == typeof(float) || expression.Type == typeof(double))
			{
				bool flag = expression.Type == typeof(float);
				double num = Convert.ToDouble(expression.Value, Constants.DefaultFormatProvider);
				if (double.IsPositiveInfinity(num))
				{
					builder.Append((!flag) ? "System.Double.PositiveInfinity" : "System.Single.PositiveInfinity");
				}
				if (double.IsNegativeInfinity(num))
				{
					builder.Append((!flag) ? "System.Double.NegativeInfinity" : "System.Single.NegativeInfinity");
				}
				if (double.IsNaN(num))
				{
					builder.Append((!flag) ? "System.Double.NaN" : "System.Single.NaN");
				}
				else
				{
					builder.Append(num.ToString("R", Constants.DefaultFormatProvider));
				}
				builder.Append((!flag) ? "d" : "f");
			}
			else if (expression.Type == typeof(decimal))
			{
				builder.Append(text).Append("m");
			}
			else if (expression.Type == typeof(bool))
			{
				builder.Append(text.ToLowerInvariant());
			}
			else
			{
				if (expression.Type != typeof(byte) && expression.Type != typeof(sbyte) && expression.Type != typeof(short) && expression.Type != typeof(int))
				{
					throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_INVALIDCONSTANTEXPRESSION, expression.Type));
				}
				builder.Append(text);
			}
		}

		private static void RenderUnary(UnaryExpression expression, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			bool flag = expression.NodeType == ExpressionType.NegateChecked || (expression.NodeType != ExpressionType.Negate && checkedScope);
			bool flag2 = false;
			if (flag && !checkedScope)
			{
				builder.Append("checked(");
				checkedScope = true;
				flag2 = true;
			}
			else if (!flag && checkedScope)
			{
				builder.Append("unchecked(");
				checkedScope = false;
				flag2 = true;
			}
			else if (!wrapped)
			{
				builder.Append("(");
				flag2 = true;
			}
			switch (expression.NodeType)
			{
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
				builder.Append("-");
				goto IL_187;
			case ExpressionType.UnaryPlus:
				builder.Append("+");
				goto IL_187;
			case ExpressionType.Not:
				switch (Type.GetTypeCode(expression.Operand.Type))
				{
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					builder.Append("~");
					break;
				default:
					builder.Append("~");
					break;
				}
				goto IL_187;
			}
			throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expression.NodeType));
			IL_187:
			CSharpExpressionRenderer.Render(expression.Operand, builder, false, checkedScope);
			if (flag2)
			{
				builder.Append(")");
			}
		}

		private static void RenderBinary(BinaryExpression expression, StringBuilder builder, bool wrapped, bool checkedScope)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			bool flag = expression.NodeType == ExpressionType.AddChecked || expression.NodeType == ExpressionType.MultiplyChecked || expression.NodeType == ExpressionType.SubtractChecked || (expression.NodeType != ExpressionType.Add && expression.NodeType != ExpressionType.Multiply && expression.NodeType != ExpressionType.Subtract && checkedScope);
			bool flag2 = false;
			if (flag && !checkedScope)
			{
				builder.Append("checked(");
				checkedScope = true;
				flag2 = true;
			}
			else if (!flag && checkedScope)
			{
				builder.Append("unchecked(");
				checkedScope = false;
				flag2 = true;
			}
			else if (!wrapped)
			{
				builder.Append("(");
				flag2 = true;
			}
			CSharpExpressionRenderer.Render(expression.Left, builder, false, checkedScope);
			ExpressionType nodeType = expression.NodeType;
			switch (nodeType)
			{
			case ExpressionType.Divide:
				builder.Append(" / ");
				break;
			case ExpressionType.Equal:
				builder.Append(" == ");
				break;
			case ExpressionType.ExclusiveOr:
				builder.Append(" ^ ");
				break;
			case ExpressionType.GreaterThan:
				builder.Append(" > ");
				break;
			case ExpressionType.GreaterThanOrEqual:
				builder.Append(" >= ");
				break;
			default:
				switch (nodeType)
				{
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
					builder.Append(" + ");
					goto IL_30D;
				case ExpressionType.And:
					builder.Append(" & ");
					goto IL_30D;
				case ExpressionType.AndAlso:
					builder.Append(" && ");
					goto IL_30D;
				case ExpressionType.Coalesce:
					builder.Append(" ?? ");
					goto IL_30D;
				}
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expression.NodeType));
			case ExpressionType.LeftShift:
				builder.Append(" << ");
				break;
			case ExpressionType.LessThan:
				builder.Append(" < ");
				break;
			case ExpressionType.LessThanOrEqual:
				builder.Append(" <= ");
				break;
			case ExpressionType.Modulo:
				builder.Append(" % ");
				break;
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
				builder.Append(" * ");
				break;
			case ExpressionType.NotEqual:
				builder.Append(" != ");
				break;
			case ExpressionType.Or:
				builder.Append(" | ");
				break;
			case ExpressionType.OrElse:
				builder.Append(" || ");
				break;
			case ExpressionType.Power:
				builder.Append(" ** ");
				break;
			case ExpressionType.RightShift:
				builder.Append(" >> ");
				break;
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
				builder.Append(" - ");
				break;
			}
			IL_30D:
			CSharpExpressionRenderer.Render(expression.Right, builder, false, checkedScope);
			if (flag2)
			{
				builder.Append(")");
			}
		}

		private static void RenderArguments(ReadOnlyCollection<Expression> arguments, StringBuilder builder, bool checkedScope)
		{
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			bool flag = true;
			foreach (Expression expression in arguments)
			{
				if (!flag)
				{
					builder.Append(", ");
				}
				CSharpExpressionRenderer.Render(expression, builder, true, checkedScope);
				flag = false;
			}
		}

		private static void RenderMemberBinding(MemberBinding memberBinding, StringBuilder builder, bool checkedScope)
		{
			if (memberBinding == null)
			{
				throw new ArgumentException("memberBinding");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			builder.Append(memberBinding.Member.Name).Append(" = ");
			switch (memberBinding.BindingType)
			{
			case MemberBindingType.Assignment:
				CSharpExpressionRenderer.Render(((MemberAssignment)memberBinding).Expression, builder, true, checkedScope);
				break;
			case MemberBindingType.MemberBinding:
			{
				builder.Append("{ ");
				bool flag = true;
				foreach (MemberBinding memberBinding2 in ((MemberMemberBinding)memberBinding).Bindings)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.RenderMemberBinding(memberBinding2, builder, checkedScope);
					flag = false;
				}
				builder.Append("} ");
				break;
			}
			case MemberBindingType.ListBinding:
			{
				builder.Append(" { ");
				bool flag2 = true;
				foreach (ElementInit elementInit in ((MemberListBinding)memberBinding).Initializers)
				{
					if (!flag2)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.RenderListInitializer(elementInit, builder, checkedScope);
					flag2 = false;
				}
				builder.Append(" }");
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private static void RenderListInitializer(ElementInit initializer, StringBuilder builder, bool checkedScope)
		{
			if (initializer == null)
			{
				throw new ArgumentException("initializer");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			if (initializer.Arguments.Count == 1)
			{
				CSharpExpressionRenderer.Render(initializer.Arguments[0], builder, true, checkedScope);
			}
			else
			{
				bool flag = true;
				builder.Append("{ ");
				foreach (Expression expression in initializer.Arguments)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					CSharpExpressionRenderer.Render(expression, builder, true, checkedScope);
					flag = false;
				}
				builder.Append("}");
			}
		}

		private static void RenderType(Type type, StringBuilder builder)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			int length = builder.Length;
			NameUtils.WriteFullName(type, builder, true);
			NameUtils.RemoveGenericSuffix(builder, length, builder.Length - length);
		}

		private static void RenderTextLiteral(string value, StringBuilder builder, bool isChar)
		{
			if (value == null)
			{
				throw new ArgumentException("value");
			}
			if (builder == null)
			{
				throw new ArgumentException("builder");
			}
			if (isChar && value.Length != 1)
			{
				throw new ArgumentException(string.Format(Resources.EXCEPTION_BIND_INVALIDCHARLITERAL, value));
			}
			if (isChar)
			{
				builder.Append("'");
			}
			else
			{
				builder.Append("\"");
			}
			builder.Append(value);
			for (int i = builder.Length - value.Length; i < builder.Length; i++)
			{
				if (builder[i] == '"')
				{
					builder.Insert(i, '\\');
					i++;
				}
				else if (builder[i] == '\\')
				{
					builder.Insert(i, '\\');
					i++;
				}
				else if (builder[i] == '\0')
				{
					builder[i] = '0';
					builder.Insert(i, '\\');
					i++;
				}
				else if (builder[i] == '\r')
				{
					builder[i] = 'r';
					builder.Insert(i, '\\');
					i++;
				}
				else if (builder[i] == '\n')
				{
					builder[i] = 'n';
					builder.Insert(i, '\\');
					i++;
				}
			}
			if (isChar)
			{
				builder.Append("'");
			}
			else
			{
				builder.Append("\"");
			}
		}
	}
}
