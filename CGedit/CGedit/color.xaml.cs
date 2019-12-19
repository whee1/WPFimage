using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CGedit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class color : Window
    {
        System.Drawing.Bitmap baseBack;//底板位图对象
        System.Drawing.Bitmap lightBmp;//亮度位图对象
        System.Drawing.Bitmap ColorBmp;//色谱位图对象
        System.Drawing.Color lightColor = System.Drawing.Color.Black;//亮度色谱
        int ColorPx = -10, ColorPy = -10;//选择颜色的坐标
        int lightP = 121;//选择亮度的坐标
        int ColorSelectorWidth = 18;//rectColorSelector的宽度值
        int ColorSelectorHeight = 129;//rectColorSelector的top值

        public Color returnSelectColor;//选择的颜色

        public color()
        {
            InitializeComponent();
            SetColorBack();
            SetlightBack();
            baseBack = SetBaseColorBack();
            ColorPx = 0;
            ColorPy = 0;
            returnSelectColor = Colors.Black;
            SelectColor.Background = new SolidColorBrush(returnSelectColor);
        }
        /// <summary>
        /// 获得色谱位图对象
        /// </summary>
        public void SetColorBack()
        {
            ColorBmp = new System.Drawing.Bitmap(205, ColorSelectorHeight);
            Color tempColor = Colors.White;
            int R = 255, G = 0, B = 0;
            int R2 = R, G2 = G, B2 = B;
            for (int i = 0; i < ColorSelectorHeight; i++)
            {
                for (int j = 0; j < 205; j++)
                {
                    if (j >= 0 && j <= 34)
                        G = SetColorNum(G, j, true);
                    if (j > 34 && j <= 68)
                        R = SetColorNum(R, j, false);
                    if (j > 68 && j <= 102)
                        B = SetColorNum(B, j, true);
                    if (j > 102 && j <= 136)
                        G = SetColorNum(G, j, false);
                    if (j > 136 && j <= 170)
                        R = SetColorNum(R, j, true);
                    if (j > 170 && j <= 204)
                        B = SetColorNum(B, j, false);
                    int temps = i * -2;
                    R2 = IntMaxMin(R + temps);
                    G2 = IntMaxMin(G + temps);
                    B2 = IntMaxMin(B + temps);
                    int l = Math.Max(0, lightP == 0 ? 255 : (121 - lightP) * 2);//lightP原始值
                    System.Drawing.Color tmpC = SetColorWhite(System.Drawing.Color.FromArgb(R2, G2, B2), l);
                    tempColor = Color.FromRgb(tmpC.R, tmpC.G, tmpC.B);
                    ColorBmp.SetPixel(j, i, System.Drawing.Color.FromArgb(tempColor.R, tempColor.G, tempColor.B));
                }
            }
        }
        /// <summary>
        /// 设置色谱位图
        /// </summary>
        public System.Drawing.Bitmap SetBaseColorBack()
        {
            ColorBmp = new System.Drawing.Bitmap(205, ColorSelectorHeight);
            System.Drawing.Color tempColor = System.Drawing.Color.White;
            int R = 255, G = 0, B = 0;
            int R2 = R, G2 = G, B2 = B;
            for (int i = 0; i < ColorSelectorHeight; i++)
            {
                for (int j = 0; j < 205; j++)
                {
                    if (j >= 0 && j <= 34)
                        G = SetColorNum(G, j, true);
                    if (j > 34 && j <= 68)
                        R = SetColorNum(R, j, false);
                    if (j > 68 && j <= 102)
                        B = SetColorNum(B, j, true);
                    if (j > 102 && j <= 136)
                        G = SetColorNum(G, j, false);
                    if (j > 136 && j <= 170)
                        R = SetColorNum(R, j, true);
                    if (j > 170 && j <= 204)
                        B = SetColorNum(B, j, false);
                    int temps = i * -2;
                    R2 = IntMaxMin(R + temps);
                    G2 = IntMaxMin(G + temps);
                    B2 = IntMaxMin(B + temps);
                    tempColor = System.Drawing.Color.FromArgb(R2, G2, B2);
                    ColorBmp.SetPixel(j, i, tempColor);
                }
            }
            //将Bitmap转换为BitmapSource
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(ColorBmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ColorBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage tempImage = new BitmapImage();
                tempImage.BeginInit();
                tempImage.StreamSource = new System.IO.MemoryStream(ms.ToArray());
                tempImage.EndInit();
                imgColor.Source = tempImage;
            }
            g.Dispose();
            return ColorBmp;
        }
        /// <summary>
        /// 设置亮度位图
        /// </summary>
        public void SetlightBack()
        {
            lightBmp = new System.Drawing.Bitmap(ColorSelectorWidth, ColorSelectorHeight);
            for (int i = 0; i < ColorSelectorHeight; i++)
            {
                for (int j = 0; j < ColorSelectorWidth; j++)
                {
                    System.Drawing.Color c = SetColorWhite(lightColor, Math.Max(255 - i * 2, 0));
                    lightBmp.SetPixel(j, i, c);
                }
            }
            //将Bitmap转换为BitmapSource
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(lightBmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                lightBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage tempImage = new BitmapImage();
                tempImage.BeginInit();
                tempImage.StreamSource = new System.IO.MemoryStream(ms.ToArray());
                tempImage.EndInit();
                imgColorSelector.Source = tempImage;
            }
            g.Dispose();
        }
        /// <summary>
        /// 设置具体坐标的色彩值
        /// </summary>
        public int SetColorNum(int self, int y, bool isAdd)
        {
            int addnum = 8;
            if (y == 0)
                return self;
            int num = isAdd ? self + addnum : self - addnum;
            return IntMaxMin(num);
        }
        /// <summary>
        /// 颜色变量区域限制
        /// </summary>
        public int IntMaxMin(int num)
        {
            if (num > 255)
                return 255;
            if (num < 0)
                return 0;
            return num;
        }
        /// <summary>
        /// 设置颜色为白色
        /// </summary>
        public System.Drawing.Color SetColorWhite(System.Drawing.Color c, int white = 50)
        {
            int R = Math.Min(c.R + white, 255);
            int G = Math.Min(c.G + white, 255);
            int B = Math.Min(c.B + white, 255);
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        #region 调色板鼠标事件
        private void imgColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            imgColor.MouseMove += new MouseEventHandler(imgColor_MouseMove);
            SetColorPoint(e);
        }
        private void imgColor_MouseMove(object sender, MouseEventArgs e)
        {
            SetColorPoint(e);
        }
        private void imgColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            imgColor.MouseMove -= new MouseEventHandler(imgColor_MouseMove);
        }
        /// <summary>
        /// 同时设置颜色选择点位置
        /// </summary>
        public void SetColorPoint(MouseEventArgs e)
        {
            ColorPx = (int)(e.GetPosition(imgColor).X - 4);
            ColorPy = (int)(e.GetPosition(imgColor).Y - 4);
            if (ColorPx > ColorBmp.Width - 1)
                ColorPx = ColorBmp.Width - 1;
            if (ColorPx < 0)
                ColorPx = 0;
            if (ColorPy > ColorBmp.Height - 1)
                ColorPy = ColorBmp.Height - 1;
            if (ColorPy < 0)
                ColorPy = 0;
            ImgColorShowChange();
            if (ColorPx >= 0 && ColorPx <= ColorBmp.Width && ColorPy >= 0 && ColorPy <= ColorBmp.Height)
            {
                lightColor = baseBack.GetPixel(ColorPx, ColorPy);
                System.Drawing.Color tmpClor = ColorRight(ColorBmp.GetPixel(ColorPx, ColorPy));
                returnSelectColor = Color.FromRgb(tmpClor.R, tmpClor.G, tmpClor.B);
                SelectColor.Background = new SolidColorBrush(returnSelectColor);
                MainWindow.brushColor = returnSelectColor;
               
                SetlightBack();
            }
        }
        private void ImgColorShowChange()
        {
            Canvas.SetTop(this.ellColor, ColorPy - 4);
            Canvas.SetLeft(this.ellColor, ColorPx - 4);
        }
        /// <summary>
        /// 颜色校正
        /// </summary>
        public System.Drawing.Color ColorRight(System.Drawing.Color c)
        {
            int R = IntMaxMin(c.R);
            int G = IntMaxMin(c.G);
            int B = IntMaxMin(c.B);
            return System.Drawing.Color.FromArgb(R, G, B);
        }
        #endregion

        #region 亮度调节鼠标事件
        private void imgColorSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            imgColorSelector.MouseMove += new MouseEventHandler(imgColorSelector_MouseMove);
            SetLightPoint(e);
        }
        private void imgColorSelector_MouseMove(object sender, MouseEventArgs e)
        {
            SetLightPoint(e);
        }
        private void imgColorSelector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            imgColorSelector.MouseMove -= new MouseEventHandler(imgColorSelector_MouseMove);
        }
        /// <summary>
        /// 同步设置亮度选择点位置
        /// </summary>
        public void SetLightPoint(MouseEventArgs e)
        {
            lightP = (int)(e.GetPosition(imgColorSelector).Y - 2);
            if (lightP > ColorSelectorHeight)
                lightP = ColorSelectorHeight;
            if (lightP < 0)
                lightP = 0;
            SetColorBack();
            ImgColorShowChange();
            ImgColorSelectorChange();
            if (ColorPx >= 0 && ColorPx <= ColorBmp.Width && ColorPy >= 0 && ColorPy <= ColorBmp.Height)
            {
                lightColor = baseBack.GetPixel(ColorPx, ColorPy);
                System.Drawing.Color tmpClor = ColorRight(ColorBmp.GetPixel(ColorPx, ColorPy));
                returnSelectColor = Color.FromRgb(tmpClor.R, tmpClor.G, tmpClor.B);
                SelectColor.Background = new SolidColorBrush(returnSelectColor);
                MainWindow.brushColor = returnSelectColor;
                SetlightBack();
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ImgColorSelectorChange()
        {
            Canvas.SetTop(this.rectColorSelector, lightP);
        }
        #endregion
    }
}
