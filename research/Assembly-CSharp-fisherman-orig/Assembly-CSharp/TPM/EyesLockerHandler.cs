using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM
{
	public class EyesLockerHandler : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> EChangeState = delegate
		{
		};

		public void SetLock(int flag)
		{
			this.EChangeState(flag != 0);
		}
	}
}
