using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
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

        int                        cHandRightOnImageHead  = 0;
        int                        cHandRightOnHead       = 0;
        int                        cHandLeftOnImageHead   = 0;
        int                        cHandLeftOnHead        = 0;
        Boolean                    soundHead              = false;
        Boolean                    cDateHead              = false;

        int                        cHandRightOnImageTorso = 0;
        int                        cHandLeftOnImageTorso  = 0;
        int                        cHandRightOnTorso      = 0;
        int                        cHandLeftOnTorso       = 0;
        Boolean                    soundTorso             = false;
        Boolean                    cDateTorso             = false;

        int                        cHandRightOnImageLegs  = 0;
        int                        cHandRightOnLegs       = 0;
        int                        cHandLeftOnImageLegs   = 0;
        int                        cHandLeftOnLegs        = 0;
        Boolean                    soundLegs              = false;
        Boolean                    cDateLegs              = false;

        int                        cHandRightOnImageHome  = 0;
        int                        cHandLeftOnImageHome   = 0;

        DateTime                   dHandOnImage;

        SoundPlayer                cheersSound            = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\cheers.wav");


        [Serializable]
        public struct Vector2
        {
            public double X;
            public double Y;
        }

        public UI_GameThreePieces()
        {
            InitializeComponent();
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
            mKinect.KinectChanged += miKinect_KinectChanged; //Detects if the sensor is connected or not.
            sensorChooserUI.KinectSensorChooser = mKinect;
            mKinect.Start(); //Initialize the sensor.
            Log("Kinect Start");
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
                e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
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
            
            Skeleton skeleton = GetFirstSkeleton(e); //Get a skeleton

            if (skeleton == null)
            {
                return;
            }

            GetCameraPoint(skeleton, e);

            ellipseHandLeft.Visibility  = Visibility.Visible; //Left hand
            ellipseHandRight.Visibility = Visibility.Visible; //Right hand

            #region HEAD_IMAGE
                //RIGHT HAND to the HEAD (image)
                if (handOnImage(imageHead, ellipseHandRight) && 
                    Canvas.GetLeft(imageHead) == 960 && 
                    Canvas.GetTop(imageHead) == 301) 
                {
                    cHandRightOnImageHead++;

                    Log("cHandRightOnImageHead: " + cHandRightOnImageHead.ToString());
                    labelCHandRight.Content = cHandRightOnImageHead.ToString();
                    labelHandRight.Visibility = Visibility.Visible;
                    labelCHandRight.Visibility = Visibility.Visible;

                    cHandLeftOnImageHead   = 0;
                    cHandRightOnImageLegs  = 0;
                    cHandLeftOnImageLegs   = 0;
                    cHandRightOnImageTorso = 0;
                    cHandLeftOnImageTorso  = 0; 
                }

                //LEFT HAND to the HEAD (image)
                if (handOnImage(imageHead, ellipseHandLeft) && 
                    Canvas.GetLeft(imageHead) == 960 && 
                    Canvas.GetTop(imageHead) == 301) 
                {
                    cHandLeftOnImageHead++;

                    Log("cHandLeftOnImageHead" + cHandLeftOnImageHead.ToString());
                    labelCHandLeft.Content = cHandLeftOnImageHead.ToString();
                    labelHandLeft.Visibility = Visibility.Visible;
                    labelCHandLeft.Visibility = Visibility.Visible;

                    cHandRightOnImageHead  = 0;
                    cHandRightOnImageLegs  = 0;
                    cHandLeftOnImageLegs   = 0;
                    cHandRightOnImageTorso = 0;
                    cHandLeftOnImageTorso  = 0;
                }
            #endregion

            #region TORSO_IMAGE
                //RIGHT HAND to the TORSO (image)
                if (handOnImage(imageTorso, ellipseHandRight) && 
                    Canvas.GetLeft(imageTorso) == 64 && 
                    Canvas.GetTop(imageTorso) == 420) 
                {
                    cHandRightOnImageTorso++;

                    Log("cHandRightOnImageTorso: " + cHandRightOnImageTorso.ToString());
                    labelCHandRight.Content = cHandRightOnImageTorso.ToString();
                    labelHandRight.Visibility = Visibility.Visible;
                    labelCHandRight.Visibility = Visibility.Visible;

                    cHandLeftOnImageTorso = 0;
                    cHandRightOnImageHead = 0;
                    cHandLeftOnImageHead  = 0;
                    cHandRightOnImageLegs = 0;
                    cHandLeftOnImageLegs  = 0;
                }

                //LEFT HAND to the HEAD (image)
                if (handOnImage(imageTorso, ellipseHandLeft) && 
                    Canvas.GetLeft(imageTorso) == 64 && 
                    Canvas.GetTop(imageTorso) == 420) 
                {
                    cHandLeftOnImageTorso++;

                    Log("cHandLeftOnImageTorso: " + cHandLeftOnImageTorso.ToString());
                    labelCHandLeft.Content = cHandLeftOnImageTorso.ToString();
                    labelHandLeft.Visibility = Visibility.Visible;
                    labelCHandLeft.Visibility = Visibility.Visible;

                    cHandRightOnImageTorso = 0;
                    cHandRightOnImageHead  = 0;
                    cHandLeftOnImageHead   = 0;
                    cHandRightOnImageLegs  = 0;
                    cHandLeftOnImageLegs   = 0;
                }
            #endregion

            #region LEGS_IMAGE
                //RIGHT HAND to the LEGS (image)
                if (handOnImage(imageLegs, ellipseHandRight) && 
                    Canvas.GetLeft(imageLegs) == 92 && 
                    Canvas.GetTop(imageLegs) == 193)
                {
                    cHandRightOnImageLegs++;

                    Log("cHandRightOnImageLegs: " + cHandRightOnImageLegs.ToString());
                    labelCHandRight.Content = cHandRightOnImageLegs.ToString();
                    labelHandRight.Visibility = Visibility.Visible;
                    labelCHandRight.Visibility = Visibility.Visible;

                    cHandLeftOnImageLegs   = 0;
                    cHandRightOnImageHead  = 0;
                    cHandLeftOnImageHead   = 0;
                    cHandRightOnImageTorso = 0;
                    cHandLeftOnImageTorso  = 0;
                }

                //LEFT HAND to the LEGS (image)
                if (handOnImage(imageLegs, ellipseHandLeft) && 
                    Canvas.GetLeft(imageLegs) == 92 && 
                    Canvas.GetTop(imageLegs) == 193) 
                {
                    cHandLeftOnImageLegs++;

                    Log("cHandLeftOnImageLegs: " + cHandLeftOnImageLegs.ToString());
                    labelCHandLeft.Content = cHandLeftOnImageLegs.ToString();
                    labelHandLeft.Visibility = Visibility.Visible;
                    labelCHandLeft.Visibility = Visibility.Visible;

                    cHandRightOnImageLegs  = 0;
                    cHandRightOnImageHead  = 0;
                    cHandLeftOnImageHead   = 0;
                    cHandRightOnImageTorso = 0;
                    cHandLeftOnImageTorso  = 0;
                }
            #endregion

            #region GO_HOME
                //RIGHT HAND to the HOME (image)
                if (handOnImage(imageHome, ellipseHandRight))
                {
                    cHandRightOnImageHome++;

                    cHandLeftOnImageHead   = 0;
                    cHandRightOnImageLegs  = 0;
                    cHandLeftOnImageLegs   = 0;
                    cHandRightOnImageTorso = 0;
                    cHandLeftOnImageTorso  = 0; 
                    cHandRightOnImageHead  = 0;
                    cHandLeftOnImageHome   = 0;
                }

                //LEFT HAND to the HOME (image)
                if (handOnImage(imageHome, ellipseHandLeft))
                {
                    cHandLeftOnImageHome++;

                    cHandRightOnImageHead  = 0;
                    cHandRightOnImageLegs  = 0;
                    cHandLeftOnImageLegs   = 0;
                    cHandRightOnImageTorso = 0;
                    cHandLeftOnImageTorso  = 0;
                    cHandRightOnImageHead  = 0;
                    cHandRightOnImageHome   = 0;
                }
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

                DepthImagePoint headDepthPoint           = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint leftHandDepthPoint       = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint rightHandDepthPoint      = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint shoulderCenterDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ShoulderCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint spineDepthPoint          = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipCenterDepthPoint      = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipRightDepthPoint       = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipLeftDepthPoint        = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint kneeRightDepthPoint      = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint kneeLeftDepthPoint       = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint footRightDepthPoint      = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.FootRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint footLeftDepthPoint       = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.FootLeft].Position, DepthImageFormat.Resolution640x480Fps30);

                ColorImagePoint headColorPoint           = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, headDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint leftHandColorPoint       = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, leftHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint rightHandColorPoint      = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rightHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint shoulderCenterColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, shoulderCenterDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint spineColorPoint          = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, spineDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipCenterColorPoint      = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipCenterDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipRightColorPoint       = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipLeftColorPoint        = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint kneeRightColorPoint      = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, kneeRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint kneeLeftColorPoint       = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, kneeLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint footRightColorPoint      = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, footRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint footLeftColorPoint       = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, footLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                CameraPosition(ellipseHead, headColorPoint, "HEAD");
                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND");
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
                        //RIGHT HAND in the HEAD 10 times
                        if (cHandRightOnImageHead == 10) 
                        {
                            CameraPosition(ellipseHead, headColorPoint, "HEAD - cHandRightOnImageHead");
                            CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                            soundHead = true;
                        }

                        if (cHandRightOnImageHead >= 10 && soundHead)
                        {
                            dHandOnImage = DateTime.Now;
                            // headSound.play(); Play HEAD Sound
                        }

                        if (cHandRightOnImageHead >= 10 && ((DateTime.Now - dHandOnImage).Minutes >= 2) && !cDateHead)
                        {
                            // headSound.play(); Play HEAD Sound
                            cDateHead = true;
                        }     
                               
                        if (handOnImage(ellipseHandRight, ellipseHead) && cHandRightOnImageHead >= 10)
                        {
                            cHandRightOnHead++;
                            labelResult.Content = "++: " + cHandRightOnHead;
                        }

                        if (cHandRightOnHead == 5)
                        {
                            labelResult2.Content = "BIEN";
                            labelResult2.Visibility = Visibility.Visible;

                            Canvas.SetLeft(imageHead, 540);
                            Canvas.SetTop(imageHead, 151);
                            cheersSound.Play();

                            cHandRightOnHead  = 0;
                            cHandLeftOnHead   = 0;
                            cHandRightOnTorso = 0; 
                            cHandLeftOnTorso  = 0;
                            cHandRightOnLegs  = 0;
                            cHandLeftOnLegs   = 0;
                        }
                    #endregion

                    #region HEAD_LEFT_HAND
                    if (cHandLeftOnImageHead == 10) 
                    {
                        dHandOnImage = DateTime.Now;
                        CameraPosition(ellipseHead, headColorPoint, "HEAD - cHandRightOnImageHead");
                        CameraPosition(ellipseHandLeft, leftHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");

                        cHandRightOnImageHead  = 0;
                        cHandRightOnImageLegs  = 0;
                        cHandLeftOnImageLegs   = 0;
                        cHandRightOnImageTorso = 0;
                        cHandLeftOnImageTorso  = 0; 
                    }

                    if (handOnImage(ellipseHandLeft, ellipseHead) && cHandLeftOnImageHead >= 10)
                    {
                        cHandLeftOnHead++;
                        labelResult.Content = "++: " + cHandLeftOnHead;
                        labelResult.Visibility = Visibility.Visible;

                        cHandRightOnHead  = 0;
                        cHandRightOnTorso = 0; 
                        cHandLeftOnTorso  = 0;
                        cHandRightOnLegs  = 0;
                        cHandLeftOnLegs   = 0;
                    }

                    if (cHandLeftOnHead == 5)
                    {
                        labelResult2.Content = "BIEN";
                        labelResult2.Visibility = Visibility.Visible;
                        Canvas.SetLeft(imageHead, 540);
                        Canvas.SetTop(imageHead, 151);
                        cheersSound.Play();

                        cHandLeftOnHead   = 0;
                        cHandRightOnHead  = 0;
                        cHandRightOnTorso = 0; 
                        cHandLeftOnTorso  = 0;
                        cHandRightOnLegs  = 0;
                        cHandLeftOnLegs   = 0;
                    }
                #endregion
                #endregion

                #region TORSO
                    #region TORSO_RIGHT_HAND
                    if (cHandRightOnImageTorso == 10) //MANO DERECHA PASO 10 VECES
                    {
                        dHandOnImage = DateTime.Now;
                        CameraPosition(ellipseHandLeft, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                        CameraPosition(ellipseSpine, spineColorPoint, "SPINE");;

                        cHandLeftOnImageTorso = 0;
                        cHandRightOnImageHead = 0;
                        cHandLeftOnImageHead  = 0;
                        cHandRightOnImageLegs = 0;
                        cHandLeftOnImageLegs  = 0;
                    }

                    if (handOnImage(ellipseHandRight, ellipseSpine) && cHandRightOnImageTorso >= 10)
                    {
                        cHandRightOnTorso++;
                        labelResult.Content = "++: " + cHandRightOnTorso;
                        labelResult.Visibility = Visibility.Visible;

                        cHandLeftOnTorso = 0;
                        cHandRightOnHead = 0;
                        cHandLeftOnHead  = 0;
                        cHandRightOnLegs = 0;
                        cHandLeftOnLegs  = 0;
                    }

                    if (cHandRightOnTorso == 5)
                    {
                        labelResult2.Content = "BIEN";
                        labelResult2.Visibility = Visibility.Visible;

                        Canvas.SetLeft(imageTorso, 473);
                        Canvas.SetTop(imageTorso, 324);
                        cheersSound.Play();

                        cHandRightOnTorso = 0;
                        cHandLeftOnTorso = 0;
                        cHandRightOnHead = 0;
                        cHandLeftOnHead  = 0;
                        cHandRightOnLegs = 0;
                        cHandLeftOnLegs  = 0;
                    }
                    #endregion

                    #region TORSO_LEFT_HAND
                        if (cHandLeftOnImageTorso == 10) //MANO IZQUIERDA PASO 10 VECES
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHandLeft, leftHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                            CameraPosition(ellipseSpine, spineColorPoint, "SPINE");

                            cHandRightOnImageTorso = 0;
                            cHandRightOnImageHead  = 0;
                            cHandLeftOnImageHead   = 0;
                            cHandRightOnImageLegs  = 0;
                            cHandLeftOnImageLegs   = 0;
                        }

                        if (handOnImage(ellipseHandLeft, ellipseSpine) && cHandLeftOnImageTorso >= 10)
                        {
                            cHandLeftOnTorso++;
                            labelResult.Content = "++: " + cHandLeftOnTorso;
                            labelResult.Visibility = Visibility.Visible;

                            cHandRightOnTorso = 0;
                            cHandRightOnHead  = 0;
                            cHandLeftOnHead   = 0;
                            cHandRightOnLegs  = 0;
                            cHandLeftOnLegs   = 0;
                        }

                        if (cHandLeftOnTorso == 5)
                        {
                            labelResult2.Content = "BIEN";
                            labelResult2.Visibility = Visibility.Visible;
                            Canvas.SetLeft(imageTorso, 473);
                            Canvas.SetTop(imageTorso, 324);
                            cheersSound.Play();

                            cHandLeftOnTorso  = 0;
                            cHandRightOnTorso = 0;
                            cHandRightOnHead  = 0;
                            cHandLeftOnHead   = 0;
                            cHandRightOnLegs  = 0;
                            cHandLeftOnLegs   = 0;
                        }
                    #endregion

                #endregion

                #region LEGS
                    #region LEGS_RIGHT_HAND
                        if (cHandRightOnImageLegs == 10) //MANO DERECHA PASO 10 VECES
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHandLeft, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                            CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                            CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP LEFT");
                            CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                            CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE LEFT");

                            cHandLeftOnImageLegs   = 0;
                            cHandRightOnImageHead  = 0;
                            cHandLeftOnImageHead   = 0;
                            cHandRightOnImageTorso = 0;
                            cHandLeftOnImageTorso  = 0;
                        }

                        if ((handOnImage(ellipseHandRight, ellipseHipRight) ||
                            handOnImage(ellipseHandRight, ellipseHipLeft) ||
                            handOnImage(ellipseHandRight, ellipseKneeRight) ||
                            handOnImage(ellipseHandRight, ellipseKneeLeft)) && cHandRightOnImageLegs >= 10)
                        {
                            cHandRightOnLegs++;
                            labelResult.Content = "++: " + cHandRightOnLegs;
                            labelResult.Visibility = Visibility.Visible;

                            cHandLeftOnLegs    = 0;
                            cHandRightOnHead   = 0;
                            cHandLeftOnHead    = 0;
                            cHandRightOnTorso  = 0;
                            cHandLeftOnTorso   = 0;
                        }

                        if (cHandRightOnLegs == 5)
                        {
                            labelResult2.Content = "BIEN";
                            labelResult2.Visibility = Visibility.Visible;
                            Canvas.SetLeft(imageLegs,523);
                            Canvas.SetTop(imageLegs,580);
                            cheersSound.Play();

                            cHandRightOnLegs   = 0;
                            cHandLeftOnLegs    = 0;
                            cHandRightOnHead   = 0;
                            cHandLeftOnHead    = 0;
                            cHandRightOnTorso  = 0;
                            cHandLeftOnTorso   = 0;
                        }
                #endregion
                    #region LEGS_LEFT_HAND
                        if (cHandLeftOnImageLegs == 10) //MANO IZQUIERDA PASO 10 VECES
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHandLeft, leftHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                            CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                            CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP LEFT");
                            CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                            CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE LEFT");

                            cHandRightOnImageLegs  = 0;
                            cHandRightOnImageHead  = 0;
                            cHandLeftOnImageHead   = 0;
                            cHandRightOnImageTorso = 0;
                            cHandLeftOnImageTorso  = 0;
                        }

                        if ((handOnImage(ellipseHandLeft, ellipseHipRight) ||
                            handOnImage(ellipseHandLeft, ellipseHipLeft) ||
                            handOnImage(ellipseHandLeft, ellipseKneeRight) ||
                            handOnImage(ellipseHandLeft, ellipseKneeLeft)) && cHandLeftOnImageLegs >= 10)
                        {
                            cHandLeftOnLegs++;
                            labelResult.Content = "++: " + cHandLeftOnLegs;
                            labelResult.Visibility = Visibility.Visible;

                            cHandRightOnLegs   = 0;
                            cHandRightOnHead   = 0;
                            cHandLeftOnHead    = 0;
                            cHandRightOnTorso  = 0;
                            cHandLeftOnTorso   = 0;
                        }

                        if (cHandLeftOnLegs == 5)
                        {
                            labelResult2.Content = "BIEN";
                            labelResult2.Visibility = Visibility.Visible;
                            Canvas.SetLeft(imageLegs,523);
                            Canvas.SetTop(imageLegs,580);
                            cheersSound.Play();       
                            
                            cHandLeftOnLegs    = 0;                     
                            cHandRightOnLegs   = 0;
                            cHandRightOnHead   = 0;
                            cHandLeftOnHead    = 0;
                            cHandRightOnTorso  = 0;
                            cHandLeftOnTorso   = 0;
                        }
                #endregion
                #endregion

                #region GO_HOME
                    #region GO_HOME_RIGHT_HAND
                        if (cHandRightOnImageHome == 20) 
                        {
                            mKinect.Stop();
                            UI_MainMenu main = new UI_MainMenu();
                            main.Show();
                            this.Close();                            
                        }
                #endregion
                    #region GO_HOME_LEFT_HAND
                        if (cHandLeftOnImageHome == 20) 
                        {
                            mKinect.Stop();
                            UI_MainMenu main = new UI_MainMenu();
                            main.Show();
                            this.Close();                            
                        }
                    #endregion
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
    }
}
