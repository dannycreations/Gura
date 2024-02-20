using System;
using System.Collections;
using UnityEngine;

namespace FireWork
{
	public class ParticleSystemSound : MonoBehaviour
	{
		public void LateUpdate()
		{
			if (SoundController.instance == null)
			{
				return;
			}
			ParticleSystem component = base.GetComponent<ParticleSystem>();
			ParticleSystem.Particle[] array = new ParticleSystem.Particle[component.particleCount];
			int particles = component.GetParticles(array);
			for (int i = 0; i < particles; i++)
			{
				if (this._explosionSound.Length > 0 && array[i].remainingLifetime < Time.deltaTime)
				{
					SoundController.instance.Play(this._explosionSound[Random.Range(0, this._explosionSound.Length)], Random.Range(this._explosionVolumeMax, this._explosionVolumeMin), Random.Range(this._explosionPitchMin, this._explosionPitchMax), array[i].position);
					if (this._crackleSound.Length > 0)
					{
						for (int j = 0; j < this._crackleMultiplier; j++)
						{
							base.StartCoroutine(this.Crackle(array[i].position, this._crackleDelay + (float)j * 0.1f));
						}
					}
				}
				if (this._shootSound.Length > 0 && array[i].remainingLifetime >= array[i].startLifetime - Time.deltaTime)
				{
					SoundController.instance.Play(this._shootSound[Random.Range(0, this._shootSound.Length)], Random.Range(this._shootVolumeMax, this._shootVolumeMin), Random.Range(this._shootPitchMin, this._shootPitchMax), array[i].position);
				}
			}
		}

		public IEnumerator Crackle(Vector3 pos, float delay)
		{
			yield return new WaitForSeconds(delay);
			SoundController.instance.Play(this._crackleSound[Random.Range(0, this._crackleSound.Length)], Random.Range(this._crackleVolumeMax, this._crackleVolumeMin), Random.Range(this._cracklePitchMax, this._cracklePitchMin), pos);
			yield break;
		}

		public AudioClip[] _shootSound;

		public float _shootPitchMax = 1.25f;

		public float _shootPitchMin = 0.75f;

		public float _shootVolumeMax = 0.75f;

		public float _shootVolumeMin = 0.25f;

		public AudioClip[] _explosionSound;

		public float _explosionPitchMax = 1.25f;

		public float _explosionPitchMin = 0.75f;

		public float _explosionVolumeMax = 0.75f;

		public float _explosionVolumeMin = 0.25f;

		public AudioClip[] _crackleSound;

		public float _crackleDelay = 0.25f;

		public int _crackleMultiplier = 3;

		public float _cracklePitchMax = 1.25f;

		public float _cracklePitchMin = 0.75f;

		public float _crackleVolumeMax = 0.75f;

		public float _crackleVolumeMin = 0.25f;
	}
}
