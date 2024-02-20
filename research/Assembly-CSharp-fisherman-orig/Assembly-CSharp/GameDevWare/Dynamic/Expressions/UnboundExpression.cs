using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions
{
	public sealed class UnboundExpression
	{
		public UnboundExpression(IDictionary<string, object> node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			this.compiledExpressions = new Dictionary<MethodCallSignature, Expression>();
			this.syntaxTree = ((!(node is SyntaxTreeNode)) ? new SyntaxTreeNode(node) : ((SyntaxTreeNode)node));
		}

		public SyntaxTreeNode SyntaxTree
		{
			get
			{
				return this.syntaxTree;
			}
		}

		public Func<ResultT> Bind<ResultT>()
		{
			MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(ResultT));
			Expression expression = null;
			object obj = this.compiledExpressions;
			lock (obj)
			{
				if (!this.compiledExpressions.TryGetValue(methodCallSignature, out expression))
				{
					ReadOnlyCollection<ParameterExpression> readOnlyCollection = new ReadOnlyCollection<ParameterExpression>(new ParameterExpression[0]);
					Binder binder = new Binder(readOnlyCollection, typeof(ResultT), null);
					expression = binder.Bind(this.SyntaxTree, null);
					this.compiledExpressions.Add(methodCallSignature, expression);
				}
			}
			return ((Expression<Func<ResultT>>)expression).CompileAot(false);
		}

		public Func<Arg1T, ResultT> Bind<Arg1T, ResultT>(string arg1Name = null)
		{
			MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), arg1Name ?? "arg1", typeof(ResultT));
			Expression expression = null;
			object obj = this.compiledExpressions;
			lock (obj)
			{
				if (!this.compiledExpressions.TryGetValue(methodCallSignature, out expression))
				{
					ReadOnlyCollection<ParameterExpression> readOnlyCollection = UnboundExpression.CreateParameters(new Type[] { typeof(Arg1T) }, new string[] { arg1Name ?? "arg1" });
					Binder binder = new Binder(readOnlyCollection, typeof(ResultT), null);
					expression = binder.Bind(this.SyntaxTree, null);
					this.compiledExpressions.Add(methodCallSignature, expression);
				}
			}
			return ((Expression<Func<Arg1T, ResultT>>)expression).CompileAot(false);
		}

		public Func<Arg1T, Arg2T, ResultT> Bind<Arg1T, Arg2T, ResultT>(string arg1Name = null, string arg2Name = null)
		{
			MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), arg1Name ?? "arg1", typeof(Arg2T), arg2Name ?? "arg2", typeof(ResultT));
			Expression expression = null;
			object obj = this.compiledExpressions;
			lock (obj)
			{
				if (!this.compiledExpressions.TryGetValue(methodCallSignature, out expression))
				{
					ReadOnlyCollection<ParameterExpression> readOnlyCollection = UnboundExpression.CreateParameters(new Type[]
					{
						typeof(Arg1T),
						typeof(Arg2T)
					}, new string[]
					{
						arg1Name ?? "arg1",
						arg2Name ?? "arg2"
					});
					Binder binder = new Binder(readOnlyCollection, typeof(ResultT), null);
					expression = binder.Bind(this.SyntaxTree, null);
					this.compiledExpressions.Add(methodCallSignature, expression);
				}
			}
			return ((Expression<Func<Arg1T, Arg2T, ResultT>>)expression).CompileAot(false);
		}

		public Func<Arg1T, Arg2T, Arg3T, ResultT> Bind<Arg1T, Arg2T, Arg3T, ResultT>(string arg1Name = null, string arg2Name = null, string arg3Name = null)
		{
			MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), arg1Name ?? "arg1", typeof(Arg2T), arg2Name ?? "arg2", typeof(Arg3T), arg3Name ?? "arg3", typeof(ResultT));
			Expression expression = null;
			object obj = this.compiledExpressions;
			lock (obj)
			{
				if (!this.compiledExpressions.TryGetValue(methodCallSignature, out expression))
				{
					ReadOnlyCollection<ParameterExpression> readOnlyCollection = UnboundExpression.CreateParameters(new Type[]
					{
						typeof(Arg1T),
						typeof(Arg2T),
						typeof(Arg3T)
					}, new string[]
					{
						arg1Name ?? "arg1",
						arg2Name ?? "arg2",
						arg3Name ?? "arg3"
					});
					Binder binder = new Binder(readOnlyCollection, typeof(ResultT), null);
					expression = binder.Bind(this.SyntaxTree, null);
					this.compiledExpressions.Add(methodCallSignature, expression);
				}
			}
			return ((Expression<Func<Arg1T, Arg2T, Arg3T, ResultT>>)expression).CompileAot(false);
		}

		public Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT> Bind<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(string arg1Name = null, string arg2Name = null, string arg3Name = null, string arg4Name = null)
		{
			MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), arg1Name ?? "arg1", typeof(Arg2T), arg2Name ?? "arg2", typeof(Arg3T), arg3Name ?? "arg3", typeof(Arg4T), arg4Name ?? "arg4", typeof(ResultT));
			Expression expression = null;
			object obj = this.compiledExpressions;
			lock (obj)
			{
				if (!this.compiledExpressions.TryGetValue(methodCallSignature, out expression))
				{
					ReadOnlyCollection<ParameterExpression> readOnlyCollection = UnboundExpression.CreateParameters(new Type[]
					{
						typeof(Arg1T),
						typeof(Arg2T),
						typeof(Arg3T),
						typeof(Arg4T)
					}, new string[]
					{
						arg1Name ?? "arg1",
						arg2Name ?? "arg2",
						arg3Name ?? "arg3",
						arg4Name ?? "arg4"
					});
					Binder binder = new Binder(readOnlyCollection, typeof(ResultT), null);
					expression = binder.Bind(this.SyntaxTree, null);
					this.compiledExpressions.Add(methodCallSignature, expression);
				}
			}
			return ((Expression<Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>>)expression).CompileAot(false);
		}

		private static ReadOnlyCollection<ParameterExpression> CreateParameters(Type[] types, string[] names)
		{
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			if (names == null)
			{
				throw new ArgumentNullException("names");
			}
			if (types.Length != names.Length)
			{
				throw new ArgumentException(Resources.EXCEPTION_UNBOUNDEXPR_TYPESDOESNTMATCHNAMES, "types");
			}
			ParameterExpression[] array = new ParameterExpression[types.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (Array.IndexOf<string>(names, names[i]) != i)
				{
					throw new ArgumentException(string.Format(Resources.EXCEPTION_UNBOUNDEXPR_DUPLICATEPARAMNAME, names[i]), "names");
				}
				array[i] = Expression.Parameter(types[i], names[i]);
			}
			return new ReadOnlyCollection<ParameterExpression>(array);
		}

		public override bool Equals(object obj)
		{
			UnboundExpression unboundExpression = obj as UnboundExpression;
			return unboundExpression != null && this.syntaxTree.SequenceEqual(unboundExpression.SyntaxTree);
		}

		public override int GetHashCode()
		{
			return this.syntaxTree.GetHashCode();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			object obj = this.compiledExpressions;
			lock (obj)
			{
				foreach (KeyValuePair<MethodCallSignature, Expression> keyValuePair in this.compiledExpressions)
				{
					stringBuilder.Append(keyValuePair.Key).Append(": ").Append(keyValuePair.Value)
						.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		private readonly Dictionary<MethodCallSignature, Expression> compiledExpressions;

		private readonly SyntaxTreeNode syntaxTree;
	}
}
