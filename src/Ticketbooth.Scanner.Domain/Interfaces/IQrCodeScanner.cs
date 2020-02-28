﻿using System;
using System.Threading.Tasks;

namespace Ticketbooth.Scanner.Domain.Interfaces
{
    public interface IQrCodeScanner
    {
        public Func<string, Task> Validation { set; }

        public Action ScanStarted { set; }

        public Action CameraNotFound { set; }

        public Action CameraError { set; }

        ValueTask Start();
    }
}
