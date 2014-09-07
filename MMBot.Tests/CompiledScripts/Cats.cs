﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using MMBot.Scripts;

namespace MMBot.Tests.CompiledScripts
{

    public class Cats : IMMBotScript
    {
        private const string Url = "http://thecatapi.com/api/images/get?format=xml&results_per_page={0}&api_key=MTAzNjQ";

        public void Register(IRobot robot)
        {
            robot.Respond(@"(cat|cats)( gif)( \d+)?$", async msg =>
            {
                int number = 1;
                try
                {
                    number = Int32.Parse(msg.Match[3]);
                }
                catch (Exception) { }
                if (number == 0)
                {
                    number = 1;
                }

                await CatMeGifCore(msg, number);
            });
            
            robot.Respond(@"(cat|cats)( me)?( \d+)?$", async msg =>
            {
                int number = 1;
                try
                {
                    number = Int32.Parse(msg.Match[3]);
                }
                catch (Exception) { }
                if (number == 0)
                {
                    number = 1;
                }

                await CatMeCore(msg, number);
            });
        }

        private static async Task CatMeCore(IResponse<TextMessage> msg, int number)
        {
            var xDoc = await msg.Http(string.Format(Url, number)).GetXml();

            try
            {
                var urls = xDoc.SelectNodes("//url");
                foreach (XmlNode url in urls)
                {
                    msg.Send(url.InnerText);
                }
            }
            catch (Exception)
            {
                msg.Send("erm....issues, move along");
            }
        }

        private static async Task CatMeGifCore(IResponse<TextMessage> msg, int number)
        {
            var xDoc = await msg.Http(string.Format(Url, number) + "&type=gif").GetXml();

            try
            {
                var urls = xDoc.SelectNodes("//url");
                foreach (XmlNode url in urls)
                {
                    msg.Send(url.InnerText);
                }
            }
            catch (Exception)
            {
                msg.Send("erm....issues, move along");
            }
        }

        public IEnumerable<string> GetHelp()
        {
            return new[]
            {
                "mmbot cat me <number> - Returns a number of cat pictures.",
                "mmbot cat me - Returns a cat picture.",
                "mmbot cat gif <number> - Returns a number of cat gifs.",
                "mmbot cat gif - Returns a cat gif."
            };
        }
    }
}
