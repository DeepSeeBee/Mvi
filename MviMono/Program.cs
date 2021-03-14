using CharlyBeck.Mvi.Mono.GameCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Mvi.Mono.Wpf;
using CharlyBeck.Utils3.Strings;
using System.IO;
using CharlyBeck.Utils3.SystemIo;
using CharlyBeck.Mvi.ContentManager;
using CharlyBeck.Mvi.FontConvert;

namespace CharlyBeck.Mvi.Mono
{
    internal static class Program
    {

        private static void InterpretCommandLineArgs()
        {
            if (Environment.CommandLine.Contains("/BuildContentList"))
            {
                CContentManager.BuildContentList();
            }
            if (Environment.CommandLine.Contains("/BuildSpriteFonts"))
            {
                new CTtfToBmpConverter().Convert();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            InterpretCommandLineArgs();

            var aUseDebugWindow = true;
            if(aUseDebugWindow)
            {
                CDebugWindow.Show();
            }
            else
            {
                var aGame = new CGame();
                aGame.Run();
            }
        }
    }
}
