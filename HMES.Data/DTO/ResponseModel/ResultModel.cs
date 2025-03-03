namespace HMES.Data.DTO.ResponseModel
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
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }

        public bool LastPage => CurrentPage >= TotalPages;
    }
}