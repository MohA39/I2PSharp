using System;
using System.Collections.Generic;
using System.Text;

namespace I2PSharp
{
    public enum SAMResponseResults
    {
        OK,
        CANT_REACH_PEER,
        DUPLICATED_DEST,
        I2P_ERROR,
        INVALID_KEY,
        KEY_NOT_FOUND,
        PEER_NOT_FOUND,
        TIMEOUT,
        INVALID_ID,
        NONE
    }
}
