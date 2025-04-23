namespace Libro.MarvelFunctionApp.Model
{
    public class Request
    {
        public string NameStartsWith { get; set; }
        public int LimitRecords { get; set; }
        public string HashCode { get; set; }
        public string TimeStamp { get; set; }
        public string PublicKey { get; set; }
    }
}
