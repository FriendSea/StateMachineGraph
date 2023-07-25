using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IContextContainer
{
	public T Get<T>() where T : class;
	int GetValue(object target);
	int SetValue(object target, int value);
}
