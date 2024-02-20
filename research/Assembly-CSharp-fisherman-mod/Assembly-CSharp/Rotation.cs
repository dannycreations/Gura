using System;
using UnityEngine;

public class Rotation : MonoBehaviour
{
	private void Update()
	{
		if (this.m_useLocalYAxis)
		{
			base.transform.rotation *= Quaternion.AngleAxis(this.m_angle * Time.deltaTime, base.transform.up);
		}
		else
		{
			base.transform.rotation *= Quaternion.AngleAxis(this.m_angle * Time.deltaTime, this.m_axis);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (this.m_useLocalYAxis)
		{
			Gizmos.DrawRay(base.transform.position, base.transform.up * 10f);
		}
		else
		{
			Gizmos.DrawRay(base.transform.position, this.m_axis * 10f);
		}
	}

	[SerializeField]
	private float m_angle = 5f;

	[SerializeField]
	private bool m_useLocalYAxis = true;

	[SerializeField]
	private Vector3 m_axis = Vector3.up;
}
