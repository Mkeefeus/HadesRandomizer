using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Hades.ViewModels;

public abstract class FormatViewModelBase : ObservableObject
{
    public string Title { get; set; } = "";
    private string _seed = "";
    public string Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            OnPropertyChanged();
        }
    }
    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public IAsyncRelayCommand RandomizeCommand { get; set; } = null!;
    public IRelayCommand LaunchCommand { get; set; } = null!;

    private bool _isBusy;

    public bool IsBusy 
    { 
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }
}
