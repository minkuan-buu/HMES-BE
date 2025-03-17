using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TicketResponseRepositories;

public class TicketResponseRepositories : GenericRepositories<TicketResponse>, ITicketResponseRepositories
{
    public TicketResponseRepositories(HmesContext context) : base(context)
    {
    }
}