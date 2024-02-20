using System;

public class UnderlineAndTextsHighlight : UnderlineHint
{
	protected override void Show()
	{
		base.Show();
		this.ColorHighlighter.Highlight();
	}

	protected override void Hide()
	{
		base.Hide();
		this.ColorHighlighter.RestoreColors();
	}

	public ParentTextChildrenColorsHighlight ColorHighlighter;
}
