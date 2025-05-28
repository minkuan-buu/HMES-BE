namespace HMES.Data.DTO.ResponseModel;

public class PlantResModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class PlantResModelWithTarget
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
    public List<TargetInPhaseDto> phases { get; set; } = null!;
}

public class TargetResModel 
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public decimal MinValue { get; set; }

    public decimal MaxValue { get; set; }
}

public class TargetInPhaseDto
{
    public Guid? PhaseId { get; set; }
    public string PhaseName { get; set; } = null!;
    public List<TargetResModel> Target { get; set; } = null!;
    
}

public class TargetResModelWithPlants
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public decimal MinValue { get; set; }

    public decimal MaxValue { get; set; }
    
    public List<PlantAndPhaseForTargetListDto> Plants { get; set; } = null!;
}

public class PlantAndPhaseForTargetListDto
{
    public Guid PlantOfPhaseId { get; set; }
    public Guid PlantId { get; set; }
    public string PlantName { get; set; } = null!;
    public Guid? PhaseId { get; set; }
    public string PhaseName { get; set; } = null!;
}

public class PhaseResModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsDefault { get; set; } = false;
}

