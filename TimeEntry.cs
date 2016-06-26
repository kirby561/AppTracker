using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTracker {
    class ProcessEntry {
        public ProcessEntry(String name, double startingTime) {
            Name = name;
            Time = startingTime;
        }

        public String Name { get; set; }
        public double Time { get; set; }
    }
}
