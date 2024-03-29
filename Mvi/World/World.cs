﻿using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Mvi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CharlyBeck.Utils3.Enumerables;
using Mvi.Models;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sprites.Asteroid;
using Microsoft.Xna.Framework;
using CharlyBeck.Mvi.Extensions;

using CDoubleRange = System.Tuple<double, double>;
using CIntegerRange = System.Tuple<int, int>;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Sprites.Crosshair;
using CharlyBeck.Mvi.Sprites.Explosion;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Utils3.Asap;
using CharlyBeck.Mvi.Sprites.Avatar;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.Sprites.Gem;
using CharlyBeck.Mvi.Sprites.GemSlot;
using CharlyBeck.Mvi.Sfx;
using CharlyBeck.Mvi.Input;

namespace CharlyBeck.Mvi.World
{

    public struct CVector3Dbl
    {
        public CVector3Dbl(double xyz) : this(xyz, xyz, xyz) { }
        public CVector3Dbl(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public readonly double x;
        public readonly double y;
        public readonly double z;
        public override string ToString() => this.x.ToString() + "|" + this.y.ToString() + "|" + this.z.ToString() + "|";

        internal double GetLength()
            => (float)Math.Sqrt((this.x * this.x) + (this.y * this.y) + (this.z * this.z));

        internal static CVector3Dbl Min(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(Math.Min(lhs.x, rhs.x),
                             Math.Min(lhs.y, rhs.y),
                             Math.Min(lhs.z, rhs.z));
        internal static CVector3Dbl Max(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(Math.Max(lhs.x, rhs.x),
                             Math.Max(lhs.y, rhs.y),
                             Math.Max(lhs.z, rhs.z));

        internal double GetDistance(CVector3Dbl rhs)
            => (Max(this, rhs) - Min(this, rhs)).GetLength();

        public static bool operator ==(CVector3Dbl lhs, CVector3Dbl rhs)
            => lhs.x == rhs.x
            && lhs.y == rhs.y
            && lhs.z == rhs.z
            ;

        public override bool Equals(object obj)
        {
            if (obj is CVector3Dbl pos)
                return this == pos;
            return base.Equals(obj);
        }
        public override int GetHashCode()
            => throw new NotImplementedException(); // this.CalcHashCode(3);

        public static bool operator !=(CVector3Dbl lhs, CVector3Dbl rhs)
            => !(lhs == rhs);
        public static CVector3Dbl operator *(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        public static CVector3Dbl operator /(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
        public static CVector3Dbl operator +(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        public static CVector3Dbl operator -(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);

        internal static CVector3Dbl Parse(string aText)
        {
            var aParts = (from aPart in aText.Split('|') select double.Parse(aPart)).ToArray();
            if (aParts.Length == 3)
            {
                return new CVector3Dbl(aParts[0], aParts[1], aParts[2]);
            }
            throw new ArgumentException();
        }
        internal int CalcHashCode(double aEdgeLen)
            => (int)((this.x + (this.y * aEdgeLen) + this.z * aEdgeLen * aEdgeLen));

        public double GetRadiansXy()
            => Math.Atan(this.y / this.x);

        public double GetRadiansXz()
            => Math.Atan(this.z / this.x);

        public double GetRadiansYz()
            => Math.Atan(this.y / this.z);
    }

}

namespace CharlyBeck.Mvi.World
{

    //internal sealed class CTileBuilder : CServiceLocatorNode
    //{
    //    #region ctor
    //    internal CTileBuilder(CServiceLocatorNode aParent) : base(aParent)
    //    {
    //        this.RandomGenerator = new CRandomGenerator(this);
    //        this.Facade = this.ServiceContainer.GetService<CFacade>();
    //        this.Init();
    //    }
    //    public override T Throw<T>(Exception aException)
    //       => aException.Throw<T>();
    //    #endregion
    //    #region Tile
    //    private CTile TileM;
    //    internal CTile Tile => CLazyLoad.Get(ref this.TileM, () => this.ServiceContainer.GetService<CTile>());
    //    #endregion
    //    #region TileDescriptor
    //    internal CMviQuadrantUserData NewTileDescriptor() => CRootTileDescriptor.New(this, this);

    //    //internal CLoadedTileData NewLoadedTileData(CTileDataLoadProxy aTileDataLoadProxy)
    //    //{
    //    //  //  this.RandomGenerator.Reset(this.Tile);
    //    //   return CLoadedTileData.New(this, this.NewTileDescriptor());
    //    //}
    //    #endregion
    //    #region World
    //    private CWorld WorldM;
    //    internal CWorld World => CLazyLoad.Get(ref this.WorldM, () => this.ServiceContainer.GetService<CWorld>());
    //    #endregion
    //    #region RandomGenerator
    //    internal readonly CRandomGenerator RandomGenerator;
    //    #endregion
    //    #region Facade
    //    internal CFacade Facade { get; private set; }
    //    #endregion
    //}

    internal sealed class CWorldSpriteManagers : CRootSpriteManager
    {
        #region ctor
        internal CWorldSpriteManagers(CServiceLocatorNode aParent) : base(aParent)
        {
            this.AvatarManager = new CAvatarManager(this);
            this.ShotManager = new CShotManager(this);
            this.CrosshairManager = new CCrosshairManager(this);
            this.ExplosionsManager = new CExplosionsManager(this);
            this.SolarSystemSpriteManagers = this.Cube.Quadrants.Select(aQ => aQ.ServiceContainer.GetService<CQuadrantSpriteManager>()).ToArray();
            this.GemSpriteManager = new CGemSpriteManager(this);
            this.GemSlotControlsSpriteManager = new CGemSlotControlsSpriteManager(this);

            this.Items = this.ItemsPr.ToArray();
            this.Init();
        }
        public override void Load()
        {
            base.Load();

            foreach (var aSpriteManager in this.LeafSpriteManagers)
                aSpriteManager.Load();
        }
        #endregion
        #region ShotSprites
        internal readonly CShotManager ShotManager;
        internal void Shoot(CShotEnum aShotEnum)
            => this.ShotManager.Shoot(aShotEnum);
        #endregion
        #region Crosshair
        private readonly CCrosshairManager CrosshairManager;
        #endregion
        #region Explosions
        internal readonly CExplosionsManager ExplosionsManager;
        #endregion
        #region Avatar
        private readonly CAvatarManager AvatarManager;
        internal CVector3Dbl AvatarPos { get => this.AvatarManager.AvatarPos; set => this.AvatarManager.AvatarPos = value; }
        #endregion
        #region Gems
        private readonly CGemSpriteManager GemSpriteManager;
        #endregion
        #region GemSlotControls
        private readonly CGemSlotControlsSpriteManager GemSlotControlsSpriteManager;
        #endregion
        #region Cubes
        internal ICube Cube => this.WormholeCubes;
        private CWormholeCubes WormholeCubesM;
        internal CWormholeCubes WormholeCubes => CLazyLoad.Get(ref this.WormholeCubesM, () => new CWormholeCubes(this));
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CWorldSpriteManagers>(() => this);
            aServiceContainer.AddService<CGemSlotControlsSpriteManager>(() => this.GemSlotControlsSpriteManager);
            return aServiceContainer;
        }
        #endregion
        #region SolarSystemSpriteManagers
        internal readonly CQuadrantSpriteManager[] SolarSystemSpriteManagers;
        #endregion
        #region Composite
        private IEnumerable<CSpriteManager> ItemsPr
        {
            get
            {
                yield return this.AvatarManager;
                yield return this.ShotManager;
                yield return this.CrosshairManager;
                yield return this.ExplosionsManager;
                foreach (var aSpriteManager in this.SolarSystemSpriteManagers)
                    yield return aSpriteManager;
                yield return this.GemSpriteManager;
                yield return this.GemSlotControlsSpriteManager;
            }
        }
        private CSpriteManager[] Items { get; set; }
        internal override IEnumerable<CSprite> BaseSprites
        {
            get
            {
                foreach (var aItem in this.Items)
                    foreach (var aSprite in aItem.BaseSprites)
                        yield return aSprite;
            }
        }

        internal CAvatarSprite AvatarSprite => this.AvatarManager.AvatarSprite;
        #endregion
        #region Update
        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);

            //var a = this.Items;
            //var c = a.Length;
            //for(var i = 0; i < c; ++i)
            //{
            //    var aSpriteManager = a[i];
            //    aSpriteManager.Update(aFrameInfo);
            //}
        }
        #endregion
    }

    public sealed class CWorld : CServiceLocatorNode
    {
        #region ctor
        internal CWorld(CServiceLocatorNode aParent) : base(aParent)
        {
            this.SpaceSwitchQuadrantObjectPool = new CObjectPool<CSpaceSwitchQuadrant>();
            this.EdgeLen = 1.0d;
            this.EdgeLenAsPos = new CVector3Dbl(this.EdgeLen);
            this.TileAsteroidCountMin = CStaticParameters.TileAsteroidCountMin;
            this.TileAsteroidCountMax = CStaticParameters.TileAsteroidCountMax;
            this.AsteroidGravityRadiusMax = 1.0d;
            this.AsteroidGravityStrengthMax = 1.0d;
            this.NearAsteroidSpeedMin = 0.0001;
            this.NearAsteroidSpeedForRadius0 = 0.001;
            this.SphereScaleCount = 25;
            this.Joystick = new CJoystick(this);
            
        }
        public override void Load()
        {
            base.Load();
            this.Cube.Load();
            this.WorldSpriteManagers.Load();
        }
        #endregion
        #region Look
        public double LookUpDown { get; set; }
        public double LookLeftRight { get; set; }
        #endregion
        #region SpriteManagers
        private CWorldSpriteManagers WorldSpriteManagersM;
        private CWorldSpriteManagers WorldSpriteManagers => CLazyLoad.Get(ref this.WorldSpriteManagersM, this.NewWorldSpriteManagers);
        private CWorldSpriteManagers NewWorldSpriteManagers()
        {
            var aSpriteManagers = new CWorldSpriteManagers(this);
            aSpriteManagers.ShotManager.ShotFired += this.OnShotFired;
            return aSpriteManagers;
        }
        #endregion
        #region Shot
        public void Shoot(CShotEnum aShotEnum)
            => this.WorldSpriteManagers.Shoot(aShotEnum);
        #endregion
        #region SpaceSwitchQuadrantObjectPool
        private CObjectPool<CSpaceSwitchQuadrant> SpaceSwitchQuadrantObjectPool;
        #endregion

        private CSpaceSwitchQuadrant NewSpaceSwitchQuadrant(CServiceLocatorNode aParent)
        {
            return this.SpaceSwitchQuadrantObjectPool.Allocate(new Func<CSpaceSwitchQuadrant>(() => new CSpaceSwitchQuadrant(aParent)));
        }
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CWorld>(() => this);
            aServiceContainer.AddService<CNewBorderFunc>(() => new CNewBorderFunc(()=> this.CubeSize));
            aServiceContainer.AddService<CWormholeCubes>(() => this.WorldSpriteManagers.WormholeCubes);
            aServiceContainer.AddService<CNewQuadrantFunc>(() => new CNewQuadrantFunc(this.NewSpaceSwitchQuadrant));
            aServiceContainer.AddService<CCubePersistentData>(() => this.CubePersistentData);
            aServiceContainer.AddService<CJoystickState>(() => this.Joystick.JoystickState);
            aServiceContainer.AddService<CAvatarSprite>(() => this.AvatarSprite);
            return aServiceContainer;
        }
        #endregion
        #region Draw
        internal void Draw()
        {
            //var aSuns = this.Sprites.OfType<CharlyBeck.Mvi.Sprites.SolarSystem.CSun>();
            foreach (var aSprite in this.Sprites) // TODO - SpriteMAnagers.Draw verwenden mit schnelleren iteration über riesen-Array.
                aSprite.Draw();
        }
        #endregion
        #region Models
        private CModels ModelsM;
        public CModels Models => CLazyLoad.Get(ref this.ModelsM, () => new CModels(this));
        #endregion
        #region Avatar
        internal CAvatarSprite AvatarSprite => this.WorldSpriteManagers.AvatarSprite;
        public CVector3Dbl AvatarWorldPos { get => this.WorldSpriteManagers.AvatarPos; set => this.WorldSpriteManagers.AvatarPos = value; }
        public CVector3Dbl AvatarShootDirection { get; set; }
        public double AvatarSpeed { get; set; }
        #endregion
        #region Joystick
        internal readonly CJoystick Joystick;
        #endregion
        public readonly double EdgeLen;
        internal readonly CVector3Dbl EdgeLenAsPos;
        internal readonly double NearAsteroidSpeedForRadius0;
        internal readonly int TileAsteroidCountMin;
        internal readonly int TileAsteroidCountMax;
        internal readonly CDoubleRange DefaultAsteroidRadiusMax = CStaticParameters.DefaultAsteroidRadiusMax;
        internal readonly CDoubleRange SunRadiusMax = CStaticParameters.SunRadiusMax; 
        internal readonly CDoubleRange PlanetRadiusMax = CStaticParameters.PlanetRadiusMax;  
        internal readonly CDoubleRange MoonRadiusMax = CStaticParameters.MoonRadiusMax; 

        internal readonly double AsteroidGravityRadiusMax;
        internal readonly double AsteroidGravityStrengthMax;
        internal readonly double NearAsteroidSpeedMin;
        internal readonly Int64 CubeSize = CStaticParameters.Cube_Size;
        internal double OrbDayDurationMin => 1d;
        internal double OrbDayDurationMax => 20d;
        internal CDoubleRange PlanetYearDurationRange => new CDoubleRange(0.1d, 0.3d); // new CDoubleRange(0.01d, 20d);
        internal CDoubleRange MoonYearDurationRange => new CDoubleRange(0.01d, 0.5d);
        internal CIntegerRange PlanetMoonCountRange => new CIntegerRange(0, 2);
        internal double PlanetHasMoonsProbability => 0.3d;
        internal CDoubleRange PlanetOrbitRange => new CDoubleRange(2d, 3d);
        internal CDoubleRange MoonOrbitRange => new CDoubleRange(4.0d, 5.0d);

        #region FrameInfo
        private void RefreshFrameInfo()
        {
            this.FrameInfoM = default;
        }
        private CFrameInfo? FrameInfoM;
        public CFrameInfo FrameInfo => CLazyLoad.Get(ref this.FrameInfoM, () => new CFrameInfo(this)); //{ get =>; internal set; }
        #endregion

        public int SphereScaleCount;
        internal GameTime GameTimeNullable { get; set; }
        public TimeSpan GameTimeTotal => this.GameTimeNullable is object ? this.GameTimeNullable.TotalGameTime : new TimeSpan(0,0,0,0,1); // 1 to avoid division by 0
        public TimeSpan GameTimeElapsed => this.GameTimeNullable is object ? this.GameTimeNullable.ElapsedGameTime : new TimeSpan(0, 0, 0, 0, 0);
        public double Speed = 1.0d;

        public GameTime GameTime { get => this.GameTimeNullable; set => this.GameTimeNullable = value; }
        private IEnumerable<CSpaceQuadrant> SpaceQuadrants => this.Cube.Quadrants.OfType<CSpaceQuadrant>();


        internal IEnumerable<CSprite> Sprites
        {
            get
            {
                if (!this.InitFrame)
                {
                    foreach (var aSprite in this.WorldSpriteManagers.BaseSprites)
                        yield return aSprite;

                    //foreach (var aSprite in from aItem in this.SpaceQuadrants from aSprite in aItem.Sprites select aSprite)
                    //    yield return aSprite;

                }
            }
        }
        #region Events
        internal event Action ShootFired;
        internal void OnShotFired()
        {
            if(this.ShootFired is object)
            {
                this.ShootFired();
            }    
        }
        internal event Action<CCollectedGemSprite> GemCollected;
        internal void OnGemCollected(CCollectedGemSprite aGemSprite)
        {
            if(this.GemCollected is object)
            {
                this.GemCollected(aGemSprite);
            }    
        }
        internal event Action<CCollectedGemSprite, Action<CSoundFile>> GemCollectedSoundStarting;
        internal void OnGemCollectedSoundStarting(CCollectedGemSprite aGemSprite, Action<CSoundFile> aAddFollowUp)
        {
            if (this.GemCollectedSoundStarting is object)
            {
                this.GemCollectedSoundStarting(aGemSprite, aAddFollowUp);
            }
        }
        internal event Action<CCollectedGemSprite> GemActivated;
        internal void OnGemActivated(CCollectedGemSprite aGemSprite)
        {
            if(this.GemActivated is object)
            {
                this.GemActivated(aGemSprite);
            }
        }
        #endregion
        internal bool InitFrame = true;
        #region Values
        [CMemberDeclaration]
        private static readonly CBoolDeclaration GravitationValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_Gravitation, new Guid("61de4feb-eb03-4569-baea-055d520844b9"), true, CStaticParameters.Gravity_Enabled);
        private CBoolValue GravitationValueM;
        public  CBoolValue GravitationValue => CLazyLoad.Get(ref this.GravitationValueM, () => CValue.GetStaticValue<CBoolValue>(this, GravitationValueDeclaration));
        #endregion
        public void Update()
        {
            this.InitFrame = false;
            this.MoveVectorM = default;
            this.Joystick.Update();
            this.Cube.MoveTo(this.GetCubePos(this.AvatarWorldPos), true);
            this.WorldSpriteManagers.UpdateAvatarPos();
            this.RefreshFrameInfo();
            this.WorldSpriteManagers.Update(this.FrameInfo);
            this.WorldSpriteManagers.Collide();
            this.WorldSpriteManagers.RemoveDeadSprites();
        }

        internal CCubePos GetCubePos(CVector3Dbl aWorldPos)
            => CWorldPosToCubePos.GetCubePos(aWorldPos, this.EdgeLenAsPos);

        #region OnHit
        internal event Action<CSprite, CShotSprite> SpriteDestroyedByShot;
        internal void OnDestroyedByShot(CSprite aSprite, CShotSprite aShotSprite)
        {
            if(this.SpriteDestroyedByShot is object)
            {
                this.SpriteDestroyedByShot(aSprite, aShotSprite);
            }

            this.WorldSpriteManagers.ExplosionsManager.AddExplosion(aSprite.WorldPos.Value, aSprite.Radius.GetValueOrDefault(1.0d));
        }
        #endregion
        #region Explosion


        public event Action<CSprite> WormholeEntered;
        internal void OnWormholeEntered(CSprite aSprite)
        {
            if(this.WormholeEntered is object)
            {
                this.WormholeEntered(aSprite);
            }
        }
        #endregion

        #region CubePersistentData
        private CCubePersistentData CubePersistentDataM;
        internal CCubePersistentData CubePersistentData => CLazyLoad.Get(ref this.CubePersistentDataM, () => new CCubePersistentData(this));
        #endregion
        #region MoveVector
        public CVector3Dbl ThroodleDirection { get; set; }

        internal CVector3Dbl OldMoveVector;
        CVector3Dbl? MoveVectorM;
        public CVector3Dbl MoveVector => CLazyLoad.Get(ref this.MoveVectorM, ()=>this.NewMoveVector());
        internal CVector3Dbl NewMoveVector()
        {
            var aMoveVector = this.OldMoveVector;
            var aAttraction = this.FrameInfo.Attraction;
            var aThroodle = this.Throodle;
            var aThroodleVec = this.ThroodleDirection.MakeLongerDelta(aThroodle);
            var aNewMoveVector1 = aMoveVector
                               + aThroodleVec
                               + aThroodleVec
                               ;
            var aNewMoveVector2a = this.SlowDownNearObjectValue.Value
                               ? aNewMoveVector1 * new CVector3Dbl(this.FrameInfo.NearPlanetSpeed)
                               : aNewMoveVector1
                               ;
            var aNewMoveVector2 = aNewMoveVector2a * new CVector3Dbl(0.05d);
            //this.OldMoveVector = aNewMoveVector2;

            var aMaxSpeed = new CVector3Dbl(0.0002);
            var aNewMoveVector3 = aNewMoveVector2.Min(aMaxSpeed);
            var aNewMoveVector4 = aNewMoveVector3 + aAttraction;
            
            return aNewMoveVector2;
        }
        public double Throodle
        {
            get;
            set;
        }
        #endregion
        [CMemberDeclaration]
        private static readonly CBoolDeclaration LandingDeclaration = new CBoolDeclaration
            (CValueEnum.Global_LandingMode, new Guid("065a36b6-cebe-43c4-bffc-c4e9bae64334"), false, false);
        private CBoolValue SlowDownNearObjectValueM;
        public CBoolValue SlowDownNearObjectValue => CLazyLoad.Get(ref this.SlowDownNearObjectValueM, () => CValue.GetStaticValue<CBoolValue>(this, LandingDeclaration));
        #region Cube
        internal ICube Cube => this.WorldSpriteManagers.Cube;
        #endregion
    }

