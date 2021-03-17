using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Input
{
    public enum CJoystickButtonEnum
    {
        Fire1,
        Fire2,

        TipTopLeft,
        TipTopRight,
        TipBotLeft,
        TipBotRight,

        SideTopLeft,
        SideTopRight,
        SideMidLeft,
        SideMidRight,
        SideBotLeft,
        SideBotRight,
    }

    public enum CJoystickAxisEnum
    {
        X,
        Y,
        Z,
        Throttle
    }

    public interface IJoystick
    {
        void Update(CJoystickState aJoystickState);
    }

    public sealed class CJoystickState
    {
        internal CJoystickState()
        {
            this.ButtonStates = new bool[typeof(CJoystickButtonEnum).GetEnumMaxValue() + 1];
            this.AxisStates = new double[typeof(CJoystickAxisEnum).GetEnumMaxValue() + 1];
        }
        internal readonly bool[] ButtonStates;
        internal readonly double[] AxisStates;
        public bool this[CJoystickButtonEnum aButtonEnum]
        {
            get => this.ButtonStates[(int)aButtonEnum];
            set => this.ButtonStates[(int)aButtonEnum] = value;
        }
        public double this[CJoystickAxisEnum aAxisEnum]
        {
            get => this.AxisStates[(int)aAxisEnum];
            set => this.AxisStates[(int)aAxisEnum] = value;
        }
    }

    internal sealed class CJoystick : CServiceLocatorNode
    {
        internal CJoystick(CServiceLocatorNode aParent):base(aParent)
        {
            this.Jostick = this.ServiceContainer.GetService<IJoystick>();
            this.JoystickState = new CJoystickState();
        }
        private readonly IJoystick Jostick;
        internal readonly CJoystickState JoystickState;
        public void Update()
        {
            this.Jostick.Update(this.JoystickState);
        }
    }
}
