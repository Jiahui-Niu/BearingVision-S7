using System.Collections.ObjectModel;
using System.Windows;

namespace WpfApp1.ViewModel
{
    public class CheckResultViewModel : ViewModelBase
    {
        private int _totalCount;
        private int _okCount;
        private int _ngCount;
        private string _passRate = "NaN";

        private int _defectConveyor;
        private int _defectNoRunner;
        private int _defectFlaw;
        private int _defectAi;

        public int TotalCount
        {
            get => _totalCount;
            set { SetField(ref _totalCount, value); UpdatePassRate(); }
        }

        public int OKCount
        {
            get => _okCount;
            set { SetField(ref _okCount, value); UpdatePassRate(); }
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

        public int DefectConveyor
        {
            get => _defectConveyor;
            set => SetField(ref _defectConveyor, value);
        }

        public int DefectNoRunner
        {
            get => _defectNoRunner;
            set => SetField(ref _defectNoRunner, value);
        }

        public int DefectFlaw
        {
            get => _defectFlaw;
            set => SetField(ref _defectFlaw, value);
        }

        public int DefectAi
        {
            get => _defectAi;
            set => SetField(ref _defectAi, value);
        }

        private void UpdatePassRate()
        {
            if (_totalCount > 0)
                PassRate = $"{(double)_okCount / _totalCount * 100:F2}%";
            else
                PassRate = "NaN";
        }

        public void Reset()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                TotalCount = 0;
                OKCount = 0;
                NGCount = 0;
                PassRate = "NaN";
                DefectConveyor = 0;
                DefectNoRunner = 0;
                DefectFlaw = 0;
                DefectAi = 0;
            });
        }

        public void AddResult(bool isOk)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                TotalCount++;
                if (isOk) OKCount++;
                else NGCount++;
            });
        }
    }
}
