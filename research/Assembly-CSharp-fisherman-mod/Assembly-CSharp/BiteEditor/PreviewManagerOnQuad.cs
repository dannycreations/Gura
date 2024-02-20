using System;
using UnityEngine;

namespace BiteEditor
{
	[Serializable]
	public class PreviewManagerOnQuad
	{
		public Vector3 Position
		{
			get
			{
				return this._projector.transform.position;
			}
		}

		public bool Enable
		{
			get
			{
				return this._projector.activeInHierarchy;
			}
			set
			{
				this._projector.SetActive(value);
			}
		}

		public void CreateProjector(Material material)
		{
			if (this._projector != null)
			{
				return;
			}
			this._projector = GameObject.CreatePrimitive(5);
			this._projector.transform.rotation = Quaternion.AngleAxis(90f, Vector3.right);
			this._projector.GetComponent<MeshCollider>().enabled = false;
			this._projector.hideFlags = 61;
			this._renderer = this._projector.GetComponent<Renderer>();
			this._renderer.sharedMaterial = material;
		}

		public void SelectBrush(Texture2D brushTexture, Vector2 brushSize, BiteMapPatch patch)
		{
			this._renderer.sharedMaterial.mainTexture = brushTexture;
			this.ChangeBrushSize(brushSize, patch);
		}

		public void ChangeBrushSize(Vector2 brushSize, BiteMapPatch patch)
		{
			this._renderer.transform.localScale = new Vector3(brushSize.x, brushSize.y, 0.1f);
		}

		public void TransformNow(Vector3 position)
		{
			this._projector.transform.position = position + new Vector3(0f, 0.01f, 0f);
		}

		public void DestoryProjector()
		{
			if (this._projector != null)
			{
				Object.DestroyImmediate(this._projector);
			}
		}

		private GameObject _projector;

		private Renderer _renderer;
	}
}
