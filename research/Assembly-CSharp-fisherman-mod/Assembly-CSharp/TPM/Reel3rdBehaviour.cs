using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPM
{
	public class Reel3rdBehaviour : ReelBehaviour
	{
		public Reel3rdBehaviour(ReelController owner, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot)
			: base(owner, rodAssembly, rodSlot, null)
		{
		}

		public new void Init()
		{
			this._renderers = RenderersHelper.GetAllRenderersForObject<SkinnedMeshRenderer>(base.gameObject.transform);
		}

		public void SetVisibility(bool flag)
		{
			for (int i = 0; i < this._renderers.Count; i++)
			{
				this._renderers[i].enabled = flag;
			}
		}

		public void SetOpaque(float prc)
		{
			for (int i = 0; i < this._renderers.Count; i++)
			{
				this._renderers[i].material.SetFloat("_CharacterOpaque", prc);
			}
		}

		private List<SkinnedMeshRenderer> _renderers;
	}
}
