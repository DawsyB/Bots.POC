namespace EmergencyServicesBot
{
    using Autofac;
    using System.Configuration;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.History;
    using System.Web.Http;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            //var builder = new ContainerBuilder();
            //RegisterDependencies(builder);                   
            //var container = builder.Build();
            var store = new TableBotDataStore(ConfigurationManager.AppSettings["StorageConnectionString"]);
            Conversation.UpdateContainer(
           builder =>
           {
               builder.Register(c => store)
                         .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                         .AsSelf()
                         .SingleInstance();

               builder.Register(c => new CachingBotDataStore(store,
                          CachingBotDataStoreConsistencyPolicy
                          .ETagBasedConsistency))
                          .As<IBotDataStore<BotData>>()
                          .AsSelf()
                          .InstancePerLifetimeScope();

               bool traceAllActivities = false;
               bool.TryParse(ConfigurationManager.AppSettings["TraceAllActivities"], out traceAllActivities);
               if (traceAllActivities)
               {
                   builder.RegisterType<TraceActivityLogger>().AsImplementedInterfaces().InstancePerDependency();
               }

               
           });



        }
    }
}
