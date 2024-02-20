using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameDevWare.Dynamic.Expressions
{
	public class ArgumentsTree : IDictionary<string, SyntaxTreeNode>, IEnumerable, ICollection<KeyValuePair<string, SyntaxTreeNode>>, IEnumerable<KeyValuePair<string, SyntaxTreeNode>>
	{
		private ArgumentsTree()
		{
			this.innerDictionary = new Dictionary<string, SyntaxTreeNode>();
		}

		public ArgumentsTree(Dictionary<string, SyntaxTreeNode> innerDictionary)
		{
			if (innerDictionary == null)
			{
				throw new ArgumentNullException("innerDictionary");
			}
			this.innerDictionary = innerDictionary;
		}

		void IDictionary<string, SyntaxTreeNode>.Add(string key, SyntaxTreeNode value)
		{
			throw new NotSupportedException();
		}

		public bool ContainsKey(string key)
		{
			return this.innerDictionary.ContainsKey(key);
		}

		public bool ContainsKey(int position)
		{
			return this.innerDictionary.ContainsKey(Constants.GetIndexAsString(position));
		}

		public ICollection<string> Keys
		{
			get
			{
				return this.innerDictionary.Keys;
			}
		}

		bool IDictionary<string, SyntaxTreeNode>.Remove(string key)
		{
			throw new NotSupportedException();
		}

		public bool TryGetValue(string key, out SyntaxTreeNode value)
		{
			return this.innerDictionary.TryGetValue(key, out value);
		}

		public bool TryGetValue(int position, out SyntaxTreeNode value)
		{
			return this.innerDictionary.TryGetValue(Constants.GetIndexAsString(position), out value);
		}

		public ICollection<SyntaxTreeNode> Values
		{
			get
			{
				return this.innerDictionary.Values;
			}
		}

		public SyntaxTreeNode this[string key]
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

		public SyntaxTreeNode this[int position]
		{
			get
			{
				return this.innerDictionary[Constants.GetIndexAsString(position)];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		void ICollection<KeyValuePair<string, SyntaxTreeNode>>.Add(KeyValuePair<string, SyntaxTreeNode> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<KeyValuePair<string, SyntaxTreeNode>>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<KeyValuePair<string, SyntaxTreeNode>>.Contains(KeyValuePair<string, SyntaxTreeNode> item)
		{
			return ((ICollection<KeyValuePair<string, SyntaxTreeNode>>)this.innerDictionary).Contains(item);
		}

		void ICollection<KeyValuePair<string, SyntaxTreeNode>>.CopyTo(KeyValuePair<string, SyntaxTreeNode>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, SyntaxTreeNode>>)this.innerDictionary).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get
			{
				return this.innerDictionary.Count;
			}
		}

		bool ICollection<KeyValuePair<string, SyntaxTreeNode>>.IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<string, SyntaxTreeNode>>)this.innerDictionary).IsReadOnly;
			}
		}

		bool ICollection<KeyValuePair<string, SyntaxTreeNode>>.Remove(KeyValuePair<string, SyntaxTreeNode> item)
		{
			return ((ICollection<KeyValuePair<string, SyntaxTreeNode>>)this.innerDictionary).Remove(item);
		}

		IEnumerator<KeyValuePair<string, SyntaxTreeNode>> IEnumerable<KeyValuePair<string, SyntaxTreeNode>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, SyntaxTreeNode>>)this.innerDictionary).GetEnumerator();
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
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, SyntaxTreeNode> keyValuePair in this.innerDictionary)
			{
				stringBuilder.Append(keyValuePair.Key).Append(": ").Append(keyValuePair.Value.ToString())
					.Append(", ");
			}
			if (stringBuilder.Length > 2)
			{
				stringBuilder.Length -= 2;
			}
			return stringBuilder.ToString();
		}

		public static readonly ArgumentsTree Empty = new ArgumentsTree();

		private readonly Dictionary<string, SyntaxTreeNode> innerDictionary;
	}
}
