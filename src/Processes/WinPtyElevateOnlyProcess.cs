﻿using gsudo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsudo.Processes
{
    class WinPtyElevateOnlyProcess
    {
        private NamedPipeServerStream pipe;
        private Process process;

        public WinPtyElevateOnlyProcess(NamedPipeServerStream pipe)
        {
            this.pipe = pipe;
        }

        public async Task Start(ElevationRequest request)
        {
            try
            {
                process = ProcessStarter.StartDetached(request.FileName, request.Arguments, false);
                await pipe.WriteAsync($"{Globals.TOKEN_FOCUS}{process.MainWindowHandle}{Globals.TOKEN_FOCUS}");
                await pipe.WriteAsync($"{Globals.TOKEN_EXITCODE}0{Globals.TOKEN_EXITCODE}");
                pipe.Flush();
                pipe.WaitForPipeDrain();
                pipe.Close();
                return;
            }
            catch (Exception ex)
            {
                Globals.Logger.Log(ex.ToString(), LogLevel.Error);
                if (pipe.IsConnected)
                    await pipe.WriteAsync(Globals.TOKEN_ERROR + "Server Error: " + ex.ToString() + "\r\n");
                pipe.Flush();
                pipe.WaitForPipeDrain();
                pipe.Close();
                return;
            }
        }
    }
}