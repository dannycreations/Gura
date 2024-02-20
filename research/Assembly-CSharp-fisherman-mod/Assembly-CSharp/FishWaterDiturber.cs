using System;
using System.Collections.Generic;
using UnityEngine;

public class FishWaterDiturber
{
	public FishWaterDiturber(Transform root, float size, bool isVisible = true)
	{
		this._isVisible = isVisible;
		this.rootTransform = root;
		this.allTransforms.Add(new HistoryTransform(root, true));
		this.ProcessHierarchy(root);
		this.fishSize = size;
		this.maxDisturbanceDepth = size;
		this.rootDisturbanceSize = this.rootDisturbanceSize * size / 0.3f;
		this.maxDisturbanceForce = this.maxDisturbanceForce * size / 0.3f;
	}

	private void ProcessHierarchy(Transform transform)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			this.allTransforms.Add(new HistoryTransform(child, false));
			this.ProcessHierarchy(child);
		}
	}

	public void SetVisibility(bool flag)
	{
		this._isVisible = flag;
	}

	public void Update()
	{
		if (this.timeSpent < 1f)
		{
			this.timeSpent += Time.deltaTime;
		}
		if (this.isObjectDestroyed || GameFactory.Water == null)
		{
			return;
		}
		try
		{
			for (int i = 0; i < this.allTransforms.Count; i++)
			{
				HistoryTransform historyTransform = this.allTransforms[i];
				if (historyTransform.Transform == null)
				{
					break;
				}
				if (historyTransform.HasCollidedWithWater && historyTransform.Transform.position.y < 0.05f)
				{
					if (historyTransform.IsRoot)
					{
						this.timeSpent = 0f;
						if (this._isVisible)
						{
							DynWaterParticlesController.CreateSplash(GameFactory.Player.CameraController.transform, historyTransform.Transform.position, "2D/Splashes/pSplash_universal", this.rootDisturbanceSize, 1f, true, true, 1);
						}
						if (historyTransform.IsEntering)
						{
							if (this._isVisible)
							{
								RandomSounds.PlaySoundAtPoint("Sounds/Actions/FishSplash/Fish_Fall_In_Water", historyTransform.Transform.position, this.rootDisturbanceSize * 0.125f, false);
							}
						}
						else if (this._isVisible)
						{
							RandomSounds.PlaySoundAtPoint("Sounds/Actions/FishSplash/Fish_Splash_Big", historyTransform.Transform.position, this.rootDisturbanceSize * 0.125f, false);
						}
					}
					else
					{
						this.Disturbances.Enqueue(historyTransform);
						if (this.timeSpent > 1f && Random.Range(0f, 1f) < 0.05f)
						{
							if (this._isVisible)
							{
								RandomSounds.PlaySoundAtPoint("Sounds/Actions/FishSplash/Fish_Splash_Regular", historyTransform.Transform.position, this.rootDisturbanceSize * 0.125f, false);
							}
							this.timeSpent = 0f;
						}
					}
				}
			}
			if (this.Disturbances.Count > 25)
			{
				for (int j = 0; j < this.Disturbances.Count - 25; j++)
				{
					this.Disturbances.Dequeue();
				}
			}
			int num = 0;
			while (num < this.Disturbances.Count && num < 5)
			{
				HistoryTransform historyTransform2 = this.Disturbances.Dequeue();
				try
				{
					if (this._isVisible)
					{
						GameFactory.Water.AddWaterDisturb(historyTransform2.Transform.position, 0.01f, WaterDisturbForce.XXSmall);
					}
				}
				catch (MissingReferenceException)
				{
				}
				num++;
			}
			float num2 = this.maxDisturbanceDepth + this.rootTransform.position.y;
			if (num2 > 0f && num2 < this.maxDisturbanceDepth)
			{
				float num3 = num2 / this.maxDisturbanceDepth;
				if (this._isVisible)
				{
					GameFactory.Water.AddWaterDisturb(this.rootTransform.position, this.fishSize, Mathf.Lerp(1f, this.maxDisturbanceForce, num3));
				}
				if (this.timeSpent > 1f && Random.Range(0f, 1f) < 0.05f)
				{
					if (this._isVisible)
					{
						RandomSounds.PlaySoundAtPoint("Sounds/Actions/FishSplash/Fish_Splash_Small", this.rootTransform.position, this.rootDisturbanceSize * 0.125f, false);
					}
					this.timeSpent = 0f;
				}
			}
		}
		catch (MissingReferenceException)
		{
			this.isObjectDestroyed = true;
		}
	}

	private const int MaxDisturbanceCount = 25;

	private readonly float rootDisturbanceSize = 3f;

	private const float StdFishSize = 0.3f;

	private const float DisturbanceRadius = 0.01f;

	private const WaterDisturbForce DisturbanceForce = WaterDisturbForce.XXSmall;

	private readonly Transform rootTransform;

	private readonly List<HistoryTransform> allTransforms = new List<HistoryTransform>();

	public readonly Queue<HistoryTransform> Disturbances = new Queue<HistoryTransform>();

	private readonly float fishSize;

	private readonly float maxDisturbanceDepth;

	private const float MinDisturbanceForce = 1f;

	private readonly float maxDisturbanceForce = 8f;

	private float timeSpent;

	private const float PlayTimeout = 1f;

	private bool isObjectDestroyed;

	private bool _isVisible;
}
