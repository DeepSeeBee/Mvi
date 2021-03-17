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
using CharlyBeck.Mvi.Input;

namespace CharlyBeck.Mvi.Sprites.GemSlot
{
    enum CGemSideEnum
    {
        Left,
        Right,
    }

    internal static class CConsts
    {
        internal const int GemClass_SlotCount = 7;
        internal const int GemActiveBar_SlotCount = 15;

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
        internal CGemSideEnum? GemSideEnum;

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

    internal abstract class CGemSlots : CServiceLocatorNode
    {
        internal CGemSlots(CServiceLocatorNode aParent) : base(aParent)
        {

        }

        internal void Allocate(int aCount, Action<CGemSlot> aSetup)
        {
            var aItems = new CGemSlot[aCount];
            for(var i = 0; i < aCount; ++i)
            {
                var aGemSlot = new CGemSlot(this);
                aGemSlot.Index = i;
                aSetup(aGemSlot);
                aItems[i] = aGemSlot;
            }
            this.Items = aItems;
        }
        internal int Count => this.Items.Length;
        internal CGemSlot[] Items { get; private set; }
        internal CGemSlot FindFreeSlotNullable()
            => this.Items.Where(s => s.IsFree).FirstOrDefault();        
        internal CGemSprite RemoveAt(int i)
        {
            var aGemSpriteNullable = default(CGemSprite);
            bool aDone = false;
            for (var aIdx = i; aIdx < this.Items.Length - 1 && !aDone; ++aIdx)
            {
                var aSrcSlot = this.Items[aIdx + 1];
                var aDstSlot = this.Items[aIdx];
                var aDstGem = aDstSlot.GemSpriteNullable;
                //if (aDstGem is object)
                {
                    if (aIdx == i)
                    {
                        aGemSpriteNullable = aDstGem;
                    }
                    var aSrcGem = aSrcSlot.GemSpriteNullable;
                    aSrcSlot.GemSpriteNullable = default;
                    aDstSlot.GemSpriteNullable = aSrcGem;
                    if (!(aSrcGem is object))
                    {
                        aDone = true;
                    }
                }
                //else
                //{
                //    aDone = true;
                //}
            }
            return aGemSpriteNullable;
        }
        internal int GetCount()
            => this.Count.Range().Where(i => this.Items[i].IsFree).First();
    }

    internal sealed class CGemClassBarSlots : CGemSlots
    {
        internal CGemClassBarSlots(CServiceLocatorNode aParent) : base(aParent)
        {
        }
    }

    internal sealed class CGemClassBar : CServiceLocatorNode
    {
        internal CGemClassBar(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GemClassBarSlots = new CGemClassBarSlots(this);
            this.JoystickState = this.ServiceContainer.GetService<CJoystickState>();

            this.Init();
        }
        public override void Load()
        {
            base.Load();

            this.GemClassBarSlots.Allocate(CConsts.GemClass_SlotCount,
                delegate (CGemSlot aGemSlot)
                {
                    aGemSlot.GemClass = this.GemClass;
                    aGemSlot.GemSideEnum = this.GemSideEnum;
                });

            { // GetJoystickButton
                CJoystickButtonEnum? aJoystickButtonEnum;
                switch (this.GemSideEnum)
                {
                    case CGemSideEnum.Left:
                        switch (this.GemClass.GemClassEnum)
                        {
                            case CGemCategoryEnum.Defense:
                                aJoystickButtonEnum = CJoystickButtonEnum.SideMidLeft;
                                break;
                            case CGemCategoryEnum.Offense:
                                aJoystickButtonEnum = CJoystickButtonEnum.SideBotLeft;
                                break;
                            case CGemCategoryEnum.Navigation:
                                aJoystickButtonEnum = CJoystickButtonEnum.SideTopLeft;
                                break;
                            default:
                                aJoystickButtonEnum = default;
                                break;
                        }
                        break;

                    case CGemSideEnum.Right:
                        switch (this.GemClass.GemClassEnum)
                        {
                            case CGemCategoryEnum.Defense:
                                aJoystickButtonEnum = CJoystickButtonEnum.SideMidRight;
                                break;
                            case CGemCategoryEnum.Offense:
                                aJoystickButtonEnum = CJoystickButtonEnum.SideBotRight;
                                break;
                            case CGemCategoryEnum.Navigation:
                                aJoystickButtonEnum = CJoystickButtonEnum.SideTopRight;
                                break;
                            default:
                                aJoystickButtonEnum = default;
                                break;
                        }
                        break;
                    default:
                        aJoystickButtonEnum = default;
                        break;
                }
                this.JoystickButtonEnum = aJoystickButtonEnum;
            }
        }

