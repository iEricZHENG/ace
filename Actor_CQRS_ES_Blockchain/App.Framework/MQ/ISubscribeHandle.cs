using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core.EventSourcing;
using App.Core;

namespace App.Framework.MQ
{
    public interface ISubscribeHandler
    {
        Task Notice(byte[] data);
    }
}
