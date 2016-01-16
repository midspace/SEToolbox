namespace SEToolbox.Support
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Text.RegularExpressions;

    using SEToolbox.Controls;

    public enum CodeRepositoryType { CodePlex, GitHub }

    /// <summary>
    /// Extracts the GitHub website information to determine the version of the current release.
    /// </summary>
    public class CodeRepositoryReleases
    {
        /// <summary>
        /// search for html in the form:  <h1 class="release-title"><a href="/midspace/SEToolbox/releases/tag/v1.117.002.1">SEToolbox 01.117.002 Release 1</a></h1>
        /// </summary>
        const string GitHubPattern = @"\<h1\s+class\s*=\s*\""release\-title\""\>\s*\<a\s+href\s*=\s*(?:""(?<url>[^""]|.*?)"")\s*\>\s*(?<title>(?:[^\<\>\""]*?))\s(?<version>[^\<\>\""]*)\<\/a\>";

        /// <summary>
        /// search for html in the form:  <h1 class="page_title wordwrap">SEToolbox 01.025.021 Release 2</h1> 
        /// </summary>
        const string  CodePlexPattern = @"\<h1 class=\""(?:[^\""]*)\""\>(?<title>(?:[^\<\>\""]*?))\s(?<version>[^\<\>\""]*)\<\/h1\>";

        #region CheckForUpdates

        public static ApplicationRelease CheckForUpdates(CodeRepositoryType repositoryType,  string updatesUrl)
        {
            if (GlobalSettings.Default.AlwaysCheckForUpdates.HasValue && !GlobalSettings.Default.AlwaysCheckForUpdates.Value)
                return null;

#if DEBUG
            // Skip the load check, as it may take a few seconds during development.
            if (Debugger.IsAttached)
                return null;
#endif

            var currentVersion = GlobalSettings.GetAppVersion();
            string webContent;
            string link;

            // Create the WebClient with Proxy Credentials.
            using (var webclient = new MyWebClient())
            {
                try
                {
                    if (webclient.Proxy != null)
                    {
                        // For Proxy servers on Corporate networks.
                        webclient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }
                }
                catch
                {
                    // Reading from the Proxy may cause a network releated error.
                    // Error creating the Web Proxy specified in the 'system.net/defaultProxy' configuration section. 
                }
                webclient.Headers.Add(HttpRequestHeader.UserAgent, string.Format("Mozilla/5.0 ({0}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.114 Safari/537.36", Environment.OSVersion)); // Crude useragent.

                try
                {
                    webContent = webclient.DownloadString(updatesUrl);
                }
                catch
                {
                    // Ignore any errors.
                    // If it cannot connect, then there may be an intermittant connection issue, either with the internet, or the website itself.
                    return null;
                }

                if (string.IsNullOrEmpty(webContent))
                    return null;

                // link should be in the form "http://setoolbox.codeplex.com/releases/view/120855"
                link = webclient.ResponseUri == null ? null : webclient.ResponseUri.AbsoluteUri;
            }

            string pattern = "";
            if (repositoryType == CodeRepositoryType.CodePlex)
                pattern = CodePlexPattern;
            if (repositoryType == CodeRepositoryType.GitHub)
                pattern = GitHubPattern;

            var match = Regex.Match(webContent, pattern);

            if (!match.Success)
                return null;

            var item = new ApplicationRelease { Link = link, Version = GetVersion(match.Groups["version"].Value) };
            Version ignoreVersion;
            Version.TryParse(GlobalSettings.Default.IgnoreUpdateVersion, out ignoreVersion);
            if (item.Version > currentVersion && item.Version != ignoreVersion)
                return item;

            return null;
        }

        #endregion

        #region GetVersion

        private static Version GetVersion(string version)
        {
            var match = Regex.Match(version, @"(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+)\sRelease\s(?<v4>\d+)");
            if (match.Success)
            {
                return new Version(match.Groups["v1"].Value + "." + match.Groups["v2"].Value + "." + match.Groups["v3"].Value + "." + match.Groups["v4"].Value);
            }

            match = Regex.Match(version, @"(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+).(?<v4>\d+)");
            if (match.Success)
            {
                return new Version(match.Groups["v1"].Value + "." + match.Groups["v2"].Value + "." + match.Groups["v3"].Value + "." + match.Groups["v4"].Value);
            }

            return new Version(0, 0, 0, 0);
        }

        #endregion
    }

    public class ApplicationRelease
    {
        public string Link { get; set; }
        public Version Version { get; set; }
    }
}
