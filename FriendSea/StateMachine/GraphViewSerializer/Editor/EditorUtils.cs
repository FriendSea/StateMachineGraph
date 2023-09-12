using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using System.ComponentModel;
using System.Text.RegularExpressions;

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

    public static class SerializesJsonUtils
    {
		public record TypeInfo
		{
			public string assemblyName;
			public string namespaceName;
			public string typeName;
		}

		public static IEnumerable<TypeInfo> GetMissingTypes(string json)
		{
			var matches = Regex.Matches(json, "\"type\": ?\\{\\n.+\"class\": ?\"(.+)\",\\n.+\"ns\": ?\"(.*)\",\\n.+\"asm\": \"(.+)\"\\n");
			foreach (var m in matches)
			{
				var matchString = (m as Match).Groups[0].Value;
				var typeName = (m as Match).Groups[1].Value;
				var nameSpace = (m as Match).Groups[2].Value;
				var assemblyName = (m as Match).Groups[3].Value;

				var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == assemblyName);
				var type = assembly.GetType(string.IsNullOrEmpty(nameSpace) ? typeName : $"{nameSpace}.{typeName}");
				if (type != null) continue;

				yield return new TypeInfo() {
					assemblyName = assemblyName,
					namespaceName = nameSpace,
					typeName = typeName,
				};
			}
		}

		public static string ChangeReferenceType(string json, TypeInfo info, Type newType)
		{
			return Regex.Replace(json,
				$"\"type\": ?\\{{\\n.+\"class\": ?\"{info.typeName}\",\\n.+\"ns\": ?\"{info.namespaceName}\",\\n.+\"asm\": \"{info.assemblyName}\"\\n",
				$"\"type\": {{\n\"class\": \"{newType?.Name}\",\n\"ns\": \"{newType?.Namespace}\",\n\"asm\": \"{newType?.Assembly?.FullName}\"\n",
				RegexOptions.Multiline);
		}

        public static string NullifyMissingReferences(string json)
        {
			var missings = GetMissingTypes(json);
			foreach (var t in missings)
				json = ChangeReferenceType(json, t, null);
			return json;
		}
    }
}
