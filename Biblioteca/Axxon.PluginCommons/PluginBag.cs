

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace Axxon.PluginCommons
{
    /// <summary>
    /// Adaptación de https://github.com/rappen/JonasPluginBase/blob/master/JonasPluginBase. A bag
    /// of useful stuff when developing plugins of different types for Microsoft Dynamics 365
    /// </summary>
    public partial class PluginBag : IDisposable
    {
        private readonly CodeActivityContext _codeActivityContext;

        private readonly IExecutionContext _context;

        /// <summary>
        /// Constructor to use when JonasPluginBag is used in custom applications
        /// </summary>
        /// <param name="service">IOrganizationService connected to Microsoft Dynamics CRM (365)</param>
        /// <param name="context"></param>
        /// <param name="trace"></param>
        public PluginBag(IOrganizationService service, IPluginExecutionContext context, ITracingService trace)
        {
            this.TracingService = trace;
            this.Service = service;
            _context = context;
            this.Init();
        }

        /// <summary>
        /// Constructor to be used from a Microsoft Dynamics CRM (365) custom workflow activity
        /// </summary>
        /// <param name="executionContext">
        /// CodeActivityContext passed to the CodeActivity.Execute method
        /// </param>
        public PluginBag(CodeActivityContext executionContext)
        {
            _codeActivityContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));

            this.TracingService = executionContext.GetExtension<ITracingService>();
            _context = executionContext.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            this.Service = serviceFactory.CreateOrganizationService(_context.InitiatingUserId);

            this.Init();
        }

        /// <summary>
        /// Constructor to be used from a Microsoft Dynamics CRM (365) plugin
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider passed to the IPlugin.Execute method</param>
        public PluginBag(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.Service = serviceFactory.CreateOrganizationService(_context.UserId);
        }

        public IPluginExecutionContext PluginContext => _context as IPluginExecutionContext;

        public Entity PostImage => _context != null &&
                   _context.PostEntityImages != null &&
                   _context.PostEntityImages.Count > 0
                   ? _context.PostEntityImages.Values.FirstOrDefault()
                     : null;

        public Entity PreImage => _context != null &&
                   _context.PreEntityImages != null && _context.PreEntityImages.Count > 0
                   ? _context.PreEntityImages.Values.FirstOrDefault()
                     : null;

        public IOrganizationService Service { get; }

        public Entity TargetEntity
        {
            get
            {
                if (_context != null && _context.InputParameters.Contains("Target"))
                {
                    var target = _context.InputParameters["Target"];

                    if (target is Entity)
                    {
                        return target as Entity;
                    }

                    var targetReference = target as EntityReference;

                    if (targetReference != null)
                    {
                        return new Entity(targetReference.LogicalName, targetReference.Id);
                    }
                }

                return null;
            }

            set => _context.InputParameters["Target"] = value;
        }

        public ITracingService TracingService { get; }

        public IWorkflowContext WorkflowContext => _context as IWorkflowContext;

        protected virtual void Dispose(bool disposeInternal)
        {
            if (disposeInternal)
            {
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Check if the plugin fired in the specific message. The comparisson ignores case.
        /// </summary>
        /// <param name="messageName">Plugin message name</param>
        /// <returns></returns>
        public bool ExecutedWithinMessage(string messageName) => _context.MessageName.Equals(messageName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Get label for specified optionset attribute and value
        /// </summary>
        /// <param name="entity">Entity where the attribute is used</param>
        /// <param name="attribute">Attribute name</param>
        /// <param name="value">Value of the optionset for which to return label</param>
        /// <returns></returns>
        public string GetOptionsetLabel(string entity, string attribute, int value)
        {
            this.Trace($"Getting metadata for {entity}.{attribute}");
            var req = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity,
                LogicalName = attribute,
                RetrieveAsIfPublished = true
            };

            var resp = (RetrieveAttributeResponse)this.Service.Execute(req);
            var plmeta = (PicklistAttributeMetadata)resp.AttributeMetadata;
            if (plmeta == null)
            {
                throw new InvalidPluginExecutionException($"{entity}.{attribute} does not appear to be an optionset");
            }
            var result = plmeta.OptionSet.Options.FirstOrDefault(o => o.Value == value)?.Label?.UserLocalizedLabel?.Label;
            this.Trace($"Returning label for value {value}: {result}");
            return result;
        }

        /// <summary>
        /// Trace method automatically adding timestamp to each traced item
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args) => this.TracingService.Trace(format, args);

        public string GetEnvironmentVariableValue(string displayName)
        {
            string fetchXml = $@"
                <fetch top='1'>
	                <entity name='environmentvariabledefinition'>
		                <filter type='and'>
			                <condition attribute='displayname' operator= 'eq' value='{displayName}' />
		                </filter>
	                </entity>
                </fetch>";

            EntityCollection results = this.Service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Count == 0) return null;

            Entity entity = results.Entities.FirstOrDefault();

            if (!entity.Attributes.ContainsKey("defaultvalue")) return null;

            return entity.Attributes["defaultvalue"].ToString();
        }

        internal string PrimaryAttribute(string entityName)
        {
            var metabase = (RetrieveEntityResponse)this.Service.Execute(new RetrieveEntityRequest()
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            });
            this.Trace("Metadata retrieved for {0}", entityName);
            if (metabase != null)
            {
                var meta = metabase.EntityMetadata;
                var result = meta.PrimaryNameAttribute;
                this.Trace("Primary attribute is: {0}", result);
                return result;
            }
            else
            {
                throw new InvalidPluginExecutionException(
                    "Unable to retrieve metadata/primaryattribute for entity: " + entityName);
            }
        }

        private void Init() => this.LogTheContext(_context);

        private void LogTheContext(IExecutionContext context)
        {
            if (context == null) { return; }

            var step = context.OwningExtension != null ? !string.IsNullOrEmpty(context.OwningExtension.Name) ? context.OwningExtension.Name : context.OwningExtension.Id.ToString() : "null";
            var stage = context is IPluginExecutionContext ccontext ? ccontext.Stage : 0;

            this.Trace($@"Context details:
              Step:  {step}
              Msg:   {context.MessageName}
              Stage: {stage}
              Mode:  {context.Mode}
              Depth: {context.Depth}
              Corr:  {context.CorrelationId}
              Type:  {context.PrimaryEntityName}
              Id:    {context.PrimaryEntityId}
              User:  {context.UserId}
              IUser: {context.InitiatingUserId}
            ");
        }
    }
}