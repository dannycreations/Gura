using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollArrow : MonoBehaviour
{
	private void Update()
	{
		this.img.material.mainTextureOffset += Time.deltaTime * (Vector2.right * this.HorizontalScrollRate + Vector2.up * this.VerticalScrollRate);
	}

	public Image img;

	public float HorizontalScrollRate;

	public float VerticalScrollRate;
}
