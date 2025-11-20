namespace InternetShop.Requests;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}