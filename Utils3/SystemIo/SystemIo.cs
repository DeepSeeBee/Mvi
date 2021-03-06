using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils3.SystemIo
{
    public static class SystemIo
    {
        public static IEnumerable<FileInfo> GetFilesRecursive(this DirectoryInfo d)
        {
            foreach (var sd in d.GetDirectories())
                foreach (var f in sd.GetFilesRecursive())
                    yield return f;
            foreach (var f in d.GetFiles())
                yield return f;
        }
    }
}
