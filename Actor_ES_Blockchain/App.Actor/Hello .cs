using App.IActor;

namespace App.Actor
{
    public class Hello : Orleans.Grain, IHello
    {

#pragma warning disable CS8618 //
        private static string tempData;
#pragma warning restore CS8618 // 

        Task<string> IHello.SayHello(string greeting)
        {
            var result = string.IsNullOrEmpty(tempData) ? $"You said: '{greeting}', I say: Hello!" : $"You said:'{tempData}-{greeting}'";
            return Task.FromResult(result);
        }

        public Task SetValue(string temp)
        {
            tempData = temp;
            return Task.CompletedTask;
        }
    }
}
