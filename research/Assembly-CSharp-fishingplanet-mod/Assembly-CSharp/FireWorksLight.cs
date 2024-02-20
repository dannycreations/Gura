using System;
using UnityEngine;

public class FireWorksLight : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		this.InitializeIfNeeded();
		int particles = this.m_System.GetParticles(this.m_Particles);
		for (int i = 0; i < particles; i++)
		{
			if (this.m_Particles[i].remainingLifetime < Time.deltaTime)
			{
				this.light = new GameObject("Fireworks Light");
				this.light.transform.parent = base.gameObject.transform;
				this.light.transform.position = this.m_Particles[i].position;
				Light light = this.light.AddComponent<Light>();
				light.color = this.color;
				light.type = 2;
				light.shadows = 0;
				light.renderMode = 2;
				light.range = this.range;
				light.intensity = this.intensity / 2f;
				Object.Destroy(this.light, this.destroyDelay);
			}
		}
	}

	private void InitializeIfNeeded()
	{
		if (this.m_System == null)
		{
			this.m_System = base.GetComponent<ParticleSystem>();
		}
		if (this.m_Particles == null || this.m_Particles.Length < this.m_System.maxParticles)
		{
			this.m_Particles = new ParticleSystem.Particle[this.m_System.maxParticles];
		}
	}

	public Color color = Color.white;

	public float destroyDelay = 0.5f;

	public float intensity = 7f;

	public float range = 400f;

	private ParticleSystem m_System;

	private ParticleSystem.Particle[] m_Particles;

	private GameObject light;
}
