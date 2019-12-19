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
        private bool isSelect = false;//标志现在是选区模式

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


        public MainWindow()
        {
            InitializeComponent();
            
            _instance = this;
            tbtn = DrawLineBtn;
            /*ract1.Visibility = Visibility.Hidden;*/
        }

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
                    drawingGroup1.Children.Add(imageDrawing);
                    //新建drawingimage类 传入待绘制的复合图形 然后传给组件
                    DrawingImage drawingImage = new DrawingImage(drawingGroup1);
                    image.Source = bf;
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
            BitmapSource bitmap=BitmapSource.Create(1024,1024,96,96,PixelFormats.Pbgra32,null,array,1024*4);//创建一个位图
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
            drawingGroup1.Children.Add(g);
            //新建drawingimage类 传入待绘制的复合图形 然后传给组件
            DrawingImage drawingImage = new DrawingImage(drawingGroup1);
            image.Source = drawingImage;
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
            double width = origin.Width;
            double height = origin.Height;
            origin = Sharpening(origin);//可以修改传参
            ImageDrawing imageDrawing = new ImageDrawing();
            imageDrawing.Rect = new Rect(0, 0, width, height);
            imageDrawing.ImageSource = origin;
            drawingGroup1.Children.Clear();
            drawingGroup1.Children.Add(imageDrawing);
            //在控件中显示
            DrawingImage drawImage = new DrawingImage(drawingGroup1);
            image.Source = drawImage;
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
        private void MohuBtn_Click(object sender, RoutedEventArgs e) //模糊按钮事件
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)MyImagepixelX, (int)MyImagePixely, (int)MyImageDpiX,
                (int)MyImagedpiY, PixelFormats.Pbgra32);
            rtb.Render(image);
            origin = ImageHelper.ConventToBitmapImage(rtb);
            double width = origin.Width;
            double height = origin.Height;
            origin = GaussBlur(origin);//可以修改传参


            ImageDrawing imageDrawing = new ImageDrawing();
            imageDrawing.Rect = new Rect(0, 0, width, height);
            imageDrawing.ImageSource = origin;
            drawingGroup1.Children.Clear();
            drawingGroup1.Children.Add(imageDrawing);
            //在控件中显示
            DrawingImage drawImage = new DrawingImage(drawingGroup1);
            image.Source = drawImage;
        }


        /// <summary>
        /// 复制粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (rectangle1.Visibility ==Visibility.Visible)
            {
                try
                {
                    rectangle1.Visibility = Visibility.Hidden;//把标志选区的框隐藏
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
            //rectangle1.Visibility = Visibility.Hidden;//把标志选区的框隐藏
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                //粘贴之后显示粘贴区域
                rectangle1.Visibility = Visibility.Visible;
                rectangle1.Width = rect[2];
                rectangle1.Height = rect[3];
                rectangle1.Margin = new Thickness(50, 50, 0, 0);

                BitmapSource bs = Clipboard.GetImage();
                ImageDrawing imageDrawing = new ImageDrawing();
                imageDrawing.Rect =new Rect(50,50, rect[2], rect[3]);
                imageDrawing.ImageSource = bs;
                drawingGroup1.Children.Add(imageDrawing);
                DrawingImage d =new DrawingImage(drawingGroup1);
                image.Source = d;
                isSelect = true; //粘贴之后默认进入选区模式

                //rect = new double[4];//清空选区矩形框
            }
            
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            /*if (e.Delta<0&&scale.ScaleX<0.3&&scale.ScaleY<0.3)
            {
                return;
            }
            if (e.Delta > 0 && scale.ScaleX >1 && scale.ScaleY > 1)
            {
                return;
            }
            scale.ScaleX += (double)e.Delta / 1000;
            scale.ScaleY += (double)e.Delta / 1000;*/
        }
        /// <summary>
        /// 工具栏按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawLineBtn_Checked(object sender, RoutedEventArgs e)
        {
            tbtn.IsChecked = false;
            tbtn = (ToggleButton)sender;
            brush.IsChecked = false;
            braser.IsChecked = false;
        }
        private void DrawRectBtn_Checked(object sender, RoutedEventArgs e)
        {
            tbtn.IsChecked = false;
            tbtn = (ToggleButton)sender;
            brush.IsChecked = false;
            braser.IsChecked = false;
            rectangle1.Visibility = Visibility.Hidden;
           

        }
        private void DrawEllipseBtn_Checked(object sender, RoutedEventArgs e)
        {
            tbtn.IsChecked = false;
            tbtn = (ToggleButton)sender;
            brush.IsChecked = false;
            braser.IsChecked = false;


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
        private double[] rect; //记录矩形选区的坐标
        private PointCollection myPointCollection = new PointCollection();
        private PointCollection drawPointCollection = new PointCollection();
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (tbtn == DrawRectBtn && tbtn.IsChecked == true)
            {

                Point position = e.GetPosition(image);
                x = position.X;
                y = position.Y;
                rectangle1.Visibility = Visibility.Visible;


                //显示用
                Point position2 = e.GetPosition(grid1);
                visualx = position2.X;
                visualy = position2.Y;
                rectangle1.Width = 1;
                rectangle1.Height = 1;
                rectangle1.Margin = new Thickness(visualx, visualy, 0, 0);
                mark = true;
            }
            if (tbtn == DrawEllipseBtn && tbtn.IsChecked == true) //画椭圆
            {
                Point position = e.GetPosition(image);
                x = position.X;
                y = position.Y;
                ellipse1.Visibility = Visibility.Visible;

                Point position2 = e.GetPosition(grid1);
                visualx = position2.X;
                visualy = position2.Y;
                ellipse1.Width = 1;
                ellipse1.Height = 1;
                ellipse1.Margin = new Thickness(visualx, visualy, 0, 0);

                mark = true;
            }
            if (tbtn == DrawLineBtn && tbtn.IsChecked == true)  //如果是画线
            {
                Point position = e.GetPosition(image);
                x = position.X;
                y = position.Y;
                line.Visibility = Visibility.Visible;

                mark = true;
            }

            if (brush.IsChecked == true||braser.IsChecked==true)   //画笔或橡皮擦
            {
                line.Visibility = Visibility.Visible;

                Point position = e.GetPosition(image);
                drawPointCollection.Clear();
                drawPointCollection.Add(position);

                
                Point position2 = e.GetPosition(grid1);
                myPointCollection.Clear();
                myPointCollection.Add(position2);


                //设置画笔属性 颜色和大小
                line.Stroke = new SolidColorBrush((bool)brush.IsChecked?brushColor:Colors.White);
                line.StrokeThickness = (bool)brush.IsChecked?brushSlider.Value:braserSlider.Value;
                mark = true;
            }

        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (mark)
            {
                if (tbtn == DrawRectBtn && tbtn.IsChecked == true)
                {

                    Point position = e.GetPosition(grid1);
                    double px = position.X;
                    double py = position.Y;
                    double[] xs = makeRectangle(visualx, visualy, px, py);
                    rectangle1.Width = xs[2];
                    rectangle1.Height = xs[3];
                    rectangle1.Margin = new Thickness(xs[0], xs[1], 0, 0);


                }
                if (tbtn == DrawEllipseBtn && tbtn.IsChecked == true)
                {
                    Point position = e.GetPosition(grid1);
                    double px = position.X;
                    double py = position.Y;
                    double[] xs = makeRectangle(visualx, visualy, px, py);
                    ellipse1.Width = xs[2];
                    ellipse1.Height = xs[3];
                    ellipse1.Margin = new Thickness(xs[0], xs[1], 0, 0);//这里是视觉上出现画圆的效果
                }
                if (tbtn == DrawLineBtn && tbtn.IsChecked == true)
                {
                    Point position = e.GetPosition(grid1);
                    
                }
                if (brush.IsChecked == true||braser.IsChecked==true)
                {
                    Point position = e.GetPosition(image);//真实的绘制用的点集
                    drawPointCollection.Add(position);
                    Point position2 = e.GetPosition(grid1); //虚拟的 给用户看的
                    myPointCollection.Add(position2);
                    line.Points = myPointCollection;

                }
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mark = false;
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mark = false;
            //椭圆
            if (tbtn == DrawEllipseBtn && tbtn.IsChecked == true)  //如果是画椭圆
            {
                //当鼠标抬起时 获取视觉上画圆得出来的坐标和大小 正式使用geometry画圆
                ellipse1.Visibility = Visibility.Hidden;
                Point position = e.GetPosition(image);
                double px = position.X;
                double py = position.Y;
                double[] xs = makeRectangle(x, y, px, py);
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
                drawingGroup1.Children.Add(g);

                //在控件中显示
                DrawingImage drawImage = new DrawingImage(drawingGroup1);
                image.Source = drawImage;
            }

            //选区
            if (tbtn == DrawRectBtn && tbtn.IsChecked == true)
            {
                //rectangle1.Visibility = Visibility.Hidden;
                Point position = e.GetPosition(image);
                double px = position.X;
                double py = position.Y;
                rect = makeRectangle(x, y, px, py); //得到了矩形框
                MessageBox.Show(rect[0].ToString() + " " + rect[1].ToString() + " " + rect[2].ToString() + " " + rect[3].ToString());
                isSelect = true; //进入选区模式
            }

            //如果是画线
            if (tbtn == DrawLineBtn && tbtn.IsChecked == true) 
            {
                //line.Visibility = Visibility.Hidden;
                Point position = e.GetPosition(image);
                double px = position.X;
                double py = position.Y;
                LineGeometry lineGeometry = new LineGeometry();
                lineGeometry.EndPoint = new Point(px, py);
                lineGeometry.StartPoint = new Point(x, y);
                GeometryDrawing g = new GeometryDrawing();
                g.Geometry = lineGeometry;
                g.Pen = new Pen(new SolidColorBrush(brushColor), 2);
                drawingGroup1.Children.Add(g);
                //在控件中显示
                DrawingImage drawImage = new DrawingImage(drawingGroup1);
                image.Source = drawImage;
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
                GeometryGroup lineGroup = new GeometryGroup(); //存放直线
                for (int i = 0; i < drawPointCollection.Count-1; i++)
                {
                    LineGeometry l = new LineGeometry(drawPointCollection[i],drawPointCollection[i+1]);
                    lineGroup.Children.Add(l);//存入组
                }
                //
                GeometryDrawing g = new GeometryDrawing();
                g.Geometry = lineGroup;
                g.Pen = pen;
                drawingGroup1.Children.Add(g);
                //在控件中显示
                DrawingImage drawImage = new DrawingImage(drawingGroup1);
                image.Source = drawImage;
            }

          
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (brush.IsChecked==true||braser.IsChecked==true)
            {
                tbtn.IsChecked = false;
            }

            if (rectangle1.Visibility == Visibility.Hidden)
            {
                isSelect = false;
            }
        }
    }
}
