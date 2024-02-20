using System;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MoveByKeys : MonoBehaviour
{
	public void Start()
	{
		this.isSprite = base.GetComponent<SpriteRenderer>() != null;
		this.body2d = base.GetComponent<Rigidbody2D>();
		this.body = base.GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		if (!base.photonView.isMine)
		{
			return;
		}
		if (Input.GetKey(97))
		{
			base.transform.position += Vector3.left * (this.Speed * Time.deltaTime);
		}
		if (Input.GetKey(100))
		{
			base.transform.position += Vector3.right * (this.Speed * Time.deltaTime);
		}
		if (this.jumpingTime <= 0f)
		{
			if ((this.body != null || this.body2d != null) && Input.GetKey(32))
			{
				this.jumpingTime = this.JumpTimeout;
				Vector2 vector = Vector2.up * this.JumpForce;
				if (this.body2d != null)
				{
					this.body2d.AddForce(vector);
				}
				else if (this.body != null)
				{
					this.body.AddForce(vector);
				}
			}
		}
		else
		{
			this.jumpingTime -= Time.deltaTime;
		}
		if (!this.isSprite)
		{
			if (Input.GetKey(119))
			{
				base.transform.position += Vector3.forward * (this.Speed * Time.deltaTime);
			}
			if (Input.GetKey(115))
			{
				base.transform.position -= Vector3.forward * (this.Speed * Time.deltaTime);
			}
		}
	}

	public float Speed = 10f;

	public float JumpForce = 200f;

	public float JumpTimeout = 0.5f;

	private bool isSprite;

	private float jumpingTime;

	private Rigidbody body;

	private Rigidbody2D body2d;
}
