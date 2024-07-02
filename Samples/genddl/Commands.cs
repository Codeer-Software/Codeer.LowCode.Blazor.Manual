using System.Diagnostics;

namespace genddl
{
    internal class Commands
    {
        public static void PrintHelp()
        {

            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} [options] filename.xlsx");
            Console.WriteLine($"  --db <DbType>                  Set database type, default is sqlserver.");
            Console.WriteLine($"      sqlserver,mssql   Microsoft SQL Server");
            Console.WriteLine($"      sqlite            SQLite");
            Console.WriteLine($"      postgresql,pgsql  PostgreSQL");
            Console.WriteLine($"      oracle            Oracle");
            Console.WriteLine($"  --output <OutputDirectory>     Set output directory.");
            Console.WriteLine($"  --ddl <DDLFileName>            Set output ddl file name.");
            Console.WriteLine($"  --datasource <DataSourceName>  Set DataSource name.");
            Console.WriteLine($"  --help                         Print this help.");
        }
    }
}
