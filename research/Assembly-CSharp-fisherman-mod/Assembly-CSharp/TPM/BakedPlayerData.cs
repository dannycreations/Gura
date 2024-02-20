using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class BakedPlayerData : MonoBehaviour
	{
		public Dictionary<CustomizedParts, TPMModelLayerSettings> Parts
		{
			get
			{
				return this._parts;
			}
		}

		public Player Player
		{
			get
			{
				return this._player;
			}
		}

		public SkinnedMeshRenderer BakedRenderer
		{
			get
			{
				return this._bakedRenderer;
			}
		}

		public void SetData(Player player, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, SkinnedMeshRenderer bakedRenderer)
		{
			this._player = player;
			this._parts = parts;
			this._bakedRenderer = bakedRenderer;
		}

		private Dictionary<CustomizedParts, TPMModelLayerSettings> _parts;

		private Player _player;

		private SkinnedMeshRenderer _bakedRenderer;
	}
}
