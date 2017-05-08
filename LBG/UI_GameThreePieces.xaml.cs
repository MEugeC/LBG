using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Linq;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using Coding4Fun.Kinect.Wpf;



namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_GameThreePieces.xaml
    /// </summary>
    public partial class UI_GameThreePieces : Window
    {
        static KinectSensorChooser mKinect;
        const int                  PostureDetectionNumber = 100;
        bool                       closing                = false;
        const int                  skeletonCount          = 6;
        Skeleton[]                 allSkeletons           = new Skeleton[skeletonCount];
        int                        cHandRight             = 0;
        int                        cHandLeft              = 0;
        int                        cHandOnHead            = 0;
        DateTime                   dHandOnImage;
        DateTime                   dHandOnHead;

        [Serializable]
        public struct Vector2
        {
            public double X;
            public double Y;
        }

        public UI_GameThreePieces()
        {
            InitializeComponent();
            Log("INITIALIZE GAME THREE PIECES");
        }

        public static void Log(string logMessage)
        {
            StreamWriter log;

            if (!File.Exists("ThreePiecesGame.txt"))
            {
                log = new StreamWriter("ThreePiecesGame.txt");
            }
            else
            {
                log = File.AppendText("ThreePiecesGame.txt");
            }
            log.Write("Game Three Pieces - ");
            log.WriteLine("{0} {1} - {2} ", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), logMessage);
            log.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            MainCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

            mKinect = new KinectSensorChooser();
            mKinect.KinectChanged += miKinect_KinectChanged; //detecta si un kinect se conecta o esta desconectado, etc... si lo desconectamos nos manda al evento
            sensorChooserUI.KinectSensorChooser = mKinect;
            mKinect.Start(); //inicializar el kinect
            Log("Kinect Start");
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
                e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);

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

            //ZonaCursor.KinectSensor = e.NewSensor; //ya tenemos el cursor
            e.NewSensor.AllFramesReady += kinect_SkeletonFrameReady;
        }

        void kinect_SkeletonFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }
            //Get a skeleton
            Skeleton skeleton = GetFirstSkeleton(e);

            if (skeleton == null)
            {
                return;
            }

            GetCameraPoint(skeleton, e);

            ellipseHead.Visibility = Visibility.Visible;
            ellipseHandLeft.Visibility = Visibility.Visible;
            ellipseHandRight.Visibility = Visibility.Visible;

            //Llamar handOnHead hasta que este bien o hasta que pase cierto tiempo - CONTADOR 
            if (handOnImage(imageHead, ellipseHandRight)) //mano derecha en la cabeza
            {
                cHandRight++;
                Thread.Sleep(1000);

                Log("cHandRight" + cHandRight.ToString());
                labelHead.Content = "cHandRight" + cHandRight.ToString();
                labelHead.Visibility = Visibility.Visible;
            }

            if (handOnImage(imageHead, ellipseHandLeft)) //mano izquierda en la cabeza
            {
                cHandLeft++;
                Thread.Sleep(1000);
                Log("cHandLeft" + cHandLeft.ToString());
                labelHead.Content = "cHandLeft" + cHandLeft.ToString();
                labelHead.Visibility = Visibility.Visible;
            }
            

            if(cHandRight == 10)
            {
                dHandOnImage = DateTime.Now;
                Log("----------------------------------------------------------------------------------------------------");
                Log("headOnImage - MANO DERECHA");
                labelResult.Content = "headOnImage - MANO DERECHA";
                labelResult.Visibility = Visibility.Visible;
                //if (handOnHead(ellipseHandRight, ellipseHead))
                //{
                //    Log("----------------------------------------------------------------------------------------------------");
                //    Log("handOnHead - BIEN");
                //    labelResult.Content = "BIEN";
                //    labelResult.Visibility = Visibility.Visible;
                //    //cHandRight = 0;
                //}
                //else
                //{
                //    Log("----------------------------------------------------------------------------------------------------");
                //    Log("handOnHead - MAL");
                //    labelResult.Content = "MAL";
                //    labelResult.Visibility = Visibility.Visible;
                //}
            }

            if(cHandLeft == 10)
            {
                dHandOnImage = DateTime.Now;
                Log("----------------------------------------------------------------------------------------------------");
                Log("headOnImage - MANO IZQUIERDA");
                labelResult.Content = "headOnImage - MANO IZQUIERDA";
                labelResult.Visibility = Visibility.Visible;
                //if (handOnHead(ellipseHandLeft, ellipseHead))
                //{
                //    Log("----------------------------------------------------------------------------------------------------");
                //    Log("handOnHead - BIEN");
                //    labelResult.Content = "BIEN";
                //    labelResult.Visibility = Visibility.Visible;
                //    //cHandLeft = 0; 
                //}
                //else
                //{
                //    Log("----------------------------------------------------------------------------------------------------");
                //    Log("handOnHead - MAL");
                //    labelResult.Content = "MAL";
                //    labelResult.Visibility = Visibility.Visible;
                //}
            }
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
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();
                return first;
            }
        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null || mKinect == null)
                {
                    return;
                }

                DepthImagePoint headDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);

                DepthImagePoint leftHandDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);

                DepthImagePoint rightHandDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);

                ColorImagePoint headColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, headDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                ColorImagePoint leftHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, leftHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                ColorImagePoint rightHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rightHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                CameraPosition(ellipseHead, headColorPoint, "HEAD");
                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND");
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point, String name)
        {
            //Canvas.SetLeft(element, point.X + element.Width / 2);
            //Canvas.SetTop(element, point.Y + element.Height / 2);
            Canvas.SetLeft(element, point.X);
            Canvas.SetTop(element, point.Y);
        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            double height = System.Windows.SystemParameters.PrimaryScreenHeight;
            double width = System.Windows.SystemParameters.PrimaryScreenWidth;
            int intHeight = Convert.ToInt32(height);
            int intWidth = Convert.ToInt32(width);

            Joint scaledJoint = joint.ScaleTo(intWidth, intHeight, .9f, .9f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);
        }

        bool handOnImage(FrameworkElement element1, FrameworkElement element2)
        {
            Log("HAND ON IMAGE");

            double e1X1 = Canvas.GetLeft(element1);
            double e1Y1 = Canvas.GetTop(element1);

            double e1X2 = Canvas.GetLeft(element1) + element1.Width;
            double e1Y2 = Canvas.GetTop(element1) + element1.Height;

            double e2X1 = Canvas.GetLeft(element2);
            double e2Y1 = Canvas.GetTop(element2);

            double e2X2 = Canvas.GetLeft(element2) + element2.Width;
            double e2Y2 = Canvas.GetTop(element2) + element2.Height;

            bool distance = (e1X1 < e2X1) && (e2X2 < e1X2) && (e1Y1 < e2Y1) && (e2Y2 < e1Y2);

            Log("Mano X1:" + e1X1.ToString());
            Log("Mano Y1:" + e1Y1.ToString());
            Log("Mano X2:" + e1X2.ToString());
            Log("Mano Y2:" + e1Y2.ToString());
            Log("Dibujo Cabeza X1:" + e2X1.ToString());
            Log("Dibujo Cabeza Y1:" + e2Y1.ToString());
            Log("Dibujo Cabeza X2:" + e2X2.ToString());
            Log("Dibujo Cabeza Y2:" + e2Y2.ToString());

            if (distance)
            {
                Log("handOnImage: true");
                return distance;
            }
            else
            {
                Log("handOnImage: false");
                return distance;
            }
        }

        bool handOnHead(FrameworkElement element1, FrameworkElement element2)
        {
            Thread.Sleep(2000);
            Log("----------------------------------------------------------------------------------------------------");
            Log("handOnHead");

            Vector2 hand, head;
            hand.X = Canvas.GetLeft(element1);
            hand.Y = Canvas.GetTop(element1);
            head.X = Canvas.GetLeft(element2);
            head.Y = Canvas.GetTop(element2);
            Log("----------------------------------------------------------------------------------------------------");
            Log("handOnHead - Cabeza X " + Canvas.GetLeft(element1).ToString());
            Log("handOnHead - Cabeza Y " + Canvas.GetTop(element1).ToString());
            Log("handOnHead - Mano X " + Canvas.GetLeft(element2).ToString());
            Log("handOnHead - Mano Y " + Canvas.GetTop(element2).ToString());
            
            if(GetDistanceBetweenJoints(hand, head))
            {
                cHandOnHead ++;
            }
            Log("----------------------------------------------------------------------------------------------------");
            Log("Hand On Head - Result " + GetDistanceBetweenJoints(hand, head));
            Log("Hand On Head - cHandOnHead " + cHandOnHead.ToString());

            dHandOnHead = DateTime.Now;
            int minutes = (int) dHandOnHead.Subtract(dHandOnImage).TotalMinutes;

            Log("Hand On Head - minutes " + minutes.ToString());

            if (cHandOnHead == 10 && !(minutes < 2)) //Si la mano esta en la cabeza 10 veces y pasaron menos de 2 minutos 
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetDistanceBetweenJoints(Vector2 firstJointType, Vector2 secondJointType)
        {
            double distance = (firstJointType.X - secondJointType.X) +
                              (firstJointType.Y - secondJointType.Y);
            Log("----------------------------------------------------------------------------------------------------");
            if (Math.Abs(distance) > 0.05f)
            {
                Log("GetDistanceBetweenJoints - FALSE - " + distance);
                return false;
            }
            else
            {
                Log("GetDistanceBetweenJoints - TRUE: " + distance);
                return true;
            }
        }

        private void btn_back(object sender, RoutedEventArgs e)
        {
            mKinect.Stop();
            Log("GOING BACK");
            UI_GameOne game1 = new UI_GameOne();
            game1.Show();
            this.Close();
        }

    }
}
