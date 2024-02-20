using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace Cayman
{
	public class CaymanGenerator : MonoBehaviour
	{
		public float JUMPING_DIR_GOOD_ANGLE
		{
			get
			{
				return this._jumpingDirGoodAngle;
			}
		}

		public float JUMPING_MIN_DIST_TO_PLAYER
		{
			get
			{
				return this._settings[2].MinPlayerDistance;
			}
		}

		public static CaymanGenerator Instance
		{
			get
			{
				return CaymanGenerator._instance;
			}
		}

		public bool IsJumpReady
		{
			get
			{
				return this._isJumpReady;
			}
		}

		private void Awake()
		{
			if (CaymanGenerator._instance != null)
			{
				LogHelper.Error("Only one CaymanGenerator could be active at any time!", new object[0]);
				return;
			}
			CaymanGenerator._instance = this;
			this._actions = new Action[]
			{
				new Action(this.OnTimeToWalk),
				new Action(this.OnTimeToSwim),
				new Action(this.OnTimeToJump)
			};
			this._walkingPivots = base.transform.GetChild(0);
			Dictionary<string, string> dictionary = ((PhotonConnectionFactory.Instance.Profile != null) ? PhotonConnectionFactory.Instance.Profile.Settings : new Dictionary<string, string>());
			for (int i = 0; i < this._settings.Length; i++)
			{
				CaymanGenerator.ActivitySettings activitySettings = this._settings[i];
				float num = ((!dictionary.ContainsKey(activitySettings.LblCooldown)) ? ((float)activitySettings.Cooldown) : Math3d.ParseFloat(dictionary[activitySettings.LblCooldown]));
				if (num < (float)activitySettings.ChanceCooldown)
				{
					num = (float)activitySettings.ChanceCooldown;
				}
				if (num > 0f)
				{
					this._timers.AddTimer((CaymanGenerator.Activity)i, num, this._actions[i], false);
				}
			}
		}

		private void Update()
		{
			this._timers.Update(Time.deltaTime);
		}

		private void OnTimeToWalk()
		{
			CaymanGenerator.Activity activity = CaymanGenerator.Activity.WALKING;
			int num = this.TryToActivate(activity);
			if (num != -1)
			{
				CaymanGenerator.ActivitySettings activitySettings = this._settings[num];
				List<Transform> list = new List<Transform>();
				if (GameFactory.Player != null)
				{
					for (int i = 0; i < this._walkingPivots.childCount; i++)
					{
						Transform child = this._walkingPivots.GetChild(i);
						Vector3 vector = Math3d.ProjectOXZ(child.position - GameFactory.Player.transform.position);
						float magnitude = vector.magnitude;
						if (magnitude > activitySettings.MinPlayerDistance && magnitude < activitySettings.MaxPlayerDistance)
						{
							Vector3 vector2 = Math3d.ProjectOXZ(GameFactory.Player.CameraController.Camera.transform.forward);
							float num2 = Vector3.Angle(vector2, vector);
							if (num2 > 60f && !GameFactory.Player.IsAnyPlayerCloseEnough(child.position, this.PLAYER_AVOIDANCE_DISTANCE, false))
							{
								list.Add(child);
							}
						}
					}
				}
				if (list.Count > 0)
				{
					int num3 = Random.Range(0, list.Count);
					Transform transform = list[num3];
					CaymanWalking caymanWalking = Object.Instantiate<CaymanActivity>(activitySettings.Prefab) as CaymanWalking;
					caymanWalking.Init(transform);
					this._timers.AddTimer(activity, (float)activitySettings.Cooldown, this._actions[num], false);
				}
				else
				{
					this._timers.AddTimer(activity, (float)activitySettings.ChanceCooldown, this._actions[num], false);
				}
			}
		}

		private bool IsAllPointDeepEnough(Vector3 from, Vector3 dir, float dist)
		{
			while (dist > 1f)
			{
				Vector3 vector = from + dir * dist;
				if (Init3D.SceneSettings.HeightMap.GetBottomHeight(new Vector3f(vector)) > this.SWIM_ENOUGH_DEEP)
				{
					return false;
				}
				dist -= 1f;
			}
			return true;
		}

		private void OnTimeToSwim()
		{
			CaymanGenerator.Activity activity = CaymanGenerator.Activity.SWIMMING;
			int num = this.TryToActivate(activity);
			if (num != -1 && GameFactory.Player != null)
			{
				CaymanGenerator.ActivitySettings activitySettings = this._settings[num];
				if (!GameFactory.Player.IsActiveSailing)
				{
					Transform transform = GameFactory.Player.CameraController.Camera.transform;
					Vector3 normalized = Math3d.ProjectOXZ(transform.forward).normalized;
					float num2 = Random.Range(activitySettings.MinPlayerDistance, activitySettings.MaxPlayerDistance);
					Vector3 vector = Math3d.ProjectOXZ(transform.position) + normalized * num2;
					int num3 = ((Random.Range(0, 2) != 0) ? (-90) : 90);
					float y = transform.eulerAngles.y;
					float num4 = y + (float)num3 + Random.Range(-this.SWIMMING_DIR_MAX_ANGLE, this.SWIMMING_DIR_MAX_ANGLE);
					float swimming_MOVE_MIN = this.SWIMMING_MOVE_MIN;
					Vector3 vector2 = Quaternion.AngleAxis(num4, Vector3.up) * (Vector3.forward * this.SWIMMING_MOVE_MIN);
					Vector3 vector3 = vector + vector2;
					if (!GameFactory.Player.IsAnyPlayerCloseEnough(vector3, this.PLAYER_AVOIDANCE_DISTANCE, false))
					{
						float bottomHeight = Init3D.SceneSettings.HeightMap.GetBottomHeight(new Vector3f(vector3));
						if (bottomHeight < this.SPAWN_MIN_DEEP)
						{
							float num5 = num4 + 180f;
							float num6 = Random.Range(this.SWIMMING_MOVE_MIN, this.SWIMMING_MOVE_MAX);
							float num7 = swimming_MOVE_MIN + num6;
							Vector3 vector4 = Quaternion.AngleAxis(num5, Vector3.up) * (Vector3.forward * num6);
							Vector3 vector5 = vector + vector4;
							float bottomHeight2 = Init3D.SceneSettings.HeightMap.GetBottomHeight(new Vector3f(vector5));
							if (bottomHeight2 < this.SPAWN_MIN_DEEP && this.IsAllPointDeepEnough(vector3, vector4.normalized, num7 - 1f))
							{
								CaymanSwimming caymanSwimming = activitySettings.Prefab as CaymanSwimming;
								if (caymanSwimming != null)
								{
									this._lastSwimmingCayman = Object.Instantiate<CaymanSwimming>(caymanSwimming);
									this._lastSwimmingCayman.Init(vector3, vector5, num5, this.SPAWN_MIN_DEEP);
									this._timers.AddTimer(CaymanGenerator.Activity.SCARE_CHECK, 2f, new Action(this.OnScareCheck), true);
									this._timers.AddTimer(activity, (float)activitySettings.Cooldown, this._actions[num], false);
									return;
								}
							}
						}
					}
				}
				this._timers.AddTimer(activity, (float)activitySettings.ChanceCooldown, this._actions[num], false);
			}
		}

		private void OnScareCheck()
		{
			if (this._lastSwimmingCayman != null)
			{
				if (GameFactory.Player.IsAnyTackleCloseEnough(this._lastSwimmingCayman.transform.position, this._lastSwimmingCayman.ScareDistance))
				{
					this.OnScareAction();
				}
			}
			else
			{
				this._timers.RemoveTimer(CaymanGenerator.Activity.SCARE_CHECK);
			}
		}

		private void OnTimeToJump()
		{
			int num = this.TryToActivate(CaymanGenerator.Activity.JUMPING);
			if (num != -1)
			{
				this._isJumpReady = true;
			}
		}

		public void OnTackleTakeWater(Vector3 pos)
		{
			if (this._lastSwimmingCayman != null && (this._lastSwimmingCayman.transform.position - pos).magnitude < this._lastSwimmingCayman.ScareDistance)
			{
				this._lastSwimmingCayman.Scare();
			}
		}

		public void OnScareAction()
		{
			if (this._lastSwimmingCayman != null)
			{
				this._timers.RemoveTimer(CaymanGenerator.Activity.SCARE_CHECK);
				this._lastSwimmingCayman.Scare();
			}
		}

		public void OnFishCaugh()
		{
			if (this._isJumpReady)
			{
				CaymanGenerator.Activity activity = CaymanGenerator.Activity.JUMPING;
				CaymanGenerator.ActivitySettings activitySettings = this._settings[(int)((byte)activity)];
				Transform transform = GameFactory.Player.CameraController.Camera.transform;
				float num = Random.Range(-this.JUMPING_DIR_MAX_ANGLE, this.JUMPING_DIR_MAX_ANGLE);
				float y = transform.eulerAngles.y;
				float num2 = y + num;
				Vector3 vector = Quaternion.AngleAxis(num2, Vector3.up) * (Vector3.forward * this.JUMPING_DIST_TO_TACKLE);
				Vector3 position = GameFactory.Player.Tackle.Position;
				position..ctor(position.x, 0f, position.z);
				Vector3 vector2 = position + vector;
				float bottomHeight = Init3D.SceneSettings.HeightMap.GetBottomHeight(new Vector3f(vector2));
				CaymanJumping caymanJumping = activitySettings.Prefab as CaymanJumping;
				if (bottomHeight < caymanJumping.InitialY && !GameFactory.Player.IsAnyPlayerCloseEnough(vector2, this.PLAYER_AVOIDANCE_DISTANCE, true))
				{
					this._isJumpReady = false;
					CaymanJumping caymanJumping2 = Object.Instantiate<CaymanJumping>(caymanJumping);
					caymanJumping2.Init(vector2, num2 + 180f);
					this._timers.AddTimer(activity, (float)activitySettings.ChanceCooldown, this._actions[(int)((byte)activity)], false);
				}
			}
		}

		private int TryToActivate(CaymanGenerator.Activity activity)
		{
			CaymanGenerator.ActivitySettings activitySettings = this._settings[(int)activity];
			float num = Random.Range(0f, 1f);
			if (num < activitySettings.ActivationChance)
			{
				return (int)activity;
			}
			this._timers.AddTimer(activity, (float)activitySettings.ChanceCooldown, this._actions[(int)activity], false);
			return -1;
		}

		public void OnGotoGlobalMap()
		{
			this._timers.Clear();
			Dictionary<string, string> settings = PhotonConnectionFactory.Instance.Profile.Settings;
			for (int i = 0; i < this._settings.Length; i++)
			{
				settings.Remove(this._settings[i].LblCooldown);
			}
			this._isCleared = true;
		}

		private void OnDestroy()
		{
			if (!this._isCleared && PhotonConnectionFactory.Instance.Profile != null)
			{
				Dictionary<string, string> settings = PhotonConnectionFactory.Instance.Profile.Settings;
				for (int i = 0; i < this._settings.Length; i++)
				{
					CaymanGenerator.ActivitySettings activitySettings = this._settings[i];
					float? timerLeftTime = this._timers.GetTimerLeftTime((CaymanGenerator.Activity)i);
					if (timerLeftTime != null)
					{
						settings[activitySettings.LblCooldown] = timerLeftTime.Value.ToString();
					}
				}
				if (this._isJumpReady)
				{
					CaymanGenerator.ActivitySettings activitySettings2 = this._settings[2];
					settings[activitySettings2.LblCooldown] = activitySettings2.ChanceCooldown.ToString();
				}
			}
			CaymanGenerator._instance = null;
		}

		[SerializeField]
		private float PLAYER_AVOIDANCE_DISTANCE = 10f;

		[SerializeField]
		private float SWIMMING_DIR_MAX_ANGLE = 45f;

		[SerializeField]
		private float SWIMMING_MOVE_MIN = 5f;

		[SerializeField]
		private float SWIMMING_MOVE_MAX = 20f;

		[SerializeField]
		private float SPAWN_MIN_DEEP = -1.5f;

		[SerializeField]
		private float SWIM_ENOUGH_DEEP = -0.5f;

		[SerializeField]
		private float JUMPING_DIR_MAX_ANGLE = 15f;

		[Tooltip("Max angle between player's dir on tackle and rod tip's dir on tackle")]
		[SerializeField]
		private float _jumpingDirGoodAngle = 10f;

		[SerializeField]
		private float JUMPING_DIST_TO_TACKLE = 2f;

		private const byte SCARE_CHECK_DELAY = 2;

		private Action[] _actions;

		[SerializeField]
		private bool _isDebug;

		[SerializeField]
		private bool _skipLookTest;

		[SerializeField]
		private CaymanGenerator.ActivitySettings[] _settings;

		private TimerCore<CaymanGenerator.Activity> _timers = new TimerCore<CaymanGenerator.Activity>();

		private static CaymanGenerator _instance;

		private Transform _walkingPivots;

		private bool _isJumpReady;

		private CaymanSwimming _lastSwimmingCayman;

		private bool _isCleared;

		private enum Activity
		{
			WALKING,
			SWIMMING,
			JUMPING,
			SCARE_CHECK
		}

		[Serializable]
		private class ActivitySettings
		{
			public int Cooldown
			{
				get
				{
					return this._srvCooldown * 60;
				}
			}

			public float ActivationChance
			{
				get
				{
					return this._srvActivationChance / 100f;
				}
				set
				{
					this._srvActivationChance = value;
				}
			}

			public int ChanceCooldown
			{
				get
				{
					return this._srvChanceCooldown * 60;
				}
			}

			public string LblCooldown
			{
				get
				{
					return this._lblCooldown;
				}
			}

			public CaymanActivity Prefab
			{
				get
				{
					return this._prefab;
				}
			}

			public float MinPlayerDistance
			{
				get
				{
					return this._minPlayerDistance;
				}
			}

			public float MaxPlayerDistance
			{
				get
				{
					return this._maxPlayerDistance;
				}
			}

			[Tooltip("Debug property. Delay in seconds between actions")]
			[SerializeField]
			private int _cooldown;

			[Tooltip("Game property. Delay in minutes between actions")]
			[SerializeField]
			private int _srvCooldown;

			[Tooltip("Debug property. Probability (%) to activate when cooldown will expired")]
			[SerializeField]
			private float _activationChance;

			[Tooltip("Game property. Probability (%) to activate when cooldown will expired")]
			[SerializeField]
			private float _srvActivationChance;

			[Tooltip("Debug property. How long (seconds) wait till the next activation try")]
			[SerializeField]
			private int _chanceCooldown;

			[Tooltip("Game property. How long (minutes) wait till the next activation try")]
			[SerializeField]
			private int _srvChanceCooldown;

			[SerializeField]
			private string _lblCooldown;

			[SerializeField]
			private CaymanActivity _prefab;

			[SerializeField]
			private float _minPlayerDistance;

			[SerializeField]
			private float _maxPlayerDistance;
		}
	}
}
