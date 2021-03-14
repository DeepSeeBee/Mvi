using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.Avatar
{
    internal sealed class CAvatarValues : CValueObject
    {
        #region ctor
        internal CAvatarValues(CServiceLocatorNode aParent):base(aParent)
        {
            this.AmmoEnergyValue = AmmoEnergyDecl.NewDoubleValue(this);
            this.AmmoFireRateValue = AmmoFireRateDecl.NewDoubleValue(this);
            this.AmmoSpeedValue = AmmoSpeedDecl.NewDoubleValue(this);
            this.AmmoThicknessValue = AmmoThicknessDecl.NewDoubleValue(this);
            this.AntiGravityValue = AntiGravityDecl.NewDoubleValue(this);
            this.DrillCountValue = DrillCountDecl.NewInt64Value(this);
            this.LifeCountValue = LifeCountDecl.NewInt64Value(this);
            this.GuidedMissileCountValue = GuidedMissileCountDecl.NewInt64Value(this);
            this.KruskalScannerCountValue = KruskalScannerCountDecl.NewInt64Value(this);
            this.NuclearMissileCountValue = NuclearMissileCountDecl.NewInt64Value(this);
            this.ShellValue = ShellDecl.NewDoubleValue(this);
            this.SlowMotionValue= SlowMotionDecl.NewDoubleValue(this);
            this.SpaceGripValue = SpaceGripDecl.NewDoubleValue(this);
            this.ThermalShieldValue = ThermalShieldDecl.NewBoolValue(this);

            this.Add(this.AmmoEnergyValue);
            this.Add(this.AmmoFireRateValue);
            this.Add(this.AmmoSpeedValue);
            this.Add(this.AmmoThicknessValue);

            // TODO: Noch nicht implementiert: this.Add(this.AntiGravityValue);
            // TODO: Noch nicht implementiert: this.Add(this.DrillCountValue);
            // TODO: Noch nicht implementiert: this.Add(this.LifeCountValue);
            // TODO: Noch nicht implementiert: this.Add(this.GuidedMissileCountValue);
            // TODO: Noch nicht implementiert: this.Add(this.KruskalScannerCountValue);
            // TODO: Noch nicht implementiert: this.Add(this.NuclearMissileCountValue);
            // TODO: Noch nicht implementiert: this.Add(this.ShellValue);
            // TODO: Noch nicht implementiert: this.Add(this.SlowMotionValue);
            // TODO: Noch nicht implementiert: this.Add(this.SpaceGripValue);
            // TODO: Noch nicht implementiert: this.Add(this.ThermalShieldValue);

            this.Init();
        }
        #endregion
        #region ValueDeclarations
        private const Int64 MaxItemCount = 10; // TODO
        private static readonly CDoubleDeclaration AmmoEnergyDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_AmmoEnergy,
                new Guid("82b3a59b-44c3-4a0b-abf5-bb57171b574c"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );
        private static readonly CDoubleDeclaration AmmoFireRateDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_AmmoFireRate,
                new Guid("103a26fc-12c8-4f04-a7a6-f3a16e420504"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );

        private static readonly CDoubleDeclaration AmmoSpeedDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_AmmoSpeed,
                new Guid("739ad609-fe56-434f-aed0-acc201c18d2c"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );

        private static readonly CDoubleDeclaration AmmoThicknessDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_AmmoThickness,
                new Guid("4380c7af-bbef-490d-acb0-366810cc6d74"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );

        private static readonly CDoubleDeclaration AntiGravityDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_AntiGravity,
                new Guid("a03d6d84-97b8-4256-9b3e-e6ff3db9a2ed"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );
        private static readonly CInt64Declaration DrillCountDecl = new CInt64Declaration
            (
                Value.CValueEnum.Object_Avatar_Drill,
                new Guid("dc1a2ccc-5f27-49af-b00c-8791b1c0a382"),
                true, // aIsPersistent
                CUnitEnum.Count, // UnitEnum
                0, // Default
                0, // Min
                MaxItemCount, // Max
                1, // SmallChange
                1 // LargeChange
            );
        private static readonly CInt64Declaration LifeCountDecl = new CInt64Declaration
            (
                Value.CValueEnum.Object_Avatar_LifeCount,
                new Guid("41515983-ad37-4bb7-8021-a70833d8d17d"),
                true, // aIsPersistent
                CUnitEnum.Count, // UnitEnum
                0, // Default
                0, // Min
                10, // Max
                1, // SmallChange
                1 // LargeChange
            );
        private static readonly CInt64Declaration GuidedMissileCountDecl = new CInt64Declaration
            (
                Value.CValueEnum.Object_Avatar_GuidedMissileCount,
                new Guid("41f01b21-d810-44fc-8de4-d4ed739114a2"),
                true, // aIsPersistent
                CUnitEnum.Count, // UnitEnum
                0, // Default
                0, // Min
                MaxItemCount, // Max
                1, // SmallChange
                1 // LargeChange
            );
        private static readonly CInt64Declaration KruskalScannerCountDecl = new CInt64Declaration
            (
                Value.CValueEnum.Object_Avatar_NuclearMissileCount,
                new Guid("65a21bbc-1895-4ee1-a4fb-fb3a3076a66a"),
                true, // aIsPersistent
                CUnitEnum.Count, // UnitEnum
                0, // Default
                0, // Min
                MaxItemCount, // Max
                1, // SmallChange
                1 // LargeChange
            );
        private static readonly CInt64Declaration NuclearMissileCountDecl = new CInt64Declaration
            (
                Value.CValueEnum.Object_Avatar_NuclearMissileCount,
                new Guid("0194fdf9-75f7-470d-97e7-17a4f46b38a5"),
                true, // aIsPersistent
                CUnitEnum.Count, // UnitEnum
                0, // Default
                0, // Min
                MaxItemCount, // Max
                1, // SmallChange
                1 // LargeChange
            );
        private static readonly CDoubleDeclaration ShellDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_Shell,
                new Guid("da244ae6-aa75-40a8-ada6-1b8da98b7666"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );
        private static readonly CDoubleDeclaration SlowMotionDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_SlowMotion,
                new Guid("1478d3a6-2d4e-4508-914b-cbbc33e61f10"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                1.0d, // Default
                0.1d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );
        private static readonly CDoubleDeclaration SpaceGripDecl = new CDoubleDeclaration
            (
                Value.CValueEnum.Object_Avatar_SpaceGrip,
                new Guid("3fa5b351-72c1-46e6-850f-b81c4f4e34ec"), // Guid
                true, // IsPersistent
                CGuiEnum.Slider, // GuiEnum
                CUnitEnum.Percent, // UnitEnum
                0d, // Default
                0d, // Min
                1d, // Max
                0.05d, // SmallChange
                0.1d, // LargeChange
                0 // Digits
            );
        private static readonly CBoolDeclaration ThermalShieldDecl = new CBoolDeclaration
            (
                Value.CValueEnum.Object_Avatar_ThermalShield,
                new Guid("b9f26f5c-bc3c-4eb0-914d-eff40647adbd"), // Guid
                true, // IsPersistent
                false // Default
            );
        #endregion
        #region Values
        internal readonly CDoubleValue AmmoEnergyValue;
        internal readonly CDoubleValue AmmoFireRateValue;
        internal readonly CDoubleValue AmmoSpeedValue;
        internal readonly CDoubleValue AmmoThicknessValue;
        internal readonly CDoubleValue AntiGravityValue;
        internal readonly CInt64Value DrillCountValue;
        internal readonly CInt64Value LifeCountValue;
        internal readonly CInt64Value GuidedMissileCountValue;
        internal readonly CInt64Value KruskalScannerCountValue;
        internal readonly CInt64Value NuclearMissileCountValue;
        internal readonly CDoubleValue ShellValue;
        internal readonly CDoubleValue SlowMotionValue;
        internal readonly CDoubleValue SpaceGripValue;
        internal readonly CBoolValue ThermalShieldValue;
        #endregion
    }

    public sealed class CAvatarSprite : CSprite
    {

        internal CAvatarSprite(CServiceLocatorNode aParent):base(aParent)
        {
            this.ValueObjectIsDefined = true;
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Avatar;
            this.SetCollisionIsEnabled(CCollisionSourceEnum.Gem, true);
            this.BuildIsDone = true;
            this.Radius = 0.01;

            this.AvatarValues = new CAvatarValues(this);
            this.Init();
        }

        #region AvatarValues
        internal readonly CAvatarValues AvatarValues;
        internal override CValueObject ValueObject => this.AvatarValues;
        #endregion
    }


    internal sealed class CAvatarManager : CSinglePoolSpriteManager<CAvatarSprite>
    {
        internal CAvatarManager(CServiceLocatorNode aParent) :base(aParent)
        {
            this.AddOnAllocate = true;
            this.Init();
        }

        protected override void Init()
        {
            base.Init();

            var aLock = true;
            this.Reserve(1, aLock);
        }

        internal override void InitialAllocate()
        {
            base.InitialAllocate();
            this.AvatarSprite = this.AllocateSpriteNullable();
        }
        protected override CAvatarSprite NewSprite()
            => new CAvatarSprite(this);

        internal CAvatarSprite AvatarSprite { get; private set; }

        internal CVector3Dbl AvatarPos { get => this.AvatarSprite.WorldPos.Value; set => this.AvatarSprite.WorldPos = value; }
    }
}
