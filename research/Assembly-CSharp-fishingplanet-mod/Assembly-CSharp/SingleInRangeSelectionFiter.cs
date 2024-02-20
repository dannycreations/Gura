using System;
using System.Linq.Expressions;

public class SingleInRangeSelectionFiter : SingleSelectionFilter
{
	public string FilterFieldNameMin { get; set; }

	public string FilterFieldNameMax { get; set; }

	public override BinaryExpression GetExpression(ParameterExpression param)
	{
		return Expression.And(Expression.LessThanOrEqual(Expression.Property(param, this.FilterFieldNameMin), Expression.Constant(base.Value, base.FilterFieldType)), Expression.GreaterThanOrEqual(Expression.Property(param, this.FilterFieldNameMax), Expression.Constant(base.Value, base.FilterFieldType)));
	}
}
