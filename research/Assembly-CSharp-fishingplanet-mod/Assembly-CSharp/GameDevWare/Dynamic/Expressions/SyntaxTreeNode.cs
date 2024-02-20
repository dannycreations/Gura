using System;
using System.Collections;
using System.Collections.Generic;
using GameDevWare.Dynamic.Expressions.CSharp;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions
{
	public class SyntaxTreeNode : IDictionary<string, object>, ILineInfo, IEnumerable, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>
	{
		public SyntaxTreeNode(IDictionary<string, object> node)
		{
			this.innerDictionary = SyntaxTreeNode.PrepareNode(node);
		}

		private static Dictionary<string, object> PrepareNode(IDictionary<string, object> node)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>(node.Count);
			foreach (KeyValuePair<string, object> keyValuePair in node)
			{
				if (keyValuePair.Value is IDictionary<string, object> && !(keyValuePair.Value is SyntaxTreeNode))
				{
					dictionary[keyValuePair.Key] = new SyntaxTreeNode((IDictionary<string, object>)keyValuePair.Value);
				}
				else
				{
					dictionary[keyValuePair.Key] = keyValuePair.Value;
				}
			}
			return dictionary;
		}

		public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
		{
			object obj = null;
			T t = default(T);
			T t2;
			if (!this.TryGetValue(key, out obj) || !(obj is T))
			{
				t2 = defaultValue;
			}
			else
			{
				t2 = (T)((object)obj);
			}
			return t2;
		}

		internal string GetExpressionType(bool throwOnError)
		{
			object obj = null;
			if (this.TryGetValue("expressionType", out obj) && obj is string)
			{
				return (string)obj;
			}
			if (throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expressionType", this.GetExpressionType(true)), this);
			}
			return null;
		}

		internal object GetTypeName(bool throwOnError)
		{
			object obj = null;
			if (!this.TryGetValue("type", out obj) || (!(obj is string) && !(obj is SyntaxTreeNode)))
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "type", this.GetExpressionType(true)), this);
			}
			return obj;
		}

		internal object GetValue(bool throwOnError)
		{
			object obj = null;
			if (!this.TryGetValue("value", out obj))
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "value", this.GetExpressionType(true), this), 0, 0, 0);
			}
			return obj;
		}

		internal SyntaxTreeNode GetExpression(bool throwOnError)
		{
			return this.GetExpression("expression", throwOnError);
		}

		private SyntaxTreeNode GetExpression(string attributeName, bool throwOnError)
		{
			object obj = null;
			if (this.TryGetValue(attributeName, out obj) && obj != null && obj is SyntaxTreeNode)
			{
				return (SyntaxTreeNode)obj;
			}
			if (throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, attributeName, this.GetExpressionType(true)), this);
			}
			return null;
		}

		internal SyntaxTreeNode GetLeftExpression(bool throwOnError)
		{
			return this.GetExpression("left", throwOnError);
		}

		internal SyntaxTreeNode GetRightExpression(bool throwOnError)
		{
			return this.GetExpression("right", throwOnError);
		}

		internal SyntaxTreeNode GetTestExpression(bool throwOnError)
		{
			return this.GetExpression("test", throwOnError);
		}

		internal SyntaxTreeNode GetIfTrueExpression(bool throwOnError)
		{
			return this.GetExpression("ifTrue", throwOnError);
		}

		internal SyntaxTreeNode GetIfFalseExpression(bool throwOnError)
		{
			return this.GetExpression("ifFalse", throwOnError);
		}

		internal ArgumentsTree GetArguments(bool throwOnError)
		{
			object obj = null;
			if (!this.TryGetValue("arguments", out obj) || obj == null || !(obj is SyntaxTreeNode))
			{
				if (throwOnError)
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "arguments", this.GetExpressionType(true)), this);
				}
				return ArgumentsTree.Empty;
			}
			else
			{
				Dictionary<string, SyntaxTreeNode> dictionary = new Dictionary<string, SyntaxTreeNode>(((SyntaxTreeNode)obj).Count);
				foreach (KeyValuePair<string, object> keyValuePair in ((IEnumerable<KeyValuePair<string, object>>)((SyntaxTreeNode)obj)))
				{
					SyntaxTreeNode syntaxTreeNode = keyValuePair.Value as SyntaxTreeNode;
					if (syntaxTreeNode == null)
					{
						throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGORWRONGARGUMENT, keyValuePair.Key), this);
					}
					dictionary.Add(keyValuePair.Key, syntaxTreeNode);
				}
				if (dictionary.Count > 100)
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_TOOMANYARGUMENTS, 100.ToString()), this);
				}
				return new ArgumentsTree(dictionary);
			}
		}

		internal string GetPropertyOrFieldName(bool throwOnError)
		{
			object obj = null;
			if (this.TryGetValue("propertyOrFieldName", out obj) && obj is string)
			{
				return (string)obj;
			}
			if (throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "propertyOrFieldName", this.GetExpressionType(true)), this);
			}
			return null;
		}

		internal object GetMethodName(bool throwOnError)
		{
			object obj = null;
			if (this.TryGetValue("method", out obj) && obj != null)
			{
				return obj;
			}
			if (throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "method", this.GetExpressionType(true)), this);
			}
			return null;
		}

		internal bool GetUseNullPropagation(bool throwOnError)
		{
			object obj = null;
			if (this.TryGetValue("useNullPropagation", out obj) && obj != null)
			{
				return Convert.ToBoolean(obj, Constants.DefaultFormatProvider);
			}
			if (throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "useNullPropagation", this.GetExpressionType(true)), this);
			}
			return false;
		}

		internal int GetLineNumber(bool throwOnError)
		{
			object obj = null;
			int num = 0;
			if (!this.TryGetValue("$lineNum", out obj))
			{
				return this.GetExpressionPosition(throwOnError).LineNumber;
			}
			if (!int.TryParse(Convert.ToString(obj, Constants.DefaultFormatProvider), out num) && throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "$lineNum", this.GetExpressionType(true)), this);
			}
			return num;
		}

		internal int GetColumnNumber(bool throwOnError)
		{
			object obj = null;
			int num = 0;
			if (!this.TryGetValue("$columnNum", out obj))
			{
				return this.GetExpressionPosition(throwOnError).ColumnNumber;
			}
			if (!int.TryParse(Convert.ToString(obj, Constants.DefaultFormatProvider), out num) && throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "$columnNum", this.GetExpressionType(true)), this);
			}
			return num;
		}

		internal int GetTokenLength(bool throwOnError)
		{
			object obj = null;
			int num = 0;
			if (!this.TryGetValue("$tokenLength", out obj))
			{
				return this.GetExpressionPosition(throwOnError).TokenLength;
			}
			if (!int.TryParse(Convert.ToString(obj, Constants.DefaultFormatProvider), out num) && throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "$tokenLength", this.GetExpressionType(true)), this);
			}
			return num;
		}

		internal string GetPosition(bool throwOnError)
		{
			return this.GetExpressionPosition(throwOnError).ToString();
		}

		internal ExpressionPosition GetExpressionPosition(bool throwOnError)
		{
			object obj = null;
			if (!this.TryGetValue("_pos", out obj) || (!(obj is string) && !(obj is ILineInfo)))
			{
				if (this.ContainsKey("$lineNum") && this.ContainsKey("$columnNum") && this.ContainsKey("$tokenLength"))
				{
					obj = string.Format(Constants.DefaultFormatProvider, "{0}:{1}+{2}", new object[]
					{
						this.GetValueOrDefault<string>("$lineNum", "0"),
						this.GetValueOrDefault<string>("$columnNum", "0"),
						this.GetValueOrDefault<string>("$tokenLength", "0")
					});
				}
				if (throwOnError)
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "_pos", this.GetExpressionType(true)), this);
				}
				return default(ExpressionPosition);
			}
			else
			{
				ILineInfo lineInfo = obj as ILineInfo;
				string text = obj as string;
				if (lineInfo != null)
				{
					return new ExpressionPosition(lineInfo.GetLineNumber(), lineInfo.GetColumnNumber(), lineInfo.GetTokenLength());
				}
				return ExpressionPosition.Parse(text);
			}
		}

		internal string GetOriginalExpression(bool throwOnError)
		{
			object obj = null;
			string text = null;
			if (this.TryGetValue("_src", out obj) || this.TryGetValue("$originalExpression", out obj))
			{
				return Convert.ToString(obj, Constants.DefaultFormatProvider);
			}
			if (throwOnError)
			{
				throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "_src", this.GetExpressionType(true)), this);
			}
			return text;
		}

		int ILineInfo.GetLineNumber()
		{
			return this.GetLineNumber(false);
		}

		int ILineInfo.GetColumnNumber()
		{
			return this.GetColumnNumber(false);
		}

		int ILineInfo.GetTokenLength()
		{
			return this.GetTokenLength(false);
		}

		void IDictionary<string, object>.Add(string key, object value)
		{
			throw new NotSupportedException();
		}

		public bool ContainsKey(string key)
		{
			return this.innerDictionary.ContainsKey(key);
		}

		public ICollection<string> Keys
		{
			get
			{
				return this.innerDictionary.Keys;
			}
		}

		bool IDictionary<string, object>.Remove(string key)
		{
			throw new NotSupportedException();
		}

		public bool TryGetValue(string key, out object value)
		{
			return this.innerDictionary.TryGetValue(key, out value);
		}

		public ICollection<object> Values
		{
			get
			{
				return this.innerDictionary.Values;
			}
		}

		public object this[string key]
		{
			get
			{
				return this.innerDictionary[key];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<KeyValuePair<string, object>>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)this.innerDictionary).Contains(item);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, object>>)this.innerDictionary).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get
			{
				return this.innerDictionary.Count;
			}
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<string, object>>)this.innerDictionary).IsReadOnly;
			}
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)this.innerDictionary).Remove(item);
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>)this.innerDictionary).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.innerDictionary).GetEnumerator();
		}

		public override bool Equals(object obj)
		{
			return this.innerDictionary.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.innerDictionary.GetHashCode();
		}

		public override string ToString()
		{
			string text = this.GetOriginalExpression(false);
			try
			{
				if (string.IsNullOrEmpty(text))
				{
					text = this.Render(true);
				}
			}
			catch (Exception ex)
			{
				text = "/failed to render expression '" + ex.Message + "'/";
			}
			return text;
		}

		private readonly Dictionary<string, object> innerDictionary;
	}
}
