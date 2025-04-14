using FriendSea.StateMachine.Behaviours;
using FriendSea.StateMachine.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Conditions
{
	[DisplayName("Variables/GreaterThan")]
	partial class VariableGreaterThanTransition : Transition.ICondition
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public bool IsValid(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] > value;
	}

	[DisplayName("Variables/LessThan")]
	partial class VariableLessThanTransition : Transition.ICondition
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public bool IsValid(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] < value;
	}

	[DisplayName("Variables/Equals")]
	partial class VariableEqualsTransition : Transition.ICondition
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public bool IsValid(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] == value;
	}
}
