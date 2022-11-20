using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

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
    /// for repeated xml nodes, i.e. lists.
    /// Default value is "List", so for repeated nodes
    /// named "Customer", the generated property
    /// will be named "CustomerList".
    /// <summary>
    public static string ListSuffix { get; set; } = "List";

    /// <summary>
    /// Parses the given xml file and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="path">The full path to the xml file.</param>
    /// <returns>A dynamic object generated from the xml data.</returns>
    public static dynamic ParseFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File '{path}' was not found.");
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(path);

        var root = xmlDoc.DocumentElement;
        return AddRecursive(new ToStringExpandoObject(), root);
    }

    /// <summary>
    /// Parses the given xml and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="text">The xml content.</param>
    /// <returns>A dynamic object generated from the xml data.</returns>
    public static dynamic ParseText(string text)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(text);

        var root = xmlDoc.DocumentElement;
        return AddRecursive(new ToStringExpandoObject(), root);
    }

    private static dynamic AddRecursive(ToStringExpandoObject parent, XmlNode xmlObj)
    {
        var propertiesList = new List<Tuple<string, XmlNode>>();

        if (xmlObj.ChildNodes != null)
        {
            // ignore xml comments, text and cdata nodes
            // text and cdata are directly added as value
            foreach (var xmlChild in xmlObj.ChildNodes.OfType<XmlNode>().Where(
                c => (c.LocalName != "#comment") 
                && (c.LocalName != "#text") 
                && (c.LocalName != "#cdata-section")).ToList())
            {
                //Console.WriteLine(xmlChild.LocalName);
                string name = getValidName(xmlChild.LocalName);
                propertiesList.Add(new Tuple<string, XmlNode>(name, xmlChild));
            }
        }
        if (xmlObj.Attributes != null)
        {
            foreach (var xmlChild in xmlObj.Attributes.OfType<XmlAttribute>().ToList())
            {
                string name = getValidName(xmlChild.LocalName);
                propertiesList.Add(new Tuple<string, XmlNode>(name, xmlChild));
            }
        }

        // attribute + list names
        var groups = propertiesList.GroupBy(x => x.Item1);
        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                // add property to parent
                dynamic newMember = new ToStringExpandoObject();
                XmlNode node = group.First().Item2;
                // ignore xml comments
                newMember.__value = getXmlNodeValue(node);
                newMember.ToString = (ToStringFunc)(() => newMember.__value);
                string newMemberName = group.Key;

                ((IDictionary<string, Object>)parent.Members).Add(newMemberName, newMember);
                AddRecursive(newMember, node);
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
                    XmlNode node = listNode.Item2;
                    newMember.__value = getXmlNodeValue(node);
                    newMember.ToString = (ToStringFunc)(() => newMember.__value);
                    //string newMemberName = group.Key;

                    newList.Add(newMember);
                    AddRecursive(newMember, node);
                }
            }
        }

        return parent;
    }

    private static string getXmlNodeValue(XmlNode node)
    {
        if (node is XmlAttribute)
        {
            return (node as XmlAttribute).Value;
        }
        if (node is XmlElement)
        {
            var e = (node as XmlElement);
            return e.Value ?? e.ChildNodes.OfType<XmlNode>().FirstOrDefault(
                c => (c.LocalName == "#text") 
                || (c.LocalName == "#cdata-section"))?.Value;
        }
        if (node is XmlCDataSection)
        {
            var e = (node as XmlCDataSection);
            return e.Value;
        }
        throw new NotSupportedException($"Type {node.GetType().FullName} is not supported");
    }        

    private static string getValidName(string nodeName)
    {
        Regex rgx = new Regex("[^0-9a-zA-Z]+");
        string res = rgx.Replace(nodeName, "");
        return res;
    }
}
