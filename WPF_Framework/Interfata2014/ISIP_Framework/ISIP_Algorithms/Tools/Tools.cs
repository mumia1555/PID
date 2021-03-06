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

        public static Image<Gray, byte> Binarizare(Image<Gray, byte> InputImage)
        {
            int T = 0;
            UserInputDialog dlg = new UserInputDialog("T", new string[] { "T" }, 300, 250);
            if (dlg.ShowDialog().Value == true)
            {
                T = (int)dlg.Values[0];
            }
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if (InputImage.Data[y, x, 0] < T)
                        Result.Data[y, x, 0] = 0;
                    else
                        Result.Data[y, x, 0] = 255;
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

            UserInputDialog dlg = new UserInputDialog("Medie aritmetica", new string[] { "dim" });
            if (dlg.ShowDialog().Value == true)
            {
                dim = (int)dlg.Values[0];
            }
            if (dim % 2 == 0)
                dim++;

            ImagineIntegrala(InputImage, integrala);

            for (int y = 0 + dim / 2 + 1; y < InputImage.Height - dim / 2; y++)
            {
                for (int x = 0 + dim / 2 + 1; x < InputImage.Width - dim / 2; x++)
                {
                    int sum = 0;

                    int x0 = x - dim / 2 - 1,
                       x1 = x + dim / 2,
                       y0 = y - dim / 2 - 1,
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
                    result.Data[y, x, 0] = (byte)(sum + 0.5);
                }
            }

            return result;
        }
        public static Image<Gray, byte> SobelFilterVertic(Image<Gray, byte> InputImage, int threshold)
        {
            Image<Gray, byte> ResultImage = new Image<Gray, byte>(InputImage.Size);
            int[,] SX = new int[InputImage.Size.Height, InputImage.Size.Width];
            int[,] SY = new int[InputImage.Size.Height, InputImage.Size.Width];

            int[,] matrix = new int[3, 3];

            for (int y = 1; y < InputImage.Height - 1; y++)
            {
                for (int x = 1; x < InputImage.Width - 1; x++)
                {
                    matrix[0, 0] = InputImage.Data[y - 1, x - 1, 0];
                    matrix[1, 0] = InputImage.Data[y - 1, x, 0];
                    matrix[2, 0] = InputImage.Data[y - 1, x + 1, 0];
                    matrix[0, 1] = InputImage.Data[y, x - 1, 0];
                    matrix[1, 1] = InputImage.Data[y, x, 0];
                    matrix[2, 1] = InputImage.Data[y, x + 1, 0];
                    matrix[0, 2] = InputImage.Data[y + 1, x - 1, 0];
                    matrix[1, 2] = InputImage.Data[y + 1, x, 0];
                    matrix[2, 2] = InputImage.Data[y + 1, x + 1, 0];

                    SX[y, x] = Sx(matrix);
                    SY[y, x] = Sy(matrix);
                }
            }

            for (int i = 1; i < InputImage.Height - 1; i++)
            {
                for (int j = 1; j < InputImage.Width - 1; j++)
                {
                    int val = (int)Math.Sqrt(Math.Pow(SX[i, j], 2) + Math.Pow(SY[i, j], 2));
                    if (val < threshold)
                        ResultImage.Data[i, j, 0] = 0;
                    else
                    {
                        double angle = Math.Atan2(SY[i, j], SX[i, j]);
                        int degrees = (int)(angle * 180 / Math.PI);
                        if ((degrees >= 85 && degrees <= 95) || (degrees <= -85 && degrees >= -95))
                            ResultImage.Data[i, j, 0] = 255;
                    }
                }
            }
            return ResultImage;
        }
        private static int Sx(int[,] matrix)
        {
            return (matrix[2, 0] - matrix[0, 0] + 2 * matrix[2, 1] - 2 * matrix[0, 1] + matrix[2, 2] - matrix[0, 2]);
        }
        private static int Sy(int[,] matrix)
        {
            return (matrix[0, 2] - matrix[0, 0] + 2 * matrix[1, 2] - 2 * matrix[1, 0] + matrix[2, 2] - matrix[2, 0]);
        }

        public static bool Check(Image<Gray, byte> InputImage, int x, int y, int mask, int value)
        {
            for (int j = y - mask / 2; j <= y + mask / 2; j++)
                for (int i = x - mask / 2; i <= x + mask / 2; i++)
                    if (InputImage.Data[j, i, 0] == value)
                        return true;
            return false;
        }
        public static Image<Gray, byte> Dilation(Image<Gray, byte> InputImage, int mask)
        {
            Image<Gray, byte> ResultImage = new Image<Gray, byte>(InputImage.Size);
            for (int y = mask / 2; y < InputImage.Height - mask / 2; y++)
            {
                for (int x = mask / 2; x < InputImage.Width - mask / 2; x++)
                {
                    if (Check(InputImage, x, y, mask, 255))
                        ResultImage.Data[y, x, 0] = 255;
                    else
                        ResultImage.Data[y, x, 0] = 0;
                }
            }
            return ResultImage;
        }
        public static Image<Gray, byte> Eroding(Image<Gray, byte> InputImage, int mask)
        {
            Image<Gray, byte> ResultImage = new Image<Gray, byte>(InputImage.Size);
            for (int y = mask / 2; y < InputImage.Height - mask / 2; y++)
            {
                for (int x = mask / 2; x < InputImage.Width - mask / 2; x++)
                {
                    if (Check(InputImage, x, y, mask, 0))
                        ResultImage.Data[y, x, 0] = 0;
                    else
                        ResultImage.Data[y, x, 0] = 255;
                }
            }
            return ResultImage;
        }
        public static Image<Gray, byte> Opening(Image<Gray, byte> InputImage, int mask)
        {
            Image<Gray, byte> ResultImage = Eroding(InputImage, mask);
            return Dilation(ResultImage, mask);
        }
        public static Image<Gray, byte> Closing(Image<Gray, byte> InputImage, int mask)
        {
            Image<Gray, byte> ResultImage = Dilation(InputImage, mask);
            return Eroding(ResultImage, mask);
        }
        public static Image<Gray, byte> Twirl(Image<Gray, byte> inputImage)
        {
            double alfa = 0, rmax = 0;
            UserInputDialog dlg = new UserInputDialog("", new string[] { "alfa", "r max" });
            if (dlg.ShowDialog().Value == true)
            {
                alfa = (double)dlg.Values[0];
                rmax = (double)dlg.Values[0];
            }
            alfa = Math.PI * alfa / 180;

            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            result.Data = inputImage.Data;
            int xo = result.Width / 2;
            int yo = result.Height / 2;


            for (int y = 0; y < result.Height; y++)
                for (int x = 0; x < result.Width; x++)
                {
                    int dx = x - xo;
                    int dy = y - yo;
                    double r = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                    double beta = Math.Atan2(dy, dx) + alfa * ((rmax - r) / rmax);
                    double xC;
                    double yC;
                    if (r > rmax)
                    {
                        xC = x;
                        yC = y;
                    }
                    else
                    {
                        xC = xo + r * Math.Cos(beta);
                        yC = yo + r * Math.Sin(beta);
                    }

                    int x0 = (int)xC;
                    int y0 = (int)yC;

                    if (x0 + 1 < result.Width && y0 + 1 < result.Height)
                    {
                        double firstValue = (inputImage.Data[y0, x0 + 1, 0] - inputImage.Data[y0, x0, 0]) * (xC - x0) + inputImage.Data[y0, x0, 0];
                        double secondValue = (inputImage.Data[y0 + 1, x0 + 1, 0] - inputImage.Data[y0 + 1, x0, 0]) * (xC - x0) + inputImage.Data[y0 + 1, x0, 0];
                        double finalValue = (secondValue - firstValue) * (yC - y0) + firstValue;

                        result.Data[y, x, 0] = (byte)finalValue;
                    }


                }
            return result;
        }

    }
}

