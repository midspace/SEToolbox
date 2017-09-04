namespace SEToolbox.Support
{
    using System.Web.UI;

    internal static class HtmlExtensions
    {
        #region BeginDocument

        internal static void BeginDocument(this HtmlTextWriter writer, string title, string inlineStyleSheet)
        {
            writer.AddAttribute("http-equiv", "Content-Type");
            writer.AddAttribute("content", "text/html;charset=UTF-8");
            writer.RenderBeginTag(HtmlTextWriterTag.Meta);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Style);
            writer.Write(inlineStyleSheet);
            writer.RenderEndTag(); // Style

            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.Write(title);
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Body);
        }

        #endregion

        #region EndDocument

        internal static void EndDocument(this HtmlTextWriter writer)
        {
            writer.RenderEndTag(); // Body
            writer.RenderEndTag(); // Html
        }

        #endregion

        #region RenderElement

        internal static void RenderElement(this HtmlTextWriter writer, HtmlTextWriterTag tag)
        {
            writer.RenderBeginTag(tag);
            writer.RenderEndTag();
        }

        internal static void RenderElement(this HtmlTextWriter writer, HtmlTextWriterTag tag, object value)
        {
            writer.RenderElement(tag, value.ToString());
        }

        internal static void RenderElement(this HtmlTextWriter writer, HtmlTextWriterTag tag, string format, params object[] arg)
        {
            writer.RenderBeginTag(tag);
            if (format != null)
                writer.Write(format, arg);
            writer.RenderEndTag();
        }

        #endregion

        #region BeginTable

        internal static void BeginTable(this HtmlTextWriter writer, string border, string cellpadding, string cellspacing, string[] headings)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Border, border);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, cellpadding);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, cellspacing);
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Thead);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            foreach (var header in headings)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(header);
                writer.RenderEndTag(); // Td
            }

            writer.RenderEndTag(); // Tr
            writer.RenderEndTag(); // Thead
        }

        #endregion

        #region EndTable

        internal static void EndTable(this HtmlTextWriter writer)
        {
            writer.RenderEndTag(); // Table
        }

        #endregion
    }
}
