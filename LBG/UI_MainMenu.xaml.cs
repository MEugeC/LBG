using System;
using System.Windows;
using System.Media;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_MainMenu.xaml
    /// </summary>
    public partial class UI_MainMenu : Window
    {
        static KinectSensorChooser mKinect; 
        SoundPlayer                cheersSound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\cheers.wav");

        public UI_MainMenu()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            MainGrid.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

            mKinect = new KinectSensorChooser();
            mKinect.KinectChanged += miKinect_KinectChanged; //Detects if the sensor is connected or not.
            sensorChooserUI.KinectSensorChooser = mKinect;
            mKinect.Start(); //Initialize the sensor.

            //Sonido //Audio Comenzando
        }

        void miKinect_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            bool error = true; 
            if (e.OldSensor == null) //NULL = disconnected
            {
                try
                {
                    e.OldSensor.DepthStream.Disable(); //Disable depth.
                    e.OldSensor.SkeletonStream.Disable(); //Disable skeleton.
                }
                catch (Exception)
                {
                    error = true;
                }
            }

            if (e.NewSensor == null) //Check if a sensor is connected.
                return;

            try //Enable the depth and the skeleton.
            {
                e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); 
                e.NewSensor.SkeletonStream.Enable();

                try
                {
                    //Default allows us to detect all joints, Seated only the top 10.
                    e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                    //Closest range only for kinect windows.
                    e.NewSensor.DepthStream.Range = DepthRange.Near;
                    //Closest range for the skeleton.
                    e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
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
        }

        private void btn_gameOne(object sender, RoutedEventArgs e)
        {
            mKinect.Stop();
            UI_GameThreePieces game1 = new UI_GameThreePieces();
            game1.Show();
            this.Close();
        }

        private void btn_gameTwo(object sender, RoutedEventArgs e)
        {
            mKinect.Stop();
            UI_GameThreePiecesWithHelp game2 = new UI_GameThreePiecesWithHelp();
            game2.Show();
            this.Close();
        }

        private void btn_gameThree(object sender, RoutedEventArgs e)
        {
            mKinect.Stop();
            UI_GameSixPiecesWithHelp game2 = new UI_GameSixPiecesWithHelp();
            game2.Show();
            this.Close();
        }

        private void btn_gameFour(object sender, RoutedEventArgs e)
        {
            mKinect.Stop();
            UI_GameSixPieces game2 = new UI_GameSixPieces();
            game2.Show();
            this.Close();
        }
    }
}
