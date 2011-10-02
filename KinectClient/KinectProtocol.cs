using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.IO;

namespace KinectClient
{
    using crimsonwoods.windows.library.KinectExtLibrary;

    public enum KinectProtocolCommandType : int
    {
        Request = 1,
        Reply,
        Notification,
    }

    public class KinectProtocolHeader
    {
        public KinectProtocolHeader()
        {
        }

        public KinectProtocolCommandType CommandType
        {
            get;
            set;
        }

        public int BodyLength
        {
            get;
            set;
        }

        public byte[] CreateHeader()
        {
            byte[] bytes = null;
            using(BinaryStreamWriter w = new BinaryStreamWriter(new MemoryStream())){
                w.Write((int)CommandType);
                w.Write(BodyLength);
                bytes = ((MemoryStream)w.Stream).ToArray();
                w.Close();
            }
            return bytes;
        }
    }

    public class KinectProtocolBase
    {
        protected KinectProtocolBase(KinectProtocolCommandType commandType)
        {
            Header = new KinectProtocolHeader();
            Header.CommandType = commandType;
        }

        public KinectProtocolHeader Header
        {
            get;
            set;
        }

        protected virtual byte[] CreateBody()
        {
            return null;
        }

        public byte[] CreatePacket()
        {
            byte[] packet = null;
            using (MemoryStream s = new MemoryStream())
            {
                byte[] body = CreateBody();
                if (null == body)
                {
                    Header.BodyLength = 0;
                    byte[] header = Header.CreateHeader();
                    s.Write(header, 0, header.Length);
                }
                else
                {
                    Header.BodyLength = body.Length;
                    byte[] header = Header.CreateHeader();
                    s.Write(header, 0, header.Length);
                    s.Write(body, 0, body.Length);
                }
                packet = s.ToArray();
                s.Close();
            }
            return packet;
        }
    }

    public class KinectProtocolNotification : KinectProtocolBase
    {
        private SkeletonFrameAlternative skeletonFrame;
        private byte[] rawData;

        public KinectProtocolNotification(SkeletonFrameAlternative frame)
            : base(KinectProtocolCommandType.Notification)
        {
            skeletonFrame = frame;
            rawData = null;
        }

        public KinectProtocolNotification(byte[] data)
            : base(KinectProtocolCommandType.Notification)
        {
            skeletonFrame = null;
            rawData = data;
        }

        public KinectProtocolNotification(byte[] data, int offset)
            : base(KinectProtocolCommandType.Notification)
        {
            skeletonFrame = null;
            rawData = new byte[data.Length - offset];
            data.CopyTo(rawData, 0);
        }

        protected override byte[] CreateBody()
        {
            if (null == rawData)
            {
                rawData = SkeletonFrameSerializer.Serialize(skeletonFrame);
            }
            return rawData;
        }
    }
}
