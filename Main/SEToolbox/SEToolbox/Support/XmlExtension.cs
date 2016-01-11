namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Media.Media3D;
    using System.Xml;
    using System.Xml.XPath;

    internal static class XmlExtension
    {
        #region BuildXmlNamespaceManager

        internal static XmlNamespaceManager BuildXmlNamespaceManager(this XmlDocument document)
        {
            var nav = document.CreateNavigator();
            var manager = new XmlNamespaceManager(nav.NameTable);

            // Fetch out the namespace from the file. This is hacky approach.
            var matches = Regex.Matches(document.InnerXml, @"(?:\bxmlns:?(?<schema>[^=]*)=[""](?<key>[^""]*)""[\s>])");
            foreach (Match match in matches)
            {
                var schemaName = match.Groups["schema"].Value;
                if (string.IsNullOrEmpty(schemaName))
                {
                    manager.AddNamespace("", match.Groups["key"].Value);
                }
                else
                {
                    manager.AddNamespace(schemaName, match.Groups["key"].Value);
                }
            }

            return manager;
        }

        #endregion

        #region GetXMLObject

        /// <summary>
        /// Helper extension method to load string data from XML node values into native types that aren't necessarily serializable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="navRoot"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static T ToValue<T>(this XPathNavigator navRoot, string name)
        {
            object item = null;
            var node = navRoot.SelectSingleNode(name);
            if (node != null)
            {
                item = node.Value;
            }

            if (typeof(T).Equals(typeof(string)))
            {
                return (T)item;
            }

            if (typeof(T).Equals(typeof(int)))
            {
                item = Convert.ToInt32(item);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(long)))
            {
                item = Convert.ToInt64(item);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(IntPtr)))
            {
                // The Convert must be big enough to convert a pointer from a x64 system.
                item = new IntPtr(Convert.ToInt64(item));
                return (T)item;
            }

            if (typeof(T).Equals(typeof(double)))
            {
                item = Convert.ToDouble(item, CultureInfo.InvariantCulture);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(DateTime)))
            {
                item = DateTime.Parse((string)item, null);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(DateTimeOffset)))
            {
                item = DateTimeOffset.Parse((string)item, null);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(bool)))
            {
                int result;
                if (Int32.TryParse((string)item, out result))
                {
                    item = Convert.ToBoolean(result);
                }
                else
                {
                    item = Convert.ToBoolean((string)item);
                }
                return (T)item;
            }

            if (typeof(T).Equals(typeof(Guid)))
            {
                item = new Guid((string)item);
                return (T)item;
            }

            if (typeof(T).BaseType.Equals(typeof(Enum)))
            {
                item = Enum.Parse(typeof(T), (string)item);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(CultureInfo)))
            {
                item = CultureInfo.GetCultureInfoByIetfLanguageTag((string)item);
                return (T)item;
            }

            if (typeof(T).Equals(typeof(Point3D)))
            {
                item = new Point3D(Convert.ToDouble(node.SelectSingleNode("X").Value, CultureInfo.InvariantCulture), Convert.ToDouble(node.SelectSingleNode("Y").Value, CultureInfo.InvariantCulture), Convert.ToDouble(node.SelectSingleNode("Z").Value, CultureInfo.InvariantCulture));
                return (T)item;
            }

            //if (typeof(T).Equals(typeof(Rect)))
            //{
            //    item = new RectConverter().ConvertFromString((string)item);
            //    return (T)item;
            //}

            throw new NotImplementedException(string.Format("The datatype [{0}] has not been catered for.", typeof(T).Name));
            ////return (T)item;
            ////object ret = null;
            ////return (T)ret;
        }

        #endregion

        #region WriteElementFormat

        internal static void WriteElementFormat(this XmlWriter writer, string localName, string format, params object[] arg)
        {
            writer.WriteElementString(localName, string.Format(format, arg));
        }

        #endregion

        #region WriteAttributeFormat

        internal static void WriteAttributeFormat(this XmlWriter writer, string localName, string format, params object[] arg)
        {
            writer.WriteAttributeString(localName, string.Format(format, arg));
        }

        #endregion
    }
}
