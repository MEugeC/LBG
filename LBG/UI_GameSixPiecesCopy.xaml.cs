using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Coding4Fun.Kinect.Wpf;

namespace LBG
{
    /// <summary>
    /// Interaction logic for UI_GameSixPieces.xaml
    /// </summary>
    public partial class UI_GameSixPiecesCopy : Window
    {
        static KinectSensorChooser mKinect;
        const int                  PostureDetectionNumber = 100;
        bool                       closing                = false;
        const int                  skeletonCount          = 6;
        Skeleton[]                 allSkeletons           = new Skeleton[skeletonCount];
        bool soundNext = false;
        int cHandRightOnImageHead      = 0;
        int cHandRightOnHead           = 0;
        int cHandLeftOnImageHead       = 0;
        int cHandLeftOnHead            = 0;
        bool headOK                    = false;

        int cHandRightOnImageRightHand = 0;
        int cHandLeftOnImageRightHand  = 0;
        int cHandRightOnRightHand      = 0;
        int cHandLeftOnRightHand       = 0;
        bool rightHandOK               = false;

        int cHandRightOnImageLeftLeg  = 0;
        int cHandRightOnLeftLeg       = 0;
        int cHandLeftOnImageLeftLeg   = 0;
        int cHandLeftOnLeftLeg        = 0;
        bool leftLegOK                = false;

        DateTime dHandOnImage;

        SoundPlayer  headSound      = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\headSound.wav");
        SoundPlayer  rightHandSound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\rightHandSound.wav");
        SoundPlayer  leftLegSound   = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\leftLegSound.wav");
        SoundPlayer  finishSound    = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\finishSound.wav");

        [Serializable]
        public struct Vector2
        {
            public double X;
            public double Y;
        }

        public UI_GameSixPiecesCopy()
        {
            InitializeComponent();
            SoundPlayer letsPlaySound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\letsPlaySound.wav");
            letsPlaySound.Play();
        }

        private static UI_GameSixPieces level2;
        public static UI_GameSixPieces getInstance()
        {
            if (level2 == null)
            {
                level2 = new UI_GameSixPieces();
                level2.Show();
                level2.Activate();
            }
            return level2;
        }

