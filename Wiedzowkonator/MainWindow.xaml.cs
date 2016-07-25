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
using Button = System.Windows.Controls.Button;

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
        public string[] points = new string[12]; //Number of participant points
        public string[] participants = new string[12]; //Name of participants
        public string[] _screenshotsLocalizationPath; //Path to quiz files (screenshot)
        public enum answerType { noAnswer, fileNameAnswer, customAnswer };
        public answerType curAnswerType;
        public enum quizType { screenshot, text, music, mixed };
        public quizType curQuizType;
    }
    [Serializable]
    public class SerializationDataText
    {
        public List<string> answeredQuestions = new List<string>(); //List of which text questions were already shown
        public string[] points = new string[12]; //Number of participant points
        public string[] participants = new string[12]; //Name of participants
        public string txtFileLocalization; //Path to quiz files (text questions)
        public enum answerType { noAnswer, fileNameAnswer, customAnswer };
        public answerType curAnswerType;
        public enum quizType { screenshot, text, music, mixed };
        public quizType curQuizType;
    }
    [Serializable]
    public class SerializationDataMusic
    {
        public List<string> answeredQuestions = new List<string>(); //List of which music questions were already shown
        public string[] points = new string[12]; //Number of participant points
        public string[] participants = new string[12]; //Name of participants
        public string[] musicFilesLocalization; //Path to quiz files (music)
        public enum answerType { noAnswer, fileNameAnswer, customAnswer };
        public answerType curAnswerType;
        public enum quizType { screenshot, text, music, mixed };
        public quizType curQuizType;
    }

    [Serializable]
    public class SerializationDataMixed
    {
        public List<string> answeredScreenshots = new List<string>();
        public List<string> answeredText = new List<string>();
        public List<string> answeredMusic = new List<string>();
        public string[] localizationScreenshot;
        public string localizationText;
        public string[] localizationMusic;

        public string[] points = new string[12];
        public string[] participants = new string[12];

        public enum answerType { noAnswer, fileNameAnswer, customAnswer };
        public answerType curAnswerType;
        public enum quizType { screenshot, text, music, mixed };
        public quizType curQuizType;
    }

    public partial class MainWindow : Window
    {
        //public Participants participants;
        /*** Screenshot quiz variables ***/
        SerializationData serializationData = new SerializationData(); //Variable class
        SerializationDataText serializationText = new SerializationDataText();
        SerializationDataMusic serializationMusic = new SerializationDataMusic();
        SerializationDataMixed serializationMixed = new SerializationDataMixed();

        int numberOfParticipants = 12;
        bool finishedQuiz = false;
        System.Windows.Controls.TextBox[] participantsNames = new System.Windows.Controls.TextBox[16];
        System.Windows.Controls.TextBlock[] participantsPoints = new System.Windows.Controls.TextBlock[16];
        System.Windows.Controls.Button[] participantsPlus = new Button[16];
        System.Windows.Controls.Button[] participantsMinus = new Button[16];
        /************************/

        public Image[] screenshots; //Screenshots that will be shown on canvas
        BitmapImage[] bitmapImage; //Images that will be written into "screenshots" variable
        public string[] nameOfScreenshots; //Name of all screenshots - it is used to check which one was already shown
        public bool screenshotQuizStarted; //Checking if quiz has started - switching between choosing and answering phase
        int lastScreenshotIndex; //Getting index of last shown screenshot in case if user want to see it once again
        int screenshotsCompleted; //If user already answered this screenshot it won't be shown again
        static string userName = Environment.UserName; //Name of user logged on Windows account
        string quickSavePath = "C:/Users/" + userName + "/AppData/LocalLow/Wiedzowkonator/"; //Choosing directory path
        string[] screenshotsLocalizationPath; //Localizition of directory which contains screenshots
        int customAnswerIndex; //Index that increases after pressing "next"; Help with managing custom answers
        /*** ___________________________ ***/
        /*** Text quiz variables ***/
        public string[] textQuestions; //Imported questions. It's every second line of txt file (starting from 1st line)
        string[] textTitles; //Imported titles. It's every second line of txt file (starting from 1st line); Every title is written is quotation marks >>""<<
        string[] textAnswers; //Imported answers; It's every second line of txt file (starting from 2nd line)
        int lastQuestionIndex; //Last question index
        int textQuestionsCompleted; //Checking how many questions were already shown to contestants
        string txtFilePath; //Path to txt file from which question/titles/answers were (or will be if saved) imported
        /*** ___________________________ ***/
        /*** Music quiz variables ***/
        private MediaPlayer mediaPlayer = new MediaPlayer();
        //bool draggingProgressSlider = false; //Currently unused
        string[] musicFilesPath;
        int musicQuestionsCompleted;
        /*** Mixed quiz variables ***/
        List<int> mixedQuizIndexes = new List<int>();
        List<int> mixedQuizScreenshot = new List<int>();
        List<int> mixedQuizText = new List<int>();
        List<int> mixedQuizMusic = new List<int>();
        bool isMixedQuiz = false;
        int mixedIndex = 0; //Something similar to lastIndex, but since there are 2 layers of lists then I need other index for mixedQuizText; Music; Screen

        //Enum type that shows which state of quiz is currently in progress
        enum quizState { customizingQuestions, choosingQuestion, answeringQuestion, givingPoints };
        quizState curQuizState;

        enum answerType { noAnswer, fileNameAnswer, customAnswer };
        answerType curAnswerType;

        enum quizType { screenshot, text, music, mixed };
        quizType curQuizType;

        //This enum is used >>ONLY<< for mixed quiz. Don't touch when operating on other quizes
        enum subType { screenshot, text, music }
        subType curSubType;

        /* END OF VARIABLES */
        public MainWindow()
        {
            InitializeComponent(); //Opening window
            Start(); //Initializing window's properties like width, sizes etc.
            //Variables used for making simple mp3 player for music quiz
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        public void Start()
        {
            if (!Directory.Exists(quickSavePath)) //If directory for quicksave doesn't exists, one is made
            {
                Directory.CreateDirectory(quickSavePath);
            }
            InitializingParticipants();
            curQuizType = quizType.text;
            Application.Current.MainWindow.WindowState = WindowState.Maximized; //Starting fullscreen
            canvasBorder.BorderThickness = new Thickness(2.5f);
            DeletingScreeshots();
            SwitchingOffAllFields(); //Setting all unused currently controls off (not really switching off, rather making them "disappear" for a moment)
            ChoosingQuizType();
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
                int questionLength = 0; //Variable to check the biggest number of question for different types of quiz
                bool isNumeric = int.TryParse(questionNumberBox.Text, out tryingParse);
                if (isNumeric)
                    isNumeric = Math.Sign(int.Parse(questionNumberBox.Text)) == 1;
                //Correctly written number
                if (curQuizType == quizType.screenshot)
                    questionLength = screenshots.Length - screenshotsCompleted;
                else if (curQuizType == quizType.text)
                    questionLength = textQuestions.Length - textQuestionsCompleted;
                else if (curQuizType == quizType.music)
                    questionLength = musicFilesPath.Length - musicQuestionsCompleted;
                else if (curQuizType == quizType.mixed)
                    questionLength = mixedQuizIndexes.Count;
                if (!string.IsNullOrWhiteSpace(questionNumberBox.Text) && isNumeric && int.Parse(questionNumberBox.Text) <= questionLength)
                {
                    if (curQuizType == quizType.screenshot)
                    {
                        //Substracting 1 to let user writting down from "1 to x" instead of "0 to x"
                        int currentScreenshot = lastScreenshotIndex = int.Parse(questionNumberBox.Text) - 1;
                        canvasScreenshotQuiz.Width = screenshots[currentScreenshot].Width;
                        canvasScreenshotQuiz.Height = screenshots[currentScreenshot].Height;

                        screenshots[currentScreenshot].Stretch = Stretch.Fill;
                        ScreenshotSkipper.Focus();

                        canvasScreenshotQuiz.Children.Add(screenshots[currentScreenshot]);
                    }
                    else if (curQuizType == quizType.text)
                    {
                        int currentQuestion = lastQuestionIndex = int.Parse(questionNumberBox.Text) - 1;
                        questionText.Text = textQuestions[currentQuestion];
                        ShowingQuestion(); //Showing question text and button to skip to answer and giving points
                    }
                    else if (curQuizType == quizType.music)
                    {
                        int currentMusicQuestionIndex = lastQuestionIndex = int.Parse(questionNumberBox.Text) - 1;
                        mediaPlayer.Open(new Uri(musicFilesPath[currentMusicQuestionIndex]));
                        MusicHUDShow();
                    }
                    else if (curQuizType == quizType.mixed)
                        StartMixedQuiz(lastQuestionIndex = int.Parse(questionNumberBox.Text) - 1);

                    questionNumberBox.Text = "";
                }
                else //Uncorrectly written number
                {
                    if (curQuizType == quizType.screenshot)
                    {
                        if (screenshots.Length - screenshotsCompleted == 0) MessageBox.Show("Skończyły się pytania");
                        else MessageBox.Show("Wybierz liczby od 1 do " + (screenshots.Length - screenshotsCompleted));
                    }
                    else if (curQuizType == quizType.text)
                    {
                        if (textQuestions.Length - textQuestionsCompleted == 0) MessageBox.Show("Skończyły się pytania");
                        else MessageBox.Show("Wybierz liczby od 1 do " + (textQuestions.Length - textQuestionsCompleted));
                    }
                    else if (curQuizType == quizType.music)
                    {
                        if (musicFilesPath.Length - musicQuestionsCompleted == 0) MessageBox.Show("Skończyły się pytania");
                        else MessageBox.Show("Wybierz liczby od 1 do " + (musicFilesPath.Length - musicQuestionsCompleted));
                    }
                    else if (curQuizType == quizType.mixed)
                    {
                        if (mixedQuizIndexes.Count == 0) MessageBox.Show("Skończyły się pytania");
                        else MessageBox.Show("Wybierz liczby od 1 do " + (mixedQuizIndexes.Count));
                    }
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
                questionText.Text = "";
            }

            if (nonBuggedEntry) //If no error occured
            {
                ShowingHUD();
            }
        }

        void ShowingHUD()
        {
            //In giving points phase, all controls that allows to give participants points are being shown
            if (participantsPlus[0].Width == 0 && curQuizState == quizState.givingPoints)
            {
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    participantsPlus[i].Width = 33;
                    participantsMinus[i].Width = 33;
                }
                confirmPoints.Width = 75;
                goBack.Width = 75;
                correctAnswer.Width = 200;
                if (curQuizType == quizType.text) questionText.Width = 0; //Hiding textblock with question
            }
            else //Leaving this phase
            {
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    participantsPlus[i].Width = 0;
                    participantsMinus[i].Width = 0;
                }
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    participantsNames[i].Width = 0;
                    participantsPoints[i].Width = 0;
                }
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
            string name = (sender as Button).Name.ToString();
            name = name.Remove(0, "plusParticipant".Length);
            int index = int.Parse(name) - 1;
            float curPoints = float.Parse(participantsPoints[index].Text);
            curPoints++;
            if (curPoints == (int)curPoints)
                participantsPoints[index].Text = curPoints.ToString() + ",0";
            else
                participantsPoints[index].Text = curPoints.ToString();
        }

        private void minutFirstParticipant_Click(object sender, RoutedEventArgs e) //Decreasing 1st participant points
        {
            string name = (sender as Button).Name.ToString();
            name = name.Remove(0, "minusParticipant".Length);
            int index = int.Parse(name) - 1;
            float curPoints = float.Parse(participantsPoints[index].Text);
            curPoints--;
            if (curPoints == (int)curPoints)
                participantsPoints[index].Text = curPoints.ToString() + ",0";
            else
                participantsPoints[index].Text = curPoints.ToString();
        }
        //If user is done, then he leaves giving points phase and go back to choosing question
        private void confirmPoints_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.choosingQuestion;
            ShowingHUD();
            if (curQuizType == quizType.screenshot)
            {
                screenshots[lastScreenshotIndex] = null;
                FileInfo info = new FileInfo(bitmapImage[lastScreenshotIndex].UriSource.LocalPath);
                string[] nameToPass = info.FullName.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                serializationData.answeredScreenshots.Add(nameToPass[0] + info.Extension); //Adding name of completed question

                screenshotsCompleted++;
                for (int i = lastScreenshotIndex; i < screenshots.Length - 1; i++)
                {
                    screenshots[i] = screenshots[i + 1];
                    bitmapImage[i] = bitmapImage[i + 1];
                }

                if (screenshots.Length == screenshotsCompleted)
                {
                    screenshots[lastScreenshotIndex] = null;
                    MessageBox.Show("KONIEC WIEDZÓWKI!");
                    FinishedQuiz();
                    finishedQuiz = true;
                }
            }
            else if (curQuizType == quizType.text)
            {
                //Setting every three values to null for last index
                serializationText.answeredQuestions.Add(textQuestions[lastQuestionIndex]);
                textQuestions[lastQuestionIndex] = null; textAnswers[lastQuestionIndex] = null; textTitles[lastQuestionIndex] = null;
                textQuestionsCompleted++;
                for (int i = lastQuestionIndex; i < textQuestions.Length - 1; i++)
                {
                    textQuestions[i] = textQuestions[i + 1];
                    textAnswers[i] = textAnswers[i + 1];
                    textTitles[i] = textTitles[i + 1];
                }
                if (textQuestions.Length == textQuestionsCompleted)
                {
                    textQuestions[lastQuestionIndex] = null; textAnswers[lastQuestionIndex] = null; textTitles[lastQuestionIndex] = null;
                    MessageBox.Show("KONIEC WIEDZÓWKI!");
                    FinishedQuiz();
                    finishedQuiz = true;
                }
            }
            else if (curQuizType == quizType.music)
            {
                serializationMusic.answeredQuestions.Add(musicFilesPath[lastQuestionIndex]);
                musicFilesPath[lastQuestionIndex] = null;
                musicQuestionsCompleted++;
                for (int i = lastQuestionIndex; i < musicFilesPath.Length - 1; i++)
                {
                    musicFilesPath[i] = musicFilesPath[i + 1];
                }
                if (musicFilesPath.Length == musicQuestionsCompleted)
                {
                    musicFilesPath[lastQuestionIndex] = null;
                    MessageBox.Show("KONIEC WIEDZÓWKI!");
                    FinishedQuiz();
                    finishedQuiz = true;
                }
            }
            else if (curQuizType == quizType.mixed)
                ConfirmPointsMixed();
            QuickSave(); //If everything went good, current state is being save and if power is off or program crashes, user can load his last save
            if (finishedQuiz == false)
            {
                SwitchingOffAllFields();
                ChoosingQuestion();
            }
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
            if (curQuizState == quizState.givingPoints)
            {
                curQuizState = quizState.answeringQuestion;
                ShowingHUD(); //TODO -- a bit out of date but it's working good
                if (screenshotQuizStarted == false)
                    screenshotQuizStarted = true;
                else
                    screenshotQuizStarted = false;
                if (curQuizType == quizType.screenshot)
                {
                    canvasScreenshotQuiz.Children.Add(screenshots[lastScreenshotIndex]);
                    ScreenshotSkipper.Focus();
                }
                else if (curQuizType == quizType.text)
                {
                    SwitchingOffAllFields();
                    ShowingQuestion();
                    questionText.Text = textQuestions[lastQuestionIndex];
                }
                else if (curQuizType == quizType.music)
                {
                    SwitchingOffAllFields();
                    MusicHUDShow();
                }
                else if (curQuizType == quizType.mixed)
                {
                    if (curSubType == subType.screenshot)
                        canvasScreenshotQuiz.Children.Add(screenshots[mixedIndex]);
                    else if (curSubType == subType.text)
                    {
                        SwitchingOffAllFields();
                        ShowingQuestion();
                        questionText.Text = textQuestions[mixedIndex];
                    }
                    else if (curSubType == subType.music)
                    {
                        SwitchingOffAllFields();
                        MusicHUDShow();
                    }
                }
            }
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
            if (e.LeftButton == MouseButtonState.Pressed && curQuizState != quizState.customizingQuestions)
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
                    if (file.Name.Contains("@correct_answer_")) //If file has "correct_answer" in name, but user want use other checking answers method
                    {
                        string[] correctAnswerToPass = file.Name.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        correctAnswer.Text = "Poprawna odpowiedź: " + correctAnswerToPass[0];
                    }
                    else //When file is untouched
                        correctAnswer.Text = "Poprawna odpowiedź: " + file.Name.Replace(file.Extension, "");
                }
                else if (curAnswerType == answerType.customAnswer)
                {
                    FileInfo pathInfo = new FileInfo(bitmapImage[lastScreenshotIndex].UriSource.LocalPath);
                    string path = pathInfo.Name.Replace(pathInfo.Extension, "");
                    if (path.Contains("@correct_answer_"))
                    {
                        string[] correctAnswerToPass = path.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        correctAnswer.Text = "Poprawna odpowiedź: " + correctAnswerToPass[1];
                    }
                }
                SwitchingOffAllFields();
                GivingPoints_ShowingAnswers();
            }
        }


        private void ScreenshotSkipper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && curQuizState != quizState.customizingQuestions)
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
                    if (file.Name.Contains("@correct_answer_")) //If file has "correct_answer" in name, but user want use other checking answers method
                    {
                        string[] correctAnswerToPass = file.Name.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        correctAnswer.Text = "Poprawna odpowiedź: " + correctAnswerToPass[0];
                    }
                    else //When file is untouched
                        correctAnswer.Text = "Poprawna odpowiedź: " + file.Name.Replace(file.Extension, "");
                }
                else if (curAnswerType == answerType.customAnswer)
                {
                    FileInfo pathInfo = new FileInfo(bitmapImage[lastScreenshotIndex].UriSource.LocalPath);
                    string path = pathInfo.Name.Replace(pathInfo.Extension, "");
                    if (path.Contains("@correct_answer_"))
                    {
                        string[] correctAnswerToPass = path.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        correctAnswer.Text = "Poprawna odpowiedź: " + correctAnswerToPass[1];
                    }
                }
                SwitchingOffAllFields();
                GivingPoints_ShowingAnswers();
            }
        }

        private void SkipQuestion_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.givingPoints;
            StartClick(null, null);
            if (curQuizType == quizType.text) correctAnswer.Text = textAnswers[lastQuestionIndex];
            else if (curSubType == subType.text) correctAnswer.Text = textAnswers[mixedIndex];
            SwitchingOffAllFields();
            GivingPoints_ShowingAnswers(); //Showing answer and giving points to contestants
        }

        private void SkipMusicQuestion_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.givingPoints;
            StartClick(null, null);
            mediaPlayer.Stop();
            SwitchingOffAllFields();
            GivingPoints_ShowingAnswers();
        }

        #region Open, Save, Load
        /********************* Managing menu tabs ************************/
        private void Open(object sender, RoutedEventArgs e) //Importing new screenshots
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (curQuizType == quizType.screenshot)
            {
                fileDialog.Multiselect = true;
                #region fileDialog.Filter
                fileDialog.Filter =
                "Graphic Files (*.*)| *.png;*.jpg;*.jpeg;*.jpe;*.jfif;*.bmp;*.dib;*.rle;*.gif;*.tif;*.tiff|PNG Files (*.PNG)| *.png|JPEG Files (*.JPG); (*.JPEG); (*.JPE); (*JFIF)|*.jpg;*.jpeg;*.jpe;*.jfif|BMP Files (*.BMP); (*.DIB); (*.RLE)|*.bmp;*.dib;*.rle|GIF Files (*.GIF)|*.gif|TIFF Files (*.TIF); (*.TIFF)|*.tif;*.tiff";
                #endregion
                //fileDialog.Filter = bmpFiles;
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string[] path = fileDialog.FileNames;
                    //Initialize new array sizes depending of screenshot numbers
                    bitmapImage = new BitmapImage[path.Length];
                    screenshots = new Image[bitmapImage.Length];
                    nameOfScreenshots = new string[bitmapImage.Length];
                    //screenshotsLocalizationPath = Directory.GetParent(path[0]).ToString(); //Getting directory where screenshots are stored
                    //Temporary image and bitmapImage arrays. They're used to pass screenshots when randomizing questions
                    screenshotsLocalizationPath = path;
                    Image[] screenshotsToPass = new Image[bitmapImage.Length];
                    BitmapImage[] bitmapImageToPass = new BitmapImage[bitmapImage.Length];
                    int index = 0; //Index of each screenshot
                    foreach (string screen in path)
                    {
                        bitmapImage[index] = bitmapImageToPass[index] = new BitmapImage(new Uri(screen, UriKind.Absolute));
                        screenshots[index] = screenshotsToPass[index] = new Image();
                        screenshots[index].Source = screenshotsToPass[index].Source = bitmapImage[index];
                        screenshots[index].Width = screenshotsToPass[index].Width = bitmapImage[index].Width;
                        screenshots[index].Height = screenshotsToPass[index].Height = bitmapImage[index].Height;
                        nameOfScreenshots[index] = screen;

                        index++;
                    }
                    Random random = new Random();
                    List<int> indexes = new List<int>();
                    for (int i = 0; i < screenshots.Length; i++)
                    {
                        indexes.Add(i);
                    }

                    for (int i = 0; i < screenshots.Length; i++)
                    {
                        int toDelete = random.Next(0, indexes.Count);
                        int toPass = indexes[toDelete];
                        screenshots[toPass] = screenshotsToPass[i];
                        bitmapImage[toPass] = bitmapImageToPass[i];
                        indexes.RemoveAt(toDelete);
                    }

                    noAnswerButton.Width = 800;
                    fileNameAnswerButton.Width = 800;
                    customAnswerButton.Width = 800;
                }
            }
            else if (curQuizType == quizType.text)
            {
                fileDialog.Multiselect = false;
                fileDialog.Filter = "TXT Files (*.TXT) | *.txt";
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fileDialog.FileName;
                    if (new FileInfo(path).Extension == ".txt")
                    {
                        txtFilePath = path;
                        OpeningTextQuiz(path);
                    }
                    else
                        MessageBox.Show("Proszę wybrać plik o odpowiednim rozszerzeniu (*.txt). Obecnie wybrany plik: \n" + path + " posiada niepoprawne rozszerzenie");
                }
            }
            else if (curQuizType == quizType.music)
            {
                fileDialog.Multiselect = true;
                #region fileDialog.Filter 
                fileDialog.Filter =
                "Music Files (*.*)|*.mp3;*.flac;*.wma;*.mp2;*.mpg;*.mpe;*.mpeg;*.aif;*.aiff;*.aifc;*.aifr;*.mp4|MP3 Files (*.MP3)|*.mp3|FLAC Files (*.FLAC)|*.flac|WMA Files (*.WMA)|*.wma|MP Files (*.MP2); (*.MPG); (*.MPE); (*.MPEG); (*.MPEG2)|*.mp2;*.mpg;*.mpe;*.mpeg";
                #endregion
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    musicFilesPath = new string[fileDialog.FileNames.Length];
                    serializationMusic.musicFilesLocalization = new string[musicFilesPath.Length];
                    for (int i = 0; i < musicFilesPath.Length; i++)
                    {
                        musicFilesPath[i] = fileDialog.FileNames[i];
                        serializationMusic.musicFilesLocalization[i] = musicFilesPath[i];
                    }
                }
            }
            else if (curQuizType == quizType.mixed)
                OpenMixedQuiz();

        }
        //Saving current state to file in chosen location
        private void Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream; //Stream has to be closed separately because I'm using different extensions for each stream and it can't be initialized universally
                if (curQuizType == quizType.screenshot)
                {
                    stream = File.Create(path + ".animescreen");
                    SerializationData data = new SerializationData();

                    data.answeredScreenshots = serializationData.answeredScreenshots;
                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        data.points[i] = participantsPoints[i].Text;
                        data.participants[i] = participantsNames[i].Text;
                    }
                    data._screenshotsLocalizationPath = screenshotsLocalizationPath;
                    data.curAnswerType = serializationData.curAnswerType = (SerializationData.answerType)curAnswerType;
                    data.curQuizType = serializationData.curQuizType = (SerializationData.quizType)curQuizType;

                    bf.Serialize(stream, data);
                    stream.Close(); //Closing stream to prevent from damaging data
                }
                else if (curQuizType == quizType.text)
                {
                    stream = File.Create(path + ".animetext");
                    SerializationDataText data = new SerializationDataText();

                    data.answeredQuestions = serializationText.answeredQuestions;
                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        data.points[i] = participantsPoints[i].Text;
                        data.participants[i] = participantsNames[i].Text;
                    }
                    data.txtFileLocalization = txtFilePath;
                    data.curAnswerType = serializationText.curAnswerType = (SerializationDataText.answerType)curAnswerType;
                    data.curQuizType = serializationText.curQuizType = (SerializationDataText.quizType)curQuizType;

                    bf.Serialize(stream, data);
                    stream.Close(); //Closing stream to prevent from damaging data
                }
                else if (curQuizType == quizType.music)
                {
                    stream = File.Create(path + ".animemusic");
                    SerializationDataMusic data = new SerializationDataMusic();

                    data.answeredQuestions = serializationMusic.answeredQuestions;
                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        data.points[i] = participantsPoints[i].Text;
                        data.participants[i] = participantsNames[i].Text;
                    }
                    data.musicFilesLocalization = serializationMusic.musicFilesLocalization;
                    data.curAnswerType = serializationMusic.curAnswerType = (SerializationDataMusic.answerType)curAnswerType;
                    data.curQuizType = serializationMusic.curQuizType = (SerializationDataMusic.quizType)curQuizType;

                    bf.Serialize(stream, data);
                    stream.Close(); //Closing stream to prevent from damaging data
                }
                else if (curQuizType == quizType.mixed)
                    SaveMixedQuiz();
            }
        }
        //Loading state of quiz from .anime file
        private void Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (curQuizType == quizType.screenshot) fileDialog.Filter = "AnimeScreenshot (*.ANIMESCREEN)| *.animescreen";
            else if (curQuizType == quizType.text) fileDialog.Filter = "AnimeText (*.ANIMETEXT)| *.animetext";
            else if (curQuizType == quizType.music) fileDialog.Filter = "AnimeMusic (*.ANIMEMUSIC)| *.animemusic";
            else if (curQuizType == quizType.mixed)
                LoadMixedQuiz();

            if (curQuizType != quizType.mixed)
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(path, FileMode.Open);
                if (curQuizType == quizType.screenshot)
                {
                    SerializationData data = (SerializationData)bf.Deserialize(stream);
                    serializationData.answeredScreenshots = data.answeredScreenshots;
                    
                    //nameFirstParticipant.Text = data.participants[0];
                    //pointsFirstParticipant.Text = data.points[0];
                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        participantsPoints[i].Text = data.points[i];
                        participantsNames[i].Text = data.participants[i];
                    }
                    screenshotsLocalizationPath = data._screenshotsLocalizationPath;
                    curAnswerType = (answerType)data.curAnswerType;
                    curQuizType = (quizType)data.curQuizType;
                    MessageBox.Show(curAnswerType.ToString());

                    bitmapImage = new BitmapImage[screenshotsLocalizationPath.Length];
                    screenshots = new Image[bitmapImage.Length];
                    nameOfScreenshots = new string[bitmapImage.Length];
                    for (int k = 0; k < screenshotsLocalizationPath.Length; k++)
                    {
                        bitmapImage[k] = new BitmapImage(new Uri(screenshotsLocalizationPath[k], UriKind.Absolute));
                        screenshots[k] = new Image();
                        screenshots[k].Source = bitmapImage[k];
                        screenshots[k].Width = bitmapImage[k].Width;
                        screenshots[k].Height = bitmapImage[k].Height;
                        nameOfScreenshots[k] = screenshotsLocalizationPath[k];

                        string[] correctNameOfScreenshot = nameOfScreenshots[k].Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        FileInfo info = new FileInfo(nameOfScreenshots[k]);
                        nameOfScreenshots[k] = correctNameOfScreenshot[0] + info.Extension;
                    }

                    for (int i = 0; i < serializationData.answeredScreenshots.Count; i++)
                    {
                        for (int j = 0; j < nameOfScreenshots.Length; j++)
                        {
                            if (serializationData.answeredScreenshots[i] == nameOfScreenshots[j])
                            {
                                screenshotsCompleted++;

                                for (int k = j; k < screenshots.Length - 1; k++)
                                {
                                    screenshots[k] = screenshots[k + 1];
                                    bitmapImage[k] = bitmapImage[k + 1];
                                }
                            }
                        }
                    }
                }
                else if (curQuizType == quizType.text)
                {
                    SerializationDataText data = (SerializationDataText)bf.Deserialize(stream);
                    serializationText.answeredQuestions = data.answeredQuestions;
                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        participantsPoints[i].Text = data.points[i];
                        participantsNames[i].Text = data.participants[i];
                    }
                    txtFilePath = data.txtFileLocalization;
                    curAnswerType = (answerType)data.curAnswerType;
                    curQuizType = (quizType)data.curQuizType;

                    if (File.Exists(txtFilePath))
                        OpeningTextQuiz(txtFilePath); //Loading new quiz from file
                    else MessageBox.Show("Błąd importowania! Proszę zresetować program i upewnić się, że plik " + txtFilePath + " istnieje.");

                    for (int i = 0; i < serializationText.answeredQuestions.Count; i++)
                    {
                        for (int j = 0; j < textQuestions.Length; j++)
                        {
                            if (serializationText.answeredQuestions[i] == textQuestions[j])
                            {
                                textQuestionsCompleted++;
                                textQuestions[i] = null; textTitles[i] = null; textAnswers[i] = null;
                                for (int k = j; k < textQuestions.Length - 1; k++)
                                {
                                    textQuestions[k] = textQuestions[k + 1];
                                    textAnswers[k] = textAnswers[k + 1];
                                    textTitles[k] = textTitles[k + 1];
                                }
                            }
                        }
                    }
                }
                else if (curQuizType == quizType.music)
                {
                    SerializationDataMusic data = (SerializationDataMusic)bf.Deserialize(stream);
                    serializationMusic.answeredQuestions = data.answeredQuestions;
                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        participantsPoints[i].Text = data.points[i];
                        participantsNames[i].Text = data.participants[i];
                    }
                    musicFilesPath = data.musicFilesLocalization;
                    curAnswerType = (answerType)data.curAnswerType;
                    curQuizType = (quizType)data.curQuizType;

                    for (int i = 0; i < serializationMusic.answeredQuestions.Count; i++)
                    {
                        for (int j = 0; j < musicFilesPath.Length; j++)
                        {
                            if (serializationMusic.answeredQuestions[i] == musicFilesPath[j])
                            {
                                musicQuestionsCompleted++;
                                musicFilesPath[i] = null;
                                for (int k = j; k < musicFilesPath.Length - 1; k++)
                                {
                                    musicFilesPath[k] = musicFilesPath[k + 1];
                                }
                            }
                        }
                    }
                }
                stream.Close(); //Closing stream to prevent from damaging data
                    curQuizState = quizState.choosingQuestion;
            }
        }

        private void DeleteEndings(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] paths = fileDialog.FileNames;
                string[] pathParts = new string[2];
                string readyPath;
                for (int i = 0; i < paths.Length; i++)
                {
                    readyPath = "";
                    FileInfo info = new FileInfo(paths[i]);
                    pathParts = paths[i].Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                    readyPath = pathParts[0] + info.Extension;

                    if (readyPath != "")
                        File.Move(paths[i], readyPath);
                }
            }
        }

        private void QuickSave() //Saving after all confirmed answer
        {
            string path = quickSavePath + "quickSave-" + DateTime.Now.ToString("dd/MM/yyyy HH_mm");

            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream;
            if (curQuizType == quizType.screenshot)
            {
                stream = File.Create(path + ".animescreen");
                SerializationData data = new SerializationData();

                data.answeredScreenshots = serializationData.answeredScreenshots;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    data.points[i] = participantsPoints[i].Text;
                    data.participants[i] = participantsNames[i].Text;
                }
                data._screenshotsLocalizationPath = screenshotsLocalizationPath;
                data.curAnswerType = serializationData.curAnswerType = (SerializationData.answerType)curAnswerType;
                data.curQuizType = serializationData.curQuizType = (SerializationData.quizType)curQuizType;

                bf.Serialize(stream, data);
                stream.Close(); //Closing stream to prevent from damaging data
            }
            else if (curQuizType == quizType.text)
            {
                stream = File.Create(path + ".animetext");
                SerializationDataText data = new SerializationDataText();

                data.answeredQuestions = serializationText.answeredQuestions;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    data.points[i] = participantsPoints[i].Text;
                    data.participants[i] = participantsNames[i].Text;
                }   
                data.txtFileLocalization = txtFilePath;
                data.curAnswerType = serializationText.curAnswerType = (SerializationDataText.answerType)curAnswerType;
                data.curQuizType = serializationText.curQuizType = (SerializationDataText.quizType)curQuizType;

                bf.Serialize(stream, data);
                stream.Close(); //Closing stream to prevent from damaging data      
            }

            else if (curQuizType == quizType.music)
            {
                stream = File.Create(path + ".animemusic");
                SerializationDataMusic data = new SerializationDataMusic();

                data.answeredQuestions = serializationMusic.answeredQuestions;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    data.points[i] = participantsPoints[i].Text;
                    data.participants[i] = participantsNames[i].Text;
                }
                data.musicFilesLocalization = serializationMusic.musicFilesLocalization;
                data.curAnswerType = serializationMusic.curAnswerType = (SerializationDataMusic.answerType)curAnswerType;
                data.curQuizType = serializationMusic.curQuizType = (SerializationDataMusic.quizType)curQuizType;
                bf.Serialize(stream, data);
                stream.Close(); //Closing stream to prevent from damaging da
            }
            else if (curQuizType == quizType.mixed)
                QuickSaveMixedQuiz();
        }

        private void QuickLoad() //Loading at the start of quiz
        {
            //Loading data containing deeper information and values of variables
            DirectoryInfo directory = new DirectoryInfo(quickSavePath);
            FileInfo[] pathToCheck = directory.GetFiles();
            List<FileInfo> extensionPaths = new List<FileInfo>();
            for (int i = 0; i < pathToCheck.Length; i++)
            {
                if (curQuizType == quizType.screenshot)
                {
                    if (pathToCheck[i].Extension == ".animescreen") extensionPaths.Add(pathToCheck[i]);
                }
                else if (curQuizType == quizType.text)
                {
                    if (pathToCheck[i].Extension == ".animetext") extensionPaths.Add(pathToCheck[i]);
                }
                else if (curQuizType == quizType.music)
                {
                    if (pathToCheck[i].Extension == ".animemusic") extensionPaths.Add(pathToCheck[i]);
                }
                else if (curQuizType == quizType.mixed)
                {
                    if (pathToCheck[i].Extension == ".animemixed") extensionPaths.Add(pathToCheck[i]);
                }
            }
            string path = (from f in extensionPaths
                           orderby f.LastWriteTime descending
                           select f).First().FullName;
            //MessageBox.Show(path);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);
            if (curQuizType == quizType.mixed) stream.Close(); //Instant closing stream to get access in another method if opening mixed Quiz

            if (curQuizType == quizType.screenshot)
            {
                SerializationData data = (SerializationData)bf.Deserialize(stream);
                serializationData.answeredScreenshots = data.answeredScreenshots;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    participantsPoints[i].Text = data.points[i];
                    participantsNames[i].Text = data.participants[i];
                }
                screenshotsLocalizationPath = data._screenshotsLocalizationPath;
                curAnswerType = (answerType)data.curAnswerType;

                MessageBox.Show(curAnswerType.ToString());
                bitmapImage = new BitmapImage[screenshotsLocalizationPath.Length];
                screenshots = new Image[bitmapImage.Length];
                nameOfScreenshots = new string[bitmapImage.Length];
                for (int k = 0; k < screenshotsLocalizationPath.Length; k++)
                {
                    bitmapImage[k] = new BitmapImage(new Uri(screenshotsLocalizationPath[k], UriKind.Absolute));
                    screenshots[k] = new Image();
                    screenshots[k].Source = bitmapImage[k];
                    screenshots[k].Width = bitmapImage[k].Width;
                    screenshots[k].Height = bitmapImage[k].Height;
                    nameOfScreenshots[k] = screenshotsLocalizationPath[k];

                    string[] correctNameOfScreenshot = nameOfScreenshots[k].Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                    FileInfo info = new FileInfo(nameOfScreenshots[k]);
                    nameOfScreenshots[k] = correctNameOfScreenshot[0] + info.Extension;
                }

                for (int i = 0; i < serializationData.answeredScreenshots.Count; i++)
                {
                    for (int j = 0; j < nameOfScreenshots.Length; j++)
                    {
                        if (serializationData.answeredScreenshots[i] == nameOfScreenshots[j])
                        {
                            screenshotsCompleted++;
                            for (int k = j; k < screenshots.Length - 1; k++)
                            {
                                screenshots[k] = screenshots[k + 1];
                                bitmapImage[k] = bitmapImage[k + 1];
                            }
                        }
                    }
                }
            }
            else if (curQuizType == quizType.text)
            {
                SerializationDataText data = (SerializationDataText)bf.Deserialize(stream);
                serializationText.answeredQuestions = data.answeredQuestions;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    participantsPoints[i].Text = data.points[i];
                    participantsNames[i].Text = data.participants[i];
                }
                txtFilePath = data.txtFileLocalization;
                curAnswerType = (answerType)data.curAnswerType;
                curQuizType = (quizType)data.curQuizType;

                if (File.Exists(txtFilePath))
                    OpeningTextQuiz(txtFilePath); //Loading new quiz from file
                else MessageBox.Show("Błąd importowania! Proszę zresetować program i upewnić się, że plik " + txtFilePath + " istnieje.");

                for (int i = 0; i < serializationText.answeredQuestions.Count; i++)
                {
                    for (int j = 0; j < textQuestions.Length; j++)
                    {
                        if (serializationText.answeredQuestions[i] == textQuestions[j])
                        {
                            textQuestionsCompleted++;
                            textQuestions[i] = null; textTitles[i] = null; textAnswers[i] = null;
                            for (int k = j; k < textQuestions.Length - 1; k++)
                            {
                                textQuestions[k] = textQuestions[k + 1];
                                textAnswers[k] = textAnswers[k + 1];
                                textTitles[k] = textTitles[k + 1];
                            }
                        }
                    }
                }
            }
            else if (curQuizType == quizType.music)
            {
                SerializationDataMusic data = (SerializationDataMusic)bf.Deserialize(stream);
                serializationMusic.answeredQuestions = data.answeredQuestions;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    participantsPoints[i].Text = data.points[i];
                    participantsNames[i].Text = data.participants[i];
                }
                musicFilesPath = data.musicFilesLocalization;
                curAnswerType = (answerType)data.curAnswerType;
                curQuizType = (quizType)data.curQuizType;

                for (int i = 0; i < serializationMusic.answeredQuestions.Count; i++)
                {
                    for (int j = 0; j < musicFilesPath.Length; j++)
                    {
                        if (serializationMusic.answeredQuestions[i] == musicFilesPath[j])
                        {
                            musicQuestionsCompleted++;
                            musicFilesPath[i] = null;
                            for (int k = j; k < musicFilesPath.Length - 1; k++)
                            {
                                musicFilesPath[k] = musicFilesPath[k + 1];
                            }
                        }
                    }
                }
            }
            else if (curQuizType == quizType.mixed)
                QuickLoadMixedQuiz(path);
            stream.Close(); //Closing stream to prevent from damaging data
            curQuizState = quizState.choosingQuestion;
        }
        #endregion
        /* Importing new quizes */
        #region Importing new quizes (screenshots)
        private void customAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            curAnswerType = answerType.customAnswer;
            serializationData.curAnswerType = SerializationData.answerType.customAnswer;
            customAnswerButton.Width = fileNameAnswerButton.Width = noAnswerButton.Width = 0;
            customAnswerIndex = 0; //Initializing index position with 0
            canvasScreenshotQuiz.Width = screenshots[customAnswerIndex].Width;
            canvasScreenshotQuiz.Height = screenshots[customAnswerIndex].Height;
            canvasScreenshotQuiz.Children.Add(screenshots[customAnswerIndex]); //Adding first screenshot to canvas

            FileInfo file = new FileInfo(bitmapImage[0].UriSource.LocalPath);
            if (file.Name.Contains("@correct_answer_"))
            {
                string fileName = file.Name.Replace(file.Extension, "");
                string[] correctAnswerToPass = fileName.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                customAnswerAdding.Text = correctAnswerToPass[1];
            }

            File.Create(quickSavePath + "toDelete.txt").Close();
            SwitchingOffAllFields(); //Clearing all currently open fields to width = 0
            CustomScreenshotAnswersAdding(); //Showing fields to add custom answers
        }

        private void fileNameAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            curAnswerType = answerType.fileNameAnswer;
            serializationData.curAnswerType = SerializationData.answerType.fileNameAnswer;
            customAnswerButton.Width = fileNameAnswerButton.Width = noAnswerButton.Width = 0;
            QuizHUDAfterImporting(true);
            curQuizState = quizState.choosingQuestion;
            SwitchingOffAllFields();
            ChoosingQuestion();
            if (isMixedQuiz)
            {
                SwitchingOffAllFields();
                OpenMixedQuiz();
            }
        }

        private void noAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            curAnswerType = answerType.noAnswer;
            serializationData.curAnswerType = SerializationData.answerType.noAnswer;
            customAnswerButton.Width = fileNameAnswerButton.Width = noAnswerButton.Width = 0;
            QuizHUDAfterImporting(true);
            curQuizState = quizState.choosingQuestion;
            SwitchingOffAllFields();
            ChoosingQuestion();
            if (isMixedQuiz)
            {
                SwitchingOffAllFields();
                OpenMixedQuiz();
            }
        }

        private void addingCustomAnswers(object sender, KeyEventArgs e)
        {
            if (curQuizState == quizState.customizingQuestions && e.Key == Key.Enter)
                customAnswerConfirm_Click(null, null);
        }

        private void customAnswerConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (curQuizState == quizState.customizingQuestions)
                customAnswerAdding_Button_or_Enter();
        }

        void customAnswerAdding_Button_or_Enter()
        {
            if (curQuizState == quizState.customizingQuestions)
            {
                if (customAnswerAdding.Text == "")
                    MessageBox.Show("Uwaga! Zostawiasz pustą odpowiedź.");
                FileInfo infoToPass = new FileInfo(bitmapImage[customAnswerIndex].UriSource.LocalPath);
                string newPath;
                if (!infoToPass.Name.Contains("@correct_answer_")) //When file doesn't contain correct answer in its name
                {
                    newPath = infoToPass.FullName.Replace(infoToPass.Extension, "")
                                + "@correct_answer_" + customAnswerAdding.Text + infoToPass.Extension;
                }
                else
                {
                    string[] pathPart = infoToPass.FullName.Split(new[] { "@correct_answer_" }, StringSplitOptions.None); ;
                    newPath = pathPart[0] + "@correct_answer_" + customAnswerAdding.Text + infoToPass.Extension;
                }
                if (!File.Exists(newPath))
                {
                    File.Copy(infoToPass.FullName, newPath);

                    bitmapImage[customAnswerIndex] = new BitmapImage(new Uri(newPath, UriKind.Absolute));
                    screenshots[customAnswerIndex] = new Image();
                    screenshots[customAnswerIndex].Source = bitmapImage[customAnswerIndex];
                    screenshots[customAnswerIndex].Width = bitmapImage[customAnswerIndex].Width;
                    screenshots[customAnswerIndex].Height = bitmapImage[customAnswerIndex].Height;

                    using (StreamWriter sw = File.AppendText(quickSavePath + "toDelete.txt"))
                        sw.WriteLine(infoToPass.FullName);
                }
                customAnswerIndex++;
                if (customAnswerIndex < screenshots.Length)
                {
                    canvasScreenshotQuiz.Children.Clear();
                    canvasScreenshotQuiz.Width = screenshots[customAnswerIndex].Width;
                    canvasScreenshotQuiz.Height = screenshots[customAnswerIndex].Height;
                    canvasScreenshotQuiz.Children.Add(screenshots[customAnswerIndex]);
                    customAnswerAdding.Text = "";
                    infoToPass = new FileInfo(bitmapImage[customAnswerIndex].UriSource.LocalPath);
                    if (infoToPass.Name.Contains("@correct_answer_"))
                    {
                        string fileName = infoToPass.Name.Replace(infoToPass.Extension, "");
                        string[] correctAnswerToPass = fileName.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        customAnswerAdding.Text = correctAnswerToPass[1];
                    }
                }
                else
                {
                    canvasScreenshotQuiz.Children.Clear();
                    QuizHUDAfterImporting(true);
                    curQuizState = quizState.choosingQuestion;
                    SwitchingOffAllFields();
                    ChoosingQuestion(); //Choosing question state after choosing all custom answers
                    if (isMixedQuiz)
                    {
                        SwitchingOffAllFields();
                        OpenMixedQuiz();
                    }
                }
            }
        }

        private void customAnswerBack_Click(object sender, RoutedEventArgs e)
        {
            customAnswerIndex--;
            if (customAnswerIndex < screenshots.Length)
            {
                canvasScreenshotQuiz.Children.Clear();
                canvasScreenshotQuiz.Width = screenshots[customAnswerIndex].Width;
                canvasScreenshotQuiz.Height = screenshots[customAnswerIndex].Height;
                canvasScreenshotQuiz.Children.Add(screenshots[customAnswerIndex]);
            }
        }

        void QuizHUDAfterImporting(bool show)
        {
            if (show)
            {
                questionNumberBox.Width = 132;
                StartButton.Width = 150;
                customAnswerAdding.Width = 0;
                customAnswerConfirm.Width = 0;
                customAnswerBack.Width = 0;
            }
            else
            {
                StartButton.Width = 0;
                questionNumberBox.Width = 0;
            }
        }

        void DeletingScreeshots()
        {
            if (File.Exists(quickSavePath + "toDelete.txt"))
            {
                using (StreamReader sr = File.OpenText(quickSavePath + "toDelete.txt"))
                {
                    string text = sr.ReadToEnd();
                    string[] lines = text.Split('\r');
                    foreach (string s in lines)
                    {
                        string toDelete = s.Replace("\\", "/");
                        toDelete = toDelete.Trim();
                        if (File.Exists(toDelete))
                            File.Delete(toDelete); //Deleting each file
                    }
                }

                File.Delete(quickSavePath + "toDelete.txt"); //Deleting file with data
            }
        }

        void OpeningTextQuiz(string pathToFile)
        {
            using (StreamReader sr = File.OpenText(pathToFile))
            {
                int currentIndex = 0;
                string text = sr.ReadToEnd();
                text = text.Trim();
                string[] lines = text.Split('\r');
                textTitles = new string[lines.Length / 2];
                textQuestions = new string[lines.Length / 2];
                textAnswers = new string[lines.Length / 2];

                foreach (string s in lines)
                {
                    s.Trim();
                    int count = s.Split('\"').Length - 1;
                    if (currentIndex % 2 == 0)
                    {
                        if (count == 2) //If in line are exactly 2 quotation marks
                        {
                            string[] textToShow = s.Split('\"'); //Splitting text to get title
                            textTitles[currentIndex / 2] = textToShow[1].Trim();
                            textQuestions[currentIndex / 2] = textToShow[2].Trim();
                        }
                        else //If there are more or less than 2 quotation marks
                        {
                            textTitles[currentIndex / 2] = "";
                            textQuestions[currentIndex / 2] = s.Trim();
                        }
                    }
                    else if (currentIndex % 2 == 1)
                        textAnswers[currentIndex / 2] = s.Trim();
                    currentIndex++;
                }
            }
        }

        /***************************************** Showing/hiding all fields and managing them ************************************************************/

        void SwitchingOffAllFields()
        {
            //Choosing type of quiz
            textQuizButton.Width = 0;
            screenshotQuizButton.Width = 0;
            musicQuizButton.Width = 0;
            mixedQuizButton.Width = 0;
            //Choosing how to import the quiz
            openQuizButton.Width = 0;
            loadQuizButton.Width = 0;
            quickLoadQuizButton.Width = 0;
            //Choosing number of question
            StartButton.Width = 0;
            questionNumberBox.Width = 0;
            //Showing question and button to proceed to its answer
            questionText.Width = 0;
            SkipQuestion.Width = 0;
            //Giving points, confirming points, going back to question
            confirmPoints.Width = 0;
            goBack.Width = 0;
            //correctAnswer.Width = 0; // --> This value is set to auto
            for (int i = 0; i < numberOfParticipants; i++)
            {
                participantsPlus[i].Width = 0;
                participantsMinus[i].Width = 0;
            }
            for (int i = 0; i < numberOfParticipants; i++)
            {
                participantsNames[i].Width = 0;
                participantsPoints[i].Width = 0;
            }
            //Asking user if he wants to reload last quiz 
            //TODO -- Rather unused in current context. Will fix in the future
            ReloadTextBlock.Width = 0;
            ReloadButton_Yes.Width = 0;
            ReloadButton_No.Width = 0;
            //Choosing type of answer's showing
            noAnswerButton.Width = 0;
            fileNameAnswerButton.Width = 0;
            customAnswerButton.Width = 0;
            //Adding custom answers, confirming, going back to last one
            customAnswerAdding.Width = 0;
            customAnswerConfirm.Width = 0;
            customAnswerBack.Width = 0;
            //Music player HUD
            playAudioButton.Width = 0;
            pauseAudioButton.Width = 0;
            stopAudioButton.Width = 0;
            progressAudioSlider.Width = 0;
            volumeAudioSlider.Width = 0;
            //TODO -- maybe add timer to music player HUD
            SkipMusicQuestion.Width = 0;
        }

        void ChoosingQuizType()
        {
            textQuizButton.Width = 250;
            screenshotQuizButton.Width = 250;
            musicQuizButton.Width = 250;
            mixedQuizButton.Width = 250;
        }

        void ChoosingQuizOpeningType()
        {
            //TODO - Ustalić wartości zależne od długości znaków w przycisku (+ ~15px)
            openQuizButton.Width = 250;
            loadQuizButton.Width = 250;
            quickLoadQuizButton.Width = 250;
        }

        void ChoosingQuestion()
        {
            StartButton.Width = 150;
            questionNumberBox.Width = 132; //This field is focusable (XAML)
            Keyboard.Focus(questionNumberBox);
        }

        void ShowingQuestion()
        {
            if (curQuizType == quizType.text || curSubType == subType.text)
            {
                questionText.Width = 250;
                SkipQuestion.Width = 150;
            }
        }

        void GivingPoints_ShowingAnswers()
        {
            //TODO -- ogarnąć wartości procentowe
            confirmPoints.Width = 75;
            goBack.Width = 75;
            //correctAnswer.Width = 0;
            for (int i = 0; i < numberOfParticipants; i++)
            {
                participantsNames[i].Width = 120;
                participantsPoints[i].Width = 33;
            }
            for (int i = 0; i < numberOfParticipants; i++)
            {
                participantsPlus[i].Width = 33;
                participantsMinus[i].Width = 33;
            }
        }

        void CustomScreenshotAnswers()
        {
            noAnswerButton.Width = 800;
            fileNameAnswerButton.Width = 800;
            customAnswerButton.Width = 800;
        }

        void CustomScreenshotAnswersAdding()
        {
            customAnswerAdding.Width = 120;
            customAnswerConfirm.Width = 75;
            customAnswerBack.Width = 75;
        }

        void MusicHUDShow()
        {
            playAudioButton.Width = 100;
            pauseAudioButton.Width = 100;
            stopAudioButton.Width = 100;
            progressAudioSlider.Width = 500;
            volumeAudioSlider.Width = 50;
            //Button to skip current music question
            SkipMusicQuestion.Width = 150;
        }
        /* END OF HUD METHODS */
        private void textQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curQuizType = quizType.text;
            SwitchingOffAllFields();
            ChoosingQuizOpeningType();
        }

        private void screenshotQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curQuizType = quizType.screenshot;
            SwitchingOffAllFields();
            ChoosingQuizOpeningType();
        }

        private void musicQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curQuizType = quizType.music;
            SwitchingOffAllFields();
            ChoosingQuizOpeningType();
        }

        private void mixedQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curQuizType = quizType.mixed;
            SwitchingOffAllFields();
            ChoosingQuizOpeningType();
        }

        private void openQuizButton_Click(object sender, RoutedEventArgs e)
        {
            Open(null, null);
            if (textQuestions != null || screenshots != null || musicFilesPath != null)
            {
                if (curQuizType == quizType.screenshot)
                {
                    SwitchingOffAllFields();
                    CustomScreenshotAnswers();
                }
                else
                {
                    SwitchingOffAllFields();
                    ChoosingQuestion();
                }
            }
        }

        private void loadQuizButton_Click(object sender, RoutedEventArgs e)
        {
            Load(null, null);
            if (textQuestions != null || screenshots != null || musicFilesPath != null)
            {
                SwitchingOffAllFields();
                ChoosingQuestion();
            }
        }

        private void quickLoadQuizButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO -- sprawdzanie rodzaju importowanej wiedzówki
            //TODO -- dodanie przycisku, który ładuje ostatnią wiedzówkę, niezależnie jakiego typu była
            QuickLoad();
            if (textQuestions != null || screenshots != null || musicFilesPath != null)
            {
                SwitchingOffAllFields();
                ChoosingQuestion();
            }
        }

        /************************************** MUSIC QUIZ METHODS *******************************************/
        private void playAudioButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Volume = volumeAudioSlider.Value / 10;
            mediaPlayer.Play();
        }

        private void pauseAudioButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void stopAudioButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void volumeAudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = e.NewValue / 10;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan))
            {
                progressAudioSlider.Minimum = 0;
                progressAudioSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                progressAudioSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void progressAudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(progressAudioSlider.Value);
        }
        /*********************************** MIXED QUIZ METHODS **********************************************/
        void OpenMixedQuiz()
        {
            int mixedListSize = 0;
            if (isMixedQuiz == false)
            {
                isMixedQuiz = true;
                curQuizType = quizType.screenshot;
                while (screenshots == null)
                    Open(null, null);
                for (int i = 0; i < screenshots.Length; i++)
                {
                    mixedQuizIndexes.Add(i);
                    mixedQuizScreenshot.Add(i);
                }
            }
            else if (isMixedQuiz)
            {
                //TODO -- na razie będzie na siłę otwierało okno wybieranie plików, do zmiany
                curQuizType = quizType.text;
                while (textQuestions == null)
                    Open(null, null);
                mixedListSize = mixedQuizIndexes.Count;
                for (int i = mixedQuizIndexes.Count; i < mixedListSize + textQuestions.Length; i++)
                {
                    mixedQuizIndexes.Add(i);
                    mixedQuizText.Add(i);
                }
                curQuizType = quizType.music;
                while (musicFilesPath == null)
                    Open(null, null);
                mixedListSize = mixedQuizIndexes.Count;
                for (int i = mixedQuizIndexes.Count; i < mixedListSize + musicFilesPath.Length; i++)
                {
                    mixedQuizIndexes.Add(i);
                    mixedQuizMusic.Add(i);
                }
                curQuizType = quizType.mixed;
                ChoosingQuestion();
            }
        }

        void StartMixedQuiz(int index)
        {
            correctAnswer.Text = "";
            if (mixedQuizScreenshot.Contains(index))
            {
                curSubType = subType.screenshot;
                for (int i = 0; i < mixedQuizScreenshot.Count; i++)
                {
                    if (mixedQuizScreenshot[i] == index)
                    {
                        mixedIndex = i;
                        break;
                    }
                }
                canvasScreenshotQuiz.Width = screenshots[mixedIndex].Width;
                canvasScreenshotQuiz.Height = screenshots[mixedIndex].Height;

                canvasScreenshotQuiz.Children.Add(screenshots[mixedIndex]);
            }
            else if (mixedQuizText.Contains(index))
            {
                curSubType = subType.text;
                mixedIndex = mixedQuizText.IndexOf(index);
                questionText.Text = textQuestions[mixedIndex];
                ShowingQuestion();
            }
            else if (mixedQuizMusic.Contains(index))
            {
                curSubType = subType.music;
                mixedIndex = mixedQuizMusic.IndexOf(index);
                mediaPlayer.Open(new Uri(musicFilesPath[mixedIndex]));
                MusicHUDShow();
            }
        }

        void ConfirmPointsMixed()
        {
            for (int i = lastQuestionIndex; i < mixedQuizIndexes.Count; i++)
                mixedQuizIndexes[i]--;
            mixedQuizIndexes.RemoveAt(lastQuestionIndex);
            for (int i = 0; i < mixedQuizScreenshot.Count; i++)
                if (mixedQuizScreenshot[i] > lastQuestionIndex)
                    mixedQuizScreenshot[i]--;
            for (int i = 0; i < mixedQuizText.Count; i++)
                if (mixedQuizText[i] > lastQuestionIndex)
                    mixedQuizText[i]--;
            for (int i = 0; i < mixedQuizMusic.Count; i++)
                if (mixedQuizMusic[i] > lastQuestionIndex)
                    mixedQuizMusic[i]--;

            if (curSubType == subType.screenshot)
            {
                screenshots[mixedIndex] = null;
                FileInfo info = new FileInfo(bitmapImage[mixedIndex].UriSource.LocalPath);
                string[] nameToPass = info.FullName.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                serializationData.answeredScreenshots.Add(nameToPass[0] + info.Extension); //Adding name of completed question

                screenshotsCompleted++;
                for (int i = mixedIndex; i < screenshots.Length - 1; i++)
                {
                    screenshots[i] = screenshots[i + 1];
                    bitmapImage[i] = bitmapImage[i + 1];
                }
                if (screenshotsCompleted == screenshots.Length)
                    mixedQuizScreenshot = new List<int>();
            }
            else if (curSubType == subType.text)
            {
                serializationText.answeredQuestions.Add(textQuestions[mixedIndex]);
                textQuestions[mixedIndex] = null; textAnswers[mixedIndex] = null; textTitles[mixedIndex] = null;
                textQuestionsCompleted++;
                for (int i = mixedIndex; i < textQuestions.Length - 1; i++)
                {
                    textQuestions[i] = textQuestions[i + 1];
                    textAnswers[i] = textAnswers[i + 1];
                    textTitles[i] = textTitles[i + 1];
                }
                if (textQuestionsCompleted == textQuestions.Length)
                    mixedQuizText = new List<int>();
            }
            else if (curSubType == subType.music)
            {
                serializationMusic.answeredQuestions.Add(musicFilesPath[mixedIndex]);
                musicFilesPath[mixedIndex] = null;
                musicQuestionsCompleted++;
                for (int i = mixedIndex; i < musicFilesPath.Length - 1; i++)
                {
                    musicFilesPath[i] = musicFilesPath[i + 1];
                }
                if (musicQuestionsCompleted == musicFilesPath.Length)
                    mixedQuizMusic = new List<int>();
            }

            if (mixedQuizMusic.Count == 0 && mixedQuizScreenshot.Count == 0 && mixedQuizText.Count == 0)
            {
                MessageBox.Show("KONIEC WIEDZÓWKI!");
                FinishedQuiz();
                finishedQuiz = true;
            }
        }

        void SaveMixedQuiz()
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Create(path + ".animemixed");
                SerializationDataMixed data = new SerializationDataMixed();

                data.answeredScreenshots = serializationData.answeredScreenshots;
                data.answeredText = serializationText.answeredQuestions;
                data.answeredMusic = serializationMusic.answeredQuestions;
                data.localizationText = serializationText.txtFileLocalization = txtFilePath;
                data.localizationScreenshot = serializationData._screenshotsLocalizationPath = screenshotsLocalizationPath;
                data.localizationMusic = serializationMusic.musicFilesLocalization = musicFilesPath;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    data.points[i] = participantsPoints[i].Text;
                    data.participants[i] = participantsNames[i].Text;
                }
                data.curAnswerType = serializationMixed.curAnswerType = (SerializationDataMixed.answerType)curAnswerType;
                data.curQuizType = serializationMixed.curQuizType = (SerializationDataMixed.quizType)curQuizType;


                bf.Serialize(stream, data);
                stream.Close();
            }
        }

        void QuickSaveMixedQuiz()
        {
            string path = quickSavePath + "quickSave-" + DateTime.Now.ToString("dd/MM/yyyy HH_mm");

            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Create(path + ".animemixed");
            SerializationDataMixed data = new SerializationDataMixed();

            data.answeredScreenshots = serializationData.answeredScreenshots;
            data.answeredText = serializationText.answeredQuestions;
            data.answeredMusic = serializationMusic.answeredQuestions;
            data.localizationText = serializationText.txtFileLocalization = txtFilePath;
            data.localizationScreenshot = serializationData._screenshotsLocalizationPath = screenshotsLocalizationPath;
            data.localizationMusic = serializationMusic.musicFilesLocalization = musicFilesPath;
            for (int i = 0; i < numberOfParticipants; i++)
            {
                data.points[i] = participantsPoints[i].Text;
                data.participants[i] = participantsNames[i].Text;
            }
            data.curAnswerType = serializationMixed.curAnswerType = (SerializationDataMixed.answerType)curAnswerType;
            data.curQuizType = serializationMixed.curQuizType = (SerializationDataMixed.quizType)curQuizType;

            bf.Serialize(stream, data);
            stream.Close();
        }

        void LoadMixedQuiz()
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Multiselect = false;
            filedialog.Filter = "AnimeMixed (*.ANIMEMIXED)|*.animemixed";

            if (filedialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = filedialog.FileName;
                QuickLoadMixedQuiz(path);
            }
        }

        void QuickLoadMixedQuiz(string path)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);
            SerializationDataMixed data = (SerializationDataMixed)bf.Deserialize(stream);

            serializationData.answeredScreenshots = data.answeredScreenshots;
            serializationText.answeredQuestions = data.answeredText;
            serializationMusic.answeredQuestions = data.answeredMusic;
            txtFilePath = serializationText.txtFileLocalization = data.localizationText;
            screenshotsLocalizationPath = serializationData._screenshotsLocalizationPath = data.localizationScreenshot;
            musicFilesPath = serializationMusic.musicFilesLocalization = data.localizationMusic;
            for (int i = 0; i < numberOfParticipants; i++)
            {
                participantsPoints[i].Text = data.points[i];
                participantsNames[i].Text = data.participants[i];
            }
            curQuizType = (quizType)data.curQuizType;
            curAnswerType = (answerType)data.curAnswerType;

            stream.Close();
            /**************** IMPORTING SCREENSHOT QUIZ DATA *************************************/
            for (int i = 0; i < data.localizationScreenshot.Length; i++)
                screenshotsLocalizationPath[i] = serializationData._screenshotsLocalizationPath[i];
            bitmapImage = new BitmapImage[screenshotsLocalizationPath.Length];
            screenshots = new Image[bitmapImage.Length];
            nameOfScreenshots = new string[bitmapImage.Length];
            for (int k = 0; k < screenshotsLocalizationPath.Length; k++)
            {
                bitmapImage[k] = new BitmapImage(new Uri(screenshotsLocalizationPath[k], UriKind.Absolute));
                screenshots[k] = new Image();
                screenshots[k].Source = bitmapImage[k];
                screenshots[k].Width = bitmapImage[k].Width;
                screenshots[k].Height = bitmapImage[k].Height;
                nameOfScreenshots[k] = screenshotsLocalizationPath[k];

                string[] correctNameOfScreenshot = nameOfScreenshots[k].Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                FileInfo info = new FileInfo(nameOfScreenshots[k]);
                nameOfScreenshots[k] = correctNameOfScreenshot[0] + info.Extension;
            }
            /**************** IMPORTING QUIZ DATA *************************************/
            txtFilePath = serializationText.txtFileLocalization;
            if (File.Exists(txtFilePath))
                OpeningTextQuiz(txtFilePath); //Loading new quiz from file
            else MessageBox.Show("Błąd importowania! Proszę zresetować program i upewnić się, że plik " + txtFilePath + " istnieje.");
            /******************* Setting MixedQuizXXX variables (lists) *****************/
            for (int i = 0; i < screenshots.Length; i++)
            {
                mixedQuizIndexes.Add(i);
                mixedQuizScreenshot.Add(i);
            }
            int mixedListSize = mixedQuizIndexes.Count;
            for (int i = mixedQuizIndexes.Count; i < mixedListSize + textQuestions.Length; i++)
            {
                mixedQuizIndexes.Add(i);
                mixedQuizText.Add(i);
            }
            mixedListSize = mixedQuizIndexes.Count;
            for (int i = mixedQuizIndexes.Count; i < mixedListSize + musicFilesPath.Length; i++)
            {
                mixedQuizIndexes.Add(i);
                mixedQuizMusic.Add(i);
            }
            /*********************** Checking for already answered questions *******************************/
            for (int i = 0; i < data.answeredScreenshots.Count; i++)
            {
                for (int j = 0; j < nameOfScreenshots.Length; j++)
                {
                    if (data.answeredScreenshots[i] == nameOfScreenshots[j])
                    {
                        mixedIndex = j;
                        lastQuestionIndex = mixedQuizScreenshot[j];
                        RemoveIndexesMixed();

                        screenshotsCompleted++;
                        screenshots[j] = null;
                        for (int k = mixedIndex; k < screenshots.Length - 1; k++)
                        {
                            screenshots[k] = screenshots[k + 1];
                            bitmapImage[k] = bitmapImage[k + 1];
                        }
                        if (screenshotsCompleted == screenshots.Length)
                            mixedQuizScreenshot = new List<int>();
                    }
                }
            }
            for (int i = 0; i < data.answeredText.Count; i++)
            {
                for (int j = 0; j < textQuestions.Length; j++)
                {
                    if (data.answeredText[i] == textQuestions[j])
                    {
                        mixedIndex = j;
                        lastQuestionIndex = mixedQuizText[j];
                        RemoveIndexesMixed();

                        textQuestionsCompleted++;
                        textQuestions[j] = null; textTitles[j] = null; textAnswers[j] = null;
                        for (int k = mixedIndex; k < textQuestions.Length - 1; k++)
                        {
                            textQuestions[k] = textQuestions[k + 1];
                            textAnswers[k] = textAnswers[k + 1];
                            textTitles[k] = textTitles[k + 1];
                        }
                        if (textQuestionsCompleted == textQuestions.Length)
                            mixedQuizText = new List<int>();
                    }
                }
            }
            for (int i = 0; i < data.answeredMusic.Count; i++)
            {
                for (int j = 0; j < musicFilesPath.Length; j++)
                {
                    if (data.answeredMusic[i] == musicFilesPath[j])
                    {
                        mixedIndex = j;
                        lastQuestionIndex = mixedQuizMusic[j];
                        RemoveIndexesMixed();

                        musicQuestionsCompleted++;
                        musicFilesPath[j] = null;
                        for (int k = mixedIndex; k < musicFilesPath.Length - 1; k++)
                        {
                            musicFilesPath[k] = musicFilesPath[k + 1];
                        }
                        if (musicQuestionsCompleted == musicFilesPath.Length)
                            mixedQuizMusic = new List<int>();
                    }
                }
            }
            if (data.answeredMusic != null)
                mixedQuizIndexes.RemoveAt(mixedQuizIndexes.Count - 1);
        }

        void RemoveIndexesMixed()
        {
            for (int i = lastQuestionIndex; i < mixedQuizIndexes.Count; i++)
                mixedQuizIndexes[i]--;
            mixedQuizIndexes.RemoveAt(lastQuestionIndex);
            for (int i = 0; i < mixedQuizScreenshot.Count; i++)
                if (mixedQuizScreenshot[i] > lastQuestionIndex)
                    mixedQuizScreenshot[i]--;
            for (int i = 0; i < mixedQuizText.Count; i++)
                if (mixedQuizText[i] > lastQuestionIndex)
                    mixedQuizText[i]--;
            for (int i = 0; i < mixedQuizMusic.Count; i++)
                if (mixedQuizMusic[i] > lastQuestionIndex)
                    mixedQuizMusic[i]--;
        }
        //////////////////////////////////////////////////////////////////////
        /*************** PARTICIPANTS REGION ********************************/
        void InitializingParticipants()
        {
            for (int i = 0; i < participantsNames.Length; i++)
            {
                participantsNames[i] = null;
                participantsPoints[i] = null;
                participantsPlus[i] = null;
                participantsMinus[i] = null;
            }
            participantsNames[0] = nameParticipant1;
            participantsPoints[0] = pointsParticipant1;
            participantsPlus[0] = plusParticipant1;
            participantsMinus[0] = minusParticipant1;

participantsNames[1] = nameParticipant2; participantsPoints[1] = pointsParticipant2; participantsPlus[1] = plusParticipant2; participantsMinus[1] = minusParticipant2;
participantsNames[2] = nameParticipant3; participantsPoints[2] = pointsParticipant3; participantsPlus[2] = plusParticipant3; participantsMinus[2] = minusParticipant3;
participantsNames[3] = nameParticipant4; participantsPoints[3] = pointsParticipant4; participantsPlus[3] = plusParticipant4; participantsMinus[3] = minusParticipant4;
participantsNames[4] = nameParticipant5; participantsPoints[4] = pointsParticipant5; participantsPlus[4] = plusParticipant5; participantsMinus[4] = minusParticipant5;
participantsNames[5] = nameParticipant6; participantsPoints[5] = pointsParticipant6; participantsPlus[5] = plusParticipant6; participantsMinus[5] = minusParticipant6;
participantsNames[6] = nameParticipant7; participantsPoints[6] = pointsParticipant7; participantsPlus[6] = plusParticipant7; participantsMinus[6] = minusParticipant7;
participantsNames[7] = nameParticipant8; participantsPoints[7] = pointsParticipant8; participantsPlus[7] = plusParticipant8; participantsMinus[7] = minusParticipant8;
participantsNames[8] = nameParticipant9; participantsPoints[8] = pointsParticipant9; participantsPlus[8] = plusParticipant9; participantsMinus[8] = minusParticipant9;
participantsNames[9] = nameParticipant10; participantsPoints[9] = pointsParticipant10;participantsPlus[9] = plusParticipant10; participantsMinus[9] = minusParticipant10;
participantsNames[10] = nameParticipant11; participantsPoints[10] = pointsParticipant11;participantsPlus[10] = plusParticipant11;participantsMinus[10]=minusParticipant11;
participantsNames[11] = nameParticipant12;participantsPoints[11] = pointsParticipant12;participantsPlus[11] = plusParticipant12;participantsMinus[11]=minusParticipant12;

            for (int i = 0; i < participantsNames.Length; i++)
            {
                if (participantsNames[i] != null)
                {
                    participantsNames[i].Text = "";
                    participantsPoints[i].Text = "0,0";
                }
            }
        }

        void FinishedQuiz()
        {
            SwitchingOffAllFields();
            GivingPoints_ShowingAnswers();
            confirmPoints.Width = 0;
            goBack.Width = 0;

            int[] pointsList = new int[numberOfParticipants];

            for (int i = 0; i < numberOfParticipants; i++)
            {
                if (participantsNames[i].Text != "" && participantsNames[i].Text != null)
                {
                    string points = participantsPoints[i].Text.Replace(",0", "");
                    pointsList[i] = int.Parse(points);
                }
            }
            Array.Sort(pointsList);
            Array.Reverse(pointsList);
            int currentPositionOfWinners = 1;
            for (int i = 0; i < numberOfParticipants; i++)
            {
                for (int j = 0; j < numberOfParticipants; j++)
                {
                    string points = participantsPoints[j].Text.Replace(",0", "");
                    if (pointsList[i] == int.Parse(points) && participantsNames[j].Text != null && participantsNames[j].Text != "")
                    {
                        winners.Text += currentPositionOfWinners.ToString() + ". " + participantsNames[j].Text + " ; " + points + "\r";
                        currentPositionOfWinners++;
                    }
                }
            }
            MessageBox.Show(winners.Text);
        }
        #endregion
    }
}