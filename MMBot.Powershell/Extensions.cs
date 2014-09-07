﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using System.Threading;
using System.IO;
using MMBot;

namespace MMBot.Powershell
{
    public static class Extensions
    {
        public static string ConvertToString(this PSObject psObject)
        {
                string message = string.Empty;

                if (psObject.BaseObject.GetType() == typeof(string))
                    return psObject.ToString();

                else if (psObject.BaseObject.GetType() == typeof(Hashtable))
                {
                    Hashtable hashTable = (Hashtable)psObject.BaseObject;
                    StringBuilder sb = new StringBuilder();
                    foreach (DictionaryEntry dictionaryEntry in hashTable)
                        sb.AppendFormat("{0} = {1}\n", dictionaryEntry.Key, dictionaryEntry.Value);
                    return sb.ToString();
                }
                else
                    return psObject.ToString();
        }

        public static IEnumerable<string> ExecutePowershellCommand(this IRobot robot, string command)
        {
            var host = new MMBotHost(robot);
            using (var runspace = RunspaceFactory.CreateRunspace(host))
            {
                runspace.Open();
                using (var invoker = new RunspaceInvoke(runspace))
                {
                    Collection<PSObject> psObjects = new Collection<PSObject>();
                    try
                    {
                        IList errors;
                        psObjects = invoker.Invoke(command, null, out errors);
                        if (errors.Count > 0)
                        {
                            string errorString = string.Empty;
                            foreach (var error in errors)
                                errorString += error.ToString();

                            psObjects.Add(new PSObject(errorString));
                        }

                    }
                    catch (Exception ex)
                    {
                        psObjects.Add(new PSObject(ex.Message));
                    }

                    foreach (var psObject in psObjects)
                    {
                        yield return psObject.ConvertToString();
                    }
                }
            }
        }

        public static IEnumerable<string> ExecutePowershellModule(this IRobot robot, string moduleCommand)
        {
            var scriptFolder = robot.GetConfigVariable("MMBOT_POWERSHELL_SCRIPTSPATH");

            var commandArgs = moduleCommand.Split(' ');
            var scriptName = commandArgs[0];
            var parameters = string.Empty;
            for (int i = 1; i < commandArgs.Length; i++)
                parameters += commandArgs[i] + " ";

            var scriptPath = Path.Combine(scriptFolder, scriptName + ".psm1");

            if (!File.Exists(scriptPath))
            {
                yield return "Command not found";
            }
            else
            {
                var host = new MMBotHost(robot);
                using (var runspace = RunspaceFactory.CreateRunspace(host))
                {
                    runspace.Open();
                    using (var invoker = new RunspaceInvoke(runspace))
                    {
                        Collection<PSObject> psObjects = new Collection<PSObject>();
                        invoker.Invoke(string.Format("Import-Module {0}", scriptPath));
                        try
                        {
                            IList errors;
                            psObjects = invoker.Invoke(string.Format("{0} {1}", scriptName, parameters), null, out errors);
                            if (errors.Count > 0)
                            {
                                string errorString = string.Empty;
                                foreach (var error in errors)
                                    errorString += error.ToString();

                                psObjects.Add(new PSObject(errorString));
                            }

                        }
                        catch (Exception ex)
                        {
                            psObjects.Add(new PSObject(ex.Message));
                        }

                        foreach (var psObject in psObjects)
                        {
                            yield return psObject.ConvertToString();
                        }
                    }
                }
            }
            
        }
    }

    public class MMBotHost : PSHost
    {

        #region Fields

        private Guid m_InstanceId;
        private PSHostUserInterface m_UI;

        #endregion

        #region PSHost Members

        private string _name = "mmbot";

        IRobot _robot;

        public MMBotHost(IRobot robot)
        {
            _name = robot.Name;
            _robot = robot;
        }
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        public override Guid InstanceId
        {
            get
            {
                if (m_InstanceId == Guid.Empty)
                {
                    m_InstanceId = Guid.NewGuid();
                }
                return m_InstanceId;
            }
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override System.Management.Automation.Host.PSHostUserInterface UI
        {
            get
            {
                if (m_UI == null)
                {
                    m_UI = new MMBotPSUserInterface(_robot);
                }
                return m_UI;
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }
        public override void NotifyBeginApplication()
        {
            throw new NotImplementedException();
        }

        public override void NotifyEndApplication()
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class MMBotPSUserInterface : PSHostUserInterface
    {

        #region Fields

        private PSHostRawUserInterface m_RawUI;

        #endregion

        #region PSHostUserInterface Members

        private IRobot _robot;

        public MMBotPSUserInterface(IRobot robot)
        {
            _robot = robot;
        }
        #region Input Methods

        // it's a bot - we don't support input

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                if (m_RawUI == null)
                {
                    m_RawUI = new MMBotRawUserInterface();
                }
                return m_RawUI;
            }
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }
        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Script Output Methods

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _robot.Logger.Info(value);
        }

        public override void Write(string value)
        {
            this.Write(this.RawUI.ForegroundColor, this.RawUI.BackgroundColor, value);
        }

        public override void WriteLine(string value)
        {
            this.Write(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            this.Write(record.PercentComplete.ToString());
        }

        #endregion

        #region Logging Output Methods

        public override void WriteDebugLine(string message)
        {
            _robot.Logger.Debug(message);
        }

        public override void WriteErrorLine(string value)
        {
            _robot.Logger.Error(value);
        }

        public override void WriteVerboseLine(string message)
        {
            _robot.Logger.Info(message);
        }

        public override void WriteWarningLine(string message)
        {
            _robot.Logger.Warn(message);
        }

        #endregion

        #endregion

    }

    public class MMBotRawUserInterface : PSHostRawUserInterface
    {

        #region Fields

        private ConsoleColor m_BackgroundColor = ConsoleColor.Black;
        private Size m_BufferSize = new Size(80, 25);
        private Coordinates m_CursorPosition = new Coordinates(0, 0);
        private int m_CursorSize = 1;
        private ConsoleColor m_ForegroundColor = ConsoleColor.White;
        #endregion

        #region PSHostRawUserInterface Members

        public override ConsoleColor BackgroundColor
        {
            get
            {
                return m_BackgroundColor;
            }
            set
            {
                m_BackgroundColor = value;
            }
        }

        public override Size BufferSize
        {
            get
            {
                return m_BufferSize;
            }
            set
            {
                m_BufferSize = value;
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                return m_CursorPosition;
            }
            set
            {
                m_CursorPosition = value;
            }
        }

        public override int CursorSize
        {
            get
            {
                return m_CursorSize;
            }
            set
            {
                m_CursorSize = value;
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                return m_ForegroundColor;
            }
            set
            {
                m_ForegroundColor = value;
            }
        }

        public override bool KeyAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Coordinates WindowPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Size WindowSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string WindowTitle
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void FlushInputBuffer()
        {
            throw new NotImplementedException();
        }
        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
