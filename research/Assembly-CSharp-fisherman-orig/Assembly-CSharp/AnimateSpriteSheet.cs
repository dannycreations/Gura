using System;
using System.Collections;
using UnityEngine;

internal class AnimateSpriteSheet : MonoBehaviour
{
	public float RunTimeInSeconds
	{
		get
		{
			return 1f / this.FramesPerSecond * (float)(this.Columns * this.Rows);
		}
	}

	private void Start()
	{
		this.materialCopy = new Material(base.GetComponent<Renderer>().sharedMaterial);
		base.GetComponent<Renderer>().sharedMaterial = this.materialCopy;
		Vector2 vector;
		vector..ctor(1f / (float)this.Columns, 1f / (float)this.Rows);
		base.GetComponent<Renderer>().sharedMaterial.SetTextureScale("_MainTex", vector);
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.UpdateTiling());
	}

	private IEnumerator UpdateTiling()
	{
		float x = 0f;
		float y = 0f;
		Vector2 offset = Vector2.zero;
		do
		{
			for (int i = this.Rows - 1; i >= 0; i--)
			{
				y = (float)i / (float)this.Rows;
				for (int j = 0; j <= this.Columns - 1; j++)
				{
					x = (float)j / (float)this.Columns;
					offset.Set(x, y);
					base.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
					yield return new WaitForSeconds(1f / this.FramesPerSecond);
				}
			}
		}
		while (!this.RunOnce);
		yield break;
		yield break;
	}

	public int Columns = 5;

	public int Rows = 5;

	public float FramesPerSecond = 10f;

	public bool RunOnce = true;

	private Material materialCopy;
}
