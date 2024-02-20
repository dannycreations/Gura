using System;
using UnityEngine;

[ExecuteInEditMode]
public class AssignGeneratorUV : MonoBehaviour
{
	private void Update()
	{
		for (int i = 0; i < this.renderers.Length; i++)
		{
			if (this.generator)
			{
				this.position = this.generator.Position;
				this.dimensions = this.generator.Dimensions;
			}
			Vector4 zero = Vector4.zero;
			if (this.dimensions.x < this.dimensions.y)
			{
				zero..ctor(this.dimensions.x * (this.dimensions.y / this.dimensions.x), this.dimensions.y, this.position.x, this.position.z);
			}
			else
			{
				zero..ctor(this.dimensions.x, this.dimensions.y * (this.dimensions.x / this.dimensions.y), this.position.x, this.position.z);
			}
			if (this.assignToSharedMaterial)
			{
				this.renderers[i].sharedMaterial.SetVector(this.uvVectorName, zero);
			}
			else
			{
				this.renderers[i].material.SetVector(this.uvVectorName, zero);
			}
		}
	}

	[SerializeField]
	private FlowmapGenerator generator;

	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Vector2 dimensions;

	[SerializeField]
	private Renderer[] renderers;

	[SerializeField]
	private bool assignToSharedMaterial = true;

	[SerializeField]
	private string uvVectorName = "_FlowmapUV";
}
