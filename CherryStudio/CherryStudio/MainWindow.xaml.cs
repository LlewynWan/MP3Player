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

<<<<<<< HEAD
=======

>>>>>>> 加入最爱列表/优化界面及播放事件
namespace CherryStudio
{
    public class musicInfo
    {
        public string Name { get; set; }
<<<<<<< HEAD

=======
        public object Clone()
        {
            return this.MemberwiseClone();
        }
>>>>>>> 加入最爱列表/优化界面及播放事件
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<musicInfo> items = new List<musicInfo>();
<<<<<<< HEAD
=======
        List<musicInfo> favourite_list = new List<musicInfo>();

>>>>>>> 加入最爱列表/优化界面及播放事件
        private MediaPlayer mediaPlayer = new MediaPlayer();
        List<string> urlList = new List<string>();
        double max, min;//控制processbar

<<<<<<< HEAD
        public MainWindow()
        {
            InitializeComponent();
            
=======
        private string current;

        public MainWindow()
        {
            InitializeComponent();

>>>>>>> 加入最爱列表/优化界面及播放事件
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
<<<<<<< HEAD
                foreach (string url in nameList)
                {
                    items.Add(new musicInfo() {});
                    urlList.Add(url);
                }
                musiclist.ItemsSource = items;
              
=======
                
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
>>>>>>> 加入最爱列表/优化界面及播放事件

            }

        }
        //在播放列表中选择歌曲播放
<<<<<<< HEAD
        private void musiclist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectIndex = musiclist.SelectedIndex;
            mediaPlayer.Open(new Uri(urlList[selectIndex]));
            mediaPlayer.Play();
        }
        //点击播放按钮开始播放歌曲
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (musiclist.SelectedIndex == -1)
=======
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
            }
        }

        //点击播放按钮开始播放歌曲
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            /*if (musiclist.SelectedIndex == -1)
>>>>>>> 加入最爱列表/优化界面及播放事件
            {
                int selectedIndex = musiclist.SelectedIndex == -1 ? 0 : musiclist.SelectedIndex;
                musiclist.SelectedItem = musiclist.Items[selectedIndex];
                mediaPlayer.Open(new Uri(urlList[selectedIndex]));
                mediaPlayer.Play();
            }
            else
            {
                mediaPlayer.Play();
<<<<<<< HEAD
            }
            
=======
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
>>>>>>> 加入最爱列表/优化界面及播放事件

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
<<<<<<< HEAD
=======

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
>>>>>>> 加入最爱列表/优化界面及播放事件
    }
}
