using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SetAlphaForAllChildren : MonoBehaviour
{
	private void Update()
	{
		foreach (Graphic graphic in base.GetComponentsInChildren<Graphic>())
		{
			graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, this.parent.color.a);
		}
	}

	public Graphic parent;
}
