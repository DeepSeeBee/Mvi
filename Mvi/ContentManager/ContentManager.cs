using CharlyBeck.Utils3.SystemIo;
using CharlyBeck.Utils3.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Utils3.Strings;
using CharlyBeck.Utils3.LazyLoad;

namespace CharlyBeck.Mvi.ContentManager
{

    internal enum CContentTypeEnum
    {
        Audio
    }

    public sealed class CContentManager : CServiceLocatorNode
    {
        internal CContentManager(CServiceLocatorNode aParent) :base(aParent)
        {
        }

        internal static DirectoryInfo GetBaseDirectoryInfo()
             => new DirectoryInfo(Path.Combine(new FileInfo(typeof(CContentManager).Assembly.Location).Directory.FullName, "Content"));

        internal static string TrimBaseDirectory(DirectoryInfo aDir)
            => aDir.FullName.TrimStart(GetBaseDirectoryInfo().FullName).TrimStartPathSeperator();

        public static string TrimBaseDirectory(FileInfo aFileInfo)
            => aFileInfo.FullName.TrimStart(Path.Combine(GetBaseDirectoryInfo().FullName, "dummy").TrimEnd("dummy"));

        private IEnumerable<string> ContentListDicM;
        private IEnumerable<string> ContentList => CLazyLoad.Get(ref this.ContentListDicM, this.NewContentListDic);
        private IEnumerable<string> NewContentListDic()
        {
            var aType = this.GetType();
            var aStream = aType.Assembly.GetManifestResourceStream("CharlyBeck.Mvi.Content.Content.txt");
            var aLines = aStream.ReadAllLines();
            return aLines;
        }

        internal IEnumerable<FileInfo> GetDirectoryContent(DirectoryInfo aDirectoryInfo)
            => GetDirectoryContent(TrimBaseDirectory(aDirectoryInfo)).Select(aItem=>new FileInfo(Path.Combine(GetBaseDirectoryInfo().FullName, aItem)));
        internal IEnumerable<string> GetDirectoryContent(string aPrefix)
            => this.ContentList.Where(aItem => aItem.StartsWith(aPrefix));

        public static void BuildContentList()
        {
            var aContentTypeEnums = typeof(CContentTypeEnum).GetEnumValues().Cast<CContentTypeEnum>();
            var aDir1 = new DirectoryInfo(@"C:\Karle\Cloud\GoogleDrive\Ablage\Dev\Mvi\MviMono\Content");
            var aDir3 = new DirectoryInfo(Path.Combine(aDir1.FullName, "..", "..", "Mvi", "Content"));
            var aLines = new List<string>();

            foreach (var aContentType in aContentTypeEnums)
            {
                var aDir2 = new DirectoryInfo(Path.Combine(aDir1.FullName, aContentType.ToString()));
                var aFiles1 = aDir2.GetFilesRecursive();
                var aExtensions = new string[] { ".mp3" };
                var aFiles2 = aFiles1.Where(aFile => aExtensions.Contains(aFile.Extension.ToLower()));
                var aFiles = aFiles2;
                var aContentTypeLines = aFiles.Select(aFile => aFile.FullName.TrimStart(aDir1.FullName).TrimStartPathSeperator().TrimEnd(aFile.Extension));
                aLines.AddRange(aContentTypeLines);
            }
            var aContentListFile = new FileInfo(System.IO.Path.Combine(aDir3.FullName, "Content.txt"));
            File.WriteAllLines(aContentListFile.FullName, aLines);
        }
    }
}
