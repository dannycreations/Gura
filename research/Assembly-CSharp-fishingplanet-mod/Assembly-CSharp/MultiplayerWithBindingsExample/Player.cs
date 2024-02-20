using System;
using UnityEngine;

namespace MultiplayerWithBindingsExample
{
	public class Player : MonoBehaviour
	{
		public PlayerActions Actions { get; set; }

		private void OnDisable()
		{
			if (this.Actions != null)
			{
				this.Actions.Destroy();
			}
		}

		private void Start()
		{
			this.cachedRenderer = base.GetComponent<Renderer>();
		}

		private void Update()
		{
			if (this.Actions == null)
			{
				this.cachedRenderer.material.color = new Color(1f, 1f, 1f, 0.2f);
			}
			else
			{
				this.cachedRenderer.material.color = this.GetColorFromInput();
				base.transform.Rotate(Vector3.down, 500f * Time.deltaTime * this.Actions.Rotate.X, 0);
				base.transform.Rotate(Vector3.right, 500f * Time.deltaTime * this.Actions.Rotate.Y, 0);
			}
		}

		private Color GetColorFromInput()
		{
			if (this.Actions.Green)
			{
				return Color.green;
			}
			if (this.Actions.Red)
			{
				return Color.red;
			}
			if (this.Actions.Blue)
			{
				return Color.blue;
			}
			if (this.Actions.Yellow)
			{
				return Color.yellow;
			}
			return Color.white;
		}

		private Renderer cachedRenderer;
	}
}
