using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public class KinectExtLibraryException : ApplicationException
    {
        public KinectExtLibraryException()
            : base()
        {
        }

        public KinectExtLibraryException(string message)
            : base(message)
        {
        }

        public KinectExtLibraryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
