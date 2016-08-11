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
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wiedzowkonator
{
    /// <summary>
    /// Interaction logic for Lottery_options.xaml
    /// </summary>
    /// 
    [Serializable]
    public class LotteryOptions
    {
        public int gainMin;
        public int gainMax;
        public int lostMin;
        public int lostMax;
    }

    public partial class Lottery_options : Window
    {
        MainWindow window = new MainWindow();
        bool canEnterSaveChanges = false;

        public Lottery_options()
        {
            //InitializeComponent();
        }
        
        public void Start(MainWindow windowOuter, bool instaClose)
        {
            window = windowOuter;

            if (File.Exists(window.quickSavePath + "lottery_options.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(window.quickSavePath + "lottery_options.dat", FileMode.Open);

                LotteryOptions data = (LotteryOptions)bf.Deserialize(stream);
                window.gainedMinimumPointsLottery = data.gainMin;
                window.gainedMaximumPointsLottery = data.gainMax;
                window.lostMinimumPointsLottery = data.lostMin;
                window.lostMaximumPointsLottery = data.lostMax;

                stream.Close();

                if (instaClose == false)
                {
                    GainedPointsMin.Text = window.gainedMinimumPointsLottery.ToString();
                    GainedPointsMax.Text = window.gainedMaximumPointsLottery.ToString();
                    LosingPointsMin.Text = window.lostMinimumPointsLottery.ToString();
                    LosingPointsMax.Text = window.lostMaximumPointsLottery.ToString();
                }
            }
            if (instaClose == true) //If it's entry on MainWindow startup then lottery options are loaded and window is instantly close
                this.Close();
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges_Click(null, null);
            this.Close();
        }

        private void AbortChanges_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            //Checking if user has written number
            int varForParsing = 0;
            bool tryGainMin = int.TryParse(GainedPointsMin.Text, out varForParsing);
            bool tryGainMax = int.TryParse(GainedPointsMax.Text, out varForParsing);
            bool tryLostMin = int.TryParse(LosingPointsMin.Text, out varForParsing);
            bool tryLostMax = int.TryParse(LosingPointsMax.Text, out varForParsing);

            if (tryGainMax && tryGainMin && tryLostMax && tryLostMin) //If user has written number
            {
                if (int.Parse(GainedPointsMin.Text) >= 0 && int.Parse(GainedPointsMax.Text) >= 0 && //Checking if number are greater or equal to 0
                    int.Parse(LosingPointsMin.Text) >= 0 && int.Parse(LosingPointsMax.Text) >= 0)
                {
                    if (int.Parse(GainedPointsMax.Text) < int.Parse(GainedPointsMin.Text)) //If max < min then alert shows
                    {
                        MessageBox.Show("Maksymalna liczba otrzymywanych lub traconych punktów nie może być mniejsza od minimalnej.");
                    }
                    else if (int.Parse(LosingPointsMax.Text) < int.Parse(LosingPointsMin.Text)) //If max < min then alert shows
                    {
                        MessageBox.Show("Maksymalna liczba otrzymywanych lub traconych punktów nie może być mniejsza od minimalnej.");
                    }
                    else //If everything went OK
                    {
                        canEnterSaveChanges = true;
                    }
                }
                else //If numbers are negative; alert shows
                {
                    MessageBox.Show("W pola określające przedziały należy wpisać liczby naturalne.");
                }
            }
            else //If user has written down something other than numbers or left empty fields
            {
                MessageBox.Show("W pola określające przedziały należy wpisać liczby naturalne.");
            }

            if (canEnterSaveChanges)
            {
                //Setting minimum points that participants will get for answering questions
                if (tryGainMin && int.Parse(GainedPointsMin.Text) >= 0)
                    window.gainedMinimumPointsLottery = int.Parse(GainedPointsMin.Text);
                else
                    MessageBox.Show("Przedział uzyskiwanych i traconych punkt musi być określony liczbami naturalnymi (wraz z zerem).");
                //Setting maximum points that participants will get for answering questions
                if (tryGainMax && int.Parse(GainedPointsMax.Text) >= 0)
                    window.gainedMaximumPointsLottery = int.Parse(GainedPointsMax.Text);
                else
                    MessageBox.Show("Przedział uzyskiwanych i traconych punkt musi być określony liczbami naturalnymi (wraz z zerem).");
                //Setting minimum points that participants will lose for answering questions
                if (tryLostMin && int.Parse(LosingPointsMin.Text) >= 0)
                    window.lostMinimumPointsLottery = int.Parse(LosingPointsMin.Text);
                else
                    MessageBox.Show("Przedział uzyskiwanych i traconych punkt musi być określony liczbami naturalnymi (wraz z zerem).");
                //Setting maximum points that participants will lose for answering questions
                if (tryLostMax && int.Parse(LosingPointsMax.Text) >= 0)
                    window.lostMaximumPointsLottery = int.Parse(LosingPointsMax.Text);
                else
                    MessageBox.Show("Przedział uzyskiwanych i traconych punkt musi być określony liczbami naturalnymi (wraz z zerem).");

                canEnterSaveChanges = false; //Resetting enter bool. It has to check conditions every time to avoid bugs

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Create(window.quickSavePath + "lottery_options.dat");
                LotteryOptions data = new LotteryOptions();

                data.gainMin = window.gainedMinimumPointsLottery;
                data.gainMax = window.gainedMaximumPointsLottery;
                data.lostMin = window.lostMinimumPointsLottery;
                data.lostMax = window.lostMaximumPointsLottery;

                bf.Serialize(stream, data);
                stream.Close();
            }
        }
    }
}
