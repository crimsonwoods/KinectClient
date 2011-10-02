using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Research.Kinect.Nui;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public static class SkeletonDataSerializer
    {
        public static byte[] Serialize(SkeletonDataAlternative data)
        {
            byte[] bytes = null;
            using (BinaryStreamWriter w = new BinaryStreamWriter(new MemoryStream()))
            {
                w.Write(data.TrackingID);
                w.Write(data.UserIndex);
                w.Write(data.TrackingState);
                w.Write(data.Quality);
                switch (data.TrackingState)
                {
                    case SkeletonTrackingState.Tracked:
                        w.Write(data.Position);
                        w.Write(data.Joints.Count);
                        foreach (Joint joint in data.Joints)
                        {
                            w.Write(joint);
                        }
                        break;
                    case SkeletonTrackingState.NotTracked:
                        break;
                    case SkeletonTrackingState.PositionOnly:
                        w.Write(data.Position);
                        break;
                    default:
                        throw new UnknownSkeletonTrackingStateException(data.TrackingState);
                }
                bytes = ((MemoryStream)w.Stream).ToArray();
                w.Close();
            }
            return bytes;
        }

        public static void Serialize(TextWriter writer, SkeletonDataAlternative data)
        {
            writer.Write(data.TrackingID);
            writer.Write(data.UserIndex);
            writer.Write(data.TrackingState);
            writer.Write(data.Quality);
            switch (data.TrackingState)
            {
                case SkeletonTrackingState.Tracked:
                    writer.Write(data.Position);
                    writer.Write(data.Joints.Count);
                    foreach (Joint joint in data.Joints)
                    {
                        writer.Write(joint);
                    }
                    break;
                case SkeletonTrackingState.NotTracked:
                    break;
                case SkeletonTrackingState.PositionOnly:
                    writer.Write(data.Position);
                    break;
                default:
                    throw new UnknownSkeletonTrackingStateException(data.TrackingState);
            }
        }
    }
}
