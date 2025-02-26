namespace HMES.Data.DTO.ResponseModel;

public class CategoryResModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? ParentCategoryId { get; set; }

    public string Attachment { get; set; } = null!;
}

public class CategoryRecursiveResModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? Attachment { get; set; }
    public string Status { get; set; } = null!;
    public CategoryRecursiveResModel? ParentCategory { get; set; }
}

