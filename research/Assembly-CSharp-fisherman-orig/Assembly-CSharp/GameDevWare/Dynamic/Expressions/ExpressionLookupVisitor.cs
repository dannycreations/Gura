using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions
{
	internal sealed class ExpressionLookupVisitor : ExpressionVisitor
	{
		public ExpressionLookupVisitor(List<Expression> lookupList)
		{
			if (lookupList == null)
			{
				throw new ArgumentNullException("lookupList");
			}
			this.lookupList = lookupList;
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			if (this.lookupList.Contains(binaryExpression))
			{
				this.found++;
			}
			return base.VisitBinary(binaryExpression);
		}

		protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
		{
			if (this.lookupList.Contains(conditionalExpression))
			{
				this.found++;
			}
			return base.VisitConditional(conditionalExpression);
		}

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			if (this.lookupList.Contains(constantExpression))
			{
				this.found++;
			}
			return base.VisitConstant(constantExpression);
		}

		protected override Expression VisitInvocation(InvocationExpression invocationExpression)
		{
			if (this.lookupList.Contains(invocationExpression))
			{
				this.found++;
			}
			return base.VisitInvocation(invocationExpression);
		}

		protected override Expression VisitLambda(LambdaExpression lambda)
		{
			if (this.lookupList.Contains(lambda))
			{
				this.found++;
			}
			return base.VisitLambda(lambda);
		}

		protected override Expression VisitListInit(ListInitExpression listInitExpression)
		{
			if (this.lookupList.Contains(listInitExpression))
			{
				this.found++;
			}
			return base.VisitListInit(listInitExpression);
		}

		protected override Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			if (this.lookupList.Contains(memberExpression))
			{
				this.found++;
			}
			return base.VisitMemberAccess(memberExpression);
		}

		protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
		{
			if (this.lookupList.Contains(memberInitExpression))
			{
				this.found++;
			}
			return base.VisitMemberInit(memberInitExpression);
		}

		protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			if (this.lookupList.Contains(methodCallExpression))
			{
				this.found++;
			}
			return base.VisitMethodCall(methodCallExpression);
		}

		protected override NewExpression VisitNew(NewExpression newExpression)
		{
			if (this.lookupList.Contains(newExpression))
			{
				this.found++;
			}
			return base.VisitNew(newExpression);
		}

		protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
		{
			if (this.lookupList.Contains(newArrayExpression))
			{
				this.found++;
			}
			return base.VisitNewArray(newArrayExpression);
		}

		protected override Expression VisitParameter(ParameterExpression parameterExpression)
		{
			if (this.lookupList.Contains(parameterExpression))
			{
				this.found++;
			}
			return base.VisitParameter(parameterExpression);
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression typeBinaryExpression)
		{
			if (this.lookupList.Contains(typeBinaryExpression))
			{
				this.found++;
			}
			return base.VisitTypeIs(typeBinaryExpression);
		}

		protected override Expression VisitUnary(UnaryExpression unaryExpression)
		{
			if (this.lookupList.Contains(unaryExpression))
			{
				this.found++;
			}
			return base.VisitUnary(unaryExpression);
		}

		public static bool Lookup(Expression expression, List<Expression> lookupList)
		{
			ExpressionLookupVisitor expressionLookupVisitor = new ExpressionLookupVisitor(lookupList);
			expressionLookupVisitor.Visit(expression);
			return expressionLookupVisitor.found == lookupList.Count;
		}

		private readonly List<Expression> lookupList;

		private int found;
	}
}
