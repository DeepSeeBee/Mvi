using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.LazyLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CharlyBeck.Mvi.Mono.Wpf
{
    /// <summary>
    /// Interaktionslogik für CGameDebug.xaml
    /// </summary>
    public partial class CDebugWindow : Window
    {
        internal CDebugWindow(CGame aGame)
        {
            InitializeComponent();

            this.Game = aGame;
            this.DataContext = aGame;
        }
        public static void Show()
        {
            var aGame = new CGame();
            var aWindowThread = new Thread(ShowWindowThread);
            aWindowThread.SetApartmentState(ApartmentState.STA);
            aWindowThread.Start(aGame);
            aGame.Run();
        }

        private static void ShowWindowThread(object aGameObj)
        {
            var aGame = (CGame)aGameObj;
            var aWindow = new CDebugWindow(aGame);
            aWindow.ShowDialog();
        }

        private readonly CGame Game;

        private void UpdateGame(Action aAction)
            => this.Game.DebugWindowUpdate.AddAction(aAction);

        private float? LookDegreesFM;
        private float LookDegreesF { get => CLazyLoad.Get(ref this.LookDegreesFM, () => 45f); set => this.LookDegreesFM = value; }
        private float ToLookDegrees(string aText)
        {
            var aText1 = aText.Trim();
            var aText2 = aText1.EndsWith("°")
                       ? aText1.TrimEnd('°')
                       : aText1;
            var aFloat = float.Parse(aText2);
            return aFloat;
        }
        public string LookDegrees
        {
            set => this.LookDegreesF = ToLookDegrees(value);
            get => this.LookDegreesF + "°";
        }

        private void LookLeft(object sender, RoutedEventArgs e)
        {
            this.UpdateGame(delegate () { this.Game.DebugWindowUpdate.LookLeftRight = (-LookDegreesF).ToRadians(); });
        }

        private void LookUp(object sender, RoutedEventArgs e)
        {
            this.UpdateGame(delegate () { this.Game.DebugWindowUpdate.LookUpDown = (-LookDegreesF).ToRadians(); });
        }

        private void LookRight(object sender, RoutedEventArgs e)
        {
            this.UpdateGame(delegate () { this.Game.DebugWindowUpdate.LookLeftRight = (LookDegreesF).ToRadians(); });
        }

        private void LookDown(object sender, RoutedEventArgs e)
        {
            this.UpdateGame(delegate () { this.Game.DebugWindowUpdate.LookUpDown = (LookDegreesF).ToRadians(); });
        }
    }
}
