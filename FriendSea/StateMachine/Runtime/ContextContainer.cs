using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IContextContainer
{
	public T Get<T>() where T : class;
	int GetValue(object target);
	int SetValue(object target, int value);
}

public class GameObjectContextContainer : IContextContainer
{
	GameObject obj;

	public GameObjectContextContainer(GameObject obj) => this.obj = obj;

	Dictionary<System.Type, Component> cache = new Dictionary<System.Type, Component>();
	public T Get<T>() where T : class
	{
		if (typeof(T) == typeof(GameObject)) return obj as T;
		if (!cache.ContainsKey(typeof(T)))
			cache.Add(typeof(T), obj.GetComponentInChildren<T>(true) as Component);
		return cache[typeof(T)] as T;
	}

	Dictionary<object, int> values = new Dictionary<object, int>();
	public int GetValue(object target)
	{
		if (!values.ContainsKey(target))
			values.Add(target, 0);
		return values[target];
	}
	public int SetValue(object target, int value) => values[target] = value;
}
