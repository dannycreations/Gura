using System;
using System.Collections.Generic;
using UnityEngine;

namespace mset
{
	[ExecuteInEditMode]
	public class SkyManager : MonoBehaviour
	{
		public static SkyManager Get()
		{
			if (SkyManager._Instance == null)
			{
				SkyManager._Instance = Object.FindObjectOfType<SkyManager>();
			}
			return SkyManager._Instance;
		}

		public bool BlendingSupport
		{
			get
			{
				return this._BlendingSupport;
			}
			set
			{
				this._BlendingSupport = value;
				Sky.EnableBlendingSupport(value);
				if (!value)
				{
					Sky.EnableTerrainBlending(false);
				}
			}
		}

		public bool ProjectionSupport
		{
			get
			{
				return this._ProjectionSupport;
			}
			set
			{
				this._ProjectionSupport = value;
				Sky.EnableProjectionSupport(value);
			}
		}

		public Sky GlobalSky
		{
			get
			{
				return this._GlobalSky;
			}
			set
			{
				this.BlendToGlobalSky(value, 0f);
			}
		}

		public void BlendToGlobalSky(Sky next)
		{
			this.BlendToGlobalSky(next, this.GlobalBlendTime, 0f);
		}

		public void BlendToGlobalSky(Sky next, float blendTime)
		{
			this.BlendToGlobalSky(next, blendTime, 0f);
		}

		public void BlendToGlobalSky(Sky next, float blendTime, float skipTime)
		{
			if (next != null)
			{
				this.nextSky = next;
				this.nextBlendTime = blendTime;
				this.nextSkipTime = skipTime;
			}
			this._GlobalSky = this.nextSky;
		}

		private void ResetLightBlend()
		{
			if (this.nextLights != null)
			{
				for (int i = 0; i < this.nextLights.Length; i++)
				{
					this.nextLights[i].intensity = this.nextIntensities[i];
					this.nextLights[i].enabled = true;
				}
				this.nextLights = null;
				this.nextIntensities = null;
			}
			if (this.prevLights != null)
			{
				for (int j = 0; j < this.prevLights.Length; j++)
				{
					this.prevLights[j].intensity = this.prevIntensities[j];
					this.prevLights[j].enabled = false;
				}
				this.prevLights = null;
				this.prevIntensities = null;
			}
		}

		private void StartLightBlend(Sky prev, Sky next)
		{
			this.prevLights = null;
			this.prevIntensities = null;
			if (prev)
			{
				this.prevLights = prev.GetComponentsInChildren<Light>();
				if (this.prevLights != null && this.prevLights.Length > 0)
				{
					this.prevIntensities = new float[this.prevLights.Length];
					for (int i = 0; i < this.prevLights.Length; i++)
					{
						this.prevLights[i].enabled = true;
						this.prevIntensities[i] = this.prevLights[i].intensity;
					}
				}
			}
			this.nextLights = null;
			this.nextIntensities = null;
			if (next)
			{
				this.nextLights = next.GetComponentsInChildren<Light>();
				if (this.nextLights != null && this.nextLights.Length > 0)
				{
					this.nextIntensities = new float[this.nextLights.Length];
					for (int j = 0; j < this.nextLights.Length; j++)
					{
						this.nextIntensities[j] = this.nextLights[j].intensity;
						this.nextLights[j].enabled = true;
						this.nextLights[j].intensity = 0f;
					}
				}
			}
		}

		private void UpdateLightBlend()
		{
			if (this.GlobalBlender.IsBlending)
			{
				float blendWeight = this.GlobalBlender.BlendWeight;
				float num = 1f - blendWeight;
				for (int i = 0; i < this.prevLights.Length; i++)
				{
					this.prevLights[i].intensity = num * this.prevIntensities[i];
				}
				for (int j = 0; j < this.nextLights.Length; j++)
				{
					this.nextLights[j].intensity = blendWeight * this.nextIntensities[j];
				}
			}
			else
			{
				this.ResetLightBlend();
			}
		}

