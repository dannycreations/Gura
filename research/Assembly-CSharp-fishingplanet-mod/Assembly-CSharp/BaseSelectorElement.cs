using System;

public class BaseSelectorElement : IScrollSelectorElement
{
	public string Text { get; set; }

	public bool IsSelected { get; set; }

	public bool IsDefault
	{
		get
		{
			return this._isDefault;
		}
	}

	public bool _isDefault;
}
