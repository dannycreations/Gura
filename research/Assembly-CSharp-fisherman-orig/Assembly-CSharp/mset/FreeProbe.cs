using System;
using System.Collections.Generic;
using UnityEngine;

namespace mset
{
	[RequireComponent(typeof(Camera))]
	public class FreeProbe : MonoBehaviour
	{
		private Cubemap targetCube
		{
			get
			{
				return this._targetCube;
			}
			set
			{
				this._targetCube = value;
				this.UpdateFaceTexture();
			}
		}

		private void UpdateFaceTexture()
		{
			if (this._targetCube == null)
			{
				return;
			}
			if (this.faceTexture == null || this.faceTexture.width != this._targetCube.width)
			{
				if (this.faceTexture)
				{
					Object.DestroyImmediate(this.faceTexture);
				}
				this.faceTexture = new Texture2D(this._targetCube.width, this._targetCube.width, 5, true, false);
				this.RT = RenderTexture.GetTemporary(this._targetCube.width, this._targetCube.width, 24, 2, 1);
				this.RT.isCubemap = false;
				this.RT.useMipMap = false;
				this.RT.autoGenerateMips = false;
				if (!this.RT.IsCreated() && !this.RT.Create())
				{
					Debug.LogWarning("Failed to create HDR RenderTexture, capturing in LDR mode.");
					RenderTexture.ReleaseTemporary(this.RT);
					this.RT = null;
				}
			}
		}

		private void FreeFaceTexture()
		{
			if (this.faceTexture)
			{
				Object.DestroyImmediate(this.faceTexture);
				this.faceTexture = null;
			}
			if (this.RT)
			{
				if (RenderTexture.active == this.RT)
				{
					RenderTexture.active = null;
				}
				RenderTexture.ReleaseTemporary(this.RT);
				this.RT = null;
			}
			this.probeQueue = null;
		}

		private void Start()
		{
			this.UpdateFaceTexture();
			this.convolveSkybox = new Material(Shader.Find("Hidden/Marmoset/RGBM Convolve"));
			this.convolveSkybox.name = "Internal Convolution Skybox";
		}

		private void Awake()
		{
			this.sceneSkybox = RenderSettings.skybox;
			SkyManager skyManager = SkyManager.Get();
			if (skyManager && skyManager.ProbeCamera)
			{
				base.GetComponent<Camera>().CopyFrom(skyManager.ProbeCamera);
			}
			else if (Camera.main)
			{
				base.GetComponent<Camera>().CopyFrom(Camera.main);
			}
		}

		public void QueueSkies(Sky[] skiesToProbe)
		{
			if (this.probeQueue == null)
			{
				this.probeQueue = new Queue<FreeProbe.ProbeTarget>();
			}
			else
			{
				this.probeQueue.Clear();
			}
			foreach (Sky sky in skiesToProbe)
			{
				if (sky != null && sky.SpecularCube as Cubemap != null)
				{
					this.QueueCubemap(sky.SpecularCube as Cubemap, sky.HDRSpec, sky.transform.position, sky.transform.rotation);
				}
			}
		}

		public void QueueCubemap(Cubemap cube, bool HDR, Vector3 pos, Quaternion rot)
		{
			if (cube != null)
			{
				FreeProbe.ProbeTarget probeTarget = new FreeProbe.ProbeTarget();
				probeTarget.cube = cube;
				probeTarget.position = pos;
				probeTarget.rotation = rot;
				probeTarget.HDR = HDR;
				this.probeQueue.Enqueue(probeTarget);
				this.progressTotal++;
			}
		}

		private void ClearQueue()
		{
			this.probeQueue = null;
			this.progressTotal = 0;
			this.progress = 0;
		}

		public void RunQueue()
		{
			this.probeQueue.Enqueue(null);
			SkyProbe.buildRandomValueTable();
			SkyManager skyManager = SkyManager.Get();
			if (skyManager.ProbeCamera)
			{
				base.GetComponent<Camera>().CopyFrom(skyManager.ProbeCamera);
				this.defaultCullMask = skyManager.ProbeCamera.cullingMask;
			}
			else if (Camera.main)
			{
				base.GetComponent<Camera>().CopyFrom(Camera.main);
				this.defaultCullMask = base.GetComponent<Camera>().cullingMask;
			}
			this.disabledCameras.Clear();
			foreach (Camera camera in Camera.allCameras)
			{
				if (camera.enabled)
				{
					camera.enabled = false;
					this.disabledCameras.Add(camera);
				}
			}
			base.GetComponent<Camera>().enabled = true;
			base.GetComponent<Camera>().fieldOfView = 90f;
			base.GetComponent<Camera>().clearFlags = 1;
			base.GetComponent<Camera>().cullingMask = this.defaultCullMask;
			base.GetComponent<Camera>().useOcclusionCulling = false;
			this.StartStage(FreeProbe.Stage.NEXTSKY);
		}

