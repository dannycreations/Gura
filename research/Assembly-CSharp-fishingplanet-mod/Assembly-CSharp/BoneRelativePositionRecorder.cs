using System;
using System.Collections.Generic;
using UnityEngine;

public class BoneRelativePositionRecorder : MonoBehaviour
{
	private void Start()
	{
		this.nextRecordAt = Time.time;
	}

	private void Update()
	{
		if (this.root != null && Time.time > this.nextRecordAt)
		{
			this.nextRecordAt = Time.time + this.dt;
			for (int i = 0; i < this.bonesToTrack.Count; i++)
			{
				GameObject gameObject = this.bonesToTrack[i];
				if (gameObject != null)
				{
				}
			}
		}
	}

	public GameObject root;

	public List<GameObject> bonesToTrack = new List<GameObject>();

	public float dt = 1f;

	private float nextRecordAt;
}
