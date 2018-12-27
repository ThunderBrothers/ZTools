using System;
using System.Collections;

namespace ZTools.FileIO
{
    //json模板
    [Serializable]
    public class JsonDataBase:object
    {
        public string Company;
        public int Index;
        public Projects[] Projects;
    }
    [Serializable]
    public class Projects 
    {
        public string name;
        public int score;
    }
}

