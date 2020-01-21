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
using Accord.Video;
using Accord.Video.FFMPEG;
using System.Drawing;
using System.Drawing.Imaging;
using Accord.Audio;
using Accord.DirectSound;
using Accord.Audio.Formats;


namespace Conspiracy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Recorder recorder;
        public MainWindow()
        {
            InitializeComponent();
            
            State.Text = "No recording";
            audioDevices.ItemsSource = new AudioDeviceCollection(AudioDeviceCategory.Capture);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            recorder = new Recorder(0, 0, 1920, 1080, (AudioDeviceInfo)audioDevices.SelectedItem);
            recorder.StartRecord();
            State.Text = "Recording";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            recorder.StopRecord();
            State.Text = "Stopped recording";
        }
    }

    public class Recorder
    {
        ScreenCaptureStream captureStream;
        VideoFileWriter video;
        AudioCaptureDevice audio;
        WaveEncoder audioEncoder;
        bool isRecording;
        public Recorder(int xlocation, int ylocation, int width, int height, AudioDeviceInfo audioDevice)
        {
            audioEncoder = new WaveEncoder(@"D:\Projects\test_audio.wav");
            audio = new AudioCaptureDevice(audioDevice);
            captureStream = new ScreenCaptureStream(new System.Drawing.Rectangle(xlocation, ylocation, width, height));
            video = new VideoFileWriter();

            audio.DesiredFrameSize = 4096;
            audio.SampleRate = 44100;
            audio.NewFrame += Audio_NewFrame;

            captureStream.FrameInterval = 1;
            captureStream.NewFrame += CaptureStream_NewFrame;
        }

        private void CaptureStream_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            video.WriteVideoFrame(eventArgs.Frame);
        }

        private void Audio_NewFrame(object sender, Accord.Audio.NewFrameEventArgs e)
        {
            audioEncoder.Encode(e.Signal);
        }

        public void StartRecord()
        {
            if (!isRecording)
            {
                isRecording = true;
                video.Open(@"D:\Projects\new_test.mp4", 1920, 1080, new Accord.Math.Rational(60.0), VideoCodec.MPEG4);
                captureStream.Start();
                audio.Start();
            }
        }
        public void StopRecord()
        {
            if (isRecording)
            {
                isRecording = false;
                captureStream.Stop();
                audio.Stop();
                System.Threading.Thread.Sleep(100);
                video.Close();
            }
        }

    }
}
