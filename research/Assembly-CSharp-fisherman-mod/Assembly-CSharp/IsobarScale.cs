using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsobarScale
{
	public IsobarScale(Image scale, Text low, GameObject ruller, float lowestDepth)
	{
		this.scale = scale;
		this.low = low;
		this.minHeight = -lowestDepth;
		low.text = ((int)MeasuringSystemManager.LineLength(this.minHeight + 1f)).ToString();
		this.ruller = ruller;
	}

	public void GenerateScale(float depthStep)
	{
		int num = (int)(this.minHeight / depthStep);
		if (num == this.steps)
		{
			return;
		}
		this.steps = num;
		for (int i = 0; i < this.rullers.Count; i++)
		{
			Object.Destroy(this.rullers[i]);
		}
		this.rullers.Clear();
		if (this.texture != null)
		{
			Object.Destroy(this.texture);
		}
		this.texture = new Texture2D(1, this.steps + 1, 3, false);
		this.texture.filterMode = 0;
		float num2 = -this.scale.GetComponent<RectTransform>().sizeDelta.y / 2f;
		float num3 = this.scale.GetComponent<RectTransform>().sizeDelta.y / (float)(this.steps + 1);
		for (int j = 0; j <= this.steps; j++)
		{
			Color color = this.WaterColor * (float)(j + 1) / (float)this.steps;
			color = 2.2f * (color - new Color(0.5f, 0.5f, 0.5f, 0f)) + new Color(0.5f, 0.5f, 0.5f, 0f) + new Color(0.2f, 0.2f, 0.2f, 0f);
			this.texture.SetPixel(0, j, new Color(Mathf.LinearToGammaSpace(color.r), Mathf.LinearToGammaSpace(color.g), Mathf.LinearToGammaSpace(color.b), 1f));
			GameObject gameObject = Object.Instantiate<GameObject>(this.ruller, this.scale.transform);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num2 + num3 * (float)(j + 1));
			this.rullers.Add(gameObject);
		}
		this.texture.Apply();
		this.scale.overrideSprite = Sprite.Create(this.texture, new Rect(0f, 0f, (float)this.texture.width, (float)this.texture.height), new Vector2(0.5f, 0.5f));
	}

	private Image scale;

	private Color WaterColor = new Color(0f, 0.32f, 1f, 1f);

	private float minHeight;

	private float maxHeight;

	private Texture2D texture;

	private Text low;

	private GameObject ruller;

	private List<GameObject> rullers = new List<GameObject>();

	private int steps;
}
