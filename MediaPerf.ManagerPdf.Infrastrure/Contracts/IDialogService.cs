using System.Windows;

namespace MediaPerf.ManagerPdf.Infrastrure.Contracts
{
    public interface IDialogService
    {
        bool? ShowMessage(string message);
        bool? ShowMessage(string message, string title, MessageBoxButton yesNoCancel,
            MessageBoxImage question, MessageBoxResult oK);

        bool? ShowMessageNO(string message, string title, MessageBoxButton yesNoCancel,
            MessageBoxImage question, MessageBoxResult no);
    }
}
