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
using System.IO;

namespace Wiedzowkonator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public Image[] screenshots;
        public bool screenshotQuizStarted;
        public string path = "E:/screeny/";
        // BitmapImage bitmapImage = new BitmapImage(new Uri("E:/screeny/1.jpg", UriKind.Absolute));
        // BitmapImage bitmapImage2 = new BitmapImage(new Uri("E:/screeny/2.jpg", UriKind.Absolute));
        BitmapImage[] bitmapImage;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        public void Start()
        {
            canvasBorder.BorderThickness = new Thickness(2.5f);
            bitmapImage = new BitmapImage[Directory.GetFiles(path).Length]; //Initializing BitmapImage with amount of images as its size
            screenshots = new Image[bitmapImage.Length]; //Array length is same as BitmapImage

            string[] fileNames = Directory.GetFiles(path);
            for (int i = 0; i < fileNames.Length; i++)
            {
                //MessageBox.Show(fileNames[i]);
                bitmapImage[i] = new BitmapImage(new Uri(fileNames[i], UriKind.Absolute));
                screenshots[i] = new Image();
                screenshots[i].Source = bitmapImage[i];
                screenshots[i].Width = bitmapImage[i].Width;
                screenshots[i].Height = bitmapImage[i].Height;
            }

            /*
            screenshots[0] = new Image();
            screenshots[0].Source = bitmapImage[0];
            screenshots[0].Width = bitmapImage[0].Width;
            screenshots[0].Height = bitmapImage[0].Height;
            canvas1.Children.Add(screenshots[0]);

            screenshots[1] = new Image();
            screenshots[1].Source = bitmapImage[1];
            screenshots[1].Width = bitmapImage[1].Width;
            screenshots[1].Height = bitmapImage[1].Height;
            canvas1.Children.Add(screenshots[1]);
            */
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            bool nonBuggedEntry = true; //Checking if there was any error when passing question number

            if (screenshotQuizStarted == false)
                screenshotQuizStarted = true;
            else
                screenshotQuizStarted = false;

            if (screenshotQuizStarted)
            {

                if (!string.IsNullOrWhiteSpace(questionNumberBox.Text) && Math.Sign(int.Parse(questionNumberBox.Text)) == 1)
                {
                    canvasScreenshotQuiz.Children.Add(screenshots[int.Parse(questionNumberBox.Text)]);
                    questionNumberBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Wybierz liczby od 1 do " + screenshots.Length);
                    questionNumberBox.Text = "";
                    if (screenshotQuizStarted == false)
                        screenshotQuizStarted = true;
                    else
                        screenshotQuizStarted = false;

                    nonBuggedEntry = false; //There was problem so entry is failed - current window won't change
                }
            }
            else
            {
                canvasScreenshotQuiz.Children.Clear();
            }
            //StartButton.Width = 0;

            if (nonBuggedEntry) //If no error occured
            {
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