using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Effects;
using Image = System.Drawing.Image;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace CGedit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    ///
    
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow _instance ;
        static public Color brushColor;
        private BitmapImage origin=new BitmapImage(); //记录图源
        private DrawingGroup drawingGroup1 = new DrawingGroup();   //将多个绘图组合为复合绘图
        byte[] array = new byte[1024*4*1024]; //记录位图图像的数组
        private double MyImageX, MyImageY;
        private double MyImagepixelX, MyImagePixely;
        private double MyImageDpiX, MyImagedpiY;//位图图像每英寸点数
        private PixelFormat MyPixelFormat = new PixelFormat();
        private ToggleButton tbtn; //记录当前按下的按钮
        private ToggleButton sbtn;//记录当前按下的画笔shape按钮
        private bool isSelect = false;//标志现在是选区模式
        private Point mousePoint = new Point(); //鼠标当前相对于画布的坐标
        private double[] makeRectangle(double x1, double y1, double x2, double y2)
        {
            double Top, left, bottom, right;
            Top = y1 <= y2 ? y1 : y2;
            left = x1 <= x2 ? x1 : x2;
            bottom = y1 > y2 ? y1 : y2;
            right = x1 > x2 ? x1 : x2;
            double[] xs = { left, Top, right - left, bottom - Top };
            return xs;
            //获得矩形框
        }
        private enum BrushBraserShape
        {
            line=0,
            rect=1,
            circle=2,
        }
        private BrushBraserShape myBrushBraserShape = BrushBraserShape.line; //标记画笔形状

        private void Drawimage(Drawing g) //在控件中显示Drawing
        {
            drawingGroup1.Children.Add(g);
            //在控件中显示
            DrawingImage drawImage = new DrawingImage(drawingGroup1);
            image.Source = drawImage;
        }
        private bool PointInRect(Point p, Rect r) //判断点在不在矩形框内
        {
            return p.X <= r.Right && p.X >= r.Left && p.Y >= r.Top && p.Y <= r.Bottom;
        }

        private Point Offset(Point start, Point end)
        {
            return new Point(end.X-start.X,end.Y-start.Y); //两个值为x方向的偏移量和y方向的偏移量
        }
        public MainWindow()
        {
            InitializeComponent();
            
            _instance = this;
            tbtn = DrawEllipseBtn;
            sbtn = LineBtn;
            /*ract1.Visibility = Visibility.Hidden;*/
            Clipboard.Clear();
        }
        /// <summary>
        /// 打开保存新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            rectangle1.Visibility = Visibility.Hidden;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请打开图片";
            dialog.Filter = "图像文件 | *.jpg; *.png; *.jpeg; *.bmp; *.gif | 所有文件 | *.* ";
            if ((bool)dialog.ShowDialog())
            {
                //MessageBox.Show(drawingGroup1.Children.Count().ToString());
                {
                    image.Stretch = Stretch.UniformToFill;
                    drawingGroup1.Children.Clear();
                    BitmapFrame bf = BitmapFrame.Create(new Uri(dialog.FileName));
                    MyImageX = bf.Width;
                    MyImageY = bf.Height;
                    MyImagePixely = bf.PixelHeight;
                    MyImagepixelX = bf.PixelWidth;
                    MyImageDpiX = bf.DpiX;
                    MyImagedpiY = bf.DpiY;
                    image.Width = bf.Width;
                    image.Height = bf.Height;
                    MyPixelFormat = bf.Format;
                    ImageDrawing imageDrawing = new ImageDrawing();
                    imageDrawing.Rect = new Rect(0, 0, bf.Width, bf.Height);
                    imageDrawing.ImageSource = bf;
                    Drawimage(imageDrawing);
                    

                }
            }
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (image.Source!=null)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)MyImagepixelX, (int)MyImagePixely, (int)MyImageDpiX, (int)MyImagedpiY, PixelFormats.Pbgra32);
                rtb.Render(image);
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Title = "请选择保存位置";
                dialog.Filter = "图像文件(*.png)|*.png";
                if ((bool)dialog.ShowDialog())
                {

                    /*RenderTargetBitmap bmp = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight,1/96,1/96,PixelFormats.Pbgra32);
                    bmp.Render(canvas);*/
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        //drawingGroup1.Children.Clear();
                        encoder.Save(stream);
                    }
                }
            }
            else
            {
                MessageBox.Show("无法保存");
            }
           
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            color.Background = new SolidColorBrush(brushColor);//改变调色板颜色
        }
        private void New_Click(object sender, RoutedEventArgs e) //新建白图
        {
            rectangle1.Visibility = Visibility.Hidden;
            drawingGroup1.Children.Clear();
            BitmapSource bitmap=BitmapSource.Create(1024,800,96,96,PixelFormats.Pbgra32,null,array,1024*4);//创建一个位图
            MyImageX = bitmap.Width;
            MyImageY = bitmap.Height;
            MyImagePixely = bitmap.PixelHeight;
            MyImagepixelX = bitmap.PixelWidth;
            MyImageDpiX = bitmap.DpiX;
            MyImagedpiY = bitmap.DpiY;
            image.Width = bitmap.Width;
            image.Height = bitmap.Height;
            MyPixelFormat = bitmap.Format;
            //上面是创建位图 下面因为是image 所以用image来绘制
            ImageDrawing imageDrawing = new ImageDrawing(); 
            imageDrawing.ImageSource = bitmap;
            drawingGroup1.Children.Add(imageDrawing);
            //绘制矩形
            RectangleGeometry rect = new RectangleGeometry();
            rect.Rect = new Rect(0,0,1024,1024);//创建一个抽象矩形
            GeometryDrawing g = new GeometryDrawing();//用这个类来绘制
            g.Geometry = rect;
            g.Brush = Brushes.White;
            Drawimage(g);
            
        }
        /// <summary>
        /// 锐化
        /// </summary>
        private void RuihuaBtn_Click(object sender, RoutedEventArgs e) //锐化按钮事件
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int) MyImagepixelX, (int) MyImagePixely, (int) MyImageDpiX,
                (int) MyImagedpiY, PixelFormats.Pbgra32);
            rtb.Render(image);
            origin = ImageHelper.ConventToBitmapImage(rtb);
            origin = Sharpening(origin);//可以修改传参
            if (isSelect)//如果是在选区状态下
            {
                BitmapSource fs = origin as BitmapSource;
                CroppedBitmap cb = new CroppedBitmap(fs, new Int32Rect((int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3])); //然后切割图片 拿到修改的像素区域
                Clipboard.SetImage(cb);//复制然后粘贴
                stickImage(new Rect(rect[0], rect[1], rect[2], rect[3]));
                Clipboard.Clear();
            }
            else
            {
                double width = origin.Width;
                double height = origin.Height;
                ImageDrawing imageDrawing = new ImageDrawing();
                imageDrawing.Rect = new Rect(0, 0, width, height);
                imageDrawing.ImageSource = origin;
                drawingGroup1.Children.Clear();
                Drawimage(imageDrawing);
            }
        }
        public BitmapImage Sharpening(BitmapImage pic,int opacity = 255)
        {
            try
            {
                System.Drawing.Bitmap bmap = new System.Drawing.Bitmap(pic.StreamSource);
                int height = bmap.Height;
                int width = bmap.Width;
                System.Drawing.Color pixel;
                //拉普拉斯模板
                int[] Laplacian = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int r = 0, g = 0, b = 0, a = 0;
                        int Index = 0;
                        for (int col = -1; col <= 1; col++)
                            for (int row = -1; row <= 1; row++)
                            {
                                pixel = bmap.GetPixel(x + row, y + col);
                                r += pixel.R * Laplacian[Index];
                                g += pixel.G * Laplacian[Index];
                                b += pixel.B * Laplacian[Index];
                                Index++;
                                if (pixel.A < opacity)
                                {
                                    a = pixel.A;
                                }
                                else
                                {
                                    a = opacity;
                                }
                            }
                        //处理颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;

                        bmap.SetPixel(x - 1, y - 1, System.Drawing.Color.FromArgb(a, r, g, b));
                    }
                }

                return ImageHelper.BitmapToBitmapImage(bmap, ImageFormat.Png);
            }
            catch (Exception)
            {
                return (BitmapImage)image.Source;
            }
        }
        /// <summary>
        /// 模糊
        /// </summary>
        private void MohuBtn_Click(object sender, RoutedEventArgs e) //模糊按钮事件
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)MyImagepixelX, (int)MyImagePixely, (int)MyImageDpiX,
                (int)MyImagedpiY, PixelFormats.Pbgra32);
            rtb.Render(image);
            origin = ImageHelper.ConventToBitmapImage(rtb);
            origin = GaussBlur(origin);//先做处理
            if (isSelect)//如果是在选区状态下
            {

                BitmapSource fs = origin as BitmapSource;
                CroppedBitmap cb = new CroppedBitmap(fs, new Int32Rect((int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3])); //然后切割图片 拿到修改的像素区域
                Clipboard.SetImage(cb);//复制然后粘贴
                stickImage(new Rect(rect[0], rect[1], rect[2], rect[3]));
                Clipboard.Clear();
            }
            else
            {
                double width = origin.Width;
                double height = origin.Height;
                ImageDrawing imageDrawing = new ImageDrawing();
                imageDrawing.Rect = new Rect(0, 0, width, height);
                imageDrawing.ImageSource = origin;
                drawingGroup1.Children.Clear();
                Drawimage(imageDrawing);
            }

        }
        public BitmapImage GaussBlur(BitmapImage pic, int opacity = 255)
        {
            try
            {
                System.Drawing.Bitmap bmap = new System.Drawing.Bitmap(pic.StreamSource);
                int height = bmap.Height;
                int width = bmap.Width;
                System.Drawing.Color pixel;
                
                int[] Gauss = new int[9] { 1, 2, 1, 2, 4, 2, 1, 2, 1 };   // 高斯算子
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int r = 0, g = 0, b = 0, a = 0;
                        int Index = 0;
                        for (int col = -1; col <= 1; col++)
                            for (int row = -1; row <= 1; row++)
                            {
                                pixel = bmap.GetPixel(x + row, y + col);
                                r += pixel.R * Gauss[Index];
                                g += pixel.G * Gauss[Index];
                                b += pixel.B * Gauss[Index];
                                Index++;
                                if (pixel.A < opacity)
                                {
                                    a = pixel.A;
                                }
                                else
                                {
                                    a = opacity;
                                }
                            }
                        //处理颜色值溢出
                        r = r / 16;
                        g = g / 16;
                        b = b / 16;
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                    
                        bmap.SetPixel(x - 1, y - 1, System.Drawing.Color.FromArgb(a, r, g, b));
                    }
                }

                return ImageHelper.BitmapToBitmapImage(bmap, ImageFormat.Png);
            }
            catch (Exception)
            {
                return (BitmapImage)image.Source;
            }
        }
        /// <summary>
        /// 复制粘贴 剪切 删除选区 右键菜单栏按钮功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (rectangle1.Visibility ==Visibility.Visible)
            {
                try
                {
                    //rectangle1.Visibility = Visibility.Hidden;//把标志选区的框隐藏
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)MyImagepixelX, (int)MyImagePixely, (int)MyImageDpiX, (int)MyImagedpiY, PixelFormats.Pbgra32);
                    rtb.Render(image);
                    BitmapSource bs = rtb as BitmapSource;
                    CroppedBitmap cb = new CroppedBitmap(bs, new Int32Rect((int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3]));
                    Clipboard.SetImage(cb);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }  
        }
        private void Stick_Click(object sender, RoutedEventArgs e)
        {
            if (sender== rightStick) stickImage(new Rect(mousePoint.X-rect[2]/2, mousePoint.Y-rect[3]/2, rect[2], rect[3]));//第一个参数是粘贴的位置
            else stickImage(new Rect(new Point(0, 0), new Size(rect[2], rect[3])));//第一个参数是粘贴的位置
        }
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Copy_Click(sender,e);
            RectangleGeometry r = new RectangleGeometry();
            r.Rect=new Rect(rect[0],rect[1],rect[2],rect[3]);
            GeometryDrawing g = new GeometryDrawing();
            g.Geometry = r;
            g.Brush = Brushes.White;
            Drawimage(g);
        }
        private void CancelSelect_Click(object sender, RoutedEventArgs e)
        {
            rectangle1.Visibility = Visibility.Hidden;
        }
        private void stickImage(Rect r)
        {
            //rectangle1.Visibility = Visibility.Hidden;//把标志选区的框隐藏
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                //粘贴之后显示粘贴区域
                rectangle1.Visibility = Visibility.Visible;
                rectangle1.Width = rect[2];
                rectangle1.Height = rect[3];
                rectangle1.Margin = new Thickness(r.X, r.Y, 0, 0);
                rect[0] = r.X;
                rect[1] = r.Y;
                BitmapSource bs = Clipboard.GetImage();
                ImageDrawing imageDrawing = new ImageDrawing();
                imageDrawing.Rect = r;
                imageDrawing.ImageSource = bs;
                Drawimage(imageDrawing);
              
                isSelect = true; //粘贴之后默认进入选区模式

                //rect = new double[4];//清空选区矩形框
            }
        }
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
         }
        private void DeleteSelect_Clic(object sender, RoutedEventArgs e)
        {
            RectangleGeometry r = new RectangleGeometry();
            r.Rect = new Rect(rect[0], rect[1], rect[2], rect[3]);
            GeometryDrawing g = new GeometryDrawing();
            g.Geometry = r;
            g.Brush = Brushes.White;
            Drawimage(g);
        }
        /// <summary>
        /// 工具栏按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawBtn_Checked(object sender, RoutedEventArgs e)
        {
            tbtn.IsChecked = false;
            tbtn = (ToggleButton)sender;
        }
        /// <summary>
        /// 调色板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Color_Click(object sender, RoutedEventArgs e)//调色板按钮
        {
            Window a = new color();
            a.Show();
            
        }
        /// <summary>
        /// 画圆画直线画笔橡皮擦功能实现区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        private double visualx, visualy;
        private double x, y;
        private bool mark = false;
        private double[] rect; //记录矩形选区的坐标和大小 坐标系相对于image
        private PointCollection myPointCollection = new PointCollection();
        private PointCollection drawPointCollection = new PointCollection();
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(image);
            x = position.X;
            y = position.Y;
            //显示用
            Point position2 = e.GetPosition(grid1);
            visualx = position2.X;
            visualy = position2.Y;

            if (tbtn == DrawRectBtn && tbtn.IsChecked == true)
            {
                rectangle1.Visibility = Visibility.Visible;
                rectangle1.Width = 1;
                rectangle1.Height = 1;
                rectangle1.Margin = new Thickness(visualx, visualy, 0, 0);
                
            }
            if (tbtn == DrawEllipseBtn && tbtn.IsChecked == true) //画椭圆
            {
                rectangle1.Visibility = Visibility.Hidden;
                ellipse1.Visibility = Visibility.Visible;
                ellipse1.Width = 1;
                ellipse1.Height = 1;
                ellipse1.Margin = new Thickness(visualx, visualy, 0, 0);
                
            }
            if (brush.IsChecked == true||braser.IsChecked==true)   //画笔或橡皮擦
            {
                line.Visibility = Visibility.Visible;
                drawPointCollection.Clear();
                drawPointCollection.Add(position);
                myPointCollection.Clear();
                myPointCollection.Add(position2);
                //设置画笔属性 颜色和大小
                line.Stroke = new SolidColorBrush((bool)brush.IsChecked?brushColor:Colors.White);
                line.StrokeThickness = (bool)brush.IsChecked?brushSlider.Value:braserSlider.Value;
               
            }
            //移动选区：在选区状态下 如果选中了移动按钮 按下鼠标左键时记录坐标 同时调用剪切功能 抬起时记录坐标 算出偏移 然后对rect进行偏移 在mouseup函数中 松开的时候在rect区域内粘贴  拖动过程中只对rectangle组件进行视觉上的移动操作
            if (isSelect&&SelectMoveBtn.IsChecked==true)
            {
                tbtn.IsChecked = false; //把所有按钮选中状态改为未选中
                brush.IsChecked = false;
                braser.IsChecked = false;
                if (PointInRect(position,new Rect(rect[0],rect[1],rect[2],rect[3]))) //按下的时候必须要在选区范围内
                {
                    Cut_Click(sender, e);//调用剪切事件
                }
                
            }
            mark = true;//标志 鼠标按下
        }
        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePoint = e.GetPosition(image);//右键时获得鼠标坐标
        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (mark)
            {
                Point position2 = e.GetPosition(image);//真实的绘制用的点集
                Point position = e.GetPosition(grid1);//虚拟点集
                double px = position.X;
                double py = position.Y;
                double[] xs = makeRectangle(visualx, visualy, px, py);
                if (tbtn == DrawRectBtn && tbtn.IsChecked == true)
                {
                    rectangle1.Width = xs[2];
                    rectangle1.Height = xs[3];
                    rectangle1.Margin = new Thickness(xs[0], xs[1], 0, 0);
                 }
                if (tbtn == DrawEllipseBtn && tbtn.IsChecked == true)
                {   ellipse1.Width = xs[2];
                    ellipse1.Height = xs[3];
                    ellipse1.Margin = new Thickness(xs[0], xs[1], 0, 0);//这里是视觉上出现画圆的效果
                }
                if (brush.IsChecked == true||braser.IsChecked==true)
                {
                    drawPointCollection.Add(position);
                    myPointCollection.Add(position2);
                    line.Points = myPointCollection;
                }
            }
        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mark = false;
            //rectangle1.Visibility = Visibility.Hidden;
        }
        private void Window_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            mark = false;
            Point position = e.GetPosition(image);
            double px = position.X;
            double py = position.Y;
            double[] xs = makeRectangle(x, y, px, py);
            //椭圆
            if (tbtn == DrawEllipseBtn && tbtn.IsChecked == true)  //如果是画椭圆
            {
                //当鼠标抬起时 获取视觉上画圆得出来的坐标和大小 正式使用geometry画圆
                ellipse1.Visibility = Visibility.Hidden;
                Point p = new Point(xs[0] + xs[2] / 2, xs[1] + xs[3] / 2);
                //拿到中心点 为画圆做准备
                EllipseGeometry ellipseGeometry = new EllipseGeometry();
                ellipseGeometry.Center = p;
                ellipseGeometry.RadiusX = xs[2] / 2;
                ellipseGeometry.RadiusY = xs[3] / 2;
                GeometryDrawing g = new GeometryDrawing();  //绘制多边形用的类
                g.Geometry = ellipseGeometry;      //设定图形
                g.Pen = new Pen(new SolidColorBrush(brushColor), 2); ;   //设定笔触
                //把这个加入复合组中
                Drawimage(g);
                
            }
            //选区
            if (tbtn == DrawRectBtn && tbtn.IsChecked == true)
            {
                //rectangle1.Visibility = Visibility.Hidden;
                rect = makeRectangle(x, y, px, py); //得到了矩形框
                MessageBox.Show(rect[0].ToString() + " " + rect[1].ToString() + " " + rect[2].ToString() + " " + rect[3].ToString());
                isSelect = true; //进入选区模式
                tbtn.IsChecked = false;

            }
            //如果是画笔或者橡皮擦
            if (brush.IsChecked==true||braser.IsChecked ==true)
            {
                Pen pen = new Pen();
                line.Visibility = Visibility.Hidden;
                if ((bool)brush.IsChecked)
                {
                    pen.Brush = new SolidColorBrush(brushColor);
                    pen.Thickness = brushSlider.Value;
                }
                else
                {
                    pen.Brush = Brushes.White;
                    pen.Thickness = braserSlider.Value;
                }
                pen.Freeze();
                GeometryGroup lineGroup = new GeometryGroup(); //存放

                switch (myBrushBraserShape)
                {
                    //绘制的时候判断一下在不在选区内
                    case BrushBraserShape.line:
                    {
                        for (int i = 0; i < drawPointCollection.Count - 1; i++)
                        {
                            LineGeometry l = new LineGeometry();
                            if (isSelect) //选取状态下
                            {
                                if (PointInRect(drawPointCollection[i],new Rect(rect[0],rect[1],rect[2],rect[3])))//判断点在不在选区范围内
                                {
                                    l.StartPoint = drawPointCollection[i];
                                    l.EndPoint = drawPointCollection[i + 1];
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                l.StartPoint = drawPointCollection[i];
                                l.EndPoint = drawPointCollection[i + 1];

                             }
                            lineGroup.Children.Add(l);//存入组
                            }
                    }
                        break;
                    case BrushBraserShape.circle:
                    {
                        for (int i = 0; i < drawPointCollection.Count - 1; i++)
                        {
                            EllipseGeometry el = new EllipseGeometry();
                            if (isSelect)
                            {
                                if (PointInRect(drawPointCollection[i], new Rect(rect[0], rect[1], rect[2], rect[3])))//判断点在不在选区范围内
                                {
                                    el.Center = drawPointCollection[i];
                                    el.RadiusX = pen.Thickness / 2;
                                    el.RadiusY = pen.Thickness / 2;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                el.Center = drawPointCollection[i];
                                el.RadiusX = pen.Thickness / 2;
                                el.RadiusY = pen.Thickness / 2;
                            }
                               
                            lineGroup.Children.Add(el);
                        }
                    }
                        break;
                    case BrushBraserShape.rect:
                    {
                        for (int i = 0; i < drawPointCollection.Count - 1; i++)
                        {
                            RectangleGeometry el = new RectangleGeometry();
                            if (isSelect)
                            {
                                if (PointInRect(drawPointCollection[i], new Rect(rect[0], rect[1], rect[2], rect[3])))
                                {
                                    el.Rect = new Rect(drawPointCollection[i],
                                        new Size(pen.Thickness / 2, pen.Thickness / 2));
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                el.Rect = new Rect(drawPointCollection[i],
                                    new Size(pen.Thickness / 2, pen.Thickness / 2));
                             }
                           
                            lineGroup.Children.Add(el);
                        }
                    }
                        break;
                }
              //
                GeometryDrawing g = new GeometryDrawing();
                g.Geometry = lineGroup;
                g.Pen = pen;
                g.Brush = pen.Brush;
                Drawimage(g);
                
            }
            //移动选区
            if (isSelect && SelectMoveBtn.IsChecked == true) // 在选区可移动状态下 这里是松开鼠标 粘贴
            {
                Point offset = Offset(new Point(x, y), position);//算出偏移
                //矩形选框位置加上偏移量
                rect[0] += offset.X;
                rect[1] += offset.Y;
                stickImage(new Rect(rect[0], rect[1], rect[2], rect[3]));
            }
        }
        /// <summary>
        /// 修改画笔和橡皮擦形状
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShapeBtn_Checked(object sender, RoutedEventArgs e)
        {
            sbtn.IsChecked = false;
            sbtn = (ToggleButton)sender;
            if (sender==RectBtn) myBrushBraserShape = BrushBraserShape.rect;
            else if (sender == EllipseBtn) myBrushBraserShape = BrushBraserShape.circle;
            else if (sender == LineBtn) myBrushBraserShape = BrushBraserShape.line;
            
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (rectangle1.Visibility == Visibility.Hidden)
            {
                isSelect = false;
            }
            //按钮状态同步
            if (!isSelect)
            {
                SelectMoveBtn.IsChecked = false;
            }
            SelectMoveBtn.IsEnabled = isSelect;
            deleteSelect.IsEnabled = isSelect;
            cancelSelect.IsEnabled = isSelect;
            copy.IsEnabled = isSelect;
            cut.IsEnabled = isSelect;
            copy1.IsEnabled = isSelect;
            cut1.IsEnabled = isSelect;
            //mousePoint = e.GetPosition(image);
        }
    }
}
