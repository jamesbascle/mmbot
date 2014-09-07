using System.Collections.Generic;
using MMBot.Scripts;

namespace MMBot.Tests.CompiledScripts
{
    public class TextListenerTest : IMMBotScript
    {
        public void Register(IRobot robot)
        {
            robot.Respond(@"(gif|giphy)( me)? (.*)", msg => msg.Send(msg.Match[3]));
        }

        public IEnumerable<string> GetHelp()
        {
            return new string[0];
        }
    }
}
