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
	}
}
