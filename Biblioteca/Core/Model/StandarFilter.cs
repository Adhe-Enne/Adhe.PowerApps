using Core.Abstractions;

namespace Core.Model
{
    public class StandarFilter : BasicFilter, IPageParams, IHttpParameters
    {
        public StandarFilter()
        {
            
        }

        public StandarFilter(IRequestFilter parameters) : base (parameters)
        {
            this.Page = parameters.Page;
            this.Rows = parameters.Rows;
        }

        public int? Page { get; set; }
        public int? Rows { get; set; }
    }

    public class BasicFilter : IHttpParameters
    {
        public BasicFilter()
        {
            
        }

        public BasicFilter(IRequestFilter parameters)
        {
            this.Expand = parameters.Expand;
            this.OrderBy = parameters.OrderBy;
            this.Top = parameters.Top;
            this.Filter = parameters.Filter;
            this.Select = parameters.Select;    
        }

        public string Select { get; set; }
        public string Expand { get; set; }
        public int? Top { get; set; }
        public string Filter { get; set; }
        public List<string> OrderBy { get; set; }
    }
}
