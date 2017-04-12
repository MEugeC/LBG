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

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_GameSixPieces.xaml
    /// </summary>
    public partial class UI_GameSixPieces : Window
    {
        static KinectSensorChooser miKinect;

        public UI_GameSixPieces()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            miKinect = new KinectSensorChooser();
            miKinect.KinectChanged += miKinect_KinectChanged;
            //detecta si un kinect se conecta o esta desconectado, etc...
            // si lo desconectamos nos manda al evento
            sensorChooserUI.KinectSensorChooser = miKinect;
            miKinect.Start(); //inicializar el kinect
        }

        void miKinect_KinectChanged(object sender, KinectChangedEventArgs e)
        {

            if (e.OldSensor == null) //esto va de KinectChangedEventArgs, si es null es que lo desconcectamos
            {
                try
                {
                    e.OldSensor.DepthStream.Disable(); //desabilitar la profundidad y el esqueleto
                    e.OldSensor.SkeletonStream.Disable();
                }
                catch (Exception)
                {
                }
            }

            if (e.NewSensor == null) //verifico si un nuevo kinect se conecto
                return;

            try //habilitar la profundidad y esqueletos
            {
                e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                //asigno el formato a la imagen
                e.NewSensor.SkeletonStream.Enable();

                try
                {
                    e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    //este modo nos permite estar sentado pero detectar las articulaciones de la parte superior del cuerpo
                    e.NewSensor.DepthStream.Range = DepthRange.Near;
                    //para que tenga rango mas cercano solo para kinect windows
                    e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    //para el rango cercano de los esqueletos
                }
                catch (InvalidOperationException)
                {
                    e.NewSensor.DepthStream.Range = DepthRange.Default;
                    e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                }
            }
            catch (InvalidOperationException)
            {
            }

            ZonaCursor.KinectSensor = e.NewSensor; //ya tenemos el cursor
        }

        private void btn_head(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CABEZA");
        }

        private void btn_torso(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TORSO");
        }

        private void btn_rightHand(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MANO IZQUIERDA");
        }

        private void btn_leftHand(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MANO DERECHA");
        }

        private void btn_rightLeg(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PIE DERECHO");
        }

        private void btn_leftLeg(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PIE IZQUIERDO");
        }

        private void btn_back(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
            UI_GameOne game1 = new UI_GameOne();
            game1.Show();
            //MessageBox.Show("Bien Hecho");
            this.Close();
        }
    }
}
