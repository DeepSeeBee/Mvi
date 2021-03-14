using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Utils3.Strings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.FontConvert
{
    public sealed class CTtfToBmpConverter
    {
        public FileInfo FontFileInfo;
        public void Convert()
        {
            var aFontFileInfo = new FileInfo(@"C:\Karle\Cloud\GoogleDrive\Ablage\Dev\Mvi\MviMono\Content\Font\ethnocentric rg.ttf");
            var aFonts = new PrivateFontCollection();
            aFonts.AddFontFile(aFontFileInfo.FullName);
            var aStartChar = (int)'\x20';
            var aEndChar = (int)'\x60';
            var aChars = Enumerable.Range(aStartChar, aEndChar - aStartChar).Select(i => ((char)i).ToString()).JoinString();
            var aAdjust = true;
            var aAdjustY = aAdjust ? 14 : 0; 
            var aAdjustXDefault = aAdjust ? 14 : 0;
            var aCharAdjustXDic = aChars.Select(c => ((char)c).ToString()).ToDictionary(c => c, c => aAdjustXDefault) ;
            if(aAdjust)
            {
                aCharAdjustXDic[" "] = 0;
                aCharAdjustXDic["_"] = 0;
            }
            var aCharAdjustX = aCharAdjustXDic.Values.ToArray();

            var aCharCount = aChars.Length;
            var aFontFamily = (FontFamily)aFonts.Families.GetValue(0);
            var aFontSize = 72;
            var aFont = new Font(aFontFamily, aFontSize, FontStyle.Regular);
            var aSize = Graphics.FromImage(new Bitmap(1,1)).MeasureString(aChars, aFont);
            var aSeperator = 1;
            var aMeasureGraphics = Graphics.FromImage(new Bitmap(1, 1));
            var aCharSizes1 = (from aChar in aChars select aMeasureGraphics.MeasureString(aChar.ToString(), aFont)).ToArray();
            var aCharSizes2 = (from s in aCharSizes1 select new Point((int)Math.Ceiling(s.Width), (int)Math.Ceiling(s.Height))).ToArray();
            var aCharWidths1 = aCharSizes2.Select(s => s.X).ToArray();
            var aCharHeights = aCharSizes2.Select(s => s.Y).ToArray();
            var aCharWidths2 = aCharCount.Range().Select(i => aCharWidths1[i] + aSeperator - aCharAdjustX[i] * (i == 0 ? 1 :2)).ToArray();
            var aDx = aCharWidths2.Sum() + aSeperator; 
            var aPos = aSeperator;
            var aCharXs = new int[aCharCount];
            foreach(var i in Enumerable.Range(0, aCharCount))
            {
                aCharXs[i] = aPos;
                aPos = aPos + aCharWidths2[i];
            }
            var aDy = aCharHeights.Max() + aSeperator + aSeperator - aAdjustY - aAdjustY;
            var aCharWidth = aCharWidths1.Max();
            var aCharHeight = aCharHeights.Max();
            var aCharBmp = new Bitmap((int)aCharWidth, (int)aCharHeight); 
            var aBmp = new Bitmap((int)aDx, (int)aDy);
            var aGraphics = Graphics.FromImage(aBmp);
            var aCharGraphics = Graphics.FromImage(aCharBmp);
            var aForegroundColor = Color.White; 
            var aCharBackgroundColor = Color.Black;
            var aBackgroundColor = Color.FromArgb(255, 0, 255);
            var aForegroundBrush = new SolidBrush(aForegroundColor);
            var aBackgroundBrush = new SolidBrush(aBackgroundColor);
            var aCharBackgroundBrush = new SolidBrush(aCharBackgroundColor);
            var aPen = new Pen(aBackgroundBrush);
            aGraphics.FillRectangle(aBackgroundBrush, 0, 0, aDx, aDy);

            foreach (var i in Enumerable.Range(0, aChars.Length))
            {
                var aChar = aChars[i].ToString();
                var aCharSize = aCharSizes2[i];
                var x = aCharXs[i];
                aCharGraphics.FillRectangle(aCharBackgroundBrush, 0, 0, aCharBmp.Width, aCharBmp.Height);
                aCharGraphics.DrawString(aChar, aFont, aForegroundBrush, 0, 0);
                var aAdjustXChar = aCharAdjustX[i];
                var aAdjustYChar = aAdjustY;
                var sx = aAdjustXChar;
                var sdx = aCharSize.X - aAdjustXChar - aAdjustXChar;
                var sy = aAdjustYChar;
                var sdy = aCharSize.Y - aAdjustYChar - aAdjustYChar;
                var aSrcRect = new Rectangle((int)sx, (int)sy, (int)sdx, (int)sdy);
                var aDstRect = new Rectangle((int)x, (int)aSeperator, (int)sdx, (int)sdy);
                aGraphics.DrawImage(aCharBmp, aDstRect, aSrcRect, GraphicsUnit.Pixel);
                System.Diagnostics.Debug.Print("Char(" + aChar + ").X=" + x.ToString());
                //x = x + aCharSize.X + aSeperator - aAdjustXChar - aAdjustXChar;
            }
            var aSaveBmp = aBmp.Clone(new Rectangle(0, 0, aBmp.Width, aBmp.Height), System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
            aSaveBmp.Save(@"C:\Karle\Cloud\GoogleDrive\Ablage\Dev\Mvi\MviMono\Content\SpriteFont\ethnocentric" + aFontSize.ToString() + ".bmp");
        }
    }
}
