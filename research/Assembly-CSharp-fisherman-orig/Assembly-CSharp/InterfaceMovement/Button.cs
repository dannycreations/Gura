using System;
using UnityEngine;

namespace InterfaceMovement
{
	public class Button : MonoBehaviour
	{
		private void Start()
		{
			this.cachedRenderer = base.GetComponent<Renderer>();
		}

		private void Update()
		{
			bool flag = base.transform.parent.GetComponent<ButtonManager>().focusedButton == this;
			Color color = this.cachedRenderer.material.color;
			color.a = Mathf.MoveTowards(color.a, (!flag) ? 0.5f : 1f, Time.deltaTime * 3f);
			this.cachedRenderer.material.color = color;
		}

		private Renderer cachedRenderer;

		public Button up;

		public Button down;

		public Button left;

		public Button right;
	}
}