		private void StartStage(FreeProbe.Stage nextStage)
		{
			if (this.probeQueue == null)
			{
				nextStage = FreeProbe.Stage.DONE;
			}
			if (nextStage == FreeProbe.Stage.NEXTSKY)
			{
				RenderSettings.skybox = this.sceneSkybox;
				FreeProbe.ProbeTarget probeTarget = this.probeQueue.Dequeue();
				if (probeTarget != null)
				{
					this.progress++;
					if (this.ProgressCallback != null && this.progressTotal > 0)
					{
						this.ProgressCallback((float)this.progress / (float)this.progressTotal);
					}
					this.targetCube = probeTarget.cube;
					this.captureHDR = probeTarget.HDR && this.RT != null;
					this.lookPos = probeTarget.position;
					this.lookRot = probeTarget.rotation;
				}
				else
				{
					nextStage = FreeProbe.Stage.DONE;
				}
			}
			if (nextStage == FreeProbe.Stage.CAPTURE)
			{
				this.drawShot = -1;
				RenderSettings.skybox = this.sceneSkybox;
				this.targetMip = 0;
				this.captureSize = this.targetCube.width;
				this.mipCount = QPow.Log2i(this.captureSize) - 1;
				base.GetComponent<Camera>().cullingMask = this.defaultCullMask;
			}
			if (nextStage == FreeProbe.Stage.CONVOLVE)
			{
				Shader.SetGlobalVector("_UniformOcclusion", Vector4.one);
				this.drawShot = 0;
				this.targetMip = 1;
				if (this.targetMip < this.mipCount)
				{
					base.GetComponent<Camera>().cullingMask = 0;
					RenderSettings.skybox = this.convolveSkybox;
					Matrix4x4 identity = Matrix4x4.identity;
					this.convolveSkybox.SetMatrix("_SkyMatrix", identity);
					this.convolveSkybox.SetTexture("_CubeHDR", this.targetCube);
					FreeProbe.toggleKeywordPair("MARMO_RGBM_INPUT_ON", "MARMO_RGBM_INPUT_OFF", this.captureHDR && this.RT != null);
					FreeProbe.toggleKeywordPair("MARMO_RGBM_OUTPUT_ON", "MARMO_RGBM_OUTPUT_OFF", this.captureHDR && this.RT != null);
					SkyProbe.bindRandomValueTable(this.convolveSkybox, "_PhongRands", this.targetCube.width);
				}
			}
			if (nextStage == FreeProbe.Stage.DONE)
			{
				RenderSettings.skybox = this.sceneSkybox;
				this.ClearQueue();
				this.FreeFaceTexture();
				foreach (Camera camera in this.disabledCameras)
				{
					camera.enabled = true;
				}
				this.disabledCameras.Clear();
				if (this.DoneCallback != null)
				{
					this.DoneCallback();
					this.DoneCallback = null;
				}
			}
			this.stage = nextStage;
		}

		private void OnPreCull()
		{
			if (this.stage == FreeProbe.Stage.CAPTURE || this.stage == FreeProbe.Stage.CONVOLVE || this.stage == FreeProbe.Stage.PRECAPTURE)
			{
				if (this.stage == FreeProbe.Stage.CONVOLVE)
				{
					this.captureSize = 1 << this.mipCount - this.targetMip;
					float num = (float)QPow.clampedDownShift(this.maxExponent, this.targetMip - 1, 1);
					this.convolveSkybox.SetFloat("_SpecularExp", num);
					this.convolveSkybox.SetFloat("_SpecularScale", this.convolutionScale);
				}
				if (this.stage == FreeProbe.Stage.CAPTURE || this.stage == FreeProbe.Stage.PRECAPTURE)
				{
					Shader.SetGlobalVector("_UniformOcclusion", this.exposures);
				}
				int num2 = this.captureSize;
				float num3 = (float)num2 / (float)Screen.width;
				float num4 = (float)num2 / (float)Screen.height;
				base.GetComponent<Camera>().rect = new Rect(0f, 0f, num3, num4);
				base.GetComponent<Camera>().pixelRect = new Rect(0f, 0f, (float)num2, (float)num2);
				base.transform.position = this.lookPos;
				base.transform.rotation = this.lookRot;
				if (this.stage == FreeProbe.Stage.CAPTURE || this.stage == FreeProbe.Stage.PRECAPTURE)
				{
					this.upLook = base.transform.up;
					this.forwardLook = base.transform.forward;
					this.rightLook = base.transform.right;
				}
				else
				{
					this.upLook = Vector3.up;
					this.forwardLook = Vector3.forward;
					this.rightLook = Vector3.right;
				}
				if (this.drawShot == 0)
				{
					base.transform.LookAt(this.lookPos + this.forwardLook, this.upLook);
				}
				else if (this.drawShot == 1)
				{
					base.transform.LookAt(this.lookPos - this.forwardLook, this.upLook);
				}
				else if (this.drawShot == 2)
				{
					base.transform.LookAt(this.lookPos - this.rightLook, this.upLook);
				}
				else if (this.drawShot == 3)
				{
					base.transform.LookAt(this.lookPos + this.rightLook, this.upLook);
				}
				else if (this.drawShot == 4)
				{
					base.transform.LookAt(this.lookPos + this.upLook, this.forwardLook);
				}
				else if (this.drawShot == 5)
				{
					base.transform.LookAt(this.lookPos - this.upLook, -this.forwardLook);
				}
				base.GetComponent<Camera>().ResetWorldToCameraMatrix();
			}
		}

