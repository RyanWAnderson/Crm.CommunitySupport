namespace Crm.CommunitySupport.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class to add Dictionary (Key:Value) parsing to plugin configuration
    /// </summary>
    public class PluginConfiguration
    {
        #region Constructor(s)
        public PluginConfiguration(string unsecure, string secure)
        {
            Unsecure = unsecure;
            Secure = secure;

            UnsecureDictionary = PluginConfiguration.parseStringIntoDict(unsecure);
            SecureDictionary = PluginConfiguration.parseStringIntoDict(secure);
        }
        #endregion

        private static IReadOnlyDictionary<string, string> parseStringIntoDict(string s)
        {
            var dict = new Dictionary<string, string>();

            try /*weakly*/
            {
                foreach (var line in Regex.Split(s, Environment.NewLine))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
                    {
                        continue;
                    }

                    var parts = line.Split(new char[] { ':' }, 2);
                    dict.Add(parts[0], parts[1]);
                }
            }
            catch (Exception)
            {
                // swallow ex
            }

            return dict;
        }

        public readonly string Unsecure;
        public readonly IReadOnlyDictionary<string, string> UnsecureDictionary;

        public readonly string Secure;
        public readonly IReadOnlyDictionary<string, string> SecureDictionary;
    }
}
