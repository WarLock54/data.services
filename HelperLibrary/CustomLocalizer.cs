using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public class CustomLocalizer : IStringLocalizer
    {
        private ResourceManager _resourceManager;

        public LocalizedString this[string name]
        {
            get
            {
                var value = _resourceManager.GetString(name);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public LocalizedString this[string name, string resourceFile]
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = GetAssemblyFromName(resourceFile);
                }
                var value = _resourceManager.GetString(name);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }
        public LocalizedString this[string name,params object[] arguments]
        {
            get
            {
                if (_resourceManager == null)
                    _resourceManager = GetAssemblyFromName(arguments[0].ToString());
                var value = _resourceManager.GetString(name,CultureInfo.CurrentCulture);
                if (arguments.Length > 1) {
                    value=string.Format(value,arguments?.Skip(1).ToArray());
                }
                return new LocalizedString(name,value ?? name, value == null);
            }
        }
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }
        private ResourceManager GetAssemblyFromName(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var resourceAssembly = assembly.GetManifestResourceNames();
            var targerResource = resourceAssembly.FirstOrDefault(x => x.Contains(resourceName) || x.Equals(resourceName + ".resx"));

            if (targerResource != null)
            {
                return new ResourceManager(targerResource.Replace(".resources", string.Empty), assembly);
            }

            return null;
        }
    }
}
