using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Greeting;
using Grpc.Core;
using static Greeting.GreetingService;

namespace server
{
    public class GreetingServiceImpl : GreetingServiceBase
    {
        public override async Task<Greetingresponse> greet_with_deadline(GreetingRequest request, ServerCallContext context)
        {
            await Task.Delay(300);

            return new Greetingresponse() { Result = "Hello " + request.Name };
        }
    }
}
