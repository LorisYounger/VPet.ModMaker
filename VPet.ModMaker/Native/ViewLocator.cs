using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.Wpf;
using HKW.WPF.MVVMDialogs;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker;

/// <summary>
/// 视图定位器
/// </summary>
internal class ViewLocator : StrongViewLocatorX
{
    /// <inheritdoc/>
    public ViewLocator()
    {
        Register<ModMakerWindowVM, ModMakerWindow>();
        Register<ModEditWindowVM, ModEditWindow>();
        Register<I18nEditWindowVM, I18nEditWindow>();
        Register<SaveTranslationModWindowVM, SaveTranslationModWindow>();
        Register<FoodEditWindowVM, FoodEditWindow>();
        RegisterPage<FoodPageVM, FoodPage>();
        Register<ClickTextEditWindowVM, ClickTextEditWindow>();
        RegisterPage<ClickTextPageVM, ClickTextPage>();
        Register<LowTextEditWindowVM, LowTextEditWindow>();
        RegisterPage<LowTextPageVM, LowTextPage>();
        Register<SelectTextEditWindowVM, SelectTextEditWindow>();
        RegisterPage<SelectTextPageVM, SelectTextPage>();
        Register<PetEditWindowVM, PetEditWindow>();
        RegisterPage<PetPageVM, PetPage>();
        Register<MoveEditWindowVM, MoveEditWindow>();
        RegisterPage<MovePageVM, MovePage>();
        Register<WorkEditWindowVM, WorkEditWindow>();
        RegisterPage<WorkPageVM, WorkPage>();
        Register<AnimeEditWindowVM, AnimeEditWindow>();
        Register<FoodAnimeEditWindowVM, FoodAnimeEditWindow>();
        Register<AnimeEditWindowVM, AnimeEditWindow>();
        Register<SelectGraphTypeWindowVM, SelectGraphTypeWindow>();
        //RegisterPage<HouseVM, HousePage>();
        //RegisterPage<RoomVM, RoomPage>();

        this.RegisterAllDialogX();
        //this.RegisterLoadHouseTemplateDialog();
    }
}
