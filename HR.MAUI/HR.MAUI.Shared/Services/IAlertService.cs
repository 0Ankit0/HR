namespace HR.MAUI.Shared.Services
{
    public interface IAlertService
    {
        Task ShowAlert(string message, string title = "Alert");
        Task<bool> ShowConfirm(string message, string title = "Confirm");
    }
}
