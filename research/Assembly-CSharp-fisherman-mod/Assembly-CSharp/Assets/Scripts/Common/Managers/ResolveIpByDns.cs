using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Common.Managers
{
	public class ResolveIpByDns : MonoBehaviour
	{
		private void Start()
		{
			base.StartCoroutine(this.ResolveDNS());
		}

		private IEnumerator ResolveDNS()
		{
			StaticUserData.ServerConnectionString = StaticUserData.GetDefaultServerConnectionString;
			yield return null;
			yield break;
		}
	}
}
