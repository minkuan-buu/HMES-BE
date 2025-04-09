using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.NutritionRDRepositories;

public class NutritionRDRepositories : GenericRepositories<NutritionReportDetail>, INutritionRDRepositories
{
    public NutritionRDRepositories(HmesContext context) : base(context)
    {
    }
}