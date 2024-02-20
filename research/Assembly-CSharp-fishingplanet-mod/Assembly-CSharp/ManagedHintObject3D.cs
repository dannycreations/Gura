using System;
using UnityEngine;

public class ManagedHintObject3D : ManagedHintObject
{
	private void Awake()
	{
		this.rendrer = base.GetComponent<MeshRenderer>();
		this.color = this.rendrer.material.color;
	}

	protected override void Show()
	{
		this.rendrer.material.color = this.color;
	}

	protected override void Hide()
	{
		this.rendrer.material.color = Color.clear;
	}

	private MeshRenderer rendrer;

	private Color color;
}
