using System.Collections.ObjectModel;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class BrightnessViewModel : ViewModelBase
    {
        private ObservableCollection<BrightnessStageViewModel> _stages = new ObservableCollection<BrightnessStageViewModel>();

        public ObservableCollection<BrightnessStageViewModel> Stages
        {
            get => _stages;
            set => SetField(ref _stages, value);
        }

        public void LoadFromConfig(AppConfig config)
        {
            _stages.Clear();
            foreach (var s in config.BrightnessStages)
            {
                _stages.Add(new BrightnessStageViewModel
                {
                    StageName = s.StageName,
                    BackLight = s.BackLight,
                    RingLight = s.RingLight,
                    TopLight = s.TopLight
                });
            }
        }

        public void SaveToConfig(AppConfig config)
        {
            config.BrightnessStages.Clear();
            foreach (var vm in _stages)
            {
                config.BrightnessStages.Add(new BrightnessStage
                {
                    StageName = vm.StageName,
                    BackLight = vm.BackLight,
                    RingLight = vm.RingLight,
                    TopLight = vm.TopLight
                });
            }
        }
    }

    public class BrightnessStageViewModel : ViewModelBase
    {
        private string _stageName;
        private int _backLight;
        private int _ringLight;
        private int _topLight;

        public string StageName
        {
            get => _stageName;
            set => SetField(ref _stageName, value);
        }

        public int BackLight
        {
            get => _backLight;
            set => SetField(ref _backLight, value);
        }

        public int RingLight
        {
            get => _ringLight;
            set => SetField(ref _ringLight, value);
        }

        public int TopLight
        {
            get => _topLight;
            set => SetField(ref _topLight, value);
        }
    }
}
