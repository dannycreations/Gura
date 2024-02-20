using System;
using System.Collections.Generic;
using System.Diagnostics;
using Phy;
using UnityEngine;

namespace TPM
{
	public class Line3rdBehaviour : LineBehaviour
	{
		public Line3rdBehaviour(LineController owner, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot)
			: base(owner, rodAssembly, rodSlot)
		{
			this._tmpCurve = new BezierCurve(2);
			this._wasInitialized = false;
			this._isLeaderVisible = false;
			this.lrLineObject.SetVertexCount(20);
			for (int i = 0; i < 20; i++)
			{
				this.lrLineObject.SetPosition(i, Vector3.zero);
			}
			this._prevMainAndLeaderPoints = new List<Vector3>(3);
			this._targetMainAndLeaderPoints = new List<Vector3>(3);
			for (int j = 0; j < 3; j++)
			{
				this._prevMainAndLeaderPoints.Add(Vector3.zero);
				this._targetMainAndLeaderPoints.Add(Vector3.zero);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Line3rdBehaviour.FinishThrowingDelegate EFinishThrowing = delegate
		{
		};

		public Vector3 LurePoint
		{
			get
			{
				return this._mainLineEndPoint;
			}
		}

		public Vector3 BobberPoint
		{
			get
			{
				return this._bobberPoint;
			}
		}

		public Vector3 LineEndPoint
		{
			get
			{
				return this._leaderEndPoint;
			}
		}

		public void SyncInit(Rod3rdBehaviour rodBehaviour)
		{
			this.rodLinePointsCount = rodBehaviour.LineLocatorsProp.Count + 1;
			this.lrLineOnRodObject.SetVertexCount(this.rodLinePointsCount);
			for (int i = 0; i < this.rodLinePointsCount; i++)
			{
				this.lrLineOnRodObject.SetPosition(i, Vector3.zero);
			}
			base.SetRendererLineWidth(0.0015f);
		}

		public new void SetVisibility(bool flag)
		{
			this.lrLineObject.enabled = flag;
			this.lrLineOnRodObject.enabled = flag;
			this.lrLineLeaderObject.enabled = flag;
			for (int i = 0; i < this._sinkerRenderers.Length; i++)
			{
				this._sinkerRenderers[i].enabled = false;
			}
		}

		public void SetOpaque(float prc)
		{
			this.lrLineObject.material.SetFloat("_CharacterOpaque", prc);
			this.lrLineOnRodObject.material.SetFloat("_CharacterOpaque", prc);
			this.lrLineLeaderObject.material.SetFloat("_CharacterOpaque", prc);
			for (int i = 0; i < this._sinkerRenderers.Length; i++)
			{
				this._sinkerRenderers[i].material.SetFloat("_CharacterOpaque", prc);
			}
		}

		public override void TransitToNewLineWidth()
		{
		}

		public void ServerUpdate(TPMAssembledRod rod, bool isTackleThrown, bool isShowSmallFishState, bool isLineContactGround, bool isPitching, bool isPhotomode, float dtPrc)
		{
			this._isShowSmallFishState = isShowSmallFishState;
			this._isPhotomode = isPhotomode;
			this._isPitching = isPitching;
			this._isLineContactGround = isLineContactGround;
			this._isTackleThrown = isTackleThrown;
			Vector3[] mainAndLeaderPoints = rod.lineData.mainAndLeaderPoints;
			Vector3 sinkersFirstPoint = rod.lineData.sinkersFirstPoint;
			if (this._wasInitialized)
			{
				for (int i = 0; i < mainAndLeaderPoints.Length; i++)
				{
					this._prevMainAndLeaderPoints[i] = Vector3.Lerp(this._prevMainAndLeaderPoints[i], this._targetMainAndLeaderPoints[i], dtPrc);
					this._targetMainAndLeaderPoints[i] = mainAndLeaderPoints[i];
				}
				this._prevSinkersFirstPoint = Vector3.Lerp(this._prevSinkersFirstPoint, this._targetSinkersFirstPoint, dtPrc);
				this._targetSinkersFirstPoint = sinkersFirstPoint;
			}
			else
			{
				this._wasInitialized = true;
				for (int j = 0; j < mainAndLeaderPoints.Length; j++)
				{
					this._prevMainAndLeaderPoints[j] = mainAndLeaderPoints[j];
					this._targetMainAndLeaderPoints[j] = mainAndLeaderPoints[j];
				}
				this._prevSinkersFirstPoint = sinkersFirstPoint;
				this._targetSinkersFirstPoint = sinkersFirstPoint;
			}
			this._isLeaderVisible = rod.lineData.isLeaderVisible && !isShowSmallFishState;
			if (rod.tackleData != null)
			{
				float? throwStartAngle = rod.tackleData.throwStartAngle;
				if (throwStartAngle != null)
				{
					this._throwStartAngle = rod.tackleData.throwStartAngle;
					this._throwTargetPosition = rod.tackleData.throwTargetPosition;
					return;
				}
			}
			if (this._throwPath != null)
			{
				this.EFinishThrowing();
			}
			this._throwPath = null;
			this._throwStartAngle = null;
			this._throwTargetPosition = null;
		}

		public void RodSyncUpdate(Rod3rdBehaviour rodBehaviour, List<Fish3rdBehaviour> fishes, Vector3 lineArcLocator, float dtPrc)
		{
			this._serverRodPeakPoint = rodBehaviour.ServerTipPos;
			this._clientRodPeakPoint = rodBehaviour.GetRodPeakPosition();
			this._serverEndPoint = Vector3.Lerp(this._prevMainAndLeaderPoints[2], this._targetMainAndLeaderPoints[2], dtPrc);
			float num = 1f;
			bool flag = false;
			if (!this._isPhotomode && fishes != null)
			{
				for (int i = 0; i < fishes.Count; i++)
				{
					if (fishes[i].state == TPMFishState.Hooked)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag && !this._isLineContactGround)
			{
				num = this.CalcGlobalNorm(rodBehaviour, dtPrc);
			}
			if (this._throwPath == null && this._throwTargetPosition != null && this._throwStartAngle != null)
			{
				Vector3 vector = this.CalcLinePoint(rodBehaviour, 1, num, dtPrc);
				this._throwPath = TackleThrowManager.GetPath(vector, this._throwTargetPosition.Value, this._throwStartAngle.Value, out this._flightDuration);
				this._throwStartAt = Time.time;
			}
			this.UpdateLeaderLine(rodBehaviour, num, dtPrc);
			this.UpdateMainLine(rodBehaviour, num, dtPrc);
			this.UpdateRodLine(rodBehaviour, lineArcLocator);
		}

		private float CalcGroundNorm(float dtPrc)
		{
			return 1f - ((this._serverEndPoint.y <= 0.01f) ? 0f : Math.Min((this._serverEndPoint.y - 0.01f) / 0.29000002f, 1f));
		}

		private float CalcGlobalNorm(Rod3rdBehaviour rodBehaviour, float dtPrc)
		{
			if (!this._isTackleThrown)
			{
				return 0f;
			}
			float num = this.CalcGroundNorm(dtPrc);
			Vector3 position = rodBehaviour.ServerRodTransform.position;
			float magnitude = (position - this._serverEndPoint).magnitude;
			float num2 = ((magnitude <= 6f) ? 0f : Math.Min((magnitude - 6f) / 4f, 1f));
			return Math.Max(num, num2);
		}

		private Vector3 CalcLinePoint(Rod3rdBehaviour rodBehaviour, int linePointIndex, float globalNorm, float dtPrc)
		{
			float num = 0f;
			if (!this._isTackleThrown && this._isLineContactGround && !this._isPitching)
			{
				if (linePointIndex == 0)
				{
					num = 0.5f;
				}
				else if (linePointIndex == 2)
				{
					num = ((!this._isLeaderVisible) ? 1f : 2f);
				}
				else
				{
					num = 1f;
				}
			}
			return this.CalcPoint(rodBehaviour, this._prevMainAndLeaderPoints[linePointIndex], this._targetMainAndLeaderPoints[linePointIndex], globalNorm, num, dtPrc);
		}

		private Vector3 CalcPoint(Rod3rdBehaviour rodBehaviour, Vector3 prevPoint, Vector3 targetPoint, float globalNorm, float simpleLinePrc, float dtPrc)
		{
			Vector3 vector = Vector3.Lerp(prevPoint, targetPoint, dtPrc);
			if (this._isPhotomode || this._isShowSmallFishState || (!this._isTackleThrown && this._isPitching))
			{
				Vector3 vector2 = rodBehaviour.ServerRodTransform.InverseTransformPoint(vector);
				Vector3 vector3 = rodBehaviour.transform.TransformPoint(vector2);
				return globalNorm * vector + (1f - globalNorm) * vector3;
			}
			if (!this._isTackleThrown && this._isLineContactGround && !this._isPitching)
			{
				return this._clientRodPeakPoint + new Vector3(0f, -simpleLinePrc * base.MinLineLength, 0f);
			}
			Vector3 vector4 = vector - this._serverRodPeakPoint;
			Vector3 vector5 = this._clientRodPeakPoint + vector4;
			if (this._isTackleThrown)
			{
				return globalNorm * vector + (1f - globalNorm) * vector5;
			}
			return vector5;
		}

		private float FlightPrc
		{
			get
			{
				return (this._throwPath == null) ? 0f : Math.Min((Time.time - this._throwStartAt) / this._flightDuration, 1f);
			}
		}

		private Vector3 CalcTrowingLinePoint(Vector3 rodPeakPoint, Vector3 clientEndPoint, Vector3 serverEndPoint, Vector3 serverPoint)
		{
			float magnitude = (serverPoint - serverEndPoint).magnitude;
			Vector3 normalized = (rodPeakPoint - clientEndPoint).normalized;
			return clientEndPoint + normalized * magnitude;
		}

		private void UpdateLeaderLine(Rod3rdBehaviour rodBehaviour, float globalNorm, float dtPrc)
		{
			if (this._throwPath != null)
			{
				this._leaderEndPoint = this._throwPath.GetPoint(this.FlightPrc);
			}
			else
			{
				this._leaderEndPoint = this.CalcLinePoint(rodBehaviour, 2, globalNorm, dtPrc);
			}
			if (this._isLeaderVisible)
			{
				if (this._throwPath != null)
				{
					this._tmpCurve.AnchorPoints[0] = this.CalcTrowingLinePoint(this._clientRodPeakPoint, this._leaderEndPoint, this._serverEndPoint, Vector3.Lerp(this._prevMainAndLeaderPoints[1], this._targetMainAndLeaderPoints[1], dtPrc));
					this._tmpCurve.AnchorPoints[1] = this.CalcTrowingLinePoint(this._clientRodPeakPoint, this._leaderEndPoint, this._serverEndPoint, Vector3.Lerp(this._prevSinkersFirstPoint, this._targetSinkersFirstPoint, dtPrc));
				}
				else
				{
					this._tmpCurve.AnchorPoints[0] = this.CalcLinePoint(rodBehaviour, 1, globalNorm, dtPrc);
					this._tmpCurve.AnchorPoints[1] = this.CalcPoint(rodBehaviour, this._prevSinkersFirstPoint, this._targetSinkersFirstPoint, globalNorm, 0.5f, dtPrc);
				}
				this._tmpCurve.AnchorPoints[2] = this._leaderEndPoint;
				this.lrLineLeaderObject.SetVertexCount(10);
				for (int i = 0; i < 10; i++)
				{
					this.lrLineLeaderObject.SetPosition(i, this._tmpCurve.Point((float)i / 9f));
				}
				float num = 0.5f / (float)base.Sinkers.Count;
				for (int j = 0; j < base.Sinkers.Count; j++)
				{
					base.Sinkers[j].transform.position = this._tmpCurve.Point(0.5f + (float)j * num);
				}
			}
			else
			{
				this.lrLineLeaderObject.SetVertexCount(0);
			}
		}

		private void UpdateMainLine(Rod3rdBehaviour rodBehaviour, float globalNorm, float dtPrc)
		{
			this._tmpCurve.AnchorPoints[0] = this._clientRodPeakPoint;
			if (this._throwPath != null)
			{
				this._tmpCurve.AnchorPoints[1] = this.CalcTrowingLinePoint(this._clientRodPeakPoint, this._leaderEndPoint, this._serverEndPoint, Vector3.Lerp(this._prevMainAndLeaderPoints[0], this._targetMainAndLeaderPoints[0], dtPrc));
				if (this._isLeaderVisible)
				{
					this._mainLineEndPoint = this.CalcTrowingLinePoint(this._clientRodPeakPoint, this._leaderEndPoint, this._serverEndPoint, Vector3.Lerp(this._prevMainAndLeaderPoints[1], this._targetMainAndLeaderPoints[1], dtPrc));
				}
				else
				{
					this._mainLineEndPoint = this._leaderEndPoint;
				}
				this._bobberPoint = this._mainLineEndPoint;
				this._tmpCurve.AnchorPoints[2] = this._mainLineEndPoint;
			}
			else
			{
				this._mainLineEndPoint = this.CalcLinePoint(rodBehaviour, this._isLeaderVisible ? 1 : 2, globalNorm, dtPrc);
				this._tmpCurve.AnchorPoints[1] = (this._clientRodPeakPoint + this._mainLineEndPoint) * 0.5f;
				Vector3 vector = this.CalcLinePoint(rodBehaviour, 1, globalNorm, dtPrc);
				float num = (this._clientRodPeakPoint - vector).magnitude / (this._clientRodPeakPoint - this._mainLineEndPoint).magnitude;
				this._tmpCurve.AnchorPoints[2] = this._mainLineEndPoint;
				this._bobberPoint = this._tmpCurve.Point(num);
			}
			for (int i = 0; i < 20; i++)
			{
				this.lrLineObject.SetPosition(i, this._tmpCurve.Point((float)i / 19f));
			}
		}

		private void UpdateRodLine(Rod3rdBehaviour rodBehaviour, Vector3 reelLineArcLocatorPos)
		{
			this.lrLineOnRodObject.SetPosition(0, reelLineArcLocatorPos);
			this.lrLineOnRodObject.SetPosition(1, base.GetSecondRodPoint(rodBehaviour, reelLineArcLocatorPos));
			for (int i = 2; i < this.rodLinePointsCount; i++)
			{
				this.lrLineOnRodObject.SetPosition(i, rodBehaviour.LineLocatorsProp[i - 1].position);
			}
		}

		private const int MAIN_LINE_INTERPOLATED_POINTS_COUNT = 20;

		private const int LIDER_INTERPOLATED_POINTS_COUNT = 10;

		private const float GLOBAL_STATE_MIN_WATER_H = 0.01f;

		private const float GLOBAL_STATE_MAX_WATER_H = 0.3f;

		private const float GLOBAL_STATE_MIN_DIST_TO_ROD = 6f;

		private const float GLOBAL_STATE_MAX_DIST_TO_ROD = 10f;

		private const float LINE_WIDTH = 0.0015f;

		private const bool SINKERS_VISIBILITY = false;

		private List<Vector3> _prevMainAndLeaderPoints;

		private List<Vector3> _targetMainAndLeaderPoints;

		private Vector3 _prevSinkersFirstPoint;

		private Vector3 _targetSinkersFirstPoint;

		private bool _isLeaderVisible;

		private bool _wasInitialized;

		private BezierCurve _tmpCurve;

		private bool _isTackleThrown;

		private bool _isLineContactGround;

		private bool _isPitching;

		private bool _isPhotomode;

		private bool _isShowSmallFishState;

		private Vector3? _throwTargetPosition;

		private float? _throwStartAngle;

		private VerticalParabola _throwPath;

		private float _throwStartAt;

		private float _flightDuration;

		private Vector3 _mainLineEndPoint;

		private Vector3 _bobberPoint;

		private Vector3 _leaderEndPoint;

		private Vector3 _clientRodPeakPoint;

		private Vector3 _serverRodPeakPoint;

		private Vector3 _serverEndPoint;

		public delegate void FinishThrowingDelegate();
	}
}
