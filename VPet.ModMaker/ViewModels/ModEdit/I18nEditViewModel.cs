using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// I18n编辑
/// </summary>
public partial class I18nEditViewModel : ViewModelBase, IModalDialogViewModel, IViewClosed
{
    /// <inheritdoc/>
    public I18nEditViewModel()
    {
        this.WhenAnyValue(x => x.Search, x => x.SearchTargets.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => I18nDatas?.Refresh());

        I18nDatas = new([], [], DataFilter);
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (SearchTargets.Count > 0)
        {
            SearchTargets.Clear();
            SearchTargets.Add("ID");
            SearchTargets.SelectedItem = SearchTargets[0];
        }

        if (oldValue is not null)
        {
            I18nDatas.Clear();
        }

        if (newValue is not null)
        {
            foreach (var value in ModInfo.I18nResource.DatasByKey.Values)
            {
                I18nDatas.Add(value);
            }
            foreach (var culture in ModInfo.I18nResource.Cultures)
            {
                SearchTargets.Add(culture.Name);
            }
        }
    }

    /// <summary>
    /// 单元格编辑
    /// </summary>
    public bool CellEdit { get; set; } = false;

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;
    public bool? DialogResult { get; set; }

    /// <summary>
    /// 全部I18n资源
    /// <para>
    /// (ID, (CultureName, Value))
    /// </para>
    /// </summary>
    public FilteredListWrapper<
        ObservableCultureDataList<string, string>,
        List<ObservableCultureDataList<string, string>>,
        ObservableList<ObservableCultureDataList<string, string>>
    > I18nDatas { get; } = null!;

    /// <summary>
    /// 搜索目标列表
    /// </summary>
    public ObservableSelectableList<string> SearchTargets { get; } = ["ID"];

    private bool DataFilter(ObservableCultureDataList<string, string> item)
    {
        if (SearchTargets.SelectedItem == nameof(ModInfoModel.ID))
        {
            // 如果是ID则搜索ID
            return item.Key.Contains(Search, StringComparison.CurrentCultureIgnoreCase);
        }
        else
        {
            // 如果是I18n数据则搜索对应文化
            if (
                ModInfo.I18nResource.IndexByCulture.TryGetValue(
                    CultureInfo.GetCultureInfo(SearchTargets.SelectedItem),
                    out var index
                )
            )
            {
                return item[index]?.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
                    is true;
            }
            return false;
        }
    }

    #region Event

    public void OnClosed()
    {
        ModInfo = null!;
    }
    #endregion
}
