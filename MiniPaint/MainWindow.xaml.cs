using Microsoft.Win32;
using MiniPaint.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiniPaint
{
    #region 프로젝트 설명
    #region 개체
    ///     텍스트
    ///     도형 : 직선, 사각형, 원형
    ///     이미지
    #endregion
    #region 속성 및 제어
    ///     개체 선택 ㅇ
    ///     > 이미지 미구현
    ///     개체 생성
    ///     > 텍스트, 도형, 이미지 생성 가능
    ///     개체 삭제
    ///     > 모두 삭제만 가능
    ///     개체 위치 이동
    ///     > 이미지 빼고 가능
    ///     개체 크기 변경
    ///     > 미구현
    ///     텍스트 폰트 변경
    ///     > 가능
    #endregion
    #region 마우스 제어
    ///     개체 선택 및 이동
    ///     > 가능
    ///     개체 크기 변경 - 마우스 드래그
    ///     > 미구현
    #endregion
    #endregion

    public partial class MainWindow : Window
    {
        #region 필드
        // 시작, 기본 상태
        DrawMode MyDrawMode = DrawMode.not_Draw;
        ActionMode MyActionMode = ActionMode.stay;
        private Point prePosition = new Point(); // 시작점
        private Point nowPosition = new Point(); // 끝점
        private Point tempPosition = new Point(); // 임시
        private Point mvStartPosition = new Point(); // 도형 이동 시작점
        private Line line; // 라인
        private Rectangle rect, tempRect, hilightRect; // 사각형
        private Ellipse ellipse, tempEllipse; // 타원형
        private TextBox txtBox, _txtBox, tempTxtBox; // 텍스트 박스
        private Label lbl_txtBox, tempLabel; // 라벨
        private string lbl_txtBox_content;
        private SolidColorBrush preColor, nowColor = new SolidColorBrush(); // 색상
        private Rect hltRect; // 테두리 사각형
        private Border border, tempborder;
        private Canvas cvs;
        private List<string> imgList = new List<string>();
        private List<Image> imgCtrolList = new List<Image>();
        private bool isDragging = false; //드래그 여부
        private Point lastPoint; //마지막 포인트
        HitType hitType = HitType.NONE; // 히트 타입
        ContentControl cnttControl;
        //이미지 저장

        #endregion
        #region 메인윈도우
        public MainWindow()
        {
            InitializeComponent();
            #region 메뉴 이벤트
            // 직선
            mnItemLine.Click += mnItemLine_click;
            // 자유선
            mnItemPolyline.Click += mnItemPolyline_click;
            // 사각형
            mnItemRectangle.Click += mnItemRectangle_click;
            // 타원형
            mnItemElipse.Click += mnItemElipse_click;
            // 텍스트박스
            mnItemText.Click += mnItemText_click;
            // 텍스트박스 생성
            menuItem_txtBox.Click += menuItem_txtBox_click;
            // 폰트 - 이탤릭체
            menuItem_Font_Italic.Click += menuItem_Font_Italic_click;
            // 폰트 - 볼드
            menuItem_Font_Bold.Click += menuItem_Font_Bold_click;
            // 폰트 - 사이즈 업
            menuItem_Font_SizeUp.Click += menuItem_Font_SizeUp_click;
            // 폰트 - 사이즈 다운
            menuItem_Font_SizeDown.Click += menuItem_Font_SizeDown_click;
            // 이미지
            // 이미지 - 오픈 파일 다이얼로그
            mnItemImageSelect.Click += mnItemImageSelect_click;
            // 이미지 - 이미지 저장
            mnItemImageSave.Click += mnItemImageSave_click;
            #endregion
            #region 캔버스 마우스 이벤트
            Mycanvas.MouseDown += Mycanvas_MouseDown;
            Mycanvas.MouseMove += Mycanvas_MouseMove;
            Mycanvas.MouseUp += Mycanvas_MouseUp;
            #endregion
            #region 객체 속성
            preColor = Brushes.Red;
            nowColor = Brushes.Black;
            #endregion
        }
        #endregion
        #region 이벤트 - 윈도우
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //캔버스 배경 초기화
            Mycanvas.Background = Brushes.White;
        }
        #endregion
        #region 이벤트 - Line, Rectangle, Ellipse
        // 선 - 직선
        private void mnItemLine_click(object sender, RoutedEventArgs e)
        {
            hilightRect = null;
            border = null;
            MyDrawMode = DrawMode.line_st;
        }
        // 선 - 자유선
        private void mnItemPolyline_click(object sender, RoutedEventArgs e)
        {
            // 메뉴 제어를 위한 flag
            MyDrawMode = DrawMode.line_Poly;
        }

        // 도형 - 사각형
        private void mnItemRectangle_click(object sender, RoutedEventArgs e)
        {
            MyDrawMode = DrawMode.shape_rect;
            rect = null;
            tempRect = null;
            hilightRect = null;
            border = null;
        }
        // 도형 - 타원형
        private void mnItemElipse_click(object sender, RoutedEventArgs e)
        {
            MyDrawMode = DrawMode.shape_ellipse;
            ellipse = null;
            tempEllipse = null;
            hilightRect = null;
            border = null;
        }
        #endregion
        #region 이벤트 - 텍스트 박스
        // 텍스트 박스 - 모드 전환
        private void mnItemText_click(object sender, RoutedEventArgs e)
        {
            MyDrawMode = DrawMode.txt;
        }
        // 텍스트 박스 - 새로운 텍스트 박스 생성
        private void menuItem_txtBox_click(object sender, RoutedEventArgs e)
        {
            txtBox = null;
            lbl_txtBox = null;
        }
        // 텍스트 박스 - 캔버스 위에 라벨 생성
        private void txtBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                lbl_txtBox_content = txtBox.Text;
                lbl_txtBox.Content = lbl_txtBox_content;
                lbl_txtBox.HorizontalAlignment = HorizontalAlignment.Center;
                lbl_txtBox.VerticalAlignment = VerticalAlignment.Center;

                Mycanvas.Children.Remove(txtBox);
                border.Child = lbl_txtBox;
                border.BorderBrush = null;
                Mycanvas.Children.Add(border);
            }
        }
        // 폰트 - 이탤릭
        private void menuItem_Font_Italic_click(object sender, RoutedEventArgs e)
        {
            if (lbl_txtBox.FontStyle == FontStyles.Italic)
            {
                lbl_txtBox.FontStyle = FontStyles.Normal;
                //폰트
            }
            else
            {
                lbl_txtBox.FontStyle = FontStyles.Italic;
            }
        }
        // 폰트 - 볼드
        private void menuItem_Font_Bold_click(object sender, RoutedEventArgs e)
        {
            if (lbl_txtBox.FontWeight == FontWeights.Bold)
            {
                lbl_txtBox.FontWeight = FontWeights.Normal;
            }
            else
            {
                lbl_txtBox.FontWeight = FontWeights.Bold;
            }
        }
        // 폰트 - 사이즈 다운
        private void menuItem_Font_SizeDown_click(object sender, RoutedEventArgs e)
        {
            lbl_txtBox.FontSize -= 10;
        }
        // 폰트 - 사이즈 업
        private void menuItem_Font_SizeUp_click(object sender, RoutedEventArgs e)
        {
            lbl_txtBox.FontSize += 10;
        }

        #endregion
        #region 이벤트 - 이미지
        // 이미지
        // 이미지 - 이미지 모드 전환
        private void mnItemImageSelect_click(object sender, RoutedEventArgs e)
        {
            MyDrawMode = DrawMode.img;

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                string fullPath = ofd.FileName;
                string fileName = ofd.SafeFileName;
                string path = fullPath.Replace(fileName, "");
                string[] files = Directory.GetFiles(path);
                imgList = files.Where(x => x.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) >= 0
                                        || x.IndexOf(".png", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(x => x).ToList();
            }
            //이미지 생성
            CreateImage(imgList);
        }

        private void mnItemImageSave_click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)Mycanvas.ActualWidth, (int)Mycanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(Mycanvas);
            
            using (Stream stream = new FileStream(System.Environment.CurrentDirectory + @"\background.bmp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(stream);
            }
            // 경로 
            // C:\Users\REDT_DEV\Desktop\work\그림판\코드리뷰\MiniPaint\MiniPaint\bin\Debug
            MessageBox.Show("이미지 저장 완료");
        }

        #endregion
        #region 이벤트 - 캔버스
        // 마우스 다운
        private void Mycanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            prePosition = e.MouseDevice.GetPosition(Mycanvas);

            if (MyDrawMode != DrawMode.not_Draw && hitType == HitType.NONE)
            {
                switch (MyDrawMode)
                {
                    case DrawMode.line_st:
                        CreateLine();
                        CreateHilightRect();
                        CreateBorder();
                        break;
                    case DrawMode.line_Poly:
                        CreateHilightRect();
                        CreateCanvas();
                        CreateBorder();
                        break;
                    case DrawMode.shape_rect:
                        if (rect == null)
                        {
                            CreateRectangle();
                            CreateHilightRect();
                            CreateBorder();
                        }
                        break;
                    case DrawMode.shape_ellipse:
                        if (ellipse == null)
                        {
                            CreateEllipse();
                            CreateHilightRect();
                            CreateBorder();
                        }
                        break;
                    case DrawMode.txt:
                        if (txtBox == null)
                        {
                            CreateTextBox();
                            Create_txtBox_Label();
                            CreateHilightRect();
                            CreateBorder();
                        }
                        break;
                    case DrawMode.img:
                        break;
                    default:
                        break;
                }
            }
        }
        // 마우스 무브
        private void Mycanvas_MouseMove(object sender, MouseEventArgs e)
        {
            nowPosition = e.GetPosition(Mycanvas);
            lastPoint = nowPosition;

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed && isDragging != true)
            {
                switch (MyDrawMode)
                {
                    // 선 - 직선
                    case DrawMode.line_st:
                        line.X1 = prePosition.X;
                        line.Y1 = prePosition.Y;
                        line.X2 = nowPosition.X;
                        line.Y2 = nowPosition.Y;
                        if (hilightRect != null)
                        {
                            double left = nowPosition.X;
                            double top = nowPosition.Y;
                            if (nowPosition.X > prePosition.X)
                            {
                                left = prePosition.X;
                            }
                            if (nowPosition.Y > prePosition.Y)
                            {
                                top = prePosition.Y;
                            }
                            // 하이라이트 렉트
                            // 생성
                            hilightRect.Width = Math.Abs(nowPosition.X - prePosition.X);
                            hilightRect.Height = Math.Abs(nowPosition.Y - prePosition.Y);
                            Canvas.SetLeft(hilightRect, left);
                            Canvas.SetTop(hilightRect, top);
                            Mycanvas.Children.Remove(hilightRect);
                            // 보더 생성
                            border.BorderBrush = new VisualBrush(hilightRect);
                            //border.Width = hilightRect.Width;
                            //border.Height = hilightRect.Height;
                            
                            tempborder = border;

                            Canvas.SetLeft(border, left);
                            Canvas.SetTop(border, top);
                            // 보더 이벤트 등록
                            border.MouseDown += shape_Click;
                            border.MouseMove += shape_Move;
                            border.MouseUp += shape_Released;
                            
                            Mycanvas.Children.Remove(border);
                            
                            tempRect = hilightRect;
                        }
                        break;
                    case DrawMode.line_Poly:
                        // 자유선
                        line = new Line();
                        line.Stroke = nowColor;
                        line.StrokeThickness = 2;
                        line.X1 = nowPosition.X;
                        line.Y1 = nowPosition.Y;
                        line.X2 = prePosition.X;
                        line.Y2 = prePosition.Y;
                        prePosition = e.GetPosition(Mycanvas);
                        Mycanvas.Children.Add(line);
                        break;
                    case DrawMode.shape_rect: //사각형
                        if (rect != null)
                        {
                            double left = nowPosition.X;
                            double top = nowPosition.Y;
                            if (nowPosition.X > prePosition.X)
                            {
                                left = prePosition.X;
                            }
                            if (nowPosition.Y > prePosition.Y)
                            {
                                top = prePosition.Y;
                            }
                            rect.Width = Math.Abs(nowPosition.X - prePosition.X);
                            rect.Height = Math.Abs(nowPosition.Y - prePosition.Y);
                            Canvas.SetLeft(rect, left);
                            Canvas.SetTop(rect, top);
                            // 하이라이트 렉트
                            // 생성
                            hilightRect.Width = rect.Width;
                            hilightRect.Height = rect.Height;
                            Canvas.SetLeft(hilightRect, left);
                            Canvas.SetTop(hilightRect, top);
                            Mycanvas.Children.Remove(hilightRect);
                            // 보더 생성
                            border.BorderBrush = new VisualBrush(hilightRect);
                            border.Width = hilightRect.Width;
                            border.Height = hilightRect.Height;
                            Canvas.SetLeft(border, left);
                            Canvas.SetTop(border, top);
                            border.MouseDown += shape_Click;
                            border.MouseMove += shape_Move;
                            border.MouseUp += shape_Released;

                            Mycanvas.Children.Remove(border);
                            //Mycanvas.Children.Add(border);

                            tempRect = rect;
                        }
                        break;
                    case DrawMode.shape_ellipse: // 타원형
                        if (ellipse != null)
                        {
                            double left = nowPosition.X;
                            double top = nowPosition.Y;
                            if (nowPosition.X > prePosition.X)
                            {
                                left = prePosition.X;
                            }
                            if (nowPosition.Y > prePosition.Y)
                            {
                                top = prePosition.Y;
                            }
                            ellipse.Width = Math.Abs(nowPosition.X - prePosition.X);
                            ellipse.Height = Math.Abs(nowPosition.Y - prePosition.Y);
                            Canvas.SetLeft(ellipse, left);
                            Canvas.SetTop(ellipse, top);

                            // 하이라이트 렉트
                            // 생성
                            hilightRect.Width = ellipse.Width;
                            hilightRect.Height = ellipse.Height;
                            Canvas.SetLeft(hilightRect, left);
                            Canvas.SetTop(hilightRect, top);
                            Mycanvas.Children.Remove(hilightRect);
                            // 보더 생성
                            border.BorderBrush = new VisualBrush(hilightRect);
                            border.Width = hilightRect.Width;
                            border.Height = hilightRect.Height;
                            Canvas.SetLeft(border, left);
                            Canvas.SetTop(border, top);
                            border.MouseDown += shape_Click;
                            border.MouseMove += shape_Move;
                            border.MouseUp += shape_Released;

                            Mycanvas.Children.Remove(border);
                            //Mycanvas.Children.Add(border);

                            tempEllipse = ellipse;
                        }
                        break;
                    case DrawMode.txt:
                        if (txtBox != null)
                        {
                            double left = nowPosition.X;
                            double top = nowPosition.Y;
                            if (nowPosition.X > prePosition.X)
                            {
                                left = prePosition.X;
                            }
                            if (nowPosition.Y > prePosition.Y)
                            {
                                top = prePosition.Y;
                            }
                            txtBox.Width = Math.Abs(nowPosition.X - prePosition.X);
                            txtBox.Height = Math.Abs(nowPosition.Y - prePosition.Y);
                            Canvas.SetLeft(txtBox, left);
                            Canvas.SetTop(txtBox, top);

                            lbl_txtBox.Margin = txtBox.Margin;
                            lbl_txtBox.Width = txtBox.Width;
                            lbl_txtBox.Height = txtBox.Height;
                            // 하이라이트 렉트
                            hilightRect.Width = Math.Abs(nowPosition.X - prePosition.X);
                            hilightRect.Height = Math.Abs(nowPosition.Y - prePosition.Y);
                            Canvas.SetLeft(hilightRect, left);
                            Canvas.SetTop(hilightRect, top);
                            Mycanvas.Children.Remove(hilightRect);
                            // 하이라이트 보더
                            border.BorderBrush = new VisualBrush(hilightRect);
                            border.Width = hilightRect.Width;
                            border.Height = hilightRect.Height;
                            Canvas.SetLeft(border, left);
                            Canvas.SetTop(border, top);

                            border.MouseDown += shape_Click;
                            border.MouseMove += shape_Move;
                            border.MouseUp += shape_Released;

                            Mycanvas.Children.Remove(border);
                            
                            tempRect = hilightRect;

                            tempTxtBox = txtBox;
                            tempLabel = lbl_txtBox;
                        }
                        break;
                    case DrawMode.img:
                        break;
                    default:
                        break;
                }

            }
        }
        // 마우스 업
        private void Mycanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            nowPosition = e.GetPosition(Mycanvas);
            lastPoint = nowPosition;

            if (e.MouseDevice.LeftButton == MouseButtonState.Released && isDragging != true)
            {
                switch (MyDrawMode)
                {
                    case DrawMode.line_st:
                        line.Stroke = nowColor;
                        if ((line.X1 == line.X2) && (line.Y1 == line.Y2))
                        {
                            Mycanvas.Children.Remove(line);
                        }
                        Mycanvas.Children.Remove(line);
                        line.X1 = 0;
                        line.Y1 = 0;
                        line.X2 = border.Width;
                        line.Y2 = border.Height;
                        border.Child = line;
                        Mycanvas.Children.Remove(border);
                        Mycanvas.Children.Add(border);
                        border.Visibility = Visibility.Collapsed;
                        break;
                    case DrawMode.line_Poly:
                        break;
                    case DrawMode.shape_rect:
                        if (rect == tempRect)
                        {
                            rect.Stroke = nowColor;
                            rect.Fill = nowColor;
                            //Mycanvas.Children.Remove(rect);
                            //border.Child = rect;
                            //border.Width += 10;
                            //border.Height += 10;
                            //Mycanvas.Children.Remove(border);
                            //Mycanvas.Children.Add(border);
                        }
                        break;
                    case DrawMode.shape_ellipse:
                        if (ellipse == tempEllipse)
                        {
                            ellipse.Stroke = nowColor;
                            ellipse.Fill = nowColor;
                            //Mycanvas.Children.Remove(ellipse);
                            //border.Child = ellipse;
                            //border.Width += 10;
                            //border.Height += 10;
                            //Mycanvas.Children.Remove(border);
                            //Mycanvas.Children.Add(border);
                        }
                        break;
                    case DrawMode.txt:
                        break;
                    case DrawMode.img:
                        break;
                    default:
                        break;
                }
            }
            MyDrawMode = DrawMode.not_Draw;
            MyActionMode = ActionMode.move;
        }
        // 삭제
        private void menuItem_Remove_click(object sender, RoutedEventArgs e)
        {
            Mycanvas.Children.Clear();
        }
        #endregion
        #region 이벤트 - 객체
        // 이벤트 - 객체 클릭
        private void shape_Click(object sender, MouseButtonEventArgs e)
        {
            //Border sp = (Border)sender;
            this.hitType = GetHitType(this.border, e.GetPosition(this.Mycanvas));
            Border br = (Border)sender;
            br.CaptureMouse();

            SetMouseCursor();

            if (this.hitType == HitType.NONE)
            {
                return;
            }

            //마우스가 찍은 위치
            this.lastPoint = Mouse.GetPosition(this.Mycanvas);

            this.isDragging = true;
        }
        /*private void shape_Click(object sender, MouseButtonEventArgs e)
        {
            mvStartPosition = e.GetPosition(Mycanvas);

            MyActionMode = ActionMode.select;
            UIElement el = (UIElement)sender;
            
            el.CaptureMouse();
            el.Opacity = 0.475;
        }*/
        // 이벤트 - 객체 이동
        private void shape_Move(object sender, MouseEventArgs e)
        {

            if (!this.isDragging)
            {
                //드래그모드가 아닌 크기 조절
                //Mouse.GetPosition(Mycanvas)
                //마우스가 움직이고 멈춘 위치

                this.hitType = GetHitType(border, Mouse.GetPosition(Mycanvas));

                SetMouseCursor();
            }
            else
            {
                Point point = e.GetPosition(Mycanvas);
                object obj = (UIElement) sender;
                //Border sp = (Border)e.Source;

                double offsetX = point.X - this.lastPoint.X;
                double offsetY = point.Y - this.lastPoint.Y;

                double newX = Canvas.GetLeft(border);
                double newY = Canvas.GetTop(border);
                double newWidth = border.Width;
                double newHeight = border.Height;

                switch (this.hitType)
                {
                    case HitType.BODY:

                        newX += offsetX;
                        newY += offsetY;

                        break;

                    case HitType.UPPER_LEFT:

                        newX += offsetX;
                        newY += offsetY;

                        newWidth -= offsetX;
                        newHeight -= offsetY;

                        break;

                    case HitType.UPPER_RIGHT:

                        newY += offsetY;

                        newWidth += offsetX;
                        newHeight -= offsetY;

                        break;

                    case HitType.LOWER_RIGHT:

                        newWidth += offsetX;
                        newHeight += offsetY;

                        break;

                    case HitType.LOWER_LEFT:

                        newX += offsetX;

                        newWidth -= offsetX;
                        newHeight += offsetY;

                        break;

                    case HitType.LEFT:

                        newX += offsetX;

                        newWidth -= offsetX;

                        break;

                    case HitType.RIGHT:

                        newWidth += offsetX;

                        break;

                    case HitType.BOTTOM:

                        newHeight += offsetY;

                        break;

                    case HitType.TOP:

                        newY += offsetY;

                        newHeight -= offsetY;

                        break;
                }

                if ((newWidth > 0) && (newHeight > 0))
                {
                    Canvas.SetLeft(border, newX);
                    Canvas.SetTop(border, newY);

                    this.border.Width = newWidth;
                    this.border.Height = newHeight;

                    this.lastPoint = point;
                }
            }
        }
        /*private void shape_Move(object sender, MouseEventArgs e)
        {
            UIElement el = (UIElement)sender;

            if (!el.IsMouseCaptured)
            {
                return;
            }
            else
            {
                Point p = e.GetPosition(Mycanvas);
                Border sp = (Border)e.Source;
                Point pre_P = prePosition;
                Point cur_p = nowPosition;
                if ((e.MouseDevice.LeftButton == MouseButtonState.Pressed))
                {
                    MyActionMode = ActionMode.move;
                    double left = p.X - (sp.ActualWidth / 2);
                    double top = p.Y - (sp.ActualWidth / 2);
                    Canvas.SetLeft(sp, left);
                    Canvas.SetTop(sp, top);
                }
                MyActionMode = ActionMode.stay;
                tempPosition = p;
            }
        }*/
        // 이벤트 - 객체 놓음
        private void shape_Released(object sender, MouseEventArgs e)
        {
            Border br = (Border)sender;
            br.ReleaseMouseCapture();
            this.isDragging = false;

        }
        /*private void shape_Released(object sender, MouseEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
            el.Opacity = 1;
        }*/
        #endregion
        #region 선 - 직선
        private void CreateLine() 
        {
            line = new Line();
            line.Stroke = preColor;
            line.StrokeThickness = 2;
            line.Opacity = 1;
            Mycanvas.Children.Add(line);
        }
        #endregion
        #region 도형 - 사각형
        private void CreateRectangle()
        {
            rect = new Rectangle();
            rect.Stroke = preColor;
            rect.StrokeThickness = 2;
            rect.Opacity = 1;
            Mycanvas.Children.Add(rect);
        }
        #endregion
        #region Rect - 하이라이트
        private void CreateHilightRect()
        {
            hilightRect = new Rectangle();
            hilightRect.Stroke = preColor;
            hilightRect.StrokeThickness = 2;
            hilightRect.Opacity = 1;
            hilightRect.StrokeDashArray = new DoubleCollection() { 2 };
            Mycanvas.Children.Add(hilightRect);
        }
        #endregion
        #region 보더
        private void CreateBorder()
        {
            border = new Border();
            tempborder = new Border();
            border.BorderThickness = new Thickness(2);
            border.Background = Brushes.Transparent;
        }
        #endregion
        #region 캔버스
        private void CreateCanvas() 
        {
            cvs = new Canvas();
        }
        #endregion
        #region 도형 - 원형
        private void CreateEllipse()
        {
            ellipse = new Ellipse();
            ellipse.Stroke = preColor;
            ellipse.Fill = Brushes.Transparent;
            ellipse.StrokeThickness = 2;
            ellipse.Opacity = 1;
            Mycanvas.Children.Add(ellipse);
        }
        #endregion
        #region 텍스트박스
        private void CreateTextBox()
        {
            txtBox = new TextBox();
            txtBox.KeyDown += txtBox_KeyDown;
            txtBox.Opacity = 1;
            Mycanvas.Children.Add(txtBox);
        }
        #endregion
        #region 라벨
        private void Create_txtBox_Label()
        {
            lbl_txtBox = new Label();
            lbl_txtBox.Opacity = 1;
        }
        #endregion
        #region 이미지
        private void CreateImage(List<string> imgList) 
        {
            for (int i = 0; i < imgList.Count; i++)
            {
                Image image = new Image();
                CreateBitmap(image, imgList[i]);
                imgCtrolList.Add(image);
                Mycanvas.Children.Add(image);
            }
        }
        private void CreateBitmap(Image image, string imageList) 
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.CreateOptions = BitmapCreateOptions.DelayCreation;
            img.DecodePixelWidth = 500;
            img.UriSource = new Uri(imageList.ToString());
            img.EndInit();
            image.Source = img;
        }
        #endregion
        #region 히트 타입 구하기 - GetHitType(border, point)
        /// <summary>
        /// 히트 타입 구하기
        /// </summary>
        /// <param name="border">보더</param>
        /// <param name="point">포인트</param>
        /// <returns>히트 타입</returns>
        private HitType GetHitType(Border border, Point point)
        {
            double left = Canvas.GetLeft(this.border);
            double top = Canvas.GetTop(this.border);
            double right = left + this.border.Width;
            double bottom = top + this.border.Height;

            if (point.X < left)
            {
                return HitType.NONE;
            }

            if (point.X > right)
            {
                return HitType.NONE;
            }

            if (point.Y < top)
            {
                return HitType.NONE;
            }

            if (point.Y > bottom)
            {
                return HitType.NONE;
            }

            const double GAP = 10;

            if (point.X - left < GAP)
            {
                if (point.Y - top < GAP)
                {
                    return HitType.UPPER_LEFT;
                }

                if (bottom - point.Y < GAP)
                {
                    return HitType.LOWER_LEFT;
                }

                return HitType.LEFT;
            }

            if (right - point.X < GAP)
            {
                if (point.Y - top < GAP)
                {
                    return HitType.UPPER_RIGHT;
                }

                if (bottom - point.Y < GAP)
                {
                    return HitType.LOWER_RIGHT;
                }

                return HitType.RIGHT;
            }

            if (point.Y - top < GAP)
            {
                return HitType.TOP;
            }

            if (bottom - point.Y < GAP)
            {
                return HitType.BOTTOM;
            }

            return HitType.BODY;
        }
        #endregion
        #region 마우스 커서 설정하기 - SetMouseCursor()

        /// <summary>
        /// 마우스 커서 설정하기
        /// </summary>
        private void SetMouseCursor()
        {
            Cursor cursor = Cursors.Arrow;

            switch (this.hitType)
            {
                case HitType.NONE: cursor = Cursors.Arrow; break;
                case HitType.BODY: cursor = Cursors.ScrollAll; break;
                case HitType.UPPER_LEFT:
                case HitType.LOWER_RIGHT: cursor = Cursors.SizeNWSE; break;
                case HitType.LOWER_LEFT:
                case HitType.UPPER_RIGHT: cursor = Cursors.SizeNESW; break;
                case HitType.TOP:
                case HitType.BOTTOM: cursor = Cursors.SizeNS; break;
                case HitType.LEFT:
                case HitType.RIGHT: cursor = Cursors.SizeWE; break;
            }

            if (Cursor != cursor)
            {
                Cursor = cursor;
            }
        }
        #endregion
    }
}
