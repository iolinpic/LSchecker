using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSchecker
{
    public class LookupResult
    {
        public int Id { get; set; }
        public string Link { get; set; } = "";
        public string Result { get; set; } = "";
        public DateTime? Created { get; set; }
    }
}