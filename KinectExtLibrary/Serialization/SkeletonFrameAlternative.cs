using System;
using Microsoft.Research.Kinect.Nui;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public sealed class SkeletonFrameAlternative
    {
        public SkeletonFrameAlternative(SkeletonFrame frame)
        {
            FrameNumber = frame.FrameNumber;
            Skeletons = new SkeletonDataAlternative[frame.Skeletons.Length];
            int i = 0;
            foreach (SkeletonData data in frame.Skeletons)
            {
                Skeletons[i++] = new SkeletonDataAlternative(data);
            }
        }

        public int FrameNumber
        {
            get;
            set;
        }

        public SkeletonDataAlternative[] Skeletons
        {
            get;
            set;
        }
    }
}
