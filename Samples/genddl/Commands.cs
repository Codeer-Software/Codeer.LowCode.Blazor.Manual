using System.Diagnostics;

namespace genddl
{
    internal class Commands
    {
        public static void PrintHelp()
        {

            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} [--db DbType] [--output OutputDirectory] [--ddl DDLFileName] filename.xlsx");
            Console.WriteLine($"  --db <DbType>                 Set database type, default is sqlserver.");
            Console.WriteLine($"      sqlserver,mssql   Microsoft SQL Server");
            Console.WriteLine($"      sqlite            SQLite");
            Console.WriteLine($"      postgresql,pgsql  PostgreSQL");
            Console.WriteLine($"      oracle            Oracle");
            Console.WriteLine($"  --output <OutputDirectory>    Set output directory to OutputDirectory");
            Console.WriteLine($"  --ddl <DDLFileName>           Set output ddl file name to DDLFileName");
            Console.WriteLine($"  --help                        Print this help.");
        }
    }
}
