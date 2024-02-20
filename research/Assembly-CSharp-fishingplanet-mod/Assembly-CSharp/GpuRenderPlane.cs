using System;
using UnityEngine;

[ExecuteInEditMode]
public class GpuRenderPlane : MonoBehaviour
{
	private void Update()
	{
		if (this.field == null)
		{
			if (Application.isPlaying && base.gameObject)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DestroyImmediate(base.gameObject);
			}
		}
		else if (this.field.RenderPlane != this)
		{
			if (Application.isPlaying && base.gameObject)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DestroyImmediate(base.gameObject);
			}
		}
	}

	public FlowSimulationField field;
}
