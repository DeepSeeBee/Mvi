using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Gem;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Utils3.Enumerables;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using CharlyBeck.Mvi.XnaExtensions;

namespace CharlyBeck.Mvi.Sprites.GemSlot
{
    enum CGemButtonEnum
    {
        Side6_LO,
        Side6_LM,
        Side6_LU,
        Side6_RO,
        Side6_RM,
        Side6_RU,

        Top_LO,
        Top_LU,
        Top_RO,
        Top_RU
    }

    internal abstract class CGemJoystickAdapter
    {
        internal abstract bool IsPressed(CGemButtonEnum aButton);
    }

    enum CGemSideEnum
    {
        Left,
        Right,
    }

    internal static class CConsts
    {
        internal const int GemClass_SlotCount = 5;
        public static readonly int GemClass_Count = typeof(CGemCategoryEnum).GetEnumMaxValue() + 1;

    }

    internal sealed class CGemSlot : CServiceLocatorNode
    {
        internal CGemSlot(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        protected override void Init()
        {
            base.Init();
        }
        internal void Draw() { }

        internal int? Index;
        internal CGemCategory GemClass;
        internal CGemSideEnum? GemSlotSideEnum;

        private CGemSprite GemSpriteNullableM;
        internal CGemSprite GemSpriteNullable
        {
            get => this.GemSpriteNullableM;
            set
            {
                var aOldValue = this.GemSpriteNullableM;
                if(this.GemSpriteNullableM is object)
                {
                    if(this.GemSpriteNullableM.GemSlotNullable.RefEquals<CGemSlot>(this))
                    {
                        this.GemSpriteNullableM.GemSlotNullable = default;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                    this.GemSpriteNullableM = null;
                }
                if(value is object)
                {
                    if(!(value.GemSlotNullable is object))
                    {
                        value.GemSlotNullable = this;
                        this.GemSpriteNullableM = value;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }
        internal bool IsFree => !(this.GemSpriteNullable is object);

    }

    internal sealed class CGemClassBar : CServiceLocatorNode
    {
        

        internal CGemClassBar(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        public override void Load()
        {
            base.Load();
            var aCount = CConsts.GemClass_SlotCount;
            var aGemSlots = new CGemSlot[aCount];
            foreach (var aIdx in Enumerable.Range(0, aCount))
            {
                var aGemSlot = new CGemSlot(this)
                {
                    Index = aIdx,
                    GemClass = this.GemClass,
                    GemSlotSideEnum = this.GemSideEnum
                };
                aGemSlots[aIdx] = aGemSlot;
            }
            this.GemSlots = aGemSlots;
        }

        internal CGemSlot[] GemSlots { get; private set; }
        internal CGemCategory GemClass;
        internal CGemSideEnum? GemSideEnum;

        internal CGemSlot FindFreeSlotNullable()
           =>this.GemSlots.Where(s => s.IsFree).FirstOrDefault();
    }

    internal sealed class CGemSideBar : CServiceLocatorNode
    {
        #region ctor
        internal CGemSideBar(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GemCategories = this.ServiceContainer.GetService<CGemCategories>();
        }
        public override void Load()
        {
            base.Load();
            this.GemClassBars.Load();
            foreach (var aGemClassBar in this.GemClassBars)
                aGemClassBar.Load();
        }
        #endregion
        #region GemSlotSideEnum
        internal CGemSideEnum GemSideEnum;
        #endregion
        #region GemClassBars
        private CGemClassBar[] GemClassBarsM;
        private CGemClassBar[] GemClassBars => CLazyLoad.Get(ref this.GemClassBarsM, this.NewGemClassBars);
        private CGemClassBar[] NewGemClassBars()
        {
            var aGemCategories = this.GemCategories.Array;
            var aCount = aGemCategories.Length;
            var aGemClassBars = new CGemClassBar[aCount];
            foreach (var aIdx in Enumerable.Range(0, aCount))
            {
                var aGemClassBar = new CGemClassBar(this)
                {
                    GemClass = aGemCategories[aIdx],
                    GemSideEnum = this.GemSideEnum
                };
                aGemClassBars[aIdx] = aGemClassBar;
            }
            return aGemClassBars;
        }
        internal CGemClassBar GetGemClassBar(CGemCategoryEnum aGemClassEnum)
            => this.GemClassBars[(int)aGemClassEnum];

        internal CGemSlot FindFreeSlotNullable(CGemCategoryEnum aGemClassEnum)
            => this.GetGemClassBar(aGemClassEnum).FindFreeSlotNullable();
        #endregion
        #region GemSlots
        internal IEnumerable<CGemSlot> GemSlots => (from aGemClassBar in this.GemClassBars
                                                    from aGemSlot in aGemClassBar.GemSlots
                                                    select aGemSlot);
        #endregion
        #region GemCategories
        private readonly CGemCategories GemCategories;
        #endregion
    }
    /*
internal sealed class CHorizontalSlots
{

    internal CGemSlotSideEnum GemAlternateSlotEnum;

    private CClassSlot[] ClassSlots;         
}

internal sealed class CActiveGemSlot : CGemSlot
{
}

internal sealed class CActiveGemsSlots : CGemSlot
{

    private CActiveGemSlot[] Slots;
}

internal abstract class CTmpActivatorSlot
{
}*/

    public sealed class CGemSlotModel : CModel
    {
        internal CGemSlotModel(CServiceLocatorNode aParent) : base(aParent)
        {
            var aTopLeft = new CVector3Dbl(0, 0, 0);
            var aWidth = 1d;
            var aHeight = 1d;
            this.TopLeft = aTopLeft;
            this.Width = aWidth;
            this.Height = aHeight;
            this.TopRight = new CVector3Dbl(aTopLeft.x + aWidth, aTopLeft.y, aTopLeft.z);
            this.BottomLeft = new CVector3Dbl(aTopLeft.x, aTopLeft.y + aHeight, aTopLeft.z);
            this.BottomRight = new CVector3Dbl(aTopLeft.x + aWidth, aTopLeft.y + aHeight, aTopLeft.z);
            this.TriangleList = default;
            this.LineList = default;
            this.TriangleList = new CVector3Dbl[]
            {
                this.BottomLeft,
                this.TopLeft,
                this.TopRight,
                this.BottomLeft,
                this.TopRight,
                this.BottomRight,
            };
            this.LineList = new CVector3Dbl[]
            {
                this.TopLeft,
                this.TopRight,
                this.TopLeft,
                this.BottomLeft,
                this.TopRight,
                this.BottomRight,
                this.BottomLeft,
                this.BottomRight,
            };
        }

        public CVector3Dbl TopLeft { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public CVector3Dbl TopRight { get; private set; }
        public CVector3Dbl BottomLeft { get; private set; }
        public CVector3Dbl BottomRight { get; private set; }
        public CVector3Dbl[] TriangleList { get; private set; }
        public CVector3Dbl[] LineList { get; private set; }
        public CVector3Dbl Translate { get; private set; }
    }

    public struct CGemSlotDrawInfo
    {
        internal CGemSlotDrawInfo(CGemSlot aGemSlot, CGemSlotModel aGemSlotModel)
        {
            this.GemSlot = aGemSlot;
            this.GemSlotModel = aGemSlotModel;

            var aIndex = aGemSlot.Index.Value;
            var aSide = aGemSlot.GemSlotSideEnum.Value;
            var aGemClassEnum = aGemSlot.GemClass.GemClassEnum;

            var aMarginX = 0.01;
            var aScreenWidth = 2f;
            var aScreenHeight = 2f;
            var aGemClassIdx = (int)aGemClassEnum;
            var aDx = 0.1f;
            var aTranslateX = aSide == CGemSideEnum.Left
                            ? -aScreenWidth / 2f + aMarginX
                            : aSide == CGemSideEnum.Right
                            ? aScreenWidth / 2f - aDx - aMarginX
                            : throw new NotImplementedException()
                            ;
            var aSlotCount1 = CConsts.GemClass_SlotCount + 1;
            var aSlotCount2 = aSlotCount1 * CConsts.GemClass_Count;
            var aSlotDy = aScreenHeight / aSlotCount2;
            var aScreenSlotIndex = aGemClassIdx * aSlotCount1
                           + aIndex;
            var aTranslateY = -1d + aSlotDy * aScreenSlotIndex + aSlotDy / 2f;
            var aTranslateZ = 0.1d;
            var aTranslate = new CVector3Dbl(aTranslateX, aTranslateY, aTranslateZ);
            var sm = Matrix.CreateScale(aDx, aSlotDy, 1f);
            var tm = Matrix.CreateTranslation(aTranslate.ToVector3());
            this.Matrix =  sm * tm;
        }


        internal readonly CGemSlot GemSlot;
        internal readonly CGemSlotModel GemSlotModel;

        public bool TriangleListIsDefined => !this.GemSlot.IsFree;
        public CVector3Dbl Color => this.GemSlot.GemClass.Color;

        public readonly Matrix Matrix;
    }


    public sealed class CGemControlsModel : CModel
    {
        internal CGemControlsModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GemSlotModel = new CGemSlotModel(this);
        }

        public readonly CGemSlotModel GemSlotModel;

        public IEnumerable<CGemSlotDrawInfo> GetDrawInfo(CGemSlotControlsSprite aControlsSprite)
            => aControlsSprite.GemSlots.Select(aGemSlot=>new CGemSlotDrawInfo(aGemSlot, this.GemSlotModel));
    }
    public sealed class CGemSlotControlsSprite : CSprite
    {
        #region ctor
        internal CGemSlotControlsSprite(CServiceLocatorNode aParent) :base(aParent)
        {
            this.PlattformSpriteEnum = Mvi.Facade.CPlatformSpriteEnum.GemSlotControls;
            this.HasDistanceToAvatar = false;
            this.RandomGenerator = new CRandomGenerator(this);
            this.RandomGenerator.Begin();

            this.Init();
        }
        public override void Load()
        {
            base.Load();

            foreach(var aGemSideBar in this.GemSideBars)
            {
                aGemSideBar.Load();
            }

            this.GemSlots = (from aSideBar in this.GemSideBars
                             from aSlot in aSideBar.GemSlots
                             select aSlot).ToArray();
        }
        protected override void OnBeginUse()
        {
            base.OnBeginUse();
            this.BuildIsDone = true;

            this.Load();
        }
        #endregion
        #region GemSideBars
        private CGemSideBar[] GemSideBarsM;
        private CGemSideBar[] GemSideBars => CLazyLoad.Get(ref this.GemSideBarsM, this.NewGemSideBars);
        private CGemSideBar[] NewGemSideBars()
        {
            var aGemSideEnums = typeof(CGemSideEnum).GetEnumValues().Cast<CGemSideEnum>().ToArray();
            var aCount = aGemSideEnums.Length;
            var aGemSideBars = new CGemSideBar[aCount];
            foreach (var aIdx in Enumerable.Range(0, aCount))
            {
                var aGemSideBar = new CGemSideBar(this)
                {
                    GemSideEnum = aGemSideEnums[aIdx],
                };
                aGemSideBars[aIdx] = aGemSideBar;
            }
            return aGemSideBars;
        }
        private CGemSideBar GetGemSideBar(CGemSideEnum aSide)
            => this.GemSideBars[(int)aSide];
        #endregion
        internal readonly CRandomGenerator RandomGenerator;
        #region GemSlots
        internal CGemSlot FindFreeSlotNullable(CGemCategoryEnum aGemClassEnum)
        {
            var aSideBars = this.RandomGenerator.NextShuffledSequence(this.GemSideBars);
            foreach(var aIdx in Enumerable.Range(0, aSideBars.Length))
            {
                var aGemSlotNullable = aSideBars[aIdx].FindFreeSlotNullable(aGemClassEnum);
                if (aGemSlotNullable is object)
                    return aGemSlotNullable;
            }
            return default;
        }
        internal CGemSlot[] GemSlots { get; private set; }
        #endregion
        internal override void Draw()
        {
            base.Draw();
        }
    }

    internal sealed class CGemSlotControlsSpriteManager : CSinglePoolSpriteManager<CGemSlotControlsSprite>
    {
        #region ctor
        internal CGemSlotControlsSpriteManager(CServiceLocatorNode aParent): base(aParent)
        {
            this.AddOnAllocate = true;
            this.World.GemCollected += delegate (CGemSprite aGemSprite)
            {
                var aFreeSlot = this.ControlsSprite.FindFreeSlotNullable(aGemSprite.GetCategoryEnum);
                if(aFreeSlot is object)
                {
                    aFreeSlot.GemSpriteNullable = aGemSprite;
                }
            };
            this.Init();
        }
        protected override void Init()
        {
            base.Init();
            this.Reserve(1, true);
        }
        #endregion
        #region ControlsSprite
        protected override CGemSlotControlsSprite NewSprite()
            => new CGemSlotControlsSprite(this);
        internal override void InitialAllocate()
        {
            base.InitialAllocate();
            this.ControlsSprite = this.AllocateSpriteNullable();
        }
        internal CGemSlotControlsSprite ControlsSprite;
        #endregion
    }


}
