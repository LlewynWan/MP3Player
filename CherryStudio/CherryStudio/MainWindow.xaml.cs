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

namespace CherryStudio
{
    public class musicInfo
    {
        public string Name { get; set; }

    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<musicInfo> items = new List<musicInfo>();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        List<string> urlList = new List<string>();
        double max, min;//控制processbar

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
                    items.Add(new musicInfo() {});
                    urlList.Add(url);
                }
                musiclist.ItemsSource = items;
              

            }

        }
        //在播放列表中选择歌曲播放
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
            {
                int selectedIndex = musiclist.SelectedIndex == -1 ? 0 : musiclist.SelectedIndex;
                musiclist.SelectedItem = musiclist.Items[selectedIndex];
                mediaPlayer.Open(new Uri(urlList[selectedIndex]));
                mediaPlayer.Play();
            }
            else
            {
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
    }
}
