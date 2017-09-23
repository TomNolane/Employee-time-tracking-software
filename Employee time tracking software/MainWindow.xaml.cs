using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Documents;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Data.Common;
using System.Windows.Media.Imaging;

namespace Employee_time_tracking_software
{
    public partial class MainWindow : Window
    {
        //option for local testing without internet connection.
        private const bool Test = true;

        private const int TimeIntervalScreenShot_Minuts = 3;

        private DateTime dt = new DateTime();
        private DateTime dt_day = new DateTime();
        private DateTime dynamic_time = new DateTime();
        private DateTime dynamic_time_day = new DateTime();

        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer timer_shot = new DispatcherTimer();
        private Stopwatch stopWatch = new Stopwatch();

        private FlowDocument ObjFdoc = new FlowDocument();
        private Paragraph ObjParag = new Paragraph();

        private string databaseName = Environment.CurrentDirectory + @"\worker.db";
        private string databaseName2 = Environment.CurrentDirectory + @"\timer.db";

        private List<object> ls1 = new List<object>();
        private List<object> ls2 = new List<object>();

        public MainWindow()
        {
            InitializeComponent();

            timer_shot.Interval = new TimeSpan(0, TimeIntervalScreenShot_Minuts, 0);
            timer_shot.Tick += (e, t) => { TakeScreenShot(); };
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            if (!File.Exists(databaseName))
            {
                SQLiteConnection.CreateFile(databaseName);
                SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseName));
                SQLiteCommand command = new SQLiteCommand(@"CREATE TABLE [workers] (
                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [name] char(50) NOT NULL,
                    [family] char(50) NOT NULL,
                    [age] int NOT NULL,
                    [email] char(100) NOT NULL,
                    [skype] char(50) NOT NULL,
                    [facebook] char(50) NOT NULL,
                    [other] text NULL
                    );", connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                if (Test)
                {
                    SQLiteCommand command2 = new SQLiteCommand("INSERT INTO 'workers' ('name','family','age','email','skype','facebook','other') VALUES ('Example_man_name','Example_man_family',29,'example_man@email.com','example.man','example.man@facebook.com', 'some example text');", connection);
                    connection.Open();
                    command2.ExecuteNonQuery();
                    connection.Close();
                }
            }

