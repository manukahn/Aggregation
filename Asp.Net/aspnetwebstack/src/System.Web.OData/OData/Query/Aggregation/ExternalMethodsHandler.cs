using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using System.Web.OData.OData.Query.Configuration;

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
        public IEnumerable<Uri> RemoteFilesUris { get; set; }

        /// <summary>
        /// Register custom aggregation and sampling method located in a remote assembly.
        /// </summary>
        /// <returns>The number of implementations that were registered.</returns>
        public int RegisterExternalMethods()
        {
            this.AggregationMethodsAssemblies = DownladExternalAssemblies();
            if (AggregationMethodsAssemblies == null)
            {
                return 0;
            }
            return RegisterAggregationMethods() + RegisterSamplingMethods();
        }

        /// <summary>
        /// Gets the custom aggregation or sampling remote assembly.
        /// </summary>
        public IEnumerable<Assembly> AggregationMethodsAssemblies { get; private set; }

        private int RegisterSamplingMethods()
        {
            int counter = 0;

            foreach (var aggregationMethodAssembly in AggregationMethodsAssemblies)
            {
                var samplingMethodsTypes =
                    aggregationMethodAssembly.DefinedTypes.Where(
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
                        var name = ((SamplingMethodAttribute) att).Name;

                        SamplingMethodsImplementations.RegisterAggregationImplementation(name,
                            (SamplingImplementationBase) Activator.CreateInstance(samplingMethodType));
                        counter++;
                    }
                }
            }
            return counter;
        }

        private int RegisterAggregationMethods()
        {
            int counter = 0;

            foreach (var aggregationMethodAssembly in AggregationMethodsAssemblies)
            {
                var aggregationMethodsTypes =
                    aggregationMethodAssembly.DefinedTypes.Where(
                        t => t.GetCustomAttributes(typeof(AggregationMethodAttribute), true).Any());

                foreach (var aggregationMethodType in aggregationMethodsTypes)
                {
                    if (!typeof(AggregationImplementationBase).IsAssignableFrom(aggregationMethodType))
                    {
                        throw new ArgumentException(
                            "The decorated type does not derive from AggregationImplementationBase");
                    }

                    var att =
                        aggregationMethodType.GetCustomAttributes(typeof (AggregationMethodAttribute)).FirstOrDefault();
                    if (att != null)
                    {
                        var name = ((AggregationMethodAttribute) att).Name;

                        AggregationMethodsImplementations.RegisterAggregationImplementation(name,
                            (AggregationImplementationBase) Activator.CreateInstance(aggregationMethodType));
                        counter++;
                    }
                }
            }
            return counter;
        }

        private IEnumerable<Uri> GetDefaultRemoteFileUri()
        {
            var result = new List<Uri>();
            var section = ConfigurationManager.GetSection("aggregation") as AggregationConfiguration;
            if (section != null)
            {
                foreach (Location library in section.ExternalLibraries)
                {
                    try
                    {
                        result.Add(new Uri(library.Uri));
                    }
                    catch (UriFormatException ex)
                    {
                        result.Add(new Uri(Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, library.Uri)));
                        result.Add(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, library.Uri)));
                    }
                }
            }
            
            if (result.Any())
            {
                return result;
            }

            return null;
        }

        private IEnumerable<Assembly> DownladExternalAssemblies()
        {
            if (this.RemoteFilesUris == null)
            {
                this.RemoteFilesUris = this.GetDefaultRemoteFileUri();

                if (this.RemoteFilesUris == null)
                {
                    return null;
                }
            }
            try
            {
                var result = new List<Assembly>();
                foreach (var remoteFileUri in RemoteFilesUris)
                {
                    if (!remoteFileUri.AbsoluteUri.EndsWith("dll"))
                    {
                        throw new ArgumentException("The uri of the file should end with .dll");
                    }
                    
                    if ((remoteFileUri.Scheme == "http") || (remoteFileUri.Scheme == "https"))
                    {
                        var rnd = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
                        var fileName = string.Format("AggregationMethods-{0}.dll", rnd.Next(100000));
                        var tempPath = Path.GetTempPath();
                        var downloadedFile = Path.Combine(tempPath, fileName);
                        var myWebClient = new WebClient();

                        // Download the Web resource and save it into the current file system temp folder.
                        myWebClient.DownloadFile(remoteFileUri, downloadedFile);

                        result.Add(Assembly.LoadFile(downloadedFile));
                    }
                    else if (remoteFileUri.Scheme == "file")
                    {
                        if (File.Exists(remoteFileUri.LocalPath))
                        {
                            result.Add(Assembly.LoadFile(remoteFileUri.LocalPath));
                        }
                    }
                }
                return result;
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
