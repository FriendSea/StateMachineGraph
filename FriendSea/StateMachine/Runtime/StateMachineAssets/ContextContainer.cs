using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public interface IContextContainer
	{
		int FrameCount { get; set; }
		float Time { get; set; }
		T Get<T>() where T : class;
		void Add<T>(T obj) where T : class;
	}

	public class ContextContainerBase : IContextContainer
	{
		public int FrameCount { get; set; }
		public float Time { get; set; }

		protected Dictionary<System.Type, object> contextObjects = new Dictionary<System.Type, object>();
		public virtual T Get<T>() where T : class =>
			contextObjects.ContainsKey(typeof(T)) ?
				contextObjects[typeof(T)] as T :
				null;
		public virtual void Add<T>(T obj) where T : class =>
			contextObjects[typeof(T)] = obj;
	}

	public static class ContextContainerExtensions
	{
		public static T GetOrCreate<T>(this IContextContainer ctx) where T : class, new()
		{
			if (ctx.Get<T>() == null)
				ctx.Add(new T());
			return ctx.Get<T>();
		}
	}
}