		private void Update()
		{
			this.frameID++;
			if (this.RT && this.captureHDR && this.stage == FreeProbe.Stage.CAPTURE)
			{
				this.stage = FreeProbe.Stage.PRECAPTURE;
				bool allowHDR = base.GetComponent<Camera>().allowHDR;
				base.GetComponent<Camera>().allowHDR = true;
				RenderTexture.active = RenderTexture.active;
				RenderTexture.active = this.RT;
				base.GetComponent<Camera>().targetTexture = this.RT;
				base.GetComponent<Camera>().Render();
				base.GetComponent<Camera>().allowHDR = allowHDR;
				base.GetComponent<Camera>().targetTexture = null;
				RenderTexture.active = null;
				this.stage = FreeProbe.Stage.CAPTURE;
			}
		}

		private void OnPostRender()
		{
			if (this.captureHDR && this.RT && this.stage == FreeProbe.Stage.CAPTURE)
			{
				int width = this.RT.width;
				int num = 0;
				int num2 = 0;
				if (!this.blitMat)
				{
					this.blitMat = new Material(Shader.Find("Hidden/Marmoset/RGBM Blit"));
				}
				FreeProbe.toggleKeywordPair("MARMO_RGBM_INPUT_ON", "MARMO_RGBM_INPUT_OFF", false);
				FreeProbe.toggleKeywordPair("MARMO_RGBM_OUTPUT_ON", "MARMO_RGBM_OUTPUT_OFF", true);
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, (float)width, (float)width, 0f);
				Graphics.DrawTexture(new Rect((float)num, (float)num2, (float)width, (float)width), this.RT, this.blitMat);
				GL.PopMatrix();
			}
			if (this.stage != FreeProbe.Stage.NEXTSKY)
			{
				if (this.stage == FreeProbe.Stage.CAPTURE || this.stage == FreeProbe.Stage.CONVOLVE)
				{
					int num3 = this.captureSize;
					bool flag = !this.captureHDR;
					if (num3 > Screen.width || num3 > Screen.height)
					{
						Debug.LogWarning(string.Concat(new object[]
						{
							"<b>Skipping Cubemap</b> - The viewport is too small (",
							Screen.width,
							"x",
							Screen.height,
							") to probe the cubemap \"",
							this.targetCube.name,
							"\" (",
							num3,
							"x",
							num3,
							")"
						}));
						this.StartStage(FreeProbe.Stage.NEXTSKY);
						return;
					}
					if (this.drawShot == 0)
					{
						this.faceTexture.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num3), 0, 0);
						this.faceTexture.Apply();
						FreeProbe.SetFacePixels(this.targetCube, 4, this.faceTexture, this.targetMip, false, true, flag);
					}
					else if (this.drawShot == 1)
					{
						this.faceTexture.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num3), 0, 0);
						this.faceTexture.Apply();
						FreeProbe.SetFacePixels(this.targetCube, 5, this.faceTexture, this.targetMip, false, true, flag);
					}
					else if (this.drawShot == 2)
					{
						this.faceTexture.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num3), 0, 0);
						this.faceTexture.Apply();
						FreeProbe.SetFacePixels(this.targetCube, 1, this.faceTexture, this.targetMip, false, true, flag);
					}
					else if (this.drawShot == 3)
					{
						this.faceTexture.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num3), 0, 0);
						this.faceTexture.Apply();
						FreeProbe.SetFacePixels(this.targetCube, 0, this.faceTexture, this.targetMip, false, true, flag);
					}
					else if (this.drawShot == 4)
					{
						this.faceTexture.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num3), 0, 0);
						this.faceTexture.Apply();
						FreeProbe.SetFacePixels(this.targetCube, 2, this.faceTexture, this.targetMip, true, false, flag);
					}
					else if (this.drawShot == 5)
					{
						this.faceTexture.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num3), 0, 0);
						this.faceTexture.Apply();
						FreeProbe.SetFacePixels(this.targetCube, 3, this.faceTexture, this.targetMip, true, false, flag);
						if (this.stage == FreeProbe.Stage.CAPTURE)
						{
							this.targetCube.Apply(true, false);
							this.StartStage(FreeProbe.Stage.CONVOLVE);
							return;
						}
						this.targetCube.Apply(false, false);
						this.targetMip++;
						if (this.targetMip < this.mipCount)
						{
							this.drawShot = 0;
							return;
						}
						this.StartStage(FreeProbe.Stage.NEXTSKY);
						return;
					}
					this.drawShot++;
				}
				return;
			}
			if (this.targetCube != null)
			{
				this.StartStage(FreeProbe.Stage.CAPTURE);
				return;
			}
			this.StartStage(FreeProbe.Stage.DONE);
		}

		private static void SetFacePixels(Cubemap cube, CubemapFace face, Texture2D tex, int mip, bool flipHorz, bool flipVert, bool convertHDR)
		{
			Color[] pixels = tex.GetPixels();
			Color color = Color.black;
			int num = tex.width >> mip;
			int num2 = tex.height >> mip;
			Color[] array = new Color[num * num2];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num3 = i + j * tex.width;
					int num4 = i + j * num;
					array[num4] = pixels[num3];
					if (convertHDR)
					{
						array[num4].a = 0.16666667f;
					}
				}
			}
			if (flipHorz)
			{
				for (int k = 0; k < num / 2; k++)
				{
					for (int l = 0; l < num2; l++)
					{
						int num5 = num - k - 1;
						int num6 = k + l * num;
						int num7 = num5 + l * num;
						color = array[num7];
						array[num7] = array[num6];
						array[num6] = color;
					}
				}
			}
			if (flipVert)
			{
				for (int m = 0; m < num; m++)
				{
					for (int n = 0; n < num2 / 2; n++)
					{
						int num8 = num2 - n - 1;
						int num9 = m + n * num;
						int num10 = m + num8 * num;
						color = array[num10];
						array[num10] = array[num9];
						array[num9] = color;
					}
				}
			}
			cube.SetPixels(array, face, mip);
		}

		private static void toggleKeywordPair(string on, string off, bool yes)
		{
			if (yes)
			{
				Shader.EnableKeyword(on);
				Shader.DisableKeyword(off);
			}
			else
			{
				Shader.EnableKeyword(off);
				Shader.DisableKeyword(on);
			}
		}

		private static void toggleKeywordPair(Material mat, string on, string off, bool yes)
		{
			if (yes)
			{
				mat.EnableKeyword(on);
				mat.DisableKeyword(off);
			}
			else
			{
				mat.EnableKeyword(off);
				mat.DisableKeyword(on);
			}
		}

		private RenderTexture RT;

		public Action<float> ProgressCallback;

		public Action DoneCallback;

		public bool linear = true;

		public int maxExponent = 512;

		public Vector4 exposures = Vector4.one;

		public float convolutionScale = 1f;

		private List<Camera> disabledCameras = new List<Camera>();

		private Cubemap _targetCube;

		private Texture2D faceTexture;

		private FreeProbe.Stage stage = FreeProbe.Stage.DONE;

		private int drawShot;

		private int targetMip;

		private int mipCount;

		private int captureSize;

		private bool captureHDR = true;

		private int progress;

		private int progressTotal;

		private Vector3 lookPos = Vector3.zero;

		private Quaternion lookRot = Quaternion.identity;

		private Vector3 forwardLook = Vector3.forward;

		private Vector3 rightLook = Vector3.right;

		private Vector3 upLook = Vector3.up;

		private Queue<FreeProbe.ProbeTarget> probeQueue;

		private int defaultCullMask = -1;

		private Material sceneSkybox;

		private Material convolveSkybox;

		private int frameID;

		private Material blitMat;

		private enum Stage
		{
			NEXTSKY,
			PRECAPTURE,
			CAPTURE,
			CONVOLVE,
			DONE
		}

		private class ProbeTarget
		{
			public Cubemap cube;

			public bool HDR;

			public Vector3 position = Vector3.zero;

			public Quaternion rotation = Quaternion.identity;
		}
	}
}
