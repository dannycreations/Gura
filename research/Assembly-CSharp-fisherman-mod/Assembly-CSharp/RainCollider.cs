using System;
using UnityEngine;

public class RainCollider : MonoBehaviour
{
	private void Start()
	{
		this.part = base.GetComponent<ParticleSystem>();
		this.collisionEvents = new ParticleCollisionEvent[16];
	}

	private void OnParticleCollision(GameObject other)
	{
		int safeCollisionEventSize = ParticlePhysicsExtensions.GetSafeCollisionEventSize(this.part);
		if (this.collisionEvents.Length < safeCollisionEventSize)
		{
			this.collisionEvents = new ParticleCollisionEvent[safeCollisionEventSize];
		}
		int num = ParticlePhysicsExtensions.GetCollisionEvents(this.part, other, this.collisionEvents);
		Rigidbody component = other.GetComponent<Rigidbody>();
		for (int i = 0; i < num; i++)
		{
			Vector3 intersection = this.collisionEvents[i].intersection;
			if (this.explosionObject)
			{
				Object.Instantiate<Transform>(this.explosionObject, intersection, Quaternion.identity);
			}
			if (GameFactory.Water != null)
			{
				GameFactory.Water.AddWaterDisturb(new Vector3(intersection.x, 0f, intersection.z), Random.Range(0.08f, 0.12f), Random.Range(3f, 5f));
			}
		}
	}

	public ParticleSystem part;

	public ParticleCollisionEvent[] collisionEvents;

	public Transform explosionObject;
}
