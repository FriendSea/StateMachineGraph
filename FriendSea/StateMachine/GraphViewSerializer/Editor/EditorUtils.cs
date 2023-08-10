using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace FriendSea
{
    public static class EditorUtils
    {
        static Dictionary<Type, List<Type>> subClassLists = new Dictionary<Type, List<Type>>();
        public static List<Type> GetSubClasses(Type type)
		{
            if (!subClassLists.ContainsKey(type))
                subClassLists[type] = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => TryGetTypes(assem))
                    .Where(t => type.IsAssignableFrom(t) && (t != type))
                    .Where(t => !t.IsInterface && !t.IsAbstract).ToList();
            return subClassLists[type];
		}

        static Type[] TryGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (Exception e)
            {
                if (e is ReflectionTypeLoadException)
                    return new Type[0];
                throw e;
            }
        }

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
