using System;

public interface ISelectionFilterBase
{
	string Caption { get; set; }

	object Value { get; set; }

	string FilterFieldName { get; set; }

	Type FilterFieldType { get; set; }

	CategoryFilter CategoryFilter { get; set; }
}
