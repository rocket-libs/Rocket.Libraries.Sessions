﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rocket.Libraries.Sessions.Models;
using Rocket.Libraries.Sessions.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocket.Libraries.Sessions.Services
{
    public class Impersonator : ISessionReader
    {
        public bool UseThisReader(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            return CurrentPathIsInImpersonationList(httpContext, sessionManagerSettings);
        }

        public async Task<string> ReadAsync(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            return await Task.Run(() => Read(httpContext, sessionManagerSettings));
        }

        private string Read(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            var currentPath = httpContext.Request.Path.Value;
            var pathImpersonationEntry = GetCurrentPathImpersonationEntry(httpContext, sessionManagerSettings);
            if (pathImpersonationEntry != null)
            {
                var hasImpersonationSessions = sessionManagerSettings.ImpersonationInformation?.ImpersonationSessions?.Count > 0;
                if (hasImpersonationSessions == false)
                {
                    throw new Exception($"Impersonation requested but no impersonation sessions specified");
                }
                else
                {
                    var candidateSessions = sessionManagerSettings.ImpersonationInformation.ImpersonationSessions.Where(a => a.Key.Equals(pathImpersonationEntry.ImpersonationSessionKey)).ToList();
                    if (candidateSessions.Count > 1)
                    {
                        throw new Exception($"{candidateSessions.Count} impersonation sessions with the key '{pathImpersonationEntry.ImpersonationSessionKey}' were found while only one is expected.");
                    }
                    else if (candidateSessions.Count == 0)
                    {
                        throw new Exception($"Could not find an impersonation session with the key '{pathImpersonationEntry.ImpersonationSessionKey}'");
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(candidateSessions.Single());
                    }
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private bool CurrentPathIsInImpersonationList(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            var pathImpersonationEntry = GetCurrentPathImpersonationEntry(httpContext, sessionManagerSettings);
            return pathImpersonationEntry != null;
        }

        private EndpointPath GetCurrentPathImpersonationEntry(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings)
        {
            var hasImpersonationList = sessionManagerSettings.ImpersonationInformation?.EndpointPaths?.Count > 0;
            if (hasImpersonationList && httpContext.Request.Path.HasValue)
            {
                var currentPath = httpContext.Request.Path.Value;
                var matches = GetImpersonationMatches(httpContext, sessionManagerSettings, currentPath);
                if (matches.Count > 1)
                {
                    throw new Exception($"Found {matches.Count} impersonation entries for '{currentPath}'. Only one entry per path is allowed");
                }
                else if (matches.Count == 1)
                {
                    return matches.Single();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private List<EndpointPath> GetImpersonationMatches(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings, string currentPath)
        {
            var exactMatches = sessionManagerSettings.ImpersonationInformation.EndpointPaths.Where(a => a.Path.Equals(currentPath, StringComparison.InvariantCultureIgnoreCase)).ToList();
            var hasExactMatches = exactMatches.Count > 0;
            if (hasExactMatches)
            {
                return exactMatches;
            }
            else
            {
                var wildCardMatches = GetWildCardImpersonationMatchesIfRequired(httpContext, sessionManagerSettings, currentPath).ToList();
                return wildCardMatches;
            }
        }

        private IEnumerable<EndpointPath> GetWildCardImpersonationMatchesIfRequired(HttpContext httpContext, SessionsMiddlewareSettings sessionManagerSettings, string currentPath)
        {
            const string wildCardSpecifier = "/*";

            var wildCardImpersonations = sessionManagerSettings.ImpersonationInformation.EndpointPaths.Where(a => a.Path.EndsWith(wildCardSpecifier)).ToList();
            foreach (var item in wildCardImpersonations)
            {
                var nonWildCardPart = item.Path.Substring(0, wildCardSpecifier.Length);
                var canBeTested = currentPath.Length >= nonWildCardPart.Length;
                if (canBeTested)
                {
                    var isMatch = currentPath.Substring(0, nonWildCardPart.Length).Equals(nonWildCardPart, StringComparison.InvariantCultureIgnoreCase);
                    if (isMatch)
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}