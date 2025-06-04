using System.Text.Json.Serialization;

namespace NuxibaApi.Models
{
    public class Login
    {
        public int Id { get; set; }
        public int User_id { get; set; }

        public int Extension {  get; set; }
        public int TipoMov {  get; set; }
        public DateTime Fecha { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }
}
