namespace SEToolbox.Support
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extracts the CodePlex feed information to determine version and Date detail.
    /// </summary>
    public class RssFeedItem
    {
        //<rss version="2.0">
        //<channel>
        //  <title>SEToolbox Releases Rss Feed</title>
        //  <link>https://setoolbox.codeplex.com/releases</link>
        //  <description>SEToolbox Releases Rss Description</description>
        //  <item>
        //    <title>Updated Release: SEToolbox 01.009.009 Release 1 (Dec 06, 2013)</title>

        public string Title { get; set; }
        public string Link { get; set; }
        public string Version { get; set; }

        #region methods

        public Version GetVersion()
        {
            if (this.Version == null)
            {
                var match = Regex.Match(this.Title, @"\s(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+)\sRelease\s(?<v4>\d+)\s");
                if (match.Success)
                {
                    this.Version = match.Groups["v1"].Value + "." + match.Groups["v2"].Value + "." + match.Groups["v3"].Value + "." + match.Groups["v4"].Value;
                    return new Version(this.Version);
                }

                match = Regex.Match(this.Title, @"\s(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+).(?<v4>\d+)\s");
                if (match.Success)
                {
                    this.Version = match.Groups["v1"].Value + "." + match.Groups["v2"].Value + "." + match.Groups["v3"].Value + "." + match.Groups["v4"].Value;
                    return new Version(this.Version);
                }
                return new Version(0, 0, 0, 0);
            }
            else
            {
                return new Version(this.Version);
            }
        }

        public DateTime GetReleaseDate()
        {
            var match = Regex.Match(this.Title, @"\((?<date>.*)\)$");
            if (match.Success)
            {
                return DateTime.Parse(match.Groups["date"].Value);
            }

            return DateTime.MinValue;
        }

        #endregion
    }
}
