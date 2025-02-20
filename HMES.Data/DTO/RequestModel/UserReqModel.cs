namespace HMES.Data.DTO.RequestModel
{
    public class UserReqModel
    {
    }


    public class UserLoginReqModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}