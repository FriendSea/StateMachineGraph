using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class CachedComponents : MonoBehaviour
	{
		Dictionary<System.Type, Component> cache = new Dictionary<System.Type, Component>();
		public T Get<T>() where T : Component {
			if (!cache.ContainsKey(typeof(T)))
				cache.Add(typeof(T), GetComponentInChildren<T>());
			return cache[typeof(T)] as T;
		}

		Dictionary<object, int> values = new Dictionary<object, int>();
		internal int GetValue(object target)
		{
			if (!values.ContainsKey(target))
				values.Add(target, 0);
			return values[target];
		}
		internal int SetValue(object target, int value) => values[target] = value;
	}
}
