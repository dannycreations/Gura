using System;
using UnityEngine;

namespace TPM
{
	public class ModelColorSettings : MonoBehaviour
	{
		public SkinnedMeshRenderer GetRenderer(ColorGroup colorGroup)
		{
			for (int i = 0; i < this._settings.Length; i++)
			{
				if (this._settings[i].colorGroup == colorGroup)
				{
					return this._settings[i].renderer;
				}
			}
			return null;
		}

		[SerializeField]
		private ColorPair[] _settings;
	}
}
