using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ParentTextChildrenColorsHighlight : MonoBehaviour
{
	public void Highlight()
	{
		MonoBehaviour.print("highlight called");
		List<Text> list = new List<Text>(base.transform.parent.GetComponentsInChildren<Text>());
		list = list.Where((Text x) => x.transform.parent == base.transform.parent).ToList<Text>();
		this.isHighlighting = true;
		foreach (Text text in list)
		{
			if (!this.savedColors.ContainsKey(text))
			{
				this.savedColors.Add(text, text.color);
			}
			text.color = this.HighlightColor;
		}
	}

	public void RestoreColors()
	{
		MonoBehaviour.print("restore colors called");
		this.isHighlighting = false;
		foreach (Text text in this.savedColors.Keys)
		{
			if (this.savedColors.ContainsKey(text))
			{
				text.color = this.savedColors[text];
			}
		}
	}

	public void FixedUpdate()
	{
		if (this.isHighlighting)
		{
			foreach (Text text in this.savedColors.Keys)
			{
				if (text.color != this.HighlightColor)
				{
					text.color = this.HighlightColor;
				}
			}
		}
	}

	public Color HighlightColor;

	private Dictionary<Text, Color> savedColors = new Dictionary<Text, Color>();

	private bool isHighlighting;
}
