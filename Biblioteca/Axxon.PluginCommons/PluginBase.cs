using System;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace Axxon.PluginCommons
{
    /// <summary>
    /// Adaptación de https://github.com/rappen/JonasPluginBase/blob/master/JonasPluginBase
    /// Implements IPlugin to encapsulate a PluginBag containing stuff from the service and context,
    /// and to log service requests to the Tracing Service.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            using (var bag = new PluginBag(serviceProvider))
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    bag.Trace("Execute subplugin:");
                    this.Execute(bag);
                }
                catch (Exception e)
                {
                    bag.Trace(e.ToString());
                    throw;
                }
                finally
                {
                    watch.Stop();
                    bag.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
                }
            }
        }

        public abstract void Execute(PluginBag bag);
    }
}