using System;
using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Heightmap")]
public class HeightmapField : FlowSimulationField
{
	public override FieldPass Pass
	{
		get
		{
			return FieldPass.Heightmap;
		}
	}

	protected override Shader RenderShader
	{
		get
		{
			return Shader.Find("Hidden/HeightmapFieldPreview");
		}
	}
}
