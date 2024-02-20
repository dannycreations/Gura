using System;
using System.Linq.Expressions;

public class TypesSelectionFilter : SingleSelectionFilter
{
	public object[] Values { get; set; }

	public override BinaryExpression GetExpression(ParameterExpression param)
	{
		Expression expression = param;
		foreach (string text in base.FilterFieldName.Split(new char[] { '.' }))
		{
			expression = Expression.Property(expression, text);
		}
		BinaryExpression binaryExpression = null;
		for (int j = 0; j < this.Values.Length; j++)
		{
			if (binaryExpression == null)
			{
				binaryExpression = Expression.Equal(expression, Expression.Constant(this.Values[j], base.FilterFieldType));
			}
			else
			{
				binaryExpression = Expression.Or(binaryExpression, Expression.Equal(expression, Expression.Constant(this.Values[j], base.FilterFieldType)));
			}
		}
		return binaryExpression;
	}
}
