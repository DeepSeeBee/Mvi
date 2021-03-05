using CharlyBeck.Mvi.Cube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites
{

    public abstract class CBuildable : CReuseable
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
            if (this.Visible)
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
            if (!this.IsUnloaded)
            {
                this.OnUnload();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        #endregion
        //#region SpriteRegistry
        //private CSpriteRegistry SpriteRegistryM;
        //internal CSpriteRegistry SpriteRegistry => CLazyLoad.Get(ref this.SpriteRegistryM, () => this.ServiceContainer.GetService<CSpriteRegistry>());
        //#endregion
    }

    public abstract class CTileDescriptorBuildable : CBuildable
    {
        internal CTileDescriptorBuildable(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CMviQuadrantUserData aTileDescriptor) : base(aParent, aTileBuilder)
        {
            this.TileDescriptor = aTileDescriptor;
        }

        internal readonly CMviQuadrantUserData TileDescriptor;

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
}
