using System;

public interface IScrollSelectorElement
{
	string Text { get; }

	bool IsSelected { get; set; }

	bool IsDefault { get; }
}
