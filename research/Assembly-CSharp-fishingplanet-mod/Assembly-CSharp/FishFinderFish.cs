using System;
using UnityEngine;
using UnityEngine.UI;

public class FishFinderFish : MonoBehaviour
{
	public RectTransform RectTransform
	{
		get
		{
			return base.transform as RectTransform;
		}
	}

	public Image Image;
}
