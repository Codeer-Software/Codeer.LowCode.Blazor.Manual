using Codeer.LowCode.Blazor.License;
using Microsoft.AspNetCore.Mvc;

namespace CDataSample.Server.Controllers
{
    [ApiController]
    [Route("api/license")]
    public class LicenseController : ControllerBase
    {
        static DateTime _checkedTime;

        [HttpPost("update_license")]
        public async Task UpdateLicense()
        {
            var now = DateTime.Now;
            if (now - _checkedTime < TimeSpan.FromMinutes(1)) return;
            _checkedTime = now;
            await LicenseManager.CheckServerLicense(Request);
        }
    }
}
