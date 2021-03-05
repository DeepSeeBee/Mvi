using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Mvi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites
{
    using CTranslateAndRotate = Tuple<CVector3Dbl, CVector3Dbl>;

    public abstract class CBuildable : CServiceLocatorNode
    {
        #region ctor
        internal CBuildable(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent)
        {
            this.TileBuilder = aTileBuilder;
        }
        #endregion
        #region Build
        protected virtual void OnBuildSprite() { }
        protected virtual void OnUpdate() { }
        public void Update() 
        {
            this.CheckBuildIsDone();
            this.OnUpdate();
        }
        protected void Build()
        {
            this.CheckNotBuildIsDone();
            this.OnBuild();
            this.OnBuildSprite();
            this.TileBuilder = default;
            this.BuildIsDone = true;
            this.Update();
        }
        protected abstract void OnBuild();
        private void CheckNotBuildIsDone()
        {
            if (this.BuildIsDone)
                throw new InvalidOperationException();
        }
        private void CheckBuildIsDone()
        {
            if (!this.BuildIsDone)
                throw new InvalidOperationException();
        }
        private bool BuildIsDone;
        private CTileBuilder TileBuilderM;
        internal CTileBuilder TileBuilder
        {
            get
            {
                this.CheckNotBuildIsDone();
                return this.TileBuilderM;
            }
            private set { this.TileBuilderM = value; }
        }
        internal CTile Tile => this.TileBuilder.Tile;
        protected virtual void OnDraw()
        { }
        public void Draw()
        {
            this.CheckBuildIsDone();
            if(this.Visible)
            { 
                this.OnDraw();
            }
        }
        #endregion  
        internal virtual bool Visible => true;
        #region Unload
        internal virtual void OnUnload()
        {
        }
        private bool IsUnloaded;
        internal void Unload()
        {
            if(!this.IsUnloaded)
            {
                this.OnUnload();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        #endregion
        #region SpriteRegistry
        private CSpriteRegistry SpriteRegistryM;
        internal CSpriteRegistry SpriteRegistry => CLazyLoad.Get(ref this.SpriteRegistryM, () => this.ServiceContainer.GetService<CSpriteRegistry>());
        #endregion
    }

    public abstract class CTileDescriptorBuildable : CBuildable
    {
        internal CTileDescriptorBuildable(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder)
        {
            this.TileDescriptor = aTileDescriptor;
        }
 
        internal readonly CTileDescriptor TileDescriptor;

        #region MultiverseCube
        private CMultiverseCube MultiverseCubeM;
        private CMultiverseCube MultiverseCube => CLazyLoad.Get(ref this.MultiverseCubeM, () => this.ServiceContainer.GetService<CMultiverseCube>());
        #endregion
        #region Cube
        private CCube CubeM;
        internal CCube Cube => CLazyLoad.Get(ref this.CubeM, () => this.ServiceContainer.GetService<CCube>());
        #endregion
        internal CVector3Dbl GetWorldPos(CCubePos aCubePos)
            => this.MultiverseCube.GetWorldPos(aCubePos);
    }

    public abstract class CSpriteData : CTileDescriptorBuildable
    {
        internal CSpriteData(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor):base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.WorldGenerator = aTileBuilder.WorldGenerator;
            this.Facade = aTileBuilder.Facade;
            this.Changes = new BitArray(this.ChangesCount);
            this.CubePosAbs = aTileBuilder.Tile.AbsoluteCubeCoordinates;
            aTileDescriptor.SpriteRegistry.Add(this);
        }

        internal IEnumerable<CTranslateAndRotate> TranslateAndRotates
        {
            get
            {
                yield return new CTranslateAndRotate(this.WorldPos, new CVector3Dbl(0));
            }
        }
        public readonly CCubePos CubePosAbs;
        public abstract CVector3Dbl WorldPos { get; } // => this.World.GetWorldPos(this.AbsoluteCubeCoordinates);

        protected override void OnBuildSprite()
        {
            base.OnBuildSprite();
            this.Sprite = this.NewSprite();
        }
        internal readonly CFacade Facade;
        internal CWorld World => this.Facade.World;
        internal readonly CWorldGenerator WorldGenerator;
        internal abstract ISprite NewSprite();
        internal ISprite<TData> NewSprite<TData>(TData aData)
            => this.Facade.NewSprite<TData>(aData);
        internal virtual int ChangesCount => 0;
        internal ISprite Sprite { get; private set; }
        internal readonly BitArray Changes;
        protected override void OnUpdate()
        {
            base.OnUpdate();
            //if (this.Sprite is object)
            //{
            //    this.Sprite.Update(this.Changes);
            //}
            //this.Changes.SetAll(false);
        }


        protected override void OnDraw()
        {
            base.OnDraw();
            if (this.Visible
            && this.Sprite is object) // hack
            {
                this.Sprite.Draw();
            }
        }

        internal override void OnUnload()
        {
            base.OnUnload();
            if (this.Sprite is object)
            {
                this.Sprite.Unload();
                this.Sprite = default;
            }
        }

        public bool IsNearest;

        internal virtual void UpdateAfteFrameInfo(CFrameInfo aFrameInfo)
        {
            this.IsNearest = this.World.FrameInfo.SpriteDatasOrderedByDistance.OfType<CSpriteData>().First().RefEquals<CSpriteData>(this);
        }

        private CModel ModelM;
        public CModel Model => CLazyLoad.Get(ref this.ModelM, this.NewModel);
        internal virtual CModel NewModel() => throw new NotImplementedException();
        internal virtual bool ModelIsDefined => false;

        public double GetAlpha(CVector3Dbl aCameraPos)
        {
            var d = this.WorldPos.GetDistance(aCameraPos);
            var dmax = (this.Cube.Depth -1) / 2; // ; ; // * ((this.World.Cube.EdgeLength - 1) / 2);
            var df = Math.Min(1d, Math.Max(0d, d / dmax));
            //var em = 1.0d;
            var a = 1d - df; // 1 - (Math.Exp(df * em) / Math.Pow( Math.E ,em));
            return a;
        }

        internal virtual void UpdateBeforeFrameInfo(CVector3Dbl aAvatarPos)
        {
            this.DistanceToAvatarM = default;
            this.AvatarIsInCubeM = default;
        }

        private double? DistanceToAvatarM;
        public double DistanceToAvatar { get=>CLazyLoad.Get(ref this.DistanceToAvatarM, () => this.FrameInfo.AvatarWorldPos.GetDistance(this.WorldPos)); }
        private bool? AvatarIsInCubeM;
        public bool AvatarIsInTile => CLazyLoad.Get(ref this.AvatarIsInCubeM, () => this.Cube.CubePosAbs == this.CubePosAbs);
        internal CFrameInfo FrameInfo => this.World.FrameInfo;

        //public virtual Matrix WorldMatrix => Matrix.CreateTranslation(this.WorldPos.ToVector3());
    }

    internal abstract class CRootTileDescriptor : CTileDescriptor
    {
        #region ctor
        internal CRootTileDescriptor(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent,aTileBuilder)
        {
        }

        internal static Type[] TileDescriptorTypes = new Type[] 
        {
            typeof(CBumpersTileDescriptor), 
            typeof(CSolarSystem),
        };
        internal static CRootTileDescriptor New(CServiceLocatorNode aParent, CTileBuilder aTileBuilder)
        {
            var aWorld = aTileBuilder.World;
            var aTile = aTileBuilder.Tile;
            var aWorldGenerator = aTileBuilder.WorldGenerator;
            aWorldGenerator.Begin(aTile);
            var aTypes = TileDescriptorTypes;
            var aType = aWorldGenerator.NextItem(aTypes);
            var aTileDescriptor = (CRootTileDescriptor)Activator.CreateInstance(aType, aParent, aTileBuilder);
            aWorldGenerator.End();
            return aTileDescriptor;
        }
        #endregion
        internal override bool OwnSpriteRegistryIsDefined => true;
    }

    public static class CExtensions
    {
        public static IEnumerable<CVector3Dbl> PolyPointsToLines(this IEnumerable<CVector3Dbl> aSquare)
        {
            var aFirst = aSquare.First();
            var aPrevious = aSquare.First();
            foreach (var aPoint in aSquare.Skip(1))
            {
                yield return aPrevious;
                yield return aPoint;
                aPrevious = aPoint;
            }
            yield return aPrevious;
            yield return aFirst;
        }

    }



}
