using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

namespace CharlyBeck.Mvi.Mono.GameCore
{
    using CharlyBeck.Mvi.Cube;
    using CharlyBeck.Mvi.Facade;
    using CharlyBeck.Mvi.XnaExtensions;
    using CharlyBeck.Mvi.World;
    using CharlyBeck.Utils3.Enumerables;
    using CharlyBeck.Utils3.Exceptions;
    using CharlyBeck.Utils3.LazyLoad;
    using CharlyBeck.Utils3.ServiceLocator;
    using System.Collections;
    using System.IO;
    using System.Diagnostics;
    using global::Mvi.Models;
    using CharlyBeck.Mvi.Sprites;
    using MviMono.Models;
    using CharlyBeck.Mvi.Sprites.Asteroid;
    using MviMono.Sprites.Asteroid;
    using CharlyBeck.Mvi.Value;
    using CharlyBeck.Mvi.Mono.GameViewModel;
    using CharlyBeck.Mvi.Mono.Input.Mouse;
    using CharlyBeck.Mvi.Mono.Input.Hid;
    using CharlyBeck.Mvi.Mono.Sprites;
    using CharlyBeck.Mvi.Sprites.Bumper;
    using CharlyBeck.Mvi.Sprites.Shot;
    using CharlyBeck.Mvi.Mono.Sprites.Cube;
    using CharlyBeck.Mvi.Mono.Sprites.Shot;
    using CharlyBeck.Mvi.Sprites.Crosshair;
    using CharlyBeck.Mvi.Mono.Sprites.Crosshair;
    using Microsoft.Xna.Framework.Media;
    using MviMono.Sfx;
    using CharlyBeck.Mvi.Sfx;
    using CharlyBeck.Mvi.Mono.Sprites.Explosion;
    using CharlyBeck.Mvi.Sprites.Explosion;
    using CharlyBeck.Mvi.Extensions;
    using CharlyBeck.Mvi.Mono.Sprites.Avatar;
    using CharlyBeck.Mvi.Sprites.Avatar;
    using CharlyBeck.Mvi.Sprites.GridLines;
    using CharlyBeck.Mvi.Mono.Sprites.Gem;
    using CharlyBeck.Mvi.Sprites.Gem;
    using CharlyBeck.Mvi.Mono.Sprites.GemSlot;
    using CharlyBeck.Mvi.Sprites.GemSlot;
    using CharlyBeck.Utils3.Strings;

    internal abstract class CBase : CServiceLocatorNode
    {
        internal CBase(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal CBase(CNoParentEnum aNoParent) : base(aNoParent)
        {
        }
    }
    internal sealed class CRootBase : CBase
    {
        internal CRootBase() : base(NoParent)
        {
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();
    }



    internal static class CMonoWorldExtensions
    {
        internal static CVector3Dbl GetGameCoordinates(this Vector3 aVector)
            => new CVector3Dbl(aVector.X, aVector.Y, aVector.Z);
    }




