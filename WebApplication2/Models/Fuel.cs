using Newtonsoft.Json;

namespace WebApplication2.Models
{
    public class Fuel
    {
        public decimal Gas { get; set; }
        public decimal Kerosine { get; set; }
        public decimal CO2 { get; set; }
        public short Wind { get; set; }
    }
}