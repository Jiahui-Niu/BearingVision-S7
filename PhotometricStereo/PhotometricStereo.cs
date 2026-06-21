using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotometricStereo
{
    /// <summary>
    /// 光度立体图像合成：多角度光照图像合成为增强纹理图像
    /// </summary>
    public static class PhotometricStereoProcessor
    {
        /// <summary>
        /// 将多张不同光照角度的图像合成为一张增强图像
        /// </summary>
        /// <param name="images">多角度光照图像数组（灰度）</param>
        /// <returns>合成结果图像</returns>
        public static Bitmap Process(Bitmap[] images)
        {
            if (images == null || images.Length == 0)
                throw new ArgumentException("至少需要一张图像");

            int width = images[0].Width;
            int height = images[0].Height;

            var result = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var palette = result.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            result.Palette = palette;

            var resultData = result.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            var srcDataList = new BitmapData[images.Length];
            for (int k = 0; k < images.Length; k++)
            {
                srcDataList[k] = images[k].LockBits(new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            }

            try
            {
                unsafe
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte* pResult = (byte*)resultData.Scan0 + y * resultData.Stride;
                        for (int x = 0; x < width; x++)
                        {
                            int sum = 0;
                            for (int k = 0; k < images.Length; k++)
                            {
                                byte* pSrc = (byte*)srcDataList[k].Scan0 + y * srcDataList[k].Stride + x;
                                sum += *pSrc;
                            }
                            pResult[x] = (byte)Math.Min(255, sum / images.Length);
                        }
                    }
                }
            }
            finally
            {
                result.UnlockBits(resultData);
                for (int k = 0; k < images.Length; k++)
                    images[k].UnlockBits(srcDataList[k]);
            }

            return result;
        }

        /// <summary>
        /// 计算法线图（Normal Map）
        /// </summary>
        public static Bitmap ComputeNormalMap(Bitmap[] images, float[] lightAngles)
        {
            if (images.Length < 3 || images.Length != lightAngles.Length)
                throw new ArgumentException("至少需要3张不同角度的图像");

            int width = images[0].Width;
            int height = images[0].Height;
            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // 简化实现：使用最大值投影
            var resultData = result.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                var srcDataList = new BitmapData[images.Length];
                for (int k = 0; k < images.Length; k++)
                    srcDataList[k] = images[k].LockBits(new Rectangle(0, 0, width, height),
                        ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

                unsafe
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte* pResult = (byte*)resultData.Scan0 + y * resultData.Stride;
                        for (int x = 0; x < width; x++)
                        {
                            float sumCos = 0, sumSin = 0, sumI = 0;
                            for (int k = 0; k < images.Length; k++)
                            {
                                byte* pSrc = (byte*)srcDataList[k].Scan0 + y * srcDataList[k].Stride + x;
                                float intensity = *pSrc / 255.0f;
                                float angle = lightAngles[k] * (float)Math.PI / 180.0f;
                                sumCos += intensity * (float)Math.Cos(angle);
                                sumSin += intensity * (float)Math.Sin(angle);
                                sumI += intensity;
                            }
                            byte r = (byte)(Math.Min(1.0f, Math.Abs(sumCos)) * 255);
                            byte g = (byte)(Math.Min(1.0f, Math.Abs(sumSin)) * 255);
                            byte b = (byte)(Math.Min(1.0f, sumI / images.Length) * 255);
                            pResult[x * 3 + 2] = r; // R
                            pResult[x * 3 + 1] = g; // G
                            pResult[x * 3 + 0] = b; // B
                        }
                    }
                }

                for (int k = 0; k < images.Length; k++)
                    images[k].UnlockBits(srcDataList[k]);
            }
            finally
            {
                result.UnlockBits(resultData);
            }

            return result;
        }
    }
}
