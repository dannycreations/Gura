using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class ChumSystemViewer
	{
		public ChumSystemViewer()
		{
			this._root = new GameObject("ChumSystemViewer").transform;
		}

		public void Deactivate()
		{
			if (this._isActive)
			{
				PhotonConnectionFactory.Instance.OnChumPieceUpdated -= this.OnNewPiece;
			}
			if (this._isRespondWaiting)
			{
				PhotonConnectionFactory.Instance.OnDebugBiteSystemEvents -= this.OnDebugRequestSuccess;
				PhotonConnectionFactory.Instance.OnDebugBiteSystemEventsFailed -= this.OnDebugRequestFailed;
			}
		}

		private void OnDebugRequestFailed(Failure failure)
		{
			this._isRespondWaiting = false;
			LogHelper.Error("Chum system viewer error: {0}", new object[] { failure.ErrorMessage });
			PhotonConnectionFactory.Instance.OnDebugBiteSystemEvents -= this.OnDebugRequestSuccess;
			PhotonConnectionFactory.Instance.OnDebugBiteSystemEventsFailed -= this.OnDebugRequestFailed;
		}

		private void OnDebugRequestSuccess()
		{
			this._isRespondWaiting = false;
			this._isActive = !this._isActive;
			ShowHudElements.Instance.ActivateDebugCrossHair(this._isActive);
			LogHelper.Log("Chum system viewer enabled = {0}", new object[] { this._isActive });
			PhotonConnectionFactory.Instance.OnDebugBiteSystemEvents -= this.OnDebugRequestSuccess;
			PhotonConnectionFactory.Instance.OnDebugBiteSystemEventsFailed -= this.OnDebugRequestFailed;
			if (this._isActive)
			{
				PhotonConnectionFactory.Instance.OnChumPieceUpdated += this.OnNewPiece;
			}
			else
			{
				PhotonConnectionFactory.Instance.OnChumPieceUpdated -= this.OnNewPiece;
			}
		}

		private void SwitchActive()
		{
			if (this._isRespondWaiting)
			{
				LogHelper.Log("SwitchActive({0}) request ignored till previous was not processed", new object[] { !this._isActive });
				return;
			}
			LogHelper.Log("SwitchActive({0}) request", new object[] { !this._isActive });
			this._isRespondWaiting = true;
			PhotonConnectionFactory.Instance.OnDebugBiteSystemEvents += this.OnDebugRequestSuccess;
			PhotonConnectionFactory.Instance.OnDebugBiteSystemEventsFailed += this.OnDebugRequestFailed;
			PhotonConnectionFactory.Instance.DebugBiteSystemEvents(!this._isActive);
		}

		private void OnNewPiece(ChumPiece obj)
		{
			Transform transform = this._root.Find(obj.OwnerId.ToString());
			if (transform == null)
			{
				transform = new GameObject(obj.OwnerId.ToString()).transform;
				transform.parent = this._root;
				if (!this._counters.ContainsKey(obj.OwnerId))
				{
					this._counters[obj.OwnerId] = 0;
				}
			}
			ChumPieceView chumPieceView = Resources.Load<ChumPieceView>("ChumViewer/chumPiece");
			ChumPieceView chumPieceView2 = Object.Instantiate<ChumPieceView>(chumPieceView);
			Dictionary<Guid, int> counters;
			Guid ownerId;
			chumPieceView2.name = ((counters = this._counters)[ownerId = obj.OwnerId] = counters[ownerId] + 1).ToString();
			chumPieceView2.Setup(transform, obj, GameFactory.Player.CameraController.Camera);
		}

		public void Update()
		{
			if (this._isActive)
			{
				Transform transform = GameFactory.Player.CameraController.Camera.transform;
				RaycastHit maskedRayHit = Math3d.GetMaskedRayHit(transform.position, transform.position + transform.forward * 1000f, 1 << LayerMask.NameToLayer("UI"));
				if (maskedRayHit.collider != null)
				{
					ChumPieceView component = maskedRayHit.collider.transform.parent.GetComponent<ChumPieceView>();
					if (component != null && (component != this._lastPiece || this._lastPiece == null))
					{
						if (this._lastPiece != null)
						{
							this._lastPiece.SetActive(false);
						}
						this._lastPiece = component;
						this._lastPiece.SetActive(true);
					}
				}
				else if (this._lastPiece != null)
				{
					this._lastPiece.SetActive(false);
				}
			}
			if (Input.GetKeyUp(117) && Input.GetKey(306))
			{
				this.SwitchActive();
			}
		}

		private bool _isActive;

		private bool _isRespondWaiting;

		private ChumPieceView _lastPiece;

		private Transform _root;

		private Dictionary<Guid, int> _counters = new Dictionary<Guid, int>();
	}
}
