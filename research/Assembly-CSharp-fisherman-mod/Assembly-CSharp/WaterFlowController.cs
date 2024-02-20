using System;
using System.Collections;
using System.Collections.Generic;
using BiteEditor.ObjectModel;
using UnityEngine;

public class WaterFlowController : MonoBehaviour, IWaterFlowController
{
	public FishWaterBase WaterBaseInstance
	{
		get
		{
			return this.waterBase;
		}
	}

	internal void Start()
	{
		if (GameFactory.WaterFlow != null && GameFactory.WaterFlow != this)
		{
			throw new SceneConfigException("WaterFlowController should be only one!");
		}
		GameFactory.WaterFlow = this;
		this.waterBase = base.GetComponent<FishWaterBase>();
		this.streamBoxes = new List<WaterFlowController.StreamBoxInfoEx>();
		if (this.StreamBoxesParent != null)
		{
			IEnumerator enumerator = this.StreamBoxesParent.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					StreamBoxInfo component = transform.GetComponent<StreamBoxInfo>();
					if (component != null && transform.GetComponent<Collider>() != null)
					{
						WaterFlowController.StreamBoxInfoEx streamBoxInfoEx = new WaterFlowController.StreamBoxInfoEx
						{
							Info = component,
							CollderBounds = transform.GetComponent<Collider>().bounds,
							Position = transform.position,
							Scale = transform.localScale,
							Rotation = transform.rotation
						};
						this.streamBoxes.Add(streamBoxInfoEx);
						transform.GetComponent<Collider>().enabled = false;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	public Vector3 GetStreamSpeed(Vector3 position)
	{
		Texture2D texture2D = (Texture2D)this.waterBase.sharedMaterial.GetTexture("_FlowmapTex");
		float heightModifier = FlowMap.GetHeightModifier(-position.y + 0.0125f);
		Vector3 vector = Vector3.one;
		FishWaterTile componentInChildren = this.waterBase.GetComponentInChildren<FishWaterTile>();
		if (componentInChildren != null)
		{
			vector = componentInChildren.transform.localScale;
		}
		float num = this.waterBase.sharedMaterial.GetFloat("_FlowSpeed") * heightModifier;
		Vector4 vector2 = this.waterBase.sharedMaterial.GetVector("_FlowOffsets");
		float @float = this.waterBase.sharedMaterial.GetFloat("_AnimLength");
		Vector4 vector3 = this.waterBase.sharedMaterial.GetVector("_BumpTiling");
		int num2 = (int)((position.x * vector2.w + vector2.x) * (float)texture2D.width);
		int num3 = (int)((position.z * vector2.w + vector2.z) * (float)texture2D.height);
		Color pixel = texture2D.GetPixel(num2, num3);
		float num4 = num / ((vector3.x + vector3.z) * 0.5f * @float);
		Vector3 vector4 = new Vector3(-(pixel.r * 2f - 1f), 0f, -(pixel.g * 2f - 1f)) * num4;
		Vector3 boxedSpeed = this.GetBoxedSpeed(position);
		return vector4 * 0.8f + boxedSpeed;
	}

	private Vector3 GetBoxedSpeed(Vector3 position)
	{
		WaterFlowController.StreamBoxInfoEx streamBoxInfoEx = null;
		foreach (WaterFlowController.StreamBoxInfoEx streamBoxInfoEx2 in this.streamBoxes)
		{
			if (streamBoxInfoEx2.CollderBounds.Contains(position))
			{
				streamBoxInfoEx = streamBoxInfoEx2;
				break;
			}
		}
		if (streamBoxInfoEx != null)
		{
			float num = this.CalculateScale(streamBoxInfoEx.Position, streamBoxInfoEx.Scale, position);
			return streamBoxInfoEx.Rotation * this.GetBoxStreamVelocity(streamBoxInfoEx.Info) * num;
		}
		return Vector3.zero;
	}

	private Vector3 GetBoxStreamVelocity(StreamBoxInfo info)
	{
		StreamDirection direction = info.direction;
		if (direction == StreamDirection.XAxis)
		{
			return new Vector3(info.streamSpeed, 0f, 0f);
		}
		if (direction == StreamDirection.YAxis)
		{
			return new Vector3(0f, info.streamSpeed, 0f);
		}
		if (direction != StreamDirection.ZAxis)
		{
			return Vector3.zero;
		}
		return new Vector3(0f, 0f, info.streamSpeed);
	}

	private float CalculateScale(Vector3 boxPosition, Vector3 boxScale, Vector3 point)
	{
		float num = 2f * Mathf.Abs(boxPosition.x - point.x) / boxScale.x;
		float num2 = 2f * Mathf.Abs(boxPosition.y - point.y) / boxScale.y;
		float num3 = 2f * Mathf.Abs(boxPosition.z - point.z) / boxScale.z;
		float num4 = (num + num2 + num3) / 3f;
		return Mathf.Clamp01(1f - num4);
	}

	public Transform StreamBoxesParent;

	private FishWaterBase waterBase;

	private List<WaterFlowController.StreamBoxInfoEx> streamBoxes;

	private class StreamBoxInfoEx
	{
		public StreamBoxInfo Info;

		public Bounds CollderBounds;

		public Vector3 Position;

		public Vector3 Scale;

		public Quaternion Rotation;
	}
}
