using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using Microsoft.Research.Kinect.Nui;

namespace KinectClient
{
    using crimsonwoods.windows.library.KinectExtLibrary;

    public class KinectClientThread
    {
        private Thread thread;
        private Queue<SkeletonFrameAlternative> skeletonFrameQueue = new Queue<SkeletonFrameAlternative>();
        private bool isStopRequested = false;

        public KinectClientThread()
        {
            thread = new Thread(new ParameterizedThreadStart(ThreadProc));
            thread.Name = "KinectClientThread";
        }

        public void Run(string hostName, int portNumber)
        {
            thread.Start(new ThreadParam(hostName, portNumber));
        }

        public void Stop()
        {
            lock (this)
            {
                isStopRequested = true;
            }
        }

        public void Join()
        {
            thread.Join();
        }

        public void EnqueueSkeletonFrame(SkeletonFrame frame)
        {
            lock (skeletonFrameQueue)
            {
                skeletonFrameQueue.Enqueue(new SkeletonFrameAlternative(frame));
            }
        }

        private SkeletonFrameAlternative DequeueSkeletonFrame()
        {
            lock (skeletonFrameQueue)
            {
                return skeletonFrameQueue.Dequeue();
            }
        }

        private int GetSkeletonDataCount()
        {
            lock (skeletonFrameQueue)
            {
                return skeletonFrameQueue.Count;
            }
        }

        private void ThreadProc(Object arg)
        {
            ThreadParam param = (ThreadParam)arg;

            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(param.HostName, param.PortNumber);
                Invoke(Connected, new KinectClientEventArgs());

                for (; !isStopRequested; )
                {
                    if (!tcpClient.Connected)
                    {
                        break;
                    }
                    if (0 == GetSkeletonDataCount())
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    SkeletonFrameAlternative frame = DequeueSkeletonFrame();
                    KinectProtocolNotification notification = new KinectProtocolNotification(frame);
                    NetworkStream s = tcpClient.GetStream();
                    byte[] packet = notification.CreatePacket();
                    s.Write(packet, 0, packet.Length);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex);
#endif
                Console.Instance.WriteLine(ex);
            }
            finally
            {
                tcpClient.Close();
                Invoke(Disconnected, new KinectClientEventArgs());
            }
        }

        private class ThreadParam
        {
            public ThreadParam(string hostName, int portNumber)
            {
                HostName = hostName;
                PortNumber = portNumber;
            }

            public string HostName
            {
                get;
                private set;
            }

            public int PortNumber
            {
                get;
                private set;
            }
        }

        private void Invoke(KinectClientEventHandler handler, KinectClientEventArgs e)
        {
            if (null == handler)
            {
                return;
            }
            Delegate[] d = handler.GetInvocationList();
            foreach (KinectClientEventHandler h in d)
            {
                h(this, e);
            }
        }

        public delegate void KinectClientEventHandler(object sender, KinectClientEventArgs e);

        public class KinectClientEventArgs : EventArgs
        {
            public KinectClientEventArgs()
                : base()
            {
            }
        }

        public event KinectClientEventHandler Connected;
        public event KinectClientEventHandler Disconnected;
    }
}
