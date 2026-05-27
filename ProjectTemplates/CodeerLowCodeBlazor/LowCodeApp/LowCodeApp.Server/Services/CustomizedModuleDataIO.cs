using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Extras.Services;
using Codeer.LowCode.Blazor.Repository.Data;

namespace LowCodeApp.Server.Services
{
    public class CustomizedModuleDataIO : ModuleDataIO
    {
        readonly DesignData _designData;

        public CustomizedModuleDataIO(DesignData designData, IAuthenticationContext authenticationContext, IDbAccessor dbAccess, ITemporaryFileManager temporaryFileManager)
            : base(designData, authenticationContext, dbAccess, temporaryFileManager)
        {
            _designData = designData;
        }

        protected override async Task<string> AddAsync(Guid transactionId, Guid moduleSubmitId, ModuleData data)
        {
            var moduleDesign = _designData.Modules.Find(data.Name);
            if (moduleDesign == null) throw LowCodeException.Create("invalid design");

            PasswordHashHelper.ApplyPasswordHash(moduleDesign, data);
            return await base.AddAsync(transactionId, moduleSubmitId, data);
        }

        protected async override Task UpdateAsync(Guid transactionId, Guid moduleSubmitId, ModuleData data)
        {
            var moduleDesign = _designData.Modules.Find(data.Name);
            if (moduleDesign == null) throw LowCodeException.Create("invalid design");

            PasswordHashHelper.ApplyPasswordHash(moduleDesign, data);
            await base.UpdateAsync(transactionId, moduleSubmitId, data);
        }
    }
}
