﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet;

namespace MMBot.Scripts
{
    public class NuGetScripts : IMMBotScript
    {
        private const string NuGetRepositoriesSetting = "MMBOT_NUGET_REPOS";
        private const string NuGetPackageAliasesSetting = "MMBOT_NUGET_PACKAGE_ALIASES";
        private const string NuGetResetAfterUpdateSetting = "MMBOT_NUGET_RESET";

        const string Add = "add|remember";
        const string Remove = "remove|delete|del|rem|forget";
        const string Package = "pkg|package";
        const string Source = "source|src|sources";
        const string Alias = "alias|aliases";
        const string List = "list";
        const string ParamWithNoSpaces = @"[^\s]+";
        const string Update = "update";
        const string Restart = "restart";

        private void RememberConfiguredSources(IRobot robot)
        {
	        var configuredSources = robot.GetConfigVariable(NuGetRepositoriesSetting) ?? string.Empty;
	        foreach(var source in configuredSources.Split(','))
	        {
		        AddSource(source, robot);
	        }
        }

        private List<string> GetRememberedSources(IRobot robot)
        {
	        var sources = robot.Brain.Get<List<string>>(NuGetRepositoriesSetting).Result;
	        if(sources == null)
	        {
		        sources = new List<string>();
		        Remember(NuGetRepositoriesSetting, sources, robot);
	        }

	        return sources;
        }

        private void Remember(string key, object value, IRobot robot)
        {
	        robot.Brain.Set(key, value);
        }

        private bool AddSource(string source, IRobot robot)
        {
	        var sources = GetRememberedSources(robot);
	        if (sources.Contains(source))
	        {
		        return false;
	        }
            sources.Add(source);
            Remember(NuGetRepositoriesSetting, sources, robot);
            return true;
        }

        private bool RemoveSource(string source, IRobot robot)
        {
	        var sources = GetRememberedSources(robot);
	        if (sources.Contains(source))
	        {
		        sources.Remove(source);
		        Remember(NuGetRepositoriesSetting, sources, robot);
		        return true;
	        }
            return false;
        }
        
        private void RememberConfiguredAliases(IRobot robot)
        {
	        var configuredAliases = robot.GetConfigVariable(NuGetPackageAliasesSetting) ?? string.Empty;
            foreach (var alias in configuredAliases.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries))
            {
                AddAlias(alias, robot);
            }
        }

        private Dictionary<string,string> GetRememberedAliases(IRobot robot)
        {
	        var aliases = robot.Brain.Get<Dictionary<string,string>>(NuGetPackageAliasesSetting).Result;
	        if(aliases == null)
	        {
		        aliases = new Dictionary<string,string>();
		        Remember(NuGetPackageAliasesSetting, aliases, robot);
	        }
	        return aliases;
        }

        private void AddAlias(string alias, IRobot robot)
        {
	        var aliases = GetRememberedAliases(robot);
	        var parts = alias.Split('=');
	
	        alias = parts[0].ToLower();
	        var packageName = parts[1];
	
	        aliases[alias] = packageName;

	        Remember(NuGetPackageAliasesSetting, aliases, robot);
        }

        private void RemoveAlias(string alias, IRobot robot)
        {
	        var aliases = GetRememberedAliases(robot);
	        alias = alias.Split(',')[0];
	        aliases.Remove(alias);
	        Remember(NuGetPackageAliasesSetting, aliases, robot);
        }

        private void RememberConfiguredAutoReset(IRobot robot)
        {
            var autoReset = robot.GetConfigVariable(NuGetResetAfterUpdateSetting) ?? string.Empty;
            bool autoResetValue;
            if (!bool.TryParse(autoReset, out autoResetValue))
            {
                autoResetValue = false;
            }
            Remember(NuGetResetAfterUpdateSetting, autoResetValue, robot);
        }

        private bool ShouldAutoResetAfterUpdate(IRobot robot)
        {
            return bool.Parse(robot.Brain.Get<string>(NuGetResetAfterUpdateSetting).Result);
        }

        private string BuildCommand(IEnumerable<string> parts, IEnumerable<int> optionalParams = null)
        {
            return string.Join(@"\s",
                parts.Select((part, i) =>
                {
                    var optional = (optionalParams ?? new int[0]).Contains(i);
                    return string.Format("{0}({1}){2}", 
                        optional ? "*" : string.Empty,
                        part,
                        optional ? "?" : string.Empty);
                }));
        }

        private AggregateRepository BuildPackagesRepository(IRobot robot)
        {
            var packageSources = GetRememberedSources(robot).Where(s => !string.IsNullOrWhiteSpace(s));
	        return new AggregateRepository(packageSources
		        .Select(s => PackageRepositoryFactory.Default.CreateRepository(s)));

        }

