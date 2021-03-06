﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.Strings
{
    public static class CStringExtensions
    {
        public static string TrimEnd(this string aText, string aTrim)
            => aText.EndsWith(aTrim)
            ? aText.Substring(0, aText.Length - aTrim.Length)
            : throw new ArgumentException();
        public static string TrimStart(this string aText, string aTrim)
                    => aText.StartsWith(aTrim)
                    ? aText.Substring(aTrim.Length)
                    : throw new ArgumentException();


        public static string JoinString(this IEnumerable<string> aStrings, string aSeperator = "")
        {
            var aStringBuilder = new StringBuilder();
            var aOpen = false;
            foreach (var aString in aStrings)
            {
                if (aOpen)
                    aStringBuilder.Append(aSeperator);
                aStringBuilder.Append(aString);
                aOpen = true;
            }
            var aJoinedString = aStringBuilder.ToString();
            return aJoinedString;
        }
    }
}
