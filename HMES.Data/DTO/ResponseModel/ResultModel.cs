namespace MeowWoofSocial.Data.DTO.ResponseModel
{
    public class ResultModel<T>
    {
        public int StatusCodes { get; set; }
        public T? Response { get; set; }
    }
    public class MessageResultModel
    {
        public string Message { get; set; } = null!;
    }

    public class DataResultModel<T>
    {
        public T? Data { get; set; }
    }

    public class ListDataResultModel<T>
    {
        public List<T>? Data { get; set; }
    }
}