# DotEmwin
Emergency Managers Weather Information Network Client Libraries

http://www.nws.noaa.gov/emwin/user-intro.htm

The Emergency Managers Weather Information Network -- EMWIN -- is a service that allows users to obtain weather
forecasts, warnings, and other information directly from the National Weather Service (NWS) in almost real time.
EMWIN is intended to be used primarily by emergency managers and public safety officials who need timely weather
information to make critical decisions. EMWIN consists of a round-the-clock data feed of current weather warnings,
watches, images from NESDIS, advisories, forecasts, and other products issued by the National Weather Service.

**Example of use:**

````csharp

using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Emwin.ByteBlaster;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.Processor;
using Emwin.Processor.Instrumentation;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

namespace EmwinTest
{
    internal class Program
    {
        private static void Main()
        {
            var listener = ConsoleLog.CreateListener();
            listener.EnableEvents(ByteBlasterEventSource.Log, EventLevel.Verbose);
            listener.EnableEvents(ProcessorEventSource.Log, EventLevel.Verbose);

            var processor = new WeatherProductProcessor();

            // Can subscribe to Images, Text or Bulletin observables
            processor.GetBulletinObservable().Subscribe(product =>
            {
                Console.WriteLine(product);
                Console.WriteLine(product.Header);
                product.GeoCodes.ToList().ForEach(Console.WriteLine);
                product.VtecCodes.ToList().ForEach(Console.WriteLine);
                product.Polygons.ToList().ForEach(Console.WriteLine);

                using (var file = File.CreateText(@"C:\\Data\\" + product.Filename))
                    file.Write(product.Content);
            });

            processor.Start();

            var client = new ByteBlasterClient("user@example.com");
            client.Subscribe(processor);
            client.Start();

            Console.ReadKey();

            client.Stop();
            processor.Stop();
            ByteBlasterClient.ShutdownGracefullyAsync().Wait(5000);
        }
    }
}

````
