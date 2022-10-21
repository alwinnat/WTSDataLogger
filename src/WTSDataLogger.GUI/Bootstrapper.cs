using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Windows;
using WTSDataLogger.Core;
using WTSDataLogger.GUI.ViewModels;

namespace WTSDataLogger.GUI
{
    internal sealed class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new();

        public Bootstrapper()
        {
            Initialize();
        }

        #region Overrides
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<ShellViewModel>();
        }

        protected override void Configure()
        {
            _container.Singleton<IWindowManager, WindowManager>()
                      .Singleton<IEventAggregator, EventAggregator>()
                      .Singleton<ISerialPort, SerialPort>()
                      .PerRequest<ShellViewModel>()
                      .Instance<ISerialPortSettings>(Properties.Settings.Default);
        }

        protected override void BuildUp(object instance) => _container.BuildUp(instance);
        protected override object GetInstance(Type serviceType, string key) => _container.GetInstance(serviceType, key);
        protected override IEnumerable<object> GetAllInstances(Type serviceType) => _container.GetAllInstances(serviceType);
        #endregion
    }
}
