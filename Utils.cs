using Eremite;
using Eremite.Tools.Runtime;
using UnityEngine;

namespace Stormwalker
{
    public static class Utils
    {
        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        { // Taken from http://answers.unity.com/answers/1118416/view.html
            System.Type type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst) dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(dst, field.GetValue(original));
            }
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }
            return dst as T;
        }

        public static GameObject MakeGameObject(GameObject parent, string name){
            var dummyOriginal = new GameObject();
            var result = GameObject.Instantiate(dummyOriginal, parent?.transform);
            result.name = name;
            result.hideFlags = HideFlags.HideAndDontSave;
            dummyOriginal.Destroy();
            return result;
        }
    }
}