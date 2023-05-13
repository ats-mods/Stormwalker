using System;
using System.IO;
using Eremite;
using Eremite.Controller;
using Eremite.Model;
using Eremite.Services;
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

        public static GameObject MakeGameObject(GameObject parent, string name, bool hide = true){
            var dummyOriginal = new GameObject();
            var result = GameObject.Instantiate(dummyOriginal, parent?.transform);
            result.name = name;
            if(hide)
                result.hideFlags = HideFlags.HideAndDontSave;
            dummyOriginal.Destroy();
            return result;
        }

        public static T PatchInGameObject<T>(GameObject parent, string name, Action<T> setupScript = null) where T : Component {
            var gameObject = parent.transform.Find(name)?.gameObject;
            if(gameObject != null){
                return gameObject.GetComponent<T>();
            } else {
                gameObject = MakeGameObject(parent, name);
                T result = gameObject.AddComponent<T>();
                if(setupScript != null){
                    setupScript.Invoke(result);
                }
                return result;
            }
        }

        public static T StealComponent<T>(string path) where T : Component {
            var go = GameObject.Find(path);
            return go.GetComponent<T>();
        }

        public static LocaText Text(string value, string key){
            var ts = (TextsService) MainController.Instance.AppServices.TextsService;
            if(!ts.texts.ContainsKey(key)){
                ts.texts.Add(key, value);
            }
            return new LocaText(){ key = key };
        }

        public static Sprite LoadSprite(string file) {
            var path = Path.Combine(BepInEx.Paths.PluginPath, "assets", file);
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, false);
            tex.LoadImage(fileData);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 50.0f);
            return sprite;
        }
    }
}