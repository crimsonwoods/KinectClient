using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public class UnknownSkeletonTrackingStateException : KinectExtLibraryException
    {
        public UnknownSkeletonTrackingStateException()
            : base()
        {
        }

        public UnknownSkeletonTrackingStateException(SkeletonTrackingState s)
            : base("Unknown \"SkeletonTrackingState\" value = " + s.ToString())
        {
        }

        public UnknownSkeletonTrackingStateException(string message)
            : base(message)
        {
        }

        public UnknownSkeletonTrackingStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
