using System;
using System.Linq.Expressions;

public class RangeForRangeSelectionFilter : RangeSelectionFilter
{
	public string FilterFieldNameMin { get; set; }

	public string FilterFieldNameMax { get; set; }

	public override BinaryExpression GetExpression(ParameterExpression param)
	{
		return Expression.And(Expression.LessThanOrEqual(Expression.Property(param, this.FilterFieldNameMax), Expression.Constant(base.MaxValue, base.FilterFieldType)), Expression.GreaterThanOrEqual(Expression.Property(param, this.FilterFieldNameMin), Expression.Constant(base.MinValue, base.FilterFieldType)));
	}
}
