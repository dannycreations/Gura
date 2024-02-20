using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollInit : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[ExecuteInEditMode]
	public virtual void Awake()
	{
		float num = this.MaskPanel.GetComponent<RectTransform>().rect.height / this.ContentPanel.GetComponent<RectTransform>().rect.height;
		this.ScrollBar.GetComponent<Scrollbar>().size = num;
		this.ScrollBar.GetComponent<Scrollbar>().value = 0f;
		this.ScrollBar.SetActive(false);
	}

	public virtual void Update()
	{
		float num = this.MaskPanel.GetComponent<RectTransform>().rect.height / this.ContentPanel.GetComponent<RectTransform>().rect.height;
		if (!this.ScrollBar.activeSelf && num < 1f)
		{
			this.ScrollBar.SetActive(true);
		}
		if (this.ScrollBar.activeSelf && num >= 1f)
		{
			this.ScrollBar.SetActive(false);
			return;
		}
		this.ScrollBar.GetComponent<Scrollbar>().size = num;
		float num2 = this.ContentPanel.GetComponent<RectTransform>().rect.height - this.MaskPanel.GetComponent<RectTransform>().rect.height;
		num2 *= this.ScrollBar.GetComponent<Scrollbar>().value;
		this.ContentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, num2, 0f);
		this.ContentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(this.MaskPanel.GetComponent<RectTransform>().rect.width - (float)(10 + this.HorizontalPadding), this.ContentPanel.GetComponent<RectTransform>().rect.height);
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (this.ScrollBar.activeSelf)
		{
			this.ScrollBar.GetComponent<Scrollbar>().value -= eventData.scrollDelta.y * (this.ScrollBar.GetComponent<Scrollbar>().size / 10f);
		}
	}

	public GameObject MaskPanel;

	public GameObject ContentPanel;

	public GameObject ScrollBar;

	public int HorizontalPadding;
}
