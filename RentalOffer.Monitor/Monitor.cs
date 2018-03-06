using System;
using MicroServiceWorkshop;
using MicroServiceWorkshop.RapidsRivers;
using MicroServiceWorkshop.RapidsRivers.RabbitMQ;
using Newtonsoft.Json.Linq;

namespace RentalOffer.Monitor
{
    class Monitor : River.IPacketListener
    {
        static RapidsConnection rapidsConnection;
        static void Main(string[] args)
        {
            string host = "192.168.254.120";
            string port = "5676";

            rapidsConnection = new RabbitMqRapids("monitor_in_csharp", host, port);
            var river = new River(rapidsConnection);
            // See RiverTest for various functions River supports to aid in filtering, like:
            //river.RequireValue("key", "expected_value");  // Reject packet unless key exists and has expected value
            //river.Require("key1", "key2");       // Reject packet unless it has key1 and key2
            //river.Forbid("key1", "key2");        // Reject packet if it does have key1 or key2
            river.Register(new Monitor());         // Hook up to the river to start receiving traffic
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {

            var need = jsonPacket.ToObject<NeedClass>();
            if (need.Forbidden == "n")
            {
                need.Forbidden = "y";
                string newJSON = "{\"need\":\"" + need.Need + "\", \"ID\": \"" + need.ID + "\"," +
                    " \"Forbidden\":\"" + need.Forbidden + "\", \"Value\":\"750\", \"Probability\":\"0.75\", \"Membership\":\"P\"}";

                Console.WriteLine("New need is: {0}", newJSON);
                rapidsConnection.Publish(newJSON);
            }
        }
        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            Console.WriteLine(" [x] {0}", errors);
        }
    }
}
