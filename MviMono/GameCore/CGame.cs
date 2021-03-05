﻿using System;
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
    using CharlyBeck.Mvi.Sprites.Bumper;
    using MviMono.Sprites.Bumper;
    using CharlyBeck.Mvi.Sprites.Quadrant;
    using MviMono.Sprites.Quadrant;
    using CharlyBeck.Mvi.Feature;
    using CharlyBeck.Mvi.Mono.GameViewModel;
    using CharlyBeck.Mvi.Mono.Input.Mouse;
    using CharlyBeck.Mvi.Mono.Input.Hid;
    using CharlyBeck.Mvi.Mono.Sprites;

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
        internal CMonoFacade(CServiceLocatorNode aParent, CGame aGame) : base(aParent)
        {
            this.Game = aGame;
            this.SpritePool = new CSpritePool(this);
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();

        internal readonly CGame Game;
        public override ISprite<T> NewSprite<T>(T aData)
        {
            if (typeof(T) == typeof(CBumperSpriteData))
            {
                return (ISprite<T>)(object)this.SpritePool.AllocateBumperSprite((CBumperSpriteData)(object)aData);
            }
            else if (typeof(T) == typeof(CQuadrantSpriteData))
            {
                return (ISprite<T>)(object)this.SpritePool.AllocateQuadrantSprite((CQuadrantSpriteData)(object)aData);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public override void AddInGameThreadAction(Action aAction)
            => this.Game.DebugWindowUpdate.AddAction(aAction);
        #region SpritePool
        private readonly CSpritePool SpritePool;
        #endregion
    }

    internal struct CAvatar
    {
        internal CAvatar(Vector3 aCameraPos, Vector3 aCameraTarget, Vector3 aUpVector, Vector3 aAxisX, Vector3 aAxisY)
        {
            this.CamPos = aCameraPos;
            this.CamTarget = aCameraTarget;
            this.UpVector = aUpVector;
            this.ViewMatrix = Matrix.CreateLookAt(this.CamPos, aCameraTarget, this.UpVector);
            this.AxisX = aAxisX;
            this.AxisY = aAxisY;
        }

        #region Fields
        internal readonly Vector3 CamPos;
        internal readonly Vector3 CamTarget;
        internal readonly Vector3 UpVector;
        internal readonly Vector3 AxisX;
        internal readonly Vector3 AxisY;
        #endregion

        internal static CAvatar Default
            => new CAvatar(Vector3.Zero,
                           new Vector3(0, 0, MoveVectorLength),
                           Vector3.Up,
                           new Vector3(1, 0, 0),
                           new Vector3(0, 1, 0)
                            );

        internal Vector3 CamTargetOffset => this.CamTarget - this.CamPos;

        internal const float MoveVectorLength = 1f;
        internal readonly Matrix ViewMatrix;



        internal CAvatar LookUpDown(float aRadians)
        {
            var m = Matrix.CreateFromAxisAngle(this.AxisX, aRadians); // Matrix.CreateRotationX(aRadians);
            var u = m.Rotate(this.UpVector);
            var t = this.CamPos + m.Rotate(this.CamTargetOffset);
            var x = this.AxisX;
            var y = m.Rotate(this.AxisY);
            return new CAvatar(this.CamPos, t, u, x, y);
        }


        public CAvatar LookLeftRight(float aRadians)
        {
            var m = Matrix.CreateFromAxisAngle(this.AxisY, aRadians);
            var u = m.Rotate(this.UpVector);
            var t = this.CamPos + m.Rotate(this.CamTargetOffset);
            var x = m.Rotate(this.AxisX);
            var y = this.AxisY;
            var a = new CAvatar(this.CamPos, t, u, x, y);
            return a;
        }
        //=> new CAvatar(this.CamPos, this.CamTargetOffset.RotateY(aRadians), this.UpVector, true);
        internal CAvatar MoveToOffset(Vector3 aMoveVector)
            => throw new NotImplementedException();
        // => new CAvatar(this.CamPos + aMoveVector, this.CamTargetOffset.Transform(aMoveVector), this.UpVector, true);

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
            if (aDistance != 0f)
            {
                var aMoveVector = this.CamTargetOffset;
                var aLonger = aMoveVector.MakeLongerDelta(aDistance);
                var aNewCameraPosition = this.CamPos + aLonger;
                var aNewCameraTarget = this.CamTarget + aLonger;
                var aAvatar = new CAvatar(aNewCameraPosition, aNewCameraTarget, this.UpVector, this.AxisX, this.AxisY);
                return aAvatar;
            }
            else
            {
                return this;
            }
        }
        #region SErialize
        internal void Write(Stream aStream)
        {
            aStream.Write(this.CamPos);
            aStream.Write(this.CamTarget);
            aStream.Write(this.UpVector);
            aStream.Write(this.AxisX);
            aStream.Write(this.AxisY);
        }

        internal static CAvatar Read(Stream aStream)
        {
            var aCamPos = aStream.ReadVector3();
            var aCamTarget = aStream.ReadVector3();
            var aUpVector = aStream.ReadVector3();
            var aAxisX = aStream.ReadVector3();
            var aAxisY = aStream.ReadVector3();
            return new CAvatar(aCamPos, aCamTarget, aUpVector, aAxisX, aAxisY);
        }
        private static FileInfo FileInfo => new FileInfo(Path.Combine(new FileInfo(typeof(CAvatar).Assembly.Location).Directory.FullName, "Avatar.bin"));

        internal void Save()
        {
            var aMemoryStream = new MemoryStream();
            this.Write(aMemoryStream);
            aMemoryStream.Seek(0, SeekOrigin.Begin);
            File.WriteAllBytes(CAvatar.FileInfo.FullName, aMemoryStream.ToArray());
        }
        internal static CAvatar Load()
        {
            var aFileInfo = FileInfo;
            if (aFileInfo.Exists)
            {
                try
                {
                    var aMemoryStream = new MemoryStream(File.ReadAllBytes(aFileInfo.FullName));
                    var aAvatar = Read(aMemoryStream);
                    return aAvatar;
                }
                catch (Exception)
                {
                    return CAvatar.Default;
                }
            }
            return CAvatar.Default;
        }
        #endregion
        internal CVector3Dbl WorldPos => this.CamPos.GetGameCoordinates();
    }

    internal sealed class CGame : Game
    {
        internal CGame(CServiceLocatorNode aParentNullable = null)
        {
            this.RootServiceLocatorNode = aParentNullable;

            this.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            //this.GraphicsDeviceManager.IsFullScreen = true;
            this.Content.RootDirectory = "Content\\bin";
            this.MonoFacade = new CMonoFacade(this.RootServiceLocatorNode, this);
            this.OriginFeature = CFeature.Get(this.ServiceLocatorNode, OriginFeatureDeclaration);

            this.RegisterComponentServices(this.RootServiceLocatorNode.ServiceContainer);
        }

        #region GameVm
        private CGameVm GameVmM;
        private CGameVm GameVm => CLazyLoad.Get(ref this.GameVmM, () => new CGameVm(this.ServiceLocatorNode));
        #endregion
        #region Joystick
        private CJoystick1 Joystick1M;
        private CJoystick1 Joystick1 => CLazyLoad.Get(ref this.Joystick1M, () => new CJoystick1(this.ServiceLocatorNode));
        #endregion

        protected override void EndRun()
        {
            base.EndRun();
            this.Avatar.Save();
        }

        #region ServiceLocator
        private CServiceLocatorNode RootServiceLocatorNodeM;
        internal CServiceLocatorNode RootServiceLocatorNode
        {
            get => CLazyLoad.Get(ref this.RootServiceLocatorNodeM, () => new CDefaultServiceLocatorNode());
            private set => this.RootServiceLocatorNodeM = value;
        }
        internal CServiceLocatorNode ServiceLocatorNode => this.MonoFacade;
        internal void RegisterComponentServices(CServiceContainer aServiceContainer)
        {
            this.GameVm.RegisterComponentServices(aServiceContainer);
            aServiceContainer.AddService<CGame>(() => this);
        }
        #endregion
        internal readonly CMonoFacade MonoFacade;
        public object VmMonoFacade => this.MonoFacade;

        internal CWorld World => this.MonoFacade.World;
        private readonly GraphicsDeviceManager GraphicsDeviceManager;
        private SpriteBatch SpriteBatch;
        //B   private Texture2D BallTexture;

        protected override void LoadContent()
        {

            this.Avatar = CAvatar.Load();
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            // this.BallTexture = this.Content.Load<Texture2D>("Ball");

            //this.Content.Load<Texture>
            base.LoadContent();
        }

        protected override void Initialize()
        {
            this.Init3d();

            this.MonoFacade.Load();

            base.Initialize();
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

        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration SlowDownNearObjectFeatureDeclaration = new CFeatureDeclaration(new Guid("4c4030e4-4477-4350-be27-3ad0db397e40"), "Game.SlowDownNearObject", false);
        private CFeature SlowDownNearObjectFeatureM;
        private CFeature SlowDownNearObjectFeature => CLazyLoad.Get(ref this.SlowDownNearObjectFeatureM, () => CFeature.Get(this.World, SlowDownNearObjectFeatureDeclaration));
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration XnaMouseEnabledFeatureDeclaration = new CFeatureDeclaration(new Guid("99e270ec-f288-4a86-8cde-caaf8af85cff"), "Mouse.Xna", false);
        private CFeature XnaMouseEnabledFeatureM;
        private CFeature XnaMouseEnabledFeature => CLazyLoad.Get(ref this.XnaMouseEnabledFeatureM, () => CFeature.Get(this.World, XnaMouseEnabledFeatureDeclaration));
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration FullScreenFeatureDeclaration = new CFeatureDeclaration(new Guid("99a83ab0-c037-4d78-9d2d-2adc1bcd627e"), "FullScreen", false);
        private CFeature FullScreenFeatureM;
        private CFeature FullScreenFeature => CLazyLoad.Get(ref this.FullScreenFeatureM, () => CFeature.Get(this.World, FullScreenFeatureDeclaration));
        #endregion
        #region Mouse
        private CMouse MouseM;
        private CMouse Mouse => CLazyLoad.Get(ref this.MouseM, () => new CMouse(this.ServiceLocatorNode));
        #endregion

        private void UpdateInput(GameTime aGameTime)
        {
            var aXnaMouse = this.XnaMouseEnabledFeature.Enabled;

            var aJoystick = this.Joystick1;
            var aMouse = this.Mouse;
            var aKeyboardState = Keyboard.GetState();
            var aAvatar = this.Avatar;
            bool aChanged = false;

            var aMouseDx = 0f;
            var aMouseDy = 0f;
            var aMouseThroodle = 0f;

            if (aXnaMouse)
            {
                var aMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
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

                aRotX += (float)aJoystick.GetAxis(CJoystick1.CAxisEnum.Z);

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
                var aRotY = aMouseDy * 0.5f;
                if (aKeyboardState.IsKeyDown(Keys.Down))
                    aRotY += 1.0f;
                if (aKeyboardState.IsKeyDown(Keys.Up))
                    aRotY -= 1.0f;

                //aRotY += (float)aJoystick.GetAxis(CJoystick1.CAxisEnum.Y);

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
                var aDistance2 = this.SlowDownNearObjectFeature.Enabled
                               ? aDistance1 * this.World.FrameInfo.NearPlanetSpeed
                               : aDistance1 * 0.4;
                var aDistance3 = aDistance2 * this.World.Speed;
                var aDistance = aDistance3;
                aAvatar = aAvatar.MoveAlongViewAngle((float)aDistance);
                aChanged = true;
            }

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
            //var aController = new SharpDX.XInput.Controller(0);
            //var aIsConnected = aController.IsConnected;

            //if(Joystick.IsSupported)
            //{
            //    var aJoystickState = Joystick.GetState(0);
            //    var aAxes = aJoystickState.Axes[0];
            //    //aJoystickState.Axes.Length
            //}
            //var aState = GamePad.GetState(0);


            if (aChanged)
            {
                this.Avatar = aAvatar;
            }
        }

        protected override void EndDraw()
        {


            base.EndDraw();
            this.GraphicsDeviceManager.IsFullScreen = this.FullScreenFeature.Enabled;

        }

        internal void Escape()
        {
            this.XnaMouseEnabledFeature.Enabled = false;
            this.Mouse.WinFormMouseEnabledFeature.Enabled = false;
            this.FullScreenFeature.Enabled = false;
        }
        private void UpdateWorld(GameTime aGameTime)
        {
            this.MonoFacade.WorldPos = this.Avatar.WorldPos; // aGameAvatar.WorldPos;
            this.World.Update(this.Avatar.WorldPos, aGameTime);
        }
        protected override void Update(GameTime aGameTime)
        {
            this.DebugWindowUpdate.RunUpdateActions();
            this.UpdateInput(aGameTime);
            this.UpdateWorld(aGameTime);
            //   this.DebugWindowUpdate = new CDebugWindowUpdate();
            //       this.Joystick1.BeginUpdate();

            this.Mouse.NextFrame();

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
        private CAvatar AvatarM = CAvatar.Default;
        internal CAvatar Avatar
        {
            get => this.AvatarM;
            set
            {
                this.AvatarM = value;
                if (this.AvatarChanged is object)
                    this.AvatarChanged();
            }
        }
        internal event Action AvatarChanged;
        #endregion

        #region Camera
        private float CamSpeedRatio => 1.0f; //(float)(this.MonoFacade.World.EdgeLen);
        private float CamSpeedY => this.CamSpeedRatio;
        private float CamSpeedX => this.CamSpeedRatio;
        private float CamSpeedRy => 90f;
        private float CamSpeedRx => 90f;

        private float CamSpeedThroodle => this.CamSpeedRatio;

        #endregion
        #region WorldMatrix
        internal Matrix WorldMatrix { get; private set; }
        private void InitWorldMatrix()
        {
            this.WorldMatrix = Matrix.CreateWorld(new Vector3(0), Vector3.Forward, Vector3.Up);
        }
        #endregion
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
        [CFeatureDeclaration]
        internal static readonly CFeatureDeclaration OriginFeatureDeclaration = new CFeatureDeclaration(new Guid("64f3ce9c-9960-443c-9553-a10c395695a0"), "OriginCoordinates", true);
        private readonly CFeature OriginFeature;

        private void DrawCoordinates()
        {
            var aDrawCoordinates = this.OriginFeature.Enabled;
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

        private void Draw3d()
        {
            GraphicsDevice.Clear(Color.Black);

            //this.DrawTriangle();


            this.MonoFacade.Draw();



            this.DrawCoordinates();

        }


        #endregion
        #region Models
        private CMonoModels ModelsM;
        public CMonoModels Models => CLazyLoad.Get(ref this.ModelsM, () => new CMonoModels(this.ServiceLocatorNode));
        #endregion
    }
}