        private static SoundPlayer cheersSound;
        public static SoundPlayer getCheersSound()
        {
            if (cheersSound == null)
            {
                cheersSound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\cheersSound.wav");
                cheersSound.Play();
            }
            return cheersSound;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            MainCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth;

            mKinect = new KinectSensorChooser();
            mKinect.KinectChanged += miKinect_KinectChanged; //Detects if the sensor is connected or not.
            sensorChooserUI.KinectSensorChooser = mKinect;
            mKinect.Start(); //Initialize the sensor.
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

            ellipseHandLeft.Visibility = Visibility.Visible; //Left hand
            ellipseHandRight.Visibility = Visibility.Visible; //Right hand

            #region HEAD_IMAGE
                //RIGHT HAND to the HEAD (image)
                if (handOnImage(imageHead, ellipseHandRight) && 
                    Canvas.GetLeft(imageHead) == 934 && 
                    Canvas.GetTop(imageHead) == 319) 
                {
                    cHandRightOnImageHead++;

                    //Right Hand
                    cHandRightOnImageRightHand = 0;
                    cHandRightOnImageLeftLeg  = 0;
                    //Left Hand
                    cHandLeftOnImageHead       = 0;
                    cHandLeftOnImageRightHand  = 0;
                    cHandLeftOnImageLeftLeg   = 0;
                }

                //LEFT HAND to the HEAD (image)
                if (handOnImage(imageHead, ellipseHandLeft) && 
                    Canvas.GetLeft(imageHead) == 934 && 
                    Canvas.GetTop(imageHead) == 319) 
                {
                    cHandLeftOnImageHead++;

                    //Right Hand
                    cHandRightOnImageRightHand = 0;
                    cHandRightOnImageLeftLeg  = 0;
                    cHandRightOnImageHead      = 0;
                    //Left Hand
                    cHandLeftOnImageRightHand  = 0;
                    cHandLeftOnImageLeftLeg   = 0;
                }
            #endregion

            #region TORSO_IMAGE
                
                #region RIGHT_HAND
                    if (handOnImage(imageRightHand, ellipseHandRight) && 
                        Canvas.GetLeft(imageRightHand) == 345 && 
                        Canvas.GetTop(imageRightHand) == 211) 
                    {
                        cHandRightOnImageRightHand++;

                        //Right Hand
                        cHandRightOnImageHead     = 0;
                        cHandRightOnImageLeftLeg = 0;
                                        
                        //Left Hand
                        cHandLeftOnImageHead      = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftLeg  = 0;

                    }

                    if (handOnImage(imageRightHand, ellipseHandLeft) && 
                        Canvas.GetLeft(imageRightHand) == 345 && 
                        Canvas.GetTop(imageRightHand) == 211) 
                    {
                        cHandLeftOnImageRightHand++;

                        //Right Hand
                        cHandRightOnImageHead      = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftLeg  = 0;
                 
                        //Left Hand
                        cHandLeftOnImageHead       = 0;
                        cHandLeftOnImageLeftLeg   = 0;
                    }
                #endregion

            #endregion

            #region LEGS_IMAGE

                #region LEFT_HAND
                    if (handOnImage(imageLeftLeg, ellipseHandRight) &&
                        Canvas.GetLeft(imageLeftLeg) == 186 &&
                        Canvas.GetTop(imageLeftLeg) == 536)
                    {
                        cHandRightOnImageLeftLeg++;

                        //Right Hand
                        cHandRightOnImageHead      = 0;
                        cHandRightOnImageRightHand = 0;

                        //Left Hand
                        cHandLeftOnImageHead      = 0;
                        cHandLeftOnImageRightHand = 0;
                        cHandLeftOnImageLeftLeg   = 0;

                    }

                    if (handOnImage(imageLeftLeg, ellipseHandLeft) &&
                        Canvas.GetLeft(imageLeftLeg) == 186 &&
                        Canvas.GetTop(imageLeftLeg) == 536)
                    {
                        cHandLeftOnImageLeftLeg++;

                        //Right Hand
                        cHandRightOnImageHead      = 0;
                        cHandRightOnImageRightHand = 0;
                        cHandRightOnImageLeftLeg   = 0;

                        //Left Hand
                        cHandLeftOnImageHead       = 0;
                        cHandLeftOnImageRightHand  = 0;
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
                DepthImagePoint hipLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint kneeLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeLeft].Position, DepthImageFormat.Resolution640x480Fps30);

                ColorImagePoint headColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, headDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint leftHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, leftHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint rightHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rightHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint elbowRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, elbowRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint kneeLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, kneeLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                CameraPosition(ellipseHead, headColorPoint, "HEAD");
                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND");
                CameraPosition(ellipseHandLeft, elbowRightColorPoint, "ELBOW RIGHT");
                CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP LEFT");
                CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE LEFT");

