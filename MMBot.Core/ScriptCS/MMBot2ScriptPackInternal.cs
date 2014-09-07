using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace MMBot.ScriptCS
{
    [PartNotDiscoverable]
    public class MMBot2ScriptPackInternal : IScriptPack<IRobot>
    {
        private IRobot _robot;

        public MMBot2ScriptPackInternal(IRobot robot)
        {
            _robot = robot;
        }

        public void Initialize(IScriptPackSession session)
        {

        }

        public IScriptPackContext GetContext()
        {
            return _robot;
        }

        public void Terminate()
        {

        }

        IRobot IScriptPack<IRobot>.Context
        {
            get { return _robot; }
            set { _robot = value; }
        }
    }
}