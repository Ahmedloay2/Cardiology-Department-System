namespace Caridology_Department_System.Models
{
    public class ResponseWrapperDto
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
    }
    public class ResponseWrapperDto<TData> : ResponseWrapperDto where TData : class
    {
        public TData Data { get; set; }
    }
}
