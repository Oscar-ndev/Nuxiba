namespace NuxibaApi.Models;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = "";

    public string Nombres { get; set; } = "";

    public string ApellidoPaterno { get; set; } = "";

    public string ApellidoMaterno { get; set; } = "";

    public int AreaId { get; set; }

    public Area? Area { get; set; }

    public ICollection<Login> Logins { get; set; } = new List<Login>();
}
