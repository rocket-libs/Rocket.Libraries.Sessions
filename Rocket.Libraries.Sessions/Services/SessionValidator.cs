using Rocket.Libraries.Sessions.Models;
using System;

namespace Rocket.Libraries.Sessions.Services
{
    internal class SessionValidator
    {
        public bool IsValidSession(string sessionKey, SessionInformation sessionInformation)
        {
            var sessionInformationIsNull = sessionInformation == null;
            if (sessionInformationIsNull)
            {
                LogUtility.Debug($"Could not find session with key '{sessionKey}'");
                return false;
            }
            else
            {
                var sessionDataMissing = (string.IsNullOrEmpty(sessionInformation.Value) || string.IsNullOrEmpty(sessionInformation.Key));
                if (sessionDataMissing)
                {
                    LogUtility.Debug($"Session data is incomplete");
                    return false;
                }
                else
                {
                    var sessionKeyMismatch = sessionDataMissing == false && sessionInformation.Key.Equals(sessionKey, StringComparison.InvariantCultureIgnoreCase) == false;
                    if (sessionKeyMismatch)
                    {
                        LogUtility.Debug($"Supplied session key does not match with key from server");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}