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
    //歌曲信息
    public class musicInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    //歌词
    class Lyric
    {
        public int minute;
        public float second;
        public float totalSec;
        public string strLyric;
    }
    //歌词文件
    class LyricFile
    {
        public int pos = 0;
        public List<Lyric> lyrics = new List<Lyric>();

        public void LoadFromFile(string fileName)
        {
            pos = 0;
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
        List<musicInfo> items = new List<musicInfo>();//上传列表
        List<musicInfo> favourite_list = new List<musicInfo>();//喜爱列表

        private WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        List<string> urlList = new List<string>();//上传列表URL
        List<string> favourList = new List<string>();//喜爱列表URL
        double max, min;//控制processbar
        DispatcherTimer timer = new DispatcherTimer();

        double currentTime;
        double totalSeconds = 0;

        LyricWindow lyric_window;



        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += timer_Tick;
            timer.Tick += LyricControl;
            //timer.Start();
        }


        //进度条
        void timer_Tick(object sender,EventArgs e)
        {
            if (wplayer.playState == WMPPlayState.wmppsPlaying)
            {
                totalSeconds += 0.5;
                int ts = (int)totalSeconds;
                int minute = (ts / 60) % 60;
                int second = ts % 60;
                string totalTime = "统计时长 "
                    + minute.ToString().PadLeft(2, '0') + "m"
                    + second.ToString().PadLeft(2, '0') + "s";
                Action action = () => TotalTime.Content = totalTime;
                var dispatcher = TotalTime.Dispatcher;
                if (dispatcher.CheckAccess())
                    action();
                else
                    dispatcher.Invoke(action);
            }

            max = wplayer.currentMedia.duration;
            min = wplayer.controls.currentPosition;

            int tot_min = (int)Math.Round(max) / 60;
            int tot_sec = (int)Math.Round(max) % 60;
            int cur_min = (int)Math.Round(min) / 60;
            int cur_sec = (int)Math.Round(min) % 60;
            string cur_tot = tot_min.ToString().PadLeft(2,'0') + ":"
                + tot_sec.ToString().PadLeft(2,'0') + "/" 
                + cur_min.ToString().PadLeft(2,'0') + ":" 
                + cur_sec.ToString().PadLeft(2,'0');
            Action _action = () => curVtot.Text = cur_tot;
            var _dispatcher = curVtot.Dispatcher;
            if (_dispatcher.CheckAccess())
                _action();
            else
                _dispatcher.Invoke(_action);

            musicProgress.Maximum = (int)(max);
            musicProgress.Value = (int)(min);
            if (musicProgress.Value == 0 && wplayer.playState == WMPPlayState.wmppsStopped)
                btnNext.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }


        //进度条拖动MouseDown
        private void progressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p0 = e.GetPosition(musicProgress);
            musicProgress.Value = (p0.X / musicProgress.Width) * max;
            min = musicProgress.Value;
            e.Handled = true;
        }


        //进度条拖动MouseUp
        private void progressBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            wplayer.controls.currentPosition = musicProgress.Value;
            timer.Start();
        }


        //音量调节
        private void musicVolume_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p1 = e.GetPosition(musicVolume);
            musicVolume.Value = p1.X;
            wplayer.settings.volume = (int)musicVolume.Value;
            e.Handled = true;
        }


        //关闭窗口
        protected override void OnClosing(CancelEventArgs e)
        {
            if (wplayer != null)
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


        //双击音乐列表项播放音乐
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
                    wplayer.URL = favourList[selectIndex];
                    musicName.Text = favourite_list[selectIndex].Name.ToString();
                }

                wplayer.controls.play();
                timer.Start();

                if (musiclist.ItemsSource == items)
                {
                    LoadLyric(urlList[selectIndex]);
                }
                if (musiclist.ItemsSource == favourite_list)
                {
                    LoadLyric(favourList[selectIndex]);
                }
            }
        }


        //点击播放按钮开始播放歌曲
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //如果已有选中项
            if (musiclist.SelectedIndex != -1)
            {
                if (musiclist.ItemsSource == items && wplayer.URL == urlList[musiclist.SelectedIndex])
                {
                    if (wplayer.playState == WMPLib.WMPPlayState.wmppsPaused)
                    {
                        wplayer.controls.currentPosition = currentTime;
                        wplayer.controls.play();
                    }
                }
				else if (musiclist.ItemsSource == favourList && wplayer.URL == favourList[musiclist.SelectedIndex])
				{
					if (wplayer.playState == WMPLib.WMPPlayState.wmppsPaused)
                    {
                        wplayer.controls.currentPosition = currentTime;
                        wplayer.controls.play();
                    }
				}
                else
                {
                    if (musiclist.ItemsSource == items)
                    {
                        wplayer.URL = urlList[musiclist.SelectedIndex];
                        musicName.Text = items[musiclist.SelectedIndex].Name.ToString();
                    }
                    if (musiclist.ItemsSource == favourite_list)
                    {
                        wplayer.URL = favourList[musiclist.SelectedIndex];
                        musicName.Text = favourite_list[musiclist.SelectedIndex].Name.ToString();
                    }

                    wplayer.controls.play();
                    timer.Start();

                    if (musiclist.ItemsSource == items)
                    {
                        LoadLyric(urlList[musiclist.SelectedIndex]);
                    }
                    if (musiclist.ItemsSource == favourite_list)
                    {
                        LoadLyric(favourList[musiclist.SelectedIndex]);
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(wplayer.URL))
                {
                    //选择第一项
                    musiclist.SelectedIndex ++;
                    if (musiclist.ItemsSource == items)
                    {
                        wplayer.URL = urlList[0];
                        LoadLyric(urlList[0]);
                        musicName.Text = items[0].Name.ToString();
                        wplayer.controls.play();
                    }
                    if (musiclist.ItemsSource == favourite_list)
                    {
                        wplayer.URL = favourList[0];
                        LoadLyric(favourList[0]);
                        musicName.Text = favourite_list[0].Name.ToString();
                        wplayer.controls.play();
                    }
                    timer.Start();
                }
                    
            }

        }


        //上一首
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = musiclist.SelectedIndex - 1;
            musiclist.SelectedIndex = selectedIndex < 0 ? selectedIndex = 0 : selectedIndex;

			if (musiclist.ItemsSource == items)
			{
                wplayer.URL = urlList[selectedIndex];
                musicName.Text = items[selectedIndex].Name.ToString();
                LoadLyric(urlList[musiclist.SelectedIndex]);
            }
			if (musiclist.ItemsSource == favourite_list)
			{
                wplayer.URL = favourList[selectedIndex];
                musicName.Text = favourite_list[selectedIndex].Name.ToString();
                LoadLyric(favourList[musiclist.SelectedIndex]);
            }
            wplayer.controls.play();
        }


        //下一首
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = musiclist.SelectedIndex + 1;
            musiclist.SelectedIndex = selectedIndex == musiclist.Items.Count ? selectedIndex = musiclist.Items.Count - 1 : selectedIndex;
            //wplayer.URL = urlList[selectedIndex];
			if (musiclist.ItemsSource == items)
			{
                wplayer.URL = urlList[selectedIndex];
                musicName.Text = items[selectedIndex].Name.ToString();
                LoadLyric(urlList[musiclist.SelectedIndex]);
            }
			if (musiclist.ItemsSource == favourite_list)
			{
                wplayer.URL = favourList[selectedIndex];
                musicName.Text = favourite_list[selectedIndex].Name.ToString();
                LoadLyric(favourList[musiclist.SelectedIndex]);
            }
            wplayer.controls.play();
        }


        //暂停
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            currentTime = wplayer.controls.currentPosition;
            wplayer.controls.pause();
        }


        //收藏歌曲至“我喜爱的音乐”列表
        private void btnLike(object sender, RoutedEventArgs e)
        {
            string url="";
            if(musiclist.ItemsSource == items)
            {
                url = items[musiclist.SelectedIndex].Url;
            }
            if (musiclist.ItemsSource == favourite_list)
            {
                url = favourite_list[musiclist.SelectedIndex].Url;
            }

            int startIndex = url.LastIndexOf('\\') + 1;
            int length = url.Length - startIndex - 4;
            string name = url.Substring(startIndex, length);
            favourite_list.Add(new musicInfo { Name = name, Url = url });
            favourList.Add(url);
        }


        //显示“我喜爱的音乐”列表
        private void btnShowLiked(object sender, RoutedEventArgs e)
        {
            musiclist.ItemsSource = favourite_list;
            musiclist.Items.Refresh();
        }


        //显示“我的音乐”列表
        private void btnShowItemsList(object sender, RoutedEventArgs e)
        {
            musiclist.ItemsSource = items;
            musiclist.Items.Refresh();
        }


        //显示歌词
        private void btnLyricShow(object sender, RoutedEventArgs e)
        {
            if (lyric_window != null && lyric_window.Visibility == Visibility.Hidden)
            {
                lyric_window.Visibility = Visibility.Visible;
            }
        }


        //倍速
        private void btnSpeed(object sender, RoutedEventArgs e)
        {
            double position = wplayer.controls.currentPosition;
            wplayer.controls.pause();
            wplayer.settings.rate = wplayer.settings.rate == 1 ? 1.5 : 1;
            wplayer.controls.currentPosition = position;
            wplayer.controls.play();
        }

 
        //删除歌曲
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
                favourList.Remove(favourList[i]);
            }
            btnNext.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }


        //歌词加载
        private LyricFile lyric_file;
        private void LoadLyric(string url)
        {
            if (lyric_window == null)
            {
                lyric_window = new LyricWindow();
                lyric_window.Owner = this;
                lyric_window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                lyric_window.Show();
            }

            int startIndex = url.LastIndexOf('\\') + 1;
            int length = url.Length - startIndex - 4;
            string filepath = url.Substring(startIndex, length);

            lyric_file = new LyricFile();
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

            lyric_file.LoadFromFile("./lyrics/" + filepath + ".lrc");
        }


        //歌手分类
        private void selectSinger_Click(object sender, RoutedEventArgs e)
        {
            musiclist.Items.Refresh();
        }


        //专辑分类
        private void selectAlbum_Click(object sender, RoutedEventArgs e)
        {
            musiclist.Items.Refresh();
        }


        //歌词控制
        void LyricControl(object sender, EventArgs e)
        {
            Func<LyricFile> lf = () => lyric_file;
            if (wplayer.playState != WMPPlayState.wmppsPlaying)
                return;
            string curposString = wplayer.controls.currentPositionString;

            int minute = int.Parse(curposString.Split(':')[0]);
            int second = int.Parse(curposString.Split(':')[1]);
            int cur = minute * 60 + second;

            for (; lf().pos < lf().lyrics.Count;)
            {
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
                else
                    break;
            }
        }
    }
}
