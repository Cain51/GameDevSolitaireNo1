using System;
using System.Collections.Generic;

namespace RG.Basic {

    public class InitRecorder { 

        public static HashSet<string> initializedKeys = new HashSet<string>();
         

        public static bool MarkInitialed(string key) { 
            return initializedKeys.Contains(key) ? false : initializedKeys.Add(key);
        } 

        public static bool IsInitialed(string key) { 
            return initializedKeys.Contains(key);
        }

    }
}
