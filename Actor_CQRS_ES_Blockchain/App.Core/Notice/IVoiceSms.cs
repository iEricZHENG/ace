using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Notice
{
    public interface IVoiceSms
    {
        Task<TResult> SendVoice(string mobile, string content);
    }
}
