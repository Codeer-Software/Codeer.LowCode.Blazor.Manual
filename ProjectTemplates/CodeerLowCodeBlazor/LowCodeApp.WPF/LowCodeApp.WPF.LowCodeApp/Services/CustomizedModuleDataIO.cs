using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Extras.Services;
using Codeer.LowCode.Blazor.Repository.Data;
using LowCodeApp.WPF.LowCodeApp.Services.FileManagement;
using LowCodeApp.WPF.Server.Shared;

namespace LowCodeApp.WPF.LowCodeApp.Services
{
    public class CustomizedModuleDataIO : ModuleDataIO
    {
        readonly DesignData _designData;

        public DbAccessor DbAccess { get; }
        public TemporaryFileManager TemporaryFileManager { get; }

        public CustomizedModuleDataIO(DesignData designData, IAuthenticationContext authenticationContext, DbAccessor dbAccess, TemporaryFileManager temporaryFileManager)
            : base(designData, authenticationContext, dbAccess, temporaryFileManager)
        {
            _designData = designData;
            DbAccess = dbAccess;
            TemporaryFileManager = temporaryFileManager;
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
