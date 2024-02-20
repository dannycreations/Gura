using System;
using System.Reflection;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public class SRIADebugger : MonoBehaviour
	{
		private void Update()
		{
			if (this._AdapterImpl == null)
			{
				return;
			}
			this.debugText1.text = string.Concat(new object[]
			{
				"ctSK: ",
				this.GetInternalStateFieldValue("contentPanelSkippedInsetDueToVirtualization"),
				"\nctRealSz: ",
				this.GetInternalStateFieldValue("contentPanelSize"),
				"\nctVrtSz: ",
				this.GetInternalStateFieldValue("contentPanelVirtualSize"),
				"\nctVrtIns: ",
				this.GetInternalStatePropertyValue("ContentPanelVirtualInsetFromViewportStart"),
				"\nvisCount: ",
				this.GetPropertyValue("VisibleItemsCount"),
				"\nskipTriggerCompute: ",
				this.GetFieldValue("_SkipComputeVisibilityInUpdateOrOnScroll")
			});
		}

		internal void InitWithAdapter(ISRIA adapterImpl)
		{
			if (this._AdapterImpl != null && !this.allowReinitializationWithOtherAdapter)
			{
				return;
			}
			this._AdapterImpl = adapterImpl;
			Button button;
			base.transform.GetComponentAtPath("ComputeNowButton", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				this.Call("ComputeVisibilityForCurrentPosition", new object[] { true, true, false });
			});
			base.transform.GetComponentAtPath("ComputeNowButton_PlusDelta", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				this.Call("ComputeVisibilityForCurrentPosition", new object[] { true, true, false, 0.1f });
			});
			base.transform.GetComponentAtPath("ComputeNowButton_MinusDelta", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				this.Call("ComputeVisibilityForCurrentPosition", new object[] { true, true, false, -0.1f });
			});
			base.transform.GetComponentAtPath("CorrectNowButton", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				this.Call("CorrectPositionsOfVisibleItems", new object[] { true });
			});
		}

		private object GetFieldValue(string field)
		{
			FieldInfo field2 = this.GetBaseType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return field2.GetValue(this._AdapterImpl);
		}

		private object GetPropertyValue(string prop)
		{
			PropertyInfo property = this.GetBaseType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return property.GetValue(this._AdapterImpl, null);
		}

		private object GetInternalStateFieldValue(string field)
		{
			object fieldValue = this.GetFieldValue("_InternalState");
			Type internalStateBaseType = this.GetInternalStateBaseType(fieldValue);
			FieldInfo field2 = internalStateBaseType.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return field2.GetValue(fieldValue);
		}

		private object GetInternalStatePropertyValue(string prop)
		{
			object fieldValue = this.GetFieldValue("_InternalState");
			Type internalStateBaseType = this.GetInternalStateBaseType(fieldValue);
			PropertyInfo property = internalStateBaseType.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return property.GetValue(fieldValue, null);
		}

		private Type GetBaseType()
		{
			Type type = this._AdapterImpl.GetType();
			while (!type.Name.Contains("SRIA") || !type.IsGenericType)
			{
				type = type.BaseType;
			}
			return type;
		}

		private Type GetInternalStateBaseType(object internalState)
		{
			Type type = internalState.GetType();
			while (!type.Name.Contains("InternalStateGeneric") || !type.IsGenericType)
			{
				type = type.BaseType;
			}
			return type;
		}

		private void Call(string methodName, params object[] parameters)
		{
			if (this._AdapterImpl == null)
			{
				return;
			}
			Type baseType = this.GetBaseType();
			MethodInfo method = baseType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, DotNETCoreCompat.ConvertAllToArray<object, Type>(parameters, (object p) => p.GetType()), null);
			method.Invoke(this._AdapterImpl, parameters);
		}

		private ISRIA _AdapterImpl;

		public Text debugText1;

		public Text debugText2;

		public Text debugText3;

		public Text debugText4;

		public bool allowReinitializationWithOtherAdapter;

		private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	}
}
