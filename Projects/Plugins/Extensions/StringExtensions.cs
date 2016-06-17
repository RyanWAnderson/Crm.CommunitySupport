using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.CommunitySupport.Extensions {
    public static partial class StringExtensions {
        /// <summary>
        /// Add spaces after line breaks
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string IndentNewLines(this string s) {
            return System.Text.RegularExpressions.Regex.Replace(s,"\n", "\n  ");
        }
    }
}
