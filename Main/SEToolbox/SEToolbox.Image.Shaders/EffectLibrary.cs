// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


namespace SEToolbox.ImageShaders
{
    using System;
    using System.Reflection;
    using System.Text;

    internal static class Global
    {
        public static Uri MakePackUri(string relativeFile)
        {
            var uriString = new StringBuilder();
#if !SILVERLIGHT
            uriString.Append("pack://application:,,,");
#endif
            uriString.Append("/" + AssemblyShortName + ";component/" + relativeFile);
            return new Uri(uriString.ToString(), UriKind.RelativeOrAbsolute);
        }

        private static string AssemblyShortName
        {
            get
            {
                if (_assemblyShortName == null)
                {
                    Assembly a = typeof(Global).Assembly;

                    // Pull out the short name.
                    _assemblyShortName = a.ToString().Split(',')[0];
                }

                return _assemblyShortName;
            }
        }

        private static string _assemblyShortName;
    }
}
