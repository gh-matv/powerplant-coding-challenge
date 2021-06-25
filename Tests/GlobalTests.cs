using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using powerplant.Controllers;
using powerplant.Core;

namespace powerplant.Tests
{
    [TestClass]
    public class GlobalTests
    {
        [TestMethod]
        public void Init() => Assert.IsTrue(true);

        [TestMethod]
        public void TestExamples()
        {
            string[] urls = {
                "https://raw.githubusercontent.com/gem-spaas/powerplant-coding-challenge/master/example_payloads/payload1.json",
                "https://raw.githubusercontent.com/gem-spaas/powerplant-coding-challenge/master/example_payloads/payload2.json",
                "https://raw.githubusercontent.com/gem-spaas/powerplant-coding-challenge/master/example_payloads/payload3.json",
            };

            var logger = new TestLogger();

            for(var i = 0; i != urls.Length; ++i)
            {
                try
                {
                    var controller = new ProductionPlanController(logger);
                    var req = TestUtils.GetTextFromUrl(urls[i]);
                    var ret = controller.Get(req);
                    Assert.IsInstanceOfType(ret, Type.GetType("string"));

                    var jsonquery = JsonConvert.DeserializeObject<Request>(req);
                    var jsonresponse = JsonConvert.DeserializeObject<List<ResponseE>>(ret);

                    Assert.IsTrue(jsonquery != null, nameof(jsonquery) + " != null");
                    Assert.IsTrue(jsonresponse != null, nameof(jsonresponse) + " != null");
                    Assert.IsTrue(jsonquery.Powerplants.Count == jsonresponse.Count);
                    Assert.IsTrue(jsonquery.load == jsonresponse.ToList().Sum(e => e.p));
                }
                catch(Exception)
                {
                    Assert.IsTrue(false);
                }
            }

        }
    }
}
