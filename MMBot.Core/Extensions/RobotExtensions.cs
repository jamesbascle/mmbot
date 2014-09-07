﻿using System;
using System.Reactive.Linq;

namespace MMBot
{
    public static class RobotExtensions
    {
        /// <summary>
        /// Provides http client functionality through a fluent API
        /// </summary>
        /// <param name="robot">The robot</param>
        /// <param name="baseUrl">The base url of the HTTP endpoint</param>
        /// <returns>An <see cref="HttpWrapper"/> instance</returns>
        public static HttpWrapper Http(this IRobot robot, string baseUrl)
        {
            return new HttpWrapper(baseUrl, robot.Logger);
        }

        /// <summary>
        /// Schedules an action to repeat on a schedule governed by the given timespan
        /// </summary>
        /// <param name="robot">The robot</param>
        /// <param name="timespan">The repeat interval</param>
        /// <param name="action">The action to schedule</param>
        /// <returns>A disposable which, when disposed, will cancel the schedule</returns>
        public static IDisposable ScheduleRepeat(this IRobot robot, TimeSpan timespan, Action action)
        {
            return Observable.Interval(timespan).Subscribe(_ => action());
        }
    }
}