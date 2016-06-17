using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Crm.CommunitySupport.Plugins {
    /// <summary>
    /// Class to add Dictionary (Key:Value) parsing to plugin configuration
    /// </summary>
    public class PluginConfiguration {
        #region Constructor(s)
        public PluginConfiguration(string unsecure, string secure) {
            this.Unsecure = unsecure;
            this.Secure = secure;

            this.UnsecureDictionary = parseStringIntoDict(unsecure);
            this.SecureDictionary = parseStringIntoDict(secure);
        }
        #endregion

        private static IReadOnlyDictionary<string, string> parseStringIntoDict(string s) {
            StringDictionary dict = new StringDictionary();

            try /*weakly*/ {
                foreach (string line in Regex.Split(s, Environment.NewLine)) {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
                        continue;

                    string[] parts = line.Split(new char[] { ':' }, 2);
                    dict.Add(parts[0], parts[1]);
                }
            }
            catch (Exception) {
                // swallow ex
            }

            return (IReadOnlyDictionary<string, string>)dict;
        }

        public readonly string Unsecure;
        public readonly IReadOnlyDictionary<string, string> UnsecureDictionary;

        public readonly string Secure;
        public readonly IReadOnlyDictionary<string, string> SecureDictionary;
    }
}