    public struct CAvatarInfo
    {
        public CAvatarInfo(CVector3Dbl aWorldPos, CVector3Dbl aAimAt, double aSpeed)
        {
            this.WorldPos = aWorldPos;
            this.AimAt = aAimAt;
            this.Speed = aSpeed;
        }
        public readonly CVector3Dbl WorldPos;
        public readonly CVector3Dbl AimAt;
        public readonly double Speed;

    }
}

namespace CharlyBeck.Mvi.World
{
    using CSpriteDistance = Tuple<CSprite, double>;
    public struct CFrameInfo
    {
        internal CFrameInfo(CWorld aWorld)
        {
            this.World = default;
            this.Sprites = default;
            this.SpriteDistances = default;
            this.NearestBumperAndDistance = default;
            this.NearPlanetSpeedM = default;
            this.CubePositionsM = default;
            this.AttractionM = default;
            this.AvatarMoveVectorM = default;
    
            this.World = aWorld;
            var aCube = aWorld.Cube;
            {
                this.Sprites = this.World.Sprites.Where(s=>s.BuildIsDone && s.IsInUse && !s.IsHiddenInWorld.Value).ToArray();
                this.SpriteDistances = (from aSprite in this.Sprites
                                        where aSprite.HasDistanceToAvatar
                                        select new Tuple<CSprite, double>(aSprite, aSprite.DistanceToAvatar)).ToArray().OrderBy(aDist => aDist.Item2).ToArray();
                this.NearestBumperAndDistance = (from aSpriteAndDistance in this.SpriteDistances where (aSpriteAndDistance.Item1 is CBumperSprite) select new Tuple<CBumperSprite, double>((CBumperSprite)aSpriteAndDistance.Item1, aSpriteAndDistance.Item2)).FirstOrDefault();
                this.CubePositions = this.World.Cube.CubePositions;
            }
        }

