namespace InternetShop.Requests;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}