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
        Register<ModMakerVM, ModMakerWindow>();
        Register<ModEditVM, ModEditWindow>();
        Register<AddCultureVM, AddCultureWindow>();
        Register<I18nEditVM, I18nEditWindow>();
        Register<SaveTranslationModVM, SaveTranslationModWindow>();
        Register<FoodEditVM, FoodPage, FoodEditWindow>();
        Register<ClickTextEditVM, ClickTextPage, ClickTextEditWindow>();
        Register<LowTextEditVM, LowTextPage, LowTextEditWindow>();
        Register<SelectTextEditVM, SelectTextPage, SelectTextEditWindow>();
        Register<PetEditVM, PetPage, PetEditWindow>();
        Register<MoveEditVM, MovePage, MoveEditWindow>();
        Register<WorkEditVM, WorkPage, WorkEditWindow>();
        RegisterPage<AnimeVM, AnimePage>();
        Register<AnimeEditVM, AnimeEditWindow>();
        Register<FoodAnimeEditVM, FoodAnimeEditWindow>();
        Register<SelectGraphTypeVM, SelectGraphTypeWindow>();

        this.RegisterAllDialogX();
    }
}
