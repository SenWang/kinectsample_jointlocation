using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace WpfApplication1
{

    public partial class ColorWindow : Window
    {
        KinectSensor kinect;
        public ColorWindow(KinectSensor sensor) : this()
        {
            kinect = sensor;
        }

        public ColorWindow()
        {
            InitializeComponent();
            Loaded += ColorWindow_Loaded;
            Unloaded += ColorWindow_Unloaded;
        }
        void ColorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                kinect.ColorStream.Disable();
                kinect.SkeletonStream.Disable();
                kinect.Stop();
                kinect.ColorFrameReady -= myKinect_ColorFrameReady;
                kinect.SkeletonFrameReady -= mykinect_SkeletonFrameReady;
            }
        }
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;

        void ColorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                #region 繪圖資源初始化
                ColorImageStream colorStream = kinect.ColorStream;

                _ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,colorStream.FrameHeight, 96, 96,PixelFormats.Bgr32, null);
                _ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,colorStream.FrameHeight);
                _ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorData.Source = _ColorImageBitmap;
                #endregion

                kinect.ColorStream.Enable();
                kinect.ColorFrameReady += myKinect_ColorFrameReady;
                kinect.SkeletonStream.Enable();
                kinect.SkeletonFrameReady += mykinect_SkeletonFrameReady;
                kinect.Start();
            }
        }

        void mykinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skframe = e.OpenSkeletonFrame())
            {
                if (skframe == null)
                    return;

                #region 選擇骨架
                Skeleton[] FrameSkeletons = new Skeleton[skframe.SkeletonArrayLength];
                skframe.CopySkeletonDataTo(FrameSkeletons);

                Skeleton user = (from sk in FrameSkeletons
                                where sk.TrackingState == SkeletonTrackingState.Tracked
                                select sk).FirstOrDefault();
                #endregion

                #region 取出手掌左標
                if (user != null)
                {
                        Joint jpl = user.Joints[JointType.HandLeft];
                        LeftHandMessage.Text = "追蹤到左手 X=" + jpl.Position.X + 
                            ", Y=" + jpl.Position.Y + ", Z=" + jpl.Position.Z;
                        Joint jpr = user.Joints[JointType.HandRight];
                        RightHandMessage.Text = "追蹤到右手 X=" + jpr.Position.X + 
                            ", Y=" + jpr.Position.Y + ", Z=" + jpr.Position.Z;
                }
                #endregion
            }
        }

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    _ColorImageBitmap.WritePixels(_ColorImageBitmapRect, pixelData,_ColorImageStride, 0);
                }
            }
        }
    }
}
