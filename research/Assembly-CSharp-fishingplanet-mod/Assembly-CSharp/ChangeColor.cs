using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{
	public void SetColor(int colorId)
	{
		if (base.enabled)
		{
			if (this.Text != null)
			{
				this.Text.color = this.UseColor[colorId];
			}
			if (this.Text2 != null)
			{
				this.Text2.color = this.UseColor[colorId];
			}
			if (this.Text3 != null)
			{
				this.Text3.color = this.UseColor[colorId];
			}
			if (this.Text4 != null)
			{
				this.Text4.color = this.UseColor[colorId];
			}
			if (this.Image != null)
			{
				this.Image.color = this.UseColor[colorId];
			}
		}
	}

	public Text Text;

	public Text Text2;

	public Text Text3;

	public Text Text4;

	public Image Image;

	public Color[] UseColor;
}
