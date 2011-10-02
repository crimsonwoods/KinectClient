/////////////////////////////////////////////////////////////////////////
//
// This module contains code to do Kinect NUI initialization and
// processing and also to display NUI streams on screen.
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Research.Kinect.Nui;
using System.IO;
using System.Net.Sockets;
using System.Configuration;

namespace KinectClient
{
    using crimsonwoods.windows.library.KinectExtLibrary;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Console.createInstance(ConsoleOutput);

            hostAddress.Text = Properties.Settings.Default.hostAddress;
            portNumber.Text = Convert.ToString(Properties.Settings.Default.portNumber);
            
            hostAddress.TextChanged += delegate
            {
                Properties.Settings.Default.hostAddress = hostAddress.Text;
                Properties.Settings.Default.Save();
            };
            portNumber.TextChanged += delegate
            {
                Properties.Settings.Default.portNumber = Convert.ToInt32(portNumber.Text);
                Properties.Settings.Default.Save();
            };
        }

        Runtime nui;
        int totalFrames = 0;
        int lastFrames = 0;
        DateTime lastTime = DateTime.MaxValue;
        Brush[] skeletonBrushes = new Brush[] {
            new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            new SolidColorBrush(Color.FromRgb(0, 255, 0)),
            new SolidColorBrush(Color.FromRgb(64, 255, 255)),
            new SolidColorBrush(Color.FromRgb(255, 255, 64)),
            new SolidColorBrush(Color.FromRgb(255, 64, 255)),
            new SolidColorBrush(Color.FromRgb(128, 128, 255)),
        };
        Polyline[] polyLines = new Polyline[] {
            new Polyline(),
            new Polyline(),
            new Polyline(),
            new Polyline(),
            new Polyline(),
        };
        PointCollection[] pointCollections = new PointCollection[] {
            new PointCollection(5),
            new PointCollection(5),
            new PointCollection(5),
            new PointCollection(5),
            new PointCollection(5),
        };
        KinectClientThread kinectClientThread = null;
        
        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.
        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;
        byte[] depthFrame32 = new byte[320 * 240 * 4];
        
        
        Dictionary<JointID,Brush> jointColors = new Dictionary<JointID,Brush>() { 
            {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
            {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
            {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
            {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
            {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
            {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
            {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
            {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
            {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
            {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
        };

        private void Window_Loaded(object sender, EventArgs e)
        {
            nui = new Runtime();

            try
            {
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }


            try
            {
                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            lastTime = DateTime.Now;

            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);

            SkeletonDataAlternative.JointPositionConverter = new JointPositionConverter(delegate(Joint joint)
            {
                float depthX, depthY;
                nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
                ImageViewArea iv = new ImageViewArea();
                int colorX, colorY;
                nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)(depthX * 320), (int)(depthY * 240), (short)0, out colorX, out colorY);
                Microsoft.Research.Kinect.Nui.Vector converted = new Microsoft.Research.Kinect.Nui.Vector();
                converted.X = (float)colorX / 640.0f;
                converted.Y = (float)colorY / 480.0f;
                converted.Z = joint.Position.Z;
                converted.W = joint.Position.W;
                joint.Position = converted;
                return joint;
            });
        }

        private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = depthX * 320; //convert to 320, 240 space
            depthY = depthY * 240; //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(skeleton.Width * colorX / 640.0), (int)(skeleton.Height * colorY / 480.0));
        }

        Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, int index, params JointID[] ids)
        {
            PointCollection points = pointCollections[index];

            points.Clear();

            for (int i = 0; i < ids.Length; ++i )
            {
                points.Add(getDisplayPosition(joints[ids[i]]));
            }

            Polyline polyline = polyLines[index];
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;

            lock (this)
            {
                if (null != kinectClientThread)
                {
                    kinectClientThread.EnqueueSkeletonFrame(skeletonFrame);
                }
            }

            skeleton.Children.Clear();
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = skeletonBrushes[iSkeleton % skeletonBrushes.Length];
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, 0, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, 1, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, 2, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, 3, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, 4, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));
                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        Point jointPos = getDisplayPosition(joint);
                        Line jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = jointColors[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeleton.Children.Add(jointLine);
                    }
                }
                iSkeleton++;
            } // for each skeleton

            ++totalFrames;

            DateTime cur = DateTime.Now;
            if (cur.Subtract(lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = totalFrames - lastFrames;
                lastFrames = totalFrames;
                lastTime = cur;
                frameRate.Text = frameDiff.ToString() + " fps";
            }
        }

        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            // 32-bit per pixel, RGBA image
            PlanarImage Image = e.ImageFrame.Image;
            video.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
            Environment.Exit(0);
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                if (!buttonConnect.IsEnabled)
                {
                    return;
                }
                kinectClientThread = new KinectClientThread();
                buttonConnect.IsEnabled = false;
                buttonDisconnect.IsEnabled = true;
            }
            kinectClientThread.Disconnected += delegate
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    buttonConnect.IsEnabled = true;
                    buttonDisconnect.IsEnabled = false;
                }));
            };
            kinectClientThread.Run(hostAddress.Text, int.Parse(portNumber.Text));
        }

        private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            KinectClientThread thread = null;
            lock (this)
            {
                thread = kinectClientThread;
            }
            if (null != thread)
            {
                thread.Stop();
            }
        }
    }
}
