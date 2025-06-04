namespace NuxibaApi.Models;

public class Area
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public ICollection<User> Users { get; set; } = new List<User>();
}
