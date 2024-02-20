using System;
using UnityEngine;

namespace TPM
{
	[Serializable]
	public class LayerPair
	{
		public MeshBakerLayers layer;

		[Tooltip("Could be empty if only one SkinnedMeshRenderer present - will be filled automatically in this case")]
		public SkinnedMeshRenderer renderer;

		[Tooltip("Please fill this path if you wants to replace textures for this object with High Quality copies")]
		public string texturesPath;

		[Tooltip("Fill please with meshes for lods from 0 to 3")]
		public Mesh[] lods = new Mesh[4];

		[Tooltip("Will be used for empty sections of lods array. Could be empty if all cells are filled")]
		public Mesh defaultMesh;
	}
}
