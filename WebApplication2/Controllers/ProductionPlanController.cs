using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : Controller
    {

        private class Resultat
        {
            public string Name { get; set; }
            public int P { get; set; }
            
        }

        private class MeritOrder
        {
            public Resultat Resultat { get; set; }
            public int MaxP { get; set; }
            public bool SwitchedOn { get; set; }
        }

        [HttpPost("productionplan")]
        public IActionResult Post([FromBody] Payload payloads)
        {
            //Before executing the query with Swagger, please remove the () in all fuels in the payloads, otherwise they will gave 0
            if (payloads.Fuels.Gas == 0 || payloads.Fuels.Kerosine == 0 || payloads.Fuels.CO2 == 0) return Problem("Did you remove the () in all fuels in the payload?", null,404, null, null);
            List<Resultat> result = new List<Resultat>();
            List<MeritOrder> tempResultList = new List<MeritOrder>();
            List<PowerPlant> tempPowerPlan = new List<PowerPlant>();
            var load = payloads.Load;
            var tempResult = 0;
            //Having this many loops is wrong, I need to find something to increase the speed of the process
            //which might become slower if too many data is used
            foreach (PowerPlant powerplan in payloads.PowerPlants)
            {
                //DEFINING THE COST FOR MERIT-ORDER
                switch (powerplan.Type.ToLower())
                {
                    case "gasfired":
                        tempResult = (int)((powerplan.Efficiency * (powerplan.Pmax + powerplan.Pmin) * payloads.Fuels.Gas));
                        tempResultList.Add(new MeritOrder { Resultat = new Resultat { Name = powerplan.Name, P = tempResult }, MaxP = powerplan.Pmax, SwitchedOn = true});
                        break;
                    case "turbojet":
                        tempResult = (int)((powerplan.Efficiency * (powerplan.Pmax + powerplan.Pmin)) * payloads.Fuels.Kerosine);
                        tempResultList.Add(new MeritOrder { Resultat = new Resultat { Name = powerplan.Name, P = tempResult }, MaxP = powerplan.Pmax, SwitchedOn = true });
                        break;
                    case "windturbine":
                        tempResult = (int)(powerplan.Pmax * (payloads.Fuels.Wind / 100.0));
                        tempResultList.Add(new MeritOrder { Resultat = new Resultat { Name = powerplan.Name, P = 0 }, MaxP = powerplan.Pmax, SwitchedOn = tempResult == 0 ? false : true });
                        break;
                }
            }
            
            //Ordering the powerplant by the cost of producing their max power
            tempResultList = tempResultList.OrderByDescending(x => x.SwitchedOn).ThenBy(x => x.Resultat.P).ToList();
            foreach(var item in tempResultList)
            {
                var powerplan = payloads.PowerPlants.FirstOrDefault(x => x.Name == item.Resultat.Name);
                if(powerplan != null)   tempPowerPlan.Add(powerplan);
            }

            //It looks like I am missing something because It's probably going to cost less to use the gasfired powerplan before the use of the turbojet
            //But I don't know how to implement that efficiently
            //Something like = if my powerplan produce as much as the turbojet, will it costs less? if yes then gasfired merit-order is greater than the tj

            foreach (PowerPlant powerplan in tempPowerPlan)
            {
                if(load != 0)
                {
                    if (load > powerplan.Pmax)
                    {
                        if (powerplan.Type.ToLower() == "windturbine")
                        {
                            tempResult = (int)(powerplan.Pmax * (payloads.Fuels.Wind / 100.0));
                        }
                        else tempResult = (int)(powerplan.Pmax);
                    }
                    else tempResult = load;
                    result.Add(new Resultat { Name = powerplan.Name, P = tempResult });
                    load = load - tempResult;
                }
                else result.Add(new Resultat { Name = powerplan.Name, P = 0 });
            }
            return Ok(result);
        }
    }
}
