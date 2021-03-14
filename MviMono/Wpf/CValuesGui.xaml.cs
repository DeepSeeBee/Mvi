using CharlyBeck.Mvi.Value;
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

namespace CharlyBeck.Mvi.Mono.Wpf
{
    /// <summary>
    /// Interaktionslogik für CValuesGui.xaml
    /// </summary>
    public partial class CValuesGui : UserControl
    {
        public CValuesGui()
        {
            InitializeComponent();
        }

        private void Command_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aFe = (FrameworkElement)sender;
                var aDc = aFe.DataContext;
                var aCmd = (CCommand)aDc;
                aCmd.Invoke();
            }
            catch (Exception)
            {
            }
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aFe = (FrameworkElement)sender;
                var aDc = aFe.DataContext;
                var aVal = (CValue)aDc;
                aVal.VmIncrement();
            }
            catch (Exception)
            {
            }
        }
        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aFe = (FrameworkElement)sender;
                var aDc = aFe.DataContext;
                var aVal = (CValue)aDc;
                aVal.VmDecrement();
            }
            catch (Exception)
            {
            }

        }
    }
}
