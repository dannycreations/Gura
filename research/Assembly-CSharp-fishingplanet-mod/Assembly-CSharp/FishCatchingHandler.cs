using System;
using System.Diagnostics;
using UnityEngine;

public class FishCatchingHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event FishCatchingHandler.FishCatchDelegate OnCatch = delegate
	{
	};

	public void CatchAction(int flag)
	{
		this.OnCatch(flag != 0);
	}

	public delegate void FishCatchDelegate(bool flag);
}
