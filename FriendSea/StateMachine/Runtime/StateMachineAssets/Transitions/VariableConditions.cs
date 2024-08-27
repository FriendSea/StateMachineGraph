using FriendSea.StateMachine.Behaviours;
using FriendSea.StateMachine.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Conditions
{
	[DisplayName("Variables/GreaterThan")]
	class VariableGreaterThanTransition : Transition.ICondition
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public bool IsValid(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] > value;
	}

	[DisplayName("Variables/LessThan")]
	class VariableLessThanTransition : Transition.ICondition
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public bool IsValid(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] < value;
	}

	[DisplayName("Variables/Equals")]
	class VariableEqualsTransition : Transition.ICondition
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public bool IsValid(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] == value;
	}
}
