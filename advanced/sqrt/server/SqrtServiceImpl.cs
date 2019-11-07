using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Sqrt;
using static Sqrt.SqrtService;

namespace server
{
    public class SqrtServiceImpl : SqrtServiceBase
    {
        public override async Task<SqrtReponse> sqrt(SqrtRequest request, ServerCallContext context)
        {
            await Task.Delay(1500);

            int number = request.Number;

            if (number >= 0)
                return new SqrtReponse() { SquareRoot = Math.Sqrt(number) };
            else
                throw new RpcException(new Status(StatusCode.InvalidArgument, "number < 0"));
        }
    }
}
