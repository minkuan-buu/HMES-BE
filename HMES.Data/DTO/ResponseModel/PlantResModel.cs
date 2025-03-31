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
    public List<TargetResModel> Target { get; set; } = null!;
}

public class TargetResModel
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public decimal MinValue { get; set; }

    public decimal MaxValue { get; set; }
}

public class TargetResModelWithPlants
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public decimal MinValue { get; set; }

    public decimal MaxValue { get; set; }
    
    public List<PlantResModel> Plants { get; set; } = null!;
}

