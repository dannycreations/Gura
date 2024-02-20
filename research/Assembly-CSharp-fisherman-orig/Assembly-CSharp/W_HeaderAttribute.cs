using System;
using UnityEngine;

public class W_HeaderAttribute : PropertyAttribute
{
	public W_HeaderAttribute(string header)
	{
		this.headerText = header;
	}

	public W_HeaderAttribute(string header, string text)
	{
		this.headerText = header;
		this.text = text;
	}

	public W_HeaderAttribute(string header, string text, int FontSize)
	{
		this.headerText = header;
		this.text = text;
		this.FontSize = FontSize;
	}

	public W_HeaderAttribute(string header, string text, int FontSize, string HeaderColor)
	{
		this.headerText = header;
		this.text = text;
		this.FontSize = FontSize;
		if (HeaderColor == "RedColor")
		{
			this.HeaderColor = Color.red;
		}
		else if (HeaderColor == "WhiteColor")
		{
			this.HeaderColor = Color.white;
		}
		else if (HeaderColor == "YellowColor")
		{
			this.HeaderColor = Color.yellow;
		}
		else if (HeaderColor == "BlackColor")
		{
			this.HeaderColor = Color.black;
		}
		else if (HeaderColor == "GreenColor")
		{
			this.HeaderColor = Color.green;
		}
		else if (HeaderColor == "BlueColor")
		{
			this.HeaderColor = Color.blue;
		}
	}

	public string headerText;

	public string text;

	public int FontSize;

	public Color HeaderColor = Color.gray;
}
