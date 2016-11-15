using System;
using System.Collections.Generic;
using System.Text;

using Emgu.CV;
using Emgu.Util;
using System.Drawing;
using Emgu.CV.Structure;

namespace HumanDetectionAndRecognition
{
    public class MaskData
    {
        public Point Right;
        public Point Bottom;
        public Point Top;
        public Point left;
        public Image<Gray, Byte> Mask;
    }
}
