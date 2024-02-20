using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Binding;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions
{
	public class Binder
	{
		public Binder(Type lambdaType, ITypeResolver typeResolver = null)
		{
			if (lambdaType == null)
			{
				throw new ArgumentNullException("lambdaType");
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(lambdaType);
			if (!typeDescription.IsDelegate || typeDescription.HasGenericParameters)
			{
				throw new ArgumentException(string.Format(Resources.EXCEPTION_BIND_VALIDDELEGATETYPEISEXPECTED, lambdaType), "lambdaType");
			}
			MemberDescription memberDescription = typeDescription.GetMembers("Invoke").FirstOrDefault((MemberDescription m) => !m.IsStatic && m.IsMethod);
			if (memberDescription == null)
			{
				throw new MissingMethodException(lambdaType.ToString(), "Invoke");
			}
			ParameterExpression[] array = new ParameterExpression[memberDescription.GetParametersCount()];
			for (int i = 0; i < memberDescription.GetParametersCount(); i++)
			{
				array[i] = Expression.Parameter(memberDescription.GetParameterType(i), memberDescription.GetParameterName(i));
			}
			this.lambdaType = lambdaType;
			this.parameters = new ReadOnlyCollection<ParameterExpression>(array);
			this.resultType = TypeDescription.GetTypeDescription(memberDescription.ResultType);
			this.typeResolver = typeResolver ?? new KnownTypeResolver(Binder.GetTypes(this.resultType, this.parameters), Binder.DefaultTypeResolver);
		}

		public Binder(IList<ParameterExpression> parameters, Type resultType, ITypeResolver typeResolver = null)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (resultType == null)
			{
				throw new ArgumentNullException("resultType");
			}
			if (resultType.IsGenericParameter)
			{
				throw new ArgumentException("A value can't be generic parameter type.", "resultType");
			}
			if (parameters.Count > 4)
			{
				throw new ArgumentOutOfRangeException("parameters");
			}
			if (parameters.Any((ParameterExpression p) => p == null || p.Type.IsGenericParameter))
			{
				throw new ArgumentException("Collection can't contain nulls or generic parameter types.", "parameters");
			}
			if (!(parameters is ReadOnlyCollection<ParameterExpression>))
			{
				parameters = new ReadOnlyCollection<ParameterExpression>(parameters);
			}
			Type[] array = new Type[parameters.Count + 1];
			for (int i = 0; i < parameters.Count; i++)
			{
				array[i] = parameters[i].Type;
			}
			array[array.Length - 1] = resultType;
			this.lambdaType = Expression.GetFuncType(array);
			this.parameters = (ReadOnlyCollection<ParameterExpression>)parameters;
			this.resultType = TypeDescription.GetTypeDescription(resultType);
			this.typeResolver = typeResolver ?? new KnownTypeResolver(Binder.GetTypes(resultType, parameters), Binder.DefaultTypeResolver);
		}

		public ReadOnlyCollection<ParameterExpression> Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		public Type ResultType
		{
			get
			{
				return this.resultType;
			}
		}

		public LambdaExpression Bind(SyntaxTreeNode node, Expression global = null)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			BindingContext bindingContext;
			if (node.GetExpressionType(true) == "Lambda" && !this.resultType.IsDelegate)
			{
				string[] array = LambdaBinder.ExtractArgumentNames(node);
				if (array.Length != this.parameters.Count)
				{
					throw new ExpressionParserException(Resources.EXCEPTION_BIND_UNABLEREMAPPARAMETERSCOUNTMISMATCH, node);
				}
				ParameterExpression[] array2 = new ParameterExpression[array.Length];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = Expression.Parameter(this.parameters[i].Type, array[i]);
				}
				bindingContext = new BindingContext(this.typeResolver, new ReadOnlyCollection<ParameterExpression>(array2), this.resultType, global);
				node = node.GetExpression(true);
			}
			else
			{
				bindingContext = new BindingContext(this.typeResolver, this.parameters, this.resultType, global);
			}
			Expression expression = null;
			Exception ex = null;
			if (!AnyBinder.TryBind(node, bindingContext, this.resultType, out expression, out ex))
			{
				throw ex;
			}
			bindingContext.CompleteNullPropagation(ref expression);
			if (expression.Type != this.resultType)
			{
				expression = Expression.ConvertChecked(expression, this.resultType);
			}
			return Expression.Lambda(this.lambdaType, expression, bindingContext.Parameters);
		}

		private static Type[] GetTypes(Type resultType, IList<ParameterExpression> parameters)
		{
			if (resultType == null)
			{
				throw new ArgumentNullException("resultType");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			Type[] array = new Type[parameters.Count + 1];
			for (int i = 0; i < parameters.Count; i++)
			{
				array[i] = parameters[i].Type;
			}
			array[array.Length - 1] = resultType;
			return array;
		}

		public override string ToString()
		{
			return string.Format("Binder: {0}, ({1}) -> {2}", this.lambdaType, string.Join(", ", this.parameters.Select((ParameterExpression p) => p.Name).ToArray<string>()), this.resultType);
		}

		public static ITypeResolver DefaultTypeResolver;

		private readonly ReadOnlyCollection<ParameterExpression> parameters;

		private readonly TypeDescription resultType;

		private readonly Type lambdaType;

		private readonly ITypeResolver typeResolver;
	}
}
