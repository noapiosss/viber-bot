using System.Collections.Generic;
using Web.Services.Interfaces;
using Web.Services.Models;

namespace Web.Services
{

    public class UsersInfo : IUsersInfo
    {
        //add db tables for this
        private readonly Dictionary<string, string> _usersImeis;
        private readonly Dictionary<string, Stage> _usersNavigation;

        public UsersInfo()
        {
            _usersImeis = new();
            _usersNavigation = new();
        }

        public bool TryAddImei(string userId, string imei)
        {
            return _usersImeis.TryAdd(userId, imei);
        }

        public void SetImei(string userId, string iemi)
        {
            if (!_usersImeis.ContainsKey(userId))
            {
                _usersImeis.Add(userId, iemi);
                return;
            }

            _usersImeis[userId] = iemi;
        }

        public bool Contains(string userId)
        {
            return _usersImeis.ContainsKey(userId);
        }

        public bool TryGetImei(string userId, out string imei)
        {
            return _usersImeis.TryGetValue(userId, out imei);
        }

        public void SetStage(string userId, Stage stage)
        {
            if (!_usersNavigation.ContainsKey(userId))
            {
                _usersNavigation.Add(userId, stage);
                return;
            }

            _usersNavigation[userId] = stage;
        }

        public bool UserInAnyStage(string userId)
        {
            return _usersNavigation.ContainsKey(userId);
        }

        public Stage GetStage(string userId)
        {
            return _usersNavigation[userId];
        }
    }
}