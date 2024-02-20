using System;
using System.Linq.Expressions;

public class SingleSelectionFilter : ISelectionFilterBase
{
	public string Caption { get; set; }

	public CategoryFilter CategoryFilter { get; set; }

	public string FilterFieldName { get; set; }

	public Type FilterFieldType { get; set; }

	public object Value { get; set; }

	public virtual BinaryExpression GetExpression(ParameterExpression param)
	{
		Expression expression = param;
		foreach (string text in this.FilterFieldName.Split(new char[] { '.' }))
		{
			expression = Expression.Property(expression, text);
		}
		return Expression.Equal(expression, Expression.Constant(this.Value, this.FilterFieldType));
	}
}
