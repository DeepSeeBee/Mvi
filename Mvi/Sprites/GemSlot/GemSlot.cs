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
using CharlyBeck.Mvi.Sprites.Gem.Internal;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Sprites.Avatar;
using CharlyBeck.Mvi.Value;

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
        #region ctor
        internal CGemSlot(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GemSlots = this.ServiceContainer.GetService<CGemSlots>();
            this.Init();
        }
        protected override void Init()
        {
            base.Init();
        }
        #endregion
        #region GemSlots
        private readonly CGemSlots GemSlots;
        #endregion
        internal void Draw() { }

        internal int? Index;
        internal CGemCategory GemClass;
        internal CGemSideEnum? GemSideEnum;

        private CCollectedGemSprite GemSpriteNullableM;
        internal CCollectedGemSprite GemSpriteNullable
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
                        this.GemSlots.OnDetachGem(aOldValue);
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
                        this.GemSlots.OnAttachGem(value);
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
        #region ctor
        internal CGemSlots(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        #endregion
        #region Items
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
        internal CCollectedGemSprite RemoveAt(int i)
        {
            var aGemSpriteNullable = default(CCollectedGemSprite);
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
        internal virtual void OnAttachGem(CCollectedGemSprite aGemSprite)
        {
        }
        internal virtual void OnDetachGem(CCollectedGemSprite aGemSprite)
        {
        }
        internal bool IsFull => !this.Items[this.Items.Length - 1].IsFree;
        #endregion

        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CGemSlots>(() => this);
            return aServiceContainer;
        }
        #endregion
    }

    internal sealed class CGemClassBarSlots : CGemSlots
    {
        internal CGemClassBarSlots(CServiceLocatorNode aParent) : base(aParent)
        {
        }

    }

    internal sealed class CGemCategoryBar : CServiceLocatorNode
    {
        internal CGemCategoryBar(CServiceLocatorNode aParent) : base(aParent)
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

        internal bool IsFull => this.GemClassBarSlots.IsFull;
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
        private CGemCategoryBar[] GemClassBarsM;
        private CGemCategoryBar[] GemClassBars => CLazyLoad.Get(ref this.GemClassBarsM, this.NewGemClassBars);
        private CGemCategoryBar[] NewGemClassBars()
        {
            var aGemCategories = this.GemCategories.Array;
            var aCount = aGemCategories.Length;
            var aGemClassBars = new CGemCategoryBar[aCount];
            foreach (var aIdx in Enumerable.Range(0, aCount))
            {
                var aGemClassBar = new CGemCategoryBar(this)
                {
                    GemClass = aGemCategories[aIdx],
                    GemSideEnum = this.GemSideEnum
                };
                aGemClassBars[aIdx] = aGemClassBar;
            }
            return aGemClassBars;
        }
        internal CGemCategoryBar GetGemCategoryBar(CGemCategoryEnum aGemClassEnum)
            => this.GemClassBars[(int)aGemClassEnum];
        internal CGemSlot FindFreeSlotNullable(CGemCategoryEnum aGemClassEnum)
            => this.GetGemCategoryBar(aGemClassEnum).FindFreeSlotNullable();
        #endregion
        #region GemSlots
        internal IEnumerable<CGemSlot> GemSlots => (from aGemClassBar in this.GemClassBars
                                                    from aGemSlot in aGemClassBar.GemClassBarSlots.Items
                                                    select aGemSlot);
        #endregion
        #region GemCategories
        private readonly CGemCategories GemCategories;
        internal bool GetCategoryIsFilled(CGemCategoryEnum aGemCategoryEnum)
            => this.GetGemCategoryBar(aGemCategoryEnum).IsFull;        
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
        #region ctor
        internal CGemActiveBarSlots(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        #endregion
        #region Update
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
        #endregion
        #region GemSprite
        internal override void OnAttachGem(CCollectedGemSprite aGemSprite)
        {
            base.OnAttachGem(aGemSprite);
            aGemSprite.ApplyModifiers();
        }
        internal override void OnDetachGem(CCollectedGemSprite aGemSprite)
        {
            base.OnDetachGem(aGemSprite);
            aGemSprite.UnapplyValues();
        }
        #endregion
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

            foreach(var aGemSideBar in this.GemSidebars)
            {
                aGemSideBar.Load();
            }

            this.GemActiveBars.Load();

            foreach(var aGemActiveBar in this.GemActiveBars.Items)
            {
                aGemActiveBar.Load();
            }

            this.GemSlots = (from aSideBar in this.GemSidebars
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
        private CGemSideBar[] GemSidebars => CLazyLoad.Get(ref this.GemSideBarsM, this.NewGemSideBars);
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
            => this.GemSidebars[(int)aSide];
        private IEnumerable<CGemCategoryBar> GetGemCategoryBars(CGemCategoryEnum aGemCategory)
            => this.GemSidebars.Select(sb => sb.GetGemCategoryBar(aGemCategory));
        #endregion
        internal readonly CRandomGenerator RandomGenerator;
        #region GemSlots
        internal CGemSlot FindFreeSideBarSlotNullable(CGemCategoryEnum aGemClassEnum)
        {
            var aSideBars = this.RandomGenerator.NextShuffledSequence(this.GemSidebars);
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
                var a = this.GemSidebars;
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
        private readonly CGemEnum[] GemEnums = typeof(CGemEnum).GetEnumValues().Cast<CGemEnum>().ToArray();
        private readonly CGemCategoryEnum[] GemCategoryEnums = typeof(CGemCategoryEnum).GetEnumValues().Cast<CGemCategoryEnum>().ToArray();
        internal bool GetCategoryIsFilled(CGemCategoryEnum aCategory)
            => !this.GemSidebars.Select(sb => sb.GetCategoryIsFilled(aCategory)).Contains(false);

        internal CGemCategoryEnum? GetUnfilledCategory()
            => this.GemCategoryEnums.Select(c=> this.GetCategoryIsFilled(c) ?  default(CGemCategoryEnum?) : c).Where(c=>c.HasValue).FirstOrDefault();
        IEnumerable<CGemCategoryEnum> GetLowerCategories(CGemCategoryEnum aGemCategoryEnum)
            => this.GemCategoryEnums.Where(c => ((int)c) < ((int)aGemCategoryEnum));

        internal IEnumerable<CGemEnum> GetCollectableGemEnums()
        {
            var aGemEnums = this.GemEnums;
            foreach(var aGemEnum in this.GemEnums)
            {
                var aGemCategory = aGemEnum.GetCustomAttributeIsDefined<CGemCategoryEnumAttribute>()
                                  ? aGemEnum.GetCustomAttribute<CGemCategoryEnumAttribute>().GemCategoryEnum
                                  : default(CGemCategoryEnum?)
                                  ;
                var aCollectIfValueNotFullAttribute = aGemEnum.GetCustomAttributeIsDefined<CGemCollectIfValueNotFullAttribute>()
                                                    ? aGemEnum.GetCustomAttribute<CGemCollectIfValueNotFullAttribute>()
                                                    : default
                                                    ;
                var aCollectIfValueNotFullValueEnum = aCollectIfValueNotFullAttribute is object
                                             ? aCollectIfValueNotFullAttribute.ValueEnum
                                             : default(CValueEnum?)
                                             ;
                var aCollectIfValueNotFullValue = aCollectIfValueNotFullValueEnum.HasValue
                                                 ? this.AvatarValues.GetValue(aCollectIfValueNotFullValueEnum.Value)
                                                 : default
                                                 ;
                var aCollectIfValueNotFullIsFull = aCollectIfValueNotFullValue is object
                                              ? aCollectIfValueNotFullValue.ValueIsMaximum
                                              : default(bool?)
                                              ;
                var aLowerCategories = aGemCategory.HasValue
                                     ? this.GetLowerCategories(aGemCategory.Value).ToArray()
                                     : Array.Empty<CGemCategoryEnum>();
                var aLowerCategoryIsFilled = aLowerCategories.IsEmpty()
                                          ? true
                                          : !aLowerCategories.Select(c => this.GetCategoryIsFilled(c)).Contains(false);
                bool aRecognizeGem;
                if(!aGemCategory.HasValue)
                {
                    aRecognizeGem = false;
                }
                else if(aLowerCategoryIsFilled)
                {
                    aRecognizeGem = true;
                }
                else if(aCollectIfValueNotFullIsFull == false 
                     && aLowerCategoryIsFilled == true)
                {
                    aRecognizeGem = true;
                }
                else if(aCollectIfValueNotFullIsFull == false
                    && aCollectIfValueNotFullAttribute is object 
                    && aCollectIfValueNotFullAttribute.LowerCategoryNeedsToBeFull == false)
                {
                    aRecognizeGem = true;
                }
                else
                {
                    aRecognizeGem = false;
                }
                if(aRecognizeGem)
                {
                    yield return aGemEnum;
                }
            }
        }
        #region Avatar
        private CAvatarSprite AvatarSpriteM;
        private CAvatarSprite AvatarSprite => CLazyLoad.Get(ref this.AvatarSpriteM, () => this.ServiceContainer.GetService<CAvatarSprite>());
        private CAvatarValues AvatarValues => this.AvatarSprite.AvatarValues;
        #endregion
    }

    internal sealed class CGemSlotControlsSpriteManager : CSinglePoolSpriteManager<CGemSlotControlsSprite>
    {
        #region ctor
        internal CGemSlotControlsSpriteManager(CServiceLocatorNode aParent): base(aParent)
        {
            this.AddOnAllocate = true;
            this.World.GemCollected += delegate (CCollectedGemSprite aGemSprite)
            {
                if (!aGemSprite.ActivateOnCollect)
                {
                    var aFreeSlot = this.GemSlotControlsSprite.FindFreeSideBarSlotNullable(aGemSprite.GemCategoryEnum);
                    if (aFreeSlot is object)
                    {
                        aFreeSlot.GemSpriteNullable = aGemSprite;
                    }
                    else if(aGemSprite.ActivateOnCollectIfNoSlot)
                    {
                        aGemSprite.Activate();
                    }
                    else
                    {
                        // TODO_SND: Zonk sound
                    }
                }
            };
            this.World.GemActivated += delegate (CCollectedGemSprite aGemSprite)
            {
                if(aGemSprite.ActiveDurationIsEnabled)
                {
                    var aFreeSlot = this.GemSlotControlsSprite.GemActiveBars.FindFreeSlot();
                    if (aFreeSlot is object)
                    {
                        aFreeSlot.GemSpriteNullable = aGemSprite;
                    }
                }
                else
                {
                    aGemSprite.ApplyModifiers();
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
            this.GemSlotControlsSprite = this.AllocateSpriteNullable();
        }
        internal CGemSlotControlsSprite GemSlotControlsSprite { get; private set; }
        #endregion
        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);
        }
    }


}
