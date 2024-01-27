using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace Visszaszámláló_óra
{
    public partial class Form1 : Form
    {
        private bool isMinuteReached = false;
        private bool isCountdownFinished = false;
        private Timer countdownTimer;
        private WaveOutEvent startSoundPlayer;
        private WaveOutEvent stopSoundPlayer;
        private WaveOutEvent one_minSoundPlayer;

        public Form1()
        {
            InitializeComponent();

            // Inicializáld a startSoundPlayer objektumot
            startSoundPlayer = new WaveOutEvent();
            startSoundPlayer.Init(new Mp3FileReader(@"timer-start.mp3"));

            // Inicializáld a stopSoundPlayer objektumot
            stopSoundPlayer = new WaveOutEvent();
            stopSoundPlayer.Init(new Mp3FileReader(@"timer-stop.mp3"));

            // Inicializáld a one_minSoundPlayer objektumot
            one_minSoundPlayer = new WaveOutEvent();
            one_minSoundPlayer.Init(new Mp3FileReader(@"timer-one_min.mp3"));
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Ellenőrizd, hogy az óra, perc és másodperc mezők tartalmaznak-e érvényes értéket
            int hour, minute, second;
            if (!int.TryParse(hourTextBox.Text, out hour) ||
                !int.TryParse(minuteTextBox.Text, out minute) ||
                !int.TryParse(secondTextBox.Text, out second))
            {
                MessageBox.Show("Hibás bemeneti érték!");
                return;
            }

            // Ellenőrizd, hogy az értékek a megfelelő tartományban vannak-e
            if (hour < 0 || hour > 23 ||
                minute < 0 || minute > 59 ||
                second < 0 || second > 59)
            {
                MessageBox.Show("Hibás időérték!");
                return;
            }

            // Számold ki a visszaszámlálás összes milliszecundumát
            int totalMilliseconds = (hour * 3600 + minute * 60 + second) * 1000;

            // Indítsd el a visszaszámlálást egy időzítő segítségével
            countdownTimer = new Timer();
            countdownTimer.Interval = 10; // 10 milliszecundum
            countdownTimer.Tick += (timerSender, timerArgs) =>
            {
                // Csökkentsd a visszaszámláló értékét 16-tal (16 milliszecundummal)
                totalMilliseconds -= 16;

                // Ellenőrizd, hogy lejárt-e a visszaszámlálás
                if (totalMilliseconds <= 0)
                {
                    countdownTimer.Stop(); // Ha lejárt, állítsd le az időzítőt
                    isCountdownFinished = true;
                    //MessageBox.Show("Visszaszámlálás véget ért!");

                    // Játszd le a stop hangot a végén
                    stopSoundPlayer.Play();
                }

                // Számold ki az óra, perc, másodperc és századmásodperc értékeit
                int remainingHours = totalMilliseconds / (3600 * 1000);
                int remainingMinutes = (totalMilliseconds % (3600 * 1000)) / (60 * 1000);
                int remainingSeconds = (totalMilliseconds % (60 * 1000)) / 1000;
                int remainingMilliseconds = totalMilliseconds % 1000 / 10;

                // Frissítsd a Label szövegét a visszaszámláló értékével
                if (remainingHours > 0)
                {
                    countdownLabel.Text = $"{remainingHours:D2}:{remainingMinutes:D2}:{remainingSeconds:D2}.{remainingMilliseconds:D2}";
                }
                else if (remainingMinutes > 0)
                {
                    countdownLabel.Text = $"{remainingMinutes:D2}:{remainingSeconds:D2}.{remainingMilliseconds:D2}";
                }
                else
                {
                    if (remainingSeconds == 0 && remainingMilliseconds == 0)
                    {
                        countdownLabel.Text = "00.00";
                        countdownLabel.TextAlign = ContentAlignment.MiddleCenter; // Középre igazítás
                    }
                    else
                    {
                        if (remainingSeconds == 0)
                        {
                            countdownLabel.Text = $"{remainingMilliseconds:D2}";
                        }
                        else
                        {
                            countdownLabel.Text = $"{remainingSeconds:D2}.{remainingMilliseconds:D2}";
                        }
                        countdownLabel.TextAlign = ContentAlignment.MiddleCenter; // Középre igazítás
                    }
                }

                // Ellenőrizd, hogy elérted-e az egy percet
                if (!isMinuteReached && totalMilliseconds <= 60000)
                {
                    countdownLabel.ForeColor = Color.Red; // Piros szín
                    isMinuteReached = true;
                    one_minSoundPlayer.Play(); // Játszd le a stop hangot a perc végén
                    one_minSoundPlayer = new WaveOutEvent();
                    one_minSoundPlayer.Init(new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(@"timer-one_min.mp3"))));
                }

                // Ellenőrizd, hogy a visszaszámlálás véget ért-e
                if (isCountdownFinished && totalMilliseconds <= 0)
                {
                    countdownTimer.Stop(); // Ha véget ért, állítsd le az időzítőt
                    //MessageBox.Show("Visszaszámlálás véget ért!");
                    countdownLabel.Text = "00";

                    // Játszd le a stop hangot a végén
                    stopSoundPlayer.Play();
                }
            };

            // Játszd le a start hangot
            startSoundPlayer.Play();
            countdownTimer.Start(); // Indítsd el az időzítőt
            hourTextBox.Visible = false;
            minuteTextBox.Visible = false;
            secondTextBox.Visible = false;
            startButton.Visible = false;
            restartButton.Visible = true;
            label1.Visible = false;
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            countdownTimer.Stop(); // Állítsd le az időzítőt
            countdownLabel.Text = "00:00:00.00"; // Alaphelyzetbe állítja a szöveget
            countdownLabel.ForeColor = Color.White; // Alapértelmezett fehér szín
            countdownLabel.TextAlign = ContentAlignment.MiddleCenter; // Alapértelmezett igazítás
            startSoundPlayer.Stop(); // Állítsd le a start hang lejátszását (ha éppen lejátszódik)
            stopSoundPlayer.Stop(); // Állítsd le a stop hang lejátszását (ha éppen lejátszódik)
            one_minSoundPlayer.Stop(); // Állítsd le a one_min hang lejátszását (ha éppen lejátszódik)
            stopSoundPlayer.Init(new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(@"timer-stop.mp3"))));
            startSoundPlayer.Init(new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(@"timer-start.mp3"))));
            one_minSoundPlayer.Init(new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(@"timer-one_min.mp3"))));
            hourTextBox.Visible = true;//óra beirása
            minuteTextBox.Visible = true;//perc beírása
            secondTextBox.Visible = true;//másodperc beírása
            startButton.Visible = true;//start gomb láthatósága
            restartButton.Visible = false;//reset gomb láthatósága
            label1.Visible = true;//kis szöveg láthatósága
            isMinuteReached = false;
            isCountdownFinished = false;
        }

        private void countdownLabel_Click(object sender, EventArgs e)
        {

        }

        private void hourTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
