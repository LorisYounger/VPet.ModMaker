using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.Wpf;
using HKW.WPF.MVVMDialogs;
using VPet.ModMaker.Native;
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
        Register<ModMakerVM, ModMakerWindow>();
        Register<ModEditVM, ModEditWindow>();
        Register<I18nEditVM, I18nEditWindow>();
        Register<SaveTranslationModVM, SaveTranslationModWindow>();
        Register<FoodEditVM, FoodPage, FoodEditWindow>();
        Register<ClickTextEditVM, ClickTextPage, ClickTextEditWindow>();
        Register<LowTextEditVM, LowTextPage, LowTextEditWindow>();
        Register<SelectTextEditVM, SelectTextPage, SelectTextEditWindow>();
        Register<PetEditVM, PetPage, PetEditWindow>();
        Register<MoveEditVM, MovePage, MoveEditWindow>();
        Register<WorkEditVM, WorkPage, WorkEditWindow>();
        Register<AnimeEditVM, AnimeEditWindow>();
        Register<FoodAnimeEditVM, FoodAnimeEditWindow>();
        RegisterPage<AnimeVM, AnimePage>();
        Register<SelectGraphTypeVM, SelectGraphTypeWindow>();

        this.RegisterAllDialogX();
        this.RegisterAddCultureDialog();
    }
}
