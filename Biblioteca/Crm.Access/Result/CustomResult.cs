using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Access.Result
{
    public class CustomResult : ObjectResult
    {
        public CustomResult(object? value, int statusCode) : base(value)
        {
            this.StatusCode = statusCode;
        }        
        
        public CustomResult(object? value, HttpStatusCode statusCode) : base(value)
        {
            this.StatusCode = (int?) statusCode;
        }
    }
}
