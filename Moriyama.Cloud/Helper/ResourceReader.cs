namespace Moriyama.Cloud.Helper
{
    internal class ResourceReader
    {
        private static readonly ResourceReader TheInstance = new ResourceReader();

        private ResourceReader() { }

        public static ResourceReader Instance { get { return TheInstance; } }
    }
}
