using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WFX_BulletHoleDecal : MonoBehaviour
{
	private void Awake()
	{
		this.color = base.GetComponent<Renderer>().material.GetColor("_TintColor");
		this.orgAlpha = this.color.a;
	}

	private void OnEnable()
	{
		int num = Random.Range(0, (int)(this.frames.x * this.frames.y));
		int num2 = (int)((float)num % this.frames.x);
		int num3 = (int)((float)num / this.frames.y);
		Vector2[] array = new Vector2[4];
		for (int i = 0; i < 4; i++)
		{
			array[i].x = (WFX_BulletHoleDecal.quadUVs[i].x + (float)num2) * (1f / this.frames.x);
			array[i].y = (WFX_BulletHoleDecal.quadUVs[i].y + (float)num3) * (1f / this.frames.y);
		}
		base.GetComponent<MeshFilter>().mesh.uv = array;
		if (this.randomRotation)
		{
			base.transform.Rotate(0f, 0f, Random.Range(0f, 360f), 1);
		}
		this.life = this.lifetime;
		this.fadeout = this.life * (this.fadeoutpercent / 100f);
		this.color.a = this.orgAlpha;
		base.GetComponent<Renderer>().material.SetColor("_TintColor", this.color);
		base.StopAllCoroutines();
		base.StartCoroutine("holeUpdate");
	}

	private IEnumerator holeUpdate()
	{
		while (this.life > 0f)
		{
			this.life -= Time.deltaTime;
			if (this.life <= this.fadeout)
			{
				this.color.a = Mathf.Lerp(0f, this.orgAlpha, this.life / this.fadeout);
				base.GetComponent<Renderer>().material.SetColor("_TintColor", this.color);
			}
			yield return null;
		}
		yield break;
	}

	private static Vector2[] quadUVs = new Vector2[]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f)
	};

	public float lifetime = 10f;

	public float fadeoutpercent = 80f;

	public Vector2 frames;

	public bool randomRotation;

	public bool deactivate;

	private float life;

	private float fadeout;

	private Color color;

	private float orgAlpha;
}
