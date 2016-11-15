using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace HumanDetectionAndRecognition
{
    public class ComponentData
    {
        private Image<Gray, Byte> _silhouette;
        private byte label = 0;
        private Point _upperP;
        private Point _lowerP;
        private Point _rightP;
        private Point _leftP;
        public bool col = false;

        public Image<Gray, Byte> Silhouette
        {
            set { _silhouette = value; }
            get { return _silhouette; }
        }

        #region object Label
        public byte Label
        {

            get { return label; }
            set { label = value; }
        }
        #endregion


        #region object width & height
        public int getWidth
        {
            get { return _rightP.Y - _leftP.Y; }
        }

        public int getHeight
        {
            get { return _lowerP.X - _upperP.X; }
        }
        #endregion


        #region object boundery points
        public Point UpperPoint
        {

            get { return _upperP; }
            set { _upperP = value; }
        }
        public Point LowerPoint
        {
            get { return _lowerP; }
            set { _lowerP = value; }
        }
        public Point LeftPoint
        {
            get { return _leftP; }
            set { _leftP = value; }
        }
        public Point RightPoint
        {
            get { return _rightP; }
            set { _rightP = value; }
        }
        #endregion

        public override string ToString()
        {
            return this.label.ToString();
        }
    }
}
