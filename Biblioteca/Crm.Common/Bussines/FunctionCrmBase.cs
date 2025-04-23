using Core.Framework.Bussines;
using Crm.Common.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Common.Bussines
{
    public class FunctionCrmBase : BaseManager
    {
        protected readonly CrmSettings _settings;
        public FunctionCrmBase(ILogger log, CrmSettings crmSettings): base(log)
        {
            _settings = crmSettings;
        }
    }
}
