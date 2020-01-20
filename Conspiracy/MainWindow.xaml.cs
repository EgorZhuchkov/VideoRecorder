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
            recorder = new Recorder(0, 0, 1920, 1080);
            State.Text = "No recording";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
        bool isRecording;
        public Recorder(int xlocation, int ylocation, int width, int height)
        {
            captureStream = new ScreenCaptureStream(new System.Drawing.Rectangle(xlocation, ylocation, width, height));
            video = new VideoFileWriter();
        }
        public void StartRecord()
        {
            if(!isRecording)
            {
                isRecording = true;
                captureStream.NewFrame += CaptureStream_NewFrame;
                captureStream.FrameInterval = 1;
                video.Open(@"D:\Projects\test.mp4", 1920, 1080, new Accord.Math.Rational(24.0), VideoCodec.MPEG4);
                captureStream.Start();
            }
        }
        public void StopRecord() 
        {
            if (isRecording)
            {
                isRecording = false;
                captureStream.Stop();
                System.Threading.Thread.Sleep(100);
                video.Close();
            }
        }
        private void CaptureStream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video.WriteVideoFrame(eventArgs.Frame);
        }
    }
}
