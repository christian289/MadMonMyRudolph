namespace MadMonMyRudolph.Abstractions.Interfaces;

/// <summary>
/// 다이얼로그 서비스 인터페이스
/// </summary>
public interface IDialogService
{
    void ShowMessage(string message, string title);
    string ShowOpenFileDialog(string filter, string title, bool isFolderPicker, bool isMultiSelect = false, string defaultDirectory = "");
    bool ShowSaveFileDialog(string filter, string title, string defaultFileName, string defaultDirectory = "");
}
