using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

namespace TPM
{
	internal class TPMSetupIKModel
	{
		public static void CreateIkParts(GameObject rootObject, TPMCharacterModel modelData, Transform ikTarget, Dictionary<CustomizedParts, TPMModelLayerSettings> constructedParts, bool isSetupBodyIk = false)
		{
			TPMFullIKDebugSettings iksettings = TPMCharacterCustomization.Instance.IKSettings;
			TPMModelSettings genderSettings = TPMCharacterCustomization.Instance.GetGenderSettings(TPMCharacterCustomization.Instance.GetGender(modelData.Head));
			GameObject ikpartPrefab = TPMCharacterCustomization.Instance.IKPartPrefab;
			Transform transform = ((constructedParts.Count <= 0) ? null : constructedParts.ElementAt(0).Value.transform.parent);
			foreach (KeyValuePair<CustomizedParts, TPMModelLayerSettings> keyValuePair in constructedParts)
			{
				TPMModelLayerSettings value = keyValuePair.Value;
				CustomizedParts key = keyValuePair.Key;
				GameObject gameObject = TPMSetupIKModel.CreateChild(rootObject, ikpartPrefab, key.ToString());
				value.transform.parent = gameObject.transform;
				value.transform.localPosition = Vector3.zero;
				if (key == MeshBakersController.SKELETON_SRC_PART)
				{
					LookAtIK component = gameObject.GetComponent<LookAtIK>();
					component.solver.target = ikTarget;
					Transform[] array = new Transform[iksettings.spine.Length];
					for (int i = 0; i < iksettings.spine.Length; i++)
					{
						Transform transform2 = value.transform.Find(iksettings.spine[i]);
						if (transform2 != null)
						{
							array[i] = transform2;
						}
						else
						{
							TPMSetupIKModel.LogError("LookAtIK.spines", iksettings.spine[i]);
						}
					}
					component.solver.SetChain(array, value.transform.Find(iksettings.bodyIKSettings.head), null, value.transform);
					component.solver.bodyWeight = 0.15f;
					if (isSetupBodyIk)
					{
						TPMSetupIKModel.SetupBodyIKController(gameObject, value.gameObject, iksettings);
					}
					Animator component2 = gameObject.GetComponent<Animator>();
					component2.avatar = genderSettings.avatar;
					component2.enabled = true;
					component2.Rebind();
				}
			}
			if (transform != null)
			{
				Object.DestroyImmediate(transform.gameObject);
			}
		}

		private static void SetupBodyIKController(GameObject ikPart, GameObject partModel, TPMFullIKDebugSettings ikSettings)
		{
			FullBodyBipedIK component = ikPart.GetComponent<FullBodyBipedIK>();
			component.enabled = true;
			BipedReferences bipedReferences = new BipedReferences();
			bipedReferences.root = partModel.transform;
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.pelvis, ikSettings.bodyIKSettings.pelvis);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.leftThigh, ikSettings.bodyIKSettings.leftThigh);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.leftCalf, ikSettings.bodyIKSettings.leftCalf);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.leftFoot, ikSettings.bodyIKSettings.leftFoot);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.rightThigh, ikSettings.bodyIKSettings.rightThigh);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.rightCalf, ikSettings.bodyIKSettings.rightCalf);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.rightFoot, ikSettings.bodyIKSettings.rightFoot);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.leftUpperArm, ikSettings.bodyIKSettings.leftUpperArm);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.leftForearm, ikSettings.bodyIKSettings.leftForearm);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.leftHand, ikSettings.bodyIKSettings.leftHand);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.rightUpperArm, ikSettings.bodyIKSettings.rightUpperArm);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.rightForearm, ikSettings.bodyIKSettings.rightForearm);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.rightHand, ikSettings.bodyIKSettings.rightHand);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref bipedReferences.head, ikSettings.bodyIKSettings.head);
			TPMSetupIKModel.UpdateTransformProperty(partModel, ref component.solver.rootNode, ikSettings.bodyIKSettings.rootNode);
			bipedReferences.spine = new Transform[ikSettings.spine.Length];
			for (int i = 0; i < ikSettings.spine.Length; i++)
			{
				Transform transform = partModel.transform.Find(ikSettings.spine[i]);
				if (transform != null)
				{
					bipedReferences.spine[i] = transform;
				}
				else
				{
					TPMSetupIKModel.LogError("BodyIKSettings.spine", ikSettings.spine[i]);
				}
			}
			ikPart.GetComponent<GrounderFBBIK>().ik = component;
			component.SetReferences(bipedReferences, component.solver.rootNode);
			ikPart.GetComponent<InteractionSystem>().enabled = true;
		}

		private static GameObject CreateChild(GameObject rootObject, GameObject prefab, string childNodeName)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(prefab);
			gameObject.transform.parent = rootObject.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.name = childNodeName;
			return gameObject;
		}

		private static void UpdateTransformProperty(GameObject obj, ref Transform bone, string transformPath)
		{
			Transform transform = obj.transform.Find(transformPath);
			if (transform != null)
			{
				bone = transform;
			}
			else
			{
				TPMSetupIKModel.LogError("BodyIKSettings", transformPath);
			}
		}

		private static void LogError(string rootPath, string transformPath)
		{
			LogHelper.Error(string.Format("Can't find node {1} for {0}", rootPath, transformPath), new object[0]);
		}
	}
}
