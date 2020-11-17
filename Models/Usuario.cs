using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Notas.Models
{
    public class Usuario
    {
        [Key]
        public string Mail { get; set; }
        [Required]
        public string Nombre { get; set; }
    }
}