        #region Attraction
        private CVector3Dbl? AttractionM;
        public CVector3Dbl Attraction => CLazyLoad.Get(ref this.AttractionM, this.NewAttraction);
        private CVector3Dbl NewAttraction()
        {
            if (this.World.GravitationValue.Value)
            { // TODO_OPT: Über array iterieren
                var aSprites = this.Sprites;
                var aAttractions = from aSprite in aSprites
                                   where aSprite.MassIsDefined
                                      && aSprite.IsInUse
                                   select aSprite.AttractionToAvatar;
                var aAttraction = aAttractions.Sum();
                return aAttraction;
            }
            else
            {
                return new CVector3Dbl();
            }
        }

        #endregion
        #region AvatarMoveVector
        private CVector3Dbl? AvatarMoveVectorM;
        public CVector3Dbl AvatarMoveVector => CLazyLoad.Get(ref this.AvatarMoveVectorM, this.NewAvatarMoveVector);
        private CVector3Dbl NewAvatarMoveVector()
        {
            var aAttraction = this.Attraction;
            var aFrameTileElapsed = new CVector3Dbl(this.GameTimeElapsed.TotalMilliseconds);
            var aAttractionMove = aAttraction * aFrameTileElapsed;
            return aAttractionMove;
        }
        #endregion

