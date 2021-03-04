using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CharlyBeck.Mvi.Mono.Input.Hid
{
    /// <summary>
    /// Interaktionslogik für CHidDeviceGui.xaml
    /// </summary>
    public partial class CHidDevicesGui : UserControl
    {
        public CHidDevicesGui()
        {
            InitializeComponent();

            this.DataContext = new CHidDevicesVm();
        }
    }
}
