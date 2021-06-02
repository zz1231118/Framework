using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Framework.Data;
using Framework.Data.Converters;
using Framework.Data.Entry;
using Framework.Data.Expressions;
using Framework.Data.MsSql;
using Framework.Injection;
using Framework.JavaScript;
using Framework.JavaScript.Converters;
using Framework.Linq;
using Framework.Log;
using Framework.Net.Remoting;

namespace ConsoleApp1
{
    class Program
    {
        enum Gender : sbyte
        {
            None = -1,
            Man,
            Woman,
        }

        static void Main(string[] args)
        {
            var ob = new { ID = 1 };
            var type = ob.GetType();
            var json = Framework.JavaScript.JsonSerializer.Serialize(ob);
            var text = json.ToString();

            TestInjection();
            //TestDbContext();
            TestLogger();
            //TestDatabase();
            //TestJson();
            //TestRemote();
            Console.WriteLine("Hello World!");
        }

        static void TestExpression1()
        {
            EntitySchemaManager.LoadEntity<SceneObject>();
            var view = EntitySchemaManager.GetSchema<SceneObject>(true);
            var dbProvider = new Framework.Data.MsSql.MsSqlConnectionProvider(1, "");
            var commandStruct = dbProvider.CreateCommand<SceneObject>(view.Name, DbCommandMode.Select);
            var fields = new List<long>() { };

            var guid = Guid.NewGuid();
            commandStruct.Where(p => (p.ID == 0 || fields.Contains(p.ID)) && !p.Deleted);
            commandStruct.OrderBy(p => p.Guid, true);
            //commandStruct.Where(p => fields.Contains(p.ID) && !p.Deleted);
            var commandText = commandStruct.CommandText;
        }

        static void TestExpression2()
        {
            EntitySchemaManager.LoadEntity<SceneObject>();
            var view = EntitySchemaManager.GetSchema<SceneObject>(true);
            var dbProvider = new Framework.Data.MsSql.MsSqlConnectionProvider(1, "");
            var commandStruct = dbProvider.CreateCommand<SceneObject>(view.Name, DbCommandMode.Select);
            commandStruct.Columns.Add(SqlExpression.As(SqlExpression.Function("count", SqlExpression.Symbol("*")), "Count"));
            var fields = new List<long>() { 4, 3 };
            commandStruct.Where(p => (p.Function == null || fields.Contains(p.ID)) && !p.Deleted);
            commandStruct.OrderBy(p => p.ID);

            var commandText = commandStruct.CommandText;
        }

        static void TestExpression3()
        {
            var sb = new System.Data.SqlClient.SqlConnectionStringBuilder();
            sb.DataSource = "115.159.55.137,1999";
            sb.UserID = "chenguangxu";
            sb.Password = "4572613";
            sb.InitialCatalog = "BlingAccount";
            EntitySchemaManager.LoadEntity<SceneObject>();
            var view = EntitySchemaManager.GetSchema<SceneObject>(true);
            var dbProvider = new Framework.Data.MsSql.MsSqlConnectionProvider(1, sb.ConnectionString);
            var commandStruct = dbProvider.CreateCommand<SceneObject>(view.Name, DbCommandMode.Select);
            var keys = new List<long>() { 1, 5, 7, 9 };
            commandStruct.Where(p => p.Name != null && keys.Contains(p.ID));
            commandStruct.OrderBy(p => p.ID);
            var a = dbProvider.Select(commandStruct);

        }

        static void TestLogger()
        {
            Logger.AddFactory<FileLoggerFactory>();
            var logger = Logger.GetLogger<Program>();
            logger.Error("hello world!");
        }

        static void TestDatabase()
        {
            var dbConnectionBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            dbConnectionBuilder.DataSource = "115.159.55.137,1999";
            dbConnectionBuilder.UserID = "chenguangxu";
            dbConnectionBuilder.Password = "4572613..cgx";
            dbConnectionBuilder.InitialCatalog = "Manager.Account";
            DbConnectionManager.Register(dbConnectionBuilder.InitialCatalog, new MsSqlConnectionProvider(10, dbConnectionBuilder.ConnectionString));
            EntitySchemaManager.LoadEntity(typeof(Entity));
            foreach (var view in EntitySchemaManager.Schemas)
            {
                EntityUtils.CheckSchema(view);
            }
        }

