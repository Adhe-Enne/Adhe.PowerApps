namespace Socio.Api.Model
{
    public class ParametersQuery
    {
        public int? Top {  get; set; }
        public string Filter { get; set; }
        public List<string> OrderBy { get; set; }

        public int? Page { get; set; }
        public int? Rows { get; set; }
    }
}
