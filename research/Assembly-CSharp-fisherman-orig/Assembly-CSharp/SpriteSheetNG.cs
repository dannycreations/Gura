using System;
using UnityEngine;

public class SpriteSheetNG : MonoBehaviour
{
	private void Start()
	{
		this._size = new Vector2(1f / (float)this._uvTieX, 1f / (float)this._uvTieY);
		this._myRenderer = base.GetComponent<Renderer>();
		if (this._myRenderer == null)
		{
			base.enabled = false;
		}
		this._myRenderer.material.SetTextureScale("_MainTex", this._size);
	}

	private void Update()
	{
		int num = (int)(Time.timeSinceLevelLoad * (float)this._fps) % (this._uvTieX * this._uvTieY);
		if (num != this._lastIndex)
		{
			Vector2 vector;
			vector..ctor(this.iX * this._size.x, 1f - this._size.y * this.iY);
			this.iX += 1f;
			if (this.iX / (float)this._uvTieX == 1f)
			{
				if (this._uvTieY != 1)
				{
					this.iY += 1f;
				}
				this.iX = 0f;
				if (this.iY / (float)this._uvTieY == 1f)
				{
					this.iY = 1f;
				}
			}
			this._myRenderer.material.SetTextureOffset("_MainTex", vector);
			this._lastIndex = num;
		}
	}

	private float iX;

	private float iY = 1f;

	public int _uvTieX = 1;

	public int _uvTieY = 1;

	public int _fps = 10;

	private Vector2 _size;

	private Renderer _myRenderer;

	private int _lastIndex = -1;
}
