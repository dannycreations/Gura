using System;
using System.Collections.Generic;
using ObjectModel;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

namespace TPM
{
	public class MovementTestPlayerSetup : MonoBehaviour
	{
		public void Init(Camera pCamera, InteractionObject interactionObject, Dictionary<CustomizedParts, TPMModelLayerSettings> constructedParts, TPMCharacterModel modelSettings, bool isMovementEnabled, float initialYaw = 0f)
		{
			this.playerCamera = pCamera;
			this._interactionObject = interactionObject;
			TPMSetupIKModel.CreateIkParts(this._ikPartsRoot, modelSettings, this._ikTarget.transform, constructedParts, true);
			this._ikTarget.curYaw = initialYaw;
			Transform transform = constructedParts[MeshBakersController.SKELETON_SRC_PART].transform;
			Transform parent = transform.parent;
			LookAtIK component = parent.GetComponent<LookAtIK>();
			this.solvers.Add(component);
			component.enabled = true;
			this._charMotor = parent.GetComponent<TPMPositionDrivenAnimator>();
			this._charMotor.enabled = isMovementEnabled;
			this._charMotor.Init(false, transform, 0.3f, 1f, 0f, null, true);
			this._animator = parent.GetComponent<Animator>();
			this._leftHand = TransformHelper.FindDeepChild(parent, "locRodLeft");
			this._rightHand = TransformHelper.FindDeepChild(parent, "locRodRight");
			this._interactionSystems = parent.GetComponent<InteractionSystem>();
			this._interactionSystems.enabled = true;
		}

		public void SetupParameters(TPMMecanimBParameter[] bools, BytePair[] bytes, FloatPair[] floats)
		{
			for (int i = 0; i < bools.Length; i++)
			{
				this._animator.SetBool(bools[i].ToString(), true);
			}
			for (int j = 0; j < bytes.Length; j++)
			{
				this._animator.SetInteger(bytes[j].Parameter.ToString(), (int)bytes[j].Value);
			}
			for (int k = 0; k < floats.Length; k++)
			{
				this._animator.SetFloat(floats[k].Parameter.ToString(), floats[k].Value);
			}
		}

		private void ConnectFish(Transform fish)
		{
			this._holdingFish = fish;
			this._fishBase = TransformHelper.FindDeepChild(this._holdingFish, "Aim1");
		}

		private void UpdateFishInHands()
		{
			if (this._fishBase != null)
			{
				float magnitude = (this._rightHand.position - this._leftHand.position).magnitude;
				float num = this._leftHand.position.y - this._rightHand.position.y;
				float num2 = Mathf.Atan(num / magnitude) * 57.29578f;
				this._holdingFish.rotation = Quaternion.Euler(-num2, this._charMotor.transform.rotation.eulerAngles.y - 90f, 0f);
				Vector3 vector = this._fishBase.position - this._holdingFish.transform.position - new Vector3(0f, this._offset, 0f);
				this._holdingFish.position = this._leftHand.position - vector;
			}
		}

		private void LateUpdate()
		{
			for (int i = 0; i < this.solvers.Count; i++)
			{
				this.solvers[i].UpdateSolverExternal();
			}
			this.UpdateFishInHands();
		}

		private void Start()
		{
			Transform transform = this._labelsRoot.Find("pBillboardMessage/textMesh");
			if (transform != null)
			{
				BillboardText component = transform.GetComponent<BillboardText>();
				if (this.playerCamera != null)
				{
					component.cameraToLookAt = this.playerCamera;
				}
				transform.gameObject.SetActive(this.playerCamera && this._showText);
			}
			transform = this._labelsRoot.Find("pBillboardTarget/textMesh");
			if (transform != null)
			{
				BillboardText component2 = transform.GetComponent<BillboardText>();
				if (this.playerCamera != null)
				{
					component2.cameraToLookAt = this.playerCamera;
				}
				transform.gameObject.SetActive(this.playerCamera && this._showText);
			}
			transform = this._labelsRoot.Find("pBillboardLabel/textMesh");
			if (transform != null)
			{
				BillboardText component3 = transform.GetComponent<BillboardText>();
				if (this.playerCamera != null)
				{
					component3.cameraToLookAt = this.playerCamera;
				}
				component3.SetText(this.playerName);
				transform.gameObject.SetActive(this.playerCamera && this._showText);
			}
		}

		private void Update()
		{
		}

		public Camera playerCamera;

		public string playerName;

		[SerializeField]
		private GameObject _ikPartsRoot;

		[SerializeField]
		private Transform _labelsRoot;

		[SerializeField]
		private IKTargetMover _ikTarget;

		[Tooltip("The object to interact to")]
		[SerializeField]
		private InteractionObject _interactionObject;

		[Tooltip("The effectors to interact with")]
		[SerializeField]
		private FullBodyBipedEffector _effector;

		[SerializeField]
		private bool _showText;

		[SerializeField]
		private float _offset = -0.05f;

		private Transform _holdingFish;

		private Transform _fishBase;

		private Transform _leftHand;

		private Transform _rightHand;

		private InteractionSystem _interactionSystems;

		private List<SolverManager> solvers = new List<SolverManager>();

		private TPMPositionDrivenAnimator _charMotor;

		private Animator _animator;
	}
}
