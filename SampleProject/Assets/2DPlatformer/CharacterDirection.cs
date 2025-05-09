using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDirection : MonoBehaviour, FriendSea.StateMachine.Behaviours.IDirectionable
{
	public int Direction { set => transform.localScale = new Vector3(value > 0 ? 1 : -1, 1, 1); }
}
