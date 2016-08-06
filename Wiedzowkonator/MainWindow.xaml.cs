﻿using System;
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
using System.Windows.Resources;
//
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Image = System.Windows.Controls.Image;
using Button = System.Windows.Controls.Button;
using Brush = System.Drawing.Brush;

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
        System.Windows.Controls.Button[] participantsBonus = new Button[16];
        System.Windows.Controls.Canvas[,] participantsIcons = new Canvas[16, 4];
        System.Windows.Controls.Canvas[,] participantsModifiers = new Canvas[16, 4];
        /************************/
        bool swappingWithFirstPlace;

        public Image[] screenshots; //Screenshots that will be shown on canvas
        BitmapImage[] bitmapImage; //Images that will be written into "screenshots" variable
        public string[] nameOfScreenshots; //Name of all screenshots - it is used to check which one was already shown
        public bool screenshotQuizStarted; //Checking if quiz has started - switching between choosing and answering phase
        int lastScreenshotIndex; //Getting index of last shown screenshot in case if user want to see it once again
        int screenshotsCompleted; //If user already answered this screenshot it won't be shown again
        static string userName = Environment.UserName; //Name of user logged on Windows account
        static string[] systemDirectoryChunks = Environment.SystemDirectory.ToString().Split('\\'); //Getting system Windows disk (Mostly C:/)
        string quickSavePath = systemDirectoryChunks[0] + "/Users/" + userName + "/AppData/LocalLow/Wiedzowkonator/"; //Choosing directory path
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
        /**************************/
        /****** LOTTERY QUIZ ******/
        int curLotteryBonus;
        int lotteryPointsToPass;
        //Image[] lotteryIcons;
        //BitmapImage[] lotteryBitmaps;
        string lotteryIconsDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "/Icons";
        //Lottery buffs / debuffs
        bool[] lForced = new bool[12];
        bool[] lBlocked = new bool[12];
        bool[] lDouble = new bool[12];
        bool[] lNotLoosing = new bool[12];
        int[] usersModifiers = new int[16];
        int[] lForcedLifeSpan = new int[12];
        int[] lBlockedLifeSpan = new int[12];

        //Enum type that shows which state of quiz is currently in progress
        enum quizState { customizingQuestions, choosingQuestion, answeringQuestion, givingPoints };
        quizState curQuizState;

        enum answerType { noAnswer, fileNameAnswer, customAnswer };
        answerType curAnswerType;

        enum quizType { screenshot, text, music, mixed };
        quizType curQuizType;

        enum pointsType { manual, lottery, tilesChoosing};
        pointsType curPointsType;

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
                    curQuizState = quizState.answeringQuestion;
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

        void ShowingHUD() //TODO - it's a bit old. I should get rid of it
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
                BonusesText.Width = 0;
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        participantsIcons[i, k].Children.Clear();
                        participantsModifiers[i, k].Children.Clear();
                    }
                }

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

        private void bonusParticipant_Click(object sender, RoutedEventArgs e)
        {
            Button button = (sender as Button);

            string indexOfParticipantString = button.Name.Replace("bonusParticipant", "");
            int indexOfParticipant = int.Parse(indexOfParticipantString) - 1;
            string pPoints;
            switch (curLotteryBonus)
            {

                case 0: //Adding points for participant
                    pPoints = participantsPoints[indexOfParticipant].Text.Replace(",0", "");

                    if (lDouble[indexOfParticipant] == true)
                    {
                        participantsPoints[indexOfParticipant].Text = (int.Parse(pPoints) + (int.Parse(lotteryPointsToPass.ToString())) * 2).ToString() + ",0";
                        lDouble[indexOfParticipant] = false;
                        usersModifiers[indexOfParticipant]--;
                    }
                    else participantsPoints[indexOfParticipant].Text = (int.Parse(pPoints) + int.Parse(lotteryPointsToPass.ToString())).ToString() + ",0";
                    break;

                case 1: //Substracting points for participant
                    pPoints = participantsPoints[indexOfParticipant].Text.Replace(",0", "");

                    if (lNotLoosing[indexOfParticipant] == true)
                    {
                        lNotLoosing[indexOfParticipant] = false;
                        usersModifiers[indexOfParticipant]--;
                    }
                    else
                        participantsPoints[indexOfParticipant].Text = (int.Parse(pPoints) - int.Parse(lotteryPointsToPass.ToString())).ToString() + ",0";
                    break;

                case 2: //Swapping points with first place
                    string participantToSwap = participantsNames[indexOfParticipant].Text; //Getting participant that answered question
                    List<int> firstPlaces = new List<int>();
                    int[] pointsList = new int[numberOfParticipants]; //Initializing array length with number of participants
                    int indexOfParticipantToSwap = 0; //Initialize variable that will pass index of needed participant

                    for (int i = 0; i < numberOfParticipants; i++) //Getting number of points to fill an array
                    {
                        if (participantsNames[i].Text != "" && participantsNames[i].Text != null) //If there is name of team/user written in TextBox
                        {
                            string points = participantsPoints[i].Text.Replace(",0", "");
                            pointsList[i] = int.Parse(points); //Adding points to array
                        }
                    }
                    Array.Sort(pointsList); //Sorting by points
                    if (swappingWithFirstPlace) //Reversing points list if changing 1st with other participant
                        Array.Reverse(pointsList); //Now the highest score has [0] index

                    for (int j = 0; j < numberOfParticipants; j++) //Getting name of participant written in TextBox
                    {
                        if (participantToSwap == participantsNames[j].Text && participantsNames[j].Text != null && participantsNames[j].Text != "")
                        {
                            indexOfParticipantToSwap = j; //Getting index of participant chosen by user
                        }
                    }

                    for (int i = 0; i < numberOfParticipants; i++)
                    {
                        if (pointsList[0] == pointsList[i])
                        {
                            firstPlaces.Add(pointsList[i]);
                        }
                    }
                    string chosenParticipantPoints = "";
                    for (int k = 0; k < firstPlaces.Count; k++)
                    {
                        for (int j = 0; j < numberOfParticipants; j++) //Getting name of participant with most points
                        {
                            string points = participantsPoints[j].Text.Replace(",0", ""); //Initialize points of currently checked user
                                                                                          //If this user has exact amount of points then if statement is true
                            if (pointsList[0] == int.Parse(points) && participantsNames[j].Text != null && participantsNames[j].Text != "")
                            {
                                string tempPoints = participantsPoints[j].Text; //Participant with the highest score (1st place)  
                                if (chosenParticipantPoints == "")
                                    chosenParticipantPoints = participantsPoints[indexOfParticipantToSwap].Text;

                                participantsPoints[j].Text = chosenParticipantPoints; //Participant with the highest score is getting chosen one points
                                participantsPoints[indexOfParticipantToSwap].Text = tempPoints; //Chosen participant is getting the highest score (1st place)

                            }
                        }
                    }
                    break;
                case 3: //Switching with last place
                    participantToSwap = participantsNames[indexOfParticipant].Text; //Getting participant that answered question
                    List<int> lastPlaces = new List<int>();
                    List<int> pointsIntList = new List<int>(); //Initializing array length with number of participants
                    indexOfParticipantToSwap = 0; //Initialize variable that will pass index of needed participant
                    int newPointsListLength = 0;

                    for (int i = 0; i < numberOfParticipants; i++) //Getting number of points to fill an array
                    {
                        if (participantsNames[i].Text != "" && participantsNames[i].Text != null) //If there is name of team/user written in TextBox
                        {
                            string points = participantsPoints[i].Text.Replace(",0", "");
                            pointsIntList.Add(int.Parse(points)); //Adding points to array
                            newPointsListLength++;
                        }
                    }

                    for (int j = 0; j < numberOfParticipants; j++) //Getting name of participant written in TextBox
                    {
                        if (participantToSwap == participantsNames[j].Text && participantsNames[j].Text != null && participantsNames[j].Text != "")
                        {
                            indexOfParticipantToSwap = j; //Getting index of participant chosen by user
                        }
                    }

                    pointsList = new int[newPointsListLength];
                    for (int k = 0; k < pointsIntList.Count; k++)
                    {
                        pointsList[k] = pointsIntList[k];
                    }
                    Array.Sort(pointsList);

                    for (int i = 0; i < pointsList.Length; i++)
                    {
                        if (pointsList[0] == pointsList[i])
                        {
                            lastPlaces.Add(pointsList[i]);
                        }
                    }
                    string chosenParticipantPointsLowest = "";
                    for (int k = 0; k < lastPlaces.Count; k++)
                    {
                        for (int j = 0; j < numberOfParticipants; j++) //Getting name of participant with least points
                        {
                            string points = participantsPoints[j].Text.Replace(",0", ""); //Initialize points of currently checked user
                                                                                          //If this user has exact amount of points then if statement is true
                            if (pointsList[0] == int.Parse(points) && participantsNames[j].Text != null && participantsNames[j].Text != "")
                            {
                                string tempPoints = participantsPoints[j].Text; //Participant with the lowest score (last place)  
                                if (chosenParticipantPointsLowest == "")
                                    chosenParticipantPointsLowest = participantsPoints[indexOfParticipantToSwap].Text;

                                participantsPoints[j].Text = chosenParticipantPointsLowest; //Participant with the lowest score is getting chosen one points
                                participantsPoints[indexOfParticipantToSwap].Text = tempPoints; //Chosen participant is getting the lowest score (last place)

                            }
                        }
                    }
                    break;
                case 4: //User is forced to take over questions
                    Image forcedImage = new Image();
                    BitmapImage forcedBitmap = new BitmapImage(new Uri("Icons/forced.png", UriKind.Relative));
                    forcedImage.Source = forcedBitmap;
                    forcedImage.Width = forcedBitmap.Width;
                    forcedImage.Height = forcedBitmap.Height;

                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].Children.Add(forcedImage);
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].ToolTip = "Przez następne trzy rundy, jeśli nikt nie odpowie na pytanie, musisz je przejąć";
                    lForced[indexOfParticipant] = true;
                    lForcedLifeSpan[indexOfParticipant] = 3;
                    usersModifiers[indexOfParticipant]++;
                    break;
                case 5: //For next question, user is gaining double points
                    Image doubleImage = new Image();
                    BitmapImage doubleBitmap = new BitmapImage(new Uri("Icons/doublePoints.png", UriKind.Relative));
                    doubleImage.Source = doubleBitmap;
                    doubleImage.Width = doubleBitmap.Width;
                    doubleImage.Height = doubleBitmap.Height;

                    lDouble[indexOfParticipant] = true;
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].Children.Add(doubleImage);
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].ToolTip = "Za następne pytanie na które odpowiesz otrzymujesz 2x punktów";
                    usersModifiers[indexOfParticipant]++;
                    break;
                case 6: //You can't answer other participant's questions for three rounds
                    Image blockedImage = new Image();
                    BitmapImage blockedBitmap = new BitmapImage(new Uri("Icons/blocked.png", UriKind.Relative));
                    blockedImage.Source = blockedBitmap;
                    blockedImage.Width = blockedBitmap.Width;
                    blockedImage.Height = blockedBitmap.Height;

                    lBlocked[indexOfParticipant] = true;
                    lBlockedLifeSpan[indexOfParticipant] = 3;
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].Children.Add(blockedImage);
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].ToolTip = "Przez trzy tury nie możesz przejmować pytań";
                    usersModifiers[indexOfParticipant]++;
                    break;
                case 7: //You're not loosing points for next unfortunate question take over
                    Image loseImage = new Image();
                    BitmapImage loseBitmap = new BitmapImage(new Uri("Icons/noLosingPoints.png", UriKind.Relative));
                    loseImage.Source = loseBitmap;
                    loseImage.Width = loseBitmap.Width;
                    loseImage.Height = loseBitmap.Height;

                    lNotLoosing[indexOfParticipant] = true;
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].Children.Add(loseImage);
                    participantsIcons[indexOfParticipant, usersModifiers[indexOfParticipant]].ToolTip = "Podczas następnego nieudanego przejęcia nie tracisz punktów";
                    usersModifiers[indexOfParticipant]++;
                    break;
            }
        }

        //If user is done, then he leaves giving points phase and go back to choosing question
        private void confirmPoints_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.choosingQuestion;
            ShowingHUD();
            if (curPointsType == pointsType.lottery)
            {
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    if (lForcedLifeSpan[i] >= 0 && lForced[i] == true)
                    {
                        lForcedLifeSpan[i]--;
                        if (lForcedLifeSpan[i] < 0)
                        {
                            lForcedLifeSpan[i] = 0;
                            lForced[i] = false;
                            usersModifiers[i]--;
                        }
                    }
                    if (lBlockedLifeSpan[i] >= 0 && lBlocked[i] == true)
                    {
                        lBlockedLifeSpan[i]--;
                        if (lBlockedLifeSpan[i] < 0)
                        {
                            lBlockedLifeSpan[i] = 0;
                            lBlocked[i] = false;
                            usersModifiers[i]--;
                        }
                    }
                }
            }

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
        #region Skipping Questions
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
            if (e.Key == Key.Enter && curQuizState != quizState.customizingQuestions &&
                (curQuizType == quizType.screenshot || (curQuizType == quizType.mixed && curSubType == subType.screenshot)))
            {
                MessageBox.Show("OK");
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
            if (curQuizState != quizState.customizingQuestions)
            {
                curQuizState = quizState.givingPoints;
                StartClick(null, null);
                mediaPlayer.Stop();
                SwitchingOffAllFields();
                GivingPoints_ShowingAnswers();
                if (curAnswerType == answerType.customAnswer)
                    CustomMusicAnswersShowing();
                else if (curAnswerType == answerType.fileNameAnswer)
                {
                    artistNameMusicAnswer.Width = 400;
                    artistNameMusicAnswer.Height = 50;
                    FileInfo fileInfo = new FileInfo(musicFilesPath[lastQuestionIndex]);
                    artistNameMusicAnswer.Text = fileInfo.Name.Replace(fileInfo.Extension, "");
                }
                else if (curAnswerType == answerType.noAnswer)
                    correctAnswer.Text = ""; //TODO -- wstawić tutaj odpowiedź na pytanie muzyczne
            }
            else
            {
                customAnswerAdding_Button_or_Enter();
            }
        }
        #endregion
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
                    string[] musicFilesPathToPass = new string[fileDialog.FileNames.Length];
                    musicFilesPath = new string[musicFilesPathToPass.Length];
                    serializationMusic.musicFilesLocalization = new string[musicFilesPath.Length];
                    for (int i = 0; i < musicFilesPath.Length; i++)
                    {
                        musicFilesPathToPass[i] = fileDialog.FileNames[i];
                        serializationMusic.musicFilesLocalization[i] = musicFilesPathToPass[i];
                    }
                    Random random = new Random();
                    List<int> indexes = new List<int>();
                    for (int i = 0; i < musicFilesPath.Length; i++)
                        indexes.Add(i);

                    for (int i = 0; i < musicFilesPath.Length; i++)
                    {
                        int toDelete = random.Next(0, indexes.Count);
                        int toPass = indexes[toDelete];
                        musicFilesPath[toPass] = musicFilesPathToPass[i];
                        indexes.RemoveAt(toDelete);
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

            if (pathToCheck.Length > 0)
            {
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
                                textQuestions[j] = null; textTitles[j] = null; textAnswers[j] = null;
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
                                musicFilesPath[j] = null;
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
            else //If there in no files in quicksave/quickload directory
            {
                MessageBox.Show("Brak plików w folderze szybkiego zapisu. Oznacza to, że nie utworzono do tej pory"
                    + "żadnej wiedzówki. Aby to zrobić kliknij \"Utwórz nową wiedzówkę...\"");
            }
        }
        #endregion
        /* Importing new quizes */
        #region Importing new quizes (screenshots & music)
        private void customAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            curAnswerType = answerType.customAnswer;
            SwitchingOffAllFields(); //Clearing all currently open fields to width = 0
            if (curQuizType == quizType.screenshot)
            {
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
                CustomScreenshotAnswersAdding(); //Showing fields to add custom answers
                File.Create(quickSavePath + "toDelete.txt").Close();
            }
            else if (curQuizType == quizType.music)
            {//TODO -- EDIT
                CustomMusicAnswersAdding();
                serializationMusic.curAnswerType = SerializationDataMusic.answerType.customAnswer;
                customAnswerButton.Width = fileNameAnswerButton.Width = noAnswerButton.Width = 0;
                customAnswerIndex = 0; //Initializing index position with 0
                FillingCustomMusicQuestions(customAnswerIndex);

                File.Create(quickSavePath + "toDelete.txt").Close();
                MusicHUDShow();
                mediaPlayer.Open(new Uri(musicFilesPath[customAnswerIndex]));
            }
        }

        private void fileNameAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            curAnswerType = answerType.fileNameAnswer;
            if (curQuizType == quizType.screenshot || (curQuizType == quizType.mixed && curSubType == subType.screenshot))
                serializationData.curAnswerType = SerializationData.answerType.fileNameAnswer;
            else if (curQuizType == quizType.music)
                serializationMusic.curAnswerType = SerializationDataMusic.answerType.fileNameAnswer;
            customAnswerButton.Width = fileNameAnswerButton.Width = noAnswerButton.Width = 0;
            QuizHUDAfterImporting(true);
            curQuizState = quizState.choosingQuestion;

            SwitchingOffAllFields();
            if (isMixedQuiz)
                OpenMixedQuiz();
            else
                ChoosingPointsType();
        }

        private void noAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            curAnswerType = answerType.noAnswer;
            if (curQuizType == quizType.screenshot || (curQuizType == quizType.mixed && curSubType == subType.screenshot))
                serializationData.curAnswerType = SerializationData.answerType.fileNameAnswer;
            else if (curQuizType == quizType.music)
                serializationMusic.curAnswerType = SerializationDataMusic.answerType.fileNameAnswer;
            customAnswerButton.Width = fileNameAnswerButton.Width = noAnswerButton.Width = 0;
            QuizHUDAfterImporting(true);
            curQuizState = quizState.choosingQuestion;

            SwitchingOffAllFields();
            if (isMixedQuiz)
                OpenMixedQuiz();
            else
                ChoosingPointsType();
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
                if (curQuizType == quizType.screenshot || (curQuizType == quizType.mixed && curSubType == subType.screenshot))
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
                        if (isMixedQuiz)
                            OpenMixedQuiz();
                        else
                            ChoosingPointsType();
                    }
                }
                else if (curQuizType == quizType.music)
                {
                    if (artistNameMusic.Text == "" && animeNameMusic.Text == "" && titleMusic.Text == "")
                        MessageBox.Show("Uwaga! Zostawiasz wszystkie puste odpowiedzi.");
                    FileInfo infoToPass = new FileInfo(musicFilesPath[customAnswerIndex]);
                    string newPath;
                    MessageBox.Show(infoToPass.Name);

                    //When file doesn't contain correct answer in its name
                    if (!infoToPass.Name.Contains("@artist_") && !infoToPass.Name.Contains("@title_") && !infoToPass.Name.Contains("@anime_name_")) 
                    {
                        newPath = infoToPass.FullName.Replace(infoToPass.Extension, "") + "@artist_" + artistNameMusic.Text + "@title_" + titleMusic.Text
                            + "@anime_name_" + animeNameMusic.Text + infoToPass.Extension;
                    }
                    else
                    {
                        string[] pathPart = infoToPass.FullName.Split(new[] { "@artist_" }, StringSplitOptions.None);
                        newPath = pathPart[0] + "@artist_" + artistNameMusic.Text + "@title_" + titleMusic.Text
                            + "@anime_name_" + animeNameMusic.Text + infoToPass.Extension;

                        MessageBox.Show(animeNameMusic.Text);
                    }
                    if (!File.Exists(newPath))
                    {
                        File.Copy(infoToPass.FullName, newPath);

                        musicFilesPath[customAnswerIndex] = newPath;

                        using (StreamWriter sw = File.AppendText(quickSavePath + "toDelete.txt"))
                            sw.WriteLine(infoToPass.FullName);
                    }
                    customAnswerIndex++;
                    if (customAnswerIndex < musicFilesPath.Length)
                    {
                        //SwitchingOffAllFields();
                        artistNameMusic.Text = "";
                        animeNameMusic.Text = "";
                        titleMusic.Text = "";
                        FillingCustomMusicQuestions(customAnswerIndex);
                    }
                    else
                    {
                        artistNameMusic.Text = "";
                        animeNameMusic.Text = "";
                        titleMusic.Text = "";
                        QuizHUDAfterImporting(true);
                        curQuizState = quizState.choosingQuestion;

                        SwitchingOffAllFields();
                        if (isMixedQuiz)
                            OpenMixedQuiz();
                        else
                            ChoosingPointsType();
                    }

                }
            }
        }

        void FillingCustomMusicQuestions(int customAnswerIndex)
        {
            FileInfo file = new FileInfo(musicFilesPath[customAnswerIndex]);
            string fileName;

            string[] correctAnswerToPass = new string[2];
            if (file.Name.Contains("@anime_name_"))
            {
                fileName = file.Name.Replace(file.Extension, "");
                correctAnswerToPass = fileName.Split(new[] { "@anime_name_" }, StringSplitOptions.None);
                animeNameMusic.Text = correctAnswerToPass[1];
            }
            if (file.Name.Contains("@title_"))
            {
                if (file.Name.Contains("@anime_name_"))
                    correctAnswerToPass = correctAnswerToPass[0].Split(new[] { "@title_" }, StringSplitOptions.None);
                else
                {
                    fileName = file.Name.Replace(file.Extension, "");
                    correctAnswerToPass = fileName.Split(new[] { "@title_" }, StringSplitOptions.None);
                }
                titleMusic.Text = correctAnswerToPass[1];
            }
            if (file.Name.Contains("@artist_"))
            {
                if (file.Name.Contains("@anime_name_") || file.Name.Contains("@title_"))
                    correctAnswerToPass = correctAnswerToPass[0].Split(new[] { "@artist_" }, StringSplitOptions.None);
                else
                {
                    fileName = file.Name.Replace(file.Extension, "");
                    correctAnswerToPass = fileName.Split(new[] { "@arist_" }, StringSplitOptions.None);
                }
                artistNameMusic.Text = correctAnswerToPass[1];
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
        #endregion

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
                Random random = new Random();
                List<int> indexes = new List<int>();
                string[] textTitlesToPass = new string[textTitles.Length];
                string[] textQuestionsToPass = new string[textTitles.Length];
                string[] textAnswersToPass = new string[textTitles.Length];
                for (int i = 0; i < textTitles.Length; i++)
                {
                    indexes.Add(i);
                    textTitlesToPass[i] = textTitles[i];
                    textQuestionsToPass[i] = textQuestions[i];
                    textAnswersToPass[i] = textAnswers[i];
                }

                for (int i = 0; i < textTitles.Length; i++)
                {
                    int toDelete = random.Next(0, indexes.Count);
                    int toPass = indexes[toDelete];
                    textTitles[toPass] = textTitlesToPass[i];
                    textQuestions[toPass] = textQuestionsToPass[i];
                    textAnswers[toPass] = textAnswersToPass[i];
                    indexes.RemoveAt(toDelete);
                }
            }
        }

        #region HUD
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
                participantsBonus[i].Width = 0;
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
            //Choosing type of assigning points
            manualQuizButton.Width = 0;
            lotteryQuizButton.Width = 0;
            tilesChoosingQuizButton.Width = 0;

            LotteryText.Width = 0;
            BonusesText.Width = 0;
            BonusesText.Text = "";
            //Customizing questions for music type
            artistNameMusic.Width = 0;
            titleMusic.Width = 0;
            animeNameMusic.Width = 0;
            //Getting answer for music type questions
            artistNameMusicAnswer.Width = 0;
            titleMusicAnswer.Width = 0;
            animeNameMusicAnswer.Width = 0;
            for (int i = 0; i < numberOfParticipants; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    participantsIcons[i, k].Children.Clear();
                    participantsModifiers[i, k].Children.Clear();
                }
            }
        }

        void ChoosingPointsType()
        {
            manualQuizButton.Width = 250;
            lotteryQuizButton.Width = 250;
            tilesChoosingQuizButton.Width = 250;
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
            BonusesText.Width = 250;
            int participantIndex = 0;
            BonusesText.Text = "Obecne modyfikatory:";
            for (int i = 0; i < numberOfParticipants; i++)
            {
                int index = 0;
                if (lForced[i] == true)
                {
                    Image forcedImage = new Image();
                    BitmapImage forcedBitmap = new BitmapImage(new Uri("Icons/forced.png", UriKind.Relative));
                    forcedImage.Source = forcedBitmap;
                    forcedImage.Width = forcedBitmap.Width;
                    forcedImage.Height = forcedBitmap.Height;

                    participantsModifiers[participantIndex, index].Children.Add(forcedImage);
                    participantsModifiers[participantIndex, index].ToolTip = "Przez następne trzy rundy, jeśli nikt nie odpowie na pytanie, musisz je przejąć";
                    index++;
                }
                if (lDouble[i] == true)
                {
                    Image doubleImage = new Image();
                    BitmapImage doubleBitmap = new BitmapImage(new Uri("Icons/doublePoints.png", UriKind.Relative));
                    doubleImage.Source = doubleBitmap;
                    doubleImage.Width = doubleBitmap.Width;
                    doubleImage.Height = doubleBitmap.Height;

                    participantsModifiers[participantIndex, index].Children.Add(doubleImage);
                    participantsModifiers[participantIndex, index].ToolTip = "Za następne pytanie na które odpowiesz otrzymujesz 2x punktów";
                    index++;
                }
                if (lBlocked[i] == true)
                {
                    Image blockedImage = new Image();
                    BitmapImage blockedBitmap = new BitmapImage(new Uri("Icons/blocked.png", UriKind.Relative));
                    blockedImage.Source = blockedBitmap;
                    blockedImage.Width = blockedBitmap.Width;
                    blockedImage.Height = blockedBitmap.Height;

                    participantsModifiers[participantIndex, index].Children.Add(blockedImage);
                    participantsModifiers[participantIndex, index].ToolTip = "Przez trzy tury nie możesz przejmować pytań";
                    index++;
                }
                if (lNotLoosing[i] == true)
                {
                    Image loseImage = new Image();
                    BitmapImage loseBitmap = new BitmapImage(new Uri("Icons/noLosingPoints.png", UriKind.Relative));
                    loseImage.Source = loseBitmap;
                    loseImage.Width = loseBitmap.Width;
                    loseImage.Height = loseBitmap.Height;

                    participantsModifiers[participantIndex, index].Children.Add(loseImage);
                    participantsModifiers[participantIndex, index].ToolTip = "Podczas następnego nieudanego przejęcia nie tracisz punktów";
                }
                if (lBlocked[i] || lNotLoosing[i] || lDouble[i] || lForced[i])
                {
                    participantIndex++;
                    BonusesText.Text = BonusesText.Text + Environment.NewLine;
                    for (int k = 0; k < index; k++)
                    {
                        BonusesText.Text = BonusesText.Text + "       ";
                    }
                    BonusesText.Text = BonusesText.Text + participantsNames[i].Text;
                }
            }
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
                if (curPointsType == pointsType.manual)
                {
                    participantsPlus[i].Width = 33;
                    participantsMinus[i].Width = 33;
                }
                else if (curPointsType == pointsType.lottery)
                {
                    //TODO for testing
                    participantsPlus[i].Width = 33;
                    participantsMinus[i].Width = 33;
                    participantsBonus[i].Width = 30;
                    int index = 0;
                    if (lForced[i] == true)
                    {
                        Image forcedImage = new Image();
                        BitmapImage forcedBitmap = new BitmapImage(new Uri("Icons/forced.png", UriKind.Relative));
                        forcedImage.Source = forcedBitmap;
                        forcedImage.Width = forcedBitmap.Width;
                        forcedImage.Height = forcedBitmap.Height;

                        participantsIcons[i, index].Children.Add(forcedImage);
                        participantsIcons[i, index].ToolTip = "Przez następne trzy rundy, jeśli nikt nie odpowie na pytanie, musisz je przejąć";
                        index++;
                    }
                    if (lDouble[i] == true)
                    {
                        Image doubleImage = new Image();
                        BitmapImage doubleBitmap = new BitmapImage(new Uri("Icons/doublePoints.png", UriKind.Relative));
                        doubleImage.Source = doubleBitmap;
                        doubleImage.Width = doubleBitmap.Width;
                        doubleImage.Height = doubleBitmap.Height;

                        participantsIcons[i, index].Children.Add(doubleImage);
                        participantsIcons[i, index].ToolTip = "Za następne pytanie na które odpowiesz otrzymujesz 2x punktów";
                        index++;
                    }
                    if (lBlocked[i] == true)
                    {
                        Image blockedImage = new Image();
                        BitmapImage blockedBitmap = new BitmapImage(new Uri("Icons/blocked.png", UriKind.Relative));
                        blockedImage.Source = blockedBitmap;
                        blockedImage.Width = blockedBitmap.Width;
                        blockedImage.Height = blockedBitmap.Height;

                        participantsIcons[i, index].Children.Add(blockedImage);
                        participantsIcons[i, index].ToolTip = "Przez trzy tury nie możesz przejmować pytań";
                        index++;
                    }
                    if (lNotLoosing[i] == true)
                    {
                        Image loseImage = new Image();
                        BitmapImage loseBitmap = new BitmapImage(new Uri("Icons/noLosingPoints.png", UriKind.Relative));
                        loseImage.Source = loseBitmap;
                        loseImage.Width = loseBitmap.Width;
                        loseImage.Height = loseBitmap.Height;

                        participantsIcons[i, index].Children.Add(loseImage);
                        participantsIcons[i, index].ToolTip = "Podczas następnego nieudanego przejęcia nie tracisz punktów";
                    }
                }
            }

            if (curPointsType == pointsType.lottery)
            {
                StartLottery();
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

        void CustomMusicAnswersAdding()
        {
            //Customizing questions for music type
            artistNameMusic.Width = 250;
            titleMusic.Width = 250;
            animeNameMusic.Width = 250;
        }

        void CustomMusicAnswersShowing()
        {
            //Getting answer for music type questions
            artistNameMusicAnswer.Width = 250;
            titleMusicAnswer.Width = 250;
            animeNameMusicAnswer.Width = 250;

            FillingCustomMusicQuestions(lastQuestionIndex);
            artistNameMusicAnswer.Text = "Wykonawca: " + artistNameMusic.Text.First().ToString().ToUpper() + artistNameMusic.Text.Substring(1);
            animeNameMusicAnswer.Text = "Anime: " + animeNameMusic.Text.First().ToString().ToUpper() + animeNameMusic.Text.Substring(1); 
            titleMusicAnswer.Text = "Tytuł utworu: " + titleMusic.Text.First().ToString().ToUpper() + titleMusic.Text.Substring(1);

            titleMusic.Text = ""; titleMusic.Width = 0;
            animeNameMusic.Text = ""; animeNameMusic.Width = 0;
            artistNameMusic.Text = ""; artistNameMusic.Width = 0;
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
        /**************************** END OF HUD METHODS *****************************/
        #endregion
        /*****************************************************************************/
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
                if (curQuizType == quizType.screenshot || curQuizType == quizType.music)
                {
                    SwitchingOffAllFields();
                    CustomScreenshotAnswers();
                }
                else
                {
                    SwitchingOffAllFields();
                    ChoosingPointsType();
                }
            }
        }

        private void loadQuizButton_Click(object sender, RoutedEventArgs e)
        {
            Load(null, null);
            if (textQuestions != null || screenshots != null || musicFilesPath != null)
            {
                SwitchingOffAllFields();
                ChoosingPointsType();
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
                ChoosingPointsType();
            }
        }

        private void manualQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curPointsType = pointsType.manual;
            SwitchingOffAllFields();
            ChoosingQuestion();
        }

        private void lotteryQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curPointsType = pointsType.lottery;
            SwitchingOffAllFields();
            ChoosingQuestion();
        }

        private void tilesChoosingQuizButton_Click(object sender, RoutedEventArgs e)
        {
            curPointsType = pointsType.tilesChoosing;
            SwitchingOffAllFields();
            ChoosingQuestion();
        }

        void StartLottery()
        {
            LotteryText.Width = 350;
            string[] lotteryVariants = new string[8];
            lotteryVariants[0] = "Dostajesz.punkty";
            lotteryVariants[1] = "Tracisz.punkty";
            lotteryVariants[2] = "Zamieniasz się punktami z pierwszym miejscem";
            lotteryVariants[3] = "Zamieniasz się punktami z ostatnim miejscem";
            lotteryVariants[4] = "Przez następne trzy rundy, jeśli nikt nie odpowie na pytanie, musisz je przejąć";
            lotteryVariants[5] = "Za następne pytanie na które odpowiesz otrzymujesz x.punktów";
            lotteryVariants[6] = "Przez trzy tury nie możesz przejmować pytań";
            lotteryVariants[7] = "Podczas następnego nieudanego przejęcia nie tracisz punktów";

            Random random = new Random();
            int randomizing = random.Next(56, 60); //TODO - zmienić potem na numer od 0 do 100

            if (randomizing >= 0 && randomizing <= 55) //Variant 0-1; Adding or substracting points from user
            {
                randomizing = random.Next(1, 11);
                if (randomizing >= 1 && randomizing <= 8) //80% chance to get from 0 to 5 points
                {
                    string[] stringToPass = lotteryVariants[0].Split('.');
                    randomizing = random.Next(0, 6);
                    if (randomizing == 1) stringToPass[1] = "punkt";
                    else if (randomizing == 5) stringToPass[1] = "punktów";
                    LotteryText.Text = stringToPass[0] + " " + randomizing.ToString() + " " + stringToPass[1];
                    curLotteryBonus = 0; //Index of bonus
                    lotteryPointsToPass = randomizing; //Points that player will get for this answer
                }
                else
                {
                    string[] stringToPass = lotteryVariants[1].Split('.');
                    randomizing = random.Next(0, 6);
                    if (randomizing == 1) stringToPass[1] = "punkt";
                    else if (randomizing == 5) stringToPass[1] = "punktów";
                    LotteryText.Text = stringToPass[0] + " " + randomizing.ToString() + " " + stringToPass[1];
                    curLotteryBonus = 1; //Index of bonus
                    lotteryPointsToPass = randomizing; //Points that player will get for this answer
                }
            }
            else if (randomizing >= 56 && randomizing <= 60) //Variant 2-3; Swapping 1st and last place 
            {
                randomizing = random.Next(0, 2);
                if (randomizing == 0) //Swapping with first place
                {
                    LotteryText.Text = lotteryVariants[2] + Environment.NewLine + "Wskaż drużynę, która odpowiedziała na to pytanie:";
                    swappingWithFirstPlace = true;
                    curLotteryBonus = 2; //Index of bonus
                }
                else //Swapping with last place
                {
                    LotteryText.Text = lotteryVariants[3] + Environment.NewLine + "Wskaż drużynę, która odpowiedziała na to pytanie:";
                    swappingWithFirstPlace = false;
                    curLotteryBonus = 3; //Index of bonus
                }

            }
            else if (randomizing >= 61 && randomizing <= 70) //Variant 4; Next three rounds you're forced to answer questions
            {
                LotteryText.Text = lotteryVariants[4]; 
                curLotteryBonus = 4; //Index of bonus
                
            }
            else if (randomizing >= 71 && randomizing <= 80) //Variant 5; For next answered question you're gaining extra points
            {
                LotteryText.Text = lotteryVariants[5];
                curLotteryBonus = 5; //Index of bonus
            }
            else if (randomizing >= 81 && randomizing <= 90) //Variant 6; You can't answer other participant's questions for three rounds
            {
                LotteryText.Text = lotteryVariants[6];
                curLotteryBonus = 6; //Index of bonus
            }
            else if (randomizing >= 91 && randomizing <= 100) //Variant 7; You're not loosing points for next unfortunate question take over
            {
                LotteryText.Text = lotteryVariants[7];
                curLotteryBonus = 7; //Index of bonus
            }
        }
        #region Music quiz
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
        #endregion
        #region Mixed Quiz
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
                SwitchingOffAllFields();
                ChoosingPointsType();
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
        #endregion
        #region Participants initialization
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
                participantsBonus[i] = null;
                usersModifiers[i] = 0;
                for (int k = 0; k < 4; k++)
                {
                    participantsIcons[i, k] = null;
                    participantsModifiers[i, k] = null;
                }
            }
            participantsNames[0] = nameParticipant1;
            participantsPoints[0] = pointsParticipant1;
            participantsPlus[0] = plusParticipant1;
            participantsMinus[0] = minusParticipant1;
            participantsBonus[0] = bonusParticipant1;
            participantsIcons[0, 0] = imageParticipant1_1;
            participantsModifiers[0, 0] = modifier1_1;
            #region Setting fields
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

            participantsBonus[0] = bonusParticipant1; participantsBonus[4] = bonusParticipant5; participantsBonus[8] = bonusParticipant9;
            participantsBonus[1] = bonusParticipant2; participantsBonus[5] = bonusParticipant6; participantsBonus[9] = bonusParticipant10;
            participantsBonus[2] = bonusParticipant3; participantsBonus[6] = bonusParticipant7; participantsBonus[10] = bonusParticipant11;
            participantsBonus[3] = bonusParticipant4; participantsBonus[7] = bonusParticipant8; participantsBonus[11] = bonusParticipant12;

