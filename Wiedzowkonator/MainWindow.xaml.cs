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
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
//
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Image = System.Windows.Controls.Image;

namespace Wiedzowkonator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [Serializable]
    public class SerializationData
    {
        //public Image[] screenshots_;
        //public BitmapImage[] bitmapImage_;
        public List<string> answeredScreenshots = new List<string>(); //List of which screenshots were already shown
        public string[] points = new string[1]; //Number of participant points
        public string[] participants = new string[1]; //Name of participants
        public string _screenshotsLocalizationPath; //Path to quiz files (screenshot, music, text questions etc.)
    }

    public partial class MainWindow : Window
    {
        //TODO - add going back to last screenshot button
        SerializationData serializationData = new SerializationData(); //Variable class
        public Image[] screenshots; //Screenshots that will be shown on canvas
        BitmapImage[] bitmapImage; //Images that will be written into "screenshots" variable
        public string[] nameOfScreenshots; //Name of all screenshots - it is used to check which one was already shown
        public bool screenshotQuizStarted; //Checking if quiz has started - switching between choosing and answering phase
        int lastScreenshotIndex; //Getting index of last shown screenshot in case if user want to see it once again
        int screenshotsCompleted; //If user already answered this screenshot it won't be shown again
        static string userName = Environment.UserName; //Name of user logged on Windows account
        string quickSavePath = "C:/Users/" + userName + "/AppData/LocalLow/Wiedzowkonator/"; //Choosing directory path
        string screenshotsLocalizationPath; //Localizition of directory which contains screenshots

        //Enum type that shows which state of quiz is currently in progress
        enum quizState { choosingQuestion, answeringQuestion, givingPoints };
        quizState curQuizState;

        enum answerType { noAnswer, fileNameAnswer, customAnswer };
        answerType curAnswerType;

        /* END OF VARIABLES */
        public MainWindow()
        {
            InitializeComponent(); //Opening window
            Start(); //Initializing window's properties like width, sizes etc.
        }

        public void Start()
        {
            if (!Directory.Exists(quickSavePath)) //If directory for quicksave doesn't exists, one is made
            {
                Directory.CreateDirectory(quickSavePath);
            }

            Application.Current.MainWindow.WindowState = WindowState.Maximized; //Starting fullscreen
            canvasBorder.BorderThickness = new Thickness(2.5f);

            //Setting all unused currently controls off (not really switching off, rather making them "disappear" for a moment)
            plusFirstParticipant.Width = 0;
            minutFirstParticipant.Width = 0;
            nameFirstParticipant.Width = 0;
            pointsFirstParticipant.Width = 0;
            confirmPoints.Width = 0;
            StartButton.Width = 0;
            questionNumberBox.Width = 0;
            goBack.Width = 0;
            correctAnswer.Width = 0;
        }

        private void StartClick(object sender, RoutedEventArgs e) //Main method that contains showing screenshots
        {
            bool nonBuggedEntry = true; //Checking if there was any error when passing question number

            if (screenshotQuizStarted == false)
                screenshotQuizStarted = true;
            else
                screenshotQuizStarted = false;

            if (screenshotQuizStarted) //"True" entry chooses and shows image on canvas
            {
                //Variable needed to check if input text is number; It's only used to "if" statement
                int tryingParse; 
                bool isNumeric = int.TryParse(questionNumberBox.Text, out tryingParse);
                if (isNumeric)
                    isNumeric = Math.Sign(int.Parse(questionNumberBox.Text)) == 1;
                //Correctly written number
                if (!string.IsNullOrWhiteSpace(questionNumberBox.Text) && isNumeric && int.Parse(questionNumberBox.Text) <= screenshots.Length - screenshotsCompleted)
                {
                    //Substracting 1 to let user writting down from "1 to x" instead of "0 to x"
                    int currentScreenshot = lastScreenshotIndex = int.Parse(questionNumberBox.Text) - 1;
                    canvasScreenshotQuiz.Width = screenshots[currentScreenshot].Width;
                    canvasScreenshotQuiz.Height = screenshots[currentScreenshot].Height;

                    canvasScreenshotQuiz.Children.Add(screenshots[currentScreenshot]);
                    questionNumberBox.Text = "";
                }
                else //Uncorrectly written number
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
                canvasScreenshotQuiz.Children.Clear(); //Clearing canvas after user's mouse/enter press
            }

            if (nonBuggedEntry) //If no error occured
            {
                ShowingHUD();
            }
        }

        void ShowingHUD()
        {
            //In giving points phase, all controls that allows to give participants points are being shown
            if (plusFirstParticipant.Width == 0 && curQuizState == quizState.givingPoints)
            {
                plusFirstParticipant.Width = 33;
                minutFirstParticipant.Width = 33;
                nameFirstParticipant.Width = 120;
                pointsFirstParticipant.Width = 33;
                confirmPoints.Width = 75;
                goBack.Width = 75;
                correctAnswer.Width = 200;
            }
            else //Leaving this phase
            {
                plusFirstParticipant.Width = 0;
                minutFirstParticipant.Width = 0;
                nameFirstParticipant.Width = 0;
                pointsFirstParticipant.Width = 0;
                confirmPoints.Width = 0;
                goBack.Width = 0;
                correctAnswer.Width = 0;
            }
            //If user is choosing new question, only "start button" and box to write down number are enabled
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
        private void plusFirstParticipant_Click(object sender, RoutedEventArgs e) //Increasing 1st participant points
        {
            float curPoints = float.Parse(pointsFirstParticipant.Text);
            curPoints++;
            if (curPoints == (int)curPoints)
                pointsFirstParticipant.Text = curPoints.ToString() + ",0";
            else
                pointsFirstParticipant.Text = curPoints.ToString();
        }

        private void minutFirstParticipant_Click(object sender, RoutedEventArgs e) //Decreasing 1st participant points
        {
            float curPoints = float.Parse(pointsFirstParticipant.Text);
            curPoints--;
            if (curPoints == (int)curPoints)
                pointsFirstParticipant.Text = curPoints.ToString() + ",0";
            else
                pointsFirstParticipant.Text = curPoints.ToString();
        }
        //If user is done, then he leaves giving points phase and go back to choosing question
        private void confirmPoints_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.choosingQuestion;
            ShowingHUD();
            screenshots[lastScreenshotIndex] = null;
            serializationData.answeredScreenshots.Add(nameOfScreenshots[lastScreenshotIndex]); //Adding name of completed question

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
            QuickSave(); //If everything went good, current state is being save and if power is off or program crashes, user can load his last save
        }

        private void ReloadLastQuiz_Yes(object sender, RoutedEventArgs e) //Question on start
        {
            QuickLoad();
            ReloadButton_No.Width = 0;
            ReloadButton_Yes.Width = 0;
            ReloadTextBlock.Width = 0;
            StartButton.Width = 150;
            questionNumberBox.Width = 132;
        }

        private void ReloadLastQuiz_No(object sender, RoutedEventArgs e) //Question on start
        {
            ReloadButton_No.Width = 0;
            ReloadButton_Yes.Width = 0;
            ReloadTextBlock.Width = 0;
            StartButton.Width = 150;
            questionNumberBox.Width = 132;
        }

        private void goBack_Click(object sender, RoutedEventArgs e) //Showing again screenshot before giving points
        {
            curQuizState = quizState.answeringQuestion;
            ShowingHUD();
            if (screenshotQuizStarted == false)
                screenshotQuizStarted = true;
            else
                screenshotQuizStarted = false;

            canvasScreenshotQuiz.Children.Add(screenshots[lastScreenshotIndex]);
        }

        private void questionNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Starting quiz through pressing "Enter" key instead of clicking "Start Button"
            if (e.Key == Key.Enter && curQuizState == quizState.choosingQuestion)
            {
                curQuizState = quizState.answeringQuestion;
                StartClick(null, null);
            }
        }

        private void SkippingScreenshot(object sender, MouseButtonEventArgs e)
        {
            //Leaving answering question phase and skipping screenshot by >>pressing left mouse button<<
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                curQuizState = quizState.givingPoints;
                StartClick(null, null);

                if (curAnswerType == answerType.noAnswer)
                {
                    correctAnswer.Text = "";
                }
                else if (curAnswerType == answerType.fileNameAnswer)
                {
                    FileInfo file = new FileInfo(bitmapImage[lastScreenshotIndex].UriSource.LocalPath);
                    correctAnswer.Text = "Poprawna odpowiedź: " + file.Name.Replace(file.Extension, "");
                }
                else if (curAnswerType == answerType.customAnswer)
                {
                    //TODO - custom answer system
                }
            }
        }

        /********************* Managing menu tabs ************************/
        private void Open(object sender, RoutedEventArgs e) //Importing new screenshots
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] path = fileDialog.FileNames;
                //Initialize new array sizes depending of screenshot numbers
                bitmapImage = new BitmapImage[path.Length];
                screenshots = new Image[bitmapImage.Length];
                nameOfScreenshots = new string[bitmapImage.Length];
                screenshotsLocalizationPath = Directory.GetParent(path[0]).ToString(); //Getting directory where screenshots are stored
                int index = 0; //Index of each screenshot
                foreach (string screen in path)
                {
                    bitmapImage[index] = new BitmapImage(new Uri(screen, UriKind.Absolute));
                    screenshots[index] = new Image();
                    screenshots[index].Source = bitmapImage[index];
                    screenshots[index].Width = bitmapImage[index].Width;
                    screenshots[index].Height = bitmapImage[index].Height;
                    nameOfScreenshots[index] = screen;

                    index++;
                }
            }
        }
        //Saving current state to file in chosen location
        private void Save(object sender, RoutedEventArgs e)
        {           
            SaveFileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Create(path + ".anime");

                SerializationData data = new SerializationData();

                data.answeredScreenshots = serializationData.answeredScreenshots;
                data.points[0] = pointsFirstParticipant.Text;
                data.participants[0] = nameFirstParticipant.Text;
                data._screenshotsLocalizationPath = screenshotsLocalizationPath;

                bf.Serialize(stream, data);
                stream.Close();
            }
            /*
            SaveFileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;
                using (MemoryStream ms = new MemoryStream())
                {
                    BitmapImage __bitmapImage = new BitmapImage();
                    __bitmapImage = bitmapImage[0];
                    
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(__bitmapImage));
                    encoder.Save(ms);

                    using (FileStream file = new FileStream(path + ".bin", FileMode.Create, System.IO.FileAccess.Write))
                    {
                        byte[] bytes = new byte[ms.Length];
                        ms.Read(bytes, 0, (int)ms.Length);
                        file.Write(bytes, 0, bytes.Length);
                        ms.Close();
                    }
                }
            }*/
        }
        //Loading state of quiz from .anime file
        private void Load(object sender, RoutedEventArgs e)
        {           
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(path, FileMode.Open);

                SerializationData data = (SerializationData)bf.Deserialize(stream);
                serializationData.answeredScreenshots = data.answeredScreenshots;
                nameFirstParticipant.Text = data.participants[0];
                pointsFirstParticipant.Text = data.points[0];
                screenshotsLocalizationPath = data._screenshotsLocalizationPath;
                //Initializing arrays with imported data length and assigning new values
                /*
                bitmapImage = new BitmapImage[data.bitmapImage_.Length];
                screenshots = new Image[data.screenshots_.Length];
                screenshots = data.screenshots_;
                bitmapImage = data.bitmapImage_;*/

                stream.Close();

                for (int i = 0; i < serializationData.answeredScreenshots.Count; i++)
                {
                    for (int j = 0; j < nameOfScreenshots.Length; j++)
                    {
                        if (serializationData.answeredScreenshots[i] == nameOfScreenshots[j])
                        {
                            screenshotsCompleted++;
                            screenshots[i] = null;

                            for (int k = lastScreenshotIndex; k < screenshots.Length - 1; k++)
                            {
                                screenshots[k] = screenshots[k + 1];
                            }
                        }
                    }
                }
            }            
                /*
                string path = fileDialog.FileName;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);

                        //PngBitmapDecoder decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        /*
                        BitmapImage[] __bitmapImage = new BitmapImage[6];
                        BitmapFrame[] bitmapFrame = new BitmapFrame[6];

                        Stream imageStreamSource = new FileStream("E:/screeny/1.png", FileMode.Open, FileAccess.Read, FileShare.Read);
                        PngBitmapDecoder decoder = new PngBitmapDecoder(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        BitmapSource bitmapSource = decoder.Frames[0];

                        Image image = new Image();
                        image.Source = bitmapSource;
                        image.Width = bitmapSource.Width;
                        image.Height = bitmapSource.Height;

                        canvasScreenshotQuiz.Children.Add(image);   */
                        /*
                        BitmapImage bitmap = new BitmapImage();


                        Image image = new Image();
                        image.Source = bitmap;
                        image.Width = bitmap.Width;
                        image.Height = bitmap.Height;

                        canvasScreenshotQuiz.Children.Add(image);
                        if (canvasScreenshotQuiz.Children.Count > 0)
                            MessageBox.Show(image.Width.ToString() + "x" + image.Height.ToString() + "\n" + image.Source.ToString());
                    }
                }*/      
        }

        private void QuickSave() //Saving after all confirmed answer
        {
            string path = quickSavePath + "quickSave-" + DateTime.Now.ToString("dd/MM/yyyy HH_mm");

            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Create(path + ".anime");

            SerializationData data = new SerializationData();

            data.answeredScreenshots = serializationData.answeredScreenshots;
            data.points[0] = pointsFirstParticipant.Text;
            data.participants[0] = nameFirstParticipant.Text;
            data._screenshotsLocalizationPath = screenshotsLocalizationPath;

            bf.Serialize(stream, data);
            stream.Close();
        }

        private void QuickLoad() //Loading at the start of quiz
        {
            //Loading data containing deeper information and values of variables
            DirectoryInfo directory = new DirectoryInfo(quickSavePath);
            string path = (from f in directory.GetFiles()
                           orderby f.LastWriteTime descending
                           select f).First().FullName;

            BinaryFormatter bf = new BinaryFormatter();
            //MessageBox.Show(path);
            FileStream stream = File.Open(path, FileMode.Open);

            SerializationData data = (SerializationData)bf.Deserialize(stream);
            serializationData.answeredScreenshots = data.answeredScreenshots;
            nameFirstParticipant.Text = data.participants[0];
            pointsFirstParticipant.Text = data.points[0];
            screenshotsLocalizationPath = data._screenshotsLocalizationPath;

            stream.Close();
            //Loading screenshots from directory where they should be
            DirectoryInfo directoryWithScreenshots = new DirectoryInfo(screenshotsLocalizationPath);
            //MessageBox.Show(directoryWithScreenshots.ToString());
            string[] files = new string[directoryWithScreenshots.GetFiles().Length];
            int index = 0;
            int numberOfSkippedFiles = 0;
            string dataFile;

            foreach (FileInfo file in directoryWithScreenshots.GetFiles())
            {
                files[index] = file.FullName;
                if (file.Extension == ".anime")
                {
                    numberOfSkippedFiles++; //It'll be used in "for" loop to set number of loops
                    //Not increasing index
                    dataFile = file.FullName;
                }
                else
                {
                    index++;
                }
            }

            bitmapImage = new BitmapImage[directoryWithScreenshots.GetFiles().Length - numberOfSkippedFiles];
            screenshots = new Image[bitmapImage.Length];
            nameOfScreenshots = new string[bitmapImage.Length];
            for (int k = 0; k < directoryWithScreenshots.GetFiles().Length - numberOfSkippedFiles; k++)
            {
                bitmapImage[k] = new BitmapImage(new Uri(files[k], UriKind.Absolute));
                screenshots[k] = new Image();
                screenshots[k].Source = bitmapImage[k];
                screenshots[k].Width = bitmapImage[k].Width;
                screenshots[k].Height = bitmapImage[k].Height;
                nameOfScreenshots[k] = files[k];
            }

            for (int i = 0; i < serializationData.answeredScreenshots.Count; i++)
            {
                for (int j = 0; j < nameOfScreenshots.Length; j++)
                {
                    if (serializationData.answeredScreenshots[i] == nameOfScreenshots[j])
                    {
                        //MessageBox.Show("i = " + i + "; j = " + j);
                        screenshotsCompleted++;
                        screenshots[i] = null;

                        for (int k = lastScreenshotIndex; k < screenshots.Length - 1; k++)
                        {
                            screenshots[k] = screenshots[k + 1];
                        }
                    }
                }
            }
        }
    }
}