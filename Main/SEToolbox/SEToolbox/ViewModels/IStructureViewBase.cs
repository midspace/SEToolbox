namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IStructureViewBase
    {
        bool IsSelected { get; set; }

        IStructureBase DataModel { get; }

        //MyObjectBuilder_EntityBase EntityBase { get; set; }

        //long EntityId { get; set; }

        //MyPositionAndOrientation? PositionAndOrientation { get; set; }

        //ClassType ClassType { get; set; }

        //void UpdateFromEntityBase();
    }
}
