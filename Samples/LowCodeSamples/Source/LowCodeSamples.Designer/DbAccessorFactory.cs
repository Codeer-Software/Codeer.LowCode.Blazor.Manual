using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeSamples.Server.Shared;

namespace LowCodeSamples.Designer
{
  public class DbAccessorFactory : IDbAccessorFactory
  {
    public IDbAccessor Create(DataSource[] dataSources) => new DbAccessor(dataSources);
  }
}
