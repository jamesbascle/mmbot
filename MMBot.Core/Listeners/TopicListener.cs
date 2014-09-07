﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMBot.Scripts;

namespace MMBot
{
    public class TopicListener : IListener
    {
        private readonly IRobot _robot;
        private readonly Action<IResponse<TopicMessage>> _callback;

        public TopicListener(IRobot robot, Action<IResponse<TopicMessage>> callback)
        {
            _robot = robot;
            _callback = callback;
        }


        public ScriptSource Source { get; set; }

        public bool Call(Message message)
        {
            var lm = message as TopicMessage;
            if (lm != null)
            {
                _callback(Response.Create(_robot, lm));
                return true;
            }

            return false;
        }
    }

}