		private void HandleGlobalSkyChange()
		{
			if (this.nextSky != null)
			{
				this.ResetLightBlend();
				if (this.BlendingSupport && this.nextBlendTime > 0f)
				{
					Sky currentSky = this.GlobalBlender.CurrentSky;
					this.GlobalBlender.BlendTime = this.nextBlendTime;
					this.GlobalBlender.BlendToSky(this.nextSky);
					Sky[] array = Object.FindObjectsOfType<Sky>();
					foreach (Sky sky in array)
					{
						sky.ToggleChildLights(false);
					}
					this.GlobalBlender.SkipTime(this.nextSkipTime);
					this.StartLightBlend(currentSky, this.nextSky);
				}
				else
				{
					this.GlobalBlender.SnapToSky(this.nextSky);
					this.nextSky.Apply(0);
					this.nextSky.Apply(1);
					Sky[] array3 = Object.FindObjectsOfType<Sky>();
					foreach (Sky sky2 in array3)
					{
						sky2.ToggleChildLights(false);
					}
					this.nextSky.ToggleChildLights(true);
				}
				this._GlobalSky = this.nextSky;
				this.nextSky = null;
				if (!Application.isPlaying)
				{
					this.EditorApplySkies(true);
				}
			}
			this.UpdateLightBlend();
		}

		private Material SkyboxMaterial
		{
			get
			{
				if (this._SkyboxMaterial == null)
				{
					this._SkyboxMaterial = Resources.Load<Material>("skyboxMat");
					if (!this._SkyboxMaterial)
					{
						Debug.LogError("Failed to find skyboxMat material in Resources folder!");
					}
				}
				return this._SkyboxMaterial;
			}
		}

		public bool ShowSkybox
		{
			get
			{
				return this._ShowSkybox;
			}
			set
			{
				if (value && this.SkyboxMaterial)
				{
					if (RenderSettings.skybox != this.SkyboxMaterial)
					{
						RenderSettings.skybox = this.SkyboxMaterial;
					}
				}
				else if (RenderSettings.skybox != null && (RenderSettings.skybox == this._SkyboxMaterial || RenderSettings.skybox.name == "Internal IBL Skybox"))
				{
					RenderSettings.skybox = null;
				}
				this._ShowSkybox = value;
			}
		}

		private void Start()
		{
			Sky.ScrubGlobalKeywords();
			this._SkyboxMaterial = this.SkyboxMaterial;
			this.ShowSkybox = this._ShowSkybox;
			this.BlendingSupport = this._BlendingSupport;
			this.ProjectionSupport = this._ProjectionSupport;
			if (this._GlobalSky == null)
			{
				this._GlobalSky = base.gameObject.GetComponent<Sky>();
			}
			if (this._GlobalSky == null)
			{
				this._GlobalSky = Object.FindObjectOfType<Sky>();
			}
			this.GlobalBlender.SnapToSky(this._GlobalSky);
		}

		public void RegisterApplicator(SkyApplicator app)
		{
			this.skyApplicators.Add(app);
			foreach (Renderer renderer in this.dynamicRenderers)
			{
				app.RendererInside(renderer);
			}
			foreach (Renderer renderer2 in this.staticRenderers)
			{
				app.RendererInside(renderer2);
			}
		}

		public void UnregisterApplicator(SkyApplicator app, HashSet<Renderer> renderersToClear)
		{
			this.skyApplicators.Remove(app);
			foreach (Renderer renderer in renderersToClear)
			{
				if (this._GlobalSky != null)
				{
					this._GlobalSky.Apply(renderer, 0);
				}
			}
		}

		public void UnregisterRenderer(Renderer rend)
		{
			if (!this.dynamicRenderers.Remove(rend))
			{
				this.staticRenderers.Remove(rend);
			}
		}

