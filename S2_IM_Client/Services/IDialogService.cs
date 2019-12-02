namespace S2_IM_Client.Services
{
    public interface IDialogService
    {
        void ShowNotification(string message, string caption = "");
        bool ShowConfirmationRequest(string message, string caption = "");
        string OpenFile(string caption, string filter = @"All files (*.*)|*.*");
    }
}