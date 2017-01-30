using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gmpublish
{
    public enum AddonType
    {

    }

    public class gmosh
    {
        public void Publish(string logo = "") { }
        public void Extract(string pathGMA, string pathOut) { }
        public void Create(string pathAddon, string pathOutGMA) { }
        public List<string> CheckIlligal(string pathGMA) { throw new NotImplementedException(); }
        public List<string> ListContents(string pathGMA) { throw new NotImplementedException(); }

        public void InitAddon(string name, string addonType, string tag1, string tag2, string[] ignore, string default_changelog) { }
    }
}
