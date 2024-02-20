using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions;
using GameDevWare.Dynamic.Expressions.CSharp;

namespace Assets
{
	public sealed class PatternString<InstanceT>
	{
		public PatternString(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			this.pattern = pattern;
			this.transform = this.CreateTransformFn(pattern);
		}

		public string Tranform(InstanceT instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			return this.transform(instance);
		}

		private Func<InstanceT, string> CreateTransformFn(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			List<Expression> list = new List<Expression>();
			foreach (KeyValuePair<int, string> keyValuePair in this.Split(pattern))
			{
				if (keyValuePair.Key == 0)
				{
					list.Add(Expression.Constant(keyValuePair.Value, typeof(object)));
				}
				else
				{
					IEnumerable<Token> enumerable = Tokenizer.Tokenize(keyValuePair.Value);
					SyntaxTreeNode syntaxTreeNode = Parser.Parse(enumerable).ToSyntaxTree(false);
					LambdaExpression lambdaExpression = PatternString<InstanceT>.ExpressionBinder.Bind(syntaxTreeNode, PatternString<InstanceT>.ExpressionBinder.Parameters[0]);
					list.Add(lambdaExpression);
				}
			}
			Expression<Func<InstanceT, string>> expression = Expression.Lambda<Func<InstanceT, string>>(Expression.Call(PatternString<InstanceT>.ConcatFunc.Method, new Expression[] { Expression.NewArrayInit(typeof(object), list) }), PatternString<InstanceT>.ExpressionBinder.Parameters);
			return expression.CompileAot(false);
		}

		private IEnumerable<KeyValuePair<int, string>> Split(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			int scanStart = 0;
			int bracersLevel = 0;
			for (int i = 0; i < pattern.Length; i++)
			{
				if (pattern[i] == '{' || pattern[i] == '}')
				{
					if (pattern[i] == '{' && bracersLevel > 0)
					{
						bracersLevel++;
					}
					else if (pattern[i] == '}' && bracersLevel > 1)
					{
						bracersLevel--;
					}
					else if (pattern[i] == '{')
					{
						if (i - scanStart != 0)
						{
							yield return new KeyValuePair<int, string>(0, pattern.Substring(scanStart, i - scanStart));
						}
						scanStart = i;
						bracersLevel++;
					}
					else if (pattern[i] == '}')
					{
						if (i - scanStart > 2)
						{
							yield return new KeyValuePair<int, string>(1, pattern.Substring(scanStart + 1, i - scanStart - 1));
						}
						scanStart = i + 1;
						bracersLevel--;
					}
				}
			}
			if (bracersLevel > 0)
			{
				throw new InvalidOperationException(string.Format("Unterminated expression at position {0} in '{1}'.", scanStart, pattern));
			}
			if (scanStart < pattern.Length)
			{
				yield return new KeyValuePair<int, string>(0, pattern.Substring(scanStart, pattern.Length - scanStart));
			}
			yield break;
		}

		public override string ToString()
		{
			return this.pattern.ToString();
		}

		private const int PART_TEXT = 0;

		private const int PART_EXPR = 1;

		private static readonly Binder ExpressionBinder = new Binder(new ParameterExpression[] { Expression.Parameter(typeof(InstanceT), "p") }, typeof(object), null);

		private static readonly Func<object[], string> ConcatFunc = new Func<object[], string>(string.Concat);

		private readonly Func<InstanceT, string> transform;

		private readonly string pattern;
	}
}