        static void TestDbContext()
        {
            try
            {
                WriteToDatabase();
            }
            catch (Exception ex)
            {
                var a = ex;
            }

            void WriteToDatabase()
            {
                var dbConnectionBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                dbConnectionBuilder.DataSource = "115.159.55.137,1999";
                dbConnectionBuilder.UserID = "chenguangxu";
                dbConnectionBuilder.Password = "4572613";
                dbConnectionBuilder.InitialCatalog = "Manager.Account";
                dbConnectionBuilder.ConnectTimeout = 10;

                DbConnectionManager.Register(dbConnectionBuilder.InitialCatalog, new MsSqlConnectionProvider(10, dbConnectionBuilder.ConnectionString));
                EntitySchemaManager.LoadEntity(typeof(Entity));
                var option = new DbContextOptions();
                option.ExceptionHandling = ExceptionHandling.Skip;
                using (var dbContext = new DbContext(option))
                {
                    var dbSet = dbContext.Set<Entity>();
                    dbSet.Add(new Entity() { ID = 2, Name = "a2" });
                    dbSet.Add(new Entity() { ID = 3, Name = "a3" });
                    try
                    {
                        dbContext.SaveChanges();
                    }
                    finally
                    {
                        foreach (var row in dbSet.RowEntries)
                        {
                            var a = row.State;
                        }
                    }
                }
            }
        }

        static void TestRemote()
        {
            var setting = new ServiceSetting(new ServiceEndpoint(46000, 10, 100));
            var serviceHost = new ServiceHost(typeof(GameCommand), setting);
            serviceHost.Open();

            var addresses = Dns.GetHostAddresses(Dns.GetHostName());
            var address = addresses.First(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var endpoint = new IPEndPoint(address, 46000);
            var service = ClientBase.Create<IGameCommand>(new ClientEndpoint(endpoint));
            var node = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                service.Say("ss");
            }

            var duration = DateTime.Now - node;
        }

        static void TestInjection()
        {
            var builder = new ContainerBuilder();
            builder.EnableAutowired();
            builder.AddSingleton<Options, GatewayOptions>();
            builder.AddSingleton<SystemTarget, Gateway>();
            builder.AddSingleton<SystemTarget, Cluster>();
            var container = builder.Build();
            var gateway = container.Required<SystemTarget>();
            var a = gateway;
        }

        public abstract class Options
        {
            public string Address { get; set; }
        }

        public class GatewayOptions : Options
        { }

        public abstract class SystemTarget
        {
            [Autowired]
            private Options options;
        }

        public class Gateway : SystemTarget
        { }

        public class Cluster : Gateway
        { }
    }

    public class Function
    { }

    [EntityTable]
    class SceneObject
    {
        [EntityColumn]
        public long ID { get; set; }

        [EntityColumn]
        public string Name { get; set; }

        [EntityColumn]
        public bool Deleted { get; set; }

        [EntityColumn]
        public Guid Guid { get; set; }
    
        [EntityColumn(ConverterType = typeof(JsonEntityConverter<Function>))]
        public Function Function { get; set; }
    }

    [EntityTable(ConnectKey = "Game")]
    public class ForbiddenGameAccount : RowAdapter
    {
        [EntityColumn]
        public long GameID { get; set; }

        [EntityColumn(DbType = DbType.AnsiString, MaxLength = 20)]
        public string Account { get; set; }

        [EntityColumn]
        public bool Deleted { get; set; }
    }

    [EntityTable(ConnectKey = "Manager.Account")]
    public class Entity : RowAdapter
    {
        [EntityColumn(MaxLength = 50)]
        public string Name { get; set; }

        [EntityColumn(ConverterType = typeof(JsonConverter<JsonCollectionConverter<Permission>>))]
        public List<Permission> Permissions { get; set; }

        public class Permission
        {
            [JsonMember]
            public string Name { get; set; }
        }
    }

    [ServiceContract]
    public interface IGameCommand
    {
        [OperationContract]
        string Say(string name);
    }

    [ServiceContract]
    public class GameCommand : IGameCommand
    {
        [OperationContract]
        public string Say(string name)
        {
            return $"hello: {name}";
        }
    }
}
