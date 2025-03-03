using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.OTPRepositories
{
    public class OTPRepositories : GenericRepositories<Otp>, IOTPRepositories
    {
        public OTPRepositories(HmesContext context) : base(context)
        {
        }
    }
}