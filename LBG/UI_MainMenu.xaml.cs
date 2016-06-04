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
using System.Windows.Shapes;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_MainMenu.xaml
    /// </summary>
    public partial class UI_MainMenu : Window
    {
        public UI_MainMenu()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UI_GameOne game1 = new UI_GameOne();
            game1.Show();

            //MessageBox.Show("Bien Hecho");
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("VA AL SEGUNDO JUEGO");
        }
    }
}