        internal void Update(CFrameInfo aFrameInfo)
        {
            var aButtonPressed = this.JoystickState[this.JoystickButtonEnum.Value];
            if(!aButtonPressed)
            {
                this.JoystickButtonPressed = false;
            }
            else if(!this.JoystickButtonPressed)
            {
                var aGemSpriteNullable = this.GemClassBarSlots.RemoveAt(0); 
                if (aGemSpriteNullable is object)
                {
                    aGemSpriteNullable.Activate();
                }
                this.JoystickButtonPressed = true;
            }
        }

        internal CGemClassBarSlots GemClassBarSlots { get; private set; }
        internal CGemCategory GemClass;
        internal CGemSideEnum? GemSideEnum;
        internal CJoystickButtonEnum? JoystickButtonEnum;
        private bool JoystickButtonPressed;

        private readonly CJoystickState JoystickState;
        internal CGemSlot FindFreeSlotNullable()
            => this.GemClassBarSlots.FindFreeSlotNullable();

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
                                                    from aGemSlot in aGemClassBar.GemClassBarSlots.Items
                                                    select aGemSlot);
        #endregion
        #region GemCategories
        private readonly CGemCategories GemCategories;
        #endregion
        #region Update
        internal void Update(CFrameInfo aFrameInfo)
        {
            var a = this.GemClassBars;
            var c = a.Length;
            for(var i = 0; i < c; ++i)
            {
                a[i].Update(aFrameInfo);
            }
        }
        #endregion
    }

