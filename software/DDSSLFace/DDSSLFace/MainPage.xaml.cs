using Microsoft.Azure.Devices;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
// Based on https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/simple-camera-preview-access
// And https://msdn.microsoft.com/en-us/magazine/mt788628.aspx

namespace DDSSLFace
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private bool isPreviewing;
        private DisplayRequest displayRequest = new DisplayRequest();
        private DispatcherTimer timer = new DispatcherTimer();
        private FaceDetectionEffect _faceDetectionEffect;
        private IMediaEncodingProperties _previewProperties;

        public MainPage()
        {
            this.InitializeComponent();
            var serviceClient = ServiceClient.CreateFromConnectionString("");

            Application.Current.Suspending += Application_Suspending;

            var green = new BitmapImage(new Uri("ms-appx:///Assets/Green.png"));
            var orange = new BitmapImage(new Uri("ms-appx:///Assets/Orange.png"));
            var red = new BitmapImage(new Uri("ms-appx:///Assets/Red.png"));

            var countdown = 5;
            var status = string.Empty;
            timer.Tick += async (s, e) =>
            {
                CountDown.Text = $"{countdown:0}";
                Status.Text = status;
                if (countdown <= 0)
                {
                    countdown = 5;

                    using (var faceFrame = await mediaCapture.GetPreviewFrameAsync(new VideoFrame(BitmapPixelFormat.Bgra8, (int)PreviewControl.ActualWidth, (int)PreviewControl.ActualHeight)))
                    using (var faceStream = new InMemoryRandomAccessStream())
                    using (var faceClient = new FaceServiceClient("", "https://westeurope.api.cognitive.microsoft.com/face/v1.0"))
                    {
                        var faceEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, faceStream);
                        faceEncoder.SetSoftwareBitmap(faceFrame.SoftwareBitmap);
                        await faceEncoder.FlushAsync();

                        var faces = await faceClient.DetectAsync(faceStream.AsStreamForRead(),
                            returnFaceLandmarks: true,
                            returnFaceAttributes: new[] {
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.Smile,
                            FaceAttributeType.FacialHair,
                            FaceAttributeType.Gender,
                            FaceAttributeType.Hair});

                        Lamp.Visibility = Visibility.Collapsed;
                        var message = new
                        {
                            Name = "brightness",
                            Parameters = new
                            {
                                green = 0,
                                orange = 0,
                                red = 0
                            }
                        };

                        if (faces.Length > 0)
                        {
                            var glasses = faces[0].FaceAttributes.Glasses;
                            var smile = faces[0].FaceAttributes.Smile;
                            var emotions = faces[0].FaceAttributes.Emotion;
                            var facialHair = faces[0].FaceAttributes.FacialHair;
                            var gender = faces[0].FaceAttributes.Gender;
                            var hair = faces[0].FaceAttributes.Hair;

                            if (glasses != Glasses.NoGlasses)
                            {
                                if (smile <= 0.1d)
                                {
                                    Lamp.Source = red;
                                    message = new
                                    {
                                        Name = "brightness",
                                        Parameters = new
                                        {
                                            green = 0,
                                            orange = 0,
                                            red = 255
                                        }
                                    };
                                }
                                else if (smile <= 0.5d)
                                {
                                    Lamp.Source = orange;
                                    message = new
                                    {
                                        Name = "brightness",
                                        Parameters = new
                                        {
                                            green = 0,
                                            orange = 255,
                                            red = 0
                                        }
                                    };
                                }
                                else
                                {
                                    Lamp.Source = green;
                                    message = new
                                    {
                                        Name = "brightness",
                                        Parameters = new
                                        {
                                            green = 255,
                                            orange = 0,
                                            red = 0
                                        }
                                    };
                                }
                                Lamp.Visibility = Visibility.Visible;
                            }
                            
                            status  = $"Glasses:  {glasses}{Environment.NewLine}";
                            status += $"Smile:    {smile:0.000}{Environment.NewLine}{Environment.NewLine}";
                            status += $"Gender:   {gender}{Environment.NewLine}{Environment.NewLine}";
                            status += $"Anger:    {emotions.Anger:0.000}{Environment.NewLine}";
                            status += $"Contempt: {emotions.Contempt:0.000}{Environment.NewLine}";
                            status += $"Disgust:  {emotions.Disgust:0.000}{Environment.NewLine}";
                            status += $"Fear:     {emotions.Fear:0.000}{Environment.NewLine}";
                            status += $"Happiness:{emotions.Happiness:0.000}{Environment.NewLine}";
                            status += $"Neutral:  {emotions.Neutral:0.000}{Environment.NewLine}";
                            status += $"Sadness:  {emotions.Sadness:0.000}{Environment.NewLine}";
                            status += $"Surprise: {emotions.Surprise:0.000}{Environment.NewLine}{Environment.NewLine}";
                            status += $"Beard:    {facialHair.Beard:0.000}{Environment.NewLine}";
                            status += $"Moustache:{facialHair.Moustache:0.000}{Environment.NewLine}";
                            status += $"Sideburns:{facialHair.Sideburns:0.000}{Environment.NewLine}";
                            status += $"Bald:     {hair.Bald:0.000}";
                        }
                        else
                        {
                            status = string.Empty;
                        }
                        await serviceClient.SendAsync("", new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))));
                    }
                }
                countdown--;
            };
            timer.Interval = TimeSpan.FromSeconds(1);
        }

        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                var faceDetectionDefinition = new FaceDetectionEffectDefinition();
                faceDetectionDefinition.DetectionMode = FaceDetectionMode.HighPerformance;
                faceDetectionDefinition.SynchronousDetectionEnabled = false;
                _faceDetectionEffect = (FaceDetectionEffect)await mediaCapture.AddVideoEffectAsync(faceDetectionDefinition, MediaStreamType.VideoPreview);
                _faceDetectionEffect.FaceDetected += async (s, a) =>
                {
                    var detectedFaces = a.ResultFrame.DetectedFaces;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        FaceCanvas.Children.Clear();

                        for (int i = 0; i < detectedFaces.Count; i++)
                        {
                            var face = detectedFaces[i];
                            var faceBounds = face.FaceBox;

                            var faceHighlightRectangle = MapRectangleToDetectedFace(detectedFaces[i].FaceBox);
                            faceHighlightRectangle.Stroke = new SolidColorBrush(Colors.Red);

                            faceHighlightRectangle.StrokeThickness = 2;

                            FaceCanvas.Children.Add(faceHighlightRectangle);

                        }
                    });
                };
                _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(33);
                _faceDetectionEffect.Enabled = true;

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                await ShowMessageToUser("The app was denied access to the camera");
                return;
            }

            try
            {
                PreviewControl.Source = mediaCapture;

                await mediaCapture.StartPreviewAsync();
                timer.Start();
                _previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
                isPreviewing = true;
            }
            catch (FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                await ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

        private async Task CleanupCameraAsync()
        {
            timer.Stop();
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {

            await CleanupCameraAsync();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await StartPreviewAsync();
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            timer.Stop();
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private async Task ShowMessageToUser(string message)
        {
            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }

        private Rectangle MapRectangleToDetectedFace(BitmapBounds detectedfaceBoxCoordinates)
        {
            var faceRectangle = new Rectangle();
            var previewStreamPropterties = _previewProperties as VideoEncodingProperties;

            double mediaStreamWidth = previewStreamPropterties.Width;
            double mediaStreamHeight = previewStreamPropterties.Height;

            var faceHighlightRect = LocatePreviewStreamCoordinates(previewStreamPropterties, PreviewControl);

            faceRectangle.Width = (detectedfaceBoxCoordinates.Width / mediaStreamWidth) * faceHighlightRect.Width;
            faceRectangle.Height = (detectedfaceBoxCoordinates.Height / mediaStreamHeight) * faceHighlightRect.Height;

            var x = (detectedfaceBoxCoordinates.X / mediaStreamWidth) * faceHighlightRect.Width;
            var y = (detectedfaceBoxCoordinates.Y / mediaStreamHeight) * faceHighlightRect.Height;

            Canvas.SetLeft(faceRectangle, x);
            Canvas.SetTop(faceRectangle, y);

            return faceRectangle;
        }

        public Rect LocatePreviewStreamCoordinates(VideoEncodingProperties previewResolution, CaptureElement previewControl)
        {
            var uiRectangle = new Rect();

            var mediaStreamWidth = previewResolution.Width;
            var mediaStreamHeight = previewResolution.Height;

            uiRectangle.Width = previewControl.ActualWidth;
            uiRectangle.Height = previewControl.ActualHeight;

            var uiRatio = previewControl.ActualWidth / previewControl.ActualHeight;
            var mediaStreamRatio = mediaStreamWidth / mediaStreamHeight;

            if (uiRatio > mediaStreamRatio)
            {
                var scaleFactor = previewControl.ActualHeight / mediaStreamHeight;
                var scaledWidth = mediaStreamWidth * scaleFactor;

                uiRectangle.X = (previewControl.ActualWidth - scaledWidth) / 2.0;
                uiRectangle.Width = scaledWidth;
            }
            else
            {
                var scaleFactor = previewControl.ActualWidth / mediaStreamWidth;
                var scaledHeight = mediaStreamHeight * scaleFactor;
                uiRectangle.Y = (previewControl.ActualHeight - scaledHeight) / 2.0;
                uiRectangle.Height = scaledHeight;
            }

            return uiRectangle;
        }
    }
}