		public void RegisterNewRenderer(Renderer rend)
		{
			if (!rend.gameObject.activeInHierarchy)
			{
				return;
			}
			int num = 1 << rend.gameObject.layer;
			if ((this.IgnoredLayerMask & num) != 0)
			{
				return;
			}
			if (rend.gameObject.isStatic)
			{
				if (!this.staticRenderers.Contains(rend))
				{
					this.staticRenderers.Add(rend);
					this.ApplyCorrectSky(rend);
				}
			}
			else if (!this.dynamicRenderers.Contains(rend))
			{
				this.dynamicRenderers.Add(rend);
				if (rend.GetComponent<SkyAnchor>() == null)
				{
					rend.gameObject.AddComponent(typeof(SkyAnchor));
				}
			}
		}

		public void SeekNewRenderers()
		{
			Renderer[] array = Object.FindObjectsOfType<MeshRenderer>();
			for (int i = 0; i < array.Length; i++)
			{
				this.RegisterNewRenderer(array[i]);
			}
			array = Object.FindObjectsOfType<SkinnedMeshRenderer>();
			for (int j = 0; j < array.Length; j++)
			{
				this.RegisterNewRenderer(array[j]);
			}
		}

		public void ApplyCorrectSky(Renderer rend)
		{
			bool flag = false;
			SkyAnchor component = rend.GetComponent<SkyAnchor>();
			if (component && component.BindType == SkyAnchor.AnchorBindType.TargetSky)
			{
				component.Apply();
				flag = true;
			}
			foreach (SkyApplicator skyApplicator in this.skyApplicators)
			{
				if (flag)
				{
					skyApplicator.RemoveRenderer(rend);
				}
				else if (skyApplicator.RendererInside(rend))
				{
					flag = true;
				}
			}
			if (!flag && this._GlobalSky != null)
			{
				if (component != null)
				{
					if (component.CurrentApplicator != null)
					{
						component.CurrentApplicator.RemoveRenderer(rend);
						component.CurrentApplicator = null;
					}
					component.BlendToGlobalSky(this._GlobalSky);
				}
				if (!this.globalSkyChildren.Contains(rend))
				{
					this.globalSkyChildren.Add(rend);
				}
			}
			if ((flag || this._GlobalSky == null) && this.globalSkyChildren.Contains(rend))
			{
				this.globalSkyChildren.Remove(rend);
			}
		}

		public void EditorUpdate(bool forceApply)
		{
			Sky.EnableGlobalProjection(true);
			Sky.EnableBlendingSupport(false);
			Sky.EnableTerrainBlending(false);
			if (this._GlobalSky)
			{
				this._GlobalSky.Apply(0);
				this._GlobalSky.Apply(1);
				if (this.SkyboxMaterial)
				{
					this._GlobalSky.Apply(this.SkyboxMaterial, 0);
					this._GlobalSky.Apply(this.SkyboxMaterial, 1);
				}
				this._GlobalSky.Dirty = false;
			}
			this.HandleGlobalSkyChange();
			if (this.EditorAutoApply)
			{
				this.EditorApplySkies(forceApply);
			}
		}

		private void EditorApplySkies(bool forceApply)
		{
			Shader.SetGlobalVector("_UniformOcclusion", Vector4.one);
			SkyApplicator[] array = Object.FindObjectsOfType<SkyApplicator>();
			object[] array2 = Object.FindObjectsOfType<MeshRenderer>();
			this.EditorApplyToList(array2, array, forceApply);
			array2 = Object.FindObjectsOfType<SkinnedMeshRenderer>();
			this.EditorApplyToList(array2, array, forceApply);
		}

