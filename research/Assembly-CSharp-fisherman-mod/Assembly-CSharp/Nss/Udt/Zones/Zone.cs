using System;
using UnityEngine;

namespace Nss.Udt.Zones
{
	public class Zone : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (this.triggerType == Zone.ZoneTriggerTypes.SendMessage)
			{
				if (this.messageReceiver == null)
				{
					Debug.LogError(string.Format("UDT: Zone configured to send messages must have a receiver linked. [Zone: '{0}']", base.gameObject.name));
					return;
				}
				if (!string.IsNullOrEmpty(this.MessageEnterHandler))
				{
					this.messageReceiver.SendMessage(this.MessageEnterHandler, other, (!this.RequireReceivers) ? 1 : 0);
				}
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (this.triggerType == Zone.ZoneTriggerTypes.SendMessage)
			{
				if (this.messageReceiver == null)
				{
					Debug.LogError(string.Format("UDT: Zone configured to send messages must have a receiver linked. [Zone: '{0}']", base.gameObject.name));
					return;
				}
				if (!string.IsNullOrEmpty(this.MessageStayHandler))
				{
					this.messageReceiver.SendMessage(this.MessageStayHandler, other, (!this.RequireReceivers) ? 1 : 0);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (this.triggerType == Zone.ZoneTriggerTypes.SendMessage)
			{
				if (this.messageReceiver == null)
				{
					Debug.LogError(string.Format("UDT: Zone configured to send messages must have a receiver linked. [Zone: '{0}']", base.gameObject.name));
					return;
				}
				if (!string.IsNullOrEmpty(this.MessageExitHandler))
				{
					this.messageReceiver.SendMessage(this.MessageExitHandler, other, (!this.RequireReceivers) ? 1 : 0);
				}
			}
		}

		public Zone.ZoneTriggerTypes triggerType;

		public Color color = new Color(0f, 255f, 255f, 0.5f);

		public GameObject messageReceiver;

		public bool RequireReceivers;

		public string MessageEnterHandler;

		public string MessageStayHandler;

		public string MessageExitHandler;

		public enum ZoneTriggerTypes
		{
			Local,
			SendMessage
		}
	}
}
