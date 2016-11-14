using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.Util;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Structure;


namespace HumanDetectionAndRecognition
{
     public class ConnectedComponent
    {
        byte _nextLabel = 0;
        public List<ComponentData> components = new List<ComponentData>();

        /// <summary>
        /// Find all connected components in image
        /// </summary>
        /// <param name="inpImage"> Current frame Binary</param>
        /// <returns> List of all connected components in the image and
        /// info about them like Silhouette,top,lower,right and left points</returns>
        unsafe public List<ComponentData> Find_ConnectedComponent(Image<Gray, Byte> inpImage)
        {
            _nextLabel = 0;
            components.Clear();
            List<ComponentData> _tempComponent = new List<ComponentData>();
            inpImage = FirstPath(inpImage.Clone());
            inpImage = SecondPath(inpImage.Clone());
            FilleachObjectPar(inpImage.Clone());
            foreach (ComponentData comp in components)
            {
                //if (comp.getHeight > 30 && comp.getWidth > 13 && comp.getWidth < 40)  //50,60
                //{
                    comp.Silhouette = ExtractImage(inpImage.Clone(), comp.UpperPoint.X, comp.LowerPoint.X, comp.LeftPoint.Y, comp.RightPoint.Y, comp.Label).Mul(255);
                    _tempComponent.Add(comp);
                //}
            }
            components = _tempComponent;
            return components;
        }

        /// <summary>
        /// Copy part of Gray image based on top,lower,right and left points and part label
        /// </summary>
        /// <param name="originImage">the original image to be copied.</param>
        /// <param name="XUpper">the upper boundary</param>
        /// <param name="XLower">the lower boundary</param>
        /// <param name="YLeft">the left boundary</param>
        /// <param name="YRight">the right boundary</param>
        /// <param name="CopyLabel">the copy label</param>
        /// <returns>copied Part in Gray image </returns>
        unsafe public static Image<Gray, Byte> ExtractImage(Image<Gray, Byte> originImage, int XUpper, int XLower, int YLeft, int YRight, byte CopyLabel)
        {
            try
            {
                Image<Gray, Byte> destination = new Image<Gray, byte>(YRight - YLeft, XLower - XUpper);
                fixed (byte* ptr_src = originImage.Data)
                {
                    fixed (byte* ptr_dest = destination.Data)
                    {
                        int i_src = 0;
                        int i_dest = 0;
                        for (int row = XUpper; row < XLower; row++)
                        {
                            i_src = (row * (originImage.Data.Length / originImage.Height)) + YLeft;
                            for (int col = YLeft; col < YRight; col++)
                            {
                                if (*(ptr_src + i_src) == CopyLabel)
                                    *(ptr_dest + i_dest) = *(ptr_src + i_src);
                                i_src++;
                                i_dest++;
                            }
                            i_dest += (destination.Data.Length / destination.Height) % destination.Width;
                        }
                    }
                }
                return destination;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Error Done in ExtractImage");
            }
            return null;
        }

