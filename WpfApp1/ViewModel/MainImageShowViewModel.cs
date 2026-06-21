using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows;

namespace WpfApp1.ViewModel
{
    public class MainImageShowViewModel : ViewModelBase
    {
        private int _cameraIndex;
        private string _stationName;
        private BitmapSource _displayImage;
        private bool _isOK = true;
        private string _resultText = "";
        private string _imageInfo = "";
        private bool _isOnline = true;
        private bool _isDetecting;

        private int _totalCount;
        private int _okCount;
        private int _ngCount;
        private string _passRate = "0.00%";
        private int _aiCount;
        private int _skipCount;

        private ObservableCollection<DetectResultItem> _detectResults = new ObservableCollection<DetectResultItem>();

        public int CameraIndex
        {
            get => _cameraIndex;
            set => SetField(ref _cameraIndex, value);
        }

        public string StationName
        {
            get => _stationName;
            set => SetField(ref _stationName, value);
        }

        public BitmapSource DisplayImage
        {
            get => _displayImage;
            set => SetField(ref _displayImage, value);
        }

        public bool IsOK
        {
            get => _isOK;
            set
            {
                SetField(ref _isOK, value);
                ResultText = value ? "OK" : "NG";
            }
        }

        public string ResultText
        {
            get => _resultText;
            set => SetField(ref _resultText, value);
        }

        public string ImageInfo
        {
            get => _imageInfo;
            set => SetField(ref _imageInfo, value);
        }

        public bool IsOnline
        {
            get => _isOnline;
            set => SetField(ref _isOnline, value);
        }

        public bool IsDetecting
        {
            get => _isDetecting;
            set => SetField(ref _isDetecting, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                SetField(ref _totalCount, value);
                UpdatePassRate();
            }
        }

        public int OKCount
        {
            get => _okCount;
            set
            {
                SetField(ref _okCount, value);
                UpdatePassRate();
            }
        }

        public int NGCount
        {
            get => _ngCount;
            set => SetField(ref _ngCount, value);
        }

        public string PassRate
        {
            get => _passRate;
            set => SetField(ref _passRate, value);
        }

        public int AiCount
        {
            get => _aiCount;
            set => SetField(ref _aiCount, value);
        }

        public int SkipCount
        {
            get => _skipCount;
            set => SetField(ref _skipCount, value);
        }

        public ObservableCollection<DetectResultItem> DetectResults
        {
            get => _detectResults;
            set => SetField(ref _detectResults, value);
        }

        private void UpdatePassRate()
        {
            if (_totalCount > 0)
                PassRate = $"{(double)_okCount / _totalCount * 100:F2}%";
            else
                PassRate = "0.00%";
        }

        public void SetResult(bool isOk, BitmapSource image, string imageInfo, string defectInfo)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsOK = isOk;
                DisplayImage = image;
                ImageInfo = imageInfo;

                TotalCount++;
                if (isOk)
                    OKCount++;
                else
                    NGCount++;

                DetectResults.Clear();
                if (!string.IsNullOrEmpty(defectInfo))
                {
                    foreach (var line in defectInfo.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        DetectResults.Add(new DetectResultItem { Info = line.Trim() });
                    }
                }
            });
        }

        public void Reset()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                TotalCount = 0;
                OKCount = 0;
                NGCount = 0;
                AiCount = 0;
                SkipCount = 0;
                PassRate = "0.00%";
                DetectResults.Clear();
                IsOK = true;
                ResultText = "";
                DisplayImage = null;
            });
        }
    }

    public class DetectResultItem
    {
        public string Info { get; set; }
    }
}
