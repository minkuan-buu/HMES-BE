using HMES.Data.DTO.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Business.Utilities.Email
{
    public interface IEmail
    {
        Task SendEmail(string Subject, List<EmailReqModel> emailReqModels);
        //Task<bool> SendListEmail(string Subject, List<EmailSendingModel> sendingList);
    }
}
