using System;
using UnityEngine;

namespace cakeslice
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
	public class Outline : MonoBehaviour
	{
		public Renderer Renderer { get; private set; }

		public MeshFilter MF { get; private set; }

		public SkinnedMeshRenderer SMR { get; private set; }

		public int ColorIndex
		{
			get
			{
				return this._colorIndex;
			}
		}

		private void Awake()
		{
			this.Renderer = base.GetComponent<Renderer>();
			this.MF = base.GetComponent<MeshFilter>();
			this.SMR = base.GetComponent<SkinnedMeshRenderer>();
		}

		public void SetColor(Outline.Color color)
		{
			this._colorIndex = (int)((byte)color);
		}

		private void OnEnable()
		{
			if (OutlineEffect.Instance != null)
			{
				OutlineEffect.Instance.AddOutline(this);
			}
		}

		private void OnDisable()
		{
			if (OutlineEffect.Instance != null)
			{
				OutlineEffect.Instance.RemoveOutline(this);
			}
		}

		private void OnDestroy()
		{
			this.Renderer = null;
			this.MF = null;
			this.SMR = null;
		}

		[Range(0f, 2f)]
		[SerializeField]
		private int _colorIndex;

		public enum Color
		{
			Error,
			Valid,
			Select
		}
	}
}