    internal sealed class CMonoFacade : CFacade
    {
        #region ctor
        internal CMonoFacade(CServiceLocatorNode aParent, CGame aGame) : base(aParent)
        {
            this.Game = aGame;

            this.Init();
        }
        #endregion
        #region PlatformSpriteFactory
        protected override void BuildPlatformSpriteFactory(CPlatformSpriteFactory aPlatformSpriteFactory)
        {
            aPlatformSpriteFactory[CPlatformSpriteEnum.Avatar] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoAvatarSprite(aPair.Item1) { Sprite = (CAvatarSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.Bumper] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoBumperSprite(aPair.Item1) { Sprite = (CBumperSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.Crosshair] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoCrosshairSprite(aPair.Item1) { Sprite = (CCrosshairSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.GridLines] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoGridLinesSprite(aPair.Item1) { Sprite = (CGridLinesSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.Shot] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoShotSprite(aPair.Item1) { Sprite = (CShotSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.Explosion] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoExplosionSprite(aPair.Item1) { Sprite = (CExplosionSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.Gem] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoGemSprite(aPair.Item1) { Sprite = (CGemSprite)aPair.Item2 }; return aSprite; });
            aPlatformSpriteFactory[CPlatformSpriteEnum.GemSlotControls] = new CNewPlatformSpriteFunc(aPair => { var aSprite = new CMonoGemSlotControlsSprite(aPair.Item1) { Sprite = (CGemSlotControlsSprite)aPair.Item2 }; return aSprite; });
        }
        #endregion
        #region ServiceLocator
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CMonoModels>(() => this.MonoModels);
            aServiceContainer.AddService<CGame>(() => this.Game);
            aServiceContainer.AddService<CSoundLoader>(() => this.MonoSoundLoader);
            this.GameVm.RegisterComponentServices(aServiceContainer);
            return aServiceContainer;
        }
        #endregion
        #region GameVm
        private CGameVm GameVmM;
        private CGameVm GameVm => CLazyLoad.Get(ref this.GameVmM, () => new CGameVm(this));
        #endregion
        internal readonly CGame Game;
        public override void AddInGameThreadAction(Action aAction)
            => this.Game.DebugWindowUpdate.AddAction(aAction);
      
        #region Models
        private CMonoModels MonoModelsM;
        internal CMonoModels MonoModels => CLazyLoad.Get(ref this.MonoModelsM, () => new CMonoModels(this));
        #endregion
        #region Joystick
        private CJoystick1 Joystick1M;
        internal CJoystick1 Joystick1 => CLazyLoad.Get(ref this.Joystick1M, () => new CJoystick1(this));
        #endregion
        #region SoundLoader
        private CMonoSoundLoader MonoSoundLoaderM;
        private CMonoSoundLoader MonoSoundLoader => CLazyLoad.Get(ref this.MonoSoundLoaderM, () => new CMonoSoundLoader(this));
        #endregion
    }

    internal struct CAvatar
    {
        /// <summary>
        /// Konstruktor für Akkumulative berechnung. (Produziert nans)
        /// </summary>
        internal CAvatar(CGame aGame, Vector3 aCameraPos, Vector3 aCameraTarget, Vector3 aUpVector, Vector3 aAxisX, Vector3 aAxisY)
        {
            if(double.IsNaN(aUpVector.X))
            {
                Debug.Assert(true);
            }
            this.Game = aGame;
            this.AxisAngles = default;
            this.CamPos = aCameraPos;
            this.CamTarget = SetLength(aCameraTarget, ReferenceLength);
            this.UpVector = SetLength(aUpVector, ReferenceLength);
            this.ViewMatrix = Matrix.CreateLookAt(this.CamPos, aCameraTarget, this.UpVector);
            this.AxisX = aAxisX;
            this.AxisY = aAxisY;
        }
        private const float ReferenceLength = 5f;
        private const float ToleranceLength = float.MaxValue / 2f;
        private static Vector3 SetLength(Vector3 v, float d)
        {
            // TODO
            // omg... :'(

            var l = v.GetLength();
            if(l < ToleranceLength
            && l > -ToleranceLength)
                return v;
            else if (l == ReferenceLength)
                return v; // watn glück...^^
            else if(l < ReferenceLength)
            {
                var ld = ReferenceLength - l;
                return v.MakeLonger(ld);
            }
            else if(l > ReferenceLength)
            {
                var ld = l - ReferenceLength;
                return v.MakeLonger(-ld);
            }
            else
            {
                // 42...
                return v;
            }
        }

        /// <summary>
        /// Konstruktor für neuen ansatz. Sollte ansatztechnisch keine nans produzieren funktioniert aber 
        /// noch nicht, wenn man sich bewegt. (Rumgucken im stillstand funktioniert aber)
        /// DAzu gravity abschalten...
        /// </summary>
        /// <param name="aGame"></param>
        /// <param name="c"></param>
        /// <param name="aa">Axis angle</param>
        internal CAvatar(CGame aGame, Vector3 c, Vector3 aa)
        {
            var aNew = true;
            if(aNew)
            {       
                // Jetzt dreht es sich nicht mehr um die eigene achse, wenn man 90° nach oben/unten schaut.
                // Dafür passiert das gleiche, wenn man 90° nach rechts/links schaut... XD
                // Wieder nix XD
                var t = DefaultZ;
                var my = Matrix.CreateFromAxisAngle(DefaultY, aa.Y);
                var mx = Matrix.CreateFromAxisAngle(DefaultX, aa.X);
                var ax = mx.Rotate(DefaultX);
                var ay = my.Rotate(DefaultY);
                var u = DefaultY;
                u = my.Rotate(u);
                u = mx.Rotate(u);
                t = my.Rotate(t);
                t = mx.Rotate(t);
                t = c + t;
                this.CamPos = c;
                this.CamTarget = t;
                this.UpVector = u;
                this.ViewMatrix = Matrix.CreateLookAt(this.CamPos, this.CamTarget, this.UpVector);
                this.AxisX = ax;
                this.AxisY = ay;
                this.Game = aGame;
                this.AxisAngles = aa;
            }
            else 
            { 
                this.Game = aGame;
                this.CamPos = c;
                this.AxisAngles = aa;
                this.ViewMatrix = default;
                this.AxisX = DefaultX;
                this.AxisY = DefaultY;

                this.UpVector = DefaultY.RotateX(aa.X).RotateY(aa.Y).RotateZ(aa.Z);
                this.CamTarget = DefaultZ.RotateX(aa.X).RotateY(aa.Y).RotateZ(aa.Z);
                this.ViewMatrix = Matrix.CreateLookAt(this.CamPos, this.CamTarget, this.UpVector);
            }
        }

        private readonly CGame Game;
        #region SourcData
        internal readonly Vector3 CamPos;
        internal Vector3 AxisAngles;
        #endregion
        #region Fields (redundant for optimization / for akkumulative version of the funktions.)
        internal static readonly float Rad45 = (float)(Math.PI / 4d);
        internal static readonly float Rad90 = (float)(Math.PI / 2d);
        internal  Vector3 CamTarget;
        internal  Vector3 UpVector;
        internal  Vector3 AxisX;
        internal  Vector3 AxisY;
        #endregion
        #region Defaults
        private static readonly Vector3 DefaultX = new Vector3(1, 0, 0);
        private static readonly Vector3 DefaultY = new Vector3(0, 1, 0);
        private static readonly Vector3 DefaultZ = new Vector3(0, 0, 1);
        internal static CAvatar Default(CGame aGame)
            => new CAvatar(aGame,
                           Vector3.Zero,
                           Vector3.Zero);

        internal Vector3 CamTargetOffset => this.CamTarget - this.CamPos;

        private const float MoveVectorLength = 1f;
        internal readonly Matrix ViewMatrix;
        #endregion

        // 2 Modis:
        //   - Akkumualtiv: Werte werden aufgerechnet, 
        //      * Pro:
        //      * Con: läuft gegen positive/negative infinity / nan
        //   - Werte werden immer auf eindeutiger basis (AngleVec) berechnet.
        //      * Pro: läuft nicht gegen positive/negative infinity / nan
        //      * Con: "Only" a FPS Cam like described herein: https://www.3dgep.com/understanding-the-view-matrix/
        internal bool AccumulativeIsEnabled => this.Game.AccumulativeViewMatrixValue.Value;

        // Nach oben oder unten schauen: pitch (rotation about the X axis)
        internal CAvatar LookUpDown(float aRadians)
            => this.AccumulativeIsEnabled
             ? this.LookUpDownAccumulative(aRadians)
             : this.LookUpDownAbsolute(aRadians);

        // Nach links/rechts schauen:  yaw (rotation about the Y axis)
        internal CAvatar LookLeftRight(float aRadians)
            => this.AccumulativeIsEnabled
             ? this.LookLeftRightAccumulative(aRadians)
             : this.LookLeftRightAbsolute(aRadians)
             ;

        // Akkumulative version der Methode "LookUpDown"
        private CAvatar LookUpDownAbsolute(float aRadians)
        {
            var a1 = this.AxisAngles;
            var a2 = new Vector3(a1.X + aRadians, a1.Y, a1.Z);
            var a = new CAvatar(this.Game, this.CamPos, a2);
            return a;
        }

        // "Absolute" version der Methode "LookLeftRight"
        private CAvatar LookLeftRightAbsolute(float aRadians)
        {
            var a1 = this.AxisAngles;
            var a2 = new Vector3(a1.X, a1.Y + aRadians, a1.Z);
            var a = new CAvatar(this.Game, this.CamPos, a2);
            return a;
        }

        // "Akkumulative" version der Methode "LookUpDown"
        private CAvatar LookUpDownAccumulative(float aRadians)
        {
            var m = Matrix.CreateFromAxisAngle(this.AxisX, aRadians);
            var u = m.Rotate(this.UpVector);
            var t = this.CamPos + m.Rotate(this.CamTargetOffset);
            var x = this.AxisX;
            var y = m.Rotate(this.AxisY);
            var a = this.CheckValid(new CAvatar(this.Game, 
                                                this.CamPos, 
                                                t, // CamTarget/Lookat
                                                u, // UpVector
                                                x, // X-Axis
                                                y  // Y-Axis
                                                ));
            return a;
        }

        // "Akkumulative" version der Methode "LookLeftRight"
        private CAvatar LookLeftRightAccumulative(float aRadians)
        {
            var m = Matrix.CreateFromAxisAngle(this.AxisY, aRadians);
            var u = m.Rotate(this.UpVector);
            var t = this.CamPos + m.Rotate(this.CamTargetOffset);
            var x = m.Rotate(this.AxisX);
            var y = this.AxisY;
            var a = this.CheckValid(new CAvatar(this.Game, 
                                                this.CamPos, 
                                                t,  // CamTarget/Lookat
                                                u,  // UpVector
                                                x,  // X-Axis
                                                y   // Y-Axis
                                                ));
            return a;
        }

        internal CAvatar RotateZ(float aRadians)
        {
            var a1 = this.AxisAngles;
            var a2 = new Vector3(a1.X, a1.Y, a1.Z + aRadians);
            var a = new CAvatar(this.Game, this.CamPos, a2);
            return a;
        }

        internal CAvatar MoveToOffset(Vector3 aMoveVector)
        {
            if (this.AccumulativeIsEnabled)
            {
                var aCamPos = this.CamPos + aMoveVector;
                var aCamTarget = this.CamTarget + aMoveVector;
                var aAvatar = new CAvatar(this.Game, aCamPos, aCamTarget, this.UpVector, this.AxisX, this.AxisY);
                return aAvatar;
            }
            else
            {
                var aNewCameraPosition = this.CamPos + aMoveVector;
                var aAvatar = new CAvatar(this.Game, aNewCameraPosition, this.AxisAngles);
                return aAvatar;
            }
        }
        private CAvatar CheckValid(CAvatar aAvatar)
        {
            if (aAvatar.CheckValid())
                return aAvatar;
            return this;
        }

        internal bool CheckValid()
        {
            //if (double.IsNaN(this.AxisX.X))
            //    return false;
            //else if (double.IsNegativeInfinity(this.ViewMatrix[0, 0]))
            //    return false;
            //else if (double.IsPositiveInfinity(this.ViewMatrix[0, 0]))
            //    return false;
            return true;
        }
        //internal Matrix CamTargetRotationMatrix
        //    => Matrix.CreateRotationX(this.CamTargetOffset.GetRadiansYz())
        //     * Matrix.CreateRotationY(this.CamTargetOffset.GetRadiansXz())
        //     * Matrix.CreateRotationZ(this.CamTargetOffset.GetRadiansYz())
        //    ;

        internal CAvatar MoveSidewardsHorizontal(float aDelta)
            => this; // throw new NotImplementedException();

        internal CAvatar MoveSidewardsVertical(float aDelta)
            => this; //throw new NotImplementedException();


        //    => new CAvatar(this.CamPos + this.CamTarget.RotateZ(MathHelper.ToRadians(90)) * new Vector3(aDelta),)

        internal CAvatar MoveAlongViewAngle(float aDistance)
        {
            if (this.AccumulativeIsEnabled)
            {
                var aMoveVector = this.CamTargetOffset;
                var aLonger = aMoveVector.MakeLongerDelta(aDistance);
                var aNewCameraPosition = this.CamPos + aLonger;
                var aNewCameraTarget = this.CamTarget + aLonger;
                var aAvatar = new CAvatar(this.Game, aNewCameraPosition, aNewCameraTarget, this.UpVector, this.AxisX, this.AxisY);
                return aAvatar;
            }
            else if (aDistance != 0f)
            {
                var aMoveVector = this.CamTargetOffset;
                var aLonger = aMoveVector.MakeLongerDelta(aDistance);
                var aNewCameraPosition = this.CamPos + aLonger;
                var aAvatar = new CAvatar(this.Game, aNewCameraPosition, this.AxisAngles);
                return aAvatar;
            }
            else
            {
                return this;
            }
        }
        #region Serialize
        internal void Write(Stream aStream)
        {
           //throw new NotImplementedException();
            //aStream.Write(this.CamPos);
            //aStream.Write(this.CamTarget);
            //aStream.Write(this.UpVector);
            //aStream.Write(this.AxisX);
            //aStream.Write(this.AxisY);
        }

        internal static CAvatar Read(CGame aGame, Stream aStream)
        {
            throw new NotImplementedException();
            //var aCamPos = aStream.ReadVector3();
            //var aCamTarget = aStream.ReadVector3();
            //var aUpVector = aStream.ReadVector3();
            //var aAxisX = aStream.ReadVector3();
            //var aAxisY = aStream.ReadVector3();
            //return new CAvatar(aCamPos, aCamTarget, aUpVector, aAxisX, aAxisY);
        }
        private static FileInfo FileInfo => new FileInfo(Path.Combine(new FileInfo(typeof(CAvatar).Assembly.Location).Directory.FullName, "Avatar.bin"));

        internal void Save()
        {
            var aMemoryStream = new MemoryStream();
            this.Write(aMemoryStream);
            aMemoryStream.Seek(0, SeekOrigin.Begin);
            File.WriteAllBytes(CAvatar.FileInfo.FullName, aMemoryStream.ToArray());
        }
        internal static CAvatar Load(CGame aGame)
        {
            var aFileInfo = FileInfo;
            if (aFileInfo.Exists)
            {
                try
                {
                    var aMemoryStream = new MemoryStream(File.ReadAllBytes(aFileInfo.FullName));
                    var aAvatar = Read(aGame,aMemoryStream);
                    return aAvatar;
                }
                catch (Exception)
                {
                    return CAvatar.Default(aGame);
                }
            }
            return CAvatar.Default(aGame);
        }
        #endregion
        internal CVector3Dbl WorldPos => this.CamPos.ToVector3Dbl();
    }

    internal sealed class CGame : Game
    {
        #region ctor
        internal CGame(CServiceLocatorNode aParent)
        {
            this.Parent = aParent;
            this.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            this.Window.AllowUserResizing = true;
            this.Content.RootDirectory = "Content\\bin";
            this.OriginValue = CValue.GetStaticValue<CBoolValue>(this.ServiceLocatorNode, OriginValueDeclaration);
        }
        internal CGame():this(new CDefaultServiceLocatorNode())
        {
        }
        protected override void Initialize()
        {
            this.Init3d();

            this.MonoFacade.Load();

            base.Initialize();
        }

        protected override void EndRun()
        {
            base.EndRun();
            this.Avatar.Save();
        }
        #endregion
        #region ServiceLocator
        private CServiceLocatorNode Parent;
        internal CServiceLocatorNode ServiceLocatorNode => this.MonoFacade;
        #endregion
        #region MonoFacade
        private CMonoFacade MonoFacadeM;
        internal CMonoFacade MonoFacade => CLazyLoad.Get(ref this.MonoFacadeM, () => new CMonoFacade(this.Parent, this));
        public object VmMonoFacade => this.MonoFacade;
        #endregion
        

        internal CWorld World => this.MonoFacade.World;
        private readonly GraphicsDeviceManager GraphicsDeviceManager;

        protected override void LoadContent()
        {
            this.Avatar = CAvatar.Load(this);
            this.MonoFacade.MonoModels.LoadContent();
            base.LoadContent();
        }



        private bool SetMousePosition;

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);


            if (this.SetMousePosition)
            {
                //Mouse.SetPosition(this.GraphicsDevice.Viewport.Width / 2, this.GraphicsDevice.Viewport.Height / 2);
                this.SetMousePosition = false;
            }
            //this.SpriteBatch.Begin();
            //this.SpriteBatch.Draw(this.BallTexture, new Vector2(0, 0), Color.White);
            //this.SpriteBatch.End();

            this.Draw3d();
        }

        #region Values
        //[CMemberDeclaration]
        private static readonly CBoolDeclaration XnaMouseEnabledValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_Mouse_Xna,  new Guid("99e270ec-f288-4a86-8cde-caaf8af85cff"), true, false);
       
