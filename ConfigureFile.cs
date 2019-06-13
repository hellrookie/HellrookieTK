using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 配置文件。xml
    /// </summary>
    public class ConfigureFile
    {
        private XmlDocument doc;
        private ILogger log ;
        private XmlElement rootNode;
        private readonly string xmlInitialHeader = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<Config>";
        private readonly string xmlInitialTail = "</Config>";
        private string path;

        /// <summary>
        /// Open or create a config file with specify content in xmlFile.
        /// </summary>
        /// <param name="path">Path to create config file.</param>
        /// <param name="createIfNotExist">Create a new configure file if not exist.</param>
        /// <param name="initialContent">The initial content for new create file, should not contain xml header.</param>
        public ConfigureFile(string path, bool createIfNotExist, string initialContent = null)
        {
            this.log = AppDomainLogger.CreateLogger(Process.GetCurrentProcess().ProcessName);
            this.path = path;

            // create a new config file when not exist.
            if (!File.Exists(path))
            {
                if (createIfNotExist)
                {
                    log.Debug("Configure file not exist, path: {0}. Try to create one.", path);
                    try
                    {
                        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            string xmlContent;
                            if(!string.IsNullOrEmpty(initialContent))
                            {
                                xmlContent = string.Format("{0}\r\n{1}\r\n{2}", xmlInitialHeader, initialContent, xmlInitialTail);
                            }
                            else
                            {
                                xmlContent = string.Format("{0}\r\n{1}", xmlInitialHeader, xmlInitialTail);
                            }
                            var data = Encoding.UTF8.GetBytes(xmlContent);
                            stream.Write(data, 0, data.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Fail to create xml file. Exception: {0}", ex.ToString());
                        throw new Exception("Fail to create xml file");
                    }
                }
                else
                {
                    var msg = string.Format("File not found, path: {0}.", path);
                    log.Error(msg);
                    throw new Exception(msg);
                }
            }

            try
            {
                doc = new XmlDocument();
                doc.Load(path);
                rootNode = doc.DocumentElement;
            }
            catch(Exception ex)
            {
                log.Error("Fail load xml file. Exception: {0}", ex.ToString());
                throw new Exception("Fail load xml file.");
            }
        }

        /// <summary>
        /// Get XmlNode from configure file.
        /// </summary>
        /// <param name="xpath">Xpath for node.</param>
        /// <param name="confirmAttributeName">The name of attribute to confirm.</param>
        /// <param name="confirmAttributeValue">The value of attribute to confirm.</param>
        /// <returns></returns>
        public XmlNode GetNode(string xpath, string confirmAttributeName, string confirmAttributeValue)
        {
            return SelectNode(xpath, confirmAttributeName, confirmAttributeValue);
        }

        public string GetNodeInnerText(string xpath, string confirmAttributeName, string confirmAttributeValue)
        {
            var node = GetNode(xpath, confirmAttributeName, confirmAttributeValue);
            return node.InnerText;
        }

        /// <summary>
        /// Get attribute by node path and specify attribute value.
        /// </summary>
        /// <param name="xpath">XPath for node.</param>
        /// <param name="confirmAttributeName">Specify attribute to check.</param>
        /// <param name="confirmAttributeValue">Attribute value to check.</param>
        /// <param name="returnAttributeName">Name of attribute to get.</param>
        /// <returns></returns>
        public string GetNodeAttribute(string xpath, string confirmAttributeName, string confirmAttributeValue, string returnAttributeName)
        {
            string value = string.Empty;
            var node = SelectNode(xpath, confirmAttributeName, confirmAttributeValue);
            if(node != null)
            {
                try
                {
                    value = node.Attributes[returnAttributeName].Value;
                }
                catch
                {
                    log.Warn("Cannot find attribute {0} in node with attribute {1} value is {2} in path {3}.", returnAttributeName, confirmAttributeName, confirmAttributeValue, xpath);
                }
            }
            return value;
        }

        /// <summary>
        /// Get attribute by node path.
        /// </summary>
        /// <param name="xpath">XPath for node.</param>
        /// <param name="returnAttributeName">Name of attribute to get.</param>
        /// <returns></returns>
        public string GetNodeAttribute(string xpath, string returnAttributeName)
        {
            return GetNodeAttribute(xpath, null, null, returnAttributeName);
        }

        /// <summary>
        /// Set attribute value for specify node by path and specify attribute value. Create if attribute not exist.
        /// </summary>
        /// <param name="xpath">XPath for node.</param>
        /// <param name="confirmAttributeName">Specify attribute to check.</param>
        /// <param name="confirmAttributeValue">Attribute value to check.</param>
        /// <param name="setAttributeName">Name of attribute to set.</param>
        /// <param name="setValue">Value of attribute to set.</param>
        public void SetNodeAttribute(string xpath, string confirmAttributeName, string confirmAttributeValue, string setAttributeName, string setValue)
        {
            var node = SelectNode(xpath, confirmAttributeName, confirmAttributeValue);
            if (node != null)
            {
                try
                {
                    SetNodeAttribute(node, setAttributeName, setValue);
                }
                catch
                {
                    log.Warn("Cannot find attribute {0} in node with attribute {1} value is {2} in path {3}.", setAttributeName, confirmAttributeName, confirmAttributeValue, xpath);
                }
            }
            else
            {
                log.Warn("Cannot find attribute {0} in node with attribute {1} value is {2} in path {3}.", setAttributeName, confirmAttributeName, confirmAttributeValue, xpath);
            }
        }

        /// <summary>
        /// Set attribute value for specify node. Create if attribute not exist.
        /// </summary>
        /// <param name="xpath">XPath for node.</param>
        /// <param name="setAttributeName">Name of attribute to set.</param>
        /// <param name="setValue">Value of attribute to set.</param>
        public void SetNodeAttribute(string xpath, string setAttributeName, string setValue)
        {
            var nodes = doc.SelectNodes(xpath);
            if (nodes.Count > 0)
            {
                SetNodeAttribute(nodes[0], setAttributeName, setValue);
            }
            else
            {
                log.Warn("Cannot find node in path {0}.", xpath);
            }
        }

        private void SetNodeAttribute(XmlNode node, string setAttributeName, string setValue)
        {
            try
            {
                var att = node.Attributes[setAttributeName];
                att.Value = setValue;
                SaveToPath(path);
            }
            catch
            {
                log.Warn("Cannot find attribute {0} in node {1}. Try create one.", setAttributeName, node.Name);
                try
                {
                    var att = doc.CreateAttribute(setAttributeName);
                    att.Value = setValue;
                    node.Attributes.Append(att);
                }
                catch (Exception ex)
                {
                    log.Warn("Cannot find create attribute for node {0}. Exception: {1}", node.Name, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Create a new Node to specify path.
        /// </summary>
        /// <param name="xpath">XPath to create node.</param>
        /// <param name="name">Name of new node.</param>
        public void CreateNode(string xpath, string name)
        {
            CreateNode(xpath, name, null, null);
        }

        /// <summary>
        /// Create a new Node to specify path and check by attribute.
        /// </summary>
        /// <param name="xpath">XPath to create node.</param>
        /// <param name="name">Name of new node.</param>
        /// <param name="confirmAttributeName">Specify attribute to check.</param>
        /// <param name="confirmAttributeValue">Attribute value to check.</param>
        public void CreateNode(string xpath, string name, string confirmAttributeName, string confirmAttributeValue)
        {
            XmlNode parentNode;
            if (!string.IsNullOrEmpty(confirmAttributeName) && confirmAttributeValue != null)
            {
                parentNode = SelectNode(xpath, confirmAttributeName, confirmAttributeValue);
            }
            else
            {
                parentNode = doc.SelectSingleNode(xpath);
            }

            if (parentNode != null)
            {
                var node = doc.CreateElement(name);
                parentNode.AppendChild(node);
            }
        }

        /// <summary>
        /// Save the config file.
        /// </summary>
        public void Save()
        {
            doc.Save(this.path);
        }

        /// <summary>
        /// Save the config file to specify path.
        /// </summary>
        /// <param name="path"></param>
        public void SaveToPath(string path)
        {
            doc.Save(path);
        }

        private XmlNode SelectNode(string xpath, string attributeName, string attributeValue)
        {
            var nodes = doc.SelectNodes(xpath);

            foreach (XmlNode node in nodes)
            {
                if (ConfirmNodeByAttribute(node, attributeName, attributeValue))
                {
                    return node;
                }
            }
            return null;
        }

        private bool ConfirmNodeByAttribute(XmlNode node, string attributeName, string attributeValue)
        {
            if (attributeName == null && attributeValue == null)
            {
                return true;
            }
            try
            {
                if (node.Attributes[attributeName].Value.Equals(attributeValue))
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                log.Error("Fail in ConfermNodeByAttribute. Exception: {0}.", ex.ToString());
            }
            return false;
        }
    }
}
