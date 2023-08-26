using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IContextContainer
{
	public int FrameCount { get; set; }
	public float Time { get; set; }
	T Get<T>() where T : class;
	int GetValue(object target);
	int SetValue(object target, int value);
}
