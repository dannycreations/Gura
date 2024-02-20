using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InControl;

namespace Assets.Scripts.UI._2D.Common
{
	public class BindingSourceHelper
	{
		public static Dictionary<BindingSourceHelper.BindingType, List<BindingSource>> GetBindings(BindingSourceHelper.BindingType[] bindsForGet, ReadOnlyCollection<BindingSource> bindsSource)
		{
			Dictionary<BindingSourceHelper.BindingType, List<BindingSource>> dictionary = new Dictionary<BindingSourceHelper.BindingType, List<BindingSource>>();
			for (int i = 0; i < bindsForGet.Length; i++)
			{
				dictionary.Add(bindsForGet[i], new List<BindingSource>());
			}
			for (int j = 0; j < bindsSource.Count; j++)
			{
				BindingSource bindingSource = bindsSource[j];
				foreach (BindingSourceHelper.BindingType bindingType in bindsForGet)
				{
					if (BindingSourceHelper.BindingTypesStr[bindingType].Contains(bindingSource.DeviceName))
					{
						dictionary[bindingType].Add(bindingSource);
					}
				}
			}
			return dictionary;
		}

		public static void GetPrimaryAndSecondaryBindingSource(List<BindingSource> binds, out BindingSource primary, out BindingSource secondary)
		{
			List<BindingSource> primaries = (from p in binds
				where p.Number == 1 || p.Number > 2
				orderby p.Number
				select p).ToList<BindingSource>();
			primary = primaries.FirstOrDefault<BindingSource>();
			secondary = (from p in binds
				where p.Number == 2 || (p.Number != 1 && !primaries.Contains(p))
				orderby p.Number
				select p).FirstOrDefault<BindingSource>();
		}

		private static readonly Dictionary<BindingSourceHelper.BindingType, List<string>> BindingTypesStr = new Dictionary<BindingSourceHelper.BindingType, List<string>>
		{
			{
				BindingSourceHelper.BindingType.Keyboard,
				new List<string> { "Keyboard" }
			},
			{
				BindingSourceHelper.BindingType.Mouse,
				new List<string> { "Mouse" }
			},
			{
				BindingSourceHelper.BindingType.Controller,
				new List<string>
				{
					"Controller",
					string.Empty
				}
			}
		};

		public enum BindingType
		{
			Keyboard,
			Mouse,
			Controller
		}
	}
}
