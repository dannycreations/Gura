using System;
using UnityEngine;

public class SpriteSheet : MonoBehaviour
{
	private void Start()
	{
		this._size = new Vector2(1f / (float)this._uvTieX, 1f / (float)this._uvTieY);
		this._myRenderer = base.GetComponent<Renderer>();
		if (this._myRenderer == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		int num = (int)(Time.timeSinceLevelLoad * (float)this._fps) % (this._uvTieX * this._uvTieY);
		if (num != this._lastIndex)
		{
			int num2 = num % this._uvTieX;
			int num3 = 1;
			Vector2 vector;
			vector..ctor((float)num2 * this._size.x, 1f - this._size.y - (float)num3 * this._size.y);
			this._myRenderer.material.SetTextureOffset("_CausticTex", vector);
			this._myRenderer.material.SetTextureScale("_CausticTex", this._size);
			this._lastIndex = num;
		}
	}

	public int _uvTieX = 64;

	public int _uvTieY = 1;

	public int _fps = 10;

	private Vector2 _size;

	private Renderer _myRenderer;

	private int _lastIndex = -1;
}
