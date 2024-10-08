﻿using System;
using System.Collections.Generic;
using Alchemy.Classes;

namespace Alchemy.Handlers.WebSocket
{
    internal class WebSocketHandler : Handler
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="context">The user context.</param>
        public override void HandleRequest(Context context)
        {
            if (context.IsSetup)
            {
                context.UserContext.DataFrame_data.Append(context.Buffer, true);
                if (context.UserContext.DataFrame_data.Length <= context.MaxFrameSize)
                {
                    switch (context.UserContext.DataFrame_data.State)
                    {
                        case DataFrame.DataState.Complete:
                            context.UserContext.OnReceive();
                            break;
                        case DataFrame.DataState.Closed:
                            DataFrame closeFrame = context.UserContext.DataFrame_data.CreateInstance();
							closeFrame.State = DataFrame.DataState.Closed;
							closeFrame.Append(new byte[] { 0x8 }, true);
							context.UserContext.Send(closeFrame, false, true);
                            break;
                        case DataFrame.DataState.Ping:
                            context.UserContext.DataFrame_data.State = DataFrame.DataState.Complete;
                            DataFrame dataFrame = context.UserContext.DataFrame_data.CreateInstance();
                            dataFrame.State = DataFrame.DataState.Pong;
                            List<ArraySegment<byte>> pingData = context.UserContext.DataFrame_data.AsRaw();
                            foreach (var item in pingData)
                            {
                                dataFrame.Append(item.Array);
                            }
                            context.UserContext.Send(dataFrame);
                            break;
                        case DataFrame.DataState.Pong:
                            context.UserContext.DataFrame_data.State = DataFrame.DataState.Complete;
                            break;
                    }
                }
                else
                {
                    context.Disconnect(); //Disconnect if over MaxFrameSize
                }
            }
            else
            {
                Authentication.Authenticate(context);
            }
        }
    }
}