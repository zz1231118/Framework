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
using Framework.JavaScript;
using Framework.JavaScript.Converters;
using Framework.Log;
using Framework.Net.Remoting;

namespace ConsoleApp1
{
    class Program
    {
        public class AbstractEntity
        {
            [JsonMember]
            public virtual long ID { get; private set; }
        }

        public class GameObject : AbstractEntity
        {
            private long key;

            [JsonMember]
            public new long ID { get => key; set => key = value; }

            [JsonMember]
            public string Name { get; private set; }
        }

        static void Main(string[] args)
        {
            TestExpression2();
            //TestDbContext();
            //TestLogger();
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

            var key = 3;
            var name = "ss";
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
            commandStruct.Where(p => (p.ID == 0 || fields.Contains(p.ID)) && !p.Deleted);
            commandStruct.OrderBy(p => p.ID);

            var commandText = commandStruct.CommandText;
        }

        static void TestLogger()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider<FileLoggerProvider>();
            Logger.LoggerFactory = loggerFactory;

            var logger = Logger.GetLogger<Program>();
            logger.Error("hello world!");
            Logger.Shutdown();
        }

        static void TestJson()
        {
            var text = "{\"ID\":22,\"Name\":\"sssa\"}";
            var json = Json.Parse(text);
            var so = JsonSerializer.Deserialize<GameObject>(json);
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
            var dbConnectionBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            dbConnectionBuilder.DataSource = "115.159.55.137,1999";
            dbConnectionBuilder.UserID = "chenguangxu";
            dbConnectionBuilder.Password = "4572613..cgx";
            dbConnectionBuilder.InitialCatalog = "Manager.Account";
            DbConnectionManager.Register(dbConnectionBuilder.InitialCatalog, new MsSqlConnectionProvider(10, dbConnectionBuilder.ConnectionString));
            EntitySchemaManager.LoadEntity(typeof(Entity));
            using (var dbContext = new DbContext())
            {
                var entities = new List<Entity>();
                for (int i = 0; i < 1000; i++)
                {
                    var entity = RowAdapter.Create<Entity>();
                    entity.Name = (i + 3).ToString();
                    entities.Add(entity);
                }

                var dbSet = dbContext.Set<Entity>();
                dbSet.RemoveRange(entities);
                dbContext.SaveChanges();
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
    }

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

        [EntityColumn(ConvertType = typeof(JsonConverter<JsonCollectionConverter<Permission>>))]
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
