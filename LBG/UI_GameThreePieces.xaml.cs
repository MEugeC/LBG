using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;


namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_GameThreePieces.xaml
    /// </summary>
    public partial class UI_GameThreePieces : Window
    {
        static KinectSensorChooser miKinect;
        const int PostureDetectionNumber = 10;
        int accumulator = 0;
        Posture postureInDetection = Posture.None;
        Posture previousPosture = Posture.None;

        [Serializable]
        public struct Vector3
        {
            public float X;
            public float Y;
            public float Z;
        }


        public enum Posture
        {
            None,
            HandOnHead
        }

        public UI_GameThreePieces()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            miKinect = new KinectSensorChooser();
            miKinect.KinectChanged += miKinect_KinectChanged; //detecta si un kinect se conecta o esta desconectado, etc... si lo desconectamos nos manda al evento
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
                e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); //asigno el formato a la imagen
                e.NewSensor.SkeletonStream.Enable();

                try
                {
                    e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated; //este modo nos permite estar sentado pero detectar las articulaciones de la parte superior del cuerpo
                    e.NewSensor.DepthStream.Range = DepthRange.Near; //para que tenga rango mas cercano solo para kinect windows
                    e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true; //para el rango cercano de los esqueletos
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

        void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            foreach (SkeletonData skeleton in e.SkeletonFrame.Skeletons)
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    Vector3 handRight = new Vector3();
                    handRight.X = skeleton.Joints[JointType.HandRight].Position.X;
                    handRight.Y = skeleton.Joints[JointType.HandRight].Position.Y;
                    handRight.Z = skeleton.Joints[JointType.HandRight].Position.Z;

                    Vector3 handLeft = new Vector3();
                    handLeft.X = skeleton.Joints[JointType.HandLeft].Position.X;
                    handLeft.Y = skeleton.Joints[JointType.HandLeft].Position.Y;
                    handLeft.Z = skeleton.Joints[JointType.HandLeft].Position.Z;

                    Vector3 head = new Vector3();
                    head.X = skeleton.Joints[JointType.Head].Position.X;
                    head.Y = skeleton.Joints[JointType.Head].Position.Y;
                    head.Z = skeleton.Joints[JointType.Head].Position.Z;
                }
        }


        bool HandOnHead(Vector3 hand, Vector3 head)
        {
            float distance = (hand.X - head.X) - (hand.Y - head.Y) - (hand.Z - head.Z);
            if (Math.Abs(distance) > 0.05f)
                return false;
            else
                return true;
        }

        bool PostureDetector(Posture posture)
        {
            if (postureInDetection != posture)
            {
                accumulator = 0;
                postureInDetection = posture;
                return false;
            }
            if (accumulator < PostureDetectionNumber)
            {
                accumulator++;
                return false;
            }
            if (posture != previousPosture)
            {
                previousPosture = posture;
                accumulator = 0;
                return true;
            }
            else
                accumulator = 0;
            return false;
        }

        private void btn_head(object sender, RoutedEventArgs e)
        {
           MessageBox.Show("CABEZA");

            if (HandOnHead(handRight, head))
            {
                if (PostureDetector(Posture.HandOnHead))
                {
                    //
                }
            }
            else if (PostureDetector(Posture.None))
            {
                //
            }
        }
    

        private void btn_torso(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TORSO Y BRAZOS");
        }

        private void btn_legs(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PIERNAS");
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
