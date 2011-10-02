using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public static class SkeletonFrameSerializer
    {
        public static byte[] Serialize(SkeletonFrameAlternative frame)
        {
            byte[] bytes = null;
            using (BinaryStreamWriter w = new BinaryStreamWriter(new MemoryStream()))
            {
                w.Write(frame.FrameNumber);
                w.Write(frame.Skeletons.Length);
                foreach (SkeletonDataAlternative data in frame.Skeletons)
                {
                    w.Write(SkeletonDataSerializer.Serialize(data));
                }
                bytes = ((MemoryStream)w.Stream).ToArray();
                w.Close();
            }
            return bytes;
        }

        public static void Serialize(TextWriter writer, SkeletonFrameAlternative frame)
        {
            writer.Write(frame.FrameNumber);
            writer.Write(frame.Skeletons.Length);
            foreach (SkeletonDataAlternative data in frame.Skeletons)
            {
                SkeletonDataSerializer.Serialize(writer, data);
            }
        }
    }
}
