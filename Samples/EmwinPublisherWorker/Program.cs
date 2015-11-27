using System;
using Microsoft.WindowsAzure.ServiceRuntime;
using Newtonsoft.Json;

namespace EmwinPublisherWorker
{
    using Microsoft.Azure;
    using Topshelf;
    using Topshelf.HostConfigurators;
    using Topshelf.Logging;

    public class Program : TopshelfRoleEntryPoint
    {
        #region Private Fields

        private readonly LogWriter _log = HostLogger.Get<Program>();

        #endregion Private Fields

        #region Protected Methods

        /// <summary>
        /// Configures the specified host configurator.
        /// </summary>
        /// <param name="hostConfigurator">The host configurator.</param>
        protected override void Configure(HostConfigurator hostConfigurator)
        {
            var configuration = new Configuration
            {
                EventHubConnectionString = CloudConfigurationManager.GetSetting(nameof(Configuration.EventHubConnectionString)),
                StorageConnectionString = CloudConfigurationManager.GetSetting(nameof(Configuration.StorageConnectionString)),
                EventHubName = CloudConfigurationManager.GetSetting(nameof(Configuration.EventHubName)),
                Email = CloudConfigurationManager.GetSetting(nameof(Configuration.Email)),
                Identifier = RoleEnvironment.IsAvailable ? RoleEnvironment.CurrentRoleInstance.Id : Environment.MachineName
            };

            _log.InfoFormat("Service Configuration: {0}", JsonConvert.SerializeObject(configuration));

            hostConfigurator.Service(settings => new EventPubService(configuration), x =>
            {
                x.BeforeStartingService(context => _log.Info("Service is being started"));
                x.AfterStoppingService(context => _log.Info("Service has stopped"));
            });
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <returns>System.Int32.</returns>
        private static int Main() => (int)HostFactory.Run(new Program().Configure);

        #endregion Private Methods
    }
}
