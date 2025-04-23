namespace Core.Abstractions
{
    public interface IPageParams
    {
        public int? Page { get; set; }
        public int? Rows { get; set; }
    }

    public interface IHttpParameters
    {
        public string Select { get; set; }
        public string Expand { get; set; }
        public int? Top { get; set; }
        public string Filter { get; set; }
        public List<string> OrderBy { get; set; }
    }

    public interface IRequestFilter: IPageParams, IHttpParameters;
}
