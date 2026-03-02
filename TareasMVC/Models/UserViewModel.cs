namespace TareasMVC.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public char Sexo { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Curp { get; set; }

    }
}
