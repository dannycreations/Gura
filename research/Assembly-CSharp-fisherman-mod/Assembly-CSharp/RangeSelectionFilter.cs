using System;
using System.Linq.Expressions;

public class RangeSelectionFilter : SingleSelectionFilter
{
	public object MinValue { get; set; }

	public object MaxValue { get; set; }

	public override BinaryExpression GetExpression(ParameterExpression param)
	{
		Expression expression = param;
		foreach (string text in base.FilterFieldName.Split(new char[] { '.' }))
		{
			expression = Expression.Property(expression, text);
		}
		return Expression.And(Expression.LessThanOrEqual(expression, Expression.Constant(this.MaxValue, base.FilterFieldType)), Expression.GreaterThanOrEqual(expression, Expression.Constant(this.MinValue, base.FilterFieldType)));
	}
}
