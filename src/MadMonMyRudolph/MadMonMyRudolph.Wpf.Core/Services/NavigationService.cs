using MadMonMyRudolph.Wpf.Abstractions;

namespace MadMonMyRudolph.Wpf.Core.Services;

public class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Frame? _navigationFrame;
    private ObservableObject? _currentViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ObservableObject? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public void SetNavigationFrame(Frame frame)
    {
        _navigationFrame = frame;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as ObservableObject;
        if (viewModel == null) return;

        CurrentViewModel = viewModel;

        if (_navigationFrame == null) return;

        // ViewModel에 해당하는 View 찾기
        var viewTypeName = viewModel.GetType().Name.Replace("ViewModel", "View");
        var viewType = Type.GetType($"MadMonMyRudolph.Wpf.Views.{viewTypeName}");

        if (viewType != null && _serviceProvider.GetService(viewType) is UserControl view)
        {
            view.DataContext = viewModel;
            _navigationFrame.Content = view;
        }
    }
}