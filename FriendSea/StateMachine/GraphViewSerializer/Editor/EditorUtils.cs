using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using System.ComponentModel;

namespace FriendSea
{
    public static class EditorUtils
    {
        static Dictionary<Type, List<Type>> subClassLists = new Dictionary<Type, List<Type>>();
        public static List<Type> GetSubClasses(Type type)
		{
            if (!subClassLists.ContainsKey(type))
                subClassLists[type] = TypeCache.GetTypesDerivedFrom(type).Where(t => !t.IsAbstract).ToList();
            return subClassLists[type];
		}

		public static string GetDisplayName(this System.Type type) =>
			(type.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute)?.DisplayName ??
			type.FullName.Replace(".", "/");

		public struct WrapEnumerable<T> : IEnumerable<T>
		{
            IEnumerator<T> enumerator;

            public WrapEnumerable(IEnumerator<T> enumerator)
			{
                this.enumerator = enumerator;
			}

            public IEnumerator<T> GetEnumerator() => enumerator;
			IEnumerator IEnumerable.GetEnumerator() => enumerator;
		}
        public static WrapEnumerable<T> WrapAsEnumerable<T>(this IEnumerator<T> enumerator) => new WrapEnumerable<T>(enumerator);

        public static IEnumerator<SerializedProperty> EnumerateArray(this SerializedProperty property)
		{
            for (int i = 0; i < property.arraySize; i++)
                yield return property.GetArrayElementAtIndex(i);
		}

        public static IEnumerable<SerializedProperty> ArrayAsEnumerable(this SerializedProperty property) =>
            property.EnumerateArray().WrapAsEnumerable();
    }
}