            if (!File.Exists(databaseName2))
            {
                SQLiteConnection.CreateFile(databaseName2);
                SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseName2));
                SQLiteCommand command = new SQLiteCommand(@"CREATE TABLE [times] (
                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [id_worker] integer NOT NULL,
                    [time_ticks] char(50) NOT NULL,
                    [time_ticks_day] char(50) NOT NULL
                    );", connection); //DEFAULT '0'
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                if (Test)
                {
                    SQLiteCommand command2 = new SQLiteCommand("INSERT INTO 'times' ('id','id_worker','time_ticks','time_ticks_day') VALUES (1,1,'0','0');", connection);
                    connection.Open();
                    command2.ExecuteNonQuery();
                    connection.Close();
                }
            }

            if (Test)
            {
                SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseName2));
                SQLiteCommand command3 = new SQLiteCommand("SELECT * FROM times WHERE id_worker=1;", connection); //WHERE 'id_worker'=1  ORDER BY id DESC LIMIT 1
                connection.Open();
                SQLiteDataReader reader = command3.ExecuteReader();

                string time_ticks = string.Empty;
                string time_ticks_day = string.Empty;

                foreach (DbDataRecord record in reader)
                {
                    time_ticks = record["time_ticks"].ToString();
                    time_ticks_day = record["time_ticks_day"].ToString();
                }
                connection.Close();

                dt = new DateTime();
                dt = dt.AddTicks(long.Parse(time_ticks));

                dt_day = new DateTime();
                dt_day = dt_day.AddTicks(long.Parse(time_ticks_day));

                if (dt.Month != DateTime.Now.Month)
                {

                    dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
                    time_ticks = dt.Ticks.ToString();

                    SQLiteCommand command4 = new SQLiteCommand("UPDATE 'times' SET id_worker=1,time_ticks='" + time_ticks + "' WHERE id_worker=1;", connection);
                    connection.Open();
                    command4.ExecuteNonQuery();
                    connection.Close();
                }
                else
                {
                    time_ticks = dt.Ticks.ToString();
                }
            }

            ls1.Add(border_1);
            ls1.Add(border_2);
            ls1.Add(textBlock);
            ls1.Add(textBlock2);
            ls1.Add(myButton);

            ls2.Add(rect);
            ls2.Add(button_start);
            ls2.Add(label_timer_now);
            ls2.Add(label_b1);
            ls2.Add(label_b2);
            ls2.Add(textBlock_b1);
            ls2.Add(textBlock_b2);

            ShowElements(ls1);
            richTextBox.Visibility = Visibility.Hidden; // delete
            AddLabelText("Employee time tracking software is running!");
        }

        private void TakeScreenShot(bool b = false, string s = "")
        {
            //System.Drawing.Size sz = Screen.PrimaryScreen.Bounds.Size;
            //IntPtr hDesk = GetDesktopWindow();
            //IntPtr hSrce = GetWindowDC(hDesk);
            //IntPtr hDest = CreateCompatibleDC(hSrce);
            //IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            //IntPtr hOldBmp = SelectObject(hDest, hBmp);
            //bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            //Bitmap bmp = Bitmap.FromHbitmap(hBmp);
            //SelectObject(hDest, hOldBmp);
            //DeleteObject(hBmp);
            //DeleteDC(hDest);
            //ReleaseDC(hDesk, hSrce);
            //bmp.Save(@"c:\temp\test.png");
            //bmp.Dispose(); 

            string path = Environment.CurrentDirectory + @"\ScreenShot_" + DateTime.Now.ToShortDateString();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Thread thread = new Thread(() =>
            {
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                    }

                    string filename = string.Empty;

                    if (b) filename = "ScreenCapture-" + DateTime.Now.ToString("ddMMyyyy_HH_mm_ss") + "_" + s + ".png";
                    else filename = "ScreenCapture-" + DateTime.Now.ToString("ddMMyyyy_HH_mm_ss") + ".png";

                    bmp.Save(Path.Combine(path, filename), ImageFormat.Jpeg);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();


            //int y = 0;
            //Thread thread = new Thread(() =>
            //{
            //    foreach(var screen in Screen.AllScreens)
            //    {
            //        Thread.Sleep(1000);

            //        int screenLeft = screen.Bounds.Left;  
            //        int screenTop = screen.Bounds.Top;
            //        int screenWidth = screen.Bounds.Width;
            //        int screenHeight = screen.Bounds.Height;

            //        using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
            //        {
            //            using (Graphics g = Graphics.FromImage(bmp))
            //            {
            //                g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
            //            }

            //            string filename = "ScreenCapture-" + DateTime.Now.ToString("ddMMyyyy-HHmmss_")+ y + ".png";
            //            y++;
            //            bmp.Save(Path.Combine(path, filename), ImageFormat.Jpeg);
            //        }
            //    }

            //});
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start(); 
        }

        // P/Invoke declarations
        //[DllImport("gdi32.dll")]
        //static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        //wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        //[DllImport("user32.dll")]
        //static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        //[DllImport("gdi32.dll")]
        //static extern IntPtr DeleteDC(IntPtr hDc);
        //[DllImport("gdi32.dll")]
        //static extern IntPtr DeleteObject(IntPtr hDc);
        //[DllImport("gdi32.dll")]
        //static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        //[DllImport("gdi32.dll")]
        //static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        //[DllImport("gdi32.dll")]
        //static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        //[DllImport("user32.dll")]
        //public static extern IntPtr GetDesktopWindow();
        //[DllImport("user32.dll")]
        //public static extern IntPtr GetWindowDC(IntPtr ptr);

        private void ShowElements(List<object> ls)
        {
            foreach (object t in ls)
            {
                if (t is Border)
                {
                    (t as Border).Visibility = Visibility.Visible;
                }
                else if (t is System.Windows.Controls.Button)
                {
                    (t as System.Windows.Controls.Button).Visibility = Visibility.Visible;
                }
                else if (t is TextBlock)
                {
                    (t as TextBlock).Visibility = Visibility.Visible;
                }
                else if (t is System.Windows.Controls.Label)
                {
                    (t as System.Windows.Controls.Label).Visibility = Visibility.Visible;
                }
                else if (t is System.Windows.Shapes.Rectangle)
                {
                    (t as System.Windows.Shapes.Rectangle).Visibility = Visibility.Visible;
                }
            }
        }

        private void HideElements(List<object> ls)
        {
            foreach (object t in ls)
            {
                if (t is Border)
                {
                    (t as Border).Visibility = Visibility.Hidden;
                }
                else if (t is System.Windows.Controls.Button)
                {
                    (t as System.Windows.Controls.Button).Visibility = Visibility.Hidden;
                }
                else if (t is TextBlock)
                {
                    (t as TextBlock).Visibility = Visibility.Hidden;
                }
                else if (t is System.Windows.Controls.Label)
                {
                    (t as System.Windows.Controls.Label).Visibility = Visibility.Hidden;
                }
                else if (t is System.Windows.Shapes.Rectangle)
                {
                    (t as System.Windows.Shapes.Rectangle).Visibility = Visibility.Hidden;
                }
            }
        }

        private async Task<string> GET(string url, string referer = "")
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Get;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:50.0) Gecko/20100101 Firefox/50.0";
                request.AllowAutoRedirect = false;
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;
                request.KeepAlive = true;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Accept = "text/html,application/json,application/xml;q=0.9,*/*;q=0.8";

                return await Task.Run(() =>
                {
                    string resp = RESPONSE(request);
                    return resp;
                });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string RESPONSE(HttpWebRequest request)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var headers = response.Headers.ToString();
                string answer = string.Empty;

                if (Convert.ToInt32(response.StatusCode) == 302 || Convert.ToInt32(response.StatusCode) == 200)
                {
                    using (Stream rspStm = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(rspStm, Encoding.UTF8, true))
                        {
                            answer = reader.ReadToEnd();
                        }
                    }
                    answer = System.Text.RegularExpressions.Regex.Unescape(answer);
                    return answer;
                }
                else
                {
                    response.Close(); return WebUtility.HtmlDecode(response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                return WebUtility.HtmlDecode(ex.Message);
            }
        }

        private async Task<string> POST(string url, string postData, NameValueCollection nvc = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Post;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:50.0) Gecko/20100101 Firefox/50.0";
                request.AllowAutoRedirect = true;
                request.ProtocolVersion = HttpVersion.Version11;
                request.AllowWriteStreamBuffering = true;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                if (postData != string.Empty) { request.ContentType = "application/x-www-form-urlencoded"; }
                else { request.ContentType = "multipart/form-data; boundary=" + boundary; }
                request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                // if (referer != "") { request.Referer = System.Net.WebUtility.UrlEncode(referer); }

                if (nvc != null)
                {
                    Stream rs = request.GetRequestStream();

                    byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    string _key = string.Empty;
                    foreach (string key in nvc.Keys)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, nvc[key]);
                        byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                        rs.Write(formitembytes, 0, formitembytes.Length);
                    }
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                }

                if (postData != string.Empty)
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = byteArray.Length;
                    Stream newStream = request.GetRequestStream();
                    newStream.Write(byteArray, 0, byteArray.Length);
                    newStream.Close();
                }

                return await Task.Run(() =>
                {
                    string resp = RESPONSE(request);
                    return resp;
                });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_username.Text) || textBox_username.Text.ToLower().Contains("username") || string.IsNullOrWhiteSpace(textBox_password.Password))
            {
                AddLabelErrorText(string.IsNullOrWhiteSpace(textBox_username.Text) || textBox_username.Text.ToLower().Contains("username") ? "Username must be not empty!" : string.IsNullOrWhiteSpace(textBox_password.Password) ? "Password must be not empty!" : "");
                return;
            }
            else if (Test)
            {
                HideElements(ls1);
                ShowElements(ls2);

                AddLabelText("You have successfully entered!");

                textBlock_b1.Text = GetMonthTime("0:0:0");
                textBlock_b2.Text = GetDayTime("0:0:0");
                TakeScreenShot(true, "start");
                return;
            }

            HideElements(ls1);

            AddLabelText("Connecting ... please wait...");

            string url = "https://example.com";

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("username", textBox_username.Text.ToString());
            nvc.Add("password", textBox_password.Password.ToString());

            string result = string.Empty;

            Task.Run(async () =>
            {
                result = await POST(url, "", nvc);
            });

            if (string.IsNullOrWhiteSpace(result))
            {
                ShowElements(ls1);
                AddLabelErrorText("Incorrect login or password!");
            }
            else
            {
                ShowElements(ls2);
                AddLabelText("Connecting ... please wait...");
            }
        }

        private string GetDayTime(string s)
        {
            if (dt_day.Day != DateTime.Now.Day)
            {
                dt_day = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }

            dynamic_time_day = new DateTime(DateTime.Now.Year, DateTime.Now.Month, dt_day.Day, dt_day.Hour, dt_day.Minute, dt_day.Second).AddHours(int.Parse(s.Split(new char[] { ':' })[0])).AddMinutes(int.Parse(s.Split(new char[] { ':' })[1])).AddSeconds(int.Parse(s.Split(new char[] { ':' })[2]));

            return AddZero(dynamic_time.Hour) + ":" + AddZero(dynamic_time_day.Minute) + ":" + AddZero(dynamic_time_day.Second);
        }

        private string GetMonthTime(string s)
        {
            if (dt.Month != DateTime.Now.Month)
            {
                dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
            }

            dynamic_time = new DateTime(DateTime.Now.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second).AddHours(int.Parse(s.Split(new char[] { ':' })[0])).AddMinutes(int.Parse(s.Split(new char[] { ':' })[1])).AddSeconds(int.Parse(s.Split(new char[] { ':' })[2]));

            int hour = 0;

            if (dynamic_time.Day >= 2) hour = (dynamic_time.AddDays(-1).Day * 24) + dynamic_time.Hour;
            else hour = dt.Hour;

            return AddZero(hour) + ":" + AddZero(dynamic_time.Minute) + ":" + AddZero(dynamic_time.Second);
        }

        private void textBox_username_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox_username.Text))
            {
                if (textBox_username.Text.ToLower().Contains("username"))
                {
                    textBox_username.Text = string.Empty;
                }
            }
        }

        private void textBox_username_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_username.Text))
            {
                textBox_username.Text = "Username";
            }
        }

        private void textBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as TextBlock).Foreground = System.Windows.Media.Brushes.Gold;
        }

        private void textBlock_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var t = new System.Windows.Media.Color();
            t.A = 100;
            t.B = 161;
            t.G = 152;
            t.R = 149;
            (sender as TextBlock).Foreground = new SolidColorBrush(t);
        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Border).BorderBrush = System.Windows.Media.Brushes.Gold;
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var t = new System.Windows.Media.Color();
            t.A = 100;
            t.B = 161;
            t.G = 152;
            t.R = 149;
            (sender as Border).BorderBrush = new SolidColorBrush(t);
        }

        private void textBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_username.Text.ToString()))
            {
                AddLabelErrorText("First, enter your username or email");
                return;
            }

            string url = "https://example.com";
            string result = string.Empty;

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("username", textBox_username.Text.ToString());

            Task.Run(async () =>
            {
                result = await POST(url, "", nvc);
            });

            if (string.IsNullOrWhiteSpace(result)) AddLabelErrorText("First, enter your username or email");
            else AddLabelText("Mail has been send to you email for restore");
        }

        private void AddLabelText(string s)
        {
            label_info.Dispatcher.Invoke(() => {
                label_info.Content = s;
                label_info.Foreground = System.Windows.Media.Brushes.Black;
                label_info.ToolTip = s;
            });
        }

        private void AddLabelErrorText(string s)
        {
            label_info.Dispatcher.Invoke(() => {
                label_info.Content = s;
                label_info.Foreground = System.Windows.Media.Brushes.Red;
                label_info.ToolTip = s;
            });
        }

        private void textBlock2_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://example.com");
        }

        private void button_start_Click(object sender, RoutedEventArgs e)
        {
            label_info.Foreground = System.Windows.Media.Brushes.Black;
            button_start.Visibility = Visibility.Hidden;
            button_stop.Visibility = Visibility.Visible;

            stopWatch.Start();

            timer_shot.IsEnabled = true;
            timer_shot.Start();

            timer.IsEnabled = true;
            int temp = 0;

            timer.Tick += (o, t) =>
            {
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                if (DateTime.Now.Hour == 23 & DateTime.Now.Minute == 59 && DateTime.Now.Second == 59 && DateTime.Now.Millisecond >= 400)
                {
                    stopWatch.Reset();
                }
                else if (ts.Hours > DateTime.Now.Hour)
                {
                    stopWatch.Reset();
                }

                string elapsedTime = string.Format("{0}:{1}:{2}", AddZero(ts.Hours), AddZero(ts.Minutes), AddZero((ts.Seconds)));

                label_timer_now.Content = elapsedTime;

                textBlock_b1.Text = GetMonthTime(elapsedTime);
                textBlock_b2.Text = GetDayTime(elapsedTime);

                if (temp >= 0 && temp < 3)
                {
                    AddLabelText("Employee time tracking is start"); temp++;
                }
                else if (temp >= 3 && temp < 6)
                {
                    AddLabelText("Employee time tracking is start."); temp++;
                }
                else if (temp >= 6 && temp < 9)
                {
                    AddLabelText("Employee time tracking is start.."); temp++;
                }
                else if (temp >= 9 && temp < 12)
                {
                    AddLabelText("Employee time tracking is start..."); temp++;
                }
                else
                {
                    temp = 0;
                }

                stopWatch.Start();
            };
            timer.Start();
        }

        private string AddZero(int i)
        {
            string result = i.ToString();

            if (i.ToString().Length == 1) result = "0" + i;
            else if (i == 0) result = "00";

            return result;
        }

        private void textBlock_b2_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ColorAnimation ca = new ColorAnimation(System.Windows.Media.Color.FromArgb(100, 149, 152, 154), Colors.Black, new Duration(TimeSpan.FromSeconds(1)));
            SolidColorBrush scb = new SolidColorBrush();
            scb.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            TextEffect tfe = new TextEffect();
            tfe.Foreground = scb;
            tfe.PositionStart = 0;
            tfe.PositionCount = int.MaxValue;
            (sender as TextBlock).TextEffects = new TextEffectCollection();
            (sender as TextBlock).TextEffects.Add(tfe);
        }

        private void textBlock_b2_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ColorAnimation ca = new ColorAnimation(Colors.Black, System.Windows.Media.Color.FromArgb(100, 149, 152, 154), new Duration(TimeSpan.FromSeconds(1)));
            SolidColorBrush scb = new SolidColorBrush();
            scb.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            TextEffect tfe = new TextEffect();
            tfe.Foreground = scb;
            tfe.PositionStart = 0;
            tfe.PositionCount = int.MaxValue;
            (sender as TextBlock).TextEffects = new TextEffectCollection();
            (sender as TextBlock).TextEffects.Add(tfe);
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Stop();
            timer.IsEnabled = false;
            timer.Stop();
            timer_shot.IsEnabled = false;
            timer_shot.Stop();

            var time_ticks = dynamic_time.Ticks.ToString();
            var time_ticks_day = dynamic_time_day.Ticks.ToString();

            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseName2));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("UPDATE times SET time_ticks='" + time_ticks + "',time_ticks_day='" + time_ticks_day + "' WHERE id_worker=1;", connection);
            command.ExecuteNonQuery();
            connection.Close();

            AddLabelText("Employee time tracking on pause");
            button_start.Visibility = Visibility.Visible;
            button_stop.Visibility = Visibility.Hidden;
        }

        private void label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (richTextBox.Visibility != Visibility.Visible)
            {
                AddLabelText("Show list of your screenshots");
                HideElements(ls2);
                richTextBox.Visibility = Visibility.Visible;
                AddScreenshotsToRichTextBox();
            }
            else
            {
                AddLabelText("Show employee time tracking software");
                ShowElements(ls2);
                richTextBox.Visibility = Visibility.Hidden;
            }
        }

        private void label_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            label.Foreground = System.Windows.Media.Brushes.DarkBlue;
            label.Background = System.Windows.Media.Brushes.Yellow;
        }

        private void label_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            label.Foreground = System.Windows.Media.Brushes.Black;
            label.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void AddScreenshotsToRichTextBox()
        { 
            richTextBox.Document.Blocks.Clear();
            ObjParag = new Paragraph();
            List<string> ls_shots = new List<string>();
            var directories = Directory.GetDirectories(Environment.CurrentDirectory);
            foreach (var directory in directories)
            {
                if (directory.Contains("ScreenShot_"))
                {
                    var files = Directory.GetFiles(directory);
                    foreach (var file in files)
                    {
                        ls_shots.Add(file);
                    }
                }
            }

            foreach (var temp in ls_shots)
            {
                var f = File.GetCreationTime(temp); bool isToDay = false;
                if (DateTime.Now.Day == new DateTime(f.Ticks).Day)
                    isToDay = true;
                System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
                btn.Uid = temp;
                if (!isToDay) btn.Content = "Delete " + f;
                else btn.Content = "Delete today " + f;
                var margin = btn.Margin;
                margin.Left = (richTextBox.Width / 2) - 100;
                btn.Margin = margin;  
                btn.Click += btn_Click; 

                var bitmap_img = new BitmapImage();

                using (var stream = new FileStream(temp, FileMode.Open))
                {
                    bitmap_img.BeginInit();
                    bitmap_img.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap_img.StreamSource = stream;
                    bitmap_img.EndInit();
                    bitmap_img.Freeze(); 
                }

                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Source = bitmap_img;
                ObjParag.Inlines.Add(img);
                ObjParag.Inlines.Add("\n");
                ObjParag.Inlines.Add(btn);
                ObjParag.Inlines.Add("\n\n");
            }
            ObjFdoc.Blocks.Add(ObjParag);
            richTextBox.Dispatcher.Invoke(() =>
            {
                richTextBox.Document = ObjFdoc;
            });
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        { 
            try
            {
                Dispatcher.Invoke(() => { File.Delete((sender as System.Windows.Controls.Button).Uid); });
                AddScreenshotsToRichTextBox();
            }
           catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace);
                AddLabelErrorText(ex.Message);
            }
        }
    }

    public class PasswordBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty = DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));

        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty = DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }
}
