using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal sealed class BindingContext
	{
		public BindingContext(ITypeResolver typeResolver, ReadOnlyCollection<ParameterExpression> parameters, Type resultType, Expression global)
		{
			if (typeResolver == null)
			{
				throw new ArgumentNullException("typeResolver");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (resultType == null)
			{
				throw new ArgumentNullException("resultType");
			}
			this.typeResolver = typeResolver;
			this.parameters = parameters;
			this.resultType = resultType;
			this.global = global;
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

		public Expression Global
		{
			get
			{
				return this.global;
			}
		}

		public bool TryResolveType(object typeName, out Type type)
		{
			type = null;
			if (typeName == null)
			{
				return false;
			}
			TypeReference typeReference = null;
			return BindingContext.TryGetTypeReference(typeName, out typeReference) && this.typeResolver.TryGetType(typeReference, out type) && type != null;
		}

		public bool TryGetParameter(string parameterName, out Expression parameter)
		{
			if (parameterName == null)
			{
				throw new ArgumentNullException("parameterName");
			}
			for (int i = 0; i < this.parameters.Count; i++)
			{
				parameter = this.parameters[i];
				if (string.Equals(((ParameterExpression)parameter).Name, parameterName, StringComparison.Ordinal))
				{
					return true;
				}
			}
			parameter = null;
			return false;
		}

		public static bool TryGetTypeReference(object value, out TypeReference typeReference)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			typeReference = null;
			if (value is TypeReference)
			{
				typeReference = (TypeReference)value;
				return true;
			}
			if (value is SyntaxTreeNode)
			{
				List<SyntaxTreeNode> list = new List<SyntaxTreeNode>(10);
				for (SyntaxTreeNode syntaxTreeNode = (SyntaxTreeNode)value; syntaxTreeNode != null; syntaxTreeNode = syntaxTreeNode.GetExpression(false))
				{
					string expressionType = syntaxTreeNode.GetExpressionType(true);
					if (expressionType != "PropertyOrField")
					{
						return false;
					}
					list.Add(syntaxTreeNode);
				}
				List<string> list2 = null;
				List<TypeReference> list3 = null;
				for (int i = 0; i < list.Count; i++)
				{
					SyntaxTreeNode syntaxTreeNode2 = list[list.Count - 1 - i];
					ArgumentsTree arguments = syntaxTreeNode2.GetArguments(false);
					string text = syntaxTreeNode2.GetPropertyOrFieldName(true);
					if (list2 == null)
					{
						list2 = new List<string>();
					}
					int num = 0;
					if (arguments.Count > 0)
					{
						if (list3 == null)
						{
							list3 = new List<TypeReference>(10);
						}
						for (int j = 0; j < arguments.Count; j++)
						{
							SyntaxTreeNode syntaxTreeNode3 = null;
							TypeReference typeReference2 = null;
							string indexAsString = Constants.GetIndexAsString(j);
							if (!arguments.TryGetValue(indexAsString, out syntaxTreeNode3) || syntaxTreeNode3 == null)
							{
								throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGORWRONGARGUMENT, indexAsString), syntaxTreeNode2);
							}
							if (syntaxTreeNode3.GetExpressionType(true) == "PropertyOrField" && syntaxTreeNode3.GetPropertyOrFieldName(true) == string.Empty)
							{
								typeReference2 = TypeReference.Empty;
							}
							else if (!BindingContext.TryGetTypeReference(syntaxTreeNode3, out typeReference2))
							{
								throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGORWRONGARGUMENT, indexAsString), syntaxTreeNode2);
							}
							list3.Add(typeReference2);
							num++;
						}
						text = text + "`" + Constants.GetIndexAsString(num);
					}
					list2.Add(text);
				}
				typeReference = new TypeReference(list2, list3 ?? TypeReference.EmptyTypeArguments);
				return true;
			}
			typeReference = new TypeReference(new string[] { Convert.ToString(value, Constants.DefaultFormatProvider) }, TypeReference.EmptyTypeArguments);
			return true;
		}

		public static bool TryGetMethodReference(object value, out TypeReference methodReference)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			methodReference = null;
			if (value is TypeReference)
			{
				methodReference = (TypeReference)value;
				return true;
			}
			if (value is SyntaxTreeNode)
			{
				List<TypeReference> list = null;
				SyntaxTreeNode syntaxTreeNode = (SyntaxTreeNode)value;
				ArgumentsTree arguments = syntaxTreeNode.GetArguments(false);
				string propertyOrFieldName = syntaxTreeNode.GetPropertyOrFieldName(true);
				if (arguments.Count > 0)
				{
					list = new List<TypeReference>(10);
					for (int i = 0; i < arguments.Count; i++)
					{
						SyntaxTreeNode syntaxTreeNode2 = null;
						TypeReference typeReference = null;
						string indexAsString = Constants.GetIndexAsString(i);
						if (!arguments.TryGetValue(indexAsString, out syntaxTreeNode2) || syntaxTreeNode2 == null)
						{
							throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGORWRONGARGUMENT, indexAsString), syntaxTreeNode);
						}
						if ((syntaxTreeNode2.GetExpressionType(true) == "PropertyOrField" && syntaxTreeNode2.GetPropertyOrFieldName(true) == string.Empty) || !BindingContext.TryGetTypeReference(syntaxTreeNode2, out typeReference))
						{
							throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGORWRONGARGUMENT, indexAsString), syntaxTreeNode);
						}
						list.Add(typeReference);
					}
				}
				methodReference = new TypeReference(new string[] { propertyOrFieldName }, list ?? TypeReference.EmptyTypeArguments);
				return true;
			}
			methodReference = new TypeReference(new string[] { Convert.ToString(value, Constants.DefaultFormatProvider) }, TypeReference.EmptyTypeArguments);
			return true;
		}

		public BindingContext CreateNestedContext()
		{
			return new BindingContext(this.typeResolver, this.parameters, this.resultType, this.global);
		}

		public BindingContext CreateNestedContext(ReadOnlyCollection<ParameterExpression> newParameters, Type resultType)
		{
			if (newParameters == null)
			{
				throw new ArgumentNullException("newParameters");
			}
			if (resultType == null)
			{
				throw new ArgumentNullException("resultType");
			}
			return new BindingContext(this.typeResolver, newParameters, resultType, this.global);
		}

		public void RegisterNullPropagationTarger(Expression target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (this.nullPropagationTargets == null)
			{
				this.nullPropagationTargets = new List<Expression>();
			}
			this.nullPropagationTargets.Add(target);
		}

		public void CompleteNullPropagation(ref Expression expression)
		{
			if (expression == null || this.nullPropagationTargets == null || this.nullPropagationTargets.Count == 0)
			{
				return;
			}
			expression = ExpressionUtils.MakeNullPropagationExpression(this.nullPropagationTargets, expression);
		}

		private readonly Expression global;

		private readonly ReadOnlyCollection<ParameterExpression> parameters;

		private List<Expression> nullPropagationTargets;

		private readonly Type resultType;

		private readonly ITypeResolver typeResolver;
	}
}
