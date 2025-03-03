namespace HMES.Data.DTO.RequestModel;

public class CategoryReqModel
{
}

public class CategoryCreateReqModel
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? ParentCategoryId { get; set; }

    public string Attachment { get; set; } = null!;

    public string Status { get; set; } = null!;
}

public class CategoryUpdateReqModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = null!;
    public string Attachment { get; set; } = null!;
    public string Status { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
}