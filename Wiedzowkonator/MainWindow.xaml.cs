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
using System.Text.RegularExpressions;

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
        BitmapImage[] bitmapImage;
        int lastScreenshotIndex;
        int screenshotsCompleted;
        enum quizState { choosingQuestion, answeringQuestion, givingPoints };
        quizState curQuizState;



        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        public void Start()
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            canvasBorder.BorderThickness = new Thickness(2.5f);
            bitmapImage = new BitmapImage[Directory.GetFiles(path).Length]; //Initializing BitmapImage with amount of images as its size
            screenshots = new Image[bitmapImage.Length]; //Array length is same as BitmapImage

            plusFirstParticipant.Width = 0;
            minutFirstParticipant.Width = 0;
            nameFisrtParticipant.Width = 0;
            pointsFirstParticipant.Width = 0;
            confirmPoints.Width = 0;

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
        }

        private void StartClick(object sender, RoutedEventArgs e) //Main method that contains showing screenshots
        {
            bool nonBuggedEntry = true; //Checking if there was any error when passing question number

            if (screenshotQuizStarted == false)
                screenshotQuizStarted = true;
            else
                screenshotQuizStarted = false;

            if (screenshotQuizStarted)
            {
                //Variable needed to check if input text is number; It's only used to "if" statement
                int tryingParse; 
                bool isNumeric = int.TryParse(questionNumberBox.Text, out tryingParse);
                if (isNumeric)
                    isNumeric = Math.Sign(int.Parse(questionNumberBox.Text)) == 1;

                if (!string.IsNullOrWhiteSpace(questionNumberBox.Text) && isNumeric && int.Parse(questionNumberBox.Text) <= screenshots.Length - screenshotsCompleted)
                {
                    //Substracting 1 to let user writting down from "1 to x" instead of "0 to x"
                    int currentScreenshot = lastScreenshotIndex = int.Parse(questionNumberBox.Text) - 1;
                    canvasScreenshotQuiz.Width = screenshots[currentScreenshot].Width;
                    canvasScreenshotQuiz.Height = screenshots[currentScreenshot].Height;

                    canvasScreenshotQuiz.Children.Add(screenshots[currentScreenshot]);
                    questionNumberBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Wybierz liczby od 1 do " + (screenshots.Length - screenshotsCompleted));
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
                ShowingHUD();
            }
        }

        void ShowingHUD()
        {
            if (plusFirstParticipant.Width == 0 && curQuizState == quizState.givingPoints)
            {
                plusFirstParticipant.Width = 33;
                minutFirstParticipant.Width = 33;
                nameFisrtParticipant.Width = 120;
                pointsFirstParticipant.Width = 33;
                confirmPoints.Width = 75;
            }
            else
            {
                plusFirstParticipant.Width = 0;
                minutFirstParticipant.Width = 0;
                nameFisrtParticipant.Width = 0;
                pointsFirstParticipant.Width = 0;
                confirmPoints.Width = 0;
            }

            if (questionNumberBox.Width == 0 && curQuizState == quizState.choosingQuestion)
            {
                questionNumberBox.Width = 132;
                StartButton.Width = 150;
            }
            else
            {
                questionNumberBox.Width = 0;
                StartButton.Width = 0;
            }
        }

        /********************** Button/mouse pressed scripts ********************/
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

        private void confirmPoints_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.choosingQuestion;
            ShowingHUD();
            screenshots[lastScreenshotIndex] = null;
            screenshotsCompleted++;
            for (int i = lastScreenshotIndex; i < screenshots.Length - 1; i++)
            {
                screenshots[i] = screenshots[i + 1];
            }

            if (screenshots.Length == screenshotsCompleted)
            {
                screenshots[lastScreenshotIndex] = null;
                MessageBox.Show("KONIEC WIEDZÓWKI!");
            }
        }

        private void questionNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Starting quiz through pressing "Enter" key instead of clicking "Start Button"
            if (e.Key == Key.Enter)
            {
                curQuizState = quizState.answeringQuestion;
                StartClick(null, null);
            }
        }

        private void SkippingScreenshot(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                curQuizState = quizState.givingPoints;
                StartClick(null, null);
            }
        }

        /********************* Managing menu tabs ************************/
        private void Open(object sender, RoutedEventArgs e)
        {

        }

        private void Save(object sender, RoutedEventArgs e)
        {

        }

        private void Load(object sender, RoutedEventArgs e)
        {

        }
    }
}