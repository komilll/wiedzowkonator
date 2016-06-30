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
using System.Windows.Shapes;

namespace Wiedzowkonator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public Image[] screenshots = new Image[2];
        public bool screenshotQuizStarted;
        BitmapImage bitmapImage = new BitmapImage(new Uri("E:/screeny/1.jpg", UriKind.Absolute));
        BitmapImage bitmapImage2 = new BitmapImage(new Uri("E:/screeny/2.jpg", UriKind.Absolute));

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        public void Start()
        {
            screenshots[0] = new Image();
            screenshots[0].Source = bitmapImage;
            screenshots[0].Width = bitmapImage.Width;
            screenshots[0].Height = bitmapImage.Height;
            canvas1.Children.Add(screenshots[0]);

            screenshots[1] = new Image();
            screenshots[1].Source = bitmapImage2;
            screenshots[1].Width = bitmapImage2.Width;
            screenshots[1].Height = bitmapImage2.Height;
            canvas1.Children.Add(screenshots[1]);
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            screenshotQuizStarted = true;
            image = screenshots[1];

            canvas1.Children.Remove(screenshots[1]);
            //StartButton.Width = 0;

            if (plusFirstParticipant.Width == 0)
            {
                plusFirstParticipant.Width = 33;
                minutFirstParticipant.Width = 33;
                nameFisrtParticipant.Width = 120;
                pointsFirstParticipant.Width = 33;
            }
            else
            {
                plusFirstParticipant.Width = 0;
                minutFirstParticipant.Width = 0;
                nameFisrtParticipant.Width = 0;
                pointsFirstParticipant.Width = 0;
            }
        }

        private void plusFirstParticipant_Click(object sender, RoutedEventArgs e)
        {
            float curPoints = float.Parse(pointsFirstParticipant.Text);
            curPoints++;
            if (curPoints == (int)curPoints)
                pointsFirstParticipant.Text = curPoints.ToString() + ",0";
            else
                pointsFirstParticipant.Text = curPoints.ToString();
        }

        private void minutFirstParticipant_Click(object sender, RoutedEventArgs e)
        {
            float curPoints = float.Parse(pointsFirstParticipant.Text);
            curPoints--;
            if (curPoints == (int)curPoints)
                pointsFirstParticipant.Text = curPoints.ToString() + ",0";
            else
                pointsFirstParticipant.Text = curPoints.ToString();
        }
    }
}
