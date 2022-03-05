using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.SlashCommands;

namespace TomatenMusic.Music
{
    enum LoopType
    {
        [ChoiceName("Track")]
        TRACK,
        [ChoiceName("Queue")]
        QUEUE,
        [ChoiceName("None")]
        NONE
    }
}
