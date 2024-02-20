using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArtMode
{
	public class DebugCharacterController : MonoBehaviour
	{
		public void Init(GameObject prefab)
		{
			this._prefab = prefab;
		}

		private void Start()
		{
			GameFactory.DebugPlayer = base.transform;
			this._curSceneName = SceneManager.GetActiveScene().name;
			foreach (SceneInitPosition sceneInitPosition in this._initPoses)
			{
				if (sceneInitPosition.sceneName == this._curSceneName)
				{
					base.transform.position = sceneInitPosition.initialPos;
					base.transform.rotation = sceneInitPosition.initialRotation;
					break;
				}
			}
		}

		private void Update()
		{
			this.UpdateMovement();
			this.UpdateDynWater();
		}

		private void UpdateMovement()
		{
			float deltaTime = Time.deltaTime;
			float num = ((!Input.GetKey(this._runButton)) ? 1f : this._runK);
			float num2 = Input.GetAxis("Vertical") * deltaTime * this._forwardSpeed * num;
			float num3 = Input.GetAxis("Horizontal") * deltaTime * this._sidewaySpeed * num;
			if (this._isDirectionFromMouse)
			{
				if (num2 != 0f)
				{
					base.transform.position += num2 * Camera.main.transform.forward;
				}
				if (num3 != 0f)
				{
					base.transform.position += num3 * Camera.main.transform.right;
				}
				if (Input.GetKey(this._upButton))
				{
					base.transform.position += new Vector3(0f, this._vSpeed * num * deltaTime, 0f);
				}
				else if (Input.GetKey(this._downButton))
				{
					base.transform.position -= new Vector3(0f, this._vSpeed * num * deltaTime, 0f);
				}
			}
			else
			{
				Vector3 vector;
				vector..ctor(num3, 0f, num2);
				Vector3 vector2 = base.transform.TransformDirection(vector);
				if (Input.GetKey(this._upButton))
				{
					vector2.y += this._vSpeed * num * deltaTime;
				}
				else if (Input.GetKey(this._downButton))
				{
					vector2.y -= this._vSpeed * num * deltaTime;
				}
				else if (vector.magnitude > 0f && this._isDirectionFromMouse)
				{
					vector2.y += Mathf.Sign(Camera.main.transform.forward.y) * Mathf.Sign(vector.z) * this._forwardSpeed * num * deltaTime;
				}
				base.transform.position += vector2;
			}
		}

		private void UpdateDynWater()
		{
			if (GameFactory.Water == null)
			{
				return;
			}
			if (Vector3.Distance(this._currentDynWaterPosition, base.transform.position) > this._distanceChangesDynWater)
			{
				this._currentDynWaterPosition = base.transform.position;
				GameFactory.Water.SetDynWaterPosition(this._currentDynWaterPosition.x, this._currentDynWaterPosition.z);
			}
			GameFactory.Water.PlayerPosition = base.transform.position;
			GameFactory.Water.PlayerForward = base.transform.forward;
		}

		[SerializeField]
		private float _forwardSpeed = 10f;

		[SerializeField]
		private float _sidewaySpeed = 10f;

		[SerializeField]
		private float _vSpeed = 2f;

		[SerializeField]
		private float _runK = 2f;

		[SerializeField]
		private KeyCode _runButton = 304;

		[SerializeField]
		private KeyCode _upButton = 101;

		[SerializeField]
		private KeyCode _downButton = 113;

		[SerializeField]
		private bool _isDirectionFromMouse;

		[SerializeField]
		private float _distanceChangesDynWater = 10f;

		[SerializeField]
		private List<SceneInitPosition> _initPoses;

		private Vector3 _currentDynWaterPosition;

		private string _curSceneName;

		private GameObject _prefab;
	}
}
