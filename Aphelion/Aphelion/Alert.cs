using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;

namespace Aphelion
{
    public static class Alert
    {
        private static SpeechSynthesizer tts = new SpeechSynthesizer();

        public static void Create(string text) // TO DO: Add a text display instead of just voice
        {
            // Maybe use blocking speech (instead of async) in a separate thread to make the alerts one at a time?
            tts.Rate = 1;
            //tts.SpeakAsync(text);
        }
    }
}
