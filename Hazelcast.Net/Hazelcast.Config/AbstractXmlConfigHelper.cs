namespace Hazelcast.Config
{
    public abstract class AbstractXmlConfigHelper
    {
        //private static readonly ILogger logger = Logger.GetLogger(typeof(AbstractXmlConfigHelper));

        //protected internal bool domLevel3 = true;

        //public class IterableNodeList : IEnumerable<Node>
        //{
        //    private readonly NodeList parent;

        //    private readonly int maximum;

        //    private readonly short nodeType;

        //    public IterableNodeList(Node node) : this(node.GetChildNodes())
        //    {
        //    }

        //    public IterableNodeList(NodeList list) : this(list, (short)0)
        //    {
        //    }

        //    public IterableNodeList(Node node, short nodeType) : this(node.GetChildNodes(), nodeType)
        //    {
        //    }

        //    public IterableNodeList(NodeList parent, short nodeType)
        //    {
        //        this.parent = parent;
        //        this.nodeType = nodeType;
        //        this.maximum = parent.GetLength();
        //    }

        //    public override IEnumerator<Node> GetEnumerator()
        //    {
        //        return new _IEnumerator_61(this);
        //    }

        //    private sealed class _IEnumerator_61 : IEnumerator<Node>
        //    {
        //        public _IEnumerator_61(IterableNodeList _enclosing)
        //        {
        //            this._enclosing = _enclosing;
        //            this.index = 0;
        //        }

        //        private int index;

        //        private Node next;

        //        private bool FindNext()
        //        {
        //            this.next = null;
        //            for (; this.index < this._enclosing.maximum; this.index++)
        //            {
        //                Node item = this._enclosing.parent.Item(this.index);
        //                if (this._enclosing.nodeType == 0 || item.GetNodeType() == this._enclosing.nodeType)
        //                {
        //                    this.next = item;
        //                    return true;
        //                }
        //            }
        //            return false;
        //        }

        //        public override bool HasNext()
        //        {
        //            return this.FindNext();
        //        }

        //        public override Node Next()
        //        {
        //            if (this.FindNext())
        //            {
        //                this.index++;
        //                return this.next;
        //            }
        //            throw new NoSuchElementException();
        //        }

        //        public override void Remove()
        //        {
        //            throw new NotSupportedException();
        //        }

        //        private readonly IterableNodeList _enclosing;
        //    }
        //}

        //protected internal virtual string XmlToJavaName(string name)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    char[] charArray = name.ToCharArray();
        //    bool dash = false;
        //    StringBuilder token = new StringBuilder();
        //    foreach (char aCharArray in charArray)
        //    {
        //        if (aCharArray == '-')
        //        {
        //            AppendToken(builder, token);
        //            dash = true;
        //            continue;
        //        }
        //        token.Append(dash ? System.Char.ToUpper(aCharArray) : aCharArray);
        //        dash = false;
        //    }
        //    AppendToken(builder, token);
        //    return builder.ToString();
        //}

        //protected internal virtual void AppendToken(StringBuilder builder, StringBuilder token)
        //{
        //    string @string = token.ToString();
        //    if ("Jvm".Equals(@string))
        //    {
        //        @string = "JVM";
        //    }
        //    builder.Append(@string);
        //    token.Length = 0;
        //}

        //protected internal virtual string GetTextContent(Node node)
        //{
        //    if (node != null)
        //    {
        //        string text;
        //        if (domLevel3)
        //        {
        //            text = node.GetTextContent();
        //        }
        //        else
        //        {
        //            text = GetTextContentOld(node);
        //        }
        //        return text != null ? text.Trim() : string.Empty;
        //    }
        //    return string.Empty;
        //}

        //private string GetTextContentOld(Node node)
        //{
        //    Node child = node.GetFirstChild();
        //    if (child != null)
        //    {
        //        Node next = child.GetNextSibling();
        //        if (next == null)
        //        {
        //            return HasTextContent(child) ? child.GetNodeValue() : string.Empty;
        //        }
        //        StringBuilder buf = new StringBuilder();
        //        AppendTextContents(node, buf);
        //        return buf.ToString();
        //    }
        //    return string.Empty;
        //}

        //private void AppendTextContents(Node node, StringBuilder buf)
        //{
        //    Node child = node.GetFirstChild();
        //    while (child != null)
        //    {
        //        if (HasTextContent(child))
        //        {
        //            buf.Append(child.GetNodeValue());
        //        }
        //        child = child.GetNextSibling();
        //    }
        //}

        //protected internal bool HasTextContent(Node node)
        //{
        //    short nodeType = node.GetNodeType();
        //    return nodeType != Node.CommentNode && nodeType != Node.ProcessingInstructionNode;
        //}

        //public string CleanNodeName(Node node)
        //{
        //    return CleanNodeName(node.GetNodeName());
        //}

        //public static string CleanNodeName(string nodeName)
        //{
        //    string name = nodeName;
        //    if (name != null)
        //    {
        //        name = nodeName.ReplaceAll("\\w+:", string.Empty).ToLower();
        //    }
        //    return name;
        //}

        //protected internal virtual bool CheckTrue(string value)
        //{
        //    return Sharpen.Runtime.EqualsIgnoreCase("true", value) || Sharpen.Runtime.EqualsIgnoreCase("yes", value) || Sharpen.Runtime.EqualsIgnoreCase("on", value);
        //}

        //protected internal virtual int GetIntegerValue(string parameterName, string value, int defaultValue)
        //{
        //    try
        //    {
        //        return System.Convert.ToInt32(value);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Info(parameterName + " parameter value, [" + value + "], is not a proper integer. Default value, [" + defaultValue + "], will be used!");
        //        logger.Warning(e);
        //        return defaultValue;
        //    }
        //}

        //protected internal virtual string GetAttribute(Node node, string attName)
        //{
        //    Node attNode = node.GetAttributes().GetNamedItem(attName);
        //    if (attNode == null)
        //    {
        //        return null;
        //    }
        //    return GetTextContent(attNode);
        //}

        //protected internal virtual SocketInterceptorConfig ParseSocketInterceptorConfig(Node node)
        //{
        //    SocketInterceptorConfig socketInterceptorConfig = new SocketInterceptorConfig();
        //    NamedNodeMap atts = node.GetAttributes();
        //    Node enabledNode = atts.GetNamedItem("enabled");
        //    bool enabled = enabledNode != null ? CheckTrue(GetTextContent(enabledNode).Trim()) : false;
        //    socketInterceptorConfig.SetEnabled(enabled);
        //    foreach (Node n in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(n.GetNodeName());
        //        if ("class-name".Equals(nodeName))
        //        {
        //            socketInterceptorConfig.SetClassName(GetTextContent(n).Trim());
        //        }
        //        else
        //        {
        //            if ("properties".Equals(nodeName))
        //            {
        //                FillProperties(n, socketInterceptorConfig.GetProperties());
        //            }
        //        }
        //    }
        //    return socketInterceptorConfig;
        //}

        //protected internal virtual void FillProperties(Node node, Properties properties)
        //{
        //    if (properties == null)
        //    {
        //        return;
        //    }
        //    foreach (Node n in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        if (n.GetNodeType() == Node.TextNode || n.GetNodeType() == Node.CommentNode)
        //        {
        //            continue;
        //        }
        //        string name = CleanNodeName(n.GetNodeName());
        //        string propertyName;
        //        if ("property".Equals(name))
        //        {
        //            propertyName = GetTextContent(n.GetAttributes().GetNamedItem("name")).Trim();
        //        }
        //        else
        //        {
        //            // old way - probably should be deprecated
        //            propertyName = name;
        //        }
        //        string value = GetTextContent(n).Trim();
        //        properties.SetProperty(propertyName, value);
        //    }
        //}

        //protected internal virtual SerializationConfig ParseSerialization(Node node)
        //{
        //    SerializationConfig serializationConfig = new SerializationConfig();
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string name = CleanNodeName(child);
        //        if ("portable-version".Equals(name))
        //        {
        //            string value = GetTextContent(child);
        //            serializationConfig.SetPortableVersion(GetIntegerValue(name, value, 0));
        //        }
        //        else
        //        {
        //            if ("check-class-def-errors".Equals(name))
        //            {
        //                string value = GetTextContent(child);
        //                serializationConfig.SetCheckClassDefErrors(CheckTrue(value));
        //            }
        //            else
        //            {
        //                if ("use-native-byte-order".Equals(name))
        //                {
        //                    serializationConfig.SetUseNativebool(CheckTrue(GetTextContent(child)));
        //                }
        //                else
        //                {
        //                    if ("byte-order".Equals(name))
        //                    {
        //                        string value = GetTextContent(child);
        //                        bool bool = null;
        //                        if (bool.BigEndian.ToString().Equals(value))
        //                        {
        //                            bool = bool.BigEndian;
        //                        }
        //                        else
        //                        {
        //                            if (bool.LittleEndian.ToString().Equals(value))
        //                            {
        //                                bool = bool.LittleEndian;
        //                            }
        //                        }
        //                        serializationConfig.SetBigEndian(bool != null ? bool : bool.BigEndian);
        //                    }
        //                    else
        //                    {
        //                        if ("enable-compression".Equals(name))
        //                        {
        //                            serializationConfig.SetEnableCompression(CheckTrue(GetTextContent(child)));
        //                        }
        //                        else
        //                        {
        //                            if ("enable-shared-object".Equals(name))
        //                            {
        //                                serializationConfig.SetEnableSharedObject(CheckTrue(GetTextContent(child)));
        //                            }
        //                            else
        //                            {
        //                                if ("allow-unsafe".Equals(name))
        //                                {
        //                                    serializationConfig.SetAllowUnsafe(CheckTrue(GetTextContent(child)));
        //                                }
        //                                else
        //                                {
        //                                    if ("data-serializable-factories".Equals(name))
        //                                    {
        //                                        FillDataSerializableFactories(child, serializationConfig);
        //                                    }
        //                                    else
        //                                    {
        //                                        if ("portable-factories".Equals(name))
        //                                        {
        //                                            FillPortableFactories(child, serializationConfig);
        //                                        }
        //                                        else
        //                                        {
        //                                            if ("serializers".Equals(name))
        //                                            {
        //                                                FillSerializers(child, serializationConfig);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return serializationConfig;
        //}

        //protected internal virtual void FillDataSerializableFactories(Node node, SerializationConfig serializationConfig)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string name = CleanNodeName(child);
        //        if ("data-serializable-factory".Equals(name))
        //        {
        //            string value = GetTextContent(child);
        //            Node factoryIdNode = child.GetAttributes().GetNamedItem("factory-id");
        //            if (factoryIdNode == null)
        //            {
        //                throw new ArgumentException("'factory-id' attribute of 'data-serializable-factory' is required!");
        //            }
        //            int factoryId = System.Convert.ToInt32(GetTextContent(factoryIdNode));
        //            serializationConfig.AddDataSerializableFactoryClass(factoryId, value);
        //        }
        //    }
        //}

        //protected internal virtual void FillPortableFactories(Node node, SerializationConfig serializationConfig)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string name = CleanNodeName(child);
        //        if ("portable-factory".Equals(name))
        //        {
        //            string value = GetTextContent(child);
        //            Node factoryIdNode = child.GetAttributes().GetNamedItem("factory-id");
        //            if (factoryIdNode == null)
        //            {
        //                throw new ArgumentException("'factory-id' attribute of 'portable-factory' is required!");
        //            }
        //            int factoryId = System.Convert.ToInt32(GetTextContent(factoryIdNode));
        //            serializationConfig.AddPortableFactoryClass(factoryId, value);
        //        }
        //    }
        //}

        //protected internal virtual void FillSerializers(Node node, SerializationConfig serializationConfig)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string name = CleanNodeName(child);
        //        string value = GetTextContent(child);
        //        if ("serializer".Equals(name))
        //        {
        //            SerializerConfig serializerConfig = new SerializerConfig();
        //            serializerConfig.SetClassName(value);
        //            string typeClassName = GetAttribute(child, "type-class");
        //            serializerConfig.SetTypeClassName(typeClassName);
        //            serializationConfig.AddSerializerConfig(serializerConfig);
        //        }
        //        else
        //        {
        //            if ("global-serializer".Equals(name))
        //            {
        //                GlobalSerializerConfig globalSerializerConfig = new GlobalSerializerConfig();
        //                globalSerializerConfig.SetClassName(value);
        //                serializationConfig.SetGlobalSerializerConfig(globalSerializerConfig);
        //            }
        //        }
        //    }
        //}
    }
}