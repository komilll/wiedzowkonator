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
        public string[] _screenshotsLocalizationPath; //Path to quiz files (screenshot, music, text questions etc.)
        public enum answerType { noAnswer, fileNameAnswer, customAnswer };
        public answerType curAnswerType;
        public enum quizType { screenshot, text, music, mixed };
        public quizType curQuizType;
    }

    [Serializable]
    public class SerializationDataText
    {
        public List<string> answeredQuestions = new List<string>(); //List of which screenshots were already shown
        public string[] points = new string[1]; //Number of participant points
        public string[] participants = new string[1]; //Name of participants
        public string txtFileLocalization; //Path to quiz files (screenshot, music, text questions etc.)
        public enum answerType { noAnswer, fileNameAnswer, customAnswer };
        public answerType curAnswerType;
        public enum quizType { screenshot, text, music, mixed };
        public quizType curQuizType;
    }

    public partial class MainWindow : Window
    {
        /*** Screenshot quiz variables ***/
        SerializationData serializationData = new SerializationData(); //Variable class
        SerializationDataText serializationText = new SerializationDataText();

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
        

        //Enum type that shows which state of quiz is currently in progress
        enum quizState { customizingQuestions, choosingQuestion, answeringQuestion, givingPoints };
        quizState curQuizState;

        enum answerType { noAnswer, fileNameAnswer, customAnswer };
        answerType curAnswerType;

        enum quizType { screenshot, text, music, mixed };
        quizType curQuizType;
        
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
                    questionLength = textQuestions.Length;
                if (!string.IsNullOrWhiteSpace(questionNumberBox.Text) && isNumeric && int.Parse(questionNumberBox.Text) <= questionLength)
                {
                    if (curQuizType == quizType.screenshot)
                    {
                        //Substracting 1 to let user writting down from "1 to x" instead of "0 to x"
                        int currentScreenshot = lastScreenshotIndex = int.Parse(questionNumberBox.Text) - 1;
                        canvasScreenshotQuiz.Width = screenshots[currentScreenshot].Width;
                        canvasScreenshotQuiz.Height = screenshots[currentScreenshot].Height;

                        canvasScreenshotQuiz.Children.Add(screenshots[currentScreenshot]);
                    }
                    else if (curQuizType == quizType.text)
                    {
                        int currentQuestion = lastQuestionIndex = int.Parse(questionNumberBox.Text) - 1;
                        questionText.Text = textQuestions[currentQuestion];
                        ShowingQuestion(); //Showing question text and button to skip to answer and giving points
                    }
                    questionNumberBox.Text = "";
                }
                else //Uncorrectly written number
                {
                    if (curQuizType == quizType.screenshot)
                        MessageBox.Show("Wybierz liczby od 1 do " + (screenshots.Length - screenshotsCompleted));
                    else if (curQuizType == quizType.text)
                        MessageBox.Show("Wybierz liczby od 1 do " + (textQuestions.Length - textQuestionsCompleted));
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
            if (plusFirstParticipant.Width == 0 && curQuizState == quizState.givingPoints)
            {
                plusFirstParticipant.Width = 33;
                minutFirstParticipant.Width = 33;
                nameFirstParticipant.Width = 120;
                pointsFirstParticipant.Width = 33;
                confirmPoints.Width = 75;
                goBack.Width = 75;
                correctAnswer.Width = 200;
                if (curQuizType == quizType.text) questionText.Width = 0; //Hiding textblock with question
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
                }

            }
            QuickSave(); //If everything went good, current state is being save and if power is off or program crashes, user can load his last save
            SwitchingOffAllFields();
            ChoosingQuestion();
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
            if (curQuizType == quizType.screenshot)
                canvasScreenshotQuiz.Children.Add(screenshots[lastScreenshotIndex]);
            else if (curQuizType == quizType.text)
            {
                questionText.Width = 250;
                questionText.Text = textQuestions[lastQuestionIndex];
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
            }
        }


        private void SkipQuestion_Click(object sender, RoutedEventArgs e)
        {
            curQuizState = quizState.givingPoints;
            StartClick(null, null);
            correctAnswer.Text = textAnswers[lastQuestionIndex];
            SwitchingOffAllFields();
            GivingPoints_ShowingAnswers(); //Showing answer and giving points to contestants
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
                //fileDialog.Multiselect = false;
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
                    data.points[0] = pointsFirstParticipant.Text;
                    data.participants[0] = nameFirstParticipant.Text;
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
                    data.points[0] = pointsFirstParticipant.Text;
                    data.participants[0] = nameFirstParticipant.Text;
                    data.txtFileLocalization = txtFilePath;
                    data.curAnswerType = serializationText.curAnswerType = (SerializationDataText.answerType)curAnswerType;
                    data.curQuizType = serializationText.curQuizType = (SerializationDataText.quizType)curQuizType;

                    bf.Serialize(stream, data);
                    stream.Close(); //Closing stream to prevent from damaging data
                }
            }
        }
        //Loading state of quiz from .anime file
        private void Load(object sender, RoutedEventArgs e)
        {           
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (curQuizType == quizType.screenshot) fileDialog.Filter = "AnimeScreenshot (*.ANIMESCREEN)| *.animescreen";
                else if (curQuizType == quizType.text) fileDialog.Filter = "AnimeText (*.ANIMETEXT)| *.animetext";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fileDialog.FileName;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(path, FileMode.Open);            
                if (curQuizType == quizType.screenshot)
                {
                    SerializationData data = (SerializationData)bf.Deserialize(stream);
                    serializationData.answeredScreenshots = data.answeredScreenshots;
                    nameFirstParticipant.Text = data.participants[0];
                    pointsFirstParticipant.Text = data.points[0];
                    screenshotsLocalizationPath = data._screenshotsLocalizationPath;
                    curAnswerType = (answerType)data.curAnswerType;
                    curQuizType = (quizType)data.curQuizType;
                    MessageBox.Show(curAnswerType.ToString());
                    //DirectoryInfo directoryWithScreenshots = new DirectoryInfo(screenshotsLocalizationPath);
                    //[] files = new string[directoryWithScreenshots.GetFiles().Length];
                    //int index = 0;
                    //int numberOfSkippedFiles = 0;
                    //string dataFile;
                    /*
                    foreach (FileInfo file in directoryWithScreenshots.GetFiles())
                    {
                        files[index] = file.FullName;
                        if (file.Extension != ".jpg" && file.Extension != ".png" && file.Extension != ".bmp" && file.Extension != ".jpeg" && file.Extension != ".gif")
                        {
                            numberOfSkippedFiles++; //It'll be used in "for" loop to set number of loops
                                                    //Not increasing index
                            dataFile = file.FullName;
                        }
                        else
                        {
                            index++;
                        }
                    }*/

                    //bitmapImage = new BitmapImage[directoryWithScreenshots.GetFiles().Length - numberOfSkippedFiles];
                    //screenshots = new Image[bitmapImage.Length];
                    //nameOfScreenshots = new string[bitmapImage.Length];
                    /*for (int k = 0; k < directoryWithScreenshots.GetFiles().Length - numberOfSkippedFiles; k++)
                    {
                        bitmapImage[k] = new BitmapImage(new Uri(files[k], UriKind.Absolute));
                        screenshots[k] = new Image();
                        screenshots[k].Source = bitmapImage[k];
                        screenshots[k].Width = bitmapImage[k].Width;
                        screenshots[k].Height = bitmapImage[k].Height;
                        nameOfScreenshots[k] = files[k];

                        string[] correctNameOfScreenshot = nameOfScreenshots[k].Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                        FileInfo info = new FileInfo(nameOfScreenshots[k]);
                        nameOfScreenshots[k] = correctNameOfScreenshot[0] + info.Extension;
                    }*/
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
                    nameFirstParticipant.Text = data.participants[0];
                    pointsFirstParticipant.Text = data.points[0];
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
                stream.Close(); //Closing stream to prevent from damaging data
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
                data.points[0] = pointsFirstParticipant.Text;
                data.participants[0] = nameFirstParticipant.Text;
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
                data.points[0] = pointsFirstParticipant.Text;
                data.participants[0] = nameFirstParticipant.Text;
                data.txtFileLocalization = txtFilePath;
                data.curAnswerType = serializationText.curAnswerType = (SerializationDataText.answerType)curAnswerType;
                data.curQuizType = serializationText.curQuizType = (SerializationDataText.quizType)curQuizType;

                bf.Serialize(stream, data);
                stream.Close(); //Closing stream to prevent from damaging data      
            }
        }

        private void QuickLoad() //Loading at the start of quiz
        {
            //Loading data containing deeper information and values of variables
            DirectoryInfo directory = new DirectoryInfo(quickSavePath);
            FileInfo[] pathToCheck = directory.GetFiles();
            List<FileInfo> extensionPaths = new List<FileInfo>();
            for (int i = 0; i < pathToCheck.Length; i++)
            {
                if (curQuizType == quizType.screenshot) {
                    if (pathToCheck[i].Extension == ".animescreen") extensionPaths.Add(pathToCheck[i]);
                }
                else if (curQuizType == quizType.text) {
                    if (pathToCheck[i].Extension == ".animetext") extensionPaths.Add(pathToCheck[i]);
                }
            }
            string path = (from f in extensionPaths
                           orderby f.LastWriteTime descending
                           select f).First().FullName;
            //MessageBox.Show(path);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);

            if (curQuizType == quizType.screenshot)
            {
                SerializationData data = (SerializationData)bf.Deserialize(stream);
                serializationData.answeredScreenshots = data.answeredScreenshots;
                nameFirstParticipant.Text = data.participants[0];
                pointsFirstParticipant.Text = data.points[0];
                screenshotsLocalizationPath = data._screenshotsLocalizationPath;
                curAnswerType = (answerType)data.curAnswerType;

                MessageBox.Show(curAnswerType.ToString());
                //Loading screenshots from directory where they should be
                /*DirectoryInfo directoryWithScreenshots = new DirectoryInfo(screenshotsLocalizationPath);
                string[] files = new string[directoryWithScreenshots.GetFiles().Length];
                int index = 0;
                int numberOfSkippedFiles = 0;
                string dataFile;

                foreach (FileInfo file in directoryWithScreenshots.GetFiles())
                {
                    files[index] = file.FullName;
                    if (file.Extension != ".jpg" && file.Extension != ".png" && file.Extension != ".bmp" && file.Extension != ".jpeg" && file.Extension != ".gif")
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

                    string[] correctNameOfScreenshot = nameOfScreenshots[k].Split(new[] { "@correct_answer_" }, StringSplitOptions.None);
                    FileInfo info = new FileInfo(nameOfScreenshots[k]);
                    nameOfScreenshots[k] = correctNameOfScreenshot[0] + info.Extension;
                }
                */
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
                nameFirstParticipant.Text = data.participants[0];
                pointsFirstParticipant.Text = data.points[0];
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
            stream.Close(); //Closing stream to prevent from damaging data
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
                    string[] pathPart = infoToPass.FullName.Split(new[] { "@correct_answer_" }, StringSplitOptions.None);;
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
            plusFirstParticipant.Width = 0;
            minutFirstParticipant.Width = 0;
            nameFirstParticipant.Width = 0;
            pointsFirstParticipant.Width = 0;
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
            if (curQuizType == quizType.text)
            {
                questionText.Width = 250;
                SkipQuestion.Width = 150;
            }
        }

        void GivingPoints_ShowingAnswers()
        {
            //TODO -- dodać więcej uczestników oraz ogarnąć wartości procentowe
            confirmPoints.Width = 75;
            goBack.Width = 75;
            //correctAnswer.Width = 0;
            nameFirstParticipant.Width = 120;
            pointsFirstParticipant.Width = 33;
            plusFirstParticipant.Width = 33;
            minutFirstParticipant.Width = 33;
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
            //curQuizType = quizType.music;
            //SwitchingOffAllFields();
            //ChoosingQuizOpeningType();
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mediaPlayer.Open(new Uri(fileDialog.FileName));
            }
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
            if (textQuestions != null || screenshots != null)
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
            if (textQuestions != null || screenshots != null)
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
            if (textQuestions != null || screenshots != null)
            {
                SwitchingOffAllFields();
                ChoosingQuestion();
            }
        }

        private void playAudioButton_Click(object sender, RoutedEventArgs e)
        {
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
    }
    #endregion
}