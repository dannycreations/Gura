using System;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts._3D.Game.MissionObjects
{
	public class MOActionsController
	{
		public void SetParentTransform(Transform parentTransform)
		{
			this._parentTransform = parentTransform;
		}

		public void RunActions(ActionsSettings actions, Action onCompleteAll)
		{
			Sequence sequence = DOTween.Sequence();
			TweenSettingsExtensions.OnComplete<Sequence>(sequence, delegate
			{
				if (onCompleteAll != null)
				{
					onCompleteAll();
				}
			});
			if (actions.Movements != null)
			{
				for (int i = 0; i < actions.Movements.Length; i++)
				{
					MovementActions movementActions = actions.Movements[i];
					if (movementActions.Obj != null)
					{
						TweenSettingsExtensions.Join(sequence, ShortcutExtensions.DOLocalMove(movementActions.Obj, movementActions.Position, movementActions.Speed, false));
					}
				}
			}
			if (actions.Rotations != null)
			{
				for (int j = 0; j < actions.Rotations.Length; j++)
				{
					RotationActions rotationActions = actions.Rotations[j];
					if (rotationActions.Obj != null)
					{
						if (rotationActions.Loops > 0)
						{
							TweenSettingsExtensions.Join(sequence, TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOLocalRotate(rotationActions.Obj, rotationActions.Rotation, rotationActions.Speed, 1), rotationActions.Loops), 1));
						}
						else
						{
							TweenSettingsExtensions.Join(sequence, ShortcutExtensions.DOLocalRotate(rotationActions.Obj, rotationActions.Rotation, rotationActions.Speed, 0));
						}
					}
				}
			}
			if (actions.Particles != null)
			{
				for (int k = 0; k < actions.Particles.Length; k++)
				{
					ParticleActions particleActions = actions.Particles[k];
					if (particleActions.Ps != null)
					{
						particleActions.Ps.gameObject.SetActive(true);
						particleActions.Ps.Clear();
						particleActions.Ps.Play();
					}
				}
			}
			if (actions.Sounds != null)
			{
				for (int l = 0; l < actions.Sounds.Length; l++)
				{
					SoundActions soundActions = actions.Sounds[l];
					Vector3 vector = Vector3.zero;
					if (soundActions.IsPlayOnPlayer && GameFactory.Player != null)
					{
						vector = GameFactory.Player.transform.position;
					}
					else
					{
						vector = ((!(soundActions.Obj != null)) ? ((!(this._parentTransform != null)) ? soundActions.Position : this._parentTransform.position) : soundActions.Obj.transform.position);
					}
					RandomSounds.PlaySoundAtPoint(soundActions.Clip, vector, soundActions.Volume);
				}
			}
		}

		private Transform _parentTransform;
	}
}
