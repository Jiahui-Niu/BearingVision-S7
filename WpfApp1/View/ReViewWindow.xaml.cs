using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp1.View
{
    public class ImageRecord
    {
        public string FilePath { get; set; }
        public string CameraName { get; set; }
        public string Timestamp { get; set; }
        public bool IsOK { get; set; }
        public string ResultText => IsOK ? "OK" : "NG";
        public string DefectInfo { get; set; }
        public BitmapSource Thumbnail { get; set; }
    }

    public partial class ReViewWindow : Window
    {
        private string _imageRoot;
        private List<ImageRecord> _allRecords = new List<ImageRecord>();

        public ReViewWindow(string imageRoot)
        {
            InitializeComponent();
            _imageRoot = imageRoot;
            dpStart.SelectedDate = DateTime.Today.AddDays(-7);
            dpEnd.SelectedDate = DateTime.Today;
        }

        private void BtnQuery_Click(object sender, RoutedEventArgs e) => LoadImages();

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { }

        private void LoadImages()
        {
            _allRecords.Clear();
            lstImages.ItemsSource = null;

            if (!Directory.Exists(_imageRoot)) { txtStatus.Text = "存图目录不存在"; return; }

            var start = dpStart.SelectedDate ?? DateTime.Today.AddDays(-7);
            var end = (dpEnd.SelectedDate ?? DateTime.Today).AddDays(1);
            int camFilter = cmbCamera.SelectedIndex; // 0=全部, 1-6=相机
            bool? resultFilter = cmbResult.SelectedIndex == 0 ? (bool?)null : cmbResult.SelectedIndex == 1;

            try
            {
                for (DateTime d = start.Date; d < end.Date; d = d.AddDays(1))
                {
                    var dateDir = Path.Combine(_imageRoot, d.ToString("yyyyMMdd"));
                    if (!Directory.Exists(dateDir)) continue;

                    for (int cam = 1; cam <= 6; cam++)
                    {
                        if (camFilter > 0 && camFilter != cam) continue;
                        var camDir = Path.Combine(dateDir, $"Cam{cam}");
                        if (!Directory.Exists(camDir)) continue;

                        foreach (var resultDir in new[] { "OK", "NG" })
                        {
                            if (resultFilter.HasValue && resultFilter.Value != (resultDir == "OK")) continue;
                            var dir = Path.Combine(camDir, resultDir);
                            if (!Directory.Exists(dir)) continue;

                            foreach (var file in Directory.GetFiles(dir, "*.jpg").OrderByDescending(f => f))
                            {
                                var ts = Path.GetFileNameWithoutExtension(file);
                                _allRecords.Add(new ImageRecord
                                {
                                    FilePath = file,
                                    CameraName = $"Cam{cam}",
                                    Timestamp = ts,
                                    IsOK = resultDir == "OK",
                                });
                            }
                        }
                    }
                }

                // Load thumbnails async
                foreach (var r in _allRecords.Take(200))
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(r.FilePath);
                        bmp.DecodePixelWidth = 100;
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();
                        r.Thumbnail = bmp;
                    }
                    catch { }
                }

                lstImages.ItemsSource = _allRecords.Take(200).ToList();
                txtStatus.Text = $"共 {_allRecords.Count} 条记录，显示前 200 条";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"查询失败: {ex.Message}";
            }
        }

        private void LstImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstImages.SelectedItem is ImageRecord record)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(record.FilePath);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    imgPreview.Source = bmp;
                    txtNoImage.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    txtNoImage.Text = $"无法加载图像: {ex.Message}";
                    txtNoImage.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
