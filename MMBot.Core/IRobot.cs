using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common.Logging;
using MMBot.Brains;
using MMBot.Router;
using MMBot.Scripts;
using ScriptCs.Contracts;

namespace MMBot
{
    public interface IRobot : IScriptPackContext, IDisposable
    {
        List<ScriptMetadata> ScriptData { get; }
        event EventHandler<EventArgs> ResetRequested;
        IDictionary<string, IAdapter> Adapters { get; }
        string[] Admins { get; }
        string Alias { get; set; }
        bool AutoLoadScripts { get; set; }
        IBrain Brain { get; }
        string[] Emitters { get; }
        List<string> HelpCommands { get; }
        LoggerConfigurator LogConfig { get; }
        ILog Logger { get; }
        string Name { get; set; }
        IRouter Router { get; }
        string ScriptPath { get; set; }
        List<IListener> Listeners { get; }
        bool Watch { get; set; }
        void CatchAll(Action<IResponse<CatchAllMessage>> action);
        void Enter(Action<IResponse<EnterMessage>> action);
        void Hear(string regex, Action<IResponse<TextMessage>> action);
        void Listen<T>(Func<T, MatchResult> matcher, Action<IResponse<T>> action) where T : Message;
        void Leave(Action<IResponse<LeaveMessage>> action);
        void Receive(Message message);
        void Respond(string regex, Action<IResponse<TextMessage>> action);
        void Speak(string room, params string[] messages);
        void Speak(string adapterId, string room, params string[] messages);
        IAdapter GetAdapter(string adapterId);
        void Topic(Action<IResponse<TopicMessage>> action);
        void AddHelp(params string[] helpMessages);
        void AddMetadata(ScriptMetadata metadata);
        string GetConfigVariable(string name);
        void LoadScript<TScript>() where TScript : IMMBotScript, new();
        void LoadScriptFile(string scriptFile);
        void LoadScriptFile(string scriptName, string scriptFile);
        void LoadScriptName(string scriptName);
        void LoadScripts(Assembly assembly);
        void LoadScripts(IEnumerable<Type> scriptTypes);
        void Emit<T>(string key, T data);
        void On<T>(string key, Action<T> action);
        void RegisterCleanup(Action cleanup);
        void RemoveListener(string regexPattern);
        Task Reset();
        Task Run();
        Task Shutdown();
        void Dispose();
    }
}