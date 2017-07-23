using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Linq;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using Coding4Fun.Kinect.Wpf;
using System.Media;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_GameSixPieces.xaml
    /// </summary>
    public partial class UI_GameSixPieces : Window
    {
        static KinectSensorChooser mKinect;
        const int PostureDetectionNumber = 100;
        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        int cHandRightOnImageHead = 0;
        int cHandRightOnHead = 0;
        int cHandLeftOnImageHead = 0;
        int cHandLeftOnHead = 0;

        int cHandRightOnImageSpine = 0;
        int cHandLeftOnImageSpine = 0;
        int cHandRightOnSpine = 0;
        int cHandLeftOnSpine = 0;

        int cHandRightOnImageRightHand = 0;
        int cHandLeftOnImageRightHand = 0;
        int cHandRightOnRightHand = 0;
        int cHandLeftOnRightHand = 0;

        int cHandRightOnImageLeftHand = 0;
        int cHandLeftOnImageLeftHand = 0;
        int cHandRightOnLeftHand = 0;
        int cHandLeftOnLeftHand = 0;

        int cHandRightOnImageRightLeg = 0;
        int cHandRightOnRightLeg = 0;
        int cHandLeftOnImageRightLeg = 0;
        int cHandLeftOnRightLeg = 0;

        int cHandRightOnImageLeftLeg = 0;
        int cHandRightOnLeftLeg = 0;
        int cHandLeftOnImageLeftLeg = 0;
        int cHandLeftOnLeftLeg = 0;

        DateTime dHandOnImage;

        SoundPlayer cheersSound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\cheers.wav");

        [Serializable]
        public struct Vector2
        {
            public double X;
            public double Y;
        }

        public UI_GameSixPieces()
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
                    e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default; //Default nos permite detectar todas las articulaciones, Seated solo las 10 de arriba. 
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

            //ZonaCursor.KinectSensor = e.NewSensor; //ya tenemos el cursor
            e.NewSensor.AllFramesReady += kinect_SkeletonFrameReady;
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

            ellipseHandLeft.Visibility = Visibility.Visible; //MANO IZQUIERDA
            ellipseHandRight.Visibility = Visibility.Visible; //MANO DERECHA

            #region HEAD_IMAGE
            if (handOnImage(imageHead, ellipseHandRight) && 
                Canvas.GetLeft(imageHead) == 870 && 
                Canvas.GetTop(imageHead) == 446) //MANO DERECHA en la cabeza (imagen)
            {
                cHandRightOnImageHead++;

                Log("cHandRightOnImageHead: " + cHandRightOnImageHead.ToString());
                labelCHandRight.Content = cHandRightOnImageHead.ToString();
                labelHandRight.Visibility = Visibility.Visible;
                labelCHandRight.Visibility = Visibility.Visible;

                //Right Hand
                cHandRightOnImageRightHand = 0;
                cHandRightOnImageLeftHand = 0;
                cHandRightOnImageRightLeg = 0;
                cHandRightOnImageLeftLeg = 0;
                cHandRightOnImageSpine = 0;

                //Left Hand
                cHandLeftOnImageHead = 0;
                cHandLeftOnImageRightHand = 0;
                cHandLeftOnImageLeftHand = 0;
                cHandLeftOnImageRightLeg = 0;
                cHandLeftOnImageLeftLeg = 0;
                cHandLeftOnImageSpine = 0;
            }

            if (handOnImage(imageHead, ellipseHandLeft) && 
                Canvas.GetLeft(imageHead) == 870 && 
                Canvas.GetTop(imageHead) == 446) //MANO IZQUIERDA en la cabeza (imagen)
            {
                cHandLeftOnImageHead++;

                Log("cHandLeftOnImageHead" + cHandLeftOnImageHead.ToString());
                labelCHandLeft.Content = cHandLeftOnImageHead.ToString();
                labelHandLeft.Visibility = Visibility.Visible;
                labelCHandLeft.Visibility = Visibility.Visible;

                //Right Hand
                cHandRightOnImageRightHand = 0;
                cHandRightOnImageLeftHand = 0;
                cHandRightOnImageRightLeg = 0;
                cHandRightOnImageLeftLeg = 0;
                cHandRightOnImageSpine = 0;
                cHandRightOnImageHead = 0;

                //Left Hand
                cHandLeftOnImageRightHand = 0;
                cHandLeftOnImageLeftHand = 0;
                cHandLeftOnImageRightLeg = 0;
                cHandLeftOnImageLeftLeg = 0;
                cHandLeftOnImageSpine = 0;
            }
            #endregion

            #region TORSO_IMAGE
                #region SPINE
                    if (handOnImage(imageTorsoWithoutHands, ellipseHandRight) && 
                        Canvas.GetLeft(imageTorsoWithoutHands) == 240 && 
                        Canvas.GetTop(imageTorsoWithoutHands) == 516) //MANO DERECHA en la cabeza (imagen)
                    {
                        cHandRightOnImageSpine++;

                        Log("cHandRightOnImageTorso: " + cHandRightOnImageSpine.ToString());
                        labelCHandRight.Content = cHandRightOnImageSpine.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                                        
                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                        cHandLeftOnImageSpine = 0;

                    }

                    if (handOnImage(imageTorsoWithoutHands, ellipseHandLeft) && 
                        Canvas.GetLeft(imageTorsoWithoutHands) == 240 && 
                        Canvas.GetTop(imageTorsoWithoutHands) == 516) //MANO DERECHA en la cabeza (imagen)
                    {
                        cHandLeftOnImageSpine++;

                        Log("cHandLeftOnImageSpine: " + cHandLeftOnImageSpine.ToString());
                        labelCHandRight.Content = cHandLeftOnImageSpine.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                        cHandRightOnImageSpine = 0;
                 
                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                    }
                #endregion
                
                #region RIGHT_HAND
                    if (handOnImage(imageRightHand, ellipseHandRight) && 
                        Canvas.GetLeft(imageRightHand) == 108 && 
                        Canvas.GetTop(imageRightHand) == 340) 
                    {
                        cHandRightOnImageRightHand++;

                        Log("cHandRightOnImageRightHand: " + cHandRightOnImageRightHand.ToString());
                        labelCHandRight.Content = cHandRightOnImageRightHand.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageSpine = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                                        
                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                        cHandLeftOnImageSpine = 0;

                    }

                    if (handOnImage(imageRightHand, ellipseHandLeft) && 
                        Canvas.GetLeft(imageRightHand) == 108 && 
                        Canvas.GetTop(imageRightHand) == 340) 
                    {
                        cHandLeftOnImageRightHand++;

                        Log("cHandLeftOnImageRightHand: " + cHandLeftOnImageRightHand.ToString());
                        labelCHandRight.Content = cHandLeftOnImageRightHand.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                        cHandRightOnImageSpine = 0;
                 
                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageSpine = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                    }
                #endregion

                #region LEFT_HAND
                    if (handOnImage(imageLeftHand, ellipseHandRight) && 
                        Canvas.GetLeft(imageLeftHand) == 793 && 
                        Canvas.GetTop(imageLeftHand) == 154) 
                    {
                        cHandRightOnImageLeftHand++;

                        Log("cHandRightOnImageLeftHand: " + cHandRightOnImageLeftHand.ToString());
                        labelCHandRight.Content = cHandRightOnImageLeftHand.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageSpine = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                                        
                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                        cHandLeftOnImageSpine = 0;

                    }

                    if (handOnImage(imageLeftHand, ellipseHandLeft) && 
                        Canvas.GetLeft(imageLeftHand) == 793 && 
                        Canvas.GetTop(imageLeftHand) == 154)  
                    {
                        cHandLeftOnImageLeftHand++;

                        Log("cHandLeftOnImageLeftHand: " + cHandLeftOnImageLeftHand.ToString());
                        labelCHandRight.Content = cHandLeftOnImageLeftHand.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                        cHandRightOnImageSpine = 0;
                 
                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageSpine = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                    }
            #endregion
            #endregion

            #region LEGS_IMAGE
                #region RIGHT_LEG
                    if (handOnImage(imageRightLeg, ellipseHandRight) &&
                        Canvas.GetLeft(imageRightLeg) == 243 &&
                        Canvas.GetTop(imageRightLeg) == 178)
                    {
                        cHandRightOnImageRightLeg++;

                        Log("cHandRightOnImageRightLeg: " + cHandRightOnImageRightLeg.ToString());
                        labelCHandRight.Content = cHandRightOnImageRightLeg.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageSpine = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftLeg = 0;

                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                        cHandLeftOnImageSpine = 0;

                    }

                    if (handOnImage(imageRightLeg, ellipseHandLeft) &&
                        Canvas.GetLeft(imageRightLeg) == 243 &&
                        Canvas.GetTop(imageRightLeg) == 178)
                    {
                        cHandLeftOnImageRightLeg++;

                        Log("cHandLeftOnImageRightLeg: " + cHandLeftOnImageRightLeg.ToString());
                        labelCHandRight.Content = cHandLeftOnImageRightLeg.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                        cHandRightOnImageSpine = 0;

                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageSpine = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftLeg = 0;
                    }
                #endregion

                #region LEFT_HAND
                    if (handOnImage(imageLeftLeg, ellipseHandRight) &&
                        Canvas.GetLeft(imageLeftLeg) == 1058 &&
                        Canvas.GetTop(imageLeftLeg) == 283)
                    {
                        cHandRightOnImageLeftLeg++;

                        Log("cHandRightOnImageLeftLeg: " + cHandRightOnImageLeftLeg.ToString());
                        labelCHandRight.Content = cHandRightOnImageLeftLeg.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageSpine = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftHand = 0;

                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftLeg = 0;
                        cHandLeftOnImageSpine = 0;

                    }

                    if (handOnImage(imageLeftLeg, ellipseHandLeft) &&
                        Canvas.GetLeft(imageLeftLeg) == 1058 &&
                        Canvas.GetTop(imageLeftLeg) == 283)
                    {
                        cHandLeftOnImageLeftLeg++;

                        Log("cHandLeftOnImageLeftLeg: " + cHandLeftOnImageLeftLeg.ToString());
                        labelCHandRight.Content = cHandLeftOnImageLeftLeg.ToString();
                        labelHandRight.Visibility = Visibility.Visible;
                        labelCHandRight.Visibility = Visibility.Visible;

                        //Right Hand
                        cHandRightOnImageHead = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftHand = 0;
                        cHandRightOnImageRightLeg = 0;
                        cHandRightOnImageLeftLeg = 0;
                        cHandRightOnImageSpine = 0;

                        //Left Hand
                        cHandLeftOnImageHead = 0;
                        cHandLeftOnImageSpine = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageRightLeg = 0;
                        cHandLeftOnImageLeftHand = 0;
                    }
                #endregion
            #endregion
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
                DepthImagePoint elbowRightDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint elbowLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint shoulderCenterDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ShoulderCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint spineDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipCenterDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipRightDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint kneeRightDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint kneeLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint footRightDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.FootRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint footLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.FootLeft].Position, DepthImageFormat.Resolution640x480Fps30);

                ColorImagePoint headColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, headDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint leftHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, leftHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint rightHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rightHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint elbowRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, elbowRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint elbowLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, elbowLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint shoulderCenterColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, shoulderCenterDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint spineColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, spineDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipCenterColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipCenterDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint kneeRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, kneeRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint kneeLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, kneeLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint footRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, footRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint footLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, footLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                CameraPosition(ellipseHead, headColorPoint, "HEAD");
                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND");
                CameraPosition(ellipseHandLeft, elbowRightColorPoint, "ELBOW RIGHT");
                CameraPosition(ellipseHandRight, elbowLeftColorPoint, "ELBOW LEFT");
                CameraPosition(ellipseShoulderCenter, shoulderCenterColorPoint, "SHOULDER CENTER");
                CameraPosition(ellipseSpine, spineColorPoint, "SPINE");
                CameraPosition(ellipseHipCenter, hipCenterColorPoint, "HIP CENTER");
                CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP LEFT");
                CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE LEFT");
                CameraPosition(ellipseFootRight, footRightColorPoint, "FOOT RIGHT");
                CameraPosition(ellipseFootLeft, footLeftColorPoint, "FOOT RIGHT");

                #region HEAD
                    #region HEAD_HAND_RIGHT
                        if (cHandRightOnImageHead == 10) //MANO DERECHA PASO 10 VECES
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHead, headColorPoint, "HEAD - cHandRightOnImageHead");
                            CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                        }

                        if (handOnImage(ellipseHandRight, ellipseHead) && cHandRightOnImageHead >= 10)
                        {
                            cHandRightOnHead++;
                            labelResult.Content = "++: " + cHandRightOnHead;

                            //RIGHT
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftHand = 0;
                            cHandRightOnSpine = 0;
                            cHandRightOnRightLeg = 0;
                            cHandRightOnLeftLeg = 0;

                            //LEFT
                            cHandLeftOnHead = 0;
                            cHandLeftOnRightHand = 0;
                            cHandLeftOnLeftHand = 0;
                            cHandLeftOnSpine = 0;
                            cHandLeftOnRightLeg = 0;
                            cHandLeftOnLeftLeg = 0;
                        }

                        if (cHandRightOnHead == 10)
                        {
                            labelResult2.Content = "BIEN";
                            labelResult2.Visibility = Visibility.Visible;
                            Canvas.SetLeft(imageHead, 540);
                            Canvas.SetTop(imageHead, 194);
                            cheersSound.Play();

                            //RIGHT
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftHand = 0;
                            cHandRightOnSpine = 0;
                            cHandRightOnRightLeg = 0;
                            cHandRightOnLeftLeg = 0;

                            //LEFT
                            cHandLeftOnHead = 0;
                            cHandLeftOnRightHand = 0;
                            cHandLeftOnLeftHand = 0;
                            cHandLeftOnSpine = 0;
                            cHandLeftOnRightLeg = 0;
                            cHandLeftOnLeftLeg = 0;
                        }
                    #endregion

                    #region HEAD_LEFT_HAND
                        if (cHandLeftOnImageHead == 10) //MANO IZQUIERDA PASO 10 VECES
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHead, headColorPoint, "HEAD - cHandRightOnImageHead");
                            CameraPosition(ellipseHandLeft, leftHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                        }

                        if (handOnImage(ellipseHandLeft, ellipseHead) && cHandLeftOnImageHead >= 10)
                        {
                            cHandLeftOnHead++;
                            labelResult.Content = "++: " + cHandLeftOnHead;
                            labelResult.Visibility = Visibility.Visible;

                            //RIGHT
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftHand = 0;
                            cHandRightOnSpine = 0;
                            cHandRightOnRightLeg = 0;
                            cHandRightOnLeftLeg = 0;
                            cHandRightOnHead = 0;

                            //LEFT
                            cHandLeftOnRightHand = 0;
                            cHandLeftOnLeftHand = 0;
                            cHandLeftOnSpine = 0;
                            cHandLeftOnRightLeg = 0;
                            cHandLeftOnLeftLeg = 0;
                        }

                        if (cHandLeftOnHead == 10)
                        {
                            labelResult2.Content = "BIEN";
                            labelResult2.Visibility = Visibility.Visible;
                            Canvas.SetLeft(imageHead, 540);
                            Canvas.SetTop(imageHead, 194);
                            cheersSound.Play();

                            //RIGHT
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftHand = 0;
                            cHandRightOnSpine = 0;
                            cHandRightOnRightLeg = 0;
                            cHandRightOnLeftLeg = 0;
                            cHandRightOnHead = 0;

                            //LEFT
                            cHandLeftOnRightHand = 0;
                            cHandLeftOnLeftHand = 0;
                            cHandLeftOnSpine = 0;
                            cHandLeftOnRightLeg = 0;
                            cHandLeftOnLeftLeg = 0;
                        }
                #endregion
                #endregion

                #region TORSO
                    #region SPINE
                        #region SPINE_HAND_RIGHT
                            if (cHandRightOnImageSpine == 10) //MANO DERECHA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageSpine");
                                CameraPosition(ellipseSpine, spineColorPoint, "SPINE"); ;
                            }

                            if (handOnImage(ellipseHandRight, ellipseSpine) && cHandRightOnImageSpine >= 10)
                            {
                                cHandRightOnSpine++;
                                labelResult.Content = "++: " + cHandRightOnSpine;
                                labelResult.Visibility = Visibility.Visible;

                                //RIGHT
                                cHandRightOnRightHand = 0;
                                cHandRightOnLeftHand = 0;
                                cHandRightOnRightLeg = 0;
                                cHandRightOnLeftLeg = 0;
                                cHandRightOnHead = 0;

                                //LEFT
                                cHandLeftOnRightHand = 0;
                                cHandLeftOnLeftHand = 0;
                                cHandLeftOnHead = 0;
                                cHandLeftOnRightLeg = 0;
                                cHandLeftOnLeftLeg = 0;
                                cHandLeftOnSpine = 0;
                            }

                            if (cHandRightOnSpine == 10)
                            {
                                labelResult2.Content = "BIEN";
                                labelResult2.Visibility = Visibility.Visible;

                                Canvas.SetLeft(imageTorsoWithoutHands, 568);
                                Canvas.SetTop(imageTorsoWithoutHands, 394);
                                cheersSound.Play();

                                //RIGHT
                                cHandRightOnRightHand = 0;
                                cHandRightOnLeftHand = 0;
                                cHandRightOnRightLeg = 0;
                                cHandRightOnLeftLeg = 0;
                                cHandRightOnHead = 0;

                                //LEFT
                                cHandLeftOnRightHand = 0;
                                cHandLeftOnLeftHand = 0;
                                cHandLeftOnHead = 0;
                                cHandLeftOnRightLeg = 0;
                                cHandLeftOnLeftLeg = 0;
                                cHandLeftOnSpine = 0;
                            }
                        #endregion
                        #region SPINE_HAND_LEFT
                            if (cHandLeftOnImageSpine == 10) //MANO DERECHA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandLeft, rightHandColorPoint, "RIGHT HAND - cHandLeftOnImageSpine");
                                CameraPosition(ellipseSpine, elbowRightColorPoint, "SPINE"); ;
                            }

                            if (handOnImage(ellipseHandLeft, ellipseSpine) &&
                                cHandLeftOnImageSpine >= 10)
                            {
                                cHandLeftOnSpine++;
                                labelResult.Content = "++: " + cHandLeftOnSpine;
                                labelResult.Visibility = Visibility.Visible;

                                //RIGHT
                                cHandRightOnRightHand = 0;
                                cHandRightOnLeftHand = 0;
                                cHandRightOnRightLeg = 0;
                                cHandRightOnLeftLeg = 0;
                                cHandRightOnHead = 0;
                                cHandRightOnSpine = 0;

                                //LEFT
                                cHandLeftOnRightHand = 0;
                                cHandLeftOnLeftHand = 0;
                                cHandLeftOnHead = 0;
                                cHandLeftOnRightLeg = 0;
                                cHandLeftOnLeftLeg = 0;
                            }

                            if (cHandLeftOnSpine == 10)
                            {
                                labelResult2.Content = "BIEN";
                                labelResult2.Visibility = Visibility.Visible;

                                Canvas.SetLeft(imageTorsoWithoutHands, 568);
                                Canvas.SetTop(imageTorsoWithoutHands, 394);
                                cheersSound.Play();

                                //RIGHT
                                cHandRightOnRightHand = 0;
                                cHandRightOnLeftHand = 0;
                                cHandRightOnRightLeg = 0;
                                cHandRightOnLeftLeg = 0;
                                cHandRightOnHead = 0;
                                cHandRightOnSpine = 0;

                                //LEFT
                                cHandLeftOnRightHand = 0;
                                cHandLeftOnLeftHand = 0;
                                cHandLeftOnHead = 0;
                                cHandLeftOnRightLeg = 0;
                                cHandLeftOnLeftLeg = 0;
                            }
                        #endregion
                    #endregion

                    #region RIGHT_HAND
                            if (cHandRightOnImageRightHand == 10) //MANO IZQUIERDA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageRightHand");
                                CameraPosition(ellipseElbowRight, elbowRightColorPoint, "SPINE");
                            }

                            if (handOnImage(ellipseHandRight, ellipseElbowRight) && cHandRightOnImageRightHand >= 10)
                            {
                                cHandRightOnRightHand++;
                                labelResult.Content = "++: " + cHandRightOnRightHand;
                                labelResult.Visibility = Visibility.Visible;

                                //RIGHT
                                cHandRightOnLeftHand = 0;
                                cHandRightOnRightLeg = 0;
                                cHandRightOnLeftLeg = 0;
                                cHandRightOnHead = 0;
                                cHandRightOnSpine = 0;

                                //LEFT
                                cHandLeftOnRightHand = 0;
                                cHandLeftOnLeftHand = 0;
                                cHandLeftOnHead = 0;
                                cHandLeftOnRightLeg = 0;
                                cHandLeftOnLeftLeg = 0;
                                cHandLeftOnSpine = 0;
                            }

                            if (cHandRightOnRightHand == 10)
                            {
                                labelResult2.Content = "BIEN";
                                labelResult2.Visibility = Visibility.Visible;

                                Canvas.SetLeft(imageRightHand, 648);
                                Canvas.SetTop(imageRightHand, 512);
                                cheersSound.Play();

                                //RIGHT
                                cHandRightOnRightHand = 0;
                                cHandRightOnLeftHand = 0;
                                cHandRightOnRightLeg = 0;
                                cHandRightOnLeftLeg = 0;
                                cHandRightOnHead = 0;
                                cHandRightOnSpine = 0;

                                //LEFT
                                cHandLeftOnRightHand = 0;
                                cHandLeftOnLeftHand = 0;
                                cHandLeftOnHead = 0;
                                cHandLeftOnRightLeg = 0;
                                cHandLeftOnLeftLeg = 0;
                            }
                        #endregion

                #endregion

                #region LEGS
                //#region LEGS_RIGHT_HAND
                //if (cHandRightOnImageLegs == 10) //MANO DERECHA PASO 10 VECES
                //{
                //    dHandOnImage = DateTime.Now;
                //    CameraPosition(ellipseHandLeft, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                //    CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                //    CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP LEFT");
                //    CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                //    CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE LEFT");

                //    cHandLeftOnImageLegs = 0;
                //    cHandRightOnImageHead = 0;
                //    cHandLeftOnImageHead = 0;
                //    cHandRightOnImageTorso = 0;
                //    cHandLeftOnImageTorso = 0;
                //}

                //if ((handOnImage(ellipseHandRight, ellipseHipRight) ||
                //    handOnImage(ellipseHandRight, ellipseHipLeft) ||
                //    handOnImage(ellipseHandRight, ellipseKneeRight) ||
                //    handOnImage(ellipseHandRight, ellipseKneeLeft)) && cHandRightOnImageLegs >= 10)
                //{
                //    cHandRightOnLegs++;
                //    labelResult.Content = "++: " + cHandRightOnLegs;
                //    labelResult.Visibility = Visibility.Visible;

                //    cHandLeftOnLegs = 0;
                //    cHandRightOnHead = 0;
                //    cHandLeftOnHead = 0;
                //    cHandRightOnTorso = 0;
                //    cHandLeftOnTorso = 0;
                //}

                //if (cHandRightOnLegs == 10)
                //{
                //    labelResult2.Content = "BIEN";
                //    labelResult2.Visibility = Visibility.Visible;
                //    //imageLegsInBody.Visibility = Visibility.Visible;
                //    //imageLegs.Visibility = Visibility.Hidden;
                //    Canvas.SetLeft(imageLegs, 533);
                //    Canvas.SetTop(imageLegs, 124);
                //    cheersSound.Play();

                //    cHandRightOnLegs = 0;
                //    cHandLeftOnLegs = 0;
                //    cHandRightOnHead = 0;
                //    cHandLeftOnHead = 0;
                //    cHandRightOnTorso = 0;
                //    cHandLeftOnTorso = 0;
                //}
                //#endregion
                //#region LEGS_LEFT_HAND
                //if (cHandLeftOnImageLegs == 10) //MANO IZQUIERDA PASO 10 VECES
                //{
                //    dHandOnImage = DateTime.Now;
                //    CameraPosition(ellipseHandLeft, leftHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                //    CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                //    CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP LEFT");
                //    CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                //    CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE LEFT");

                //    cHandRightOnImageLegs = 0;
                //    cHandRightOnImageHead = 0;
                //    cHandLeftOnImageHead = 0;
                //    cHandRightOnImageTorso = 0;
                //    cHandLeftOnImageTorso = 0;
                //}

                //if ((handOnImage(ellipseHandLeft, ellipseHipRight) ||
                //    handOnImage(ellipseHandLeft, ellipseHipLeft) ||
                //    handOnImage(ellipseHandLeft, ellipseKneeRight) ||
                //    handOnImage(ellipseHandLeft, ellipseKneeLeft)) && cHandLeftOnImageLegs >= 10)
                //{
                //    cHandLeftOnLegs++;
                //    labelResult.Content = "++: " + cHandLeftOnLegs;
                //    labelResult.Visibility = Visibility.Visible;

                //    cHandRightOnLegs = 0;
                //    cHandRightOnHead = 0;
                //    cHandLeftOnHead = 0;
                //    cHandRightOnTorso = 0;
                //    cHandLeftOnTorso = 0;
                //}

                //if (cHandLeftOnLegs == 10)
                //{
                //    labelResult2.Content = "BIEN";
                //    labelResult2.Visibility = Visibility.Visible;
                //    //imageLegsInBody.Visibility = Visibility.Visible;
                //    //imageLegs.Visibility = Visibility.Hidden;
                //    Canvas.SetLeft(imageLegs, 533);
                //    Canvas.SetTop(imageLegs, 124);
                //    cheersSound.Play();

                //    cHandLeftOnLegs = 0;
                //    cHandRightOnLegs = 0;
                //    cHandRightOnHead = 0;
                //    cHandLeftOnHead = 0;
                //    cHandRightOnTorso = 0;
                //    cHandLeftOnTorso = 0;
                //}
                //#endregion
                #endregion
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
            double e1X1 = Canvas.GetLeft(element1);
            double e1Y1 = Canvas.GetTop(element1);

            double e1X2 = Canvas.GetLeft(element1) + element1.Width;
            double e1Y2 = Canvas.GetTop(element1) + element1.Height;

            double e2X1 = Canvas.GetLeft(element2);
            double e2Y1 = Canvas.GetTop(element2);

            double e2X2 = Canvas.GetLeft(element2) + element2.Width;
            double e2Y2 = Canvas.GetTop(element2) + element2.Height;

            bool distance = (e1X1 < e2X1) && (e2X2 < e1X2) && (e1Y1 < e2Y1) && (e2Y2 < e1Y2);

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

