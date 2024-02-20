using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffect
	{
		public VolumeEffect(AmplifyColorBase effect)
		{
			this.gameObject = effect;
			this.components = new List<VolumeEffectComponent>();
		}

		public static VolumeEffect BlendValuesToVolumeEffect(VolumeEffectFlags flags, VolumeEffect volume1, VolumeEffect volume2, float blend)
		{
			VolumeEffect volumeEffect = new VolumeEffect(volume1.gameObject);
			using (List<VolumeEffectComponentFlags>.Enumerator enumerator = flags.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VolumeEffectComponentFlags compFlags = enumerator.Current;
					if (compFlags.blendFlag)
					{
						VolumeEffectComponent volumeEffectComponent = volume1.components.Find((VolumeEffectComponent s) => s.componentName == compFlags.componentName);
						VolumeEffectComponent volumeEffectComponent2 = volume2.components.Find((VolumeEffectComponent s) => s.componentName == compFlags.componentName);
						if (volumeEffectComponent != null && volumeEffectComponent2 != null)
						{
							VolumeEffectComponent volumeEffectComponent3 = new VolumeEffectComponent(volumeEffectComponent.componentName);
							using (List<VolumeEffectFieldFlags>.Enumerator enumerator2 = compFlags.componentFields.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									VolumeEffectFieldFlags fieldFlags = enumerator2.Current;
									if (fieldFlags.blendFlag)
									{
										VolumeEffectField volumeEffectField = volumeEffectComponent.fields.Find((VolumeEffectField s) => s.fieldName == fieldFlags.fieldName);
										VolumeEffectField volumeEffectField2 = volumeEffectComponent2.fields.Find((VolumeEffectField s) => s.fieldName == fieldFlags.fieldName);
										if (volumeEffectField != null && volumeEffectField2 != null)
										{
											VolumeEffectField volumeEffectField3 = new VolumeEffectField(volumeEffectField.fieldName, volumeEffectField.fieldType);
											string fieldType = volumeEffectField3.fieldType;
											if (fieldType != null)
											{
												if (!(fieldType == "System.Single"))
												{
													if (!(fieldType == "System.Boolean"))
													{
														if (!(fieldType == "UnityEngine.Vector2"))
														{
															if (!(fieldType == "UnityEngine.Vector3"))
															{
																if (!(fieldType == "UnityEngine.Vector4"))
																{
																	if (fieldType == "UnityEngine.Color")
																	{
																		volumeEffectField3.valueColor = Color.Lerp(volumeEffectField.valueColor, volumeEffectField2.valueColor, blend);
																	}
																}
																else
																{
																	volumeEffectField3.valueVector4 = Vector4.Lerp(volumeEffectField.valueVector4, volumeEffectField2.valueVector4, blend);
																}
															}
															else
															{
																volumeEffectField3.valueVector3 = Vector3.Lerp(volumeEffectField.valueVector3, volumeEffectField2.valueVector3, blend);
															}
														}
														else
														{
															volumeEffectField3.valueVector2 = Vector2.Lerp(volumeEffectField.valueVector2, volumeEffectField2.valueVector2, blend);
														}
													}
													else
													{
														volumeEffectField3.valueBoolean = volumeEffectField2.valueBoolean;
													}
												}
												else
												{
													volumeEffectField3.valueSingle = Mathf.Lerp(volumeEffectField.valueSingle, volumeEffectField2.valueSingle, blend);
												}
											}
											volumeEffectComponent3.fields.Add(volumeEffectField3);
										}
									}
								}
							}
							volumeEffect.components.Add(volumeEffectComponent3);
						}
					}
				}
			}
			return volumeEffect;
		}

		public VolumeEffectComponent AddComponent(Component c, VolumeEffectComponentFlags compFlags)
		{
			if (compFlags == null)
			{
				VolumeEffectComponent volumeEffectComponent = new VolumeEffectComponent(c.GetType() + string.Empty);
				this.components.Add(volumeEffectComponent);
				return volumeEffectComponent;
			}
			VolumeEffectComponent volumeEffectComponent2;
			if ((volumeEffectComponent2 = this.components.Find((VolumeEffectComponent s) => s.componentName == c.GetType() + string.Empty)) != null)
			{
				volumeEffectComponent2.UpdateComponent(c, compFlags);
				return volumeEffectComponent2;
			}
			VolumeEffectComponent volumeEffectComponent3 = new VolumeEffectComponent(c, compFlags);
			this.components.Add(volumeEffectComponent3);
			return volumeEffectComponent3;
		}

		public void RemoveEffectComponent(VolumeEffectComponent comp)
		{
			this.components.Remove(comp);
		}

		public void UpdateVolume()
		{
			if (this.gameObject == null)
			{
				return;
			}
			VolumeEffectFlags effectFlags = this.gameObject.EffectFlags;
			foreach (VolumeEffectComponentFlags volumeEffectComponentFlags in effectFlags.components)
			{
				if (volumeEffectComponentFlags.blendFlag)
				{
					Component component = this.gameObject.GetComponent(volumeEffectComponentFlags.componentName);
					if (component != null)
					{
						this.AddComponent(component, volumeEffectComponentFlags);
					}
				}
			}
		}

		public void SetValues(AmplifyColorBase targetColor)
		{
			VolumeEffectFlags effectFlags = targetColor.EffectFlags;
			GameObject gameObject = targetColor.gameObject;
			using (List<VolumeEffectComponentFlags>.Enumerator enumerator = effectFlags.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VolumeEffectComponentFlags compFlags = enumerator.Current;
					if (compFlags.blendFlag)
					{
						Component component = gameObject.GetComponent(compFlags.componentName);
						VolumeEffectComponent volumeEffectComponent = this.components.Find((VolumeEffectComponent s) => s.componentName == compFlags.componentName);
						if (!(component == null) && volumeEffectComponent != null)
						{
							using (List<VolumeEffectFieldFlags>.Enumerator enumerator2 = compFlags.componentFields.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									VolumeEffectFieldFlags fieldFlags = enumerator2.Current;
									if (fieldFlags.blendFlag)
									{
										FieldInfo field = component.GetType().GetField(fieldFlags.fieldName);
										VolumeEffectField volumeEffectField = volumeEffectComponent.fields.Find((VolumeEffectField s) => s.fieldName == fieldFlags.fieldName);
										if (field != null && volumeEffectField != null)
										{
											string fullName = field.FieldType.FullName;
											if (fullName != null)
											{
												if (!(fullName == "System.Single"))
												{
													if (!(fullName == "System.Boolean"))
													{
														if (!(fullName == "UnityEngine.Vector2"))
														{
															if (!(fullName == "UnityEngine.Vector3"))
															{
																if (!(fullName == "UnityEngine.Vector4"))
																{
																	if (fullName == "UnityEngine.Color")
																	{
																		field.SetValue(component, volumeEffectField.valueColor);
																	}
																}
																else
																{
																	field.SetValue(component, volumeEffectField.valueVector4);
																}
															}
															else
															{
																field.SetValue(component, volumeEffectField.valueVector3);
															}
														}
														else
														{
															field.SetValue(component, volumeEffectField.valueVector2);
														}
													}
													else
													{
														field.SetValue(component, volumeEffectField.valueBoolean);
													}
												}
												else
												{
													field.SetValue(component, volumeEffectField.valueSingle);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public void BlendValues(AmplifyColorBase targetColor, VolumeEffect other, float blendAmount)
		{
			VolumeEffectFlags effectFlags = targetColor.EffectFlags;
			GameObject gameObject = targetColor.gameObject;
			using (List<VolumeEffectComponentFlags>.Enumerator enumerator = effectFlags.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VolumeEffectComponentFlags compFlags = enumerator.Current;
					if (compFlags.blendFlag)
					{
						Component component = gameObject.GetComponent(compFlags.componentName);
						VolumeEffectComponent volumeEffectComponent = this.components.Find((VolumeEffectComponent s) => s.componentName == compFlags.componentName);
						VolumeEffectComponent volumeEffectComponent2 = other.components.Find((VolumeEffectComponent s) => s.componentName == compFlags.componentName);
						if (!(component == null) && volumeEffectComponent != null && volumeEffectComponent2 != null)
						{
							using (List<VolumeEffectFieldFlags>.Enumerator enumerator2 = compFlags.componentFields.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									VolumeEffectFieldFlags fieldFlags = enumerator2.Current;
									if (fieldFlags.blendFlag)
									{
										FieldInfo field = component.GetType().GetField(fieldFlags.fieldName);
										VolumeEffectField volumeEffectField = volumeEffectComponent.fields.Find((VolumeEffectField s) => s.fieldName == fieldFlags.fieldName);
										VolumeEffectField volumeEffectField2 = volumeEffectComponent2.fields.Find((VolumeEffectField s) => s.fieldName == fieldFlags.fieldName);
										if (field != null && volumeEffectField != null && volumeEffectField2 != null)
										{
											string fullName = field.FieldType.FullName;
											if (fullName != null)
											{
												if (!(fullName == "System.Single"))
												{
													if (!(fullName == "System.Boolean"))
													{
														if (!(fullName == "UnityEngine.Vector2"))
														{
															if (!(fullName == "UnityEngine.Vector3"))
															{
																if (!(fullName == "UnityEngine.Vector4"))
																{
																	if (fullName == "UnityEngine.Color")
																	{
																		field.SetValue(component, Color.Lerp(volumeEffectField.valueColor, volumeEffectField2.valueColor, blendAmount));
																	}
																}
																else
																{
																	field.SetValue(component, Vector4.Lerp(volumeEffectField.valueVector4, volumeEffectField2.valueVector4, blendAmount));
																}
															}
															else
															{
																field.SetValue(component, Vector3.Lerp(volumeEffectField.valueVector3, volumeEffectField2.valueVector3, blendAmount));
															}
														}
														else
														{
															field.SetValue(component, Vector2.Lerp(volumeEffectField.valueVector2, volumeEffectField2.valueVector2, blendAmount));
														}
													}
													else
													{
														field.SetValue(component, volumeEffectField2.valueBoolean);
													}
												}
												else
												{
													field.SetValue(component, Mathf.Lerp(volumeEffectField.valueSingle, volumeEffectField2.valueSingle, blendAmount));
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public VolumeEffectComponent GetEffectComponent(string compName)
		{
			return this.components.Find((VolumeEffectComponent s) => s.componentName == compName);
		}

		public static Component[] ListAcceptableComponents(AmplifyColorBase go)
		{
			if (go == null)
			{
				return new Component[0];
			}
			Component[] array = go.GetComponents(typeof(Component));
			return array.Where((Component comp) => comp != null && (!(comp.GetType() + string.Empty).StartsWith("UnityEngine.") && comp.GetType() != typeof(AmplifyColorBase))).ToArray<Component>();
		}

		public string[] GetComponentNames()
		{
			return this.components.Select((VolumeEffectComponent r) => r.componentName).ToArray<string>();
		}

		public AmplifyColorBase gameObject;

		public List<VolumeEffectComponent> components;
	}
}
