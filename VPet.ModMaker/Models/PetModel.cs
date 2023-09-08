using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

public class PetModel : I18nModel<I18nPetInfoModel>
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<string> PetNameId { get; } = new();
    public ObservableValue<string> DescriptionId { get; } = new();
    public ObservableValue<ObservableRect<double>> TouchHeadRect { get; } = new(new());
    public ObservableValue<ObservableMultiStateRect> TouchRaisedRect { get; } = new(new());
    public ObservableValue<ObservableMultiStatePoint> RaisePoint { get; } = new(new());

    public ObservableCollection<WorkModel> Works { get; } = new();

    public PetModel()
    {
        PetNameId.Value = $"{Id.Value}_{nameof(PetNameId)}";
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
        Id.ValueChanged += (v) =>
        {
            PetNameId.Value = $"{v}_{nameof(PetNameId)}";
            DescriptionId.Value = $"{v}_{nameof(DescriptionId)}";
        };
    }

    public PetModel(PetModel model)
        : this()
    {
        Id.Value = model.Id.Value;
        TouchHeadRect.Value = model.TouchHeadRect.Value.Copy();
        TouchRaisedRect.Value = model.TouchRaisedRect.Value.Copy();
        RaisePoint.Value = model.RaisePoint.Value.Copy();
        foreach (var work in model.Works)
            Works.Add(work);

        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public PetModel(PetLoader loader)
        : this()
    {
        Id.Value = loader.Name;
        PetNameId.Value = loader.PetName;
        DescriptionId.Value = loader.Intor;

        TouchHeadRect.Value.SetValue(
            loader.Config.TouchHeadLocate.X,
            loader.Config.TouchHeadLocate.Y,
            loader.Config.TouchHeadSize.Width,
            loader.Config.TouchHeadSize.Height
        );

        TouchRaisedRect.Value.Happy.Value.SetValue(
            loader.Config.TouchRaisedLocate[0].X,
            loader.Config.TouchRaisedLocate[0].Y,
            loader.Config.TouchRaisedSize[0].Width,
            loader.Config.TouchRaisedSize[0].Height
        );
        TouchRaisedRect.Value.Nomal.Value.SetValue(
            loader.Config.TouchRaisedLocate[1].X,
            loader.Config.TouchRaisedLocate[1].Y,
            loader.Config.TouchRaisedSize[1].Width,
            loader.Config.TouchRaisedSize[1].Height
        );
        TouchRaisedRect.Value.PoorCondition.Value.SetValue(
            loader.Config.TouchRaisedLocate[2].X,
            loader.Config.TouchRaisedLocate[2].Y,
            loader.Config.TouchRaisedSize[2].Width,
            loader.Config.TouchRaisedSize[2].Height
        );
        TouchRaisedRect.Value.Ill.Value.SetValue(
            loader.Config.TouchRaisedLocate[3].X,
            loader.Config.TouchRaisedLocate[3].Y,
            loader.Config.TouchRaisedSize[3].Width,
            loader.Config.TouchRaisedSize[3].Height
        );

        RaisePoint.Value.Happy.Value.SetValue(
            loader.Config.RaisePoint[0].X,
            loader.Config.RaisePoint[0].Y
        );
        RaisePoint.Value.Nomal.Value.SetValue(
            loader.Config.RaisePoint[1].X,
            loader.Config.RaisePoint[1].Y
        );
        RaisePoint.Value.PoorCondition.Value.SetValue(
            loader.Config.RaisePoint[2].X,
            loader.Config.RaisePoint[2].Y
        );
        RaisePoint.Value.Ill.Value.SetValue(
            loader.Config.RaisePoint[3].X,
            loader.Config.RaisePoint[3].Y
        );

        foreach (var work in loader.Config.Works)
            Works.Add(new(work));
    }

    public void Close() { }
}

public class I18nPetInfoModel
{
    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> PetName { get; } = new();
    public ObservableValue<string> Description { get; } = new();

    public I18nPetInfoModel Copy()
    {
        var result = new I18nPetInfoModel();
        result.Name.Value = Name.Value;
        result.PetName.Value = PetName.Value;
        result.Description.Value = Description.Value;
        return result;
    }
}

public class ObservableMultiStateRect
{
    public ObservableValue<ObservableRect<double>> Happy { get; } = new(new());
    public ObservableValue<ObservableRect<double>> Nomal { get; } = new(new());
    public ObservableValue<ObservableRect<double>> PoorCondition { get; } = new(new());
    public ObservableValue<ObservableRect<double>> Ill { get; } = new(new());

    public ObservableMultiStateRect Copy()
    {
        var result = new ObservableMultiStateRect();
        result.Happy.Value = Happy.Value.Copy();
        result.Nomal.Value = Nomal.Value.Copy();
        result.PoorCondition.Value = PoorCondition.Value.Copy();
        result.Ill.Value = Ill.Value.Copy();
        return result;
    }
}

public class ObservableMultiStatePoint
{
    public ObservableValue<ObservablePoint<double>> Happy { get; } = new(new());
    public ObservableValue<ObservablePoint<double>> Nomal { get; } = new(new());
    public ObservableValue<ObservablePoint<double>> PoorCondition { get; } = new(new());
    public ObservableValue<ObservablePoint<double>> Ill { get; } = new(new());

    public ObservableMultiStatePoint Copy()
    {
        var result = new ObservableMultiStatePoint();
        result.Happy.Value = Happy.Value.Copy();
        result.Nomal.Value = Nomal.Value.Copy();
        result.PoorCondition.Value = PoorCondition.Value.Copy();
        result.Ill.Value = Ill.Value.Copy();
        return result;
    }
}

public class ObservableRect<T>
{
    public ObservableValue<T> X { get; } = new();
    public ObservableValue<T> Y { get; } = new();
    public ObservableValue<T> Width { get; } = new();
    public ObservableValue<T> Height { get; } = new();

    public void SetValue(T x, T y, T width, T hetght)
    {
        X.Value = x;
        Y.Value = y;
        Width.Value = width;
        Height.Value = hetght;
    }

    public ObservableRect<T> Copy()
    {
        var result = new ObservableRect<T>();
        result.X.Value = X.Value;
        result.Y.Value = Y.Value;
        result.Width.Value = Width.Value;
        result.Height.Value = Height.Value;
        return result;
    }
}

public class ObservablePoint<T>
{
    public ObservableValue<T> X { get; } = new();
    public ObservableValue<T> Y { get; } = new();

    public void SetValue(T x, T y)
    {
        X.Value = x;
        Y.Value = y;
    }

    public ObservablePoint<T> Copy()
    {
        var result = new ObservablePoint<T>();
        result.X.Value = X.Value;
        result.Y.Value = Y.Value;
        return result;
    }
}
