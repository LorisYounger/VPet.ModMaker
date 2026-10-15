using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.Mapper;
using HKW.MVVM;
using LinePutScript;
using Microsoft.Extensions.Logging;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

[MapTarget(typeof(Theme), MapFields = true)]
[MapTarget(typeof(ThemeModel))]
public sealed partial class ThemeModel : ObservableModelBase, IDisposable
{
    public ThemeModel() { }

    public ThemeModel(ThemeModel theme)
    {
        this.MapFrom(theme);
    }

    public ThemeModel(Theme theme)
    {
        ID = theme.Name;
        this.MapFrom(theme);
    }

    [ObservableProperty]
    [MapProperty(typeof(Theme), Ignore = true)]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public required ObservableI18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        ObservableI18nResource<string, string> oldValue,
        ObservableI18nResource<string, string> newValue
    )
    {
        I18nObserver.Resource = newValue;
    }

    [MapIgnoreProperty]
    public I18nObserver<ThemeModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Name)),
        };

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    public string Name
    {
        get => I18nObserver.GetDataOrDefault(ID);
        set => I18nObserver.SetData(ID, value);
    }

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush Primary { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush PrimaryLight { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush PrimaryLighter { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush PrimaryDark { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush PrimaryDarker { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush PrimaryText { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush Secondary { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush SecondaryLight { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush SecondaryLighter { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush SecondaryDark { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush SecondaryDarker { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush SecondaryText { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush DARKPrimary { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush DARKPrimaryLight { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush DARKPrimaryLighter { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush DARKPrimaryDark { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush DARKPrimaryDarker { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush DARKPrimaryText { get; set; } = null!;

    [ObservableProperty]
    [MapProperty(typeof(Theme), nameof(Theme.ThemeColor), typeof(BrushToLPS))]
    [MapProperty(typeof(ThemeModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush ShadowColor { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        I18nObserver?.Dispose();
    }

    internal sealed class BrushToLPS : IMapConverter<SolidColorBrush, LinePutScript.LpsDocument>
    {
        public LpsDocument Convert(SolidColorBrush value, MapConvertArgs args)
        {
            if (args.Target is not Theme theme)
                throw new InvalidOperationException();
            theme.ThemeColor.Add(new Line(args.SourcePropertyName, value.ToString()[3..]));
            return theme.ThemeColor;
        }

        public SolidColorBrush ConvertBack(LpsDocument value, MapConvertArgs args)
        {
            if (args.Source is not ThemeModel model)
                throw new InvalidOperationException();
            try
            {
                var info = value[args.SourcePropertyName].Info;
                var color = string.Empty;
                if (info.Length == 6)
                    color = "#FF" + info;
                else if (info.Length == 8)
                    color = "#" + info;
                else
                    throw new InvalidOperationException();
                return new((Color)ColorConverter.ConvertFromString(color));
            }
            catch (Exception ex)
            {
                model
                    .LogIfEnabled(LogLevel.Warning)
                    ?.Log(
                        ex,
                        "主题 '{id}' 颜色数据 '{data}' 错误",
                        model.ID,
                        value[args.SourcePropertyName]
                    );
                return new();
            }
        }
    }
}
