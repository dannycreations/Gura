using System;
using UnityEngine;

public class FishingZoneCreator : MonoBehaviour
{
	public bool TwoClickMode;

	public FishingZoneCreator.ExpandSide Side;

	public float Width = 2f;

	public float Height = 2f;

	public float Overlap = 3f;

	public float ballR = 0.1f;

	public Color Color;

	[HideInInspector]
	public bool IsActive;

	[HideInInspector]
	public bool WasPointPlaced;

	[HideInInspector]
	public FishingZone LastZone;

	public enum ExpandSide
	{
		Both,
		Left,
		Right
	}
}
