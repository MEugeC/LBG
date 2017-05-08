using System;
using System.Windows;

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_MainMenu.xaml
    /// </summary>
    public partial class UI_MainMenu : Window
    {
        static KinectSensorChooser miKinect;
        //para ver si esta conectado, si se esta inicializando, etc... la tabla de todos los estados esta en la web
        
        public UI_MainMenu()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            MainGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

            miKinect = new KinectSensorChooser();
            miKinect.KinectChanged += miKinect_KinectChanged;
            //detecta si un kinect se conecta o esta desconectado, etc...
            // si lo desconectamos nos manda al evento
            sensorChooserUI.KinectSensorChooser = miKinect;
            miKinect.Start(); //inicializar el kinect
        }

        void miKinect_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            //MessageBox.Show("miKinect_KinectChanged");
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

            ZonaCursor.KinectSensor = e.NewSensor;
            //MessageBox.Show("FIN"kkk);//ya tenemos el cursor
        }

        private void btn_gameOne(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
            UI_GameOne game1 = new UI_GameOne();
            game1.Show();
            //MessageBox.Show("Bien Hecho");
            this.Close();
        }

        private void btn_gameTwo(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("VA AL SEGUNDO JUEGO");
        }
    }
}
