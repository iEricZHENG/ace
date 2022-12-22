using System.Threading.Tasks;

namespace App.IActor
{
    public interface IHello : Orleans.IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
        Task SetValue(string greeting);
    }
}
