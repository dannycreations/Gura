using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using Phy;
using UnityEngine;

public class ChumBall : MonoBehaviour
{
	public bool WasLaunched
	{
		get
		{
			return this._launchAt > 0f;
		}
	}

	public void Launch(float delay, float dist, Vector3 forward, Chum chum = null, Func<Vector3, Vector3, Vector3?> intersectionFunc = null)
	{
		this._renderers = RenderersHelper.GetAllRenderersForObject<Renderer>(base.transform);
		this._launchAt = Time.time + delay;
		this._dist = dist;
		this._forward = forward;
		this._forward.y = 0f;
		this._forward.Normalize();
		if (chum != null)
		{
			this._chum = chum;
			this._chum.WasThrown = true;
			this._intersectionFunc = intersectionFunc;
		}
	}

	public void SetupTexture(string sColor)
	{
		Renderer component = base.transform.GetChild(0).GetComponent<Renderer>();
		component.sharedMaterial.SetColor("_Color", LineBehaviour.HexToColor(sColor, Color.white));
	}

	private void Update()
	{
		if (this._curParicle != null)
		{
			this._stopParticleAt -= Time.deltaTime;
			if (this._stopParticleAt < 0f)
			{
				Object.Destroy(this._curParicle.gameObject);
				Object.Destroy(base.gameObject);
			}
			return;
		}
		GameFactory.Player.HudFishingHandler.LineHandler.LineLength = (int)MeasuringSystemManager.LineLength(Inventory.ChumHandMaxCastLength);
		if (this._launchAt > 0f && this._launchAt < Time.time)
		{
			if (!this._wasLaunched)
			{
				this._wasLaunched = true;
				base.transform.parent = null;
				Vector3 vector = base.transform.position + this._forward * this._dist;
				RaycastHit maskedRayHit = Math3d.GetMaskedRayHit(vector, vector - new Vector3(0f, 1000f, 0f), GlobalConsts.GroundObstacleMask | GlobalConsts.WaterMask);
				if (maskedRayHit.collider != null)
				{
					this._isWaterLanding = maskedRayHit.point.y < 0.02f;
					this._hitPoint = new Vector3?(maskedRayHit.point);
				}
				float magnitude = vector.magnitude;
				float num = this._dist / this._distToHModifier;
				vector.y = -0.1f;
				this._parabola = new VerticalParabola(base.transform.position, vector, base.transform.position.y + num);
				float num2 = Mathf.Atan(4f * num / magnitude);
				float num3 = Mathf.Sin(num2);
				float num4 = num3 * num3;
				float num5 = 9.81f;
				float num6 = Mathf.Sqrt(2f * num5 * num / num4);
				this._flyTime = 2f * num6 * num3 / num5;
			}
			float num7 = Time.time - this._launchAt;
			float num8 = Mathf.Clamp01(num7 / this._flyTime);
			Vector3 point = this._parabola.GetPoint(num8);
			if (this._chum != null && this._chum.Ingredients[0].SpecialItem == InventorySpecialItem.Snow && this._intersectionFunc != null)
			{
				Vector3? vector2 = this._intersectionFunc(point, base.transform.position);
				if (vector2 != null)
				{
					this.OnContact(vector2.Value);
					return;
				}
			}
			base.transform.position = point;
			GameFactory.Player.HudFishingHandler.LineHandler.LineInWater = (int)MeasuringSystemManager.LineLength(Mathf.Lerp(0f, this._dist, num8));
			if (base.transform.position.y < 0f && this._isWaterLanding && !this._wasWaterReached)
			{
				Vector3 vector3 = (base.transform.position - this._parabola.GetPoint(Mathf.Clamp01(Mathf.Min(num7, this._flyTime - Time.fixedDeltaTime) / this._flyTime))) / Time.fixedDeltaTime;
				this._wasWaterReached = true;
				if (GameFactory.Player != null)
				{
					DynWaterParticlesController.CreateSplash(GameFactory.PlayerTransform, base.transform.position, this._splashParticle, this._splashSize, 1f, true, true, 1);
					RandomSounds.PlaySoundAtPoint(this._splashSound, base.transform.position, this._splashSize * 0.5f * SettingsManager.EnvironmentForcedVolume, false);
					if (this._chum != null)
					{
						Chum chum = PhotonConnectionFactory.Instance.Profile.Inventory.OfType<Chum>().FirstOrDefault(delegate(Chum c)
						{
							Guid? thrownChumInstanceId = c.ThrownChumInstanceId;
							bool flag = thrownChumInstanceId != null;
							Guid? instanceId = this._chum.InstanceId;
							return flag == (instanceId != null) && (thrownChumInstanceId == null || thrownChumInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
						});
						if (chum != null)
						{
							GameFactory.Player.AddIndependentChum(chum, base.transform.position, vector3);
						}
					}
				}
			}
			if (num7 > this._flyTime)
			{
				this.OnContact((this._hitPoint == null) ? base.transform.position : this._hitPoint.Value);
			}
		}
	}

	private void OnContact(Vector3 pos)
	{
		if (!this._isWaterLanding)
		{
			if (!string.IsNullOrEmpty(this._contactSound))
			{
				RandomSounds.PlaySoundAtPoint(this._contactSound, pos, SettingsManager.EnvironmentForcedVolume, true);
			}
			if (this._contactParticle != null)
			{
				this._curParicle = Object.Instantiate<ParticleSystem>(this._contactParticle);
				this._curParicle.transform.position = pos;
				this._stopParticleAt = this._curParicle.main.duration;
				for (int i = 0; i < this._renderers.Count; i++)
				{
					this._renderers[i].enabled = false;
				}
				return;
			}
		}
		Object.Destroy(base.gameObject);
	}

	private const float WATER_TILE_Y = 0.02f;

	[SerializeField]
	private string _splashParticle = "2D/Splashes/pSplash_universal";

	[SerializeField]
	private string _splashSound = "Sounds/Actions/Lure/Lure_Splash_Big";

	[SerializeField]
	private ParticleSystem _contactParticle;

	[SerializeField]
	private string _contactSound;

	[SerializeField]
	private float _splashSize = 1f;

	[SerializeField]
	private float _distToHModifier = 5f;

	private float _launchAt = -1f;

	private bool _wasLaunched;

	private float _dist;

	private VerticalParabola _parabola;

	private float _flyTime;

	private bool _wasWaterReached;

	private bool _isWaterLanding;

	private Vector3 _forward;

	private Chum _chum;

	private Func<Vector3, Vector3, Vector3?> _intersectionFunc;

	private ParticleSystem _curParicle;

	private float _stopParticleAt = -1f;

	private Vector3? _hitPoint;

	private List<Renderer> _renderers;
}
