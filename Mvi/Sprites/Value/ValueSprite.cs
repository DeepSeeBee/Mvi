using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Sprites.Avatar;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Faktor01;
using CharlyBeck.Mvi.World;
using Mvi.Models;
using CharlyBeck.Utils3.Asap;

namespace CharlyBeck.Mvi.Sprites.Value
{
    internal enum CValueSpriteEnum
    {
        SecondaryWeapon
    }

    internal sealed class CValueSpriteModel : CModel
    {
        internal CValueSpriteModel(CServiceLocatorNode aParent) :base(aParent)
        {
        }

        internal string GetText(CValueSprite aValueSprite)
            => aValueSprite.ValueAsValue.ValueDeclaration.Abbreviation;
    }

    internal abstract class CValueSprite : CSprite
    {
        internal CValueSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Value;
            this.Scale = 0.1d;
        }

        internal int Index;
        internal int Count;

        internal CVector3Dbl GetWorldPos()
        {
            var aValueSprite = this;
            var aIndex = aValueSprite.Index;
            var aCount = aValueSprite.Count;
            var aDy = this.Scale.Value;
            var aX = ((double)aIndex).F01_Map(0, aCount - 1, -0.8d, 0.8d);
            var aY = 1d - aDy;
            var aWorldPos = new CVector3Dbl(aX, aY, 0d);            
            return aWorldPos;
        }
        internal abstract CValue ValueAsValue { get; }
    }

    internal abstract class CValueSprite<TValue> : CValueSprite where TValue : CValue
    {
        internal CValueSprite(CServiceLocatorNode aParent):base(aParent)
        {
        }

        internal TValue Value;
        internal override CValue ValueAsValue => this.Value;

    }

    internal sealed class CSecondaryWeaponValueSprite : CValueSprite<CInt64Value>
    {
        internal CSecondaryWeaponValueSprite(CServiceLocatorNode aParent) : base(aParent)
        {
        }
    }

    internal sealed class CValueSpriteManager : CMultiPoolSpriteManager<CValueSprite, CValueSpriteEnum>
    {
        #region ctor
        internal CValueSpriteManager(CServiceLocatorNode aParent) : base(aParent)
        {
            this.SeconaryWeaponValueEnums = typeof(CValueEnum).GetEnumValues().Cast<CValueEnum>()
                .Where(e => e.GetCustomAttributeIsDefined<CSecondaryWeaponValueAttribute>())
                .Where(e => e.GetCustomAttribute<CSecondaryWeaponValueAttribute>().IsSeconaryWeaponValue)
                .ToArray()
                ;
            this.Init();
        }
        protected override void Init()
        {
            base.Init();
            this.Reserve();
        }

        private void Reserve()
        {
            var aLock = true;
            this.Reserve(CValueSpriteEnum.SecondaryWeapon, this.SeconaryWeaponValueEnums.Length, aLock);
        }
        internal override void InitialAllocate()
        {
            base.InitialAllocate();
            this.SecondaryWeaponValueSprites = this.SeconaryWeaponValueEnums.Length.Range().Select(
                i => this.CreateSecondaryWeaponValueSprite(i, this.SeconaryWeaponValueEnums.Length, this.SeconaryWeaponValueEnums[i])
                ).ToArray();
        }
        private CSecondaryWeaponValueSprite CreateSecondaryWeaponValueSprite(int aIndex, int aCount, CValueEnum aValueEnum)
        {            
            var aAvatarValues = this.AvatarValues;
            var aValue = (CInt64Value)aAvatarValues.GetValue(aValueEnum);
            var aValueSprite = (CSecondaryWeaponValueSprite)this.AllocateSpriteNullable(CValueSpriteEnum.SecondaryWeapon);
            aValueSprite.Value = aValue;
            aValueSprite.Index = aIndex;
            aValueSprite.Count = aCount;
            aValueSprite.WorldPos = aValueSprite.GetWorldPos();
            aValueSprite.Reposition();
            return aValueSprite;
        }
        internal override CNewFunc GetNewFunc(CValueSpriteEnum aClassEnum)
        {
            switch(aClassEnum)
            {
                case CValueSpriteEnum.SecondaryWeapon: return new CNewFunc(() => new CSecondaryWeaponValueSprite(this));
                default: throw new ArgumentException();
            }
        }
        #endregion
        private readonly CValueEnum[] SeconaryWeaponValueEnums;
        private CSecondaryWeaponValueSprite[] SecondaryWeaponValueSprites;
        #region Avatar
        private CAvatarSprite AvatarSpriteM;
        private CAvatarSprite AvatarSprite => CLazyLoad.Get(ref this.AvatarSpriteM, () => this.ServiceContainer.GetService<CAvatarSprite>());
        private CAvatarValues AvatarValues => this.AvatarSprite.AvatarValues;
        #endregion
    }

}