        private CBoolValue XnaMouseEnabledValueM;
        private CBoolValue XnaMouseEnabledValue => CLazyLoad.Get(ref this.XnaMouseEnabledValueM, () => CValue.GetStaticValue<CBoolValue>(this.World, XnaMouseEnabledValueDeclaration));
        [CMemberDeclaration]
        private static readonly CBoolDeclaration FullScreenValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_FullScreen, new Guid("99a83ab0-c037-4d78-9d2d-2adc1bcd627e"), true, false);
        private CBoolValue FullScreenValueM;
        private CBoolValue FullScreenValue => CLazyLoad.Get(ref this.FullScreenValueM, () => CValue.GetStaticValue<CBoolValue>(this.World, FullScreenValueDeclaration));
        [CMemberDeclaration]
        private static readonly CBoolDeclaration AccumulativeViewMatrixValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_AccumulativeViewMatrix, new Guid("e94a7502-aea0-45fc-a332-506e2ee750dc"), true, CStaticParameters.Value_AccumulativeViewMatrix);
        private CBoolValue AccumulativeViewMatrixValueM;
        internal CBoolValue AccumulativeViewMatrixValue => CLazyLoad.Get(ref this.AccumulativeViewMatrixValueM, () => CValue.GetStaticValue<CBoolValue>(this.World, AccumulativeViewMatrixValueDeclaration));
        #endregion
        #region Mouse
        private CMouse MouseM;
        private CMouse Mouse => CLazyLoad.Get(ref this.MouseM, () => new CMouse(this.ServiceLocatorNode));
        #endregion

