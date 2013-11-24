//namespace SEToolbox.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Xml;

//    public class SEResource : BaseModel
//    {
//        #region Fields

//        private XmlDocument sandboxDoc;
//        private string globalFile;
//        private string sandboxFile;
//        private string description;
//        private DateTimeOffset lastSaved;
//        //private Character character;
//        //private List<Structure> structures;
//        private bool isValid;

//        #endregion

//        #region Properties

//        public XmlDocument SandboxDoc
//        {
//            get
//            {
//                return this.sandboxDoc;
//            }

//            set
//            {
//                this.sandboxDoc = value;

//                this.OnPropertyChanged("SandboxDoc");
//            }
//        }

//        public string GlobalFile
//        {
//            get
//            {
//                return this.globalFile;
//            }

//            set
//            {
//                this.globalFile = value;

//                this.OnPropertyChanged("GlobalFile");
//            }
//        }

//        public string SandboxFile
//        {
//            get
//            {
//                return this.sandboxFile;
//            }

//            set
//            {
//                this.sandboxFile = value;

//                this.OnPropertyChanged("SandboxFile");
//            }
//        }

//        public string Description
//        {
//            get
//            {
//                return this.description;
//            }

//            set
//            {
//                this.description = value;

//                this.OnPropertyChanged("Description");
//            }
//        }

//        public DateTimeOffset LastSaved
//        {
//            get
//            {
//                return this.lastSaved;
//            }

//            set
//            {
//                this.lastSaved = value;

//                this.OnPropertyChanged("LastSaved");
//            }
//        }

//        //public Character Character
//        //{
//        //    get
//        //    {
//        //        return this.character;
//        //    }

//        //    set
//        //    {
//        //        this.character = value;

//        //        this.OnPropertyChanged("Character");
//        //    }
//        //}

//        //public List<Structure> Structures
//        //{
//        //    get
//        //    {
//        //        return this.structures;
//        //    }

//        //    set
//        //    {
//        //        this.structures = value;

//        //        this.OnPropertyChanged("Structures");
//        //    }
//        //}

//        public bool IsValid
//        {
//            get
//            {
//                return this.isValid;
//            }

//            set
//            {
//                this.isValid = value;

//                this.OnPropertyChanged("IsValid");
//            }
//        }

//        #endregion
//    }
//}
