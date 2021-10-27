using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace FriendSea
{
    public static class EditorUtils
    {
        static Dictionary<Type, List<Type>> subClassLists = new Dictionary<Type, List<Type>>();
        public static List<Type> GetSubClasses(Type type)
		{
            if (!subClassLists.ContainsKey(type))
                subClassLists[type] = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => assem.GetTypes()).
                    Where(t => type.IsAssignableFrom(t) && (t != type)).ToList();
            return subClassLists[type];
		}

        static Dictionary<string, Type> classList = new Dictionary<string, Type>();
        public static Type GetClass(string name)
		{
            if (!classList.ContainsKey(name))
                classList[name] = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => assem.GetTypes()).
                    Where(t => name.Contains(t.FullName)).FirstOrDefault();
            return classList[name];
        }

        static Dictionary<string, Type> typeName2Type = new Dictionary<string, Type>();
        public static Type GetTypeFromSerializedTypeName(string fieldTypeName)
        {
            if (!typeName2Type.ContainsKey(fieldTypeName))
                typeName2Type[fieldTypeName] = Assembly.Load(fieldTypeName.Split(' ')[0]).GetType(fieldTypeName.Split(' ')[1]);
            return typeName2Type[fieldTypeName];
        }

        public static string GetSerializedTypeNameFromType(Type type)
		{
            return $"{type.Assembly.ToString().Split(',').FirstOrDefault()} {type.FullName}".Replace('+', '/');
        }
    }
}
