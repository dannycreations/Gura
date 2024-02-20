using BepInEx;
using ExitGames.Client.Photon;
using Gura;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace System
{
    public static class Safe
    {
        public static T Result<T>(Func<T> func, T val = default)
        {
            try { return func(); } catch { return val; }
        }

        public static bool IsResult<T>(Func<T> func, bool val = false)
        {
            try
            {
                var result = func();
                return !EqualityComparer<T>.Default.Equals(result, default);
            }
            catch { return val; }
        }

        public static bool IsResult(Action func, bool val = false)
        {
            try { func(); return true; } catch { return val; }
        }

        public static void SetPropertyValue<T>(object obj, string propertyName, T value)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(obj, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
    }

    public static class String
    {
        public static bool Find(this string str, string query) =>
            str.Contains(query.ToLower(), StringComparison.OrdinalIgnoreCase);

        public static string Humanize(this string str)
        {
            var result = "";
            foreach (var c in str)
                if ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    c == ' '
                ) result += c;

            return result;
        }

        public static T ToJson<T>(this string str) =>
            JsonConvert.DeserializeObject<T>(str, Extention.JsonSerializerSettings);

        public static void ToFile(this string str, string fileName = "log", bool unique = true, string ext = "txt")
        {
            var filePath = Path.Combine(Paths.BepInExRootPath, "debug");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            fileName = unique ? $"{fileName}_{Date.Now}" : fileName;
            fileName = $"{fileName}.{ext}";
            using var writer = new StreamWriter(Path.Combine(filePath, fileName));
            writer.Write(str);
            Plugin.Log.LogInfo($"Saved to file {fileName}");
        }
    }

    public static class Object
    {
        public static string Stringify(this object obj) =>
            JsonConvert.SerializeObject(obj, Extention.JsonSerializerSettings);

        public static void StringifyToFile(this object obj, string fileName = "log", bool unique = true) =>
            obj.Stringify().ToFile(fileName, unique, "json");
    }

    public static class Extention
    {
        public static string PrintKeys(Hashtable hashTable)
        {
            if (hashTable == null)
                return "";

            var list = new List<string>();
            foreach (var key in hashTable.Keys)
                list.Add(key.ToString());

            return list.Stringify();
        }

        public static JsonSerializerSettings JsonSerializerSettings => new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Culture = CultureInfo.InvariantCulture,
            ContractResolver = new JsonConfigContractResolver(),
            Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs) =>
            {
                var currentError = errorArgs.ErrorContext.Error.Message;
                errorArgs.ErrorContext.Handled = true;
            }
        };
    }

    public static class Date
    {
        public static long Now =>
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

public class JsonConfigContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var list = new List<JsonProperty>();
        foreach (var jsonProperty in base.CreateProperties(type, memberSerialization))
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var property = type.GetProperty(jsonProperty.PropertyName, bindingAttr);
            if (property == null ||
                property.Name.StartsWith("_")
            ) continue;

            list.Add(jsonProperty);
        }

        return list;
    }
}