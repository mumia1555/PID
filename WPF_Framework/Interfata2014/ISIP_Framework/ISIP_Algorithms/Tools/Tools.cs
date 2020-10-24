using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using ISIP_UserControlLibrary;

namespace ISIP_Algorithms.Tools
{
    public class Tools
    {
        public static Image<Gray, byte> Invert(Image<Gray, byte> InputImage)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    Result.Data[y, x, 0] = (byte)(255 - InputImage.Data[y, x, 0]);
                }
            }
            return Result;
        }
        public static Image<Gray, byte> OperatorAfin(Image<Gray, byte> InputImage)
        {
            int r1 = 0, r2 = 0, s1 = 0, s2 = 0;
            const int L = 256;
            int[] LUT = new int[256];
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            UserInputDialog dlg = new UserInputDialog("Binary Thresholds", new string[] { "r1", "r2", "s1", "s2" }, 300, 250);
            if (dlg.ShowDialog().Value == true)
            {
                r1 = (int)dlg.Values[0];
                r2 = (int)dlg.Values[1];
                s1 = (int)dlg.Values[2];
                s2 = (int)dlg.Values[3];
            }
            float alfa = (float)s1 / r1;
            float beta = (float)(s2 - s1) / (r2 - r1);
            float gamma = (float)(255 - s2) / (255 - r2);
            for (int r = 0; r < 256; r++)
            {
                if (r >= 0 && r < r1)
                {
                    LUT[r] = (int)(alfa * r);
                }
                else if (r >= r1 && r <= r2)
                {
                    LUT[r] = (int)(beta * (r - r1) + s1);
                }
                else if (r >= r2 && r <= L - 1)
                {
                    LUT[r] = (int)(gamma * (r - r2) + s2);
                }
            }
            for (int y = 0; y < InputImage.Height; y++)
                for (int x = 0; x < InputImage.Width; x++)
                    Result.Data[y, x, 0] = (byte)LUT[InputImage.Data[y, x, 0]];
            return Result;
        }
    }
}
