using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextSize : MonoBehaviour
{
	private void Start()
	{
		this.rt = base.gameObject.GetComponent<RectTransform>();
		this.txt = base.gameObject.GetComponent<Text>();
	}

	private void Update()
	{
		this.rt.sizeDelta = new Vector2(this.rt.rect.width, Mathf.Max(this.txt.preferredHeight, this.MinHeigth));
	}

	public float MinHeigth;

	private RectTransform rt;

	private Text txt;
}