        /// <summary>
        /// Do first path of connected component algo. starting from point (0,0)
        /// </summary>
        /// <param name="inpImage">current frame</param>
        /// <returns>gray image each object has special label based on first path </returns>
        unsafe Image<Gray, Byte> FirstPath(Image<Gray, Byte> inpImage)
        {
            try
            {
                int i = 0;
                fixed (byte* ptr_binaryImage = inpImage.Data)
                {
                    for (int row = 0; row < inpImage.Height; row++)
                    {
                        for (int col = 0; col < inpImage.Data.Length / inpImage.Height; col++)
                        {
                            if (*(ptr_binaryImage + i) == (byte)255)
                            {
                                byte label = Find8Neighbours(ptr_binaryImage, i, (inpImage.Data.Length / inpImage.Height), inpImage.Data.Length);
                                if (label == 255)
                                {
                                    _nextLabel++;
                                    *(ptr_binaryImage + i) = _nextLabel;
                                }
                                else
                                    *(ptr_binaryImage + i) = label;
                            }
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Error Done in FirstPath");
            }
            return inpImage.Clone();
        }

        /// <summary>
        /// Do second path of connected component algo. starting from point (n,n)
        /// </summary>
        /// <param name="inpImage">gray image that are resulted from 1st path</param>
        /// <returns>gray image each connected component has special label based on 2ed path</returns>
        unsafe Image<Gray, Byte> SecondPath(Image<Gray, Byte> inpImage)
        {
            try
            {
                fixed (byte* ptr_binaryImage = inpImage.Data)
                {
                    int i = 0;
                    int temp = 0;
                    for (int row = inpImage.Height - 1; row > -1; row--)
                    {
                        temp = (row) * (inpImage.Data.Length / inpImage.Height);
                        for (int col = (inpImage.Data.Length / inpImage.Height) - 1; col > -1; col--)
                        {
                            i = temp + col;
                            byte v = *(ptr_binaryImage + i);
                            if (*(ptr_binaryImage + i) != 0)
                            {
                                byte label = Find8Neighbours(ptr_binaryImage, i, inpImage.Data.Length / inpImage.Height, inpImage.Data.Length);
                                if (label != 255)
                                {
                                    if (label != *(ptr_binaryImage + i))
                                        *(ptr_binaryImage + i) = label;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Error Done in SecondPath");
            }
            return inpImage.Clone();
        }

        /// <summary>
        /// Find the label of neighbour pixels
        /// </summary>
        /// <param name="ptr">pointer to your image</param>
        /// <param name="pixel_index">index of current pixel</param>
        /// <param name="rowlenth">image width</param>
        /// <param name="imageLength">image length</param>
        /// <returns>if there were labeled neighbour return min labeled neighbour else return 255</returns>
        unsafe byte Find8Neighbours(byte* ptr, int pixel_index, int rowlenth, int imageLength)
        {
            try
            {
                byte label = 255;
                if (((pixel_index + 1) % rowlenth) != 1)
                {
                    if (*((ptr + pixel_index) + rowlenth - 1) != 0)
                    {
                        if (label > *((ptr + pixel_index) + rowlenth - 1))
                            label = *((ptr + pixel_index) + rowlenth - 1);
                    }
                    if (*((ptr + pixel_index) - 1) != 0)
                    {
                        if (label > *((ptr + pixel_index) - 1))
                            label = *((ptr + pixel_index) - 1);
                    }
                    if (*((ptr + pixel_index) - rowlenth - 1) != 0)
                    {
                        if (label > *((ptr + pixel_index) - 1 - rowlenth))
                            label = *((ptr + pixel_index) - 1 - rowlenth);
                    }
                }
                if ((pixel_index + 1) > rowlenth)
                {
                    if (*((ptr + pixel_index) - rowlenth) != 0)
                    {
                        if (label > *((ptr + pixel_index) - rowlenth))
                            label = *((ptr + pixel_index) - rowlenth);
                    }
                }
                if ((pixel_index + 1) % rowlenth != 0)
                {
                    if (*((ptr + pixel_index) + rowlenth + 1) != 0)
                    {
                        if (label > *((ptr + pixel_index) + rowlenth + 1))
                            label = *((ptr + pixel_index) + rowlenth + 1);
                    }
                    if (*((ptr + pixel_index) + 1) != 0)
                    {
                        if (label > *((ptr + pixel_index) + 1))
                            label = *((ptr + pixel_index) + 1);
                    }
                    if (*((ptr + pixel_index) - rowlenth + 1) != 0)
                    {
                        if (label > *((ptr + pixel_index) - rowlenth + 1))
                            label = *((ptr + pixel_index) - rowlenth + 1);
                    }
                }
                if ((pixel_index + 1) < imageLength)
                {
                    if (*((ptr + pixel_index) + rowlenth) != 0)
                    {
                        if (label > *((ptr + pixel_index) + rowlenth))
                            label = *((ptr + pixel_index) + rowlenth);
                    }
                }
                return label;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Error Done in Find8Neighbours");
            }
            return 0;
        }

        /// <summary>
        /// Add connected component to component list that contain
        /// info about each object not add silhoutte in this fn
        /// </summary>
        /// <param name="inpImage">result image come from 2nd path</param>
        unsafe void FilleachObjectPar(Image<Gray, Byte> inpImage)
        {
            try
            {
                int i = 0;
                bool found_pixel = false;
                fixed (byte* ptr_binaryImage = inpImage.Data)
                {
                    for (int row = 0; row < inpImage.Height; row++)
                        for (int col = 0; col < inpImage.Data.Length / inpImage.Height; col++)
                        {
                            if (*(ptr_binaryImage + i) != 0)
                            {
                                foreach (ComponentData comp in components)
                                {
                                    if (*(ptr_binaryImage + i) == comp.Label)
                                    {
                                        if (col < comp.LeftPoint.Y)
                                            comp.LeftPoint = new Point(row, col);
                                        if (col > comp.RightPoint.Y)
                                            comp.RightPoint = new Point(row, col);
                                        if (row > comp.LowerPoint.X)
                                            comp.LowerPoint = new Point(row, col);
                                        found_pixel = true;
                                        break;
                                    }
                                    else
                                        found_pixel = false;
                                }
                                if (!found_pixel)
                                {
                                    ComponentData comp = new ComponentData();
                                    comp.Label = *(ptr_binaryImage + i);
                                    comp.UpperPoint = new Point(row, col);
                                    comp.LowerPoint = new Point(row, col);
                                    comp.RightPoint = new Point(row, col);
                                    comp.LeftPoint = new Point(row, col);
                                    components.Add(comp);
                                }
                            }
                            i++;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Error Done in FilleachObjectPar");
            }
        }
    }
}
