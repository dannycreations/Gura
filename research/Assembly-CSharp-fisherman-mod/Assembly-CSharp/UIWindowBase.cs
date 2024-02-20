using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIWindowBase : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	private void Start()
	{
		this.m_transform = base.GetComponent<RectTransform>();
	}

	public void OnDrag(PointerEventData eventData)
	{
		this.m_transform.position += new Vector3(eventData.delta.x, eventData.delta.y);
	}

	public void ChangeStrength(float value)
	{
		base.GetComponent<Image>().material.SetFloat("_Size", value);
	}

	public void ChangeVibrancy(float value)
	{
		base.GetComponent<Image>().material.SetFloat("_Vibrancy", value);
	}

	private RectTransform m_transform;
}
