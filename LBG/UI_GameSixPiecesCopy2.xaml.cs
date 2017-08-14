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
    public partial class UI_GameSixPiecesCopy2 : Window
    {
        static KinectSensorChooser mKinect;
        const int                  PostureDetectionNumber = 100;
        bool                       closing                = false;
        const int                  skeletonCount          = 6;
        Skeleton[]                 allSkeletons           = new Skeleton[skeletonCount];
        bool soundNext = false;
        int cHandRightOnImageSpine     = 0;
        int cHandLeftOnImageSpine      = 0;
        int cHandRightOnSpine          = 0;
        int cHandLeftOnSpine           = 0;
        bool spineOK                   = false;

        int cHandRightOnImageLeftHand  = 0;
        int cHandLeftOnImageLeftHand   = 0;
        int cHandRightOnLeftHand       = 0;
        int cHandLeftOnLeftHand        = 0;
        bool leftHandOK                = false;

        int cHandRightOnImageRightLeg  = 0;
        int cHandRightOnRightLeg       = 0;
        int cHandLeftOnImageRightLeg   = 0;
        int cHandLeftOnRightLeg        = 0;
        bool rightLegOK                = false;

        DateTime dHandOnImage;

        SoundPlayer  spineSound     = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\spineSound.wav");
        SoundPlayer  leftHandSound  = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\leftHandSound.wav");
        SoundPlayer  rightLegSound  = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\rightLegSound.wav");

        [Serializable]
        public struct Vector2
        {
            public double X;
            public double Y;
        }

        public UI_GameSixPiecesCopy2()
        {
            InitializeComponent();
            SoundPlayer letsPlaySound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\letsPlaySound.wav");
            letsPlaySound.Play();
        }

        private static UI_GameSixPiecesCopy2 level2;
        public static UI_GameSixPiecesCopy2 getInstance()
        {
            if (level2 == null)
            {
                level2 = new UI_GameSixPiecesCopy2();
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

            #region TORSO_IMAGE
                #region SPINE
                //RIGHT HAND to the SPINE (image)
                    if (handOnImage(imageTorsoWithoutHands, ellipseHandRight) && 
                        Canvas.GetLeft(imageTorsoWithoutHands) == 186 && 
                        Canvas.GetTop(imageTorsoWithoutHands) == 301) 
                    {
                        cHandRightOnImageSpine++;

                        //Right Hand
                        cHandRightOnImageLeftHand  = 0;
                        cHandRightOnImageRightLeg  = 0;
                        //Left Hand
                        cHandLeftOnImageLeftHand   = 0;
                        cHandLeftOnImageRightLeg   = 0;
                        cHandLeftOnImageSpine      = 0;

                    }
                    //LEFT HAND to the SPINE (image)
                    if (handOnImage(imageTorsoWithoutHands, ellipseHandLeft) && 
                        Canvas.GetLeft(imageTorsoWithoutHands) == 186 && 
                        Canvas.GetTop(imageTorsoWithoutHands) == 301) 
                    {
                        cHandLeftOnImageSpine++;

                        //Right Hand
                        cHandRightOnImageLeftHand  = 0;
                        cHandRightOnImageRightLeg  = 0;
                        cHandRightOnImageSpine     = 0;      
                        //Left Hand
                        cHandLeftOnImageLeftHand   = 0;
                        cHandLeftOnImageRightLeg   = 0;
                    }
                #endregion
                
                #region LEFT_HAND
                    if (handOnImage(imageLeftHand, ellipseHandRight) && 
                        Canvas.GetLeft(imageLeftHand) == 1040 && 
                        Canvas.GetTop(imageLeftHand) == 460) 
                    {
                        cHandRightOnImageLeftHand++;

                        //Right Hand
                        cHandRightOnImageSpine     = 0;
                        cHandRightOnImageRightLeg  = 0;
                                        
                        //Left Hand
                        cHandLeftOnImageLeftHand  = 0;
                        cHandLeftOnImageRightLeg  = 0;
                        cHandLeftOnImageSpine     = 0;

                    }

                    if (handOnImage(imageLeftHand, ellipseHandLeft) && 
                        Canvas.GetLeft(imageLeftHand) == 1040 && 
                        Canvas.GetTop(imageLeftHand) == 460)  
                    {
                        cHandLeftOnImageLeftHand++;

                        //Right Hand
                        cHandRightOnImageLeftHand  = 0;
                        cHandRightOnImageRightLeg  = 0;
                        cHandRightOnImageSpine     = 0;
                 
                        //Left Hand
                        cHandLeftOnImageSpine     = 0;
                        cHandLeftOnImageRightLeg  = 0;
                    }
            #endregion
            #endregion

            #region LEGS_IMAGE
                #region RIGHT_LEG
                    if (handOnImage(imageRightLeg, ellipseHandRight) &&
                        Canvas.GetLeft(imageRightLeg) == 905 &&
                        Canvas.GetTop(imageRightLeg) == 206)
                    {
                        cHandRightOnImageRightLeg++;

                        //Right Hand
                        cHandRightOnImageSpine     = 0;
                        cHandRightOnImageLeftHand  = 0;

                        //Left Hand
                        cHandLeftOnImageLeftHand   = 0;
                        cHandLeftOnImageRightLeg   = 0;
                        cHandLeftOnImageSpine      = 0;

                    }

                    if (handOnImage(imageRightLeg, ellipseHandLeft) &&
                        Canvas.GetLeft(imageRightLeg) == 905 &&
                        Canvas.GetTop(imageRightLeg) == 206)
                    {
                        cHandLeftOnImageRightLeg++;

                        //Right Hand
                        cHandRightOnImageLeftHand  = 0;
                        cHandRightOnImageRightLeg  = 0;
                        cHandRightOnImageSpine     = 0;

                        //Left Hand
                        cHandLeftOnImageSpine      = 0;
                        cHandLeftOnImageLeftHand   = 0;
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

                DepthImagePoint leftHandDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint rightHandDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint elbowLeftDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint spineDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint hipRightDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint kneeRightDepthPoint = mKinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeRight].Position, DepthImageFormat.Resolution640x480Fps30);

                ColorImagePoint leftHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, leftHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint rightHandColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rightHandDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint elbowLeftColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, elbowLeftDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint spineColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, spineDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint hipRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, hipRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);
                ColorImagePoint kneeRightColorPoint = mKinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, kneeRightDepthPoint, ColorImageFormat.RgbResolution1280x960Fps12);

                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND");
                CameraPosition(ellipseHandRight, elbowLeftColorPoint, "ELBOW LEFT");
                CameraPosition(ellipseSpine, spineColorPoint, "SPINE");
                CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");

                #region TORSO
                    #region SPINE
                        #region SPINE_HAND_RIGHT
                            if (cHandRightOnImageSpine == 5) //MANO DERECHA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageSpine");
                                CameraPosition(ellipseSpine, spineColorPoint, "SPINE");
                                spineSound.Play();
                                imageTorsoWithoutHandsRed.Visibility = Visibility.Visible;
                            }

                            if (handOnImage(ellipseHandRight, ellipseSpine) && cHandRightOnImageSpine >= 5)
                            {
                                cHandRightOnSpine++;
                            }

                            if (cHandRightOnSpine == 5)
                            {
                                imageTorsoWithoutHandsRed.Visibility = Visibility.Hidden;

                                Canvas.SetLeft(imageTorsoWithoutHands, 568);
                                Canvas.SetTop(imageTorsoWithoutHands, 394);
                                getCheersSound();
                                spineOK = true;
                                //RIGHT
                                cHandRightOnLeftHand  = 0;
                                cHandRightOnSpine     = 0;
                                cHandRightOnRightLeg  = 0;

                                //LEFT
                                cHandLeftOnLeftHand   = 0;
                                cHandLeftOnSpine      = 0;
                                cHandLeftOnRightLeg   = 0;
                            }
                        #endregion
                        #region SPINE_HAND_LEFT
                            if (cHandLeftOnImageSpine == 5) //MANO DERECHA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND - cHandLeftOnImageSpine");
                                CameraPosition(ellipseSpine, spineColorPoint, "SPINE");
                                spineSound.Play();
                                imageTorsoWithoutHandsRed.Visibility = Visibility.Visible;

                            }

                            if (handOnImage(ellipseHandLeft, ellipseSpine) &&
                                cHandLeftOnImageSpine >= 5)
                            {
                                cHandLeftOnSpine++;
                            }

                            if (cHandLeftOnSpine == 5)
                            {
                                imageTorsoWithoutHandsRed.Visibility = Visibility.Hidden;

                                Canvas.SetLeft(imageTorsoWithoutHands, 568);
                                Canvas.SetTop(imageTorsoWithoutHands, 394);
                                getCheersSound();
                                spineOK = true;

                                //RIGHT
                                cHandRightOnLeftHand  = 0;
                                cHandRightOnSpine     = 0;
                                cHandRightOnRightLeg  = 0;

                                //LEFT
                                cHandLeftOnLeftHand   = 0;
                                cHandLeftOnSpine      = 0;
                                cHandLeftOnRightLeg   = 0;
                            }
                        #endregion
                    #endregion
                    #region LEFT_HAND
                        #region LEFT_HAND_RIGHT
                            if (cHandRightOnImageLeftHand == 5) //MANO IZQUIERDA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND - cHandRightOnImageLeftHand");
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageLeftHand");
                                CameraPosition(ellipseElbowLeft, elbowLeftColorPoint, "SPINE");
                                leftHandSound.Play();
                                imageLeftHandRed.Visibility = Visibility.Visible;

                            }

                            if ((handOnImage(ellipseHandRight, ellipseElbowLeft) ||
                                 handOnImage(ellipseHandRight, ellipseHandLeft)) 
                                 && cHandRightOnImageLeftHand >= 5)
                            {
                                cHandRightOnLeftHand++;
                            }

                            if (cHandRightOnLeftHand == 5)
                            {
                                imageLeftHandRed.Visibility = Visibility.Hidden;

                                Canvas.SetLeft(imageLeftHand, 488);
                                Canvas.SetTop(imageLeftHand, 329);
                                getCheersSound();
                                leftHandOK = true;
                                //RIGHT
                                cHandRightOnLeftHand  = 0;
                                cHandRightOnSpine     = 0;
                                cHandRightOnRightLeg  = 0;

                                //LEFT
                                cHandLeftOnLeftHand   = 0;
                                cHandLeftOnSpine      = 0;
                                cHandLeftOnRightLeg   = 0;
                            }
                        #endregion
                        #region LEFT_HAND_LEFT
                            if (cHandLeftOnImageLeftHand == 5) //MANO IZQUIERDA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseElbowLeft, elbowLeftColorPoint, "SPINE");
                                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND");
                                leftHandSound.Play();
                                imageLeftHandRed.Visibility = Visibility.Visible;

                            }

                            if (handOnImage(ellipseHandLeft, ellipseElbowLeft) 
                                && cHandLeftOnImageLeftHand >= 5)
                            {
                                cHandLeftOnLeftHand++;
                            }

                            if (cHandLeftOnLeftHand == 5)
                            {
                                imageLeftHandRed.Visibility = Visibility.Hidden;

                                Canvas.SetLeft(imageLeftHand, 488);
                                Canvas.SetTop(imageLeftHand, 329);
                                getCheersSound();
                                leftHandOK = true;
                                //RIGHT
                                cHandRightOnLeftHand  = 0;
                                cHandRightOnSpine     = 0;
                                cHandRightOnRightLeg  = 0;

                                //LEFT
                                cHandLeftOnLeftHand   = 0;
                                cHandLeftOnSpine      = 0;
                                cHandLeftOnRightLeg   = 0;
                            }
                #endregion
                #endregion
                #endregion

                #region LEGS
                    #region RIGHT_LEG
                        #region RIGHT_LEG_RIGHT_HAND
                            if (cHandRightOnImageRightLeg == 5) //MANO DERECHA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandRight, rightHandColorPoint, "RIGHT HAND - cHandRightOnImageRightLeg");
                                CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                                CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                                rightLegSound.Play();
                                imageRightLegRed.Visibility = Visibility.Visible;

                            }

                            if ((handOnImage(ellipseHandRight, ellipseHipRight) ||
                                handOnImage(ellipseHandRight, ellipseKneeRight))&&
                                cHandRightOnImageRightLeg >= 5)
                            {
                                cHandRightOnRightLeg++;
                            }

                            if (cHandRightOnRightLeg == 5)
                            {
                                imageRightLegRed.Visibility = Visibility.Hidden;

                                Canvas.SetLeft(imageRightLeg, 648);
                                Canvas.SetTop(imageRightLeg, 512);
                                getCheersSound();
                                rightLegOK = true;
                                //RIGHT
                                cHandRightOnLeftHand  = 0;
                                cHandRightOnSpine     = 0;
                                cHandRightOnRightLeg  = 0;

                                //LEFT
                                cHandLeftOnLeftHand   = 0;
                                cHandLeftOnSpine      = 0;
                                cHandLeftOnRightLeg   = 0;
                            }
                        #endregion
                        #region RIGHT_LEG_LEFT_HAND
                            if (cHandLeftOnImageRightLeg == 5) //MANO IZQUIERDA PASO 10 VECES
                            {
                                dHandOnImage = DateTime.Now;
                                CameraPosition(ellipseHandLeft, leftHandColorPoint, "LEFT HAND - cHandLeftOnImageLeftLeg");
                                CameraPosition(ellipseHipRight, hipRightColorPoint, "HIP RIGHT");
                                CameraPosition(ellipseKneeRight, kneeRightColorPoint, "KNEE RIGHT");
                                rightLegSound.Play();
                                imageRightLegRed.Visibility = Visibility.Visible;

                            }

                            if ((handOnImage(ellipseHandLeft, ellipseHipRight) ||
                                handOnImage(ellipseHandLeft, ellipseKneeRight)) &&
                                cHandLeftOnImageRightLeg >= 5)
                            {
                                cHandLeftOnRightLeg++;
                            }

                            if (cHandLeftOnRightLeg == 5)
                            {
                                imageRightLegRed.Visibility = Visibility.Hidden;

                                Canvas.SetLeft(imageRightLeg, 648);
                                Canvas.SetTop(imageRightLeg, 512);
                                cheersSound.Play();
                                
                                //RIGHT
                                cHandRightOnLeftHand  = 0;
                                cHandRightOnSpine     = 0;
                                cHandRightOnRightLeg  = 0;

                                //LEFT
                                cHandLeftOnLeftHand   = 0;
                                cHandLeftOnSpine      = 0;
                                cHandLeftOnRightLeg   = 0;
                            }
                        #endregion
                    #endregion
                #endregion

                #region NEXT_LEVEL
                    if(spineOK && leftHandOK && rightLegOK )
                {
                    ellipseHandLeft.Visibility = Visibility.Hidden;
                    ellipseHandRight.Visibility = Visibility.Hidden;
                    Canvas.SetLeft(imageTorsoWithoutHands, 568);
                    Canvas.SetTop(imageTorsoWithoutHands, 394);
                    Canvas.SetLeft(imageRightLeg, 648);
                    Canvas.SetTop(imageRightLeg, 512);
                    Canvas.SetLeft(imageLeftHand, 488);
                    Canvas.SetTop(imageLeftHand, 329);
                    soundNext = true;
                }
                #endregion

                if (soundNext)
                {
                    mKinect.Stop();
                    Thread.Sleep(3000);
                    image1.Visibility = Visibility.Hidden;
                    imageFinal.Visibility = Visibility.Visible;
                    SoundPlayer finishSound = new SoundPlayer(@"D:\Documents\Visual Studio 2015\Projects\LBG\LBG\Sounds\finishSound.wav");
                    finishSound.Play();
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

