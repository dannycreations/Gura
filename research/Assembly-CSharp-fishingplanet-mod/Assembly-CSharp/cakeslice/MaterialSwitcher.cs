using System;
using UnityEngine;

namespace cakeslice
{
	public class MaterialSwitcher : MonoBehaviour
	{
		public void Update()
		{
			if (Input.GetKeyDown(109))
			{
				Material[] materials = base.GetComponent<Renderer>().materials;
				materials[this.index] = this.target;
				base.GetComponent<Renderer>().materials = materials;
			}
		}

		public Material target;

		public int index;
	}
}
