using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedComponents : MonoBehaviour
{
	Dictionary<System.Type, Component> cache = new Dictionary<System.Type, Component>();
	public T Get<T>() where T : Component {
		if (!cache.ContainsKey(typeof(T)))
			cache.Add(typeof(T), GetComponent<T>());
		return cache[typeof(T)] as T;
	}
}
