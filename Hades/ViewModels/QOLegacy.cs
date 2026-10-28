using Hades.Formats;

namespace Hades.ViewModels;

public class QOLegacyViewModel(IRandomizerFormat format) : FormatViewModelBase(format)
{
    // Pull all default option values, only BetterStatingStats in this case
    private static readonly QOLegacyOptions Defaults = new();
    private bool _betterStartingStats = Defaults.BetterStartingStats;
    public bool BetterStartingStats
    {
        get => _betterStartingStats;
        set => SetProperty(ref _betterStartingStats, value);
    }
    protected override IRandomizerOptions Options => new QOLegacyOptions
    {
        BetterStartingStats = BetterStartingStats
    };
}
