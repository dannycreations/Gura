﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotonTransformViewPositionControl
{
	public PhotonTransformViewPositionControl(PhotonTransformViewPositionModel model)
	{
		this.m_Model = model;
	}

	private Vector3 GetOldestStoredNetworkPosition()
	{
		Vector3 vector = this.m_NetworkPosition;
		if (this.m_OldNetworkPositions.Count > 0)
		{
			vector = this.m_OldNetworkPositions.Peek();
		}
		return vector;
	}

	public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
	{
		this.m_SynchronizedSpeed = speed;
		this.m_SynchronizedTurnSpeed = turnSpeed;
	}

	public Vector3 UpdatePosition(Vector3 currentPosition)
	{
		Vector3 vector = this.GetNetworkPosition() + this.GetExtrapolatedPositionOffset();
		switch (this.m_Model.InterpolateOption)
		{
		case PhotonTransformViewPositionModel.InterpolateOptions.Disabled:
			if (!this.m_UpdatedPositionAfterOnSerialize)
			{
				currentPosition = vector;
				this.m_UpdatedPositionAfterOnSerialize = true;
			}
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.FixedSpeed:
			currentPosition = Vector3.MoveTowards(currentPosition, vector, Time.deltaTime * this.m_Model.InterpolateMoveTowardsSpeed);
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.EstimatedSpeed:
		{
			int num = Mathf.Min(1, this.m_OldNetworkPositions.Count);
			float num2 = Vector3.Distance(this.m_NetworkPosition, this.GetOldestStoredNetworkPosition()) / (float)num;
			currentPosition = Vector3.MoveTowards(currentPosition, vector, Time.deltaTime * num2);
			break;
		}
		case PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues:
			if (this.m_SynchronizedSpeed.magnitude == 0f)
			{
				currentPosition = vector;
			}
			else
			{
				currentPosition = Vector3.MoveTowards(currentPosition, vector, Time.deltaTime * this.m_SynchronizedSpeed.magnitude);
			}
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.Lerp:
			currentPosition = Vector3.Lerp(currentPosition, vector, Time.deltaTime * this.m_Model.InterpolateLerpSpeed);
			break;
		}
		if (this.m_Model.TeleportEnabled && Vector3.Distance(currentPosition, this.GetNetworkPosition()) > this.m_Model.TeleportIfDistanceGreaterThan)
		{
			currentPosition = this.GetNetworkPosition();
		}
		return currentPosition;
	}

	public Vector3 GetNetworkPosition()
	{
		return this.m_NetworkPosition;
	}

	public Vector3 GetExtrapolatedPositionOffset()
	{
		float num = (float)(PhotonNetwork.time - this.m_LastSerializeTime);
		if (this.m_Model.ExtrapolateIncludingRoundTripTime)
		{
			num += (float)PhotonNetwork.GetPing() / 1000f;
		}
		Vector3 vector = Vector3.zero;
		PhotonTransformViewPositionModel.ExtrapolateOptions extrapolateOption = this.m_Model.ExtrapolateOption;
		if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues)
		{
			if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.FixedSpeed)
			{
				if (extrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.EstimateSpeedAndTurn)
				{
					Vector3 vector2 = (this.m_NetworkPosition - this.GetOldestStoredNetworkPosition()) * (float)PhotonNetwork.sendRateOnSerialize;
					vector = vector2 * num;
				}
			}
			else
			{
				Vector3 normalized = (this.m_NetworkPosition - this.GetOldestStoredNetworkPosition()).normalized;
				vector = normalized * this.m_Model.ExtrapolateSpeed * num;
			}
		}
		else
		{
			Quaternion quaternion = Quaternion.Euler(0f, this.m_SynchronizedTurnSpeed * num, 0f);
			vector = quaternion * (this.m_SynchronizedSpeed * num);
		}
		return vector;
	}

	public void OnPhotonSerializeView(Vector3 currentPosition, PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.m_Model.SynchronizeEnabled)
		{
			return;
		}
		if (stream.isWriting)
		{
			this.SerializeData(currentPosition, stream, info);
		}
		else
		{
			this.DeserializeData(stream, info);
		}
		this.m_LastSerializeTime = PhotonNetwork.time;
		this.m_UpdatedPositionAfterOnSerialize = false;
	}

	private void SerializeData(Vector3 currentPosition, PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(currentPosition);
		if (this.m_Model.ExtrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues || this.m_Model.InterpolateOption == PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues)
		{
			stream.SendNext(this.m_SynchronizedSpeed);
			stream.SendNext(this.m_SynchronizedTurnSpeed);
		}
	}

	private void DeserializeData(PhotonStream stream, PhotonMessageInfo info)
	{
		this.m_OldNetworkPositions.Enqueue(this.m_NetworkPosition);
		while (this.m_OldNetworkPositions.Count > this.m_Model.ExtrapolateNumberOfStoredPositions)
		{
			this.m_OldNetworkPositions.Dequeue();
		}
		this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
		if (this.m_Model.ExtrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues || this.m_Model.InterpolateOption == PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues)
		{
			this.m_SynchronizedSpeed = (Vector3)stream.ReceiveNext();
			this.m_SynchronizedTurnSpeed = (float)stream.ReceiveNext();
		}
	}

	private PhotonTransformViewPositionModel m_Model;

	private float m_CurrentSpeed;

	private double m_LastSerializeTime;

	private Vector3 m_SynchronizedSpeed = Vector3.zero;

	private float m_SynchronizedTurnSpeed;

	private Vector3 m_NetworkPosition;

	private Queue<Vector3> m_OldNetworkPositions = new Queue<Vector3>();

	private bool m_UpdatedPositionAfterOnSerialize = true;
}
