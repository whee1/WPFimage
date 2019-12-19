using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace CGedit
{
    public class ImageHelper  //封装好的工具类
    {
        /// <summary>
        /// 从指定路径读取图片源
        /// </summary>
        public static BitmapImage LoadBitmapImageByPath(string path)
        {
            try
            {
                //文件不存在，返回空
                if (!File.Exists(path))
                {
                    return null;
                }

                BitmapImage bi = new BitmapImage();
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        byte[] bytes = br.ReadBytes((int)stream.Length);
                        bi.BeginInit();
                        bi.StreamSource = new MemoryStream(bytes);
                        bi.EndInit();
                    }
                }
                return bi;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static System.Drawing.Image LoadImageByPath(string path)
        {
            try
            {
                //文件不存在，返回空
                if (!File.Exists(path))
                {
                    return null;
                }

                System.Drawing.Image image;
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    image = System.Drawing.Image.FromStream(stream);

                }
                return image;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 从指定路径读取图片源，并等比例缩放
        /// </summary>
        public static BitmapImage LoadBitmapImageUniform(string path, int sourceWidth, int sourceHeight)
        {
            try
            {
                //文件不存在，返回空
                if (!File.Exists(path))
                    return null;

                BitmapImage bi = new BitmapImage();
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        byte[] bytes = br.ReadBytes((int)stream.Length);
                        bi.BeginInit();
                        if (sourceWidth > 0 && sourceHeight > 0)
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                            double width = image.Width;
                            double height = image.Height;
                            double localzoom = 0;
                            if (sourceWidth < width || sourceHeight < height)
                            {
                                if (width > height)
                                {
                                    bi.DecodePixelWidth = sourceWidth;
                                    localzoom = sourceWidth / width;
                                    bi.DecodePixelHeight = (int)Math.Ceiling(height * localzoom);
                                }
                                else
                                {
                                    bi.DecodePixelHeight = sourceHeight;
                                    localzoom = sourceHeight / height;
                                    bi.DecodePixelWidth = (int)Math.Ceiling(width * localzoom);
                                }
                            }
                            else
                            {
                                double zoom = 0;

                                zoom = sourceHeight / height;
                                bi.DecodePixelWidth = (int)Math.Ceiling(width * zoom);
                                bi.DecodePixelHeight = (int)Math.Ceiling(height * zoom);
                            }
                            image.Dispose();
                            image = null;
                        }
                        bi.StreamSource = new MemoryStream(bytes);
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                    }
                }
                return bi;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// Bitmap转换为BitmapImage
        /// </summary>
        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap tempMap, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            try
            {
                BitmapImage resultImage = new BitmapImage();
                using (MemoryStream ms = new MemoryStream())
                {
                    tempMap.Save(ms, imgFormat);
                    resultImage.BeginInit();
                    resultImage.StreamSource = ms;
                    resultImage.CacheOption = BitmapCacheOption.OnLoad;
                    resultImage.EndInit();
                    resultImage.Freeze();
                }
                return resultImage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// BitmapImage转换成Iamge
        /// </summary>
        public static System.Drawing.Image BitmapImageToIamge(BitmapImage bitmapIamge)
        {
            try
            {
                Stream stream = bitmapIamge.StreamSource;
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                return image;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 将目标转换为BitmapImage
        /// </summary>
        public static BitmapImage ConventToBitmapImage(RenderTargetBitmap rtb)
        {
            try
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                BitmapImage bitmapImg = new BitmapImage();
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(ms);
                    ms.Position = 0;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        byte[] tempByteArr = br.ReadBytes((int)ms.Length);
                        bitmapImg.BeginInit();
                        bitmapImg.StreamSource = new MemoryStream(tempByteArr);
                        bitmapImg.EndInit();
                        bitmapImg.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImg.Freeze();
                    }
                }
                return bitmapImg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 以Jpg格式存储图片
        /// </summary>
        public static bool SaveBitmapImageAsJpg(BitmapImage bi, string path)
        {
            try
            {
                string folder = path.Substring(0, path.LastIndexOf("\\"));

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bi));
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    encoder.Save(fileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 以Png格式存储图片
        /// </summary>
        public static bool SaveBitmapImageAsPng(BitmapImage bi, string path)
        {
            try
            {
                string folder = path.Substring(0, path.LastIndexOf("\\"));

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bi));
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    encoder.Save(fileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 裁剪图片
        /// </summary>
        public static System.Drawing.Bitmap ClipBitmap(System.Drawing.Image image, System.Drawing.Rectangle rect)
        {
            if (image == null)
                return null;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(rect.Width, rect.Height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawImage(image, new System.Drawing.Rectangle(0, 0, rect.Width, rect.Height), rect, System.Drawing.GraphicsUnit.Pixel);
            g.Dispose();
            return bmp;
        }
    }
}