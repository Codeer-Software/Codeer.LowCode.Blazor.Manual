﻿using Microsoft.AspNetCore.Components;
using Codeer.LowCode.Blazor.RequestInterfaces;
using WebApp.Client.Shared.Services;

namespace WebApp.Services
{
    public class NavigationService : NavigationServiceBase
    {
        public NavigationService(NavigationManager nav, IAppInfoService app) : base(nav, app) { }
        public override bool CanLogout => false;
        public override async Task Logout() => await Task.CompletedTask;
    }
}