        private void UpdateInput(GameTime aGameTime)
        {
            var aXnaMouse = false;// this.XnaMouseEnabledValue.Value;
            var aJoystick = this.MonoFacade.Joystick1;
            var aMouse = this.Mouse;
            var aKeyboardState = Keyboard.GetState();
            var aAvatar = this.Avatar;
            bool aChanged = false;

            var aMouseDx = 0f;
            var aMouseDy = 0f;
            var aMouseThroodle = 0f;
                
            var aMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            if (aXnaMouse)
            {
               
                var aMousePosition = aMouseState.Position;
                if (this.MousePosition.HasValue)
                {
                    var aDelta = aMousePosition - this.MousePosition.Value;
                    aMouseDx = aDelta.X;
                    aMouseDy = aDelta.Y;
                    this.SetMousePosition = true;
                }
                else
                {
                    //  Mouse.SetPosition(this.GraphicsDevice.Viewport.Width / 2, this.GraphicsDevice.Viewport.Height / 2);

                }
                this.MousePosition = aMousePosition;
                if (aMouseState.RightButton == ButtonState.Pressed)
                {
                    aMouseThroodle = -1.0f;
                }
            }

            {
                var aOffset = this.Mouse.Offset;
                aMouseDx = (float)aOffset.Item1;
                aMouseDy = (float)aOffset.Item2;
            }

            {
                var aRotX = aMouseDx * 0.5f;
                if (aKeyboardState.IsKeyDown(Keys.Right))
                    aRotX += 1.0f;
                if (aKeyboardState.IsKeyDown(Keys.Left))
                    aRotX -= 1.0f;
                aRotX = aRotX +(float)this.World.LookLeftRight.ToDegrees();
                this.World.LookLeftRight = 0d;
               // aRotX = aRotX + (float)aJoystick.GetAxis(CJoystick1.CAxisEnum.X);

                var aRadians1 = MathHelper.ToRadians((float)(aRotX * this.CamSpeedRy * aGameTime.ElapsedGameTime.TotalSeconds));
                var aRadians2 = this.DebugWindowUpdate.LookLeftRight.Retrieve();
                var aRadians = aRadians1 + aRadians2;
                if (aRadians != 0.0f)
                {
                    aAvatar = aAvatar.LookLeftRight(aRadians);                    
                    aChanged = true;
                }
            }


            {

                var aRotZ = 0d;
                aRotZ = aRotZ -(float)aJoystick.GetAxis(CJoystick1.CAxisEnum.Z);
                if(aRotZ != 0.0f)
                {
                    var aRadians = MathHelper.ToRadians((float)(aRotZ * this.CamSpeedRz * aGameTime.ElapsedGameTime.TotalSeconds));
                    aAvatar = aAvatar.RotateZ(aRadians);
                    aChanged = true;
                }
            }

            {
                var aRotY = aMouseDy * 0.5f;
                if (aKeyboardState.IsKeyDown(Keys.Down))
                    aRotY += 1.0f;
                if (aKeyboardState.IsKeyDown(Keys.Up))
                    aRotY -= 1.0f;
                aRotY = aRotY + (float)this.World.LookUpDown.ToDegrees();
                this.World.LookUpDown = 0d;
               // aRotY += (float)aJoystick.GetAxis(CJoystick1.CAxisEnum.Y);

                var aRadians1 = MathHelper.ToRadians((float)(aRotY * this.CamSpeedRx * aGameTime.ElapsedGameTime.TotalSeconds));
                var aRadians2 = this.DebugWindowUpdate.LookUpDown.Retrieve();
                var aRadians = aRadians1 + aRadians2;
                if (aRadians != 0f)
                {
                    aAvatar = aAvatar.LookUpDown(aRadians);
                    aChanged = true;
                }
            }


            var aThroodle = aMouseThroodle;
            if (aKeyboardState.IsKeyDown(Keys.NumPad0))
                aThroodle -= 1.0f;
            if (aKeyboardState.IsKeyDown(Keys.NumPad5))
                aThroodle += 1.0f;


            aThroodle += (float)(-aJoystick.GetThroodle() * 2.0d);

            

            if (aThroodle != 0f)
            {
                var aDistance1 = (float)(aThroodle * this.CamSpeedThroodle * aGameTime.ElapsedGameTime.TotalSeconds);
                var aDistance2 = aDistance1 * 0.4;
                var aDistance3 = aDistance2; // * this.World.Speed;
                var aDistance = aDistance3;
                this.World.Throodle = aDistance;

                //aAvatar = aAvatar.MoveAlongViewAngle((float)aDistance);
                aChanged = true;
            }
            else
            {
                this.World.Throodle = 0d;
            }
            this.World.AvatarSpeed = aThroodle;

            var aDx = 0.0f;
            if (aKeyboardState.IsKeyDown(Keys.NumPad4))
                aDx += 1.0f;
            if (aKeyboardState.IsKeyDown(Keys.NumPad6))
                aDx -= 1.0f;

            if (aDx != 0)
            {
                aAvatar = aAvatar.MoveSidewardsHorizontal((float)(aDx * this.CamSpeedX * aGameTime.ElapsedGameTime.TotalSeconds));
                aChanged = true;
            }


            var aDy = 0.0f;
            if (aKeyboardState.IsKeyDown(Keys.NumPad8))
                aDy += 1.0f;
            if (aKeyboardState.IsKeyDown(Keys.NumPad2))
                aDy -= 1.0f;
     
            if (aDy != 0)
            {
                aAvatar = aAvatar.MoveSidewardsVertical((float)(aDy * this.CamSpeedY * aGameTime.ElapsedGameTime.TotalSeconds));
                aChanged = true;
            }


            if (aKeyboardState.IsKeyDown(Keys.Escape))
            {
                this.Escape();
            }


            if(aKeyboardState.IsKeyDown(Keys.Space)
            || aJoystick.IsButtonPressed(CJoystick1.CButtonEnum.StickFront)
            || this.Mouse.IsLeftButtonDown
            || aMouseState.LeftButton == ButtonState.Pressed)
            {
                this.World.Shoot();
            }

            if(aKeyboardState.IsKeyDown(Keys.L))
            {
                if(!this.DidL)
                {
                    this.World.SlowDownNearObjectValue.Value = !this.World.SlowDownNearObjectValue.Value;
                    this.DidL = true;
                }
            }
            else
            {
                this.DidL = false;
            }

            if (aKeyboardState.IsKeyDown(Keys.M))
            {
                if (!this.DidM)
                {
                    this.Mouse.WinFormMouseEnabledValue.Value = !this.Mouse.WinFormMouseEnabledValue.Value;
                    this.DidM = true;
                }
            }
            else
            {
                this.DidM = false;
            }
            this.World.ThroodleDirection = aAvatar.CamTargetOffset.ToVector3Dbl();

            var aMoveVector = this.World.MoveVector * new CVector3Dbl(this.World.GameTimeElapsed.TotalMilliseconds);
            aAvatar = aAvatar.MoveToOffset(aMoveVector.ToVector3());
            this.World.Speed = aMoveVector.GetLength();
            aChanged = true;

            if (aChanged)
            {
                this.Avatar = aAvatar;
            }
        }