                #region HEAD
                    #region HEAD_HAND_RIGHT
                        if (cHandRightOnImageHead == 5) 
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHead, headColorPoint, "HEAD - cHandRightOnImageHead");
                            CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageHead");
                            headSound.Play();
                            imageHeadRed.Visibility =Visibility.Visible;
                        }

                        if (handOnImage(ellipseHandRight, ellipseHead) && cHandRightOnImageHead >= 5)
                        {
                            cHandRightOnHead++;
                        }

                        if (cHandRightOnHead == 5)
                        {
                               imageHeadRed.Visibility =Visibility.Hidden;

                            Canvas.SetLeft(imageHead, 540);
                            Canvas.SetTop(imageHead, 201);
                            getCheersSound();
                            headOK = true;

                            //RIGHT
                            cHandRightOnHead      = 0;
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftLeg  = 0;

                            //LEFT
                            cHandLeftOnHead       = 0;
                            cHandLeftOnRightHand  = 0;
                            cHandLeftOnLeftLeg   = 0;
                        }
                    #endregion
                    #region HEAD_LEFT_HAND
                        if (cHandLeftOnImageHead == 5) //MANO IZQUIERDA PASO 10 VECES
                        {
                            dHandOnImage = DateTime.Now;
                            CameraPosition(ellipseHead, headColorPoint, "HEAD - cHandRightOnImageHead");
                            CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND - cHandRightOnImageHead");
                            headSound.Play();
                            imageHeadRed.Visibility =Visibility.Visible;

                        }

                        if (handOnImage(ellipseHandLeft, ellipseHead) && cHandLeftOnImageHead >= 5)
                        {
                            cHandLeftOnHead++;
                        }

                        if (cHandLeftOnHead == 5)
                        {
                               imageHeadRed.Visibility =Visibility.Hidden;

                            Canvas.SetLeft(imageHead, 540);
                            Canvas.SetTop(imageHead, 201);
                            getCheersSound();
                            headOK = true;

                            //RIGHT
                            cHandRightOnHead      = 0;
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftLeg  = 0;

                            //LEFT
                            cHandLeftOnHead       = 0;
                            cHandLeftOnRightHand  = 0;
                            cHandLeftOnLeftLeg   = 0;
                        }
                    #endregion
                #endregion

                #region TORSO
                    #region RIGHT_HAND
                        #region RIGHT_HAND_RIGHT
                            if (cHandRightOnImageRightHand == 5) 
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageRightHand");
                                CameraPosition(ellipseElbowRight, elbowRightColorPoint, "SPINE");
                                rightHandSound.Play();
                                imageRightHandRed.Visibility =Visibility.Visible;

                            }

                            if (handOnImage(ellipseHandRight, ellipseElbowRight) && cHandRightOnImageRightHand >= 5)
                            {
                                cHandRightOnRightHand++;
                            }

                            if (cHandRightOnRightHand == 5)
                            {
                               imageRightHandRed.Visibility =Visibility.Hidden;

                                Canvas.SetLeft(imageRightHand, 714);
                                Canvas.SetTop(imageRightHand, 345);
                                getCheersSound();
                                rightHandOK = true;
                            //RIGHT
                            cHandRightOnHead      = 0;
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftLeg  = 0;

                            //LEFT
                            cHandLeftOnHead       = 0;
                            cHandLeftOnRightHand  = 0;
                            cHandLeftOnLeftLeg   = 0;
                            }
                        #endregion
                        #region RIGHT_HAND_LEFT
                            if (cHandLeftOnImageRightHand == 5) //MANO IZQUIERDA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageRightHand");
                                CameraPosition(ellipseElbowRight, elbowRightColorPoint, "SPINE");
                                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                                rightHandSound.Play();
                                imageRightHandRed.Visibility =Visibility.Visible;
              
                            }

                            if ((handOnImage(ellipseHandLeft, ellipseElbowRight) ||
                                handOnImage(ellipseHandLeft, ellipseHandRight)) 
                                && cHandLeftOnImageRightHand >= 5)
                            {
                                cHandLeftOnRightHand++;
                            }

                            if (cHandLeftOnRightHand == 5)
                            {
                               imageRightHandRed.Visibility =Visibility.Hidden;

                                Canvas.SetLeft(imageRightHand, 714);
                                Canvas.SetTop(imageRightHand, 344);
                                getCheersSound();
                                rightHandOK = true;
                            //RIGHT
                            cHandRightOnHead      = 0;
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftLeg  = 0;

                            //LEFT
                            cHandLeftOnHead       = 0;
                            cHandLeftOnRightHand  = 0;
                            cHandLeftOnLeftLeg   = 0;
                            }
                        #endregion
                    #endregion
                #endregion

                #region LEGS
                    #region LEFT_LEG
                        #region LEFT_LEG_RIGHT_HAND
                            if (cHandRightOnImageLeftLeg == 5) //MANO DERECHA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageLeftLeg");
                                CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP RIGHT");
                                CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE RIGHT");
                                leftLegSound.Play();
                               imageLeftLegRed.Visibility =Visibility.Visible;

                            }

                            if ((handOnImage(ellipseHandRight, ellipseHipLeft) ||
                                handOnImage(ellipseHandRight, ellipseKneeLeft))&&
                                cHandRightOnImageLeftLeg >= 5)
                            {
                                cHandRightOnLeftLeg++;
                            }

                            if (cHandRightOnLeftLeg == 5)
                            {
                               imageLeftLegRed.Visibility =Visibility.Hidden;

                                Canvas.SetLeft(imageLeftLeg, 517);
                                Canvas.SetTop(imageLeftLeg, 536);
                                getCheersSound();
                                leftLegOK = true;
                            //RIGHT
                            cHandRightOnHead      = 0;
                            cHandRightOnRightHand = 0;
                            cHandRightOnLeftLeg  = 0;

                            //LEFT
                            cHandLeftOnHead       = 0;
                            cHandLeftOnRightHand  = 0;
                            cHandLeftOnLeftLeg   = 0;
                            }
                        #endregion
                        #region LEFT_LEG_LEFT_HAND
                            if (cHandLeftOnImageLeftLeg == 5) //MANO IZQUIERDA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND - cHandLeftOnImageLeftLeg");
                                CameraPosition(ellipseHipLeft, hipLeftColorPoint, "HIP RIGHT");
                                CameraPosition(ellipseKneeLeft, kneeLeftColorPoint, "KNEE RIGHT");
                                leftLegSound.Play();
                               imageLeftLegRed.Visibility =Visibility.Visible;

                            }

                            if ((handOnImage(ellipseHandLeft, ellipseHipLeft) ||
                                handOnImage(ellipseHandLeft, ellipseKneeLeft)) &&
                                cHandLeftOnImageLeftLeg >= 5)
                            {
                                cHandLeftOnLeftLeg++;
                            }

                            if (cHandLeftOnLeftLeg == 5)
                            {
                               imageLeftLegRed.Visibility =Visibility.Hidden;

                                Canvas.SetLeft(imageLeftLeg, 517);
                                Canvas.SetTop(imageLeftLeg, 536);
                                cheersSound.Play();
                                leftLegOK = true;
                                //RIGHT
                                cHandRightOnHead      = 0;
                                cHandRightOnRightHand = 0;
                                cHandRightOnLeftLeg   = 0;

                                //LEFT
                                cHandLeftOnHead       = 0;
                                cHandLeftOnRightHand  = 0;
                                cHandLeftOnLeftLeg    = 0;
                            }
                #endregion
                #endregion
                #endregion

                #region NEXT_LEVEL
                if (headOK && rightHandOK && leftLegOK)
                {
                    ellipseHandLeft.Visibility = Visibility.Hidden;
                    ellipseHandRight.Visibility = Visibility.Hidden;
                    Canvas.SetLeft(imageHead, 591);
                    Canvas.SetTop(imageHead, 193);
                    Canvas.SetLeft(imageLeftLeg, 573);
                    Canvas.SetTop(imageLeftLeg, 523);
                    Canvas.SetLeft(imageRightHand, 540);
                    Canvas.SetTop(imageRightHand, 328);
                    soundNext = true;
                }
                #endregion

                if (soundNext)
                {
                    mKinect.Stop();
                    Thread.Sleep(3000);
                    UI_GameSixPiecesCopy2 nextLeve3 = UI_GameSixPiecesCopy2.getInstance();
                    this.Close();
                    //imageFinal.Visibility = Visibility.Visible;
                }

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
                return distance;
            }
            else
            {
                return distance;
            }
        }
    }
}

