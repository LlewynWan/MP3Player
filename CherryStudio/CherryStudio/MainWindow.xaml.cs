using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.ComponentModel;

using WMPLib;
using System.Windows.Threading;

namespace CherryStudio
{
    public class musicInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    class Lyric
    {
        public int minute;
        public float second;
        public float totalSec;
        public string strLyric;
    }

    class LyricFile
    {
        public int pos = 0;
        public List<Lyric> lyrics = new List<Lyric>();

        public void LoadFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312"));

            string strLyric;

            while ((strLyric = sr.ReadLine()) != null)
            {
                if (strLyric == "")
                {
                    continue;
                }

                Lyric lyric = new Lyric();
                lyric.minute = int.Parse(strLyric.Substring(1, 2));
                lyric.second = float.Parse(strLyric.Substring(4, 5));
                lyric.totalSec = lyric.minute * 60 + lyric.second;
                lyric.strLyric = strLyric.Substring(10);

                lyrics.Add(lyric);
            }

            sr.Close();
            fs.Close();
        }
    }


    /// MainWindow.xaml 的交互逻辑
    public partial class MainWindow : Window
    {
        List<musicInfo> items = new List<musicInfo>();
        List<musicInfo> favourite_list = new List<musicInfo>();

        //private MediaPlayer mediaPlayer = new MediaPlayer();
        private WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        List<string> urlList = new List<string>();
        List<string> favorurlList = new List<string>();
        double max, min;//控制processbar
        DispatcherTimer timer = new DispatcherTimer();


        double currentTime;

        Thread PlayerLyricThread;
        LyricWindow lyric_window;

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            //timer.Start();
        }


        //进度条
        void timer_Tick(object sender,EventArgs e)
        {
            //musicProgress.Value = wplayer.controls.currentPosition;
            max = wplayer.currentMedia.duration;
            min = wplayer.controls.currentPosition;
            musicProgress.Maximum = (int)(max);
            musicProgress.Value = (int)(min);

        }


        //进度条拖动
        //private void musicProgress_KeyDown(object sender, KeyEventArgs e)
        //{
        //    while (wplayer.playState.Equals(3))
        //    {
        //        if (e.Key == Key.Right && (min + 10) <= max)
        //        {
        //            musicProgress.Value += 10;
        //        }
        //        else
        //        {
        //            musicProgress.Value = max;
        //        }
        //        if (e.Key == Key.Left && (min - 10) >= 0)
        //        {
        //            musicProgress.Value -= 10;
        //        }
        //        else
        //        {
        //            musicProgress.Value = 0;
        //        }
        //    }
        //}


        protected override void OnClosing(CancelEventArgs e)
        {
            PlayerLyricThread.Abort();
            wplayer.controls.stop();
            base.OnClosing(e);
        }

        //上传音乐
        private void uploadMusic_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Multiselect = true;
            of.Title = "请选择音乐文件";
            of.Filter = "(*.mp3)|*.mp3";
            if (of.ShowDialog() == true)
            {
                string[] nameList = of.FileNames;
                
                foreach (string url in nameList)
                {
                    int startIndex = url.LastIndexOf('\\') + 1;
                    int length = url.Length - startIndex - 4;
                    string name = url.Substring(startIndex, length);
                    items.Add(new musicInfo { Name = name, Url = url }) ;
                    urlList.Add(url);
                }
                musiclist.ItemsSource = items;
                musiclist.Items.Refresh();
            }
        }

        private void musiclist_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            int selectIndex = musiclist.SelectedIndex;
            if (selectIndex != -1)
            {
                if (musiclist.ItemsSource == items)
                {
                    wplayer.URL = urlList[selectIndex];
                    musicName.Text = items[selectIndex].Name.ToString();
                }
                if (musiclist.ItemsSource == favourite_list)
                {
                    wplayer.URL = favorurlList[selectIndex];
                    musicName.Text = favourite_list[selectIndex].Name.ToString();
                }

                wplayer.controls.play();               
                timer.Start();               


                if (lyric_window == null)
                {
                    lyric_window = new LyricWindow();
                    lyric_window.Owner = this;
                    lyric_window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    lyric_window.Show();
                }

                if (PlayerLyricThread != null && PlayerLyricThread.IsAlive)
                    PlayerLyricThread.Abort();
                PlayerLyricThread = new Thread(new ParameterizedThreadStart(LyricControl));
                if (musiclist.ItemsSource == items)
                {
                    PlayerLyricThread.Start(urlList[selectIndex]);
                }
                if (musiclist.ItemsSource == favourite_list)
                {
                    PlayerLyricThread.Start(favorurlList[selectIndex]);
                }

            }
        }

        //点击播放按钮开始播放歌曲
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (musiclist.SelectedIndex != -1)
            {
                if (wplayer.URL == urlList[musiclist.SelectedIndex])
                {
                    if (wplayer.playState == WMPLib.WMPPlayState.wmppsPaused)
                    {
                        wplayer.controls.currentPosition = currentTime;
                        wplayer.controls.play();
                    }
                }
                else
                {
                    wplayer.URL = urlList[musiclist.SelectedIndex];
                    wplayer.controls.play();
                }
            }
            else
            {
                if (wplayer.URL != null)
                    wplayer.controls.play();
            }

        }
        //上一首
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = musiclist.SelectedIndex - 1;
            musiclist.SelectedIndex = selectedIndex < 0 ? selectedIndex = 0 : selectedIndex;
            wplayer.URL = urlList[selectedIndex];
            wplayer.controls.play();
        }
        //下一首
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = musiclist.SelectedIndex + 1;
            musiclist.SelectedIndex = selectedIndex == musiclist.Items.Count ? selectedIndex = musiclist.Items.Count - 1 : selectedIndex;
            wplayer.URL = urlList[selectedIndex];
            wplayer.controls.play();
        }
        //暂停
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            currentTime = wplayer.controls.currentPosition;
            wplayer.controls.pause();
        }
       
        private void btnLike(object sender, RoutedEventArgs e)
        {
            if (musiclist.SelectedIndex != -1)
            {
                favourite_list.Add((musicInfo)items[musiclist.SelectedIndex].Clone());
                favorurlList.Add(items[musiclist.SelectedIndex].Url);
            }
                
        }

        private void btnShowLiked(object sender, RoutedEventArgs e)
        {
            musiclist.ItemsSource = favourite_list;
            musiclist.Items.Refresh();
        }

        private void btnShowItemsList(object sender, RoutedEventArgs e)
        {
            musiclist.ItemsSource = items;
            musiclist.Items.Refresh();
        }
        //删除歌曲列表中的歌
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            int i = musiclist.SelectedIndex;
            if (musiclist.ItemsSource == items)
            {
                items.Remove(items[i]);
                musiclist.ItemsSource = items;
                musiclist.Items.Refresh();
                urlList.Remove(urlList[i]);
            }
            if(musiclist.ItemsSource == favourite_list)
            {
                favourite_list.Remove(favourite_list[i]);
                musiclist.ItemsSource = favourite_list;
                musiclist.Items.Refresh();
                favorurlList.Remove(favorurlList[i]);
            }
        }
        //有问题（进度条拖动/歌曲没动）
        private void progressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p0 = e.GetPosition(musicProgress);
            musicProgress.Value = (p0.X / musicProgress.Width) * max;
            min = musicProgress.Value;
            e.Handled = true;
        }
        //有问题（进度条拖动/歌曲没动）
        private void progressBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            currentTime = musicProgress.Value;
            timer.Start();

        }

        private void LyricControl(object data)
        {
            string url = (string)data;
            int startIndex = url.LastIndexOf('\\') + 1;
            int length = url.Length - startIndex - 4;
            string filepath = url.Substring(startIndex, length);
            LyricFile lyric_file = new LyricFile();

            if (!File.Exists("./lyrics/" + filepath + ".lrc"))
            {
                Action action = () => lyric_window.Lyric.Content = "无歌词";
                var dispatcher = lyric_window.Lyric.Dispatcher;
                if (dispatcher.CheckAccess())
                    action();
                else
                    dispatcher.Invoke(action);
                return;
            }

            lyric_file.LoadFromFile("./lyrics/"+filepath+".lrc");

            Thread.Sleep(1000);
            Func<LyricFile> lf = () => lyric_file;
            for (lf().pos = 0; lf().pos < lf().lyrics.Count;)
            {
                string curposString = wplayer.controls.currentPositionString;

                int minute = int.Parse(curposString.Split(':')[0]);
                int second = int.Parse(curposString.Split(':')[1]);
                int cur = minute * 60 + second;

                if (cur >= lf().lyrics[lf().pos].minute * 60 + lf().lyrics[lf().pos].second)
                {
                    Action action = () => lyric_window.Lyric.Content = lf().lyrics[lf().pos].strLyric;
                    var dispatcher = lyric_window.Lyric.Dispatcher;
                    if (dispatcher.CheckAccess())
                        action();
                    else
                        dispatcher.Invoke(action);
                    lf().pos++;
                }
            }
        }
    }
}
