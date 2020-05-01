using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.PollyDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollySampleController : ControllerBase
    {
        private readonly AsyncRetryPolicy retryPolicy;

        //https://github.com/App-vNext/Polly -- for other examples
        public PollySampleController(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
            retryPolicy = Policy
                // pass any predicate here 
                // and the execption will be checked against this predicate and if it matches 
                // then we retry the call
                .Handle<Exception>(e => e.Message.Equals("sample exception"))
                .RetryAsync(3);// it will retry 3 times for the above predicate passed in the handle method
        }

        public IHttpClientFactory HttpClientFactory { get; }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await retryPolicy.ExecuteAsync(async () =>
            {
                using var client = HttpClientFactory.CreateClient("PollySample");

                //throw an exception when this condition is met
                if (_ = new Random().Next(1, 3) == 1) throw new Exception("new exception");

                var response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos");
                return await response.Content.ReadAsStringAsync();
            }));
        }
    }
}