participantsIcons[0, 1] = imageParticipant1_2; participantsIcons[0, 2] = imageParticipant1_3; participantsIcons[0, 3] = imageParticipant1_4;
participantsIcons[1, 0] = imageParticipant2_1; participantsIcons[1, 1] = imageParticipant2_2; participantsIcons[1, 2] = imageParticipant2_3; participantsIcons[1, 3] = imageParticipant2_4;
participantsIcons[2, 0] = imageParticipant3_1; participantsIcons[2, 1] = imageParticipant3_2; participantsIcons[2, 2] = imageParticipant3_3; participantsIcons[2, 3] = imageParticipant3_4;
participantsIcons[3, 0] = imageParticipant4_1; participantsIcons[3, 1] = imageParticipant4_2; participantsIcons[3, 2] = imageParticipant4_3; participantsIcons[3, 3] = imageParticipant4_4;
participantsIcons[4, 0] = imageParticipant5_1; participantsIcons[4, 1] = imageParticipant5_2; participantsIcons[4, 2] = imageParticipant5_3; participantsIcons[4, 3] = imageParticipant5_4;
participantsIcons[5, 0] = imageParticipant6_1; participantsIcons[5, 1] = imageParticipant6_2; participantsIcons[5, 2] = imageParticipant6_3; participantsIcons[5, 3] = imageParticipant6_4;
participantsIcons[6, 0] = imageParticipant7_1; participantsIcons[6, 1] = imageParticipant7_2; participantsIcons[6, 2] = imageParticipant7_3; participantsIcons[6, 3] = imageParticipant7_4;
participantsIcons[7, 0] = imageParticipant8_1; participantsIcons[7, 1] = imageParticipant8_2; participantsIcons[7, 2] = imageParticipant8_3; participantsIcons[7, 3] = imageParticipant8_4;
participantsIcons[8, 0] = imageParticipant9_1; participantsIcons[8, 1] = imageParticipant9_2; participantsIcons[8, 2] = imageParticipant9_3; participantsIcons[8, 3] = imageParticipant9_4;
participantsIcons[9, 0] = imageParticipant10_1; participantsIcons[9, 1] = imageParticipant10_2; participantsIcons[9, 2] = imageParticipant10_3; participantsIcons[9, 3] = imageParticipant10_4;
participantsIcons[10, 0] = imageParticipant11_1; participantsIcons[10, 1] = imageParticipant11_2; participantsIcons[10, 2] = imageParticipant11_3; participantsIcons[10, 3] = imageParticipant11_4;
participantsIcons[11, 0] = imageParticipant12_1; participantsIcons[11, 1] = imageParticipant12_2; participantsIcons[11, 2] = imageParticipant12_3; participantsIcons[11, 3] = imageParticipant12_4;

                                        participantsModifiers[0, 1] = modifier1_2; participantsModifiers[0, 2] = modifier1_3; participantsModifiers[0, 3] = modifier1_4;
