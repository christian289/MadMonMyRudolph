using MadMonMyRudolph.Abstractions.Interfaces;

namespace MadMonMyRudolph.Wpf.Core.Services;

/// <summary>
/// WPF 다이얼로그 서비스 구현
/// </summary>
public sealed class DialogService : IDialogService
{
    public string ShowOpenFileDialog(string filter, string title, bool isFolderPicker, bool isMultiSelect = false, string defaultDirectory = "")
    {
        var dialog = new CommonOpenFileDialog
        {
            Multiselect = isMultiSelect,
            IsFolderPicker = isFolderPicker,
            DefaultDirectory = string.IsNullOrWhiteSpace(defaultDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : defaultDirectory,
            Title = title
        };
        dialog.Filters.Clear();
        dialog.Filters.Add(new CommonFileDialogFilter("All Files", "*.*"));
        dialog.Filters.Add(new CommonFileDialogFilter("텍스트 파일", "*.txt"));

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            return dialog.FileName!;
        else
            return string.Empty;
    }

    public bool ShowSaveFileDialog(string filter, string title, string defaultFileName, string defaultDirectory = "")
    {
        var dialog = new CommonSaveFileDialog
        {
            InitialDirectory = string.IsNullOrWhiteSpace(defaultDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : defaultDirectory,
            DefaultExtension = Path.GetExtension(defaultFileName),
            Title = title,
            DefaultFileName = defaultFileName
        };
        dialog.Filters.Clear();
        dialog.Filters.Add(new CommonFileDialogFilter("Files", filter));
        dialog.Filters.Add(new CommonFileDialogFilter("텍스트 파일", "*.txt"));

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            // 파일 저장
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ShowMessage(string message, string title)
    {
        // 필요하다면 MesasgeBoxButton, MessageBoxImage를 다른 Enum으로 변환하여 Interface에서 추가하여 사용.
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