		private void EditorApplyToList(object[] renderers, SkyApplicator[] apps, bool forceApply)
		{
			foreach (object obj in renderers)
			{
				Renderer renderer = (Renderer)obj;
				int num = 1 << renderer.gameObject.layer;
				if ((this.IgnoredLayerMask & num) == 0)
				{
					if (renderer.gameObject.activeInHierarchy)
					{
						if (forceApply)
						{
							MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
							materialPropertyBlock.Clear();
							renderer.SetPropertyBlock(materialPropertyBlock);
						}
						SkyAnchor skyAnchor = renderer.gameObject.GetComponent<SkyAnchor>();
						if (skyAnchor && !skyAnchor.enabled)
						{
							skyAnchor = null;
						}
						bool flag = renderer.transform.hasChanged || (skyAnchor && skyAnchor.HasChanged);
						bool flag2 = false;
						if (skyAnchor && skyAnchor.BindType == SkyAnchor.AnchorBindType.TargetSky)
						{
							skyAnchor.Apply();
							flag2 = true;
						}
						if (this.GameAutoApply && !flag2)
						{
							foreach (SkyApplicator skyApplicator in apps)
							{
								if (skyApplicator.gameObject.activeInHierarchy)
								{
									if (skyApplicator.TargetSky && (forceApply || skyApplicator.HasChanged || skyApplicator.TargetSky.Dirty || flag))
									{
										flag2 |= skyApplicator.ApplyInside(renderer);
										skyApplicator.TargetSky.Dirty = false;
									}
									skyApplicator.HasChanged = false;
								}
							}
						}
						if (!flag2 && this._GlobalSky && (forceApply || this._GlobalSky.Dirty || flag))
						{
							this._GlobalSky.Apply(renderer, 0);
						}
						renderer.transform.hasChanged = false;
						if (skyAnchor)
						{
							skyAnchor.HasChanged = false;
						}
					}
				}
			}
			if (forceApply && this._GlobalSky)
			{
				this._GlobalSky.Apply(0);
				if (this._SkyboxMaterial)
				{
					this._GlobalSky.Apply(this._SkyboxMaterial, 0);
				}
				this._GlobalSky.Dirty = false;
			}
		}

		public void LateUpdate()
		{
			if (this.firstFrame && this._GlobalSky)
			{
				this.firstFrame = false;
				this._GlobalSky.Apply(0);
				this._GlobalSky.Apply(1);
				if (this._SkyboxMaterial)
				{
					this._GlobalSky.Apply(this._SkyboxMaterial, 0);
					this._GlobalSky.Apply(this._SkyboxMaterial, 1);
				}
			}
			float num = 0f;
			if (this.lastTimestamp > 0f)
			{
				num = Time.realtimeSinceStartup - this.lastTimestamp;
			}
			this.lastTimestamp = Time.realtimeSinceStartup;
			this.seekTimer -= num;
			this.HandleGlobalSkyChange();
			this.GameApplySkies(false);
		}

		public void GameApplySkies(bool forceApply)
		{
			this.GlobalBlender.ApplyToTerrain();
			this.GlobalBlender.Apply();
			if (this._SkyboxMaterial)
			{
				this.GlobalBlender.Apply(this._SkyboxMaterial);
			}
			if (this.GameAutoApply || forceApply)
			{
				if (this.seekTimer <= 0f || forceApply)
				{
					this.SeekNewRenderers();
					this.seekTimer = 0.5f;
				}
				List<SkyApplicator> list = new List<SkyApplicator>();
				foreach (SkyApplicator skyApplicator in this.skyApplicators)
				{
					if (skyApplicator == null || skyApplicator.gameObject == null)
					{
						list.Add(skyApplicator);
					}
				}
				foreach (SkyApplicator skyApplicator2 in list)
				{
					this.skyApplicators.Remove(skyApplicator2);
				}
				if (this.GlobalBlender.IsBlending || this.GlobalBlender.CurrentSky.Dirty || this.GlobalBlender.WasBlending(Time.deltaTime))
				{
					foreach (Renderer renderer in this.globalSkyChildren)
					{
						if (renderer)
						{
							SkyAnchor component = renderer.GetComponent<SkyAnchor>();
							if (component != null)
							{
								this.GlobalBlender.Apply(renderer, component.materials);
							}
						}
					}
				}
				int num = 0;
				int num2 = 0;
				List<Renderer> list2 = new List<Renderer>();
				foreach (Renderer renderer2 in this.dynamicRenderers)
				{
					num2++;
					if (forceApply || num2 >= this.renderCheckIterator)
					{
						if (renderer2 == null || renderer2.gameObject == null)
						{
							list2.Add(renderer2);
						}
						else if (renderer2.gameObject.activeInHierarchy)
						{
							this.renderCheckIterator++;
							if (!forceApply && num > 50)
							{
								num = 0;
								this.renderCheckIterator--;
								break;
							}
							SkyAnchor component2 = renderer2.GetComponent<SkyAnchor>();
							if (component2.HasChanged)
							{
								num++;
								component2.HasChanged = false;
								if (this.AutoMaterial)
								{
									component2.UpdateMaterials();
								}
								this.ApplyCorrectSky(renderer2);
							}
						}
					}
				}
				foreach (Renderer renderer3 in list2)
				{
					this.dynamicRenderers.Remove(renderer3);
				}
				if (this.renderCheckIterator >= this.dynamicRenderers.Count)
				{
					this.renderCheckIterator = 0;
				}
			}
			this._GlobalSky.Dirty = false;
		}

