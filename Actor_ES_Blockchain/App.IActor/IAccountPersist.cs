﻿using App.Core;
using App.Framework.EventSourcing;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.IActor
{
    public interface IAccountPersist : IDenormalize, IGrainWithStringKey
    {
        
    }
}
