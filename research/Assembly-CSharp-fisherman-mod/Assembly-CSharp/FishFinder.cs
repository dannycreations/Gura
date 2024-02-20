using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BiteEditor;
using BiteEditor.ObjectModel;
using Boats;
using DG.Tweening;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FishFinder : MonoBehaviour
{
	public void SetInstanceId(string id)
	{
		this.instanceId = id;
	}

	public void SetMaxSpeed(float speed)
	{
		this.MaxSpeed = speed;
	}

	public void InitRenderTextures()
	{
		if (this.BoatController != null)
		{
			if (!FishFinder.renderTextures.ContainsKey(this.BoatController))
			{
				FishFinder.renderTextures.Add(this.BoatController, new RenderTexture[2]);
				for (int i = 0; i < 2; i++)
				{
					if (FishFinder.renderTextures[this.BoatController][i] == null)
					{
						FishFinder.renderTextures[this.BoatController][i] = new RenderTexture(256, 512, 0, 0, 2);
						FishFinder.renderTextures[this.BoatController][i].useMipMap = false;
						FishFinder.renderTextures[this.BoatController][i].filterMode = 2;
					}
				}
			}
			if (this.image != null)
			{
				this.image.texture = FishFinder.renderTextures[this.BoatController][1];
			}
		}
	}

	private void InitMaterialValues()
	{
		if (!FishFinder.fishFinderMaterials.ContainsKey(this.BoatController))
		{
			FishFinder.fishFinderMaterials.Add(this.BoatController, new Material(Shader.Find("Hidden/FishFinder")));
		}
		FishFinder.fishFinderMaterials[this.BoatController].SetColor("_ClearColor", this.WaterColor);
		FishFinder.fishFinderMaterials[this.BoatController].SetColor("_SurfaceColor", this.SurfaceLineColor);
		FishFinder.fishFinderMaterials[this.BoatController].SetColor("_CutoffColor", this.SubsurfaceCutoffColor);
		FishFinder.fishFinderMaterials[this.BoatController].SetColor("_GrassColor", this.GrassColor);
		FishFinder.fishFinderMaterials[this.BoatController].SetColor("_LimboColor", this.LimboColor);
		FishFinder.fishFinderMaterials[this.BoatController].SetColor("_AbyssColor", this.AbyssColor);
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_OneFrameOffset", this.FrameOffsetPercentage);
		FishFinder.MainFishColor = this.FishColor;
		FishFinder.MainInterfaceColor = this.InterfaceColor;
	}

	public void OnEnable()
	{
		this.MaxHistory = (int)(1f / this.FrameOffsetPercentage);
		if (this.isMain && this.BoatController != null && !FishFinder.fishFinderMaterials.ContainsKey(this.BoatController))
		{
			this.InitMaterialValues();
		}
		this.InitRenderTextures();
		if (!this.subscribed && GameFactory.Player != null)
		{
			GameFactory.Player.OnBoarded += this.OnBoarded;
			this.subscribed = true;
		}
	}

	public void OnDestroy()
	{
		if (this.isMain && this.isOn)
		{
			this.SetTurnedOn(false);
		}
		if (this.subscribed && GameFactory.Player != null)
		{
			GameFactory.Player.OnBoarded -= this.OnBoarded;
			this.subscribed = false;
		}
	}

	private void OffsetFishes(float prevFrameScale)
	{
		for (int i = 0; i < this.FishesToMove.Count; i++)
		{
			FishFinderFish fishFinderFish = this.FishesToMove[i];
			fishFinderFish.RectTransform.localScale = new Vector3(fishFinderFish.RectTransform.localScale.x, fishFinderFish.RectTransform.localScale.y * prevFrameScale, fishFinderFish.RectTransform.localScale.z);
			fishFinderFish.RectTransform.anchoredPosition = new Vector2(fishFinderFish.RectTransform.anchoredPosition.x - Mathf.Floor(this.FishesParent.rect.width * this.FrameOffsetPercentage), this.FishesParent.rect.height * (1f - (1f - fishFinderFish.RectTransform.anchoredPosition.y / this.FishesParent.rect.height) * prevFrameScale));
			if (fishFinderFish.RectTransform.anchoredPosition.x <= -fishFinderFish.RectTransform.rect.width)
			{
				fishFinderFish.RectTransform.localScale = Vector3.zero;
				this.FreeFishes.Add(fishFinderFish);
				this.FishesToMove.RemoveAt(i);
			}
		}
	}

	private void SpawnNextFish()
	{
		if (!FishFinder.fishesToSpawn.ContainsKey(this.BoatController))
		{
			return;
		}
		if (this.isMain)
		{
			List<FishFinder.FishToSpawn> list = FishFinder.fishesToSpawn[this.BoatController].Where((FishFinder.FishToSpawn x) => x.timeToSpawn <= this.playTime && x.spawnedByInstances.Contains(this.instanceId)).ToList<FishFinder.FishToSpawn>();
			foreach (FishFinder.FishToSpawn fishToSpawn in list)
			{
				FishFinder.fishesToSpawn[this.BoatController].Remove(fishToSpawn);
			}
		}
		FishFinder.FishToSpawn fishToSpawn2 = FishFinder.fishesToSpawn[this.BoatController].FirstOrDefault((FishFinder.FishToSpawn x) => x.timeToSpawn <= this.playTime && !x.spawnedByInstances.Contains(this.instanceId));
		if (fishToSpawn2 == null)
		{
			return;
		}
		bool flag = this.FreeFishes.Count > 0;
		if (!this.tooFast)
		{
			fishToSpawn2.spawnedByInstances.Add(this.instanceId);
			FishFinderFish fishFinderFish = ((!flag) ? Object.Instantiate<FishFinderFish>(this.FishPrefab, this.FishesParent) : this.FreeFishes[0]);
			if (flag)
			{
				this.FreeFishes.RemoveAt(0);
			}
			float num = ((FishFinder.history.Count <= 0) ? FishFinder.depth : FishFinder.history.Last<FishFinder.ColumnData>().Depth);
			float num2;
			if (FishFinder.history.Count > 0)
			{
				num2 = CollectionUtilities.Max(FishFinder.history.Select((FishFinder.ColumnData x) => x.Depth).ToList<float>());
			}
			else
			{
				num2 = Mathf.Abs(FishFinder.depth);
			}
			float num3 = num2;
			num3 += 2f;
			float num4 = Mathf.Abs(HeightMap.GetBottomDepth(num));
			float num5 = Mathf.Abs(HeightMap.GetTopDepth(num));
			float num6 = num3 - Mathf.Abs(num) - num4 - num5;
			float num7 = 0f;
			float num8 = this.minScale + (this.maxScale - this.minScale) * Mathf.Clamp01(fishToSpawn2.weight / 5f);
			switch (fishToSpawn2.depthType)
			{
			case Depth.Bottom:
				num7 = this.FishesParent.rect.height - this.FishesParent.rect.height * Mathf.Abs(FishFinder.depth) / num3 + this.FishPrefab.RectTransform.rect.height * num8 + this.FishesParent.rect.height * (num4 / num3) * Random.value;
				break;
			case Depth.All:
			case Depth.Top:
			case Depth.Invalid:
				num7 = this.FishesParent.rect.height * (1f - num5 * Random.value / num3);
				break;
			case Depth.Middle:
				num7 = this.FishesParent.rect.height * (num3 - Mathf.Abs(FishFinder.depth) + num4 + num6 * Random.value) / num3;
				break;
			}
			fishFinderFish.RectTransform.localScale = Vector3.one * num8;
			fishFinderFish.RectTransform.anchoredPosition = new Vector2(this.FishesParent.rect.width, num7);
			fishFinderFish.Image.sprite = ((this.velocity <= 3f || Random.value <= 0.5f) ? this.normalFishSprites[Random.Range(0, this.normalFishSprites.Length)] : this.cuttedFishSprites[Random.Range(0, this.cuttedFishSprites.Length)]);
			fishFinderFish.Image.color = FishFinder.MainFishColor;
			this.FishesToMove.Add(fishFinderFish);
			this.Source.PlayOneShot(this.FishSpawned, SettingsManager.EnvironmentForcedVolume);
		}
	}

	private void UpdateCanvas(float depthMax)
	{
		float num = depthMax / (float)(this.Depths.Length - 1);
		float num2 = 0f;
		bool flag = (float)this.Depths.Length > Mathf.Floor(MeasuringSystemManager.LineLength(depthMax));
		if (this.tooFast && this.FishesParent.localScale.x > 0f)
		{
			this.FishesParent.localScale = Vector3.zero;
		}
		else if (!this.tooFast && this.FishesParent.localScale.x < 0.5f)
		{
			this.FishesParent.localScale = Vector3.one;
		}
		for (int i = 0; i < this.Depths.Length; i++)
		{
			if (flag && i % 2 == 1 && this.Depths[i].gameObject.activeInHierarchy)
			{
				this.Depths[i].gameObject.SetActive(false);
				num2 += num;
			}
			else
			{
				if (!flag && !this.Depths[i].gameObject.activeInHierarchy)
				{
					this.Depths[i].gameObject.SetActive(true);
				}
				this.Depths[i].text = Mathf.Round(MeasuringSystemManager.LineLength(num2)).ToString();
				num2 += num;
			}
		}
	}

	private void SaveHistoryAndApplyMaterial(float vScale, float depthMax)
	{
		if (FishFinder.history.Count == this.MaxHistory)
		{
			FishFinder.history.RemoveAt(0);
		}
		FishFinder.history.Add(new FishFinder.ColumnData
		{
			Depth = Mathf.Abs(this.depthSmooth),
			CutoffDisplacement = this.currentCD,
			DepthMax = depthMax,
			GrassDisplacement = this.currentGD,
			GrassFrequency = this.currentGF
		});
		if (this.tooFast && Random.value > 0.7f)
		{
			this.depthSmooth = depthMax;
			this.currentST = 0f;
			this.currentGF = 0f;
			this.currentLT = 0f;
			this.currentCD = 0f;
		}
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_VerticalScale", vScale);
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_Depth", Mathf.Abs(this.depthSmooth));
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_DepthMax", Mathf.Abs(depthMax));
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_SurfaceDisplacement", this.currentST);
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_CutoffDisplacement", this.currentCD);
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_GrassDisplacement", this.currentGD);
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_LimboDisplacement", this.currentLT);
		FishFinder.fishFinderMaterials[this.BoatController].SetFloat("_GrassFrequency", this.currentGF);
		FishFinder.renderTextures[this.BoatController][0].filterMode = 0;
		FishFinder.renderTextures[this.BoatController][1].filterMode = 0;
		Graphics.Blit(FishFinder.renderTextures[this.BoatController][0], FishFinder.renderTextures[this.BoatController][1], FishFinder.fishFinderMaterials[this.BoatController]);
		RTUtility.Swap(FishFinder.renderTextures[this.BoatController]);
		FishFinder.renderTextures[this.BoatController][0].filterMode = 1;
		FishFinder.renderTextures[this.BoatController][1].filterMode = 1;
	}

	private void FixedUpdate()
	{
		if (GameFactory.Player != null)
		{
			this.velocity = GameFactory.Player.BoatVelocity;
		}
		if (!PondControllers.Instance.IsInMenu && !BackToLobbyClick.IsLeaving && !TransferToLocation.IsMoving)
		{
			this.tooFast = this.velocity > this.MaxSpeed * 10f / 36f;
			if (this.isOn && !this.tooFast && GameFactory.Player != null && TimeAndWeatherManager.CurrentWeather != null && (GameFactory.Player.IsSailing || GameFactory.Player.IsBoatFishing))
			{
				this.DepthText.text = ((int)MeasuringSystemManager.LineLength(Mathf.Abs(FishFinder.depth))).ToString(CultureInfo.InvariantCulture) + string.Empty + MeasuringSystemManager.LineLengthSufix();
				this.TemperatureText.text = ((int)MeasuringSystemManager.Temperature((float)TimeAndWeatherManager.CurrentWeather.WaterTemperature)).ToString(CultureInfo.InvariantCulture) + string.Empty + MeasuringSystemManager.TemperatureSufix();
				this.VelocityText.text = ((int)MeasuringSystemManager.Speed(this.velocity)).ToString(CultureInfo.InvariantCulture) + string.Empty + MeasuringSystemManager.SpeedSufix();
			}
			else
			{
				this.DepthText.text = "-- " + MeasuringSystemManager.LineLengthSufix();
				this.TemperatureText.text = "-- " + MeasuringSystemManager.TemperatureSufix();
				this.VelocityText.text = "-- " + MeasuringSystemManager.SpeedSufix();
			}
			this.playTime += Time.fixedDeltaTime;
			this.updateTime += Time.fixedDeltaTime;
			this.sounderTime += Time.fixedDeltaTime;
			if (this.isMain)
			{
				this.UpdateShaderValues();
			}
			if (this.updateTime >= 1f / this.UpdateFPS)
			{
				this.updateTime = 0f;
				float num;
				if (FishFinder.history.Count == 0 || this.firstTimeOn)
				{
					num = Mathf.Abs(this.depthSmooth);
				}
				else
				{
					num = CollectionUtilities.Max(FishFinder.history.Select((FishFinder.ColumnData x) => x.Depth).ToList<float>());
				}
				float num2 = num;
				num2 += 2f;
				float num3 = ((FishFinder.history.Count != 0 && !this.firstTimeOn) ? (FishFinder.history[FishFinder.history.Count - 1].DepthMax / num2) : 1f);
				this.OffsetFishes(num3);
				if (this.isOn)
				{
					this.UpdateCanvas(num2);
					if (this.isMain)
					{
						if (this.firstTimeOn)
						{
							this.firstTimeOn = false;
							for (int i = 0; i < 2 * this.MaxHistory; i++)
							{
								this.SaveHistoryAndApplyMaterial(num3, num2);
							}
						}
						else
						{
							this.SaveHistoryAndApplyMaterial(num3, num2);
						}
					}
					else if (this.firstTimeOn)
					{
						this.firstTimeOn = false;
					}
				}
				this.SpawnNextFish();
			}
			if (this.isOn && this.isMain && this.sounderTime >= this.GetFishInterval && this.type == EchoSounderKind.DepthAndFish)
			{
				this.sounderTime = 0f;
				Vector3 position = this.BoatController.Position;
				PhotonConnectionFactory.Instance.GetSounderFish(new Point3(position.x, position.y, position.z));
			}
		}
	}

	private void OnGotFishes(float[] fishData)
	{
		if (fishData == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < fishData.Length; i += 2)
		{
			float num2 = fishData[i];
			if (num2 > 0f)
			{
				float num3 = (float)this.MinFishesToSpawn + (float)(this.MaxFishesToSpawn - this.MinFishesToSpawn) * num2;
				int num4 = 0;
				while ((float)num4 < num3)
				{
					float value = Random.value;
					float num5 = this.playTime + value * this.GetFishInterval;
					Depth depth = ((i != 0) ? ((i != 2) ? Depth.Bottom : Depth.Middle) : Depth.Top);
					if (!FishFinder.fishesToSpawn.ContainsKey(this.BoatController))
					{
						FishFinder.fishesToSpawn.Add(this.BoatController, new List<FishFinder.FishToSpawn>());
					}
					FishFinder.fishesToSpawn[this.BoatController].Add(new FishFinder.FishToSpawn
					{
						depthType = depth,
						timeToSpawn = num5,
						weight = fishData[i + 1]
					});
					num++;
					num4++;
				}
			}
		}
		if (num > 0)
		{
			LogHelper.Log(string.Format("[{0}] Will spawn : {1}, id: {2}", this.playTime, num, this.instanceId));
		}
	}

	public void SetBoatController(IBoatController bc)
	{
		this.BoatController = bc;
		bc.FishingModeSwitched += this.OnFishingModeSwitched;
	}

	private void OnFishingModeSwitched(bool isFishing)
	{
		if (isFishing && !this.isMain)
		{
			base.StartCoroutine(this.MoveTowardsEyes());
		}
		if (!isFishing)
		{
			this.OnBoarded(true);
		}
	}

	private void OnGotFishesFailed(Failure fail)
	{
		Debug.LogError(fail.ErrorMessage);
		this.sounderTime = this.GetFishInterval;
	}

	public void SetType(EchoSounderKind kind, bool isMain = false)
	{
		this.type = kind;
		this.isMain = isMain;
		this.InitMaterialValues();
	}

	private void SetTurnedOn(bool on)
	{
		bool flag = this.isOn != on;
		this.isOn = on;
		this.FishesParent.localScale = ((!this.isOn) ? Vector3.zero : Vector3.one);
		ShortcutExtensions.DOFade(this.turnedOffImage, (float)((!this.isOn) ? 1 : 0), 0.15f);
		if (!on)
		{
			this.firstTimeOn = true;
		}
		if (this.isMain)
		{
			FishFinder.fishFinderMaterials[this.BoatController].SetInt("_TurnedOn", (!this.isOn) ? 0 : 1);
		}
		if (on)
		{
			foreach (Graphic graphic in this.InterfaceElements)
			{
				if (graphic.color != FishFinder.MainInterfaceColor)
				{
					graphic.color = FishFinder.MainInterfaceColor;
				}
			}
		}
		if (this.isMain && this.type == EchoSounderKind.DepthAndFish)
		{
			if (this.isOn)
			{
				PhotonConnectionFactory.Instance.OnGotSounderFish += this.OnGotFishes;
				PhotonConnectionFactory.Instance.OnGettingSounderFishFailed += this.OnGotFishesFailed;
			}
			else
			{
				FishFinder.history.Clear();
				PhotonConnectionFactory.Instance.OnGotSounderFish -= this.OnGotFishes;
				PhotonConnectionFactory.Instance.OnGettingSounderFishFailed -= this.OnGotFishesFailed;
			}
		}
		if (flag)
		{
			this.Source.PlayOneShot((!this.isOn) ? this.TurnOff : this.TurnOn, SettingsManager.EnvironmentForcedVolume);
		}
	}

	private IEnumerator MoveTowardsEyes()
	{
		yield return new WaitForSeconds(this.TurningDelay);
		while (PondControllers.Instance.IsInMenu)
		{
			yield return null;
		}
		Transform cam = Camera.main.transform;
		Vector3 dir = base.transform.position - cam.position;
		Vector3 rotation = Quaternion.LookRotation(base.transform.InverseTransformDirection(dir.normalized), base.transform.InverseTransformDirection(Vector3.up)).eulerAngles;
		if (this.HolderYRotation != null)
		{
			ShortcutExtensions.DOLocalRotate(this.HolderYRotation, Vector3.up * rotation.y, this.TurningTime, 0);
		}
		if (this.BodyXRotation != null)
		{
			ShortcutExtensions.DOLocalRotate(this.BodyXRotation, Vector3.right * rotation.x, this.TurningTime, 0);
		}
		if (this.ScreenZRotation != null)
		{
			ShortcutExtensions.DOLocalRotate(this.ScreenZRotation, Vector3.forward * rotation.z, this.TurningTime, 0);
		}
		if (!this.isOn && this.isMain)
		{
			this.InitMaterialValues();
		}
		yield return new WaitForSeconds(this.TurningTime);
		if (!this.isOn)
		{
			this.SetTurnedOn(true);
		}
		yield break;
	}

	public void OnBoarded(bool boarded)
	{
		if (boarded && Vector3.Distance(GameFactory.Player.Position, base.transform.position) > 5f)
		{
			return;
		}
		base.StopAllCoroutines();
		if (this.HolderYRotation != null)
		{
			ShortcutExtensions.DOKill(this.HolderYRotation, false);
		}
		if (this.BodyXRotation != null)
		{
			ShortcutExtensions.DOKill(this.BodyXRotation, false);
		}
		if (this.ScreenZRotation != null)
		{
			ShortcutExtensions.DOKill(this.ScreenZRotation, false);
		}
		if (this.turnedOffImage != null)
		{
			ShortcutExtensions.DOKill(this.turnedOffImage, false);
		}
		if (boarded && this.isMain)
		{
			base.StartCoroutine(this.MoveTowardsEyes());
		}
		else
		{
			this.SetTurnedOn(false);
			if (this.HolderYRotation != null)
			{
				ShortcutExtensions.DOLocalRotate(this.HolderYRotation, Vector3.zero, this.TurningTime, 0);
			}
			if (this.BodyXRotation != null)
			{
				ShortcutExtensions.DOLocalRotate(this.BodyXRotation, Vector3.zero, this.TurningTime, 0);
			}
			if (this.ScreenZRotation != null)
			{
				ShortcutExtensions.DOLocalRotate(this.ScreenZRotation, Vector3.zero, this.TurningTime, 0);
			}
		}
	}

	private void UpdateShaderValues()
	{
		if (Init3D.SceneSettings == null)
		{
			return;
		}
		Vector3f zero = Vector3f.Zero;
		if (this.BoatController != null)
		{
			zero = new Vector3f(this.BoatController.Position);
			FishFinder.depth = Init3D.SceneSettings.HeightMap.GetBottomHeight(zero) - zero.y;
			if (this.depthSmooth == 0f)
			{
				this.depthSmooth = FishFinder.depth;
			}
		}
		if (!this.isOn)
		{
			Graphics.Blit(FishFinder.renderTextures[this.BoatController][0], FishFinder.renderTextures[this.BoatController][1], FishFinder.fishFinderMaterials[this.BoatController]);
			RTUtility.Swap(FishFinder.renderTextures[this.BoatController]);
			return;
		}
		SplatMap.LayerName name = Init3D.SceneSettings.SplatMap.GetLayer(zero).Name;
		switch (name + 1)
		{
		case SplatMap.LayerName.Sand:
			this.destinationLimboThicknessScatter = 1f;
			this.destinationLimboThicknessMin = 2.5f;
			this.destinationWidthThicknessScatter = 0.4f;
			this.destinationGrassThicknessMin = 0f;
			this.destinationSurfaceThicknessScatter = 0.05f;
			this.destinationSurfaceThicknessMin = 0.15f;
			this.destinationSurfaceDeviation = 0.1f;
			this.destinationGrassThicknessScatter = 0.1f;
			this.destinationGrassDensity = 0f;
			this.destinationWidthThicknessMin = 2f;
			break;
		case SplatMap.LayerName.Silt:
			this.destinationLimboThicknessScatter = 1f;
			this.destinationLimboThicknessMin = 2.5f;
			this.destinationWidthThicknessScatter = 0.4f;
			this.destinationGrassThicknessMin = 0f;
			this.destinationSurfaceThicknessScatter = 0.05f;
			this.destinationSurfaceThicknessMin = 0.15f;
			this.destinationSurfaceDeviation = 0.1f;
			this.destinationGrassThicknessScatter = 0f;
			this.destinationGrassDensity = 0f;
			this.destinationWidthThicknessMin = 2f;
			break;
		case SplatMap.LayerName.Gravel:
			this.destinationLimboThicknessScatter = 1.5f;
			this.destinationLimboThicknessMin = 4f;
			this.destinationWidthThicknessScatter = 0.5f;
			this.destinationGrassThicknessMin = 0f;
			this.destinationSurfaceThicknessScatter = 0.1f;
			this.destinationSurfaceThicknessMin = 0.2f;
			this.destinationSurfaceDeviation = 0.1f;
			this.destinationGrassThicknessScatter = 0.15f;
			this.destinationGrassDensity = 0.3f;
			this.destinationWidthThicknessMin = 3f;
			break;
		case SplatMap.LayerName.Stone:
			this.destinationLimboThicknessScatter = 0.75f;
			this.destinationLimboThicknessMin = 2f;
			this.destinationWidthThicknessScatter = 0.3f;
			this.destinationGrassThicknessMin = 0f;
			this.destinationSurfaceThicknessScatter = 0.05f;
			this.destinationSurfaceThicknessMin = 0.15f;
			this.destinationSurfaceDeviation = 0.15f;
			this.destinationGrassThicknessScatter = 0f;
			this.destinationGrassDensity = 0f;
			this.destinationWidthThicknessMin = 1.5f;
			break;
		case SplatMap.LayerName.Grass:
			this.destinationLimboThicknessScatter = 0.5f;
			this.destinationLimboThicknessMin = 1.5f;
			this.destinationWidthThicknessScatter = 0.2f;
			this.destinationGrassThicknessMin = 0f;
			this.destinationSurfaceThicknessScatter = 0.05f;
			this.destinationSurfaceThicknessMin = 0.1f;
			this.destinationSurfaceDeviation = 0.3f;
			this.destinationGrassThicknessScatter = 0f;
			this.destinationGrassDensity = 0f;
			this.destinationWidthThicknessMin = 1f;
			break;
		case SplatMap.LayerName.Shell:
			this.destinationLimboThicknessScatter = 1f;
			this.destinationLimboThicknessMin = 2.5f;
			this.destinationWidthThicknessScatter = 0.4f;
			this.destinationGrassThicknessMin = 0.5f;
			this.destinationSurfaceThicknessScatter = 0.15f;
			this.destinationSurfaceThicknessMin = 0.3f;
			this.destinationSurfaceDeviation = 0.1f;
			this.destinationGrassThicknessScatter = 5f;
			this.destinationGrassDensity = 0.5f;
			this.destinationWidthThicknessMin = 2f;
			break;
		case (SplatMap.LayerName)6:
			this.destinationLimboThicknessScatter = 0.75f;
			this.destinationLimboThicknessMin = 2f;
			this.destinationWidthThicknessScatter = 0.3f;
			this.destinationGrassThicknessMin = 0f;
			this.destinationSurfaceThicknessScatter = 0.05f;
			this.destinationSurfaceThicknessMin = 0.15f;
			this.destinationSurfaceDeviation = 0.2f;
			this.destinationGrassThicknessScatter = 0f;
			this.destinationGrassDensity = 0f;
			this.destinationWidthThicknessMin = 1.5f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		float value = Random.value;
		if (value < 0.5f)
		{
			this.currentSD += Mathf.Sign(this.currentSD) * -0.05f;
		}
		else if ((double)value > 0.8)
		{
			this.currentSD += Mathf.Sign(this.currentSD) * 0.05f;
		}
		this.currentSD = Mathf.Clamp(this.currentSD, -this.destinationSurfaceDeviation, this.destinationSurfaceDeviation);
		FishFinder.depth += this.currentSD;
		this.depthSmooth = Mathf.Lerp(this.depthSmooth, FishFinder.depth, Time.fixedDeltaTime * Mathf.Min(10f, Mathf.Max(3f, this.velocity / 3.6f)));
		this.currentGrassDeviation = Mathf.Clamp(this.currentGrassDeviation + (Random.value - 0.5f) * 0.2f, 0f, Mathf.Min(Mathf.Abs(this.depthSmooth) - 0.25f - this.destinationGrassThicknessMin, this.destinationGrassThicknessScatter - this.destinationGrassThicknessMin));
		if (this.firstTimeOn)
		{
			this.currentST = this.destinationSurfaceThicknessMin + Random.value * this.destinationSurfaceThicknessScatter;
			this.currentCD = this.destinationWidthThicknessMin + Random.value * this.destinationWidthThicknessScatter;
			this.currentGD = 0f;
			this.currentLT = this.destinationLimboThicknessMin + Random.value * this.destinationLimboThicknessScatter;
			this.currentGF = this.destinationGrassDensity;
		}
		else
		{
			this.currentST = Mathf.MoveTowards(this.currentST, this.destinationSurfaceThicknessMin + Random.value * this.destinationSurfaceThicknessScatter, 0.1f);
			this.currentCD = Mathf.MoveTowards(this.currentCD, this.destinationWidthThicknessMin + Random.value * this.destinationWidthThicknessScatter, 0.1f);
			this.currentGD = Mathf.MoveTowards(this.currentGD, this.destinationGrassThicknessMin + this.currentGrassDeviation, 0.1f);
			this.currentLT = Mathf.MoveTowards(this.currentLT, this.destinationLimboThicknessMin + Random.value * this.destinationLimboThicknessScatter, 0.1f);
			this.currentGF = Mathf.MoveTowards(this.currentGF, this.destinationGrassDensity, 0.01f);
		}
	}

	public int MinFishesToSpawn = 1;

	public int MaxFishesToSpawn = 5;

	public float GetFishInterval = 5f;

	public float TurningDelay = 1.2f;

	public float TurningTime = 1f;

	public Transform HolderYRotation;

	public Transform BodyXRotation;

	public Transform ScreenZRotation;

	[Space(10f)]
	public float FrameOffsetPercentage = 0.01f;

	[Space(10f)]
	public IBoatController BoatController;

	public RawImage image;

	public Image turnedOffImage;

	public Text DepthText;

	public Text TemperatureText;

	public Text VelocityText;

	public Text[] Depths;

	[Space(10f)]
	public Graphic[] InterfaceElements;

	public Color InterfaceColor;

	public Color GrassColor;

	public Color WaterColor;

	public Color SurfaceLineColor;

	public Color SubsurfaceCutoffColor;

	public Color LimboColor;

	public Color AbyssColor;

	[Space(10f)]
	public Sprite[] normalFishSprites;

	[Space(10f)]
	public Sprite[] cuttedFishSprites;

	public Color FishColor;

	private static Color MainInterfaceColor;

	private static Color MainFishColor;

	private string instanceId;

	private static Dictionary<IBoatController, RenderTexture[]> renderTextures = new Dictionary<IBoatController, RenderTexture[]>();

	private static Dictionary<IBoatController, Material> fishFinderMaterials = new Dictionary<IBoatController, Material>();

	private bool subscribed;

	private float MaxSpeed = 20f;

	private bool isMain = true;

	public float UpdateFPS = 30f;

	private float updateTime;

	private float playTime;

	private float sounderTime;

	private float depthNormalized;

	private static float depth = 0f;

	private float velocity;

	private bool tooFast;

	private float depthSmooth;

	private int MaxHistory;

	private EchoSounderKind type = EchoSounderKind.DepthAndFish;

	private bool isOn;

	private List<FishFinderFish> FishesToMove = new List<FishFinderFish>();

	private List<FishFinderFish> FreeFishes = new List<FishFinderFish>();

	public RectTransform FishesParent;

	public AudioSource Source;

	public AudioClip TurnOn;

	public AudioClip TurnOff;

	public AudioClip FishSpawned;

	public FishFinderFish FishPrefab;

	private float minScale = 0.75f;

	private float maxScale = 1.5f;

	private static Dictionary<IBoatController, List<FishFinder.FishToSpawn>> fishesToSpawn = new Dictionary<IBoatController, List<FishFinder.FishToSpawn>>();

	private const float grassDeviationMaxStep = 0.2f;

	private float currentGrassDeviation;

	private bool firstTimeOn = true;

	private static List<FishFinder.ColumnData> history = new List<FishFinder.ColumnData>();

	private float destinationSurfaceDeviation;

	private float destinationGrassThicknessScatter;

	private float destinationGrassDensity;

	private float destinationWidthThicknessMin;

	private float currentSD;

	private float currentST;

	private float currentCD;

	private float currentGD;

	private float currentGF;

	private float currentLT;

	private float destinationSurfaceThicknessMin;

	private float destinationSurfaceThicknessScatter;

	private float destinationGrassThicknessMin;

	private float destinationWidthThicknessScatter;

	private float destinationLimboThicknessMin;

	private float destinationLimboThicknessScatter;

	private class FishToSpawn
	{
		public float timeToSpawn;

		public Depth depthType;

		public float weight;

		public List<string> spawnedByInstances = new List<string>();
	}

	public class ColumnData
	{
		public float Depth;

		public float DepthMax;

		public float SurfaceDisplacement;

		public float CutoffDisplacement;

		public float GrassDisplacement;

		public float GrassFrequency;
	}
}
