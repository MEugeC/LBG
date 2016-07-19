using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_GameOne.xaml
    /// </summary>
    public partial class UI_GameOne : Window
    {
        KinectSensorChooser miKinect;

        public UI_GameOne()
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
            bool error = true; //verificar si existe algun error

            if (e.OldSensor == null) //esto va de KinectChangedEventArgs, si es null es que lo desconcectamos
            {
                try
                {
                    e.OldSensor.DepthStream.Disable(); //desabilitar la profundidad y el esqueleto
                    e.OldSensor.SkeletonStream.Disable();
                }
                catch (Exception)
                {
                    error = true;
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
                error = true;
            }

            ZonaCursor.KinectSensor = e.NewSensor; //ya tenemos el cursor
        }

        private void btn_threePieces(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
            UI_GameThreePieces game3 = new UI_GameThreePieces();
            game3.Show();
            //MessageBox.Show("Bien Hecho");
            this.Close();
        }

        private void btn_sixPieces(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
            UI_GameSixPieces game6 = new UI_GameSixPieces();
            game6.Show();
            //MessageBox.Show("Bien Hecho");
            this.Close();

        }

        private void btn_back(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
            UI_MainMenu main = new UI_MainMenu();
            main.Show();
            //MessageBox.Show("Bien Hecho");
            this.Close();

        }
    }
}
