using System;
using UnityEngine;

public class LayerCellGizmo : MonoBehaviour
{
	public void Init(Vector3 size, Color color)
	{
		this._size = size;
		this._color = color;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = this._color;
		Gizmos.DrawWireCube(base.transform.position + this._size * 0.5f, this._size);
	}

	[SerializeField]
	private Color _color;

	[SerializeField]
	private Vector3 _size;
}
