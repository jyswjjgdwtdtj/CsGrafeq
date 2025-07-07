using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ScriptCompilerEngine.ParseHelper
{
    internal static class ReservedWords
    {
        internal static string[] Words=new string[]{
            "Eqv","And", "Not", "Or", "Xor", "Imp",
            "Dim", "ReDim", "Preserve",
            "If", "Else", "ElseIf", "Then", "End",
            "For", "Next", "Each", "To", "In", "Step",
            "Do", "While", "Loop", "Until",
            "Exit","Return"
        };
        public static bool Contain(ref string word)
        {
            word=word.ToLower();
            foreach(var i in Words)
            {
                if (i.ToLower() == word)
                {
                    word = i;
                    return true;
                }
            }
            return false;
        }
    }
}
