using Microsoft.Extensions.DependencyInjection;

namespace MSDI4Unity.Register
{
    public interface IServiceCollectionRegister
    {
        public void RegisterServices(IServiceCollection services);
    }
}