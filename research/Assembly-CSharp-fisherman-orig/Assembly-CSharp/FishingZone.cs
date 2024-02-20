using System;
using UnityEngine;

public class FishingZone : MonoBehaviour
{
	private void Awake()
	{
		this.SetViewVisibility(true);
	}

	public void SetViewVisibility(bool flag)
	{
		base.transform.GetChild(0).gameObject.SetActive(flag);
	}

	public FishingZoneCreator.ExpandSide Side;

	public float Width = 2f;

	public float Height = 2f;

	public float ballR = 0.1f;

	public Vector3 From;

	public Vector3 To;
}