        private string GetPackagesPath()
        {
	        return Path.Combine(Directory.GetCurrentDirectory(), "packages");
        }

        public void Register(IRobot robot)
        {
            RememberConfiguredSources(robot);
            RememberConfiguredAliases(robot);
            RememberConfiguredAutoReset(robot);

            robot.Respond(BuildCommand(new[] {List, Package, Source}), 
                msg => msg.Send(GetRememberedSources(robot).ToArray()));

            robot.Respond(BuildCommand(new []{Add, Package, Source, ParamWithNoSpaces}), msg =>
            {
                var source = msg.Match[4].ToString(CultureInfo.InvariantCulture);
                msg.Send(!AddSource(source, robot) 
                    ? "I already know about this one." 
                    : "Consider it done.");
            });

            robot.Respond(BuildCommand(new []{Remove, Package, Source, ParamWithNoSpaces}), msg =>
            {
                var source = msg.Match[4].ToString(CultureInfo.InvariantCulture);
                msg.Send(RemoveSource(source, robot)
                    ? "I'll forget it immediately."
                    : "It's easy to forget what you never knew.");
            });

            robot.Respond(BuildCommand(new[] { Update, Package, ParamWithNoSpaces, Restart}, new[] {3}), msg =>
            {
                //ID of the package to be looked up
                var packageId = msg.Match[3].ToString(CultureInfo.InvariantCulture);
                string unaliasedPackageId;

                var knownAliases = GetRememberedAliases(robot);
                if (!knownAliases.TryGetValue(packageId.ToLower(), out unaliasedPackageId))
                {
                    unaliasedPackageId = packageId;
                }

                msg.Send("Building repositories...");
                IPackageRepository repo = BuildPackagesRepository(robot);

                //Get the list of all NuGet packages with ID 'EntityFramework'   
                msg.Send("Finding package...");
                List<IPackage> packages = repo.FindPackagesById(unaliasedPackageId).ToList();

                IPackage latestPackageVersion;

                if (packages.Any())
                {
                    //try to get the "absolute latest version" and fall back to packages.Last() if none are marked as such
                    latestPackageVersion = packages.Any(p => p.IsAbsoluteLatestVersion)
                                               ? packages.First(p => p.IsAbsoluteLatestVersion)
                                               : packages.Last();
                    msg.Send("Found it! Downloading...");
                }
                else
                {
                    msg.Send("I couldn't find it...sorry!");
                    return;
                }

                //Initialize the package manager
                string path = GetPackagesPath();
                var packageManager = new PackageManager(repo, path);

                //Download and unzip the package
                packageManager.InstallPackage(latestPackageVersion, false, true);//TODO: allow these flags to be configurable? allow user to specify version?
                msg.Send("Finished downloading...");
                
                if (ShouldAutoResetAfterUpdate(robot) || (msg.Match.Length >= 5 && Regex.IsMatch(msg.Match[4], Restart)))
                {
                    //They submitted the reset parameter or auto-reset is on.
                    msg.Send("Resetting...please wait.");
                    robot.Reset();
                }
            });

            robot.Respond(BuildCommand(new []{List, Package, Alias}), 
                msg => msg.Send(GetRememberedAliases(robot).Select(kvp => string.Format("{0} = {1}", kvp.Key, kvp.Value)).ToArray()));

            robot.Respond(BuildCommand(new []{Add, Package, Alias, ParamWithNoSpaces}), msg =>
            {
	            var alias = msg.Match[4].ToString(CultureInfo.InvariantCulture);
	            AddAlias(alias, robot);
	            msg.Send("I'll be sure to remember that.");
            });

            robot.Respond(BuildCommand(new []{Remove, Package, Alias, ParamWithNoSpaces}), msg =>
            {
	            var alias = msg.Match[4].ToString(CultureInfo.InvariantCulture);
	            RemoveAlias(alias, robot);
	            msg.Send("As you wish.");
            });
        }

        public IEnumerable<string> GetHelp()
        {
            return new List<string>
            {
                "mmbot add package source (package source url) - adds a package source to use when downloading packages",
                "mmbot remove package source (package source url) - removes a package source",
                "mmbot list package sources - lists the currently in-use package sources",
                "mmbot add package alias (alias name)=(actual package name) - adds an alias to a package name for convenience",
                "mmbot remove package alias (alias name) - removes an alias",
                "mmbot list package aliases - lists the currently in-use package aliases",
                "mmbot update (package name or alias) [restart] - updates the specified package and optionally restarts the robot to load updated packages"
            };
        }
    }
}
