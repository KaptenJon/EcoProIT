using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace EcoProIT.Main
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ExceptionManager man;
        public App(): base()
        {
            man = InitEnterprise();
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            man.HandleException(e.Exception, "ExceptionPolicy");
            string errorMessage = string.Format("An unhandled exception occurred: {0}\nSend exception log to jon.andersson@chalmers.se", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;

        }
        private ExceptionManager InitEnterprise()
        {
            var policies = new List<ExceptionPolicyDefinition>();
            var myTestExceptionPolicy = new List<ExceptionPolicyEntry>
                {
                    {
                        new ExceptionPolicyEntry(typeof (DivideByZeroException),
                                                 PostHandlingAction.NotifyRethrow,
                                                 new IExceptionHandler[]
                                                     {
                                                         new ReplaceHandler(
                                                     "Application error will be ignored and processing will continue.",
                                                     typeof (Exception))
                                                     })
                    },
                    {
                        new ExceptionPolicyEntry(typeof (InvalidCastException),
                                                 PostHandlingAction.NotifyRethrow,
                                                 new IExceptionHandler[]
                                                     {
                                                         new ReplaceHandler(
                                                     "Application error will be ignored and processing will continue.",
                                                     typeof (Exception))
                                                     })
                    },
                    {
                        new ExceptionPolicyEntry(typeof (Exception),
                                                 PostHandlingAction.NotifyRethrow,
                                                 new IExceptionHandler[]
                                                     {
                                                         new ReplaceHandler(
                                                     "Application error will be ignored and processing will continue.",
                                                     typeof (Exception))
                                                     })
                    }
                };
            policies.Add(new ExceptionPolicyDefinition(
                             "ExceptionPolicy", myTestExceptionPolicy));
            return new ExceptionManager(policies);
        }
    }
}