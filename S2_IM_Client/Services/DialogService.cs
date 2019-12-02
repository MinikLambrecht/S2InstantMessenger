using System;
using Microsoft.Win32;
using System.Windows;

namespace S2_IM_Client.Services
{
    public class DialogService : IDialogService
    {
        public string OpenFile(string caption, string filter = "All files (*.*)|*.*")
        {
            var dialog = new OpenFileDialog
                         {
                             InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                             Title = caption,
                             Filter = filter,
                             CheckFileExists = true,
                             CheckPathExists = true,
                             RestoreDirectory = true
                         };

            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }

        public bool ShowConfirmationRequest(string message, string caption = "")
        {
            var result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel);

            return result.HasFlag(MessageBoxResult.OK);
        }

        public void ShowNotification(string message, string caption = "")
        {
            MessageBox.Show(message, caption);
        }
    }
}
