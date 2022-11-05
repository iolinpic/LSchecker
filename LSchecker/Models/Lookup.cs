using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSchecker.Models
{
    public class Lookup
    {
        public int Id { get; set; }
        public string Link { get; set; } = "";
        public string Result { get; set; } = "";
        public DateTime? Created { get; set; }
    }
}