using System;
using UnityEngine;

public class TournamentsHandler : MonoBehaviour
{
	private void OnEnable()
	{
		if (!this._observersInited)
		{
			this._observersInited = true;
		}
		this._sportinited = false;
	}

	private void Update()
	{
	}

	private bool _sportinited;

	private bool _observersInited;
}
