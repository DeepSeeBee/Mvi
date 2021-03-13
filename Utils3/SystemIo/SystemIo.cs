using CharlyBeck.Utils3.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.SystemIo
{
    public static class CSystemIo
    {
        public static IEnumerable<FileInfo> GetFilesRecursive(this DirectoryInfo d)
        {
            foreach (var sd in d.GetDirectories())
                foreach (var f in sd.GetFilesRecursive())
                    yield return f;
            foreach (var f in d.GetFiles())
                yield return f;
        }

        internal static readonly string PathSeperator = Path.Combine("a", "b").TrimStart('a').TrimEnd('b');

        public static string TrimStartPathSeperator(this string s)
            => s.TrimStart(PathSeperator);

        public static IEnumerable<string> ReadAllLines(this StreamReader aStreamReader)
        {
            var aLines = new List<string>();
            while (!aStreamReader.EndOfStream)
                aLines.Add(aStreamReader.ReadLine());
            return aLines;
        }
        public static IEnumerable<string> ReadAllLines(this Stream aStream)
            => new StreamReader(aStream).ReadAllLines();


    }
}
