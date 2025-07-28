namespace MadMonMyRudolph.Wpf.Abstractions;

public interface INavigationService
{
    ObservableObject? CurrentViewModel { get; }
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
    void SetNavigationFrame(Frame frame);
}