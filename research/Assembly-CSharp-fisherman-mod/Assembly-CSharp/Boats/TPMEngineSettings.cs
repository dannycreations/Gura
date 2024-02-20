using System;
using UnityEngine;

namespace Boats
{
	[RequireComponent(typeof(Animator))]
	public class TPMEngineSettings : MonoBehaviour
	{
		public Animator Animator
		{
			get
			{
				return this._animator;
			}
		}

		private void Awake()
		{
			this._animator = base.GetComponent<Animator>();
		}

		private Animator _animator;
	}
}
