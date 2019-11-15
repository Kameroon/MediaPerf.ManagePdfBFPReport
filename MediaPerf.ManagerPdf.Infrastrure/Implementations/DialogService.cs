using MediaPerf.ManagerPdf.Infrastrure.Contracts;
using System.Windows;

namespace MediaPerf.ManagerPdf.Infrastrure.Implementations
{
    public class DialogService : IDialogService
    {
        public bool? ShowMessage(string message)
        {
            return MessageBox.Show(message, "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information)
                == MessageBoxResult.OK;
        }

        public bool? ShowConfirmationMessage(string message)
        {
            return MessageBox.Show(message, "Confimation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Exclamation)
                == MessageBoxResult.OK;
        }

        public bool? ShowMessage(string message, string title, MessageBoxButton button,
            MessageBoxImage image, MessageBoxResult result)
        {
            return MessageBox.Show(message, title, button, image) == result;
        }

        public bool? ShowMessageNO(string message, string title, MessageBoxButton button,
            MessageBoxImage image, MessageBoxResult result)
        {
            return MessageBox.Show(message, title, button, image) == result;
        }
    }
}
