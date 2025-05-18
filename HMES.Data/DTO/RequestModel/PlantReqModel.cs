using System.ComponentModel.DataAnnotations;
using HMES.Data.Enums;

namespace HMES.Data.DTO.RequestModel;

public class PlantReqModel
{
    public string? Name { get; set; } = null!;

    public PlantStatusEnums? Status { get; set; }
}

public class TargetReqModel
{
    public ValueTypeEnums Type { get; set; }
    
    
    [Required(ErrorMessage = "MinValue is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "MinValue must be greater than or equal to 0.")]
    public decimal MinValue { get; set; }

    [Required(ErrorMessage = "MaxValue is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "MaxValue must be greater than or equal to 0.")]
    public decimal MaxValue { get; set; }
}

public class ChangeTargetReqModel
{
    public Guid PlantId { get; set; }
    public Guid TargetId { get; set; }
    public Guid PhaseId { get; set; }
    public Guid NewTargetId { get; set; }
}

public class AddNewPhaseDto
{
    public string Name { get; set; } = null!;
}