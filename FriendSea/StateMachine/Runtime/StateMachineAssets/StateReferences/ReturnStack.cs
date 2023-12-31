using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Controls
{
	class ReturnStack : ISerializableStateReference
	{
		[SerializeReference]
		internal ISerializableStateReference target;

		public class Context
		{
			internal Stack<IStateReference<IContextContainer>> returnStack = new Stack<IStateReference<IContextContainer>>();
			public void PushReturnState(IStateReference<IContextContainer> state) =>
				returnStack.Push(state);
		}

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			var returnStack = obj.GetOrCreate<Context>()?.returnStack;
			return ((returnStack.Count > 0 ? returnStack.Pop() : target).GetState(obj).state ?? target.GetState(obj).state, true);
		}
	}
}
