using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM
{
	public class FireworkHandler : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event FireworkHandler.ObjectPutDelegate OnPut = delegate
		{
		};

		public void PutAction(int flag)
		{
			if (flag != 0)
			{
				this.OnPut();
			}
		}

		public delegate void ObjectPutDelegate();
	}
}
