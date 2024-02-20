using System;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Remove fluid")]
public class FluidRemoveField : FlowSimulationField
{
	public override FieldPass Pass
	{
		get
		{
			return FieldPass.RemoveFluid;
		}
	}

	protected override Shader RenderShader
	{
		get
		{
			return Shader.Find("Hidden/RemoveFluidPreview");
		}
	}
}
