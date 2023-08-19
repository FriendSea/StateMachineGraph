using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public class DisplayNameAttribute : System.Attribute
	{
		public string Name { get; }
		public DisplayNameAttribute(string name) => Name = name;
	}
}
