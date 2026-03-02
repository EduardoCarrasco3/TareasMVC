using Microsoft.AspNetCore.Identity;

namespace TareasMVC.Entidades
{
    public class User : IdentityUser
    {
        public char Sexo { get; set; }
        public DateOnly FechaNacimineto { get; set; }
        public string Curp { get; set; }

    }
}
