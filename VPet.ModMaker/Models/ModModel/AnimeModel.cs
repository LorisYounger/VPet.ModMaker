using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.GraphInfo;

namespace VPet.ModMaker.Models.ModModel;

public class AnimeModel
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<GraphInfo.AnimatType> AnimeType { get; } = new();

    public ObservableCollection<ObservableCollection<ImageModel>> MultiHappyImageModels = new();
    public ObservableCollection<ObservableCollection<ImageModel>> MultiNomalImageModels = new();
    public ObservableCollection<ObservableCollection<ImageModel>> MultiPoorConditionImageModels =
        new();
    public ObservableCollection<ObservableCollection<ImageModel>> MultiIllImageModels = new();

    public AnimeModel() { }

    public AnimeModel(string name, AnimatType animatType, IList<IGraph> graphList)
    {
        Id.Value = name;
        AnimeType.Value = animatType;
        foreach (IGraph graph in graphList)
        {
            //if(graph.)
        }
    }
}
