using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.SystemSettings;
using CustomLayoutSample.Server.Shared;

namespace CustomLayoutSample.Designer
{
    public class DbAccessorFactory : IDbAccessorFactory
    {
        public IDbAccessor Create(DataSource[] dataSources) => new DbAccessor(dataSources);
    }
}
