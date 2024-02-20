using System;
using System.Collections;
using UnityEngine;

internal class AnimateTiledTexture : MonoBehaviour
{
	private void Start()
	{
		base.StartCoroutine(this.updateTiling());
		Vector2 vector;
		vector..ctor(1f / (float)this.columns, 1f / (float)this.rows);
		base.GetComponent<Renderer>().sharedMaterial.SetTextureScale("_MainTex", vector);
	}

	private IEnumerator updateTiling()
	{
		for (;;)
		{
			this.index++;
			if (this.index >= this.rows * this.columns)
			{
				this.index = 0;
			}
			Vector2 offset = new Vector2((float)this.index / (float)this.columns - (float)(this.index / this.columns), (float)(this.index / this.columns) / (float)this.rows);
			base.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
			yield return new WaitForSeconds(1f / this.framesPerSecond);
		}
		yield break;
	}

	public int columns = 2;

	public int rows = 2;

	public float framesPerSecond = 10f;

	private int index;
}
