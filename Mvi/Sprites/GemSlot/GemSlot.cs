using CharlyBeck.Mvi.Sprites.Gem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Story.Bonus.Slot
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

    enum CGemAlternateSlotEnum
    {
        Left,
        Right,
    }

    internal abstract class CGemSlot
    {

        internal void Draw() { }
    }

    internal sealed class CClassSlot : CGemSlot
    {
        internal CGemClassEnum GemClassEnum;
    }

    internal sealed class CVerticalSlots : CGemSlot
    {

        internal CGemClassEnum GemClassEnum;

        private CClassSlot[] ClassSlots;
    }

    internal sealed class CHorizontalSlots
    {

        internal CGemAlternateSlotEnum GemAlternateSlotEnum;

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
    }
    internal sealed class CGemSlotManager
    {
        internal void AddGem(CGemSprite aGem) => throw new NotImplementedException();

        private IEnumerable<CGemSlot> GemSlots => throw new NotImplementedException();

        private CHorizontalSlots[] HorizontalSlots;
        private CActiveGemsSlots ActiveGemsSlots;


    }


}
