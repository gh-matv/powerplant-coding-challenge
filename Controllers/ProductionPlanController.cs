using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using powerplant.Core;

namespace powerplant.Controllers
{
    [ApiController]
    [Route("[controller]", Name = "productionplan")]
    public class ProductionPlanController : ControllerBase
    {

        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(ILogger<ProductionPlanController> logger)
        {
            _logger = logger;
        }

        /**
         * This function is the endpoint of the API
         */
        [HttpGet]
        public string Get(string request)
        {
            if (string.IsNullOrEmpty(request))
            {
                _logger.LogWarning("Request is null or empty");

                StatusCode(400);
                return "";
            }

            try
            {
                // Grab request
                var r = JsonConvert.DeserializeObject<Request>(request);

                // Calculate answer
                // We could transform the request to an internal type, for handling, but the input
                //  format is exactly what we need, so we directly pass the request to the computing class.
                bool success;
                var resp = ProductionPlanComputer.Compute(r, out success);

                if (!success)
                {
                    StatusCode(400);
                    return "{\"error\":\"No combination of powerplants can attain required power\"}";
                }

                // Send answer
                return JsonConvert.SerializeObject(resp);
            }
            catch (JsonReaderException e)
            {
                _logger.LogWarning("JsonReaderException: " + e.Message);

                StatusCode(400);
                return "{\"error\":\"Unable to read JSON file.\"}";
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message);

                StatusCode(500);
                return "{\"error\":\"An exception occured\"}";
            }
        }
    }
}
