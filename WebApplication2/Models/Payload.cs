using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class Payload
    {
        public int Load { get; set; }
        public Fuel Fuels { get; set; }
        public List<PowerPlant> PowerPlants { get; set; }
    }
}
