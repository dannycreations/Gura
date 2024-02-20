using System;
using UnityEngine;

[AddComponentMenu("Rendering/SetRenderQueue")]
public class SetRenderQueue : MonoBehaviour
{
	protected void Awake()
	{
		Renderer component = base.GetComponent<Renderer>();
		if (component == null)
		{
			return;
		}
		Material[] materials = component.materials;
		int num = 0;
		while (num < materials.Length && num < this.m_queues.Length)
		{
			materials[num].renderQueue = this.m_queues[num];
			num++;
		}
	}

	[SerializeField]
	protected int[] m_queues = new int[] { 3000 };
}
