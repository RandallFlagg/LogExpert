using LogExpert;

namespace GlassfishColumnizer
{
    internal class XmlConfig : IXmlLogConfiguration
    {
        #region Properties

        public string XmlStartTag { get; } = "[#|";

        public string XmlEndTag { get; } = "|#]";

        public string Stylesheet { get; } = null;

        public string[] Namespace => null;

        #endregion
    }
}