		public void ProbeSkies(GameObject[] objects, Sky[] skies, bool probeAll, bool probeIBL)
		{
			int num = 0;
			List<Sky> list = new List<Sky>();
			string text = string.Empty;
			if (skies != null)
			{
				foreach (Sky sky in skies)
				{
					if (sky)
					{
						if (probeAll || sky.IsProbe)
						{
							list.Add(sky);
						}
						else
						{
							num++;
							text = text + sky.name + "\n";
						}
					}
				}
			}
			if (objects != null)
			{
				foreach (GameObject gameObject in objects)
				{
					Sky component = gameObject.GetComponent<Sky>();
					if (component)
					{
						if (probeAll || component.IsProbe)
						{
							list.Add(component);
						}
						else
						{
							num++;
							text = text + component.name + "\n";
						}
					}
				}
			}
			if (num > 0)
			{
			}
			if (list.Count > 0)
			{
				this.ProbeExposures = ((!probeIBL) ? Vector4.zero : Vector4.one);
				this.SkiesToProbe = new Sky[list.Count];
				for (int k = 0; k < list.Count; k++)
				{
					this.SkiesToProbe[k] = list[k];
				}
			}
		}

		private static SkyManager _Instance;

		public bool LinearSpace = true;

		[SerializeField]
		private bool _BlendingSupport = true;

		[SerializeField]
		private bool _ProjectionSupport = true;

		public bool GameAutoApply = true;

		public bool EditorAutoApply = true;

		public bool AutoMaterial;

		public int IgnoredLayerMask;

		public int[] _IgnoredLayers;

		public int _IgnoredLayerCount;

		[SerializeField]
		private Sky _GlobalSky;

		[SerializeField]
		private SkyBlender GlobalBlender = new SkyBlender();

		private Sky nextSky;

		private float nextBlendTime;

		private float nextSkipTime;

		public float LocalBlendTime = 0.25f;

		public float GlobalBlendTime = 0.25f;

		private Light[] prevLights;

		private Light[] nextLights;

		private float[] prevIntensities;

		private float[] nextIntensities;

		private Material _SkyboxMaterial;

		[SerializeField]
		private bool _ShowSkybox = true;

		public Camera ProbeCamera;

		private HashSet<Renderer> staticRenderers = new HashSet<Renderer>();

		private HashSet<Renderer> dynamicRenderers = new HashSet<Renderer>();

		private HashSet<Renderer> globalSkyChildren = new HashSet<Renderer>();

		private HashSet<SkyApplicator> skyApplicators = new HashSet<SkyApplicator>();

		private float seekTimer;

		private float lastTimestamp = -1f;

		private int renderCheckIterator;

		private bool firstFrame = true;

		public Sky[] SkiesToProbe;

		public int ProbeExponent = 512;

		public Vector4 ProbeExposures = Vector4.one;

		public bool ProbeWithCubeRT = true;

		public bool ProbeOnlyStatic;
	}
}
