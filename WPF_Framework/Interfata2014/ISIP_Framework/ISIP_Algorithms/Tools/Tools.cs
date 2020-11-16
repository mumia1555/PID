﻿using System;
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

        public static Image<Gray, byte> ThresholdingAdaptiv(Image<Gray, byte> InputImage)
        {
            int dim = 0;
            double b = 0;
            Image<Gray, byte> result = new Image<Gray, byte>(InputImage.Size);
            int[,] integrala = new int[InputImage.Height, InputImage.Width];

            UserInputDialog dlg = new UserInputDialog("Binary Thresholds", new string[] { "dim", "b" });
            if (dlg.ShowDialog().Value == true)
            {
                dim = (int)dlg.Values[0];
                b = (double)dlg.Values[1];
            }
            if (dim % 2 == 0)
                dim++;

            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        integrala[y, x] = (InputImage.Data[0, 0, 0]);
                    }
                    else if (x == 0)
                    {
                        integrala[y, x] = (integrala[y - 1, 0] + InputImage.Data[y, 0, 0]);
                    }
                    else if (y == 0)
                    {
                        integrala[y, x] = (integrala[0, x - 1] + InputImage.Data[0, x, 0]);
                    }
                    else
                    {
                        integrala[y, x] = (integrala[y, x - 1] + integrala[y - 1, x] - integrala[y - 1, x - 1] + InputImage.Data[y, x, 0]);
                    }

                }
            }

            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    int sum = 0;
                    int x0 = Math.Max(x - dim / 2, 0),
                        x1 = Math.Min(x + dim / 2 - 1, InputImage.Width - 1),
                        y0 = Math.Max(y - dim / 2, 0),
                        y1 = Math.Min(y + dim / 2 - 1, InputImage.Height - 1);

                    if (x == 0 && y == 0)
                    {
                        sum = integrala[y1, x1];
                    }
                    else if (y == 0)
                    {
                        sum = integrala[y1, x1] - integrala[y1, x0];
                    }
                    else if (x == 0)
                    {
                        sum = integrala[y1, x1] - integrala[y0, x1];
                    }
                    else
                    {
                        sum = integrala[y1, x1] + integrala[y0, x0] - integrala[y1, x0] - integrala[y0, x1];
                    }

                    double T = (double)b * sum / ((y1 - y0) * (x1 - x0));
                    if (InputImage.Data[y, x, 0] <= T)
                    {
                        result.Data[y, x, 0] = 0;
                    }
                    else
                    {
                        result.Data[y, x, 0] = 255;
                    }
                }
            }

            return result;
        }
        public static void ImagineIntegrala(Image<Gray, byte> InputImage, int[,] integrala)
        {
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        integrala[y, x] = (InputImage.Data[0, 0, 0]);
                    }
                    else if (x == 0)
                    {
                        integrala[y, x] = (integrala[y - 1, 0] + InputImage.Data[y, 0, 0]);
                    }
                    else if (y == 0)
                    {
                        integrala[y, x] = (integrala[0, x - 1] + InputImage.Data[0, x, 0]);
                    }
                    else
                    {
                        integrala[y, x] = (integrala[y, x - 1] + integrala[y - 1, x] - integrala[y - 1, x - 1] + InputImage.Data[y, x, 0]);
                    }

                }
            }
        }

        public static Image<Gray, byte> FiltruMA(Image<Gray, byte> InputImage)
        {
            int dim = 0;
            int[,] integrala = new int[InputImage.Height, InputImage.Width];
            Image<Gray, byte> result = new Image<Gray, byte>(InputImage.Size);
            result.Data = InputImage.Data;

            UserInputDialog dlg = new UserInputDialog("Binary Thresholds", new string[] { "dim" });
            if (dlg.ShowDialog().Value == true)
            {
                dim = (int)dlg.Values[0];
            }
            if (dim % 2 == 0)
                dim++;

            ImagineIntegrala(InputImage, integrala);

            for (int y = 0 + dim / 2; y < InputImage.Height - dim / 2; y++)
            {
                for (int x = 0 + dim / 2; x < InputImage.Width - dim / 2; x++)
                {
                    int sum = 0;

                    int x0 = x - dim / 2,
                       x1 = x + dim / 2,
                       y0 = y - dim / 2,
                       y1 = y + dim / 2;

                    if (x == 0 && y == 0)
                    {
                        sum = integrala[y1, x1];
                    }
                    else if (y == 0)
                    {
                        sum = integrala[y1, x1] - integrala[y1, x0];
                    }
                    else if (x == 0)
                    {
                        sum = integrala[y1, x1] - integrala[y0, x1];
                    }
                    else
                    {
                        sum = integrala[y1, x1] + integrala[y0, x0] - integrala[y1, x0] - integrala[y0, x1];
                    }
                    sum /= dim * dim;
                    result.Data[y, x, 0] = (byte)sum;
                }
            }

            return result;
        }
    }
}
