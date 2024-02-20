using System;
using System.Diagnostics;
using UnityEngine;

public class CastingHandsHanlders : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event CastingHandsHanlders.SetHandDelegate OnSetHand = delegate
	{
	};

	public void SetHand(int isLeft)
	{
		if (this.IsActive)
		{
			this.OnSetHand(isLeft != 0);
		}
	}

	public bool IsActive;

	public delegate void SetHandDelegate(bool isLeft);
}
