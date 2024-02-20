using System;
using UnityEngine;

public class TriangleMovement
{
	public TriangleMovement()
	{
		this.SetRandom();
	}

	public TriangleMovement(float sideK, float coastK)
	{
		this.Set(sideK, coastK);
	}

	public float SideK
	{
		get
		{
			return this._sideK;
		}
	}

	public float CoastK
	{
		get
		{
			return this._coastK;
		}
	}

	public void SetRandom()
	{
		this._sideK = Random.Range(-1f, 1f);
		this._coastK = Random.value;
	}

	public void Set(float sideK, float coastK)
	{
		this._sideK = sideK;
		this._coastK = coastK;
	}

	private float _sideK;

	private float _coastK;
}
