using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ICPlatformTools
{
    public class XmlCompare
    {
        private string _fileName1, _fileName2;
        private List<string> xpathList;
        private List<DiffModel> diff;
        private XmlDocHelper doc;
        private XmlDocHelper doc2;
        private XmlDocHelper comparedDoc;
        private StringBuilder sb;
        private bool m_mergeMode = false;

        /// <summary>
        /// xml 对比
        /// </summary>
        /// <param name="fileName1">被对比文件</param>
        /// <param name="fileName2">对比文件</param>
        /// <param name="mergeMode">简单合并模式</param>
        public XmlCompare(string fileName1, string fileName2, bool mergeMode = false)
        {
            _fileName1 = fileName1;
            _fileName2 = fileName2;
            m_mergeMode = mergeMode;
        }

        /// <summary>
        /// 对比
        /// </summary>
        /// <param name="travelBack">true时两个两个xml对调再进行一次对比</param>
        /// <returns></returns>
        private List<DiffModel> CompareInner(bool travelBack = true)
        {
            diff = new List<DiffModel>();
            xpathList = new List<string>();
            sb = new StringBuilder();
            doc = new XmlDocHelper();
            doc2 = new XmlDocHelper();

            try
            {
                doc.Load(_fileName1);
                doc2.Load(_fileName2);
            }
            catch (Exception ex)
            {
                diff.Add(new DiffModel
                {
                    CurrentValue = "",
                    errorInfo = ex.Message,
                    Name = "",
                    OriginValue = "",
                    XPath = ""
                });
                return diff;
            }

            comparedDoc = doc2;

            // <?xml version="1.0" 下一个节点就是整个文档的父节点
            var node1 = doc.XmlDoc.FirstChild.NextSibling;
            var node2 = doc2.XmlDoc.FirstChild.NextSibling;

            try
            {
                NodesTravel(node1);

                if (m_mergeMode)
                {
                    comparedDoc.Save(comparedDoc.DocFileName);
                }
            }
            catch (Exception ex) { LogHelper.Log.Error(ex.Message, ex); }

            xpathList.Clear();
            sb.Clear();

            if (travelBack)
            {
                // 反过来再比对一次
                var temp = _fileName1;
                _fileName1 = _fileName2;
                _fileName2 = temp;
                comparedDoc = doc;

                try
                {
                    NodesTravel(node2);

                    if (m_mergeMode)
                    {
                        comparedDoc.Save(comparedDoc.DocFileName);
                    }
                }
                catch (Exception ex) { LogHelper.Log.Error(ex.Message, ex); }

                xpathList.Clear();
                sb.Clear();
            }

            return diff;
        }

        public string Compare(bool travelBack = true)
        {
            var diff = CompareInner(travelBack);
            StringBuilder resultSb = new StringBuilder();

            foreach (var d in diff)
            {
                resultSb.AppendLine(d.errorInfo);
            }

            return resultSb.ToString();
        }

        public string[] Compare2(bool travelBack = true)
        {
            var diff = CompareInner(travelBack);
            var resultList = new List<string>();

            foreach (var d in diff)
            {
                resultList.Add(d.errorInfo);
            }

            return resultList.ToArray();
        }

        private void NodesTravel(XmlNode node)
        {
            string xpath = sb.ToString() + node.Name;

            var cnt = xpathList.Count(s => s == xpath);
            xpathList.Add(xpath);
            if (cnt > 0)
            {
                xpath += string.Format("[{0}]", cnt + 1);
            }

            sb.Clear();
            sb.Append(xpath + "/");

            var node2 = comparedDoc.XmlDoc.SelectSingleNode(xpath);
            if (node2 != null)
            {
                //var str = string.Format("{0}\n{1}", node.Name, node2.Name);
                CompareNodeAttr(node, node2, xpath);
                CompareNodeText(node, node2, xpath);
            }
            else
            {
                var str = string.Format("[+-] [{0}] 文件:{1} 有, 文件:{2} 无 ", xpath, _fileName1, _fileName2);

                if (m_mergeMode)
                {
                    //合并Node
                    LogHelper.Log.InfoFormat("合并: {0} 到配置文件", xpath);
                    XmlNode newNode = comparedDoc.CreateNode(xpath);
                    if (!string.IsNullOrEmpty(node.InnerText))
                    {
                        newNode.InnerText = node.InnerText;
                    }

                    // 合并属性
                    foreach (XmlAttribute nodeAttr in node.Attributes)
                    {
                        // 合并attr
                        LogHelper.Log.InfoFormat("合并参数: {0} {1}={2} 到配置文件", xpath, nodeAttr.Name, nodeAttr.Value);
                        comparedDoc.SetNodeAttr<string>(xpath, nodeAttr.Name, nodeAttr.Value);
                    }
                }

                if (!diff.Any(s => s.XPath == xpath && s.Name == null))
                {
                    diff.Add(new DiffModel { XPath = xpath, Name = null, OriginValue = null, CurrentValue = null, errorInfo = str });
                }
            }

            if (node.HasChildNodes)
            {
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (n.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    NodesTravel(n);
                    var aa = sb.ToString().TrimEnd('/');
                    var index = aa.LastIndexOf("/");
                    sb.Clear();
                    sb.Append(aa.Substring(0, index) + "/");
                }
            }
        }

        private void CompareNodeAttr(XmlNode node1, XmlNode node2, string xpath)
        {
            var attrs1 = node1.Attributes;
            var attrs2 = node2.Attributes;
            foreach (XmlAttribute attr1 in attrs1)
            {
                int i = 0;
                for (; i < attrs2.Count; i++)
                {
                    var attr2 = attrs2[i];
                    if (attr1.Name == attr2.Name)
                    {
                        if (attr1.Value != attr2.Value)
                        {
                            var str = string.Format("[!=] [{0}, {1}] 文件:{2}: {3}, 文件:{4}: {5}.", xpath, attr1.Name, _fileName1, attr1.Value, _fileName2, attr2.Value);
                            if (!diff.Any(s => s.XPath == xpath && s.Name == attr1.Name))
                            {
                                diff.Add(new DiffModel { XPath = xpath, Name = attr1.Name, OriginValue = attr1.Value, CurrentValue = attr2.Value, errorInfo = str });
                            }
                        }
                        break;
                    }
                }
                if (i >= attrs2.Count)
                {
                    var str = string.Format("[+-] [{0}, {1}] 文件:{2} 有, 文件:{3} 无", xpath, attr1.Name, _fileName1, _fileName2);

                    if (m_mergeMode)
                    {
                        // 合并attr
                        LogHelper.Log.InfoFormat("合并参数: {0} {1}={2} 到配置文件", xpath, attr1.Name, attr1.Value);
                        comparedDoc.SetNodeAttr<string>(xpath, attr1.Name, attr1.Value);
                    }

                    if (!diff.Any(s => s.XPath == xpath && s.Name == attr1.Name))
                    {
                        diff.Add(new DiffModel { XPath = xpath, Name = attr1.Name, OriginValue = attr1.Value, CurrentValue = null, errorInfo = str });
                    }
                }
            }
        }

        private void CompareNodeText(XmlNode node1, XmlNode node2, string xpath)
        {
            var text1 = string.Empty;
            var text2 = string.Empty;
            if (node1.ChildNodes.Count == 1 && node1.FirstChild.NodeType == XmlNodeType.Text)
            {
                text1 = node1.FirstChild.Value;
            }
            if (node2.ChildNodes.Count == 1 && node2.FirstChild.NodeType == XmlNodeType.Text)
            {
                text2 = node2.FirstChild.Value;
            }

            if (text1 != text2)
            {
                var str = string.Format("[!=] [{0}] 文件:{1}: {2}, 文件:{3}: {4}.", xpath, _fileName1, text1, _fileName2, text2);
                if (!diff.Any(s => s.XPath == xpath && s.Name == null))
                {
                    diff.Add(new DiffModel { XPath = xpath, Name = null, OriginValue = text1, CurrentValue = text2, errorInfo = str });
                }
            }
        }
    }

    internal class DiffModel
    {
        public string XPath { get; set; }

        public string Name { get; set; }

        public string OriginValue { get; set; }

        public string CurrentValue { get; set; }

        public string errorInfo { get; set; }
    }
}
