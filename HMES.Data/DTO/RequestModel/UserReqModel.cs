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

    public class UserRegisterReqModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}