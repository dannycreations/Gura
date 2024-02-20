using System;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Add fluid")]
public class FluidAddField : FlowSimulationField
{
	public override FieldPass Pass
	{
		get
		{
			return FieldPass.AddFluid;
		}
	}

	protected override Shader RenderShader
	{
		get
		{
			return Shader.Find("Hidden/AddFluidPreview");
		}
	}
}
