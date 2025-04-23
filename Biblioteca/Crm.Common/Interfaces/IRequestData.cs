using Core.Abstractions;
using Crm.Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Common.Interfaces
{
    public interface IRequestData
    {
        public bool UseExternalUrl { get; set; }
        public string EntityName { get; set; }
        public string MapperName { get; set; }
    }

    public interface IRequestData<T> : IRequestData
    {
        public T Body { get; set; }
    }

    public interface IRequestGet: IRequestData, IRequestFilter;
}