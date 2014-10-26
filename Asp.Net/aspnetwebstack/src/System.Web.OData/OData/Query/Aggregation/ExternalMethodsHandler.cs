using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;

namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// Interface for ExternalMethodsHandler. 
    /// This could be used by dependency injection frameworks to provide other implementation of the ExternalMethodsHandler.
    /// </summary>
    public interface IExternalMethodHandler
    {
        /// <summary>
        /// Register custom aggregation and sampling method located in a remote assembly.
        /// </summary>
        /// <returns>The number of implementations that were registered</returns>
        int RegisterExternalMethods();
    }

    /// <summary>
    /// Register custom aggregation and sampling method located in a remote assembly.
    /// </summary>
    public class ExternalMethodsHandler : IExternalMethodHandler
    {
        /// <summary>
        /// Gets or sets the location of the remote assembly that contain custom aggregation and sampling methods. 
        /// </summary>
        public Uri RemoteFileUri { get; set; }

        /// <summary>
        /// Register custom aggregation and sampling method located in a remote assembly.
        /// </summary>
        /// <returns>The number of implementations that were registered.</returns>
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
        /// Gets the custom aggregation or sampling remote assembly.
        /// </summary>
        public Assembly AggregationMethodsAssembly { get; private set; }

        private int RegisterSamplingMethods()
        {
            int counter = 0;
            var samplingMethodsTypes =
                AggregationMethodsAssembly.DefinedTypes.Where(
                    t => t.GetCustomAttributes(typeof(SamplingMethodAttribute), true).Any());


            foreach (var samplingMethodType in samplingMethodsTypes)
            {
                if (!typeof(SamplingImplementationBase).IsAssignableFrom(samplingMethodType))
                {
                    throw new ArgumentException("The decorated type does not derive from SamplingImplementationBase");
                }

                var att = samplingMethodType.GetCustomAttributes(typeof(SamplingMethodAttribute)).FirstOrDefault();
                if (att != null)
                {
                    var name = ((SamplingMethodAttribute)att).Name;

                    SamplingMethodsImplementations.RegisterAggregationImplementation(name,
                        (SamplingImplementationBase)Activator.CreateInstance(samplingMethodType));
                    counter++;
                }
            }
            return counter;
        }

        private int RegisterAggregationMethods()
        {
            int counter = 0;
            var aggregationMethodsTypes =
                this.AggregationMethodsAssembly.DefinedTypes.Where(
                    t => t.GetCustomAttributes(typeof(AggregationMethodAttribute), true).Any());

            foreach (var aggregationMethodType in aggregationMethodsTypes)
            {
                if (!typeof(AggregationImplementationBase).IsAssignableFrom(aggregationMethodType))
                {
                    throw new ArgumentException("The decorated type does not derive from AggregationImplementationBase");
                }

                var att = aggregationMethodType.GetCustomAttributes(typeof(AggregationMethodAttribute)).FirstOrDefault();
                if (att != null)
                {
                    var name = ((AggregationMethodAttribute)att).Name;

                    AggregationMethodsImplementations.RegisterAggregationImplementation(name,
                        (AggregationImplementationBase)Activator.CreateInstance(aggregationMethodType));
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
            if (this.RemoteFileUri == null)
            {
                this.RemoteFileUri = this.GetDefaultRemoteFileUri();

                if (this.RemoteFileUri == null)
                {
                    return null;
                }
            }
            try
            {
                if (!this.RemoteFileUri.AbsoluteUri.EndsWith("dll"))
                {
                    throw new ArgumentException("The uri of the file should end with .dll");
                }

                if ((this.RemoteFileUri.Scheme == "http") || (this.RemoteFileUri.Scheme == "https"))
                {
                    var rnd = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
                    var fileName = string.Format("AggregationMethods-{0}.dll", rnd.Next(100000));
                    var tempPath = Path.GetTempPath();
                    var downloadedFile = Path.Combine(tempPath, fileName);
                    var myWebClient = new WebClient();

                    // Download the Web resource and save it into the current file system temp folder.
                    myWebClient.DownloadFile(this.RemoteFileUri, downloadedFile);

                    return Assembly.LoadFile(downloadedFile);
                }
                else if (this.RemoteFileUri.Scheme == "file")
                {
                    return Assembly.LoadFile(this.RemoteFileUri.LocalPath);
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
