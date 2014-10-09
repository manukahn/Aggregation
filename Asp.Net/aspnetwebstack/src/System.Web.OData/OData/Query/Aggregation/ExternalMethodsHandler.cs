using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;

namespace System.Web.OData.OData.Query.Aggregation
{
    public interface IExternalMethodHandler
    {
        int RegisterExternalMethods();
    }

    /// <summary>
    /// Register custom aggregation and sampling method located in a remote assembly
    /// </summary>
    public class ExternalMethodsHandler : IExternalMethodHandler
    {
        public int RegisterExternalMethods()
        {
            AggregationMethodsAssembly = DownladExternalAssembly();
            if (AggregationMethodsAssembly == null)
            {
                return 0;
            }
            return RegisterAggregationMethods() + RegisterSamplingMethods();
        }

        /// <summary>
        /// Gets or sets the location of the remote assembly that contain custom aggregation and sampling methods 
        /// </summary>
        public Uri RemoteFileUri { get; set; }

        /// <summary>
        /// Register custom aggregation and sampling method located in a remote assembly
        /// </summary>
        public Assembly AggregationMethodsAssembly { get; private set; }
       
        private int RegisterSamplingMethods()
        {
            int counter = 0;
            var samplingMethodsTypes =
                AggregationMethodsAssembly.DefinedTypes.Where(
                    t => t.GetCustomAttributes(typeof (SamplingMethodAttribute), true).Any());


            foreach (var samplingMethodType in samplingMethodsTypes)
            {
                if (!typeof (SamplingImplementationBase).IsAssignableFrom(samplingMethodType))
                {
                    throw new ArgumentException("The decorated type does not derive from SamplingImplementationBase");
                }

                var att = samplingMethodType.GetCustomAttributes(typeof(SamplingMethodAttribute)).FirstOrDefault();
                if (att != null)
                {
                    var name = ((SamplingMethodAttribute) att).Name;

                    SamplingMethodsImplementations.RegisterAggregationImplementation(name,
                        (SamplingImplementationBase) Activator.CreateInstance(samplingMethodType));
                    counter++;
                }
            }
            return counter;
        }

        private int RegisterAggregationMethods()
        {
            int counter = 0;
            var aggregationMethodsTypes =
                AggregationMethodsAssembly.DefinedTypes.Where(
                    t => t.GetCustomAttributes(typeof(AggregationMethodAttribute), true).Any());

            foreach (var aggregationMethodType in aggregationMethodsTypes)
            {
                if (!typeof (AggregationImplementationBase).IsAssignableFrom(aggregationMethodType))
                {
                    throw new ArgumentException("The decorated type does not derive from AggregationImplementationBase");
                }

                var att = aggregationMethodType.GetCustomAttributes(typeof (AggregationMethodAttribute)).FirstOrDefault();
                if (att != null)
                {
                    var name = ((AggregationMethodAttribute) att).Name;

                    AggregationMethodsImplementations.RegisterAggregationImplementation(name,
                        (AggregationImplementationBase) Activator.CreateInstance(aggregationMethodType));
                    counter++;
                }
            }
            return counter;
        }

        private Uri GetDefaultRemoteFileUri()
        {
            var path = ConfigurationManager.AppSettings.Get("AggregationMethodsFileUri");
            if (!string.IsNullOrEmpty(path))
            {
                return new Uri(path);
            }
            return null;
        }

        private Assembly DownladExternalAssembly()
        {
            if (RemoteFileUri == null) 
            {
                RemoteFileUri = GetDefaultRemoteFileUri();

                if (RemoteFileUri == null)
                {
                    return null;
                }
            }
            try
            {
                if (!RemoteFileUri.AbsoluteUri.EndsWith("dll"))
                {
                    throw new ArgumentException("The uri of the file should end with .dll");
                }
                if ((RemoteFileUri.Scheme == "http") || (RemoteFileUri.Scheme == "https"))
                {
                    var rnd = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
                    var fileName = string.Format("AggregationMethods-{0}.dll", rnd.Next(100000));
                    var tempPath = Path.GetTempPath();
                    var downloadedFile = Path.Combine(tempPath, fileName);
                    var myWebClient = new WebClient();

                    /// Download the Web resource and save it into the current file system temp folder.
                    myWebClient.DownloadFile(RemoteFileUri, downloadedFile);

                    return Assembly.LoadFile(downloadedFile);
                }
                else if (RemoteFileUri.Scheme == "file")
                {
                    return Assembly.LoadFile(RemoteFileUri.LocalPath);
                }
                throw new ArgumentException("Invalid Uri for external aggregation methods file");
            }
            catch (FileNotFoundException ex)
            {
                throw new ArgumentException("Could not find the external aggregation methods file", ex);
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException("Could not find the external aggregation methods file, check the address or your network connection", ex);
            }
        }
    }
}
