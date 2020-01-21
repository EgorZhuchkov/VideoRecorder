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
using System.Diagnostics;
using System.IO;


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

    //public class Recorder
    //{
    //    int height;
    //    int width;
    //    VideoFileWriter writer;
    //    AudioCaptureDevice audioCapture;
    //    ScreenCaptureStream screenCapture;

    //    public Recorder(int xlocation, int ylocation, int width, int height, AudioDeviceInfo deviceInfo)
    //    {
    //        height = 1080;
    //        width = 1920;
    //        writer = new VideoFileWriter();

    //        audioCapture = new AudioCaptureDevice(deviceInfo);
    //        audioCapture.NewFrame += AudioCapture_NewFrame;


    //        screenCapture = new ScreenCaptureStream(new System.Drawing.Rectangle(xlocation, ylocation, width, height));
    //        screenCapture.NewFrame += ScreenCapture_NewFrame;

    //    }
    //    private void ScreenCapture_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
    //    {
    //        writer.WriteVideoFrame(eventArgs.Frame);
    //    }

    //    private void AudioCapture_NewFrame(object sender, Accord.Audio.NewFrameEventArgs e)
    //    {
    //        writer.WriteAudioFrame(e.Signal.RawData);
    //    }

    //    internal void StartRecord()
    //    {
    //        screenCapture.Start();
    //        audioCapture.Start();

    //        var path = @"D:\Projects\TEST.mp4";

    //        int frameRate = 30;
    //        int bitRate = 400000;
    //        int audioBitrate = 320000;
    //        int sampleRate = 44100;
    //        int channels = 1;
    //        writer.Open(path, 1920, 1080, frameRate, VideoCodec.H264, bitRate,
    //        AudioCodec.MP3, audioBitrate, sampleRate, channels);
    //    }

    //    internal void StopRecord()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class Recorder
    {
        private long? StartTick;

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
            long currentTick = DateTime.Now.Ticks;
            StartTick = StartTick ?? currentTick;
            var frameOffset = new TimeSpan(currentTick - StartTick.Value);

            video.WriteVideoFrame(eventArgs.Frame, frameOffset);

        }

        private void Audio_NewFrame(object sender, Accord.Audio.NewFrameEventArgs e)
        {
            audioEncoder.Encode(e.Signal);
        }

        public void StartRecord()
        {
            if (!isRecording)
            {
                StartTick = null;
                isRecording = true;
                video.Open(@"D:\Projects\new_test.avi", 1920, 1080, new Accord.Math.Rational(60.0), VideoCodec.Default);
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

                mergeAudioAndVideo();
            }
        }

        private void mergeAudioAndVideo()
        {
            File.Delete(@"D:\Projects\final_video.avi");
            var path = @"D:\Projects\new_test.mp4";

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"D:\Projects\ffmpeg.exe";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            //startInfo.Arguments = "-i \"" + App.DataPath + "temp.avi\" -i \"" + App.DataPath + "temp.wav\" -c:v copy -c:a copy -map 0:v:0 -map 1:a:0 \"" + App.DataPath + final_videoFileName + ".avi\"";
            startInfo.Arguments = "-i \"" + @"D:\Projects\new_test.avi" + "\" -i \"" + @"D:\Projects\test_audio.wav" + "\" -c:v copy -c:a copy -map 0:v:0 -map 1:a:0 \"" + @"D:\Projects\final_video" + ".avi\"";

            process.StartInfo = startInfo;
            process.Start();
            //process.WaitForExit();

            //if (File.Exists(@"D:\Projects\final_video.avi"))
            //{
            //    File.Delete(@"D:\Projects\test_audio.wav");
            //    File.Delete(@"D:\Projects\new_test.avi");
            //}

        }
    }
}