        public TimeSpan GameTimeElapsed => this.World.GameTimeElapsed;
        public TimeSpan GameTimeTotal => this.World.GameTimeTotal;

        public readonly CWorld World;
        public CVector3Dbl AvatarWorldPos =>this.World.AvatarWorldPos;
        public readonly CSprite[] Sprites;
        public readonly CSpriteDistance[] SpriteDistances;
        public IEnumerable<CSprite> SpritesOrderedByDistance => from aItem in this.SpriteDistances select aItem.Item1;
        public readonly Tuple<CBumperSprite, double> NearestBumperAndDistance;
        public CBumperSprite NearestAsteroid => this.NearestBumperAndDistance.Item1;
        private IEnumerable<CCubePos> CubePositionsM;
        public IEnumerable<CCubePos> CubePositions { get => this.CubePositionsM is object ? this.CubePositionsM : Array.Empty<CCubePos>(); private set => this.CubePositionsM = value; }

        
        public bool NearestBumperIsDefined => this.NearestBumperAndDistance is object && this.NearestBumperAndDistance.Item1 is object;

        public double NearPlanetSpeed { get => CLazyLoad.Get(ref this.NearPlanetSpeedM, this.NewNearPlanetSpeed); }
        public double? NearPlanetSpeedM;
        private double NewNearPlanetSpeed()
        {
            if (!this.NearestBumperIsDefined)
            {
                return 1d;
            }
            else
            {
                var aDistance =  Math.Abs(this.NearestAsteroid.AvatarDistanceToSurface);
                var rm = this.World.DefaultAsteroidRadiusMax.Item2;
                var rf =  (this.NearestAsteroid.Radius / rm);
                var s1 = 0; // (1d - rf) * this.World.NearAsteroidSpeedForRadius0;
                var s2 = aDistance;
                var s3 = (s1 + s2) / 2d; // s1 + s2;
                var s = Math.Max(this.World.NearAsteroidSpeedMin, s3);
                return s;
            }
        }

    }


}