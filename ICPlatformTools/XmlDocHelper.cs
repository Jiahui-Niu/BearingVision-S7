using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.Web.Services.Description;
using System.Windows.Forms;

namespace ICPlatformTools
{
    public class XmlDocHelper
    {
        private XmlDocument m_xmlDoc = new XmlDocument();
        private string docFileName = string.Empty;

        public XmlDocument XmlDoc
        {
            get { return m_xmlDoc; }
        }

        public string DocFileName
        {
            get { return docFileName; }
        }

        public bool Load(string path)
        {
            try
            {
                m_xmlDoc.Load(path);
                docFileName = path;
                return true;
            }
            catch (Exception e)
            {
                LogHelper.Log.Error("Load xml file error.", e);
            }
            return false;
        }

        public bool Save(string path)
        {
            try
            {
                m_xmlDoc.Save(path);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.Log.Error("Save xml file error.", e);
                return false;
            }
        }

        /// <summary>
        /// 读取节点的某个属性值
        /// </summary>
        /// <param name="xpath">节点路径（选择匹配 XPath 表达式的第一个 XmlNode）</param>
        /// <param name="name">节点中的属性名称</param>
        /// <returns></returns>
        public string GetNodeAttr(string xpath, string name = "val")
        {
            try
            {
                XmlNode node = m_xmlDoc.SelectSingleNode(xpath);
                if (node != null && node.Attributes[name] != null)
                {
                    return node.Attributes[name].Value;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
            return null;
        }

        public T GetNodeAttr<T>(string xpath, string name, T defVal = default(T))
        {
            string str = GetNodeAttr(xpath, name);
            var val = defVal;
            try
            {
                val = StringToValue<T>(str);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("  xpath:" + xpath + "     name:" + name);
                LogHelper.Log.Error(ex);
            }
            return val;
        }

        public T GetNodeInnerValue<T>(string xpath, T defValue = default(T))
        {
            var val = defValue;
            try
            {
                var node = m_xmlDoc.SelectSingleNode(xpath);
                if (node != null)
                {
                    val = StringToValue<T>(node.InnerText);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
            return val;
        }

        public bool SetNodeInnerValue<T>(string xpath, T value, bool autoCreate = true)
        {
            try
            {
                var node = m_xmlDoc.SelectSingleNode(xpath);
                if (node == null && autoCreate)
                {
                    node = CreateNode(xpath);
                }
                if (node != null && value != null)
                {
                    node.InnerText = value.ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
            return false;
        }

        public List<string> GetNodesValue(string xpath)
        {
            List<string> result = new List<string>();
            try
            {
                XmlNodeList list = m_xmlDoc.SelectNodes(xpath);
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        result.Add(list[i].ChildNodes[0].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
            return result;
        }

        public bool SetNodesValue(string xpath, string elemName, List<string> values)
        {
            try
            {
                XmlNode parent = m_xmlDoc.SelectSingleNode(xpath);
                if (parent != null)
                {
                    parent.RemoveAll();
                    foreach (var val in values)
                    {
                        var node = m_xmlDoc.CreateNode(XmlNodeType.Element, elemName, "");
                        var childnode = m_xmlDoc.CreateNode(XmlNodeType.Text, "", "");
                        childnode.Value = val;
                        node.AppendChild(childnode);
                        parent.AppendChild(node);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
            return false;
        }

        /// <summary>
        /// 多个节点，相似的节点，但是节点的属性名一样，属性值不一样
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> GetNodesAttr(string xpath, string name)
        {
            List<string> result = new List<string>();
            try
            {
                XmlNodeList list = m_xmlDoc.SelectNodes(xpath);
                var nd = m_xmlDoc.SelectSingleNode(xpath);

                foreach (XmlNode node in list)
                {
                    if (node.Attributes[name] != null)
                    {
                        result.Add(node.Attributes[name].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex);
            }
            return result;
        }

        /// <summary>
        /// 设置配置参数
        /// </summary>
        /// <param name="xpath">节点路径（选择匹配 XPath 表达式的第一个 XmlNode）</param>
        /// <param name="name">节点中的属性名称</param>
        /// <param name="value">要设置的属性值</param>
        public bool SetNodeAttr<T>(string xpath, string name, T value)
        {
            try
            {
                XmlNode node = m_xmlDoc.SelectSingleNode(xpath);
                if (node == null)
                {
                    node = CreateNode(xpath);
                }
                if (node != null)
                {
                    if (node.Attributes[name] != null)
                    {
                        node.Attributes[name].Value = value.ToString();
                    }
                    else
                    {
                        XmlNode nodeAttr = m_xmlDoc.CreateNode(XmlNodeType.Attribute, name, null);
                        nodeAttr.Value = value.ToString();
                        node.Attributes.SetNamedItem(nodeAttr);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("set xml node attribute failed.", ex);
            }
            return false;
        }

        /// 新增节点并设置一个属性
        /// parentPath 
        public bool AddNode(string parentPath, string elementName, string attrName = null, string attrVal = null)
        {
            try
            {
                XmlNode Node = m_xmlDoc.SelectSingleNode(parentPath);
                XmlElement xmlElement = m_xmlDoc.CreateElement(elementName);
                if (!string.IsNullOrEmpty(attrName))
                {
                    xmlElement.SetAttribute(attrName, attrVal);
                }
                Node.AppendChild(xmlElement);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("add xml element failed.", ex);
            }
            return false;
        }


        /// 新增节点并设置多个属性
        /// parentPath 
        public bool AddNode(string parentPath, string elementName, List<KeyValuePair<string, string>> attrList)
        {
            try
            {
                XmlNode root = m_xmlDoc.SelectSingleNode(parentPath);
                if (root == null)
                {
                    return false;
                }

                XmlElement xmlElement = m_xmlDoc.CreateElement(elementName);
                root.AppendChild(xmlElement);

                foreach (var p in attrList)
                {
                    xmlElement.SetAttribute(p.Key, p.Value);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("add xml element failed.", ex);
            }
            return false;
        }

        /// 新增注释节点
        public bool AddComment(string parentPath, string elementName)
        {
            try
            {
                XmlNode Node = m_xmlDoc.SelectSingleNode(parentPath);

                XmlNodeList nodeList = Node.ChildNodes;

                foreach (var item in nodeList)
                {
                    XmlNode node = (XmlNode)item;
                    if (node.Value == elementName)
                    {
                        return false;
                    }
                }
                XmlComment xmlComment = m_xmlDoc.CreateComment(elementName);
                Node.AppendChild(xmlComment);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("add xml comment failed.", ex);
            }
            return false;
        }
        /// <summary>
        ///   删除节点属性
        /// </summary>
        /// <param name="xpath"></param>节点路径
        /// <param name="name"></param>要删除的节点特性
        public bool DeleteNodeAttr(string xpath, string name)
        {
            try
            {
                //取出根节点
                var root = m_xmlDoc.SelectSingleNode(xpath);
                root.Attributes.Remove(root.Attributes[name]);
                //XmlNode node = root.SelectSingleNode(name);
                //root.RemoveChild(node);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("delete xml node attribute failed. ", ex);
            }
            return false;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="xpath"></param>要删除的节点的上一级路径
        /// <param name="name"></param>要删除的节点名字
        public bool DeleteNode(string xpath, string name)
        {
            try
            {
                //取出根节点
                var root = m_xmlDoc.SelectSingleNode(xpath);
                var nodeList = root.SelectNodes(name);
                foreach (XmlNode node in nodeList)
                {
                    root.RemoveChild(node);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("delete xml node failed. ", ex);
            }
            return false;
        }

        public bool DeleteNode(string xpath)
        {
            try
            {
                //取出根节点
                var nodeList = m_xmlDoc.SelectNodes(xpath);
                foreach (XmlNode node in nodeList)
                {
                    var parent = node.ParentNode;
                    if (parent != null)
                    {
                        parent.RemoveChild(node);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("delete xml node failed. ", ex);
            }
            return false;
        }


        /// <summary>
        /// 设置属性列表（一个节点下面多个特性，比如val0、val1）
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="elementName"></param>
        /// <param name="attributeName">节点中特性的名字，实际上在使用时还会加后缀0、1、2等</param>
        /// <param name="attributeValueList"></param>
        public bool SetNodeAttrList(string xpath, string attributeName, List<string> attributeValueList)
        {
            try
            {
                XmlNode parentNode = m_xmlDoc.SelectSingleNode(xpath);

                //先删除原来的属性
                List<XmlAttribute> attList = new List<XmlAttribute>();
                foreach (XmlAttribute item in parentNode.Attributes)
                {
                    if (item.Name.StartsWith(attributeName))
                    {
                        attList.Add(item);
                    }
                }

                foreach (var item in attList)
                {
                    parentNode.Attributes.Remove(item);
                }

                //添加新的属性
                int index = 0;
                foreach (var item in attributeValueList)
                {
                    XmlNode node = m_xmlDoc.CreateNode(XmlNodeType.Attribute, attributeName + index++.ToString(), null);
                    node.Value = item;
                    parentNode.Attributes.SetNamedItem(node);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("set node attribute list failed.", ex);
            }
            return false;
        }

        public XmlNodeList GetNodeList(string xpath)
        {
            return m_xmlDoc.SelectNodes(xpath);
        }

        public XmlNode CreateNode(string xpath)
        {
            var node = m_xmlDoc.SelectSingleNode(xpath);
            if (node != null)
            {
                return node;
            }

            string path = xpath.Trim().TrimEnd('/');
            int lastSlash = path.LastIndexOf('/');
            XmlNode parentNode = null;
            if (lastSlash <= 0)
            {
                parentNode = m_xmlDoc.DocumentElement;
            }
            else if (lastSlash == 1 && path.StartsWith("//"))
            {
                parentNode = m_xmlDoc.DocumentElement;
            }
            else
            {
                string parentPath = path.Substring(0, lastSlash);
                parentNode = m_xmlDoc.SelectSingleNode(parentPath);
                if (parentNode == null)
                {
                    parentNode = CreateNode(parentPath);
                }
            }

            if (parentNode == null)
            {
                return null;
            }

            string nodeName = lastSlash < 0 ? path : path.Substring(lastSlash + 1);
            node = CreateNode(m_xmlDoc, nodeName);
            parentNode.AppendChild(node);
            return node;
        }

        private XmlNode CreateNode(XmlDocument doc, string nameWithAttr)
        {
            string name = Regex.Match(nameWithAttr, @"[^\[]+").Value;
            var node = doc.CreateElement(name);

            var attrs = Regex.Matches(nameWithAttr, @"(?<=@)([^=]+)\='?([^]]+?)'?")
                .Cast<Match>()
                .Select(it =>
                {
                    var groups = it.Groups.Cast<Group>().ToList();
                    return new { Name = groups[1].Value, Value = groups[2].Value };
                });
            foreach (var attr in attrs)
            {
                var a = doc.CreateAttribute(attr.Name);
                a.Value = attr.Value;
                node.Attributes.Append(a);
            }
            return node;
        }

        private static T StringToValue<T>(string str)
        {
            try
            {
                if (typeof(T) == typeof(bool))
                {
                    bool boolValue;
                    if (bool.TryParse(str, out boolValue))
                    {
                        return (T)(object)boolValue;
                    }

                    int intValue;
                    if (int.TryParse(str, out intValue))
                    {
                        return (T)Convert.ChangeType(intValue, typeof(T));
                    }
                }
                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return (T)Convert.ChangeType(str, typeof(T));
            }
        }
    }
}
