using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM
{
	public class TPMAnimatorHandler : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ESyncSaling = delegate
		{
		};

		public void OnSalingSync(int flag)
		{
			this.ESyncSaling(flag != 0);
		}
	}
}
