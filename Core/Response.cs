
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
namespace powerplant.Core
{
    public class ResponseE
    {
        public ResponseE(string n)
        {
            name = n;
        }

        public ResponseE(string n, int pow)
        {
            name = n;
            p = pow;
        }

        public string name { get; set; }
        public int p { get; set; }
    }
}
