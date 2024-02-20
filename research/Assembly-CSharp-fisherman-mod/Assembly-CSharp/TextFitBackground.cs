using System;
using UnityEngine;

public class TextFitBackground : MonoBehaviour
{
	private void Start()
	{
		this.textRenderer = this.text.GetComponent<Renderer>();
		this.lastValidXScale = this.textRenderer.bounds.size.x * this.xScaleFactor / Mathf.Cos(this.text.transform.eulerAngles.y * 0.017453292f);
	}

	private void Update()
	{
		float num = Mathf.Cos(this.text.transform.eulerAngles.y * 0.017453292f);
		float num2 = ((Mathf.Abs(num) >= 0.0001f) ? (this.textRenderer.bounds.size.x * this.xScaleFactor / num) : this.lastValidXScale);
		float num3 = this.textRenderer.bounds.size.y * this.yScaleFactor;
		base.transform.localScale = new Vector3(num2, num3, 1f);
		base.transform.localPosition = new Vector3(0f, num3 / 2f, 0f);
		this.lastValidXScale = num2;
	}

	public TextMesh text;

	private Renderer textRenderer;

	public float xScaleFactor = 1.1f;

	public float yScaleFactor = 1.03f;

	private float lastValidXScale;
}
