using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Linq;
using Coding4Fun.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Threading;

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
        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

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
            HandOnHead,
            HandOnSpine,
            HandOnLeg
        }

        Vector3 head,
                handRight,
                handLeft,
                wristRight,
                wristLeft,
                elbowRight,
                elbowLeft,
                shoulderRight,
                shoulderLeft,
                spine,
                hip,
                hipRight,
                hipLeft,
                kneeRight,
                kneeLeft,
                ankleRight,
                ankleLeft,
                footRight,
                footLeft;

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

        void kinect_SkeletonFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }
            else
            {  
                //Get a skeleton
                Skeleton skeleton = GetFirstSkeleton(e);

                if (skeleton == null)
                {
                    return;
                }
                else if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    head = getVector(skeleton, JointType.Head);

                    handRight = getVector(skeleton, JointType.HandRight);
                    handLeft = getVector(skeleton, JointType.HandLeft);

                    wristRight = getVector(skeleton, JointType.WristRight);
                    wristLeft = getVector(skeleton, JointType.WristLeft);

                    elbowRight = getVector(skeleton, JointType.ElbowRight);
                    elbowLeft = getVector(skeleton, JointType.ElbowLeft);

                    shoulderRight = getVector(skeleton, JointType.ShoulderRight);
                    shoulderLeft = getVector(skeleton, JointType.ShoulderLeft);

                    spine = getVector(skeleton, JointType.Head);
                    hip = getVector(skeleton, JointType.Head);

                    hipRight = getVector(skeleton, JointType.HipRight);
                    hipLeft = getVector(skeleton, JointType.HipLeft);

                    kneeRight = getVector(skeleton, JointType.KneeRight);
                    kneeLeft = getVector(skeleton, JointType.KneeLeft);

                    ankleRight = getVector(skeleton, JointType.AnkleRight);
                    ankleLeft = getVector(skeleton, JointType.AnkleLeft);

                    footRight = getVector(skeleton, JointType.FootRight);
                    footLeft = getVector(skeleton, JointType.FootLeft);
                }
            }
            
        }

        Vector3 getVector (Skeleton skeleton, JointType jointType)
        {
            Vector3 vector3 = new Vector3();
            vector3.X = skeleton.Joints[jointType].Position.X;
            vector3.Y = skeleton.Joints[jointType].Position.Y;
            vector3.Z = skeleton.Joints[jointType].Position.Z;

            return vector3;
        }

        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);
                Skeleton first = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                return first;
            }
        }

        bool getDistance(Vector3 vectorOne, Vector3 vectorTwo)
        {
            float distance = (vectorOne.X - vectorTwo.X) * 
                             (vectorOne.Y - vectorTwo.Y) * 
                             (vectorOne.Z - vectorTwo.Z);
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

        bool handOnHead()
        {
            return getDistance(handRight, head) ||
                   getDistance(handLeft, head);
        }

        bool handOnSpine()
        {
            return getDistance(handRight, handLeft) ||
                   getDistance(handRight, wristRight) ||
                   getDistance(handRight, wristLeft) ||
                   getDistance(handRight, elbowRight) ||
                   getDistance(handRight, elbowLeft) ||
                   getDistance(handRight, shoulderRight) ||
                   getDistance(handRight, shoulderRight) ||
                   getDistance(handRight, shoulderRight) ||
                   getDistance(handRight, spine) ||
                   getDistance(handRight, hip);
        }

        bool handOnLegs()
        {
            return getDistance(handRight, hipRight) ||
                   getDistance(handRight, hipLeft) ||
                   getDistance(handRight, kneeRight) ||
                   getDistance(handRight, kneeLeft) ||
                   getDistance(handRight, ankleRight) ||
                   getDistance(handRight, ankleLeft) ||
                   getDistance(handRight, footRight) ||
                   getDistance(handRight, footLeft);
        }

        private void btn_head(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("CABEZA");
            Thread.Sleep(5000);

            if (handOnHead())
            {
                if (PostureDetector(Posture.HandOnHead))
                {
                    MessageBox.Show("BIEN");
                }
                else
                {
                    MessageBox.Show("MAL");
                }
            }
            else if (PostureDetector(Posture.None))
            {
                MessageBox.Show("MAL");
            }
        }
    
        private void btn_torso(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TORSO Y BRAZOS");

            if (handOnSpine())
            {
                if (PostureDetector(Posture.HandOnSpine))
                {
                    MessageBox.Show("BIEN");
                }
                else
                {
                    MessageBox.Show("MAL");
                }
            }
            else if (PostureDetector(Posture.None))
            {
                MessageBox.Show("MAL");
            }
        }

        private void btn_legs(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PIERNAS");

            if (handOnLegs())
            {
                if (PostureDetector(Posture.HandOnSpine))
                {
                    MessageBox.Show("BIEN");
                }
                else
                {
                    MessageBox.Show("MAL");
                }
            }
            else if (PostureDetector(Posture.None))
            {
                MessageBox.Show("MAL");
            }

        }

        private void btn_back(object sender, RoutedEventArgs e)
        {
            miKinect.Stop();
            UI_GameOne game1 = new UI_GameOne();
            game1.Show();
            this.Close();
        }
    }
}
