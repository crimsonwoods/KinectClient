using System;
using System.Collections.Generic;
using Microsoft.Research.Kinect.Nui;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    internal static class BinaryStreamWriterExtension
    {
        public static void Write(this BinaryStreamWriter writer, Vector value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryStreamWriter writer, Joint value)
        {
            writer.Write(jointIDTable[value.ID]);
            writer.Write(jointTrackingStateTable[value.TrackingState]);
            writer.Write(value.Position);
        }

        public static void Write(this BinaryStreamWriter writer, SkeletonTrackingState value)
        {
            writer.Write(skeletonTrackingStateTable[value]);
        }

        public static void Write(this BinaryStreamWriter writer, SkeletonQuality value)
        {
            int flags = 0;
            if (value.HasFlag(SkeletonQuality.ClippedRight))
            {
                flags |= 0x01;
            }
            if (value.HasFlag(SkeletonQuality.ClippedLeft))
            {
                flags |= 0x02;
            }
            if (value.HasFlag(SkeletonQuality.ClippedTop))
            {
                flags |= 0x04;
            }
            if (value.HasFlag(SkeletonQuality.ClippedBottom))
            {
                flags |= 0x08;
            }
            writer.Write(flags);
        }

        private static readonly Dictionary<JointID, int> jointIDTable = new Dictionary<JointID, int>()
        {
            { JointID.AnkleLeft,       1 },
            { JointID.AnkleRight,      2 },
            { JointID.ElbowLeft,       3 },
            { JointID.ElbowRight,      4 },
            { JointID.FootLeft,        5 },
            { JointID.FootRight,       6 },
            { JointID.HandLeft,        7 },
            { JointID.HandRight,       8 },
            { JointID.Head,            9 },
            { JointID.HipCenter,      10 },
            { JointID.HipLeft,        11 },
            { JointID.HipRight,       12 },
            { JointID.KneeLeft,       13 },
            { JointID.KneeRight,      14 },
            { JointID.ShoulderCenter, 15 },
            { JointID.ShoulderLeft,   16 },
            { JointID.ShoulderRight,  17 },
            { JointID.Spine,          18 },
            { JointID.WristLeft,      19 },
            { JointID.WristRight,     20 },
        };

        private static readonly Dictionary<JointTrackingState, int> jointTrackingStateTable = new Dictionary<JointTrackingState, int>()
        {
            { JointTrackingState.Inferred,   1 },
            { JointTrackingState.NotTracked, 2 },
            { JointTrackingState.Tracked,    3 },
        };

        private static readonly Dictionary<SkeletonTrackingState, int> skeletonTrackingStateTable = new Dictionary<SkeletonTrackingState, int>()
        {
            { SkeletonTrackingState.NotTracked,   1 },
            { SkeletonTrackingState.PositionOnly, 2 },
            { SkeletonTrackingState.Tracked,      3 },
        };
    }
}
