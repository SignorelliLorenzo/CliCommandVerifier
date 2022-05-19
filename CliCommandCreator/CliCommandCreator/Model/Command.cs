using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CliCommandCreator.Model
{
    public class Command
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullCommand { get; set; }
        public string Params { get; set; }
        
        public Command BaseCommand { get; set; }
    }
}
