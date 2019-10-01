namespace MyV2ray.Core.Models
{
    using System.Collections.Generic;

    public struct Certificate
    {
        public string CertificateFile { get; set; }
        public string KeyFile { get; set; }
    }

    public class RayPortSettings
    {
        /// <summary>
        /// Default to false
        /// </summary>
        public bool Udp { get; set; }
        public string Address { get; set; }
        public IList<RayPortUser> Clients { get; set; }

        public RayPortSettings()
        {
            Udp = false;
            Address = null;
        }
    }

    public class WSSettings
    {
        /// <summary>
        /// Default to true
        /// </summary>
        public bool ConnectionReuse { get; set; }
        public string Path { get; set; }

        public WSSettings()
        {
            ConnectionReuse = true;
        }
    }

    public class TlsSettings
    {
        public IList<Certificate> Certificates { get; set; }
    }

    public class RayPortStreamSettings
    {
        private string netWork;
        public string NetWork
        {
            get => netWork;
            set => netWork = string.IsNullOrEmpty("value") ? "tcp" : value;
        }
        public string Security { get; set; }

        public WSSettings WSSettings { get; set; }

        public TlsSettings TlsSettings { get; set; }
    }

}
