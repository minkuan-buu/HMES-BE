using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.NutritionReportRepositories;

public class NutritionReportRepositories : GenericRepositories<NutritionReport>, INutritionReportRepositories
{
    public NutritionReportRepositories(HmesContext context) : base(context)
    {
    }
}