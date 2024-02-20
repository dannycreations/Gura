using System;
using UnityEngine;

namespace TPM
{
	public class TPMModelLayerSettings : MonoBehaviour
	{
		public LayerPair[] Layers
		{
			get
			{
				return this.settings;
			}
		}

		public TPMFlashLightController FlashlightContoller
		{
			get
			{
				return this._flashlightController;
			}
		}

		private void Awake()
		{
			if (this.settings.Length == 1)
			{
				for (int i = 0; i < base.transform.childCount; i++)
				{
					SkinnedMeshRenderer component = base.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
					if (component != null)
					{
						this.settings[0].renderer = component;
						break;
					}
				}
			}
			for (int j = 0; j < base.transform.childCount; j++)
			{
				this._flashlightController = base.transform.GetChild(j).GetComponent<TPMFlashLightController>();
				if (this._flashlightController != null)
				{
					break;
				}
			}
		}

		public void SetLod(LODS lod)
		{
			for (int i = 0; i < this.settings.Length; i++)
			{
				LayerPair layerPair = this.settings[i];
				if (layerPair.renderer != null)
				{
					if (lod < (LODS)layerPair.lods.Length && layerPair.lods[(int)lod] != null)
					{
						layerPair.renderer.sharedMesh = Object.Instantiate<Mesh>(layerPair.lods[(int)lod]);
					}
					else if (layerPair.defaultMesh != null)
					{
						layerPair.renderer.sharedMesh = Object.Instantiate<Mesh>(layerPair.defaultMesh);
					}
					else
					{
						layerPair.renderer.sharedMesh = Object.Instantiate<Mesh>(layerPair.renderer.sharedMesh);
						LogHelper.Error("Can't select lod {0} for object's {1} layer {2}", new object[] { lod, base.gameObject, layerPair.layer });
					}
					layerPair.renderer.updateWhenOffscreen = true;
					layerPair.renderer.localBounds = new Bounds(layerPair.renderer.localBounds.center, new Vector3(5f, 5f, 5f));
					layerPair.renderer.updateWhenOffscreen = false;
				}
				else
				{
					LogHelper.Error("Ignore undefined renderer for objects's {0} layer {1}", new object[] { base.gameObject, layerPair.layer });
				}
			}
		}

		public LayerPair[] settings;

		private TPMFlashLightController _flashlightController;
	}
}
