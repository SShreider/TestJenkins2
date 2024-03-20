using Microsoft.AspNetCore.Mvc;

namespace TestJenkins.Controllers
{
    [ApiController]
    [Route("math")]
    public class ArithmeticOperationsController : ControllerBase
    {

        [HttpGet]
        [Route("sum")]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [HttpGet]
        [Route("sub")]
        public int Sub(int a, int b)
        {
            return a - b;
        }

        [HttpGet]
        [Route("mul")]
        public int Mul(int a, int b)
        {
            return a * b;
        }

        [HttpGet]
        [Route("div")]
        public int Div(int a, int b)
        {
            return a / b;
        }
    }
}
