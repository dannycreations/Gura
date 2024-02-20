using System;
using UnityEngine;

public class RodPodSlot : MonoBehaviour
{
	public bool IsActive { get; private set; }

	public Collider Collider
	{
		get
		{
			return this._collider;
		}
	}

	private void Awake()
	{
		this._renderer = RenderersHelper.GetRendererForObject<Renderer>(base.transform);
	}

	public void SetupCollider(Collider collider)
	{
		this._collider = collider;
	}

	public void SetSlotVisibililty(bool flag)
	{
		this._renderer.enabled = flag;
	}

	public void SetActive(bool flag)
	{
		this.IsActive = flag;
		base.gameObject.SetActive(flag);
	}

	public void ActivateCollider(bool flag)
	{
		this._collider.enabled = flag;
	}

	[Tooltip("Please setup manually if collider is not a child of slot")]
	[SerializeField]
	private Collider _collider;

	private Renderer _renderer;
}
