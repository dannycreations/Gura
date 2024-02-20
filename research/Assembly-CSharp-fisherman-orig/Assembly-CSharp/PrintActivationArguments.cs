using System;
using System.Diagnostics;
using UnityEngine;

public class PrintActivationArguments : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action AppActivated;

	private void Awake()
	{
	}
}
