using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FriendSea.StateMachine.Behaviours
{
	[System.Serializable]
	struct SerializedAnimatorState
	{
		[SerializeField]
		internal int stateHash;

#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(SerializedAnimatorState))]
		class SerializedAnimatorStateEditor : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				var animtator = Selection.activeGameObject?.GetComponentInChildren<Animator>()?.runtimeAnimatorController as AnimatorController;
				if(animtator == null)
				{
					// EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(stateHash)));
					EditorGUI.HelpBox(position, "Select GameObject to select state.", MessageType.Info);
				}
				else
				{
					var states = animtator.layers.SelectMany(l => l.stateMachine.states).Select(s => s.state).ToList();
					EditorGUI.IntPopup(position, property.FindPropertyRelative(nameof(stateHash)), states.Select(s => new GUIContent(s.name)).ToArray(), states.Select(s => s.nameHash).ToArray(), label);
				}
			}
		}
#endif
	}

	[DisplayName("Animator/SetState")]
	public partial class SetAnimatorState : IBehaviour
	{
		[SerializeField] SerializedAnimatorState state;
		[SerializeField] float fadeTime;
		[InjectContext] Animator animator;

		public void OnEnter(IContextContainer obj)
		{
			animator.CrossFade(state.stateHash, fadeTime);
		}

		public void OnExit(IContextContainer obj) { }

		public void OnUpdate(IContextContainer obj) { }
	}

    [DisplayName("Animator/SetStateByName")]
    public partial class SetAnimatorStateByName : IBehaviour
    {
        [SerializeField] string name;
        [SerializeField] float fadeTime;
        [InjectContext] Animator animator;

        public void OnEnter(IContextContainer obj)
        {
            animator.CrossFade(name, fadeTime);
        }

        public void OnExit(IContextContainer obj) { }

        public void OnUpdate(IContextContainer obj) { }
    }
}
