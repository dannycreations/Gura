using System;
using UnityEngine;

namespace mset
{
	public class Logo : MonoBehaviour
	{
		private void Reset()
		{
			this.logoTexture = Resources.Load("renderedLogo") as Texture2D;
		}

		private void Start()
		{
		}

		private void updateTexRect()
		{
			if (this.logoTexture)
			{
				float num = (float)this.logoTexture.width;
				float num2 = (float)this.logoTexture.height;
				float num3 = 0f;
				float num4 = 0f;
				if (base.GetComponent<Camera>())
				{
					num3 = (float)base.GetComponent<Camera>().pixelWidth;
					num4 = (float)base.GetComponent<Camera>().pixelHeight;
				}
				else if (Camera.main)
				{
					num3 = (float)Camera.main.pixelWidth;
					num4 = (float)Camera.main.pixelHeight;
				}
				else if (Camera.current)
				{
				}
				float num5 = this.logoPixelOffset.x + this.logoPercentOffset.x * num3 * 0.01f;
				float num6 = this.logoPixelOffset.y + this.logoPercentOffset.y * num4 * 0.01f;
				switch (this.placement)
				{
				case Corner.TopLeft:
					this.texRect.x = num5;
					this.texRect.y = num6;
					break;
				case Corner.TopRight:
					this.texRect.x = num3 - num5 - num;
					this.texRect.y = num6;
					break;
				case Corner.BottomLeft:
					this.texRect.x = num5;
					this.texRect.y = num4 - num6 - num2;
					break;
				case Corner.BottomRight:
					this.texRect.x = num3 - num5 - num;
					this.texRect.y = num4 - num6 - num2;
					break;
				}
				this.texRect.width = num;
				this.texRect.height = num2;
			}
		}

		private void OnGUI()
		{
			this.updateTexRect();
			if (this.logoTexture)
			{
				GUI.color = this.color;
				GUI.DrawTexture(this.texRect, this.logoTexture);
			}
		}

		public Texture2D logoTexture;

		public Color color = Color.white;

		public Vector2 logoPixelOffset = new Vector2(0f, 0f);

		public Vector2 logoPercentOffset = new Vector2(0f, 0f);

		public Corner placement = Corner.BottomLeft;

		private Rect texRect = new Rect(0f, 0f, 0f, 0f);
	}
}
