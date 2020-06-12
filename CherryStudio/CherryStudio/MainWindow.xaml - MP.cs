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

namespace CherryStudio
{
    public class musicInfo
    {
        public string Name { get; set; }
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
            //Encoding encoder = Encoding.GetEncoding("UTF8");
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

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<musicInfo> items = new List<musicInfo>();
        List<musicInfo> favourite_list = new List<musicInfo>();

        private MediaPlayer mediaPlayer = new MediaPlayer();
        List<string> urlList = new List<string>();
        double max, min;//控制processbar

        Thread PlayerLyricThread;
        LyricWindow lyric_window;

        private string current;

        public MainWindow()
        {
            InitializeComponent();
        }

        //上传音乐（待完善 显示不出来+不能多次上传+间接导致底栏的名字没有随着点击更换）
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
                    items.Add(new musicInfo { Name = name });
                    urlList.Add(url);
                }
                musiclist.ItemsSource = items;
                musiclist.Items.Refresh();

            }

        }
        //在播放列表中选择歌曲播放
        /*private void musiclist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectIndex = musiclist.SelectedIndex;
            if (selectIndex != -1)
            {
                mediaPlayer.Open(new Uri(urlList[selectIndex]));
                mediaPlayer.Play();
            }
        }*/

        private void musiclist_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            int selectIndex = musiclist.SelectedIndex;
            if (selectIndex != -1)
            {
                mediaPlayer.Open(new Uri(urlList[selectIndex]));
                current = urlList[selectIndex];
                mediaPlayer.Play();
                if (lyric_window == null)
                    lyric_window = new LyricWindow();
                lyric_window.Owner = this;
                lyric_window.Show();

                PlayerLyricThread = new Thread(new ParameterizedThreadStart(LyricControl));
                PlayerLyricThread.Start(urlList[selectIndex]);
            }
        }

        //点击播放按钮开始播放歌曲
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            /*if (musiclist.SelectedIndex == -1)
            {
                int selectedIndex = musiclist.SelectedIndex == -1 ? 0 : musiclist.SelectedIndex;
                musiclist.SelectedItem = musiclist.Items[selectedIndex];
                mediaPlayer.Open(new Uri(urlList[selectedIndex]));
                mediaPlayer.Play();
            }
            else
            {
                mediaPlayer.Play();
            }*/
            if (musiclist.SelectedIndex != -1)
            {
                if (current != urlList[musiclist.SelectedIndex])
                    mediaPlayer.Open(new Uri(urlList[musiclist.SelectedIndex]));
                mediaPlayer.Play();
            }
            else
            {
                if (mediaPlayer.Source != null)
                    mediaPlayer.Play();
            }

        }
        //上一首
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = musiclist.SelectedIndex - 1;
            musiclist.SelectedIndex = selectedIndex < 0 ? selectedIndex = 0 : selectedIndex;
            mediaPlayer.Open(new Uri(urlList[selectedIndex]));
            mediaPlayer.Play();
        }
        //下一首
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = musiclist.SelectedIndex + 1;
            musiclist.SelectedIndex = selectedIndex == musiclist.Items.Count ? selectedIndex = musiclist.Items.Count - 1 : selectedIndex;
            mediaPlayer.Open(new Uri(urlList[selectedIndex]));
            mediaPlayer.Play();
        }
        //暂停
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }
        //待实现
        private void processBar(object sender, RoutedEventArgs e)
        {
            max = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            min = mediaPlayer.Position.TotalSeconds;
            musicProcess.Maximum = (int)(max);
            musicProcess.Value = (int)(min);
        }

        private void btnLike(object sender, RoutedEventArgs e)
        {
            if (musiclist.SelectedIndex != -1)
                favourite_list.Add((musicInfo)items[musiclist.SelectedIndex].Clone());
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

        private void LyricControl(object data)
        {
            string url = (string)data;
            int startIndex = url.LastIndexOf('\\') + 1;
            int length = url.Length - startIndex - 4;
            string filepath = url.Substring(startIndex, length);
            LyricFile lyric_file = new LyricFile();
            lyric_file.LoadFromFile("./lyrics/"+filepath+".lrc");

            Func<LyricFile> lf = () => lyric_file;
            for (lf().pos = 0; lf().pos < lf().lyrics.Count;)
            {
                Thread.Sleep(1000);
                string curposString = mediaPlayer.Position.ToString();
                MessageBox.Show(curposString);
                //lyric_window.Lyric.Content = curposString;
                /*int minute = int.Parse(curposString.Split(':')[1]);
                int second = int.Parse(curposString.Split(':')[2]);
                int cur = minute * 60 + second;

                if (cur >= lf().lyrics[lf().pos].minute * 60 + lf().lyrics[lf().pos].second)
                {
                    label2.Text = lf().lyrics[lf().pos].strLyric;
                    //Utf8ToGB2312(lf.lyrics[i].strLyric);
                    lf().pos++;
                }*/
            }
        }
    }
}
