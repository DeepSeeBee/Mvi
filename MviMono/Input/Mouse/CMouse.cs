﻿using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMousePos = System.Tuple<double, double>;

namespace CharlyBeck.Mvi.Mono.Input.Mouse.Internal
{
    public static class CExtensions
    {
        public static CMousePos Subtract(this CMousePos lhs, CMousePos rhs)
            => new CMousePos(lhs.Item1 - rhs.Item1, lhs.Item2 - rhs.Item2);
    }
}

namespace CharlyBeck.Mvi.Mono.Input.Mouse
{
    using CharlyBeck.Mvi.Value;
    using CharlyBeck.Mvi.Mono.Input.Mouse.Internal;
    using CharlyBeck.Utils3.Exceptions;
    using CharlyBeck.Utils3.LazyLoad;

    internal sealed class CMouse : CServiceLocatorNode
    {
        internal CMouse(CServiceLocatorNode aParent) :base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        internal CMousePos Position
        {
            get => new CMousePos(Cursor.Position.X, Cursor.Position.Y);
            set => Cursor.Position = new System.Drawing.Point((int)value.Item1, (int)value.Item2);
        }

        internal CMousePos PositionMax
        {
            get
            {
                var r = Screen.PrimaryScreen.Bounds;
                var m = new CMousePos(r.Right - r.Left, r.Bottom - r.Top);
                return m;
            }
        }

        private CMousePos LastPosition;
        internal CMousePos Offset
        {
            get => !this.WinFormMouseEnabledValue.Value
                ? new CMousePos(0d, 0d)
                : this.LastPosition is object
                ? this.Position.Subtract(this.LastPosition)
                : new CMousePos(0d, 0d);
        }

        private bool Enabled;
        internal void NextFrame()
        {
            if(this.WinFormMouseEnabledValue.Value)
            {
                var m = this.PositionMax;
                var x = m.Item1 / 2.0d;
                var y = m.Item2 / 2.0d;
                var c = new CMousePos(x, y);
                if(this.Position.Item1 != 800)
                {
                //   System.Diagnostics.Debugger.Break();
                }
                this.Position = c;
                this.LastPosition = c;
            }
        }

        [CMemberDeclaration]
        private static readonly CValueDeclaration WinFormMouseEnabledValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_Mouse_WinForm, new Guid("2ea42139-7041-469a-ab1e-90157e61c5ee"), true, false);
        private CBoolValue WinFormMouseEnabledValueM;
        internal CBoolValue WinFormMouseEnabledValue => CLazyLoad.Get(ref this.WinFormMouseEnabledValueM, () => CValue.GetStaticValue<CBoolValue>(this, WinFormMouseEnabledValueDeclaration));

        public bool IsLeftButtonDown => System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed;

        public bool IsRightButtonDown => System.Windows.Input.Mouse.RightButton == System.Windows.Input.MouseButtonState.Pressed;
    }
}
