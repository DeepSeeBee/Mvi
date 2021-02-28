using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Mvi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites
{

    public abstract class CSpriteData
    {
        internal CSpriteData(CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor)
        {
            this.WorldGenerator = aTileBuilder.WorldGenerator;
            this.Facade = aTileBuilder.Facade;
            this.Changes = new BitArray(this.ChangesCount);
            this.AbsoluteCubeCoordinates = aTileBuilder.Tile.AbsoluteCubeCoordinates;
            aTileDescriptor.SpriteDatas.Add(this);
        }
        internal virtual void Init()
        {
            this.Sprite = this.NewSprite();
            this.Update();
        }

        public readonly CCubePos AbsoluteCubeCoordinates;
        public abstract CVector3Dbl WorldPos { get; } // => this.World.GetWorldPos(this.AbsoluteCubeCoordinates);


        internal readonly CFacade Facade;
        internal CWorld World => this.Facade.World;
        internal readonly CWorldGenerator WorldGenerator;
        internal abstract ISprite NewSprite();
        internal ISprite<TData> NewSprite<TData>(TData aData)
            => this.Facade.NewSprite<TData>(aData);
        internal virtual int ChangesCount => 0;
        internal ISprite Sprite { get; private set; }
        internal readonly BitArray Changes;
        internal void Update()
        {
            if (this.Sprite is object)
            {
                this.Sprite.Update(this.Changes);
            }
            this.Changes.SetAll(false);
        }

        internal virtual bool Visible => true;
        internal void Draw()
        {
            if (this.Visible)
            {
                this.Sprite.Draw();
            }
        }

        internal void Unload()
        {
            if (this.Sprite is object)
            {
                this.Sprite.Unload();
            }
        }

        public bool IsNearest;

        internal virtual void UpdateFromFrameInfo(CFrameInfo aFrameInfo)
        {
            this.IsNearest = object.ReferenceEquals(this.World.FrameInfo.SpriteDistances.First().Item1, this);
        }

        private CModel ModelM;
        public CModel Model => CLazyLoad.Get(ref this.ModelM, this.NewModel);
        internal virtual CModel NewModel() => throw new NotImplementedException();
        internal virtual bool ModelIsDefined => false;

        public double GetAlpha(CVector3Dbl aCameraPos)
        {
            var d = this.WorldPos.GetDistance(aCameraPos);
            var dmax = this.World.EdgeLen; // * ((this.World.Cube.EdgeLength - 1) / 2);
            var df = Math.Min(1d, Math.Max(0d, d / dmax));
            //var em = 1.0d;
            var a = 1d - df; // 1 - (Math.Exp(df * em) / Math.Pow( Math.E ,em));
            return a;
        }

        internal virtual void UpdateFromWorldPos(CVector3Dbl aAvatarPos)
        {
            this.DistanceToAvatar = aAvatarPos.GetDistance(this.WorldPos);
        }

        public double DistanceToAvatar { get; private set; }
    }

    internal abstract class CWorldTileDescriptor : CTileDescriptor
    {
        #region ctor
        internal CWorldTileDescriptor(CTileBuilder aTileBuilder) : base(aTileBuilder)
        {
        }
        #endregion

    }


    internal sealed class CWorldGenerator : CRandomGenerator
    {
        #region ctor
        internal CWorldGenerator(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();


        #endregion
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