participantsModifiers[1, 0] = modifier2_1; participantsModifiers[1, 1] = modifier2_2; participantsModifiers[1, 2] = modifier2_3; participantsModifiers[1, 3] = modifier2_4;
participantsModifiers[2, 0] = modifier3_1; participantsModifiers[2, 1] = modifier3_2; participantsModifiers[2, 2] = modifier3_3; participantsModifiers[2, 3] = modifier3_4;
participantsModifiers[3, 0] = modifier4_1; participantsModifiers[3, 1] = modifier4_2; participantsModifiers[3, 2] = modifier4_3; participantsModifiers[3, 3] = modifier4_4;
participantsModifiers[4, 0] = modifier5_1; participantsModifiers[4, 1] = modifier5_2; participantsModifiers[4, 2] = modifier5_3; participantsModifiers[4, 3] = modifier5_4;
participantsModifiers[5, 0] = modifier6_1; participantsModifiers[5, 1] = modifier6_2; participantsModifiers[5, 2] = modifier6_3; participantsModifiers[5, 3] = modifier6_4;
participantsModifiers[6, 0] = modifier7_1; participantsModifiers[6, 1] = modifier7_2; participantsModifiers[6, 2] = modifier7_3; participantsModifiers[6, 3] = modifier7_4;
participantsModifiers[7, 0] = modifier8_1; participantsModifiers[7, 1] = modifier8_2; participantsModifiers[7, 2] = modifier8_3; participantsModifiers[7, 3] = modifier8_4;
participantsModifiers[8, 0] = modifier9_1; participantsModifiers[8, 1] = modifier9_2; participantsModifiers[8, 2] = modifier9_3; participantsModifiers[8, 3] = modifier9_4;
participantsModifiers[9, 0] = modifier10_1; participantsModifiers[9, 1] = modifier10_2; participantsModifiers[9, 2] = modifier10_3; participantsModifiers[9, 3] = modifier10_4;
participantsModifiers[10, 0] = modifier11_1; participantsModifiers[10, 1] = modifier11_2; participantsModifiers[10, 2] = modifier11_3; participantsModifiers[10, 3] = modifier11_4;
participantsModifiers[11, 0] = modifier12_1; participantsModifiers[11, 1] = modifier12_2; participantsModifiers[11, 2] = modifier12_3; participantsModifiers[11, 3] = modifier12_4;

            #endregion

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
            #endregion
        }
    }
}