    internal sealed class CGemActiveBarSlots : CGemSlots
    {
        internal CGemActiveBarSlots(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        internal void Update(CFrameInfo aFrameInfo)
        {
            var aCount = this.Count;
            var aItems = this.Items;
            var i = 0;
            while(i < aCount)
            {
                var aItem = aItems[i];
                var aGem = aItem.GemSpriteNullable;
                if(aGem is object
                && !aGem.GemIsActive)
                {
                    this.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }
    }
    internal sealed class CGemActiveBar : CServiceLocatorNode
    {
        internal CGemActiveBar(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GemActiveBarSlots = new CGemActiveBarSlots(this);
        }
        public override void Load()
        {
            base.Load();

            this.GemActiveBarSlots.Allocate(CConsts.GemActiveBar_SlotCount,
                delegate(CGemSlot aGemSlot)
                {
                    aGemSlot.GemSideEnum = this.GemSideEnum;
                });
        }
        internal CGemSideEnum GemSideEnum;
        internal CGemActiveBarSlots GemActiveBarSlots; 

        internal void Update(CFrameInfo aFrameInfo)
        {
            this.GemActiveBarSlots.Update(aFrameInfo);
        }
    }

    internal sealed class CGemActiveBars : CServiceLocatorNode
    {
        internal CGemActiveBars(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        public override void Load()
        {
            base.Load();

            var aCount = typeof(CGemSideEnum).GetEnumMaxValue() + 1;
            var aGemActiveBars = new CGemActiveBar[aCount];
            for(var i = 0; i < aCount; ++i)
            {
                var aGemSideEnum = (CGemSideEnum)i;
                var aGemActiveBar = new CGemActiveBar(this)
                {
                    GemSideEnum = aGemSideEnum
                };
                aGemActiveBars[i] = aGemActiveBar;
            }
            this.Items = aGemActiveBars;
            this.Counts = new int[aCount];

            foreach (var aGemActiveBar in this.Items)
                aGemActiveBar.Load();
        }

        internal CGemActiveBar[] Items { get; private set; }
        private int[] Counts;

        public CGemSlot FindFreeSlot()
        {
            var minl = default(int?);
            var mini = default(int?);
            var c = this.Items.Length;
            for (var i = 0; i < c; ++i)
            {
                var l = this.Items[i].GemActiveBarSlots.GetCount();
                if (!minl.HasValue
                || minl.Value > l)
                {
                    mini = i;
                    minl = l;
                }
            }
            if(mini.HasValue)
            {
                return this.Items[mini.Value].GemActiveBarSlots.FindFreeSlotNullable();
            }
            return default;
        }

        internal void Update(CFrameInfo aFrameInfo)
        {
            var aItems = this.Items;
            var c = aItems.Length;
            for (var i = 0; i < c; ++i)
            {
                aItems[i].Update(aFrameInfo);
            }
        }
    }

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

    public struct CTextRect
    {
        public CTextRect(double aX, double aY, double aDx, double aDy)
        {
            this.X = aX;
            this.Y = aY;
            this.Dx = aDx;
            this.Dy = aDy;
        }
        public readonly double X;
        public readonly double Y;
        public readonly double Dx;
        public readonly double Dy;

    }

    public struct CGemSlotDrawInfo
    {
        internal CGemSlotDrawInfo(CGemSlot aGemSlot, CGemSlotModel aGemSlotModel)
        {
            this.GemSlot = aGemSlot;
            this.GemSlotModel = aGemSlotModel;

            var aIndex = aGemSlot.Index.Value;
            var aSide = aGemSlot.GemSideEnum.Value;
            var aGemClassEnum = aGemSlot.GemClass.GemClassEnum;

            var aMarginX = 0.01;
            var aScreenWidth = 2f;
            var aScreenHeight = 2f;
            var aGemClassIdx = (int)aGemClassEnum;
            var aDx = 0.15f;
            var aTranslateX = aSide == CGemSideEnum.Left
                            ? -aScreenWidth / 2f + aMarginX
                            : aSide == CGemSideEnum.Right
                            ? aScreenWidth / 2f - aDx - aMarginX
                            : throw new NotImplementedException()
                            ;
            var aSlotCount1 = CConsts.GemClass_SlotCount + 1;
            var aSlotCount2 = aSlotCount1 * CConsts.GemClass_Count;
            var aDy = aScreenHeight / aSlotCount2;
            var aScreenSlotIndex = aGemClassIdx * aSlotCount1
                           + aIndex;
            var aTranslateY = -1d + aDy * aScreenSlotIndex + aDy / 2f;
            var aTranslateZ = 0d;
            var aTranslate = new CVector3Dbl(aTranslateX, aTranslateY, aTranslateZ);
            var sm = Matrix.CreateScale(aDx, aDy, 1f);
            var tm = Matrix.CreateTranslation(aTranslate.ToVector3());
            this.Matrix =  sm * tm;
            var aGemSpriteNullable = aGemSlot.GemSpriteNullable;
            var aTextScale = Math.Min(aDx, aDy);
            this.Text = aGemSpriteNullable is object ? aGemSpriteNullable.ShortName : string.Empty;
            this.TextRect = new CTextRect((float)aTranslateX , (float)aTranslateY , aDx, aDy);
        }


        internal readonly CGemSlot GemSlot;
        internal readonly CGemSlotModel GemSlotModel;

        public bool TriangleListIsDefined => !this.GemSlot.IsFree;
        public CVector3Dbl Color => this.GemSlot.GemClass.Color;

        public readonly Matrix Matrix;

        public readonly string Text;
        public readonly CTextRect TextRect;
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
            this.GemActiveBars = new CGemActiveBars(this);

            this.Init();
        }
        public override void Load()
        {
            base.Load();

            foreach(var aGemSideBar in this.GemSideBars)
            {
                aGemSideBar.Load();
            }

            this.GemActiveBars.Load();

            foreach(var aGemActiveBar in this.GemActiveBars.Items)
            {
                aGemActiveBar.Load();
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
        internal CGemSlot FindFreeSideBarSlotNullable(CGemCategoryEnum aGemClassEnum)
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
        #region ActiveSlots
        internal readonly CGemActiveBars GemActiveBars;
        internal CGemSlot FindFreeActiveBarSlotNullable()
            => this.GemActiveBars.FindFreeSlot();
        #endregion
        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);

            { // TODO_OPT
                var a = this.GemSideBars;
                var l = a.Length;
                for(var i = 0; i < l; ++i)
                {
                    a[i].Update(aFrameInfo);
                }
            }
            this.GemActiveBars.Update(aFrameInfo); // TODO_OPT
        }

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
                var aFreeSlot = this.ControlsSprite.FindFreeSideBarSlotNullable(aGemSprite.GemCategoryEnum);
                if(aFreeSlot is object)
                {
                    aFreeSlot.GemSpriteNullable = aGemSprite;
                }
            };
            this.World.GemActivated += delegate (CGemSprite aGemSprite)
            {
                var aFreeSlot = this.ControlsSprite.GemActiveBars.FindFreeSlot();
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
        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);
        }
    }


}
