using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions
{
	public abstract class ExpressionVisitor
	{
		private Exception UnhandledBindingType(MemberBindingType memberBindingType)
		{
			throw new InvalidOperationException(string.Format("Unknown binding type '{0}'.", memberBindingType));
		}

		private Exception UnhandledExpressionType(ExpressionType expressionType)
		{
			throw new InvalidOperationException(string.Format("Unknown expression type '{0}'.", expressionType));
		}

		public Expression Visit(Expression exp)
		{
			if (exp == null)
			{
				return exp;
			}
			switch (exp.NodeType)
			{
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.ArrayIndex:
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
				return this.VisitBinary((BinaryExpression)exp);
			case ExpressionType.ArrayLength:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.Negate:
			case ExpressionType.UnaryPlus:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.Quote:
			case ExpressionType.TypeAs:
				return this.VisitUnary((UnaryExpression)exp);
			case ExpressionType.Call:
				return this.VisitMethodCall((MethodCallExpression)exp);
			case ExpressionType.Conditional:
				return this.VisitConditional((ConditionalExpression)exp);
			case ExpressionType.Constant:
				return this.VisitConstant((ConstantExpression)exp);
			case ExpressionType.Invoke:
				return this.VisitInvocation((InvocationExpression)exp);
			case ExpressionType.Lambda:
				return this.VisitLambda((LambdaExpression)exp);
			case ExpressionType.ListInit:
				return this.VisitListInit((ListInitExpression)exp);
			case ExpressionType.MemberAccess:
				return this.VisitMemberAccess((MemberExpression)exp);
			case ExpressionType.MemberInit:
				return this.VisitMemberInit((MemberInitExpression)exp);
			case ExpressionType.New:
				return this.VisitNew((NewExpression)exp);
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				return this.VisitNewArray((NewArrayExpression)exp);
			case ExpressionType.Parameter:
				return this.VisitParameter((ParameterExpression)exp);
			case ExpressionType.TypeIs:
				return this.VisitTypeIs((TypeBinaryExpression)exp);
			default:
				throw this.UnhandledExpressionType(exp.NodeType);
			}
		}

		protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
		{
			Expression expression = this.Visit(binaryExpression.Left);
			Expression expression2 = this.Visit(binaryExpression.Right);
			Expression expression3 = this.Visit(binaryExpression.Conversion);
			if (expression == binaryExpression.Left && expression2 == binaryExpression.Right && expression3 == binaryExpression.Conversion)
			{
				return binaryExpression;
			}
			if (binaryExpression.NodeType == ExpressionType.Coalesce && binaryExpression.Conversion != null)
			{
				return Expression.Coalesce(expression, expression2, expression3 as LambdaExpression);
			}
			return Expression.MakeBinary(binaryExpression.NodeType, expression, expression2, binaryExpression.IsLiftedToNull, binaryExpression.Method);
		}

		protected virtual MemberBinding VisitBinding(MemberBinding binding)
		{
			MemberBindingType bindingType = binding.BindingType;
			if (bindingType == MemberBindingType.Assignment)
			{
				return this.VisitMemberAssignment((MemberAssignment)binding);
			}
			if (bindingType == MemberBindingType.MemberBinding)
			{
				return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
			}
			if (bindingType != MemberBindingType.ListBinding)
			{
				throw this.UnhandledBindingType(binding.BindingType);
			}
			return this.VisitMemberListBinding((MemberListBinding)binding);
		}

		protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
		{
			List<MemberBinding> list = null;
			int i = 0;
			int count = original.Count;
			while (i < count)
			{
				MemberBinding memberBinding = this.VisitBinding(original[i]);
				if (list != null)
				{
					list.Add(memberBinding);
				}
				else if (memberBinding != original[i])
				{
					list = new List<MemberBinding>(count);
					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(memberBinding);
				}
				i++;
			}
			if (list != null)
			{
				return list;
			}
			return original;
		}

		protected virtual Expression VisitConditional(ConditionalExpression conditionalExpression)
		{
			Expression expression = this.Visit(conditionalExpression.Test);
			Expression expression2 = this.Visit(conditionalExpression.IfTrue);
			Expression expression3 = this.Visit(conditionalExpression.IfFalse);
			if (expression == conditionalExpression.Test && expression2 == conditionalExpression.IfTrue && expression3 == conditionalExpression.IfFalse)
			{
				return conditionalExpression;
			}
			return Expression.Condition(expression, expression2, expression3);
		}

		protected virtual Expression VisitConstant(ConstantExpression constantExpression)
		{
			return constantExpression;
		}

		protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
		{
			ReadOnlyCollection<Expression> readOnlyCollection = this.VisitExpressionList(initializer.Arguments);
			if (readOnlyCollection != initializer.Arguments)
			{
				return Expression.ElementInit(initializer.AddMethod, readOnlyCollection);
			}
			return initializer;
		}

		protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
		{
			List<ElementInit> list = null;
			int i = 0;
			int count = original.Count;
			while (i < count)
			{
				ElementInit elementInit = this.VisitElementInitializer(original[i]);
				if (list != null)
				{
					list.Add(elementInit);
				}
				else if (elementInit != original[i])
				{
					list = new List<ElementInit>(count);
					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(elementInit);
				}
				i++;
			}
			if (list != null)
			{
				return list;
			}
			return original;
		}

		protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			List<Expression> list = null;
			int i = 0;
			int count = original.Count;
			while (i < count)
			{
				Expression expression = this.Visit(original[i]);
				if (list != null)
				{
					list.Add(expression);
				}
				else if (expression != original[i])
				{
					list = new List<Expression>(count);
					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(expression);
				}
				i++;
			}
			if (list != null)
			{
				return new ReadOnlyCollection<Expression>(list);
			}
			return original;
		}

		protected virtual Expression VisitInvocation(InvocationExpression invocationExpression)
		{
			IEnumerable<Expression> enumerable = this.VisitExpressionList(invocationExpression.Arguments);
			Expression expression = this.Visit(invocationExpression.Expression);
			if (enumerable == invocationExpression.Arguments && expression == invocationExpression.Expression)
			{
				return invocationExpression;
			}
			return Expression.Invoke(expression, enumerable);
		}

		protected virtual Expression VisitLambda(LambdaExpression lambda)
		{
			Expression expression = this.Visit(lambda.Body);
			if (expression != lambda.Body)
			{
				return Expression.Lambda(lambda.Type, expression, lambda.Parameters);
			}
			return lambda;
		}

		protected virtual Expression VisitListInit(ListInitExpression listInitExpression)
		{
			NewExpression newExpression = this.VisitNew(listInitExpression.NewExpression);
			IEnumerable<ElementInit> enumerable = this.VisitElementInitializerList(listInitExpression.Initializers);
			if (newExpression == listInitExpression.NewExpression && enumerable == listInitExpression.Initializers)
			{
				return listInitExpression;
			}
			return Expression.ListInit(newExpression, enumerable);
		}

		protected virtual Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			Expression expression = this.Visit(memberExpression.Expression);
			if (expression != memberExpression.Expression)
			{
				return Expression.MakeMemberAccess(expression, memberExpression.Member);
			}
			return memberExpression;
		}

		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			Expression expression = this.Visit(assignment.Expression);
			if (expression != assignment.Expression)
			{
				return Expression.Bind(assignment.Member, expression);
			}
			return assignment;
		}

		protected virtual Expression VisitMemberInit(MemberInitExpression memberInitExpression)
		{
			NewExpression newExpression = this.VisitNew(memberInitExpression.NewExpression);
			IEnumerable<MemberBinding> enumerable = this.VisitBindingList(memberInitExpression.Bindings);
			if (newExpression == memberInitExpression.NewExpression && enumerable == memberInitExpression.Bindings)
			{
				return memberInitExpression;
			}
			return Expression.MemberInit(newExpression, enumerable);
		}

		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			IEnumerable<ElementInit> enumerable = this.VisitElementInitializerList(binding.Initializers);
			if (enumerable != binding.Initializers)
			{
				return Expression.ListBind(binding.Member, enumerable);
			}
			return binding;
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			IEnumerable<MemberBinding> enumerable = this.VisitBindingList(binding.Bindings);
			if (enumerable != binding.Bindings)
			{
				return Expression.MemberBind(binding.Member, enumerable);
			}
			return binding;
		}

		protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			Expression expression = this.Visit(methodCallExpression.Object);
			IEnumerable<Expression> enumerable = this.VisitExpressionList(methodCallExpression.Arguments);
			if (expression == methodCallExpression.Object && enumerable == methodCallExpression.Arguments)
			{
				return methodCallExpression;
			}
			return Expression.Call(expression, methodCallExpression.Method, enumerable);
		}

		protected virtual NewExpression VisitNew(NewExpression newExpression)
		{
			IEnumerable<Expression> enumerable = this.VisitExpressionList(newExpression.Arguments);
			if (enumerable == newExpression.Arguments)
			{
				return newExpression;
			}
			if (newExpression.Members != null)
			{
				return Expression.New(newExpression.Constructor, enumerable, newExpression.Members);
			}
			return Expression.New(newExpression.Constructor, enumerable);
		}

		protected virtual Expression VisitNewArray(NewArrayExpression newArrayExpression)
		{
			IEnumerable<Expression> enumerable = this.VisitExpressionList(newArrayExpression.Expressions);
			if (enumerable == newArrayExpression.Expressions)
			{
				return newArrayExpression;
			}
			if (newArrayExpression.NodeType == ExpressionType.NewArrayInit)
			{
				return Expression.NewArrayInit(newArrayExpression.Type.GetElementType(), enumerable);
			}
			return Expression.NewArrayBounds(newArrayExpression.Type.GetElementType(), enumerable);
		}

		protected virtual Expression VisitParameter(ParameterExpression parameterExpression)
		{
			return parameterExpression;
		}

		protected virtual Expression VisitTypeIs(TypeBinaryExpression typeBinaryExpression)
		{
			Expression expression = this.Visit(typeBinaryExpression.Expression);
			if (expression != typeBinaryExpression.Expression)
			{
				return Expression.TypeIs(expression, typeBinaryExpression.TypeOperand);
			}
			return typeBinaryExpression;
		}

		protected virtual Expression VisitUnary(UnaryExpression unaryExpression)
		{
			Expression expression = this.Visit(unaryExpression.Operand);
			if (expression != unaryExpression.Operand)
			{
				return Expression.MakeUnary(unaryExpression.NodeType, expression, unaryExpression.Type, unaryExpression.Method);
			}
			return unaryExpression;
		}
	}
}
