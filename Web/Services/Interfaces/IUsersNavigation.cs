using Web.Services.Models;

namespace Web.Services.Interfaces
{
    public interface IUsersInfo
    {
        bool TryAddImei(string userId, string imei);
        void SetImei(string userId, string iemi);
        bool Contains(string userId);
        bool TryGetImei(string userId, out string imei);
        void SetStage(string userId, Stage stage);
        bool UserInAnyStage(string userId);
        Stage GetStage(string userId);
    }
}