using System;
using UnityEngine;

[ExecuteInEditMode]
public class TransformPositionCopier : MonoBehaviour
{
	private void Update()
	{
		base.GetComponent<RectTransform>().anchoredPosition = this.copyFrom.anchoredPosition;
		base.GetComponent<RectTransform>().sizeDelta = this.copyFrom.sizeDelta;
	}

	[SerializeField]
	private RectTransform copyFrom;
}
