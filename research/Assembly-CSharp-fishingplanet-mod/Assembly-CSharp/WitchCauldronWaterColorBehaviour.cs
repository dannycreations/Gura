using System;
using UnityEngine;

public class WitchCauldronWaterColorBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.waterMaterial = animator.gameObject.transform.Find(this.WaterObjectName).GetComponent<MeshRenderer>().material;
		this.oldColor = this.waterMaterial.GetColor("_Color");
		if (this.ColorList == null || this.ColorList.Length == 0)
		{
			this.newColor = Color.white;
		}
		else if (this.ColorList.Length == 1)
		{
			this.newColor = this.ColorList[0];
		}
		else
		{
			this.newColor = this.ColorList[Random.Range(0, this.ColorList.Length)];
		}
		this.transitionTimeStamp = Time.time;
		Debug.LogWarning(string.Concat(new object[] { "WitchCauldronWaterColorBehaviour.OnStateEnter : oldColor = ", this.oldColor, " newColor = ", this.newColor }));
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.waterMaterial.SetColor("_Color", Color.Lerp(this.oldColor, this.newColor, Mathf.SmoothStep(0f, 1f, (Time.time - this.transitionTimeStamp) / this.ColorChangeTime)));
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	[SerializeField]
	private Color[] ColorList;

	[SerializeField]
	private float ColorChangeTime = 1f;

	[SerializeField]
	private string WaterObjectName = "Water";

	private Material waterMaterial;

	private Color oldColor;

	private Color newColor;

	private float transitionTimeStamp;
}
