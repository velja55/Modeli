using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.LoadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class RegulationSchedule : SeasonDayTypeSchedule
    {
        // Referenca na RegulatingControl (0..1)
        private long regulatingControl = 0;

        public RegulationSchedule(long globalId)
            : base(globalId)
        {
        }

        public long RegulatingControl
        {
            get { return regulatingControl; }
            set { regulatingControl = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                RegulationSchedule x = (RegulationSchedule)obj;
                return this.regulatingControl == x.regulatingControl;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.REG_SCHEDULE_RG_CNTRL:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.REG_SCHEDULE_RG_CNTRL:
                    property.SetValue(regulatingControl);
                    break;
                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.REG_SCHEDULE_RG_CNTRL:
                    regulatingControl = property.AsReference();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (regulatingControl != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.REG_SCHEDULE_RG_CNTRL] = new List<long> { regulatingControl };
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            // RegulationSchedule nema kolekciju referenci, samo pojedinačnu referencu na RegulatingControl
            base.AddReference(referenceId, globalId);
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            // RegulationSchedule nema kolekciju referenci, samo pojedinačnu referencu na RegulatingControl
            base.RemoveReference(referenceId, globalId);
        }

        #endregion IReference implementation
    }
}
