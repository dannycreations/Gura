using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM
{
	[RequireComponent(typeof(SkinnedMeshRenderer))]
	public class BonesRemappingHandler : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<SkinnedMeshRenderer> EReMapped = delegate
		{
		};

		private void Awake()
		{
			this._renderer = base.GetComponent<SkinnedMeshRenderer>();
		}

		public void OnRemapped()
		{
			this.EReMapped(this._renderer);
		}

		private SkinnedMeshRenderer _renderer;
	}
}