        private bool DidL;
        private bool DidM;

        protected override void EndDraw()
        {


            base.EndDraw();
            this.GraphicsDeviceManager.IsFullScreen = this.FullScreenValue.Value;

        }

        internal void Escape()
        {
       //     this.XnaMouseEnabledValue.Value = false;
            this.Mouse.WinFormMouseEnabledValue.Value = false;
            this.FullScreenValue.Value = false;
        }
        private void UpdateWorld(GameTime aGameTime)
        {
            this.World.AvatarWorldPos = this.Avatar.WorldPos;
            this.World.AvatarShootDirection = this.Avatar.CamTargetOffset.ToVector3Dbl();           
            this.World.GameTime = aGameTime;
            this.World.Update();
        }

        private void UpdateAvatar()
        {
            var aAvatar = this.Avatar;
            var aOldAvatarPos = aAvatar.WorldPos;
            var aMoveVector = this.World.FrameInfo.AvatarMoveVector;
            var aNewAvatar = aAvatar.MoveToOffset(aMoveVector.ToVector3());
            this.Avatar = aNewAvatar;
        }
        protected override void Update(GameTime aGameTime)
        {
            this.DebugWindowUpdate.RunUpdateActions();
            this.UpdateInput(aGameTime);
            this.UpdateAvatar();
            this.UpdateWorld(aGameTime);
            this.MonoFacade.Update();
            this.Mouse.NextFrame();
            this.UpdateBasicEffect();

            base.Update(aGameTime);
        }
        private Point? MousePosition;
        #region 3d
        private void InitRasterizerState()
        {
            var aRasterizerState = new RasterizerState();
            aRasterizerState.CullMode = CullMode.None;
            this.GraphicsDevice.RasterizerState = aRasterizerState;
        }
        #region Avatar
        private CAvatar? AvatarM;
        internal CAvatar Avatar
        {
            get => CLazyLoad.Get(ref this.AvatarM, ()=>CAvatar.Default(this));
            set
            {
                if (value.CheckValid())
                {
                    this.AvatarM = value;
                    if (this.AvatarChanged is object)
                        this.AvatarChanged();
                }
            }
        }
        internal event Action AvatarChanged;
        #endregion

