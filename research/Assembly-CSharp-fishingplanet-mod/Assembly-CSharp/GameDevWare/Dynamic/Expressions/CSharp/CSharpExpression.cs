using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public static class CSharpExpression
	{
		public static ResultT Evaluate<ResultT>(string expression, ITypeResolver typeResolver = null)
		{
			Func<ResultT> func = CSharpExpression.Parse<ResultT>(expression, typeResolver).CompileAot(false);
			return func();
		}

		public static ResultT Evaluate<Arg1T, ResultT>(string expression, Arg1T arg1, string arg1Name = "arg1", ITypeResolver typeResolver = null)
		{
			Func<Arg1T, ResultT> func = CSharpExpression.Parse<Arg1T, ResultT>(expression, arg1Name, typeResolver).CompileAot(false);
			return func(arg1);
		}

		public static ResultT Evaluate<Arg1T, Arg2T, ResultT>(string expression, Arg1T arg1, Arg2T arg2, string arg1Name = "arg1", string arg2Name = "arg2", ITypeResolver typeResolver = null)
		{
			Func<Arg1T, Arg2T, ResultT> func = CSharpExpression.Parse<Arg1T, Arg2T, ResultT>(expression, arg1Name, arg2Name, typeResolver).CompileAot(false);
			return func(arg1, arg2);
		}

		public static ResultT Evaluate<Arg1T, Arg2T, Arg3T, ResultT>(string expression, Arg1T arg1, Arg2T arg2, Arg3T arg3, string arg1Name = "arg1", string arg2Name = "arg2", string arg3Name = "arg3", ITypeResolver typeResolver = null)
		{
			Func<Arg1T, Arg2T, Arg3T, ResultT> func = CSharpExpression.Parse<Arg1T, Arg2T, Arg3T, ResultT>(expression, arg1Name, arg2Name, arg3Name, typeResolver).CompileAot(false);
			return func(arg1, arg2, arg3);
		}

		public static ResultT Evaluate<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(string expression, Arg1T arg1, Arg2T arg2, Arg3T arg3, Arg4T arg4, string arg1Name = "arg1", string arg2Name = "arg2", string arg3Name = "arg3", string arg4Name = "arg4", ITypeResolver typeResolver = null)
		{
			Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT> func = CSharpExpression.Parse<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(expression, arg1Name, arg2Name, arg3Name, arg4Name, typeResolver).CompileAot(false);
			return func(arg1, arg2, arg3, arg4);
		}

		public static Expression<Func<ResultT>> Parse<ResultT>(string expression, ITypeResolver typeResolver = null)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			IEnumerable<Token> enumerable = Tokenizer.Tokenize(expression);
			ParseTreeNode parseTreeNode = Parser.Parse(enumerable);
			SyntaxTreeNode syntaxTreeNode = parseTreeNode.ToSyntaxTree(true);
			Binder binder = new Binder(new ParameterExpression[0], typeof(ResultT), typeResolver);
			return (Expression<Func<ResultT>>)binder.Bind(syntaxTreeNode, null);
		}

		public static Expression<Func<Arg1T, ResultT>> Parse<Arg1T, ResultT>(string expression, string arg1Name = "arg1", ITypeResolver typeResolver = null)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			IEnumerable<Token> enumerable = Tokenizer.Tokenize(expression);
			ParseTreeNode parseTreeNode = Parser.Parse(enumerable);
			SyntaxTreeNode syntaxTreeNode = parseTreeNode.ToSyntaxTree(true);
			Binder binder = new Binder(new ParameterExpression[] { Expression.Parameter(typeof(Arg1T), arg1Name ?? "arg1") }, typeof(ResultT), typeResolver);
			return (Expression<Func<Arg1T, ResultT>>)binder.Bind(syntaxTreeNode, null);
		}

		public static Expression<Func<Arg1T, Arg2T, ResultT>> Parse<Arg1T, Arg2T, ResultT>(string expression, string arg1Name = "arg1", string arg2Name = "arg2", ITypeResolver typeResolver = null)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			IEnumerable<Token> enumerable = Tokenizer.Tokenize(expression);
			ParseTreeNode parseTreeNode = Parser.Parse(enumerable);
			SyntaxTreeNode syntaxTreeNode = parseTreeNode.ToSyntaxTree(true);
			Binder binder = new Binder(new ParameterExpression[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? "arg1"),
				Expression.Parameter(typeof(Arg2T), arg2Name ?? "arg2")
			}, typeof(ResultT), typeResolver);
			return (Expression<Func<Arg1T, Arg2T, ResultT>>)binder.Bind(syntaxTreeNode, null);
		}

		public static Expression<Func<Arg1T, Arg2T, Arg3T, ResultT>> Parse<Arg1T, Arg2T, Arg3T, ResultT>(string expression, string arg1Name = "arg1", string arg2Name = "arg2", string arg3Name = "arg3", ITypeResolver typeResolver = null)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			IEnumerable<Token> enumerable = Tokenizer.Tokenize(expression);
			ParseTreeNode parseTreeNode = Parser.Parse(enumerable);
			SyntaxTreeNode syntaxTreeNode = parseTreeNode.ToSyntaxTree(true);
			Binder binder = new Binder(new ParameterExpression[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? "arg1"),
				Expression.Parameter(typeof(Arg2T), arg2Name ?? "arg2"),
				Expression.Parameter(typeof(Arg3T), arg3Name ?? "arg3")
			}, typeof(ResultT), typeResolver);
			return (Expression<Func<Arg1T, Arg2T, Arg3T, ResultT>>)binder.Bind(syntaxTreeNode, null);
		}

		public static Expression<Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>> Parse<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(string expression, string arg1Name = "arg1", string arg2Name = "arg2", string arg3Name = "arg3", string arg4Name = "arg4", ITypeResolver typeResolver = null)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			IEnumerable<Token> enumerable = Tokenizer.Tokenize(expression);
			ParseTreeNode parseTreeNode = Parser.Parse(enumerable);
			SyntaxTreeNode syntaxTreeNode = parseTreeNode.ToSyntaxTree(true);
			Binder binder = new Binder(new ParameterExpression[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? "arg1"),
				Expression.Parameter(typeof(Arg2T), arg2Name ?? "arg2"),
				Expression.Parameter(typeof(Arg3T), arg3Name ?? "arg3"),
				Expression.Parameter(typeof(Arg4T), arg3Name ?? "arg4")
			}, typeof(ResultT), typeResolver);
			return (Expression<Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>>)binder.Bind(syntaxTreeNode, null);
		}

		public const bool DefaultCheckedScope = true;

		public const string ARG1_DEFAULT_NAME = "arg1";

		public const string ARG2_DEFAULT_NAME = "arg2";

		public const string ARG3_DEFAULT_NAME = "arg3";

		public const string ARG4_DEFAULT_NAME = "arg4";
	}
}
