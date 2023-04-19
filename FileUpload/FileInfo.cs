using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUpload
{
    internal class FileInfo
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Extension { get; set; }

        public int UserId { get; internal set; }

    }
}
