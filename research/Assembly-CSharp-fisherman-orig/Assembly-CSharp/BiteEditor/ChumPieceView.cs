using System;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class ChumPieceView : MonoBehaviour
	{
		public float Translation
		{
			get
			{
				Vector3 vector;
				vector..ctor(this._from.transform.position.x, 0f, this._from.transform.position.z);
				Vector3 vector2;
				vector2..ctor(base.transform.position.x, 0f, base.transform.position.z);
				return (vector - vector2).magnitude;
			}
		}

		public void Setup(Transform root, ChumPiece piece, Camera camera)
		{
			this._label.cameraToLookAt = camera;
			this._label.gameObject.SetActive(false);
			this._piece = piece;
			base.transform.parent = root;
			base.transform.position = piece.Position.ToVector3();
			GameObject gameObject = new GameObject();
			gameObject.name = "Simplified flow end point";
			Vector3 vector = base.transform.position + piece.FlowV.ToVector3();
			LogHelper.Log(LogHelper.Vector3ToString("server flow", piece.FlowV.ToVector3()));
			gameObject.transform.position = vector;
			gameObject.transform.SetParent(base.transform, true);
			Transform child = base.transform.GetChild(0);
			child.localScale = new Vector3(piece.AttractionR, piece.AttractionH * 2f, piece.AttractionR);
			child.localPosition = Vector3.zero;
			this._addedAt = Time.time;
			this._removeAt = Time.time + (piece.EndAt - piece.AddedAt) * 15f;
			this._from.transform.localScale = new Vector3(piece.AttractionR, piece.AttractionH * 2f, piece.AttractionR);
			this._from.transform.position = piece.FromPosition.ToVector3();
			this._from.transform.LookAt(gameObject.transform);
			this._from.SetActive(false);
		}

		private void Update()
		{
			if (this._isActive)
			{
				float num = -Init3D.SceneSettings.HeightMap.GetBottomHeight(new Vector3f(base.transform.position));
				float num2 = -Math.Min(base.transform.position.y + this._piece.AttractionH, 0f);
				float num3 = Math.Max(num + (base.transform.position.y - this._piece.AttractionH), 0f);
				this._label.transform.parent.localPosition = new Vector3(0f, this._piece.AttractionH + this._labelDy, 0f);
				this._label.SetText(string.Format("Deep:{0:f2}, ToTop:{1:f2}, ToBottom:{2:f2}, Y:{3:f2}, H:{4:f2}, R:{5:f2}", new object[]
				{
					num,
					num2,
					num3,
					base.transform.position.y,
					this._piece.AttractionH,
					this._piece.AttractionR
				}));
			}
			this._timeLeft = this._removeAt - Time.time;
			this._growLeft = this._addedAt + this._piece.PowerFillTime * 15f - Time.time;
			this._platoLeft = ((this._growLeft >= 0f) ? 0f : (this._addedAt + (this._piece.PowerFillTime + this._piece.MaxPowerDuration) * 15f - Time.time));
			this._fallLeft = ((this._platoLeft >= 0f) ? 0f : (this._addedAt + (this._piece.PowerFillTime + this._piece.MaxPowerDuration + this._piece.PowerReleaseTime) * 15f - Time.time));
			if (this._removeAt < Time.time)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void SetActive(bool flag)
		{
			this._isActive = flag;
			this._from.SetActive(flag);
			this._label.gameObject.SetActive(flag);
		}

		[SerializeField]
		private float _timeLeft;

		[SerializeField]
		private float _growLeft;

		[SerializeField]
		private float _platoLeft;

		[SerializeField]
		private float _fallLeft;

		[SerializeField]
		private GameObject _from;

		[SerializeField]
		private BillboardText _label;

		[SerializeField]
		private float _labelDy = 0.1f;

		private ChumPiece _piece;

		private float _addedAt;

		private float _removeAt;

		private bool _isActive;
	}
}
