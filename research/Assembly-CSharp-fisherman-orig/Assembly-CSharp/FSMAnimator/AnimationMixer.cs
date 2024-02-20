using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSMAnimator
{
	[RequireComponent(typeof(Animation))]
	public class AnimationMixer<TFloat, TInt, TBool> where TFloat : struct, IConvertible where TInt : struct, IConvertible where TBool : struct, IConvertible
	{
		public void Init(AnimProperties<TFloat, TInt, TBool> properties, Animation mainBank)
		{
			this._properties = properties;
			this._banks.Add(mainBank);
		}

		public void RegisterAdditionalBank(Animation bank)
		{
			if (this._banks.Count > 1)
			{
				this._banks[1] = bank;
			}
			else
			{
				this._banks.Add(bank);
			}
		}

		public void UnregisterAdditionBank()
		{
			if (this._banks.Count > 1)
			{
				this._banks.RemoveAt(1);
			}
		}

		public void RegisterBlendTree(string name, BlendTree1D<TFloat> tree, TFloat parameter)
		{
			if (this._trees.ContainsKey(name))
			{
				LogHelper.Error("Can't register Blend Tree {0} - already present!", new object[] { name });
			}
			else if (!this._properties.ValidateProperty(parameter))
			{
				LogHelper.Error("Can't find parameter {0} to set for blend tree {1}", new object[] { parameter, name });
			}
			else
			{
				this._trees[name] = tree;
				tree.SetProperties(parameter);
			}
		}

		public void RegisterBlendTree(string name, BlendTree2DSimple<TFloat> tree, TFloat parameter1, TFloat parameter2)
		{
			if (this._trees.ContainsKey(name))
			{
				LogHelper.Error("Can't register Blend Tree {0} - already present!", new object[] { name });
			}
			else
			{
				bool flag = this._properties.ValidateProperty(parameter1);
				bool flag2 = this._properties.ValidateProperty(parameter2);
				if (!flag || !flag2)
				{
					if (!flag)
					{
						LogHelper.Error("Can't find parameter {0} to set for blend tree {1}", new object[] { parameter1, name });
					}
					if (!flag2)
					{
						LogHelper.Error("Can't find parameter {0} to set for blend tree {1}", new object[] { parameter2, name });
					}
					LogHelper.Error("Blend tree {0} was not registered", new object[] { name });
				}
				else
				{
					this._trees[name] = tree;
					tree.SetProperties(parameter1, parameter2);
				}
			}
		}

		public void PlaySequence(IAnimationSequence<TFloat> sequence)
		{
			this.StopCurrentState();
			this._curSequence = sequence;
			this._curPropertyNames = sequence.Properties;
		}

		public void PlayAnimation(string clipName, float speed = 1f, float blendTime = 0f)
		{
			this.StopCurrentState();
			this._lastClip = clipName;
			for (int i = 0; i < this._banks.Count; i++)
			{
				this._banks[i][clipName].speed = speed;
				this._banks[i].CrossFade(clipName, blendTime);
			}
		}

		public void PlayBlendTree(string name)
		{
			if (this._currentTreeName == name)
			{
				return;
			}
			if (!this._trees.ContainsKey(name))
			{
				LogHelper.Error("Blend tree {0} was not registered", new object[] { name });
			}
			else
			{
				this.StopCurrentState();
				BlendTree<TFloat> blendTree = this._trees[name];
				this._currentTreeName = name;
				this._currentClips = blendTree.Clips;
				this._curPropertyNames = blendTree.Properties;
				if (this._curPropertyNames.Length != this._curProperties.Length)
				{
					this._curProperties = new float[this._curPropertyNames.Length];
				}
				for (int i = 0; i < this._currentClips.Length; i++)
				{
					this.AddBlendClip(this._currentClips[i], 0f, 0f);
				}
			}
		}

		private void StopCurrentState()
		{
			if (this._lastClip != null)
			{
				BlendTree<TFloat> blendTree = this._trees[this._currentTreeName];
				this.AddBlendClip(this._lastClip, 0f, 0.1f);
				this._lastClip = null;
			}
			else if (this._currentTreeName != null)
			{
				BlendTree<TFloat> blendTree2 = this._trees[this._currentTreeName];
				BlendTree<TFloat>.UpdateResult[] lastResult = blendTree2.LastResult;
				for (int i = 0; i < lastResult.Length; i++)
				{
					BlendClip blendClip = this._blendsMap[lastResult[i].Clip];
					blendClip.Change(0f, blendTree2.BlendTime, true);
				}
				this._currentTreeName = null;
				this._currentClips = null;
				this._curPropertyNames = null;
			}
			else
			{
				this._curSequence.Stop();
				this._curSequence = null;
				this._curPropertyNames = null;
			}
		}

		private void AddBlendClip(string clipName, float weight, float blendTime)
		{
			if (!this._blendsMap.ContainsKey(clipName))
			{
				BlendClip blendClip = new BlendClip(this._banks, clipName, weight, blendTime, true);
				this._blendsMap[clipName] = blendClip;
				this._blends.AddLast(blendClip);
				return;
			}
			this._blendsMap[clipName].Change(weight, blendTime, true);
		}

		private void Update()
		{
			LinkedListNode<BlendClip> next;
			for (LinkedListNode<BlendClip> linkedListNode = this._blends.First; linkedListNode != null; linkedListNode = next)
			{
				next = linkedListNode.Next;
				if (linkedListNode.Value.Update())
				{
					this._blendsMap.Remove(linkedListNode.Value.Animation);
					this._blends.Remove(linkedListNode);
				}
			}
			if (this._currentTreeName != null)
			{
				for (int i = 0; i < this._curPropertyNames.Length; i++)
				{
					this._curProperties[i] = this._properties[this._curPropertyNames[i]];
				}
				BlendTree<TFloat> blendTree = this._trees[this._currentTreeName];
				blendTree.Update(this._curProperties);
				BlendTree<TFloat>.UpdateResult[] lastResult = blendTree.LastResult;
				for (int j = 0; j < lastResult.Length; j++)
				{
					BlendClip blendClip = this._blendsMap[lastResult[j].Clip];
					blendClip.Change(lastResult[j].Weight, blendTree.BlendTime, true);
				}
			}
			else if (this._curSequence != null)
			{
				for (int k = 0; k < this._curPropertyNames.Length; k++)
				{
					this._curProperties[k] = this._properties[this._curPropertyNames[k]];
				}
				this._curSequence.Update(this._curProperties);
			}
		}

		private List<Animation> _banks = new List<Animation>();

		private AnimProperties<TFloat, TInt, TBool> _properties;

		private string _lastClip;

		private string _currentTreeName;

		private IAnimationSequence<TFloat> _curSequence;

		private LinkedList<BlendClip> _blends = new LinkedList<BlendClip>();

		private Dictionary<string, BlendClip> _blendsMap = new Dictionary<string, BlendClip>();

		private string[] _currentClips;

		private TFloat[] _curPropertyNames;

		private float[] _curProperties = new float[1];

		private Dictionary<string, BlendTree<TFloat>> _trees = new Dictionary<string, BlendTree<TFloat>>();
	}
}
