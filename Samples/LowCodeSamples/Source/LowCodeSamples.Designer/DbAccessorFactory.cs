using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.SystemSettings;
using Codeer.LowCode.Blazor.Extras.Server.Db;

namespace LowCodeSamples.Designer
{
  public class DbAccessorFactory : IDbAccessorFactory
  {
    public IDbAccessor Create(DataSource[] dataSources) => new DbAccessor(dataSources);
  }
}
