using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Core.Models
{
    public class UpdateManifest
    {
        public string LatestVersion { get; set; }
        public string Changelog { get; set; }
        public string ZipFileName { get; set; }
    }

    public class UpdateFile
    {
        public string Path { get; set; }
        public string Checksum { get; set; }
    }
}
