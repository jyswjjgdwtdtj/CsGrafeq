using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ParseHelper
{
    internal static class ParseHelperRegexp
    {
        public static Regex Regexp_LetterChar = new Regex("^[a-zA-Z]$",RegexOptions.IgnoreCase|RegexOptions.Compiled|RegexOptions.Singleline);
        public static Regex Regexp_NumberChar = new Regex("^[0-9]$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        public static Regex Regexp_Operator = new Regex("^([+*/^%<>=()\",&]|-)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        public static Regex Regexp_Variable = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        public static Regex Regexp_Number = new Regex(@"^[-+]?([0-9]+\.[0-9]+|[0-9]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        public static Regex Regexp_SpaceOrTab = new Regex(@"^([ ]|\t)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        public static Regex Regexp_Rem = new Regex(@"'.*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
    }
}
