using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Diagnostics;

namespace Dandraka.Slurper;

/// <summary>
/// Implements dynamic JSON parsing, the result being a dynamic object with properties 
/// matching the json nodes. 
/// Note that if under a certain node there are multiple nodes
/// with the same name, a list property will be created. The list property's name will
/// be [common name]List, e.g. bookList.
/// </summary>
public static class JsonSlurper
{
    /// <summary>
    /// Specifies the suffix for properties generated 
    /// for repeated nodes, i.e. lists.
    /// Default value is "List", so for repeated nodes
    /// named "Customer", the generated property
    /// will be named "CustomerList".
    /// <summary>
    public static string ListSuffix { get; set; } = "List";

    /// <summary>
    /// Parses the given json file and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="path">The full path to the json file.</param>
    /// <returns>A dynamic object generated from the json data.</returns>
    public static dynamic ParseFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File '{path}' was not found.");
        }

        var jsonDoc = JsonDocument.Parse(File.ReadAllText(path));

        var root = jsonDoc.RootElement;
        return AddRecursive(new ToStringExpandoObject(), root);
    }

    /// <summary>
    /// Parses the given xml and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="text">The xml content.</param>
    /// <returns>A dynamic object generated from the xml data.</returns>
    public static dynamic ParseText(string text)
    {
        var jsonDoc = JsonDocument.Parse(text);

        var root = jsonDoc.RootElement;
        return AddRecursive(new ToStringExpandoObject(), root);
    }

    private static dynamic AddRecursive(ToStringExpandoObject parent, object obj)
    {
        object jsonObj = null;
        if (obj is JsonProperty)
        {
            var jsonValue = ((JsonProperty)obj).Value;

            // here we only care about ValueKind that
            // may have nested elements
            switch (jsonValue.ValueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    jsonObj = jsonValue;
                    break;
                default:
                    break;
            }
        }

        if (obj is JsonElement)
        {
            jsonObj = obj;
        }

        if (jsonObj == null)
        {
            return parent;
        }

        var propertiesList = new List<Tuple<string, object>>();

        if (jsonObj is JsonElement)
        {
            var jsonElement = (JsonElement)jsonObj;
            List<object> jsonChildren = null;
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                jsonChildren = jsonElement.EnumerateObject().Select(x => (object)x).ToList();
            }
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                jsonChildren = jsonElement.EnumerateArray().Select(x => (object)x).ToList();
            }

            if (jsonChildren != null && jsonChildren.Any())
            {
                foreach (var jsonChild in jsonChildren)
                {
                    string jsonName = null;
                    if (jsonChild is JsonElement)
                    {
                        jsonName = ((System.Text.Json.JsonProperty)obj).Name;
                    }
                    if (jsonChild is JsonProperty)
                    {
                        jsonName = ((JsonProperty)jsonChild).Name;
                    }
                    string name = getValidName(jsonName);
                    Debug.WriteLine($"{jsonName} = {name}");
                    propertiesList.Add(new Tuple<string, object>(name, jsonChild));
                }
            }
        }

        // determine list names
        var groups = propertiesList.GroupBy(x => x.Item1);
        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                // add property to parent
                dynamic newMember = new ToStringExpandoObject();
                object jsonObjChild = group.First().Item2;
                newMember.__value = getJsonPropertyValue(jsonObjChild);
                newMember.ToString = (ToStringFunc)(() => newMember.__value);
                string newMemberName = group.Key;

                ((IDictionary<string, Object>)parent.Members).Add(newMemberName, newMember);
                AddRecursive(newMember, jsonObjChild);
            }
            else
            {
                // lists
                string listName = $"{group.Key}{ListSuffix}";
                List<ToStringExpandoObject> newList;
                if (!((IDictionary<string, Object>)parent.Members).ContainsKey(listName))
                {
                    newList = new List<ToStringExpandoObject>();
                    ((IDictionary<string, Object>)parent.Members).Add(listName, newList);
                }
                else
                {
                    newList = ((IDictionary<string, Object>)parent.Members)[listName] as List<ToStringExpandoObject>;
                }
                foreach (var listNode in group.ToList())
                {
                    // add property to parent
                    dynamic newMember = new ToStringExpandoObject();
                    object jsonObjChild = listNode.Item2;
                    newMember.__value = getJsonPropertyValue(jsonObjChild);
                    newMember.ToString = (ToStringFunc)(() => newMember.__value);
                    //string newMemberName = group.Key;

                    newList.Add(newMember);
                    AddRecursive(newMember, jsonObjChild);
                }
            }
        }

        return parent;
    }

    private static string getJsonPropertyValue(object jsonObj)
    {
        if (!(jsonObj is JsonProperty))
        {
            // nothing to see here
            return null;
        }

        string rawText = null;
        switch (((JsonProperty)jsonObj).Value.ValueKind)
        {
            case JsonValueKind.String:
                rawText = ((JsonProperty)jsonObj).Value.GetString();
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                // ToStringExpandoObject takes care about conversion
                rawText = ((JsonProperty)jsonObj).Value.GetRawText();
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.Object:
            case JsonValueKind.Array:
            case JsonValueKind.Null:
                // stays null
                break;
            default:
                throw new NotSupportedException($"JsonProperty ValueKind {((JsonProperty)jsonObj).Value.ValueKind} is not supported");
        }

        Debug.WriteLine(rawText);
        return rawText;
    }

    private static string getValidName(string nodeName)
    {
        Regex rgx = new Regex("[^0-9a-zA-Z]+");
        string res = rgx.Replace(nodeName, "");
        return res;
    }
}
