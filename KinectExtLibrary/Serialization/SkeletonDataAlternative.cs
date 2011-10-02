using System;
using System.Collections.Generic;
using Microsoft.Research.Kinect.Nui;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public delegate Joint JointPositionConverter(Joint joint);
    public delegate Vector SkeletonDataPositionConverter(Vector position);

    public sealed class SkeletonDataAlternative
    {
        private static SkeletonDataPositionConverter skeletonDataPositionConverter = delegate(Vector position)
        {
            return position;
        };

        private static JointPositionConverter jointPositionConverter = delegate(Joint joint)
        {
            return joint;
        };

        public static SkeletonDataPositionConverter SkeletonDataPositionConverter
        {
            get
            {
                return skeletonDataPositionConverter;
            }
            set
            {
                skeletonDataPositionConverter = value;
            }
        }

        public static JointPositionConverter JointPositionConverter
        {
            get
            {
                return jointPositionConverter;
            }
            set
            {
                jointPositionConverter = value;
            }
        }

        public SkeletonDataAlternative(SkeletonData data)
        {
            Joints = new List<Joint>();
            foreach (Joint j in data.Joints)
            {
                Joints.Add(JointPositionConverter(j));
            }
            Position = SkeletonDataPositionConverter(data.Position);
            Quality = data.Quality;
            TrackingID = data.TrackingID;
            TrackingState = data.TrackingState;
            UserIndex = data.UserIndex;
        }

        public Vector Position
        {
            get;
            set;
        }

        public int TrackingID
        {
            get;
            set;
        }

        public int UserIndex
        {
            get;
            set;
        }

        public SkeletonTrackingState TrackingState
        {
            get;
            set;
        }

        public ICollection<Joint> Joints
        {
            get;
            set;
        }

        public SkeletonQuality Quality
        {
            get;
            set;
        }
    }
}
