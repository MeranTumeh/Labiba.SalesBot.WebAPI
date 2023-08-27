using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Labiba.Sales.WebAPI.Models.LabibaResponses;
using static Labiba.Sales.WebAPI.Models.Req_Models;

namespace Labiba.Sales.WebAPI.Controllers
{
    public class TestController : Controller
    {
        [HttpPost]
        [Route("api/SalesController/Test")]
        [Obsolete]
        public async Task<IActionResult> Test(Testtt parametersModel)
        {
            StateModel stateModel = new StateModel();
            string lang = parametersModel.Language;
            if (lang == "en")
            {
                stateModel.state = "Not Found";
                return Ok(stateModel);
            }
            stateModel.state = "Success";
            return Ok(stateModel);

        }

    }
}
