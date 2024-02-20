using System;
using System.Collections.Generic;
using UnityEngine;

public class BonePositionRecorder : MonoBehaviour
{
	private void Start()
	{
		this.nextRecordAt = Time.time;
	}

	private void Update()
	{
		if (Time.time > this.nextRecordAt)
		{
			this.nextRecordAt = Time.time + this.dt;
			for (int i = 0; i < this.bonesToTrack.Count; i++)
			{
				if (this.bonesToTrack[i] != null)
				{
				}
			}
		}
	}

	public List<GameObject> bonesToTrack = new List<GameObject>();

	public float dt = 1f;

	private float nextRecordAt;
}
