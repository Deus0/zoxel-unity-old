using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using Microsoft.CSharp;

namespace AnimatorSystem
{
    public static class StringUtils
    {
        public static string CreateFileName(string name)
        {
            name = StripNonAlphanumDot(name);
            if (!Char.IsUpper(name, 0))
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                name = textInfo.ToTitleCase(name);
            }
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            return String.Join("_", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public static string StripNonAlphanumDot(string name)
        {
            Regex nonAlphanum = new Regex("[^a-zA-Z0-9.]");
            name = nonAlphanum.Replace(name, "");
            return name;
        }

        public static string ClampPath(string path, float width)
        {
            int len = (int) Mathf.Min(Mathf.RoundToInt(0.137f * width) + 3, width);
            var str = path;
            if (path.Length > len && len > 0)
            {
                var start = path.Length - len;
                var length = len;
                str = "..." + path.Substring(start, length);
            }
            return str;
        }

        public static string Combine(string path1, string path2)
        {
            return path1 + "/" + path2;
        }
    }
}
