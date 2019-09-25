using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MyCShapeWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectedPath;
        public ArrayList IpList = new ArrayList();
        public int CurrentPos = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            ShowNextImages();
        }

        public void LoadData()
        {
            string userPath = null;
            string path = @"C:\Users\Klavor\AppData\Local\Packages\";
            DirectoryInfo root = new DirectoryInfo(path);
            DirectoryInfo[] dics = root.GetDirectories();
            foreach (DirectoryInfo di in dics)
            {
                if (di.Name.StartsWith("Microsoft.Windows.ContentDeliveryManager_"))
                {
                    userPath = di.FullName + "\\LocalState\\TargetedContentCache";
                    break;
                }
            }
            DirectoryInfo userDi = new DirectoryInfo(userPath);
            DirectoryInfo[] userDis = userDi.GetDirectories();
            foreach (DirectoryInfo di in userDis)
            {
                DirectoryInfo[] ds = di.GetDirectories();
                foreach (DirectoryInfo d in ds)
                {
                    FileInfo[] files = d.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        string text = System.IO.File.ReadAllText(@file.FullName);
                        JavaScriptSerializer js = new JavaScriptSerializer();//实例化一个能够序列化数据的类
                        Info list = js.Deserialize<Info>(text); //将json数据转化为对象类型并赋值给list
                        if (list.Name == "LockScreen")
                        {
                            InfoProperties infoProperties = list.Properties;
                            IpList.Add(infoProperties);
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(InfoProperties ip in IpList)
            {
                LandscapeImage landscapeImage = ip.LandscapeImage;
                PortraitImage portraitImage = ip.PortraitImage;
                string li = landscapeImage.Image;
                string pi = portraitImage.Image;
                //线程异步调用复制文件
                Thread cp1 = new Thread(new ParameterizedThreadStart(CopyFile));
                int pos1 = li.LastIndexOf("\\");
                string name1 = li.Substring(pos1 + 1);
                object o1 = new CopyFileInfo()
                {
                    From = li,
                    To = SelectedPath + "\\"+ name1 + ".jpg"
                };
                cp1.Start(o1);

                Thread cp2 = new Thread(new ParameterizedThreadStart(CopyFile));
                int pos2 = pi.LastIndexOf("\\");
                string name2 = pi.Substring(pos2 + 1);
                object o2 = new CopyFileInfo()
                {
                    From = pi,
                    To = SelectedPath + "\\"+ name2+".jpg"
                };
                cp2.Start(o2);
            }
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择Txt所在文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                SelectedPath = dialog.SelectedPath;
                label1.Content = SelectedPath;
            }
        }

        public void CopyFile(object obj)
        {
            CopyFileInfo c = obj as CopyFileInfo;

            byte[] fromb = File.ReadAllBytes(@c.From);
            File.WriteAllBytes(c.To, fromb);
        }

        private void Pbtn_Click(object sender, RoutedEventArgs e)
        {
            ShowPreviousImages();
        }

        public void ShowPreviousImages()
        {
            if (CurrentPos >= IpList.Count  || CurrentPos < 0)
            {
                return;
            }
            InfoProperties ip = IpList.ToArray()[CurrentPos] as InfoProperties;
            BitmapImage bi1 = new BitmapImage(new Uri(ip.LandscapeImage.Image));
            image1.Source = bi1;
            BitmapImage bi2 = new BitmapImage(new Uri(ip.PortraitImage.Image));
            image2.Source = bi2;
            CurrentPos--;
        }

        private void Nbtn_Click(object sender, RoutedEventArgs e)
        {
            ShowNextImages();
        }
        public void ShowNextImages()
        {
            if (CurrentPos >= IpList.Count || CurrentPos < 0)
            {
                return;
            }
            InfoProperties ip = IpList.ToArray()[CurrentPos] as InfoProperties;
            BitmapImage bi1 = new BitmapImage(new Uri(ip.LandscapeImage.Image));
            image1.Source = bi1;
            BitmapImage bi2 = new BitmapImage(new Uri(ip.PortraitImage.Image));
            image2.Source = bi2;
            CurrentPos++;
        }
    }

    public class Info
    {
        public string Name;
        public InfoProperties Properties;
    }

    public class InfoProperties
    {
        public LandscapeImage LandscapeImage;
        public PortraitImage PortraitImage;
    }

    public class LandscapeImage
    {
        public string Image;
    }

    public class PortraitImage
    {
        public string Image;
    }

    public class CopyFileInfo
    {
        public string From;
        public string To;
    }
}
