﻿using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
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
using System.Windows.Threading;

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
            this.GameState = new CGameState(this.Game.ServiceLocatorNode);
            this.DataContext = aGame;
        }
        public static new void Show()
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

        private readonly CGameState GameState;
        public object VmGameState => this.GameState;
    }

    internal sealed class CGameState : CChangeNotifier
    {
        internal CGameState(CServiceLocatorNode aParent): base(aParent)
        {
            this.Game = this.ServiceContainer.GetService<CGame>();
            this.UpdateTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250),
                                                   DispatcherPriority.Background,
                                                    delegate (object aSender, EventArgs a) { this.Update(); },
                                                    Dispatcher.CurrentDispatcher
                                                   );
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();

        internal readonly CGame Game;
        private DispatcherTimer UpdateTimer;

        private bool IgnoreUpdate;
        private void Update()
        {
            if (this.IgnoreUpdate)
            {
                var aFrameInfo = this.Game.World.FrameInfo;
                this.NearestBumperNullable = aFrameInfo.NearestBumperIsDefined ? aFrameInfo.NearestBumper : default;
                this.CubePos = aFrameInfo.CubePos;
                this.Speed = this.Game.World.Speed;

            }
            else
            {
                this.IgnoreUpdate = false;
            }
        }

        #region NearestBumper
        private CBumperSpriteData NearestBumperNullableM;
        private CBumperSpriteData NearestBumperNullable { get => this.NearestBumperNullableM; set
            {
                if(value != this.NearestBumperNullableM)
                {
                    this.NearestBumperNullableM = value;
                    this.NotifyChange(nameof(this.VmNearestBumperNullable));
                }
            }
        }
        public object VmNearestBumperNullable => this.NearestBumperNullable;
        #endregion
        #region CubePos
        private CCubePos CubePosM;
        private CCubePos CubePos { get => this.CubePosM; set { this.CubePosM = value; this.NotifyChange(nameof(this.VmCubePos)); } }
        public object VmCubePos => this.CubePos;
        #endregion

        #region Speed
        private const double SpeedScale = 10;
        private double SpeedM = double.NaN;
        private double Speed { get => this.SpeedM ; set { if (this.SpeedM != value) { this.SpeedM = value; this.NotifyChange(nameof(this.VmSpeed)); } } }
        public double VmSpeed { get => this.Speed * SpeedScale; set => this.AddAction(delegate () { this.Game.World.Speed = value / SpeedScale; }); }
        #endregion
        private void AddAction(Action aAction)
        {
            this.IgnoreUpdate = true;
            this.Game.DebugWindowUpdate.AddAction(aAction);
        }

    }

}
