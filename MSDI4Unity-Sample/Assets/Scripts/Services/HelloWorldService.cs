using UnityEngine;

namespace MSDI4Unity.Sample.Services
{
    public interface IHelloWorldService
    {
        public void SayHello();
    }

    public class HelloWorldService : IHelloWorldService
    {
        public void SayHello()
        {
            Debug.Log("Hello World!");
        }
    }
}