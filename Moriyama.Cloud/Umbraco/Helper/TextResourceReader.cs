using System.IO;
using Moriyama.Cloud.Umbraco.Interfaces.Helper;

namespace Moriyama.Cloud.Umbraco.Helper
{
    public class TextResourceReader : ITextResourceReader
    {
        private static readonly TextResourceReader TheInstance = new TextResourceReader();

        private TextResourceReader() { }

        public static TextResourceReader Instance { get { return TheInstance; } }

        public string ReadResourceFile(string resourceName)
        {
            var assem = GetType().Assembly;
            using (var stream = assem.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