        #region Camera
        private float CamSpeedRatio => 1.0f; //(float)(this.MonoFacade.World.EdgeLen);
        private float CamSpeedY => this.CamSpeedRatio;
        private float CamSpeedX => this.CamSpeedRatio;

        private float CamSpeedRx => (float) Math.PI * 4f *  4f;
        private float CamSpeedRy => (float) Math.PI * 4f * 4f;
        private float CamSpeedRz => (float) Math.PI * 4f * 4f;
        private float CamSpeedThroodle => this.CamSpeedRatio;

        #endregion
        #region WorldMatrix
        internal Matrix WorldMatrix { get; private set; }
        private void InitWorldMatrix()
        {
            this.WorldMatrix = Matrix.CreateWorld(new Vector3(0), Vector3.Forward, Vector3.Up);
        }
        #endregion
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            this.Mouse.WinFormMouseEnabledValue.Value = false;

        }
        #region ViewMatrix
        internal Matrix ViewMatrix => this.Avatar.ViewMatrix;
        //internal Matrix ViewMatrix => new CAvatar(new Vector3(-1500, 0, -6700), new Vector3(-1500, 0, 6750), Vector3.Up).ViewMatrix;
        #endregion
        #region ProjectionMatrix
        private Matrix ProjectionMatrixM;
        internal Matrix ProjectionMatrix
        {
            get => this.ProjectionMatrixM;
            set
            {
                this.ProjectionMatrixM = value;
                if (this.ProjectionMatrixChanged is object)
                    this.ProjectionMatrixChanged();
            }
        }
        internal event Action ProjectionMatrixChanged;
        private void InitProjectionMatrix()
        {
            this.UpdateProjectionMatrix();
        }
        private void UpdateProjectionMatrix()
        {
            this.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView
            (
                MathHelper.ToRadians(90f),
                GraphicsDevice.DisplayMode.AspectRatio,
                0.0001f,
                10000f
            );
        }
        #endregion
        #region BasicEffect
        internal BasicEffect BasicEffect { get; private set; }
        private void InitBasicEffect()
        {
            this.BasicEffect = new BasicEffect(GraphicsDevice);
            this.UpdateBasicEffect();
            this.AvatarChanged += this.UpdateBasicEffect;
        }
        private void UpdateBasicEffect()
        {
            this.BasicEffect.Alpha = 1f;
            this.BasicEffect.VertexColorEnabled = true;
            this.BasicEffect.View = this.ViewMatrix;
            this.BasicEffect.World = this.WorldMatrix;
            this.BasicEffect.Projection = this.ProjectionMatrix;
        }
        #endregion
        #region Triangle
        VertexPositionColor[] TriangleVertices;
        VertexBuffer VertexBuffer;
        private void DrawTriangle()
        {
            var aDrawTriangle = true;
            if (aDrawTriangle)
            {
                GraphicsDevice.SetVertexBuffer(VertexBuffer);
                foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
                {
                    aEffectPass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.
                                                  TriangleList, 0, 3);
                }
            }
        }
        private void InitTriangle()
        {
            this.TriangleVertices = new VertexPositionColor[3];
            var d = 30000;
            var e = 0;
            this.TriangleVertices[0] = new VertexPositionColor(new Vector3(d, 0, e), Color.Red);
            this.TriangleVertices[1] = new VertexPositionColor(new Vector3(0, 0, e), Color.Yellow);
            this.TriangleVertices[2] = new VertexPositionColor(new Vector3(d / 2.0f, -d, e), Color.Blue);

            //this.TriangleVertices[0] = new VertexPositionColor(new Vector3(0, d, e), Color.Red);
            //TriangleVertices[1] = new VertexPositionColor(new Vector3(-d, -d, e), Color.Green);
            //TriangleVertices[2] = new VertexPositionColor(new Vector3(d, -d, e), Color.Blue);
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionColor>(TriangleVertices);
        }
        #endregion
        #region DebugWindow
        internal sealed class CDebugWindowUpdate
        {
            internal CDebugWindowUpdate()
            {
                this.LookLeftRight = new CValueContainer<float>(this);
                this.LookUpDown = new CValueContainer<float>(this);
            }
            internal sealed class CValueContainer<T>
            {
                internal CValueContainer(CDebugWindowUpdate aWindow)
                {
                    this.Window = aWindow;
                }
                private readonly CDebugWindowUpdate Window;
                private T ValueM;
                internal T Value
                {
                    get => this.ValueM;
                    set
                    {
                        lock (this)
                        {
                            this.ValueM = value;
                        }
                    }
                }
                internal T Retrieve()
                {
                    lock (this)
                    {
                        var aValue = this.Value;
                        this.Value = default;
                        return aValue;
                    }
                }
                internal void EnqueueSet(T aValue)
                {
                    this.Window.AddAction(delegate () { this.Value = aValue; });
                }

            }
            internal readonly CValueContainer<float> LookLeftRight;
            internal readonly CValueContainer<float> LookUpDown;
            private readonly List<Action> Actions = new List<Action>();
            internal void AddAction(Action aAction)
            {
                lock (this.Actions)
                {
                    this.Actions.Add(aAction);
                }
            }
            internal void RunUpdateActions()
            {
                Action[] aActions;
                lock (this.Actions)
                {
                    aActions = this.Actions.ToArray();
                    this.Actions.Clear();
                }
                foreach (var aAction in aActions)
                {
                    aAction();
                }
            }
        }
        internal CDebugWindowUpdate DebugWindowUpdate = new CDebugWindowUpdate();
        #endregion
        #region Coordinates
        private VertexBuffer CoordinatesVertexBufferM;
        private VertexBuffer CoordinatesVertexBuffer => CLazyLoad.Get(ref this.CoordinatesVertexBufferM, this.NewCoordinatesVertexBuffer);


        private VertexBuffer NewCoordinatesVertexBuffer()
        {
            var aMaxCoord = 1;
            var aOrigin = new Vector3(0, 0, 0);
            var aMaxX = new Vector3(aMaxCoord, 0, 0);
            var aMaxY = new Vector3(0, aMaxCoord, 0);
            var aMaxZ = new Vector3(0, 0, aMaxCoord);
            var aLines = new VertexPositionColor[]
            {
               new VertexPositionColor(aOrigin, Color.Red),
               new VertexPositionColor(aMaxX, Color.Red),
                new VertexPositionColor(aOrigin, Color.Green),
                new VertexPositionColor(aMaxY, Color.Green),
                 new VertexPositionColor(aOrigin, Color.Blue),
                 new VertexPositionColor(aMaxZ, Color.Blue),
            };
            var aVertexBuffer = aLines.ToVertexBuffer(this.GraphicsDevice);
            return aVertexBuffer;
        }
        [CMemberDeclaration]
        internal static readonly CValueDeclaration OriginValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_Origin, new Guid("64f3ce9c-9960-443c-9553-a10c395695a0"), true, CStaticParameters.Value_Origin_Visible);
        private readonly CBoolValue OriginValue;

        private void DrawCoordinates()
        {
            var aDrawCoordinates = this.OriginValue.Value;
            if (aDrawCoordinates)
            {
                foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
                {
                    aEffectPass.Apply();
                    this.CoordinatesVertexBuffer.DrawLineList(this.GraphicsDevice);
                }
            }
        }
        #endregion

        private void Init3d()
        {
            this.InitRasterizerState();
            this.InitWorldMatrix();
            this.InitProjectionMatrix();
            this.InitBasicEffect();
            this.InitTriangle();
        }

        private SpriteBatch FontSpriteBatch;

        private void Draw3d()
        {
            GraphicsDevice.Clear(Color.Black);
            this.MonoFacade.Draw();
            this.DrawCoordinates();

            //this.MonoFacade.MonoModels.DrawString("HELLO", Color.Red, new Vector2(), 1f);

            //foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
            //{
            //    aEffectPass.Apply();

            //    if (!(this.FontSpriteBatch is object))
            //        this.FontSpriteBatch = new SpriteBatch(this.GraphicsDevice);
            //    var aChars = this.SpriteFont.Characters.Select(c => c.ToString()).JoinString();
            //    this.FontSpriteBatch.Begin();
            //    this.FontSpriteBatch.DrawString(this.SpriteFont, aChars , new Vector2(), Color.White);
            //    this.FontSpriteBatch.End();
            //}
        }
        #endregion

    }
}
