using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions
{
	internal class ExpressionSubstitutor : ExpressionVisitor
	{
		private ExpressionSubstitutor(Dictionary<Expression, Expression> substitutions)
		{
			if (substitutions == null)
			{
				throw new ArgumentNullException("substitutions");
			}
			this.substitutions = substitutions;
		}

		protected override Expression VisitParameter(ParameterExpression parameterExpression)
		{
			Expression expression = null;
			if (this.substitutions.TryGetValue(parameterExpression, out expression))
			{
				return expression;
			}
			return base.VisitParameter(parameterExpression);
		}

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			Expression expression = null;
			if (this.substitutions.TryGetValue(constantExpression, out expression))
			{
				return expression;
			}
			return base.VisitConstant(constantExpression);
		}

		public static Expression Visit(Expression expression, Dictionary<Expression, Expression> substitutions)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (substitutions == null)
			{
				throw new ArgumentNullException("substitutions");
			}
			ExpressionSubstitutor expressionSubstitutor = new ExpressionSubstitutor(substitutions);
			return expressionSubstitutor.Visit(expression);
		}

		private readonly Dictionary<Expression, Expression> substitutions;
	}
}
