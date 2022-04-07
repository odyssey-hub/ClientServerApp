using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSemestrClient
{
    public class FileRecord
    {
        public string Name { get; set; }
        public string Length { get; set; }
        
        public FileRecord(string name, long size)
        {
            Name = name;
            int lenght = (int) Math.Ceiling((double)size / 1024);
            Length = lenght.ToString();
        }
    }